using Base;
using Enumerations;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using System;
using System.Collections.Generic;
using System.Text;

namespace TinkerTank.MiscPeriherals
{
    public class Lights : TinkerBase
    {
        F7Micro _device;

        private Led led;

        public Lights(F7Micro device, MeadowApp appRoot)
        {
            _appRoot = appRoot;
            _device = device;

            ErrorResponse = AutomaticErrorResponse.DoNothing;
        }

        public void Init()
        {
            Status = ComponentStatus.UnInitialised;

            try
            {
                _appRoot.DebugDisplayText("LED init method started.", DisplayStatusMessageTypes.Debug);

                led = new Led(_device.CreateDigitalOutputPort(_device.Pins.D04));

                _appRoot.DebugDisplayText("LED init method complete, setting ready.", DisplayStatusMessageTypes.Debug);
                Status = ComponentStatus.Ready;
            }
            catch (Exception ex)
            {
                _appRoot.DebugDisplayText("LED error: " + ex.Message, DisplayStatusMessageTypes.Error);
                Status = ComponentStatus.Error;
            }

        }

        public void LEDOn(bool on)
        {
            if (led == null)
            {
                return;
            }

            led.IsOn = on;
        }

        public void ErrorEncountered()
        {
            throw new NotImplementedException();
        }

        public void RefreshStatus()
        {
            throw new NotImplementedException();
        }

        public void Test()
        {
            throw new NotImplementedException();
        }
    }
}
