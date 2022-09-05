using Base;
using Communications;
using Enumerations;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Units;
using Peripherals;
using Servos;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TinkerTank.Abstractions;
using TinkerTank.Data;
using TinkerTank.MiscPeriherals;
using TinkerTank.Movement;
using TinkerTank.Sensors;
using TinkerTank.Servos;
using Utilities.Power;
using static TinkerTank.Display.DisplayBase;

namespace TinkerTank
{
    public class MeadowApp : App<F7FeatherV1, MeadowApp>
    {
        public Lights LightsController;

        public MovementAbstractions movementController;
        public PowerControl powerController;

        public BlueTooth communications;

        public PCA9685 i2CPWMController;

        public Tca9548a i2cExpander;

        public PushButton button;

        public readonly int PWMFrequency = 60;//50;

        public ArmControl Arm;

        public DistanceSensorController distController;

        public PanTiltSensorCombo panTiltSensorCombo;

        public II2cBus primaryi2CBus;

        IDigitalOutputPort blueLED;
        IDigitalOutputPort greenLED;
        IDigitalOutputPort redLED;
        ComponentStatus _status;

        private List<TinkerBase> TBObjects;
        private System.Timers.Timer _statusPoller;

        private bool EnableDistanceSensors = true;
        private bool EnablePanTiltControl = true;
        public bool EnableDisplay = true;
        private bool EnableArm = false;
        private bool EnablePCA9685 = true;
        private bool EnableStatusPolling = false;
        public bool EnableWatchDog = false;
        public bool EnableLights = false;
        private bool AllowInitDB = true;
        public bool EnableDBLogging = true;
        public bool SetBTProperties = false;
        public DisplayTypes DisplayModel = DisplayTypes.SSD1306_I2C_128x64;

        public LogStatusMessageTypes MinimumLogLevel = LogStatusMessageTypes.Debug;

        public Logging Logger;

        public DataStore dbcon;
        private bool WipeDBOnStartup = true;

        public MeadowApp()
        {
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Task startup = new Task(() =>
            {
                Init();
            });

            startup.Start();
        }

        

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("******  Unhandled Exception Raised to Domain  ******");
            //DebugDisplayText("Unhandled Exception Raised to Domain. " + sender.ToString(), LogStatusMessageTypes.Error);
        }

        private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            Console.WriteLine("******  Unhandled First Chance Exception Raised to Domain  ******");
        }

        void Init()
        {

            try
            {
                //BIOS
                DebugDisplayText("Init onboard Meadow lights", LogStatusMessageTypes.Important);
                blueLED = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedBlue);
                greenLED = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedGreen);
                redLED = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedRed);

                //Console.WriteLine(String.Concat("pre-logger-msg: ", "LED startup success "));
            }
            catch (Exception)
            {
                //very bad
                DebugDisplayText("INIT Onboard meadow lighs error", LogStatusMessageTypes.Error);
            }

            //I2C

            try
            {
                DebugDisplayText("Init i2c");
                primaryi2CBus = Device.CreateI2cBus(I2cBusSpeed.Standard);
                //Console.WriteLine(String.Concat("pre-logger-msg: ", "I2C success "));
            }
            catch (Exception)
            {
                //very bad
                DebugDisplayText("INIT I2C ERROR", LogStatusMessageTypes.Critical);
            }

            try
            {
                if (AllowInitDB)
                {
                    //Initialise the database
                    DebugDisplayText("Init DB", LogStatusMessageTypes.Important);
                    dbcon = new DataStore();
                    dbcon.InitDB(WipeDBOnStartup);
                }
                else
                {
                    DebugDisplayText("DB Init disabled", LogStatusMessageTypes.Information);

                }

            }
            catch (Exception dbMasterEx)
            {
                DebugDisplayText("INIT DB error: " + dbMasterEx.Message, LogStatusMessageTypes.Error);
            }

            try
            {
                //Initialise the logger
                Logger = new Logging();
                Logger.Init(dbcon);
            }
            catch (Exception logEx)
            {
                DebugDisplayText("Broad Logger Init Error: " + logEx.Message, LogStatusMessageTypes.Error);
            }

            try
            {
                TBObjects = new List<TinkerBase>();

                //I2C Expander
                DebugDisplayText("Init i2c Expander");
                i2cExpander = new Tca9548a(primaryi2CBus, 0x70);

                if (EnablePCA9685)
                {
                    DebugDisplayText("start PCA9685", LogStatusMessageTypes.Important);
                    i2CPWMController = new PCA9685(primaryi2CBus);
                    TBObjects.Add(i2CPWMController);
                    i2CPWMController.Init();
                }

                //Communications and control
                Console.WriteLine(String.Concat("MAIN-A"));

                DebugDisplayText("start communications controller", LogStatusMessageTypes.Important);
                communications = new BlueTooth();
                TBObjects.Add(communications);
                communications.Init();


                Console.WriteLine(String.Concat("MAIN-B"));
                //Sensors
                if (EnableDistanceSensors)
                {
                    DebugDisplayText("Start distance sensor controller");

                    distController = new DistanceSensorController(i2cExpander);

                    TBObjects.Add(distController);
                    distController.Init();
                }

                //Movement and power    

                DebugDisplayText("start power controller", LogStatusMessageTypes.Important);
                powerController = new PowerControl(this);
                TBObjects.Add(powerController);
                powerController.Init(Device.Pins.D10);

                DebugDisplayText("start motor controller");
                movementController = new MovementAbstractions();
                TBObjects.Add(movementController);

                movementController.Init(
                    Device.Pins.D13, Device.Pins.D12, Device.Pins.D11,
                    Device.Pins.D05, Device.Pins.D06, Device.Pins.D02);

                if (EnableArm)
                {
                    try
                    {
                        DebugDisplayText("Start Arm");
                        Arm = new ArmControl(i2CPWMController);
                        TBObjects.Add(Arm);
                        Arm.Init();

                    }
                    catch (Exception)
                    {

                    }
                }
                Console.WriteLine("Completed startup as far as Movement. Camera next. ");


                //Camera and Sensor

                if (EnablePanTiltControl)
                {
                    DebugDisplayText("Begining Pan Tilt control init", LogStatusMessageTypes.Important);
                    try
                    {
                        panTiltSensorCombo = new
                            PanTiltSensorCombo(
                                i2CPWMController,
                                "Pan Tilt Sensor Array");

                        panTiltSensorCombo.Init(0, 1, distController.PeriscopeDistance);
                        panTiltSensorCombo.DefaultPan = new Angle(20); 
                        panTiltSensorCombo.DefaultTilt = new Angle(45); 
                        panTiltSensorCombo.AssignBluetoothCharacteristicToUpdate(communications.charPanTilt);
                    }
                    catch (Exception e)
                    {
                        DebugDisplayText("Distance Pan Tilt broad exception: " + e.Message, LogStatusMessageTypes.Error);
                    }
                }

                if (EnableLights)
                {
                    try
                    {
                        DebugDisplayText("start lights controller", LogStatusMessageTypes.Important);
                        LightsController = new Lights();
                        LightsController.Init(false);
                    }
                    catch (Exception)
                    {

                    }
                }

                //Final

                if (EnableStatusPolling)
                {
                    DebugDisplayText("Begining regular polling", LogStatusMessageTypes.Important);
                    _statusPoller = new System.Timers.Timer(4000);
                    _statusPoller.Elapsed += _statusPoller_Elapsed;
                    _statusPoller.AutoReset = true;
                    _statusPoller.Enabled = true;

                    // enable the watchdog for 10s
                    if (EnableWatchDog)
                    {
                        Device.WatchdogEnable(TimeSpan.FromSeconds(10));
                    }
                }

                SetStatus(ComponentStatus.Ready);
                DebugDisplayText("Startup Complete", LogStatusMessageTypes.Important);
            }
            catch (Exception iex)
            {
                DebugDisplayText("Main Init Exception: " + iex.Message, LogStatusMessageTypes.Error);
                SetStatus(ComponentStatus.Error);
            }
        }

        public II2cBus Geti2cBus(I2CExpanderChannel sensor)
        {
            switch (sensor)
            {
                case I2CExpanderChannel.periscopeDistance: return i2cExpander.Bus1;
                case I2CExpanderChannel.fixedForwardDistance: return i2cExpander.Bus2;
                default: return i2cExpander.Bus0;
            }
        }

        private void _statusPoller_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (EnableWatchDog)
            {
                Device.WatchdogReset();
            }
            RefreshStatus();
        }

        public void RefreshStatus()
        {
            try
            {
                SetStatus(ComponentStatus.None);

                foreach (var element in TBObjects)
                {
                    SetStatus(element.Status);

                    try
                    {
                        if (!element.Disabled)
                        {
                            if (element.Status == ComponentStatus.Error ||
                                element.Status == ComponentStatus.UnInitialised)
                            {
                                switch (element.ErrorResponse)
                                {
                                    case AutomaticErrorResponse.DoNothing: break;
                                    case AutomaticErrorResponse.TryReload: break;
                                    case AutomaticErrorResponse.Warn: break;
                                    case AutomaticErrorResponse.DisableComponent: break;
                                    case AutomaticErrorResponse.DisableMotorPower: try { powerController.Disconnect(); } catch (Exception) { }; break;
                                }

                                DebugDisplayText(element.GetType().ToString() + " inopperable.  : ", LogStatusMessageTypes.Error);

                                break;
                            }
                            else
                            {
                                element.ErrorCount = 0;
                            }
                        }
                    }
                    catch (Exception exInner)
                    {
                        element.ErrorCount++;
                        DebugDisplayText("Global status check inner error" + exInner.Message, LogStatusMessageTypes.Error);

                        if (element.ErrorCount > element.ErrorTriggerCount)
                        {
                            element.Disabled = true;
                            element.Status = ComponentStatus.Error;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebugDisplayText("Global status check broad error" + ex.Message, LogStatusMessageTypes.Error);
            }
        }

        private void SetStatus(ComponentStatus newStatus)
        {
            //None is to be ignored
            if (newStatus != _status && 
                newStatus != ComponentStatus.None)
            {
                _status = newStatus;
        
                blueLED.State = false;
                greenLED.State = false;
                redLED.State = false;
        
                switch (newStatus)
                {
                    case ComponentStatus.Error:
                        redLED.State = true;
                        DebugDisplayText("Status set to: " + newStatus.ToString(), LogStatusMessageTypes.Error);
                        break;
                    case ComponentStatus.Action:
                        blueLED.State = true;
                        DebugDisplayText("Status set to: " + newStatus.ToString(), LogStatusMessageTypes.Important);
                        break;
                    case ComponentStatus.Ready:
                        greenLED.State = true;
                        DebugDisplayText("Status set to: " + newStatus.ToString(), LogStatusMessageTypes.Important);
                        break;
                    default:
                        break;
                }
            }
        }

        public void DebugDisplayText(string newText, LogStatusMessageTypes statusType = LogStatusMessageTypes.Debug, string Remote_Request_ID = null)
        {
            if (Logger == null)
            {
                Console.WriteLine(String.Concat("pre-logger-msg: ", statusType, " ", newText));
            }
            else
            {
                Logger.AddLogEntry(newText, statusType, Remote_Request_ID);
            }
        }
    }
}
