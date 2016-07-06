using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

using Android.Util;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using SQLite;

namespace GetWifi.src.database {
    class DBConfig {
        private  SQLiteConnection connection;
        
        /*テーブルを作ります。*/
        public string createDatabase(string path) {
            try {
                
                var connection = new SQLiteConnection(path);
                connection.CreateTable<RN_tb>();
                connection.CreateTable<WifiState_tb>();
                return "Database created";
            }
            catch (SQLiteException ex) {
                return ex.Message;
            }
        } //createDatabade();

        public string queryData(string query, string path) {
            try {
                //var connection = new SQLiteConnection(platform, path);

                var result = connection.Query<RN_tb>(query);
                return result.First().ToString();
            }
            catch (SQLiteException) {
                throw;
            }
        }

        public void insertScanResult(List<Android.Net.Wifi.ScanResult> scanResult, string roomName, string path) {
            var ws = new List<WifiState_tb>();
            foreach (var sr in scanResult) {
                ws.Add(new WifiState_tb {
                    BSSID = sr.Bssid,
                    Capabilities = sr.Capabilities,
                    Frequency = sr.Frequency,
                    Level = sr.Level,
                    SSID = sr.Ssid
                });
            }
            var rn = new RN_tb {
                Room = roomName,
           };
            //var db = new SQLiteConnection(platform, path);
            
            
        }

        /*データをINSERTします。*/
        public string insertUpdateData(RN_tb rn_tb, WifiState_tb wifi_tb, string path) {
            try {
                var db = new SQLiteConnection(path);

                if (db.Insert(rn_tb) != 0)
                    db.Update(rn_tb);
                if (db.Insert(wifi_tb) != 0)
                    db.Update(wifi_tb);

                return "insert successed";
            }
            catch (SQLiteException ex) {
                return ex.Message;
            }
        } // insertUpdateData()

        /*データをすべてINSERTします。*/
        public string insertUpdateAllData(List<RN_tb> rn_tbs, List<WifiState_tb> wifi_tbs, string path) {
            try {
                var db = new SQLiteConnection(path);
                if (db.InsertAll(rn_tbs) != 0)
                    db.UpdateAll(rn_tbs);
                if (db.InsertAll(wifi_tbs) != 0)
                    db.UpdateAll(wifi_tbs);

                return "List of data inserted or updated";
            }
            catch (SQLiteException ex) {
                return ex.Message;
            }
        } // insertUpdateAllData

        public void deleteData(string path) {
            try {
                var db = new SQLiteConnection(path);
                db.DropTable<RN_tb>();
                db.DropTable<WifiState_tb>();
            }
            catch (SQLiteException) {
                throw;
            }
        }

        /*レコードの数を取得します*/
        public int findNumberRecords(string path) {
            try {
                var db = new SQLiteConnection(path);
                //すべてのレコードをカウントします。 なので遅いです。
                var count = db.ExecuteScalar<int>("SELECT Count(*) FROM WifiState_tb");

                return count;
            }
            catch (SQLiteException) {
                return -1;
            }
        } // findNumberRecords()

        /// <summary>
        /// DBのすべての値を返却します。
        /// </summary>
        /// <param name="path">データベースファイルのパス</param>
        /// <param name="query">WifiState_tbを指定したクエリ。無いときはnull</param>
        /// <returns>結果の文字列</returns>
        public List<List<string>> getTable(string path, string query, ref int colums) {
            try {
                var text = string.Empty;
                var db = new SQLiteConnection(path);
                var pList = new List<List<string>>();
                if (query == null) {
                    //Table<>のテスト
                    var rn = db.Table<RN_tb>();
                    var wifi = db.Table<WifiState_tb>();
                    var cList = new List<string>();
                    foreach (var room in rn) {
                        foreach (var ws in wifi) {
                            cList.Add(room.Room);
                            cList.Add(ws.SSID);
                            cList.Add(ws.BSSID);
                            cList.Add(ws.Level.ToString());
                        }
                    }
                    pList.Add(cList);
                    //query = "select * from WifiState_tb";
                    //var Qres = db.Query<WifiState_tb>(query);
                    //foreach (var item in Qres) { text += "\n" + item.ToString(); }
                    //var join = from tb1 in db.Table<RN_tb>()
                    //           join tb2 in db.Table<WifiState_tb>() on tb1.ID equals tb2.ID
                    //           select tb2;
                    //select new {
                    //    id = tb1.ID,
                    //    room = tb1.Room,
                    //    ssid = tb2.SSID,
                    //    bssid = tb2.BSSID,
                    //    capabilities = tb2.Capabilities,
                    //    level = tb2.Level,
                    //    frequency = tb2.Frequency
                    //};

                    //foreach (var item in join) {
                    //    text += "\n" + item.ToString();
                    //}
                    return pList;
                    foreach (RN_tb res in rn) {
                        text += "\n" + res.ToString();
                    }
                    foreach (WifiState_tb res in wifi) {
                        text += res.ToString();
                    }
                    //} else {
                    //    var QUERY = db.Query<WifiState_tb>(query);
                    //    foreach (WifiState_tb res in QUERY) {
                    //        text += "\n" + res.ToString();
                    //    }
                }
                colums = findNumberRecords(path);
                return pList;
            }
            catch (SQLiteException) {
                throw;
            }
        } //getTable()
    }
}