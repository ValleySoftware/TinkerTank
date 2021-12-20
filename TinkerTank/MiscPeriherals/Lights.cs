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

        private List<Led> LightList;
        private Led _fixedForwardLed;

        public Lights(F7Micro device, MeadowApp appRoot)
        {
            _appRoot = appRoot;
            _device = device;

            LightList = new List<Led>();

            ErrorResponse = AutomaticErrorResponse.DoNothing;
        }

        public void Init()
        {
            Status = ComponentStatus.UnInitialised;

            try
            {
                _appRoot.DebugDisplayText("LED init method started.", DisplayStatusMessageTypes.Debug);

                _fixedForwardLed = new Led(_device.CreateDigitalOutputPort(_device.Pins.D04));
                LightList.Add(_fixedForwardLed);
                LEDOn(_fixedForwardLed, false);

                _appRoot.DebugDisplayText("LED init method complete, setting ready.", DisplayStatusMessageTypes.Debug);
                Status = ComponentStatus.Ready;
            }
            catch (Exception ex)
            {
                _appRoot.DebugDisplayText("LED error: " + ex.Message, DisplayStatusMessageTypes.Error);
                Status = ComponentStatus.Error;
            }

        }

        public void RequestLightsDo(string payload)
        {

            //lightIdentifier-newOnValue
            //00-000-00000

            _appRoot.DebugDisplayText("LED request: " + payload, DisplayStatusMessageTypes.Error);

            var sp = payload.Split("-");

            if (sp.Count() == 2)
            {
                try
                {
                    int lightIndex = Convert.ToInt32(sp[0]);
                    int newStatus = Convert.ToInt32(sp[1]);
                    bool newStatusBool = false;

                    var l = LightList[lightIndex];

                    if (newStatus == 1)
                    {
                        newStatusBool = true;
                    }

                    LEDOn(l, newStatusBool);
                }
                catch (Exception)
                {

                }
            }
        }


        private void LEDOn(Led ledToChange, bool newValue)
        {
            try
            {
                if (ledToChange == null)
                {
                    return;
                }

                ledToChange.IsOn = newValue;
            }
            catch (Exception)
            {

            }
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
