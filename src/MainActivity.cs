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

        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            var LLayout = FindViewById<LinearLayout>(Resource.Id.LLayout);
 
            //スキャン結果の取得
            var results = getWifi();

            sortScanResult(results); //電波強度で昇順ソート

            for (int i = 0; i < results.Count; ++i) {
                //LayoutInflatorの取得
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
                LLayout.AddView(tb_row);
            }

            //データベースに保存する
            // create variables for onscreen widgets
            var btnCreate = FindViewById<Button>(Resource.Id.btnCreateDB);
            var btnAddData = FindViewById<Button>(Resource.Id.btnAddData);
            var btnShow = FindViewById<Button>(Resource.Id.btnShowTable);
            
            //DB作成
            var db = new database.DBConfig();
            //パスの作成
            var docsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            var pathToDatabace = System.IO.Path.Combine(docsFolder, "db_sqlnet.db");
            btnAddData.Enabled = false;
            //ボタンのイベント作成
            //btnCreate.Click += delegate {
            //    var result = db.createDatabase(pathToDatabace);
            //    txtResult.Text = result+" TO: " + pathToDatabace + "\n";
            //    //データベースに接続できたら、シングルとリストのボタンを有効に
            //    if(result == "Database created") {
            //        btnAddData.Enabled = btnShow.Enabled = true;
            //    }
            //};
            //btnAddData.Click += delegate {
            //    rn = new RN_tb { Room = "roomA" };
            //    var wifi_state = new WifiState_tb { };
            //    var result = db.insertUpdateAllData(rn,wifi_state, pathToDatabace);
            //    var records = db.findNumberRecords(pathToDatabace);
            //    txtResult.Text += string.Format("{0}\nNumver of records = {1}\n", result, records);
            //};
            //btnShow.Click += delegate {
            //    var result = db.getTable(pathToDatabace, null);
            //    txtResult.Text += result;
            //};

            } // OnCreate()

        private IList<ScanResult> getWifi() {
            //wifi情報を取得
            var wifi = (WifiManager)GetSystemService(WifiService);
            //アクセスポイントのスキャン
            wifi.StartScan();
            return wifi.ScanResults;
        }

        private void createDataList(System.Collections.Generic.IList<ScanResult> list) {

        }

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

        private async Task sleep(int ms) {
            await Task.Delay(ms);
        }

    } // class MainActivity()
} // namespace GetWifi