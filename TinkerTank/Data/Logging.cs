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

        public void Init(DataStore con)
        {
            _appRoot = MeadowApp.Current;

            dbcon = con;

            //Display
            if (_appRoot.EnableDisplay)
            {
                try
                {
                    Log("start lcd", LogStatusMessageTypes.Important);
                    lcd = new LCDDisplay_ST7789();
                    lcd.Init();
                }
                catch (Exception)
                {

                }
            }
        }

        public void Log(string newText, LogStatusMessageTypes statusType = LogStatusMessageTypes.Debug)
        {
            if (dbcon != null)
            {
                var t = new Task(() =>
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

                    if (lcd != null)
                    {
                        try
                        {
                            if (lcd.AddMessage(newEntry.Text, newEntry.StatusType))
                            {
                                newEntry.Displayed = true;
                                if (dbcon != null)
                                {
                                    dbcon.UpsertDebugLogEntry(newEntry);
                                }
                            }
                        }
                        catch (Exception) { };
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
