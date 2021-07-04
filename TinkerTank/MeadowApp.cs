using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Peripherals;
using System;
using System.Threading;
using Utilities.Power;

namespace TinkerTank
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        public IMovementInterface movementController;
        public PowerControl powerController;

        IDigitalOutputPort blueLED;
        IDigitalOutputPort greenLED;
        IDigitalOutputPort redLED;

        public enum MainStatus { Error, Ready, Action };

        public enum StatusMessageTypes { Debug, Important, Error };

        public MeadowApp()
        {
            Init();
        }

        void Init()
        {
            //Indicators to see what's going on
            blueLED = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedBlue);
            greenLED = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedGreen);
            redLED = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedRed);

            movementController = new TrackControl(this);
            movementController.Init(
                Device.Pins.D02, Device.Pins.D03, Device.Pins.D04,
                Device.Pins.D09, Device.Pins.D10, Device.Pins.D11);

            powerController = new PowerControl(this);
            powerController.Init(Device.Pins.D00);
        }

        public void SetStatus(MainStatus status)
        {
            blueLED.State = false;
            greenLED.State = false;
            redLED.State = false;

            switch (status)
            {
                case MainStatus.Error: 
                    redLED.State = true;
                    break;
                case MainStatus.Action:
                    blueLED.State = true;
                    break;
                case MainStatus.Ready:
                    greenLED.State = true;
                    break;
                default:
                    break;
            }
        }

        public void DebugDisplayText(string textToShow, StatusMessageTypes statusType = StatusMessageTypes.Debug, bool clearFirst = false, bool ConsoleOnly = false)
        {
            Console.WriteLine(textToShow);

            //if (!ConsoleOnly && lcd != null)
            //{
            //    lcd.AddNewLineOfText(textToShow, statusType, clearFirst);
            //}            
        }
    }
}
