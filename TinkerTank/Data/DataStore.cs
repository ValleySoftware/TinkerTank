using Enumerations;
using Meadow;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TinkerTank.Data
{
    [Table("DebugLogEntryModel")]
    public class DebugLogEntryModel
    {
        [PrimaryKey, AutoIncrement]
        public int SQL_ID { get; set; }
        public string ID { get; set; }
        public string Boot_ID { get; set; }
        public string Text { get; set; }
        public LogStatusMessageTypes StatusType { get; set; }
        public DateTimeOffset RecordedStamp { get; set; }
        public bool Displayed { get; set; }
        public bool Transmitted { get; set; }
        public DateTimeOffset? TransmittedStamp { get; set; }
        public string Remote_Request_ID { get; set; }
    }

    [Table("BootRecordModel")]
    public class BootRecordModel
    {
        [PrimaryKey, AutoIncrement]
        public int SQL_ID { get; set; }
        public string ID { get; set; }
        public DateTimeOffset RecordedStamp { get; set; }
    }

    public class DataStore
    {
        MeadowApp _appRoot;
        SQLiteConnection dbcon;
        private BootRecordModel thisBootRecord;

        public static string GenerateRandomString()
        {
            string first10Chars = Path.GetRandomFileName();
            first10Chars = first10Chars.Replace(".", ""); // Remove period.

            string second10Chars = Path.GetRandomFileName();
            second10Chars = second10Chars.Replace(".", ""); // Remove period.

            return string.Concat(first10Chars.Substring(0, 5), second10Chars.Substring(0, 5));
        }

        public void InitDB(bool wipeDBOnStartup)
        {
            _appRoot = MeadowApp.Current;

            var databasePath = Path.Combine(MeadowOS.FileSystem.DataDirectory, "BerthaDB.db");
            dbcon = new SQLiteConnection(databasePath);
            dbcon.CreateTable<BootRecordModel>();
            dbcon.CreateTable<DebugLogEntryModel>();

            if (wipeDBOnStartup)
            {
                try
                { 
                    dbcon.Table<BootRecordModel>().Delete(b => b.SQL_ID > 0);
                    dbcon.Table<DebugLogEntryModel>().Delete(b => b.SQL_ID > 0);
                }
                catch (Exception ex)
                {

                }
            }

            thisBootRecord = new BootRecordModel() 
            { 
                ID = GenerateRandomString(), 
                RecordedStamp = DateTimeOffset.Now 
            };

            dbcon.Insert(thisBootRecord);

            Console.WriteLine("thisBootRecord inserted successfully " + thisBootRecord.SQL_ID);

        }

        public bool UpsertDebugLogEntry(DebugLogEntryModel model)
        {
            var result = false;

            try
            {
                if (string.IsNullOrEmpty(model.ID))
                {
                    model.ID = GenerateRandomString();
                }

                if (string.IsNullOrEmpty(model.Boot_ID))
                {
                    if (thisBootRecord == null)
                    {
                        Console.WriteLine("**** ThisBootRecord not available in DataStore ****");
                    }
                    model.Boot_ID = thisBootRecord.ID;
                }

                if (_appRoot.EnableDBLogging)
                {
                    if (model.SQL_ID == 0)
                    {
                        //new
                        dbcon.Insert(model);
                    }
                    else
                    {
                        //update
                        dbcon.Update(model);
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Exception in UpsertDBLog Method");

            }

            return result;
        }
    }
}
