using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Net.Wifi;
using Android.OS;

using SQLite;

namespace GetWifi {
    [Activity(Label = "GetWifi", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity {

        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            var linearLayout = (LinearLayout)FindViewById<LinearLayout>(Resource.Id.LLayout);
            
            //TextView生成
            var tv = new TextView(this);

            /*
            //wifi情報を取得
            var wifi = (WifiManager)GetSystemService(WifiService);
            //アクセスポイントのスキャン
            wifi.StartScan();
            //結果の取得
            var results = wifi.ScanResults;

            sortScanResult(results); //電波強度で昇順ソート

            //結果をTextViewにセット
            var sb = new System.Text.StringBuilder();

            for (int i=0; i<results.Count; ++i) {
                int index = i;
                sb.Append((++index)+":\n");
                sb.Append("SSID:\t\t"+results[i].Ssid+"\n");
                sb.Append("BSSID:\t\t" + results[i].Bssid + "\n");
                sb.Append("Level:\t\t"+results[i].Level +" dBm\n");
                sb.Append("\n");                
            }
            tv.SetTextSize(Android.Util.ComplexUnitType.Dip, 20);
            tv.Text = sb.ToString();
            linearLayout.AddView(tv);
            */

            //データベースに保存する
            // create variables for onscreen widgets
            var btnCreate = FindViewById<Button>(Resource.Id.btnCreateDB);
            var btnSingle = FindViewById<Button>(Resource.Id.btnCreateSingle);
            var btnList = FindViewById<Button>(Resource.Id.btnList);
            var btnShow = FindViewById<Button>(Resource.Id.btn_showTable);
            var txtResult = FindViewById<TextView>(Resource.Id.txtResults);
            //パスの作成
            var docsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            var pathToDatabace = System.IO.Path.Combine(docsFolder, "db_sqlnet.db");
            //データベースが既にあったらボタンを無効にする？
            btnSingle.Enabled = btnList.Enabled = false;
            //ボタンのイベント作成
            btnCreate.Click += delegate {
                var result = createDatabase(pathToDatabace);
                txtResult.Text = result+" TO: " + pathToDatabace + "\n";
                //データベースに接続できたら、シングルとリストのボタンを有効に
                if(result == "Database created") {
                    btnList.Enabled = btnSingle.Enabled = true;
                }
            };
            btnSingle.Click += delegate {
                var result = insertUpdateData(new Person { FirstName = string.Format("John{0}", DateTime.Now.Ticks), LastName = "Smith" }, pathToDatabace);
                var records = findNumberRecords(pathToDatabace);
                txtResult.Text += string.Format("{0}\nNumver of records = {1}\n", result, records);
            };
            btnList.Click += delegate {
                var peopleList = new List<Person>
                {
                    new Person { FirstName = "Miguel", LastName = string.Format("de Icaza ({0})", DateTime.Now.Ticks) },
                    new Person { FirstName = string.Format("Kevin {0}", DateTime.Now.Ticks), LastName = "Mullins" },
                    new Person { FirstName = "Amy", LastName = string.Format("Burns ({0})", DateTime.Now.Ticks) }
                };
                var result = insertUpdateAllData(peopleList, pathToDatabace);
                var records = findNumberRecords(pathToDatabace);
                txtResult.Text += string.Format("{0}\nNumber of records = {1}\n", result, records);
            };
            btnShow.Click += delegate {
                var data = sqliteTest(pathToDatabace);
                txtResult.Text += data;
                
            };
                  
        } // OnCreate()
        
        /*要素を入れ替えます。*/
        private void swap(System.Collections.Generic.IList<ScanResult> list, int indexA, int indexB) {
            ScanResult temp= list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = temp;
        } // swap()

        /*resultsのリスト配列を電波強度(Level)でソート*/
        private void sortScanResult(System.Collections.Generic.IList<ScanResult> list) {
            int size = list.Count; //配列の大きさ取得
            int i, j; //for文用
            for (i = 0; i < size; ++i) {
                for (j = size - 1; j > i; --j) {
                    if (list[j - 1].Level > list[j].Level) { //前の要素の方が大きかったら
                        swap(list, j - 1, j); //入れ替え
                    }
                }
            }
        } // sortScanResult()

        /*テーブルを作ります。*/
        private string createDatabase(string path) {
            try {
                var connection = new SQLiteConnection(path);
                connection.CreateTable<Person>();
                return "Database created";
            }
            catch (SQLiteException ex) {
                return ex.Message;
            }
        } //createDatabade();

        /*データをINSERTします。*/
        private string insertUpdateData(Person data, string path) {
            try {
                var db = new SQLiteConnection(path);
                if (db.Insert(data) != 0)
                    db.Update(data);
                return "Single data file inserted or updated";
            }
            catch(SQLiteException ex) {
                return ex.Message;
            }
        } // insertUpdateData()

        /*データをすべてINSERTします。*/
        private string insertUpdateAllData(IEnumerable<Person> data, string path) {
            try {
                var db = new SQLiteConnection(path);
                if (db.InsertAll(data) != 0)
                    db.UpdateAll(data);
                return "List of data inserted or updated";
            }catch(SQLiteException ex) {
                return ex.Message;
            }
        } // insertUpdateAllData

        /*レコードの数を取得します*/
        private int findNumberRecords(string path) {
            try {
                var db = new SQLiteConnection(path);
                //すべてのレコードをカウントします。 なので遅いです。
                var count = db.ExecuteScalar<int>("SELECT Count(*) FROM Person");

                return count;
            } catch(SQLiteException) {
                return -1;
            }
        } // findNumberRecords()

        private string sqliteTest(string path) {
            try {
                
                var texts = "";
                var db = new SQLiteConnection(path);
                //Get<>のテスト
                var GET = db.Get<Person>(1); //id1を指定
                //Table<>のテスト
                var TABLE = db.Table<Person>();
                string tableTxt="";
                foreach(Person res in TABLE) {
                    tableTxt += "\n"+res.ToString();
                }
                //Query<>のテスト
                var QUERY = db.Query<Person>("SELECT * FROM Person");
                string queryTxt = "";
                foreach (Person res in QUERY) {
                    queryTxt += "\n" + res.ToString();
                }
                //Executeのテスト
                var EXECUTE = db.Execute("SELECT * FROM Person");

                texts = "\nGET:\n" + GET
                            +"\nTABLE:"+tableTxt        
                            +"\nQUERY:"+queryTxt;
                            
                return texts;
            }catch(SQLiteException ex) {
                return ex.Message;
            }
        }

    } // class MainActivity()
} // namespace GetWifi