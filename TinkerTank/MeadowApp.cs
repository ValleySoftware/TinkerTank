using Base;
using Communications;
using Display;
using Enumerations;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Units;
using Peripherals;
using Servos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TinkerTank.Abstractions;
using TinkerTank.MiscPeriherals;
using TinkerTank.Movement;
using TinkerTank.Sensors;
using TinkerTank.Servos;
using Utilities.Power;

namespace TinkerTank
{

    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        public Lights LightsController;

        public MovementAbstractions movementController;
        public PowerControl powerController;

        public LCDDisplay_ST7789 lcd;

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
        private bool EnableDisplay = false;
        private bool EnableArm = false;
        private bool EnablePCA9685 = true;
        private bool EnableStatusPolling = true;

        public bool ShowDebugLogs = true;

        public MeadowApp()
        {
            Init();
        }

        void Init()
        {
            try
            {
                TBObjects = new List<TinkerBase>();

                //InitialisePinRegister();

                //BIOS
                
                blueLED = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedBlue);
                greenLED = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedGreen);
                redLED = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedRed);

                //Communications and control

                DebugDisplayText("start communications controller", DisplayStatusMessageTypes.Important);
                communications = new BlueTooth(Device as F7Micro, this);
                TBObjects.Add(communications);
                communications.Init();

                //Display
                if (EnableDisplay)
                {
                    try
                    {
                        DebugDisplayText("start lcd", DisplayStatusMessageTypes.Important);
                        lcd = new LCDDisplay_ST7789(this);
                        TBObjects.Add(lcd);
                        lcd.Init();
                    }
                    catch (Exception)
                    {

                    }
                }

                //I2C

                DebugDisplayText("Init i2c");
                primaryi2CBus = Device.CreateI2cBus(I2cBusSpeed.Standard);

                DebugDisplayText("Init i2c Expander");
                i2cExpander = new Tca9548a(primaryi2CBus, 0x70);

                //Sensors

                if (EnableDistanceSensors)
                {
                    DebugDisplayText("Start distance sensor controller");

                    distController = new DistanceSensorController(Device, this, i2cExpander);

                    TBObjects.Add(distController);
                    distController.Init();
                }

                if (EnablePCA9685)
                {
                    DebugDisplayText("start pca9685", DisplayStatusMessageTypes.Important);
                    i2CPWMController = new PCA9685(this, primaryi2CBus);
                    TBObjects.Add(i2CPWMController);
                    i2CPWMController.Init();
                }

                //Movement and power    

                DebugDisplayText("start power controller", DisplayStatusMessageTypes.Important);
                powerController = new PowerControl(this);
                TBObjects.Add(powerController);
                powerController.Init(Device.Pins.D10);

                DebugDisplayText("start motor controller");
                movementController = new MovementAbstractions(this);
                TBObjects.Add((TinkerBase)movementController);

                movementController.Init(
                    Device.Pins.D13, Device.Pins.D12, Device.Pins.D11,
                    Device.Pins.D05, Device.Pins.D06, Device.Pins.D02);

                if (EnableArm)
                {
                    try
                    {
                        DebugDisplayText("Start Arm");
                        Arm = new ArmControl(this, i2CPWMController);
                        TBObjects.Add((TinkerBase)Arm);
                        Arm.Init();

                    }
                    catch (Exception)
                    {

                    }
                }   


                DebugDisplayText("Begining Camera and Sensor init", DisplayStatusMessageTypes.Important);
                //Camera and Sensor

                if (EnablePanTiltSensors)
                {
                    try
                    {
                        panTiltSensorCombo = new
                            PanTiltSensorCombo(
                                this,
                                i2CPWMController,
                                "Pan Tilt Sensor Array");

                        panTiltSensorCombo.Init(2, 3, distController.PeriscopeDistance);
                        panTiltSensorCombo.DefaultPan = new Angle(140); //Bigger number = counter clockwise
                        panTiltSensorCombo.DefaultTilt = new Angle(160); //Bigger number = forward/down
                        panTiltSensorCombo.GoToDefault();
                        panTiltSensorCombo.AssignBluetoothCharacteristicToUpdate(communications.charPanTilt);
                    }
                    catch (Exception e)
                    {
                        DebugDisplayText("Distance Pan Tilt broad exception: " + e.Message, DisplayStatusMessageTypes.Error);
                    }
                }


                try
                {
                    LightsController = new Lights(Device, this);
                    LightsController.Init();
                }
                catch (Exception)
                {

                }


                //Final

                if (EnableStatusPolling)
                {
                    DebugDisplayText("Begining regular polling", DisplayStatusMessageTypes.Important);
                    _statusPoller = new System.Timers.Timer(2000);
                    _statusPoller.Elapsed += _statusPoller_Elapsed;
                    _statusPoller.AutoReset = true;
                    _statusPoller.Enabled = true;
                }

                DebugDisplayText("Startup Complete", DisplayStatusMessageTypes.Important);
            }
            catch (Exception iex)
            {
                DebugDisplayText("Main Init Exception: " + iex.Message, DisplayStatusMessageTypes.Error);
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

                    DebugDisplayText(element.GetType().ToString() + " inopperable.  : ", DisplayStatusMessageTypes.Error);
                    
                    break;
                }
            }
        }

        private void SetStatus(ComponentStatus newStatus)
        {
            if (newStatus != _status)
            {
                _status = newStatus;
        
                blueLED.State = false;
                greenLED.State = false;
                redLED.State = false;
        
                switch (newStatus)
                {
                    case ComponentStatus.Error:
                        redLED.State = true;
                        DebugDisplayText("Status set to: " + newStatus.ToString(), DisplayStatusMessageTypes.Error);
                        break;
                    case ComponentStatus.Action:
                        blueLED.State = true;
                        DebugDisplayText("Status set to: " + newStatus.ToString(), DisplayStatusMessageTypes.Important);
                        break;
                    case ComponentStatus.Ready:
                        greenLED.State = true;
                        DebugDisplayText("Status set to: " + newStatus.ToString(), DisplayStatusMessageTypes.Important);
                        break;
                    default:
                        break;
                }
        
        
            }
        }

        public void NextDebugMessage()
        {
            if (lcd != null && lcd.Log.Count > 0)
            {
                var oldIndex = lcd.Log.IndexOf(lcd.CurrentLog);

                if (oldIndex >= 0)
                {
                    ShowDebugMessage(oldIndex - 1);
                }
            }
        }

        public void PreviousDebugMessage()
        {
            if (lcd != null && lcd.Log.Count > 0)
            {
                var oldIndex = lcd.Log.IndexOf(lcd.CurrentLog);

                if  (oldIndex >= 0)
                {
                    ShowDebugMessage(oldIndex + 1);
                }
            }
        }

        public void FirstMessage()
        {
            if (lcd != null && lcd.Log.Count > 0)
            {
                lcd.CurrentLog = lcd.Log[lcd.Log.Count - 1];
            }
        }

        public void LastMessage()
        {
            if (lcd != null && lcd.Log.Count > 0)
            {
                lcd.CurrentLog = lcd.Log[0];
            }
        }

        public void ShowDebugMessage(int messageIndex)
        {
            if (lcd != null && lcd.Log.Count > messageIndex)
            {

            }
        }

        public void DebugDisplayText(string newText, DisplayStatusMessageTypes statusType = DisplayStatusMessageTypes.Debug)
        {
                var t = new Task(() =>
                {
                    Console.WriteLine(newText);
        
                    if (lcd != null)
                    {
                        try
                        {
                            lcd.AddMessage(newText, statusType);
                        }
                        catch (Exception)
                        {
                            //Display add process went through,but not talking.  Is it plugged in?
                        }
                    }
                    });
                t.Start();
        }
    }
}
