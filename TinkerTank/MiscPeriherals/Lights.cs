using Base;
using Enumerations;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinkerTank.MiscPeriherals
{
    public class Lights : TinkerBase
    {
        F7Micro _device;

        private List<Meadow.Hardware.IDigitalOutputPort> LightList;
        private Meadow.Hardware.IDigitalOutputPort _fixedForwardLed;

        public Lights()
        {
            _appRoot = MeadowApp.Current;
            _device = MeadowApp.Device;

            LightList = new List<Meadow.Hardware.IDigitalOutputPort>();

            ErrorResponse = AutomaticErrorResponse.DoNothing;
        }

        public void Init(bool startWithLightsOn = false)
        {
            Status = ComponentStatus.UnInitialised;

            try
            {
                _appRoot.DebugDisplayText("LED init method started.", LogStatusMessageTypes.Debug);

                _fixedForwardLed = _device.CreateDigitalOutputPort(_device.Pins.D03);
                LightList.Add(_fixedForwardLed);
                LEDOn(_fixedForwardLed, startWithLightsOn);

                _appRoot.DebugDisplayText("LED init method complete, setting ready.", LogStatusMessageTypes.Debug);
                Status = ComponentStatus.Ready;
            }
            catch (Exception ex)
            {
                _appRoot.DebugDisplayText("LED error: " + ex.Message, LogStatusMessageTypes.Error);
                Status = ComponentStatus.Error;
            }

        }

        public void RequestLightsDo(string[] payload)
        {

            //lightIdentifier-newOnValue
            //00-000-00000

            _appRoot.DebugDisplayText("LED request: " + payload[2], LogStatusMessageTypes.Error);


            if (payload.Count() > 2)
            {
                try
                {
                    int lightIndex = Convert.ToInt32(payload[1]);
                    int newStatus = Convert.ToInt32(payload[2]);
                    bool newStatusBool = false;

                    var l = LightList[lightIndex];

                    if (newStatus == 1)
                    {
                        newStatusBool = true;
                    }

                    LEDOn(l, newStatusBool);
                }
                catch (Exception e)
                {
                    _appRoot.DebugDisplayText("RequestLightsDo method exeption."  + e.Message, LogStatusMessageTypes.Error);
                    Status = ComponentStatus.Error;
                }
            }
        }


        private void LEDOn(Meadow.Hardware.IDigitalOutputPort ledToChange, bool newValue)
        {
            try
            {
                if (ledToChange == null)
                {
                    return;
                }

                ledToChange.State = newValue;
                _appRoot.DebugDisplayText("LED on property changed to " + newValue, LogStatusMessageTypes.Information);
            }
            catch (Exception e)
            {
                _appRoot.DebugDisplayText("Error changing LED state." + e.Message, LogStatusMessageTypes.Error);
                Status = ComponentStatus.Error;
            }
        }

        public void ErrorEncountered()
        {
            //throw new NotImplementedException();
        }

        public void RefreshStatus()
        {
            //throw new NotImplementedException();
        }

        public void Test()
        {
            //throw new NotImplementedException();
        }
    }
}
