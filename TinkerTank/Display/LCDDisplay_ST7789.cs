using Base;
using Enumerations;
using Meadow;
using Meadow.Foundation;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkerTank;
using TinkerTank.Data;

namespace TinkerTank.Display
{

    public class LCDDisplay_ST7789 : DisplayBase
    {
        St7789 display;

        public LCDDisplay_ST7789(Logging parentLogger) :base(parentLogger)
        {

        }

        public override void DoDisplaySpecificInit()
        {
            NoOfLinesOnDisplay = 20;

            var config = new SpiClockConfiguration
                (
                    speed: new Meadow.Units.Frequency(6000),
                    mode: SpiClockConfiguration.Mode.Mode3
                );

                display = new St7789
                (
                    device: MeadowApp.Device,
                    spiBus: MeadowApp.Device.CreateSpiBus(
                        MeadowApp.Device.Pins.SCK, MeadowApp.Device.Pins.MOSI, MeadowApp.Device.Pins.MISO, config),
                    chipSelectPin: MeadowApp.Device.Pins.D15,
                    dcPin: MeadowApp.Device.Pins.D01,
                    resetPin: MeadowApp.Device.Pins.D00,
                    width: 240, height: 240
                );

                graphics = new MicroGraphics(display);
        }

        public override void Init()
        {
            DoDisplaySpecificInit();
            base.Init();
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
