using Meadow.Foundation.Displays.Ssd130x;
using Meadow.Foundation.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using TinkerTank.Data;

namespace TinkerTank.Display
{
    public class LCDDisplay_1306_128x32 : DisplayBase
    {
        Ssd1306 display;

        public LCDDisplay_1306_128x32(Logging parentLogger) : base(parentLogger)
        {

        }

        public override void DoDisplaySpecificInit()
        {

            NoOfLinesOnDisplay = 4;

            display = new Ssd1306
            (
                i2cBus: MeadowApp.Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.FastPlus),
                address: 60,
                displayType: Ssd1306.DisplayType.OLED128x32
            );

            graphics = new MicroGraphics(display);
        }

        public void RefreshStatus()
        {

        }

        public void Test()
        {

        }

        public void ErrorEncountered()
        {

        }
    }
}
