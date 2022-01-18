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


    public class MeadowApp : App<F7Micro, MeadowApp>
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

        private II2cBus primaryi2CBus;

        IDigitalOutputPort blueLED;
        IDigitalOutputPort greenLED;
        IDigitalOutputPort redLED;
        ComponentStatus _status;

        private List<TinkerBase> TBObjects;
        private System.Timers.Timer _statusPoller;

        private bool EnableDistanceSensors = true;
        private bool EnablePanTiltSensors = true;
        public bool EnableDisplay = true;
        private bool EnableArm = false;
        private bool EnablePCA9685 = true;
        private bool EnableStatusPolling = true;
        public DisplayTypes DisplayModel = DisplayTypes.SSD1306_2IC_128x32;

        public LogStatusMessageTypes MinimumLogLevel = LogStatusMessageTypes.Debug;

        public Logging Logger;

        public DataStore dbcon;
        private bool WipeDBOnStartup = true;

        public MeadowApp()
        {
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Init();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Unhandled Exception Raised to Domain");
            //DebugDisplayText("Unhandled Exception Raised to Domain. " + sender.ToString(), LogStatusMessageTypes.Error);
        }

        private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            Console.WriteLine("Unhandled First Chance Exception Raised to Domain");
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
            }
            catch (Exception)
            {
                //very bad
            }

            //I2C

            try
            {
                DebugDisplayText("Init i2c");
                primaryi2CBus = Device.CreateI2cBus(I2cBusSpeed.Standard);
            }
            catch (Exception)
            {
                //very bad
            }

            try
            {
                //Initialise the database
                DebugDisplayText("Init DB", LogStatusMessageTypes.Important);
                dbcon = new DataStore();
                dbcon.InitDB(WipeDBOnStartup);

            }
            catch (Exception dbMasterEx)
            {

            }

            try
            {
                //Initialise the logger
                Logger = new Logging();
                Logger.Init(dbcon);
            }
            catch (Exception logEx)
            {

            }

            try
            { 
                TBObjects = new List<TinkerBase>();

                //Communications and control

                DebugDisplayText("start communications controller", LogStatusMessageTypes.Important);
                communications = new BlueTooth();
                TBObjects.Add(communications);
                communications.Init();

                //I2C Expander
                DebugDisplayText("Init i2c Expander");
                i2cExpander = new Tca9548a(primaryi2CBus, 0x70);

                //Sensors

                if (EnableDistanceSensors)
                {
                    DebugDisplayText("Start distance sensor controller");

                    distController = new DistanceSensorController(i2cExpander);

                    TBObjects.Add(distController);
                    distController.Init();
                }

                if (EnablePCA9685)
                {
                    DebugDisplayText("start pca9685", LogStatusMessageTypes.Important);
                    i2CPWMController = new PCA9685(primaryi2CBus);
                    TBObjects.Add(i2CPWMController);
                    i2CPWMController.Init();
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
                    { }
                }   


                DebugDisplayText("Begining Camera and Sensor init", LogStatusMessageTypes.Important);
                //Camera and Sensor

                if (EnablePanTiltSensors)
                {
                    try
                    {
                        panTiltSensorCombo = new
                            PanTiltSensorCombo(
                                i2CPWMController,
                                "Pan Tilt Sensor Array");

                        panTiltSensorCombo.Init(2, 3, distController.PeriscopeDistance);
                        panTiltSensorCombo.DefaultPan = new Angle(140); //Bigger number = counter clockwise
                        panTiltSensorCombo.DefaultTilt = new Angle(160); //Bigger number = forward/down
                        panTiltSensorCombo.AssignBluetoothCharacteristicToUpdate(communications.charPanTilt);
                        //panTiltSensorCombo.GoToDefault();
                    }
                    catch (Exception e)
                    {
                        DebugDisplayText("Distance Pan Tilt broad exception: " + e.Message, LogStatusMessageTypes.Error);
                    }
                }


                try
                {
                    LightsController = new Lights();
                    LightsController.Init();
                }
                catch (Exception)
                {

                }


                //Final

                if (EnableStatusPolling)
                {
                    DebugDisplayText("Begining regular polling", LogStatusMessageTypes.Important);
                    _statusPoller = new System.Timers.Timer(2000);
                    _statusPoller.Elapsed += _statusPoller_Elapsed;
                    _statusPoller.AutoReset = true;
                    _statusPoller.Enabled = true;
                }

                DebugDisplayText("Startup Complete", LogStatusMessageTypes.Important);
            }
            catch (Exception iex)
            {
                DebugDisplayText("Main Init Exception: " + iex.Message, LogStatusMessageTypes.Error);
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
            RefreshStatus();
        }

        public void RefreshStatus()
        {
            SetStatus(ComponentStatus.None);

            foreach (var element in TBObjects)
            {
                SetStatus(element.Status);

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

        public void DebugDisplayText(string newText, LogStatusMessageTypes statusType = LogStatusMessageTypes.Debug)
        {
            if (Logger == null)
            {
                Console.WriteLine(String.Concat("pre-logger-msg: ", statusType, " ", newText));
            }
            else
            {
                Logger.AddLogEntry(newText, statusType);
            }
        }
    }
}
