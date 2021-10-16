using Base;
using Communications;
using Display;
using Enumerations;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Peripherals;
using Servos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TinkerTank.Movement;
using TinkerTank.Sensors;
using TinkerTank.Servos;
using Utilities.Power;

namespace TinkerTank
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        public IMovementInterface movementController;
        public PowerControl powerController;

        public LCDDisplay_ST7789 lcd;

        public BlueTooth communications;

        public PCA9685 i2CPWMController;

        public PushButton button;

        public Dist53l0 distance;
        public readonly int PWMFrequency = 50;

        public ArmControl Arm;

        public List<PanTiltBase> PanTilts = new List<PanTiltBase>();
        public PanTiltBase DriveCameraMovement;
        public PanTiltDistance PeriscopeCameraMovement;

        public static bool ShowDebugLogs = false;

        public II2cBus i2CBus;

        IDigitalOutputPort blueLED;
        IDigitalOutputPort greenLED;
        IDigitalOutputPort redLED;
        ComponentStatus _status;

        private List<TinkerBase> TBObjects;
        private System.Timers.Timer _statusPoller;

        public MeadowApp()
        {
            Init();
        }

        void Init()
        {
            try
            {
                TBObjects = new List<TinkerBase>();


                //Display And Logging

                //Indicators to see what's going on
                blueLED = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedBlue);
                greenLED = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedGreen);
                redLED = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedRed);

                DebugDisplayText("start lcd", DisplayStatusMessageTypes.Important);
                lcd = new LCDDisplay_ST7789(this);
                TBObjects.Add(lcd);
                lcd.Init();

                //Communications and control

                DebugDisplayText("start communications controller", DisplayStatusMessageTypes.Important);
                communications = new BlueTooth(Device as F7Micro, this);
                TBObjects.Add(communications);
                communications.Init();

                //Shared

                DebugDisplayText("Init i2c");
                i2CBus = Device.CreateI2cBus();

                DebugDisplayText("start pca9685", DisplayStatusMessageTypes.Important);
                i2CPWMController = new PCA9685(this, ref i2CBus);
                TBObjects.Add(i2CPWMController);
                i2CPWMController.Init();

                //Movement and power            

                DebugDisplayText("start power controller", DisplayStatusMessageTypes.Important);
                powerController = new PowerControl(this);
                TBObjects.Add(powerController);
                powerController.Init(Device.Pins.D10);

                DebugDisplayText("start motor controller");
                movementController = new TrackControl(Device, i2CPWMController, this);
                TBObjects.Add((TinkerBase)movementController);

                movementController.Init(
                    Device.Pins.D13, Device.Pins.D12, Device.Pins.D11,
                    Device.Pins.D05, Device.Pins.D06, Device.Pins.D02);

                DebugDisplayText("Start Arm");
                Arm = new ArmControl(this, i2CPWMController);
                TBObjects.Add((TinkerBase)Arm);
                Arm.Init();

                DebugDisplayText("Begining Camera and Sensor init", DisplayStatusMessageTypes.Important);
                //Camera and Sensor

                DriveCameraMovement = new
                   PanTiltBase(
                       this,
                       i2CPWMController.GetPwmPort(0),
                       i2CPWMController.GetPwmPort(1),
                       "DriveCamera",
                       ServoType.SG90Standard);

                PanTilts.Add(DriveCameraMovement);
                DriveCameraMovement.Init();
                DriveCameraMovement.DefaultPan = 110;
                DriveCameraMovement.DefaultTilt = 100;
                DriveCameraMovement.GoToDefault();

                PeriscopeCameraMovement = new
                    PanTiltDistance(
                        this,
                        i2CPWMController.GetPwmPort(2),
                        i2CPWMController.GetPwmPort(3),
                        "PeriscopeCamera",
                        ServoType.SG90Standard,
                        MeadowApp.Device.Pins.D09);

                PanTilts.Add(PeriscopeCameraMovement);
                PeriscopeCameraMovement.Init();
                PeriscopeCameraMovement.DefaultPan = 55;
                PeriscopeCameraMovement.DefaultTilt = 40;
                PeriscopeCameraMovement.GoToDefault();

                //Final

                DebugDisplayText("Begining regular polling", DisplayStatusMessageTypes.Important);
                _statusPoller = new System.Timers.Timer(2000);
                _statusPoller.Elapsed += _statusPoller_Elapsed;
                _statusPoller.AutoReset = true;
                _statusPoller.Enabled = true;

                DebugDisplayText("Startup Complete", DisplayStatusMessageTypes.Important);
            }
            catch (Exception iex)
            {
                DebugDisplayText("Main Init Exception: " + iex.Message, DisplayStatusMessageTypes.Error);
            }
        }

        private void _statusPoller_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            RefreshStatus();
        }

        public void RefreshStatus()
        {
            //DebugDisplayText("Checking Component Status", DisplayStatusMessageTypes.Debug, false);
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
                        DebugDisplayText("Status set to: " + newStatus.ToString(), DisplayStatusMessageTypes.Error, false);
                        break;
                    case ComponentStatus.Action:
                        blueLED.State = true;
                        DebugDisplayText("Status set to: " + newStatus.ToString(), DisplayStatusMessageTypes.Important, false);
                        break;
                    case ComponentStatus.Ready:
                        greenLED.State = true;
                        DebugDisplayText("Status set to: " + newStatus.ToString(), DisplayStatusMessageTypes.Important, true);
                        break;
                    default:
                        break;
                }
        
        
            }
        }

        public void DebugDisplayText(string textToShow, DisplayStatusMessageTypes statusType = DisplayStatusMessageTypes.Debug, bool clearFirst = false, bool ConsoleOnly = false)
        {
            if (ShowDebugLogs ||
                statusType != DisplayStatusMessageTypes.Debug)
            {
                var t = new Task(() =>
                {
                    Console.WriteLine(textToShow);
        
                    if (!ConsoleOnly && lcd != null)
                    {
                        try
                        {
                            lcd.AddNewLineOfText(textToShow, statusType, clearFirst);
                        }
                        catch (Exception displayError)
                        {
                            //Display add process went through,but not talking.  Is it plugged in?
                        }
                    }
                    });
                t.Start();
            }
        }
    }
}
