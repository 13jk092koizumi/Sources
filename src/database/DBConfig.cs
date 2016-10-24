using System;
using System.Collections.Generic;

using SQLite;

namespace GetWifi.src.database {

    class DBConfig {
        private SQLiteConnection connection;
        private string path;
        private const int ListSizeMax = 10; //スキャンしたAPの保存上限

        public DBConfig(string db_path) {
            path = db_path;
            if (path != null)
                connection = new SQLiteConnection(this.path);
        }

        ~DBConfig() {
            connection.Close();
        }

        public string createDatabase() {
            try {
                connection.CreateTable<AccessPoint>();
                connection.CreateTable<ScanData>();
                return "Database created";
            }
            catch (SQLiteException ex) {
                return ex.Message;
            }
        }

        public void insertAccessPoints(IList<Android.Net.Wifi.ScanResult> scanResult, string placeName, DateTime nowTime) {
            var wList = new List<AccessPoint>();
            int average = 0;
            int dispersion = 0;
            try {
                for (int i = 0; i < scanResult.Count && i < ListSizeMax; ++i) {
                    var scanDataTable = connection.Query<ScanData>("select * from ScanData where BSSID == ? and Date == ?", scanResult[i].Bssid, nowTime);
                    average = 0;
                    dispersion = 0;
                    int count = scanDataTable.Count;
                    //平均、分散を計算
                    calcAveAndDisp(ref average, ref dispersion, scanDataTable, count);
                    //スキャン済みの計測場所かつDB内に同じBSSIDがあった場合->更新
                    //なかった場合->カラムを作成
                    var apTable = from s in connection.Table<AccessPoint>() where s.Room == placeName select s; //スキャン済みだったら
                    if (apTable.Count() != 0) {
                        foreach (var ap_item in apTable) {
                            if (ap_item.BSSID == scanResult[i].Bssid) {
                                ap_item.ScanCount++;
                                ap_item.Level = average;
                                ap_item.Dispersion = dispersion;
                                ap_item.Date = scanDataTable[0].Date;
                            }
                            connection.Update(ap_item);
                            //Console.WriteLine("Date is {0}", scanDataTable[0].Date.ToString());
                        }
                    } else {
                        wList.Add(new AccessPoint {
                            Room = placeName,
                            SSID = scanResult[i].Ssid,
                            BSSID = scanResult[i].Bssid,
                            Level = average,
                            Dispersion = dispersion,
                            Date = scanDataTable[0].Date,
                            ScanCount = 1,
                        });
                    }
                }
                connection.InsertAll(wList);
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        } //insertAccessPoints()

        public void insertScanData(IList<Android.Net.Wifi.ScanResult> scanResult, DateTime date) {
            try {
                //電波強度の強い順でListSizeMax個保存
                var list = new List<ScanData>();
                for (int i = 0; i < scanResult.Count && i < ListSizeMax; ++i) {
                    list.Add(new ScanData {
                        BSSID = scanResult[i].Bssid,
                        Level = scanResult[i].Level,
                        Date = date,
                    });
                }
                //foreach (var item in list) { Console.WriteLine(string.Format("{0}:{1},{2}",item.ID,item.BSSID,item.Date)); }
                connection.InsertAll(list);
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        } //insertScanData()

        public void dropTable() {
            try {
                connection.DropTable<AccessPoint>();
                connection.DropTable<ScanData>();
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        }

        public void deleteData() {
            try {
                connection.DeleteAll<AccessPoint>();
                connection.DeleteAll<ScanData>();
                connection.Execute("delete from sqlite_sequence where name= 'AccessPoint'");
                connection.Execute("delete from sqlite_sequence where name='ScanData'");
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        } //deleteData

        /// <summary>
        /// apでAccessPointテーブル、scanでScanDataテーブルを削除
        /// </summary>
        /// <param name="tb_name">"ap" or "scan"</param>
        public void dropTable(string tb_name) {
            if (tb_name == "ap") {
                connection.DropTable<AccessPoint>();
            } else if (tb_name == "scan") {
                connection.DropTable<ScanData>();
            } else { Console.WriteLine("\tテーブルを削除できませんでした。\n"); }
        } //dropTable()

        public int findNumberRecords() {
            try {
                //すべてのレコードをカウントします。 なので遅いです。
                var count = connection.ExecuteScalar<int>("SELECT Count(*) FROM AccessPoint");
                return count;
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        } //findNumberRecords()

        public TableQuery<AccessPoint> getAccessPoints() {
            try {
                var table = connection.Table<AccessPoint>();
                return table;
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        }

        public TableQuery<AccessPoint> getAccessPoints(ref int colums) {
            try {
                var table = connection.Table<AccessPoint>();
                colums = findNumberRecords();
                return table;
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        } //getAccessPoints()

        public TableQuery<ScanData> getScanData() {
            try {
                return connection.Table<ScanData>();
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        } //getScanData()

        private void calcAveAndDisp(ref int average, ref int dispersion, List<ScanData> scanDataTable, int count) {
            //BSSIDごとに平均と分散を計算する。
            foreach (var tb_item in scanDataTable) {
                average += tb_item.Level;
            }
            average /= count; //平均算出
            foreach (var tb_item in scanDataTable) {
                int element = tb_item.Level - average;
                dispersion += element * element;
            }
            dispersion /= count; //分散算出
        }
    }
}