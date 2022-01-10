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

namespace Display
{

    public class LCDDisplay_ST7789 : TinkerBase, ITinkerBase
    {
        public static List<Color> statusColours = new List<Color>() { Color.White, Color.Green, Color.Red };
        MicroGraphics graphics;
        St7789 display;
        static int NoOfLinesOnDisplay = 11;
        Logging _parentLogger;

        public LCDDisplay_ST7789(Logging parentLogger)
        {
            _appRoot = MeadowApp.Current;
            _parentLogger = parentLogger;
        }

        public void ShowCurrentLog()
        {
            graphics.Clear();

            if (_parentLogger.CurrentLog != null)
            {
                int i = 0;

                var logSplitIntoLines = SplitInParts(_parentLogger.CurrentLog.Text, 19);

                foreach (var lineOfLog in logSplitIntoLines)
                {
                    graphics.DrawText(0, 24 * i, lineOfLog, statusColours[(int)_parentLogger.CurrentLog.StatusType], ScaleFactor.X1);
                    i++;

                    if (i > NoOfLinesOnDisplay)
                    {
                        break;
                    }
                }

                {
                    _parentLogger.CurrentLog.Displayed = true;
                    if (_appRoot.dbcon != null)
                    {
                        _appRoot.dbcon.UpsertDebugLogEntry(_parentLogger.CurrentLog);
                    }
                }
            }

            graphics.Show();
        }

        private static List<string> SplitInParts(string s, int partLength)
        {
            var l = new List<string>();

            var currentCharIndex = 0;
            try
            {
                while (s.Length - 1 > currentCharIndex)
                {
                    int lineLength = 0;
                    if (currentCharIndex + partLength > s.Length)
                    {
                        lineLength = s.Length - currentCharIndex;
                    }
                    else
                    {
                        lineLength = partLength;
                    }
                    l.Insert(0, s.Substring(currentCharIndex, lineLength));

                    currentCharIndex = currentCharIndex + lineLength;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("exception hit splitting string.");
            }
                return l;
        }

        public void Init()
        {
            Status = ComponentStatus.UnInitialised;

            var config = new SpiClockConfiguration
            (
                speed: new Meadow.Units.Frequency(6000),
                mode: SpiClockConfiguration.Mode.Mode3
            ); ;

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
            graphics.CurrentFont = new Font12x20();

            graphics.Clear();
            Status = ComponentStatus.Ready;
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
