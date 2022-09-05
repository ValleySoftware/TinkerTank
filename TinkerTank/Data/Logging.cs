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
        private int _counter = 0;

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
                    AddLogEntry("MinLogLevel= " + _appRoot.MinimumLogLevel, LogStatusMessageTypes.Critical); 

                    AddLogEntry(_appRoot.DisplayModel.ToString(), LogStatusMessageTypes.Important);
                    
                    switch (_appRoot.DisplayModel)
                    {
                        case DisplayBase.DisplayTypes.ST7789_SPI_240x240:
                            {
                                lcd = new LCDDisplay_ST7789(this);
                                lcd.Init();
                                lcd.ShowCurrentLog();
                                break;
                            }

                        case DisplayBase.DisplayTypes.SSD1306_I2C_128x64:
                            {
                                lcd = new LCDDisplay_1306_128x64(this);
                                lcd.Init();
                                lcd.ShowCurrentLog();
                                break;
                            }

                        case DisplayBase.DisplayTypes.SSD1306_I2C_128x32:
                            {
                                lcd = new LCDDisplay_1306_128x32(this);
                                lcd.Init();
                                lcd.ShowCurrentLog();
                                break;
                            }
                        default: AddLogEntry("No matching LCD model found", LogStatusMessageTypes.Error); ; break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(String.Concat("ERR"));
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
                                Remote_Request_ID = remoteID,

                            };

                            _counter++;
                            if (dbcon != null)
                            {
                                dbcon.UpsertDebugLogEntry(newEntry);
                            }

                            if (newEntry.StatusType == LogStatusMessageTypes.BLERecord)
                            {
                                Console.WriteLine(String.Concat("-BLE- ", remoteID, " - (", newEntry.ID, ") ", newEntry.Text));
                            }

                            if (newEntry.StatusType == LogStatusMessageTypes.Critical)
                            {
                                Console.WriteLine(String.Concat("*** - ", _counter++.ToString(), " (", newEntry.ID, ") ", newEntry.Text));
                            }

                            if (newEntry.StatusType == LogStatusMessageTypes.Error)
                            {
                                Console.WriteLine(String.Concat("* - ", _counter++.ToString(), " (", newEntry.ID, ") ", newEntry.Text));
                            }

                            if (newEntry.StatusType == LogStatusMessageTypes.Important)
                            {
                                Console.WriteLine(String.Concat("// - ", _counter++.ToString(), " (", newEntry.ID, ") ", newEntry.Text));
                            }

                            //Console.WriteLine(String.Concat(_counter++.ToString(), " - (", newEntry.ID, ") ", newEntry.Text));

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
