using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using SQLite;

namespace GetWifi.src.database {
    class DBConfig {
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
        public string insertUpdateAllData(IEnumerable<RN_tb> rn_tbs,IEnumerable<WifiState_tb> wifi_tbs, string path) {
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
        public string getTable(string path, string query) {
            try {
                string text = "";
                var db = new SQLiteConnection(path);
                if (query == null) {
                    //Table<>のテスト
                    var rn = db.Table<RN_tb>();
                    var wifi = db.Table<WifiState_tb>();

                    foreach (RN_tb res in rn) {
                        text += "\n" + res.ToString();
                    }
                    foreach (WifiState_tb res in wifi) {
                        text += res.ToString();
                    }
                } else {
                    var QUERY = db.Query<WifiState_tb>(query);
                    string queryTxt = "";
                    foreach (WifiState_tb res in QUERY) {
                        queryTxt += "\n" + res.ToString();
                    }
                }
                return text;
            }
            catch (SQLiteException ex) {
                return ex.Message;
            }
        } //getTable()
    }
}