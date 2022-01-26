using Base;
using Enumerations;
using Meadow.Foundation;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Text;
using TinkerTank.Data;

namespace TinkerTank.Display
{
    public abstract partial class DisplayBase : TinkerBase
    {
        public static List<Color> statusColours = new List<Color>() { Color.White, Color.Green, Color.Red };
        public enum DisplayTypes { ST7789_SPI_240x240, SSD1306_2IC_128x64, SSD1306_2IC_128x32 }
        private DisplayTypes _typeOfDisplay;
        public MicroGraphics graphics;
        private int lineLength = 30;
        private int _noOfLinesOnDisplay = 11;
        Logging _parentLogger;

        public DisplayBase(Logging parentLogger)
        {
            _appRoot = MeadowApp.Current;
            _parentLogger = parentLogger;
        }

        protected int NoOfLinesOnDisplay
        {
            get => _noOfLinesOnDisplay;
            set => _noOfLinesOnDisplay = value;
        }

        public void ShowCurrentLog()
        {
            graphics.Clear();

            if (_parentLogger.CurrentLog != null)
            {
                try
                {
                    int i = 0;

                    var logSplitIntoLines = SplitInParts(_parentLogger.CurrentLog.Text, lineLength);

                    foreach (var lineOfLog in logSplitIntoLines)
                    {
                        if (i >= NoOfLinesOnDisplay)
                        {
                            break;
                        }
                        graphics.CurrentFont = new Font4x8();
                        graphics.DrawText(0, 12 * i, lineOfLog.Trim(), ScaleFactor.X1);
                        i++;

                    }

                    {
                        _parentLogger.CurrentLog.Displayed = true;
                    }
                }
                catch (Exception SplitLogEx)
                {
                    Console.WriteLine(" SplitLogEx " + SplitLogEx.Message);
                }
            }
            else
            {
                Console.WriteLine("LCD - No current log entry to show");
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
                    l.Add(s.Substring(currentCharIndex, lineLength));

                    currentCharIndex = currentCharIndex + lineLength;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("exception hit splitting string.");
            }
            return l;
        }

        public DisplayTypes TypeOfDisplay
        {
            get => _typeOfDisplay;
            set => _typeOfDisplay = value;
        }

        public void Init()
        {
            try
            {
                Status = ComponentStatus.UnInitialised;

                DoDisplaySpecificInit();

                graphics.CurrentFont = new Font12x20();

                graphics.Clear();
                Status = ComponentStatus.Ready;
            }
            catch (Exception ScreenInitEx)
            {
                Console.WriteLine("ScreenInitEx - " + ScreenInitEx.Message);
            }
        }

        public abstract void DoDisplaySpecificInit();

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
