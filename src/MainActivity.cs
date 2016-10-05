using System.Threading;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Net.Wifi;
using Android.OS;

namespace GetWifi.src {
    [Activity(Label = "GetWifi", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity {
        public static MainActivity Instance;
        private WifiManager wifi;
        private IList<ScanResult> results;
        private database.DBConfig db;
        private string pathToDatabase;
        private string room;

        protected override void OnCreate(Bundle bundle) {
            SetContentView(Resource.Layout.Main);
            base.OnCreate(bundle);
            //パスの作成
            var docsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            pathToDatabase = System.IO.Path.Combine(docsFolder, "db_sqlnet.db");
            //DB作成
            db = new database.DBConfig(pathToDatabase);
            db.createDatabase();
            Instance = this;
            wifi = (WifiManager)GetSystemService(WifiService);
            
        } // OnCreate()

        protected override void OnResume() {
            base.OnResume();
            var tbLayout = FindViewById<TableLayout>(Resource.Id.tableLayout);
            //results = wifi.ScanResults;
            //// 結果を表示
            //var txtView1 = FindViewById<TextView>(Resource.Id.textView1);
            
            //foreach (var res in results) {
            //    var tb_row = LayoutInflater.Inflate(Resource.Layout.tb_layout, null);

            //    var ssid = tb_row.FindViewById<TextView>(Resource.Id.rowtext1);
            //    var bssid = tb_row.FindViewById<TextView>(Resource.Id.rowtext2);
            //    var level = tb_row.FindViewById<TextView>(Resource.Id.rowtext3);
            //    var frequency = tb_row.FindViewById<TextView>(Resource.Id.rowtext4);
            //    ssid.Text = res.Ssid;
            //    bssid.Text = res.Bssid;
            //    level.Text = res.Level.ToString();
            //    frequency.Text = res.Frequency.ToString();

            //    tbLayout.AddView(tb_row);
            //}

            var edit = FindViewById<EditText>(Resource.Id.input_editTxt);
            var btnInput = FindViewById<Button>(Resource.Id.btnInput);
            var btnAddData = FindViewById<Button>(Resource.Id.btnAddData);
            var btnShow = FindViewById<Button>(Resource.Id.btnShowTable);
            var btnDelete = FindViewById<Button>(Resource.Id.btnDelete);
            btnAddData.Enabled = btnInput.Enabled = false;

            //イベント作成
            edit.AfterTextChanged += (_, __) => btnInput.Enabled = true;
            btnInput.Click += delegate {
                //TODO:計測場所の入力が完了したらScanを始める。
                btnAddData.Enabled  = true;
                room = edit.Text;
                Toast.MakeText(this, string.Format("入力:{0}", room), ToastLength.Short).Show();
                // スキャン開始
                scanWifi();
            };
            btnDelete.Click += delegate {
                db.deleteData();
                Toast.MakeText(this, "テーブルをDELETEしました。", ToastLength.Short).Show();
                btnAddData.Enabled = false;
            };
            btnAddData.Click += delegate {
                //db.insertAccessPoints(results, edit.Text);
                Toast.MakeText(this, string.Format("{0}件追加しました。\n", results.Count), ToastLength.Short).Show();
            };
            btnShow.Click += delegate {
                //TableActivityへ移動
                var intent = new Intent(this, typeof(TableActivity));
                intent.PutExtra("path", pathToDatabase);
                StartActivity(intent);
            };
        } //onResume()

        private void scanWifi() {
            //wifi情報を取得
            var intentFilter = new IntentFilter();
            intentFilter.AddAction(WifiManager.ScanResultsAvailableAction);
            var wifiRec = new WifiReceiver(wifi,room);
            RegisterReceiver(wifiRec, intentFilter);
            wifi.StartScan();
        }

    } // class mainactivity
} // namespace GetWifi