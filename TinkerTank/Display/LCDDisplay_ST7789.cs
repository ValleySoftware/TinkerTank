using Base;
using Enumerations;
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

namespace Display
{
    public class StatusMessage
    {
        public string Text { get; set; }
        public DisplayStatusMessageTypes StatusType { get; set; }
        public DateTimeOffset TimeLogged { get; set; }
        public static List<Color> statusColours = new List<Color>() { Color.White, Color.Green, Color.Red };
    }

    public class LCDDisplay_ST7789 : TinkerBase, ITinkerBase
    {
        GraphicsLibrary graphics;
        St7789 display;
        static int NoOfLinesToShow = 11;
        private List<StatusMessage> Log;

        public LCDDisplay_ST7789(MeadowApp appRoot)
            {
                _appRoot = appRoot;
            }

        public void AddNewLineOfText(string textToDisplay, DisplayStatusMessageTypes statusType = DisplayStatusMessageTypes.Debug, bool clearFirst = false)
            {
                if (Log == null)
                {
                    Log = new List<StatusMessage>();
                }

                if (clearFirst)
                {
                    Log.Clear();
                }

                Log.Insert(0, new StatusMessage() { Text = textToDisplay, StatusType = statusType, TimeLogged = DateTimeOffset.Now });
                UpdateDisplay();

            }

        private void UpdateDisplay()
            {
                graphics.Clear();

                int i = 0;

                var noOfLinesAvailable = Math.Min(NoOfLinesToShow, Log.Count);
                --noOfLinesAvailable;

                while (i < noOfLinesAvailable)
                {
                    graphics.DrawText(0, 24 * i, Log[i].Text, StatusMessage.statusColours[(int)Log[i].StatusType], GraphicsLibrary.ScaleFactor.X1);
                    i++;
                }

                graphics.Show();
            }

        public void RefreshDisplay()
            {
                UpdateDisplay();
            }

        public void Init()
        {
            Status = ComponentStatus.UnInitialised;
            //_appRoot.DebugDisplayText("Init LCD");

            var config = new SpiClockConfiguration
            (
                speedKHz: 6000,
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

            graphics = new GraphicsLibrary(display);
            graphics.CurrentFont = new Font12x20();

            graphics.Clear();
            Status = ComponentStatus.Ready;
            //_appRoot.DebugDisplayText("LCD Ready", StatusMessageTypes.Important);
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
