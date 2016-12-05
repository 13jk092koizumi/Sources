using System;
using System.Linq;
using System.Collections.Generic;

using SQLite;

namespace GetWifi.src.database {

    public class DBConfig {
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

        public void createDatabase() {
            try {
                connection.CreateTable<AccessPoint>();
                connection.CreateTable<ScanData>();
                connection.CreateTable<BSSIDIndex>();
                connection.CreateTable<RoomIndex>();
                connection.CreateTable<ScanDateLog>();
            }
            catch (SQLiteException ex) {
                throw new Exception("can't create table!\tERROR:"+ex.Message);
            }
        }

        public void insertAccessPoints(IList<Android.Net.Wifi.ScanResult> scanResult, string placeName, DateTime nowTime) {
            var wList = new List<AccessPoint>();
            int average = 0;
            int variation = 0;
            try {
                for (int i = 0; i < scanResult.Count && i < ListSizeMax; ++i) {
                    var scanDataTable = connection.Query<ScanData>("select * from ScanData where BSSID == ? and Date == ?", scanResult[i].Bssid, nowTime);
                    average = 0;
                    variation = 0;
                    int count = scanDataTable.Count;
                    //平均、分散を計算
                    calcAveAndVar(ref average, ref variation, scanDataTable, count);
                    //スキャン済みの計測場所かつDB内に同じBSSIDがあった場合->更新
                    //なかった場合->カラムを作成
                    var apTable = from s in connection.Table<AccessPoint>() where s.Room == placeName select s; //スキャン済みだったら
                    if (apTable.Count() != 0) {
                        foreach (var ap_item in apTable) {
                            if (ap_item.BSSID == scanResult[i].Bssid) {
                                ap_item.ScanCount++;
                                ap_item.Level = average;
                                ap_item.Variation = variation;
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
                            Variation = variation,
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

        public void insertScanDateLog(string room, DateTime date) {
            try {
                #region
                /*
                List<ScanDateLog> dateLogList = new List<ScanDateLog>();
                var ap = getAccessPoints().ToList();
                var scan = getScanData().ToList();
                foreach (var ap_item in ap) {
                    //AccessPointとScanDataの日付を比較、一致した部屋名をScanDateLogに格納
                    var sameDate = scan.Where(w => ap_item.Date == w.Date).FirstOrDefault();
                    var isDistinct = dateLogList.Where(w => w.Date == sameDate.Date).FirstOrDefault();
                    if (sameDate != null && isDistinct == null) {
                        dateLogList.Add(new ScanDateLog { Room = ap_item.Room, Date = ap_item.Date });
                    }
                }*/
                #endregion
                Console.WriteLine("{0}をDBに追加しました。", room);
                connection.Insert(new ScanDateLog { Room = room, Date = date });
                
                var afterTable = getScanDateLog();
                Console.WriteLine("ScanDateLog:");
                foreach(var item in afterTable) {
                    Console.WriteLine(item.ToString());
                }
                
                //connection.DeleteAll<ScanDateLog>();
                //connection.Execute("delete from sqlite_sequence where name= 'ScanDateLog'");
            }
            catch (SQLiteException e) {
                throw new Exception(e.Message);
            }
        }
        public void insertScanData(int loop, IList<Android.Net.Wifi.ScanResult> scanResult, DateTime date) {
            try {
                //電波強度の強い順でListSizeMax個保存
                var list = new List<ScanData>();
                for (int i = 0; i < scanResult.Count && i < ListSizeMax; ++i) {
                    list.Add(new ScanData {
                        ScanNum = loop,
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

        public void insertRoomIndex(string room) {
            try {
                connection.Insert(new RoomIndex { Room = room });
            }catch(SQLiteException e) {
                throw new Exception(e.Message);
            }
        } //insertRoomIndex

        public void insertBssidIndex(string bssid) {
            try {
                connection.Insert(new BSSIDIndex { BSSID = bssid });
            }catch(SQLiteException e) {
                throw new Exception(e.Message);
            }
        }//insertBssidIndex

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
                connection.DeleteAll<RoomIndex>();
                connection.DeleteAll<BSSIDIndex>();
                connection.DeleteAll<ScanDateLog>();
                connection.Execute("delete from sqlite_sequence where name= 'AccessPoint'");
                connection.Execute("delete from sqlite_sequence where name='ScanData'");
                connection.Execute("delete from sqlite_sequence where name='RoomIndex'");
                connection.Execute("delete from sqlite_sequence where name='BSSIDIndex'");
                connection.Execute("delete from sqlite_sequence where name= 'ScanDateLog'");
                //connection.Execute("update sqlite_sequence set seq=0 where name= 'AccessPoint'");
                //connection.Execute("update sqlite_sequence set seq=0 where name='ScanData'");
                //connection.Execute("update sqlite_sequence set seq=0 name='RoomIndex'");
                //connection.Execute("update sqlite_sequence set seq=0 name='BSSIDIndex'");
                //connection.Execute("update sqlite_sequence set seq=0 name= 'ScanDateLog'");
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        } //deleteData

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

        public TableQuery<ScanDateLog> getScanDateLog() {
            try {
                return connection.Table<ScanDateLog>();
            }catch(SQLiteException e) {
                throw new Exception(e.Message);
            }
        }

        public TableQuery<ScanData> getScanData() {
            try {
                return connection.Table<ScanData>();
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        } //getScanData()

        public TableQuery<RoomIndex> getRoomIndex() {
            try {
                return connection.Table<RoomIndex>();
            }catch(SQLiteException e) {
                throw new Exception(e.Message);
            }
        } //getRoomIndex()

        public TableQuery<BSSIDIndex> getBssidIndex() {
            try {
                return connection.Table<BSSIDIndex>();
            }catch(SQLiteException e) {
                throw new Exception(e.Message);
            }
        } //getBssidIndex()

        public List<string> createCsv() {
            var scanDataList = getScanData();
            var toList = new List<string>();
            string field_line = "id,ScanNum,BSSID,LEVEL,Date";
            toList.Add(field_line);
            foreach (var list_item in scanDataList) {
                toList.Add(list_item.ToString());
            }
            return toList;
        } //createCsv()

        public void setIndexTable(string room) {
            //var room = getAccessPoints().ToList().Select(ap => ap.Room).Distinct();
            var bssid = getScanData().ToList().Select(scan => scan.BSSID).Distinct();
            Console.WriteLine("追加したデータ：");
            var hasSameRoom = connection.Table<RoomIndex>().Where(w => w.Room == room).FirstOrDefault();
            if (hasSameRoom == null) {
                Console.WriteLine("[ {0} ]",room);
                insertRoomIndex(room);
            }
            foreach (var bssid_item in bssid) {
                var hasSameBssid = connection.Table<BSSIDIndex>().Where(w => w.BSSID == bssid_item).FirstOrDefault();
                if (hasSameBssid != null) {
                    continue;
                }
                insertBssidIndex(bssid_item);
                Console.WriteLine("[ {0} ]",bssid_item);
            }
        }

        public string resetScanData() {
            try {
                connection.Execute("update sqlite_sequence set seq=0 where name='ScanData'");
                var scanDataLastColm = connection.Table<ScanData>().OrderByDescending(s => s.ID).FirstOrDefault();
                var accessPointsLastColum = connection.Table<AccessPoint>().OrderByDescending(s => s.ID).FirstOrDefault();
                if (scanDataLastColm.Date == accessPointsLastColum.Date) {
                    return "AccessPointテーブルにあるDateのカラムを参照しているため、削除しませんでした。";
                }
                int deletes = connection.Execute("delete from ScanData where Date = ?", scanDataLastColm.Date);
                return string.Format("{0} colum deleted.", deletes);
            }catch(SQLiteException e) {
                return e.Message;
            }
        }

        private void calcAveAndVar(ref int average, ref int variation, List<ScanData> scanDataTable, int count) {
            //BSSIDごとに平均と分散を計算する。
            foreach (var tb_item in scanDataTable) {
                average += tb_item.Level;
            }
            average /= count; //平均算出
            foreach (var tb_item in scanDataTable) {
                int element = tb_item.Level - average;
                variation += element * element;
            }
            variation /= count-1; //分散算出
        }
    }
}