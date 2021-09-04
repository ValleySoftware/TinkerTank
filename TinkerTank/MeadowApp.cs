using Base;
using Communications;
using Display;
using Enumerations;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Hardware;
using Meadow.Units;
using Peripherals;
using Servos;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TinkerTank.Sensors;
//using TinkerTank.Sensors;
using Utilities.Power;
using static Meadow.Foundation.Sensors.Distance.Vl53l0x;

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

        public static bool ShowDebugLogs = false;

        public II2cBus pcaBus;
        //public II2cBus vl53Bus;

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

            //Shared

            DebugDisplayText("Init i2c");
            pcaBus = Device.CreateI2cBus();

            //Communications and control

            DebugDisplayText("start communications controller", DisplayStatusMessageTypes.Important);
            communications = new BlueTooth(Device as F7Micro, this);
            TBObjects.Add(communications);
            communications.Init();

            //Movement and power

            DebugDisplayText("start pca9986", DisplayStatusMessageTypes.Important);
            i2CPWMController = new PCA9685(Device, this, ref pcaBus);
            TBObjects.Add(i2CPWMController);
            i2CPWMController.Init();

            /*
            DebugDisplayText("start motor controller");
            movementController = new TrackControl(this);
            TBObjects.Add((TinkerBase)movementController);
            movementController.Init(
                Device.Pins.D02, Device.Pins.D03, Device.Pins.D04,
                Device.Pins.D09, Device.Pins.D10, Device.Pins.D11);
            */
            DebugDisplayText("start power controller", DisplayStatusMessageTypes.Important);
            powerController = new PowerControl(this);
            TBObjects.Add(powerController);
            powerController.Init(Device.Pins.D05);

            //Sensors

            DebugDisplayText("start distance sensor", DisplayStatusMessageTypes.Important);
            distance = new Dist53l0(Device, this, ref pcaBus);
            TBObjects.Add(distance);
            distance.Init();

            //Final

            DebugDisplayText("Begining regular polling", DisplayStatusMessageTypes.Important);
            _statusPoller = new System.Timers.Timer(2000);
            _statusPoller.Elapsed += _statusPoller_Elapsed;
            _statusPoller.AutoReset = true;
            _statusPoller.Enabled = true;
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
                    DebugDisplayText(element.GetType().ToString() + " not ready.  Exiting.", DisplayStatusMessageTypes.Error, true);
                    powerController.Disconnect();
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
                        lcd.AddNewLineOfText(textToShow, statusType, clearFirst);
                    }
                });
                t.Start();
            }
        }
    }
}
