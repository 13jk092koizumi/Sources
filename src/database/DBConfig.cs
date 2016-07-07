using System;
using System.Collections.Generic;

using SQLite;

namespace GetWifi.src.database {

    class DBConfig {
        private  SQLiteConnection connection;
        private string path;

        public DBConfig(string db_path) {
            path = db_path;
            if(path != null)
            connection = new SQLiteConnection(this.path);
        }

        ~DBConfig() {
            connection.Close();
        }

        public string createDatabase() {
            try {
                //connection = new SQLiteConnection(path);
                connection.CreateTable<Wifi_tb>();
                return "Database created";
            }
            catch (SQLiteException ex) {
                return ex.Message;
            }
        }

        public void insertScanResult(IList<Android.Net.Wifi.ScanResult> scanResult, string roomName) {
            var wList = new List<Wifi_tb>();
            //�e�[�u���ɒl����
            foreach (var sr in scanResult) {
                wList.Add(new Wifi_tb {
                    Room            =   roomName,
                    SSID            =   sr.Ssid,
                    BSSID           =   sr.Bssid,
                    Capabilities    =   sr.Capabilities,
                    Frequency       =   sr.Frequency,
                    Level           =   sr.Level,
                    
                });
            }
            try {
                //Insert
                connection.InsertAll(wList);
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        }

        public void deleteData(string path) {
            try {
                connection.DropTable<Wifi_tb>();
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        }

        public int findNumberRecords() {
            try {
                //���ׂẴ��R�[�h���J�E���g���܂��B �Ȃ̂Œx���ł��B
                var count = connection.ExecuteScalar<int>("SELECT Count(*) FROM WifiState_tb");
                return count;
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        }

        public TableQuery<Wifi_tb> getTable(ref int colums) {
            try {
                var table = connection.Table<Wifi_tb>();
                colums = findNumberRecords();
                return table;
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
                
        } //getTable()
    }
}