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
        static int NoOfLinesOnDisplay = 11;
        private List<StatusMessage> Log;
        private StatusMessage CurrentLog;

        public static bool ShowDebugLogs = true;

        public LCDDisplay_ST7789(MeadowApp appRoot)
            {
                _appRoot = appRoot;
            }

        public void AddMessage(string textToDisplay, DisplayStatusMessageTypes statusType = DisplayStatusMessageTypes.Debug)
        {
            if (Log == null)
            {
                Log = new List<StatusMessage>();
            }

            Log.Add(new StatusMessage() { Text = textToDisplay, StatusType = statusType, TimeLogged = DateTimeOffset.Now });

            if (ShowDebugLogs ||
                statusType != DisplayStatusMessageTypes.Debug)
            {
                CurrentLog = Log[0];
                UpdateDisplay(CurrentLog);
            }

        }

        private void UpdateDisplay(StatusMessage messageToShow = null)
        {
            graphics.Clear();

            CurrentLog = messageToShow;

            if (CurrentLog != null)
            {
                int i = 0;

                var logSplitIntoLines = SplitInParts(CurrentLog.Text, 19);

                foreach (var lineOfLog in logSplitIntoLines)
                {
                    //Console.WriteLine(lineOfLog);
                    graphics.DrawText(0, 24 * i, lineOfLog, StatusMessage.statusColours[(int)CurrentLog.StatusType], GraphicsLibrary.ScaleFactor.X1);
                    i++;

                    if (i > NoOfLinesOnDisplay)
                    {
                        break;
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


        public void RefreshDisplay()
            {
                UpdateDisplay(CurrentLog);
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

        public void ErrorEncountered()
        {

        }
    }
}
