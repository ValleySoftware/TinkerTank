using Base;
using TinkerTank.Display;
using Enumerations;
using Meadow.Devices;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TinkerTank.Data
{
    public class Logging : TinkerBase, ITinkerBase
    {
        private DataStore dbcon;
        private DisplayBase lcd;
        private DebugLogEntryModel _currentLog;


        public Logging() : base()
        {

        }

        public void Init(DataStore con)
        {
            _appRoot = MeadowApp.Current;

            dbcon = con;

            //Display
            if (_appRoot.EnableDisplay)
            {
                try
                {
                    AddLogEntry("start lcd", LogStatusMessageTypes.Important);

                    AddLogEntry(_appRoot.DisplayModel.ToString(), LogStatusMessageTypes.Important);

                    if (_appRoot.DisplayModel == DisplayBase.DisplayTypes.ST7789_SPI_240x240)
                    { 
                        lcd = new LCDDisplay_ST7789(this);
                        lcd.Init();
                    }

                    if (_appRoot.DisplayModel == DisplayBase.DisplayTypes.SSD1306_2IC_128x64)
                    {
                        lcd = new LCDDisplay_1306_128x64(this);
                        lcd.Init();
                    }

                    if (_appRoot.DisplayModel == DisplayBase.DisplayTypes.SSD1306_2IC_128x32)
                    {
                        lcd = new LCDDisplay_1306_128x32(this);
                        lcd.Init();
                    }
                }
                catch (Exception ex)
                {
                    AddLogEntry("init lcd error " + ex.Message, LogStatusMessageTypes.Error);
                }
            }
        }

        public DebugLogEntryModel CurrentLog
        {
            get => _currentLog;
            set
            {
                _currentLog = value;
            }
        }

        public void AddLogEntry(
            string newText, 
            LogStatusMessageTypes statusType = LogStatusMessageTypes.Debug, 
            string remoteID = null)
        {
            if (dbcon != null)
            {
                Task t = new Task(() =>
                {
                    try
                    {

                        if (statusType >= _appRoot.MinimumLogLevel)
                        {
                            var newEntry = new DebugLogEntryModel()
                            {
                                RecordedStamp = DateTimeOffset.Now,
                                Displayed = false,
                                StatusType = statusType,
                                Text = newText,
                                Remote_Request_ID = remoteID
                            };

                            if (dbcon != null)
                            {
                                dbcon.UpsertDebugLogEntry(newEntry);
                            }

                            if (newEntry.StatusType == LogStatusMessageTypes.BLERecord)
                            {
                                Console.WriteLine(String.Concat("-BLE- ", remoteID, " - (", newEntry.ID, ") ", newEntry.Text));
                            }

                            if (newEntry.StatusType == LogStatusMessageTypes.CriticalError)
                            {
                                Console.WriteLine(String.Concat("*** (", newEntry.ID, ") ", newEntry.Text));
                            }

                            if (newEntry.StatusType == LogStatusMessageTypes.Error)
                            {
                                Console.WriteLine(String.Concat("* (", newEntry.ID, ") ", newEntry.Text));
                            }

                            if (newEntry.StatusType == LogStatusMessageTypes.Important)
                            {
                                Console.WriteLine(String.Concat("// (", newEntry.ID, ") ", newEntry.Text));
                            }
                            Console.WriteLine(String.Concat("(", newEntry.ID, ") ", newEntry.Text));

                        if (_appRoot.communications != null &&
                            _appRoot.communications.charLogging != null)
                            {
                                //_appRoot.communications.UpdateCharacteristicValue(_appRoot.communications.charLogging, newEntry.Text);
                            }

                            CurrentLog = newEntry;

                            if (lcd != null)
                            {
                                lcd.ShowCurrentLog();
                            }
                        }
                    }
                    catch (Exception logEx)
                    {
                        Console.WriteLine(logEx.Message);
                    }
                });

                t.Start();
            }
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
