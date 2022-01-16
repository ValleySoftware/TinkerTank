using Base;
using Display;
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
        private LCDDisplay_ST7789 lcd;
        private DebugLogEntryModel _currentLog;

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
                    lcd = new LCDDisplay_ST7789(this);
                    lcd.Init();
                }
                catch (Exception)
                {

                }
            }
        }

        public DebugLogEntryModel CurrentLog
        {
            get => _currentLog;
            set
            {
                _currentLog = value;
                if (lcd != null)
                {
                    lcd.ShowCurrentLog();
                }
            }
        }

        public async void AddLogEntry(string newText, LogStatusMessageTypes statusType = LogStatusMessageTypes.Debug)
        {
            if (dbcon != null)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        var newEntry = new DebugLogEntryModel()
                        {
                            RecordedStamp = DateTimeOffset.Now,
                            Displayed = false,
                            StatusType = statusType,
                            Text = newText
                        };

                        if (dbcon != null)
                        {
                            dbcon.UpsertDebugLogEntry(newEntry);
                        }

                        if (newEntry.StatusType == LogStatusMessageTypes.Error)
                        {
                            Console.WriteLine(String.Concat("*** (", newEntry.ID, ") ", newEntry.Text));
                        }

                        if (newEntry.StatusType == LogStatusMessageTypes.Important)
                        {
                            Console.WriteLine(String.Concat("// (", newEntry.ID, ") ", newEntry.Text));
                        }

                        if (
                                newEntry.StatusType >= LogStatusMessageTypes.Information &&
                                _appRoot.ShowDebugLogs
                            )
                        {
                            Console.WriteLine(String.Concat("(", newEntry.ID, ") ", newEntry.Text));
                        }

                        if (_appRoot.communications != null &&
                        _appRoot.communications.charLogging != null)
                        {
                            _appRoot.communications.UpdateCharacteristicValue(_appRoot.communications.charLogging, newEntry.Text);
                        }

                        if (lcd != null)
                        {
                            lcd.ShowCurrentLog();
                        }
                    }
                    catch (Exception logEx)
                    {
                        Console.WriteLine(logEx.Message);
                    }
                });
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
