using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Android.App;
using Android.Util;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Net.Wifi;
using Android.OS;

using SQLite;

namespace GetWifi.src {
    [Activity(Label = "GetWifi", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity {

        private database.DBConfig db;
        private string pathToDatabace;
        private string room;

        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            var tbLayout = FindViewById<TableLayout>(Resource.Id.tableLayout);

            // スキャン結果の取得
            var results = getWifi();
            // 結果を表示
            var txtView1 = FindViewById<TextView>(Resource.Id.textView1);
            var apNum = "APを" + results.Count + "件見つけました。";
            txtView1.Text = apNum;
            sortScanResult(results); //電波強度で昇順ソート

            for (int i = 0; i < results.Count; ++i) {
                // LayoutInflatorの取得
                var tb_row = LayoutInflater.Inflate(Resource.Layout.tb_layout, null);
                var buf_normal = TextView.BufferType.Normal;

                var ssid = tb_row.FindViewById<TextView>(Resource.Id.rowtext1);
                ssid.SetText(results[i].Ssid, buf_normal);

                var bssid = tb_row.FindViewById<TextView>(Resource.Id.rowtext2);
                bssid.SetText(results[i].Bssid, buf_normal);

                var level = tb_row.FindViewById<TextView>(Resource.Id.rowtext3);
                level.SetText(results[i].Level.ToString(), buf_normal);

                var frequency = tb_row.FindViewById<TextView>(Resource.Id.rowtext4);
                frequency.SetText(results[i].Frequency.ToString(), buf_normal);
                tbLayout.AddView(tb_row);
            }

            //データベースに保存する
            // create variables for onscreen widgets
            var edit = FindViewById<EditText>(Resource.Id.input_editTxt);
            var btnInput = FindViewById<Button>(Resource.Id.btnInput);
            var btnCreate = FindViewById<Button>(Resource.Id.btnCreateDB);
            var btnAddData = FindViewById<Button>(Resource.Id.btnAddData);
            var btnShow = FindViewById<Button>(Resource.Id.btnShowTable);
            var btnDelete = FindViewById<Button>(Resource.Id.btnDelete);

            //DB作成
            db = new database.DBConfig();
            //パスの作成
            var docsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            pathToDatabace = System.IO.Path.Combine(docsFolder, "db_sqlnet.db");
            btnAddData.Enabled = btnShow.Enabled = false;
            //イベント作成
            btnInput.Click += delegate {
                room = edit.Text;
                Toast.MakeText(this, string.Format("入力:{0}", room), ToastLength.Short).Show();
            };
            btnDelete.Click += delegate {
                db.deleteData(pathToDatabace);
                Toast.MakeText(this, "DELETEしました。", ToastLength.Short).Show();
                btnAddData.Enabled = btnShow.Enabled = false;
            };
            btnCreate.Click += delegate {
                var result = db.createDatabase(pathToDatabace);
                Toast.MakeText(this, result + " TO: " + pathToDatabace + "\n", ToastLength.Long).Show();
                //データベースに接続できたら、シングルとリストのボタンを有効に
                if (result == "Database created") {
                    btnAddData.Enabled = btnShow.Enabled = true;
                }
            };
            btnAddData.Click += delegate {
                var rn_tb = new List<RN_tb>();
                var wifi_tb = new List<WifiState_tb>();

                createDataList(ref rn_tb, ref wifi_tb, results);

                var result = db.insertUpdateAllData(rn_tb, wifi_tb, pathToDatabace);
                Toast.MakeText(this,string.Format("{0}件追加しました。\n", results.Count), ToastLength.Short).Show();
            };
            btnShow.Click += delegate {
                var colums = 0;
                //var query = "SELECT*FROM RN_tb";
                //var result = db.getTable(pathToDatabace, query, ref colums);
                var txtView = FindViewById<TextView>(Resource.Id.IventText);
                //txtView.Text = string.Format("DBから{0}件のデータを取得しました。\n",colums);
                var res = db.getTable(pathToDatabace, null, ref colums);
                foreach (var r in res) {
                    Console.WriteLine(string.Format("[{0}]\n", r.ToString()));
                    foreach (var rc in r) {
                        Console.WriteLine(string.Format("[{0}]\n", rc));
                    }
                }
            };

        } // OnCreate()

        /// <summary>
        /// StartScan()でAPを取得
        /// </summary>
        /// <returns></returns>
        private IList<ScanResult> getWifi() {
            //wifi情報を取得
            var wifi = (WifiManager)GetSystemService(WifiService);
            //アクセスポイントのスキャン
            wifi.StartScan();
            return wifi.ScanResults;
        }


        /// <summary>
        /// DBへ格納するデータの整理
        /// </summary>
        private void createDataList(ref List<RN_tb> rn_tb, ref List<WifiState_tb> wifi_tb, System.Collections.Generic.IList<ScanResult> list) {
            rn_tb.Add(new RN_tb { Room = room });
            for (int i = 0; i < list.Count; ++i) {
                wifi_tb.Add(new WifiState_tb {
                    SSID = list[i].Ssid,
                    BSSID = list[i].Bssid,
                    Capabilities = list[i].Capabilities,
                    Level = list[i].Level,
                    Frequency = list[i].Frequency,
                });
            }

        }

        /// <summary>
        /// 要素を入れ替えます。
        /// </summary>
        /// <param name="list"></param>
        /// <param name="indexA">入れ替える添え字</param>
        /// <param name="indexB">入れ替える添え字</param>
        private void swap(System.Collections.Generic.IList<ScanResult> list, int indexA, int indexB) {
            ScanResult temp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = temp;
        } // swap()

        /// <summary>
        /// resultsのリスト配列を電波強度(Level)で降順ソート
        /// </summary>
        /// <param name="list">scanresultのList配列</param>
        private void sortScanResult(System.Collections.Generic.IList<ScanResult> list) {
            int size = list.Count; //配列の大きさ取得
            int i, j; //for文用
            for (i = 0; i < size; ++i) {
                for (j = size - 1; j > i; --j) {
                    if (list[j - 1].Level < list[j].Level) { //前の要素の方が小さかったら
                        swap(list, j - 1, j); //入れ替え
                    }
                }
            }
        } // sortScanResult()

        private async Task sleep(int ms) {
            await Task.Delay(ms);
        }

    } // class MainActivity()
} // namespace GetWifi