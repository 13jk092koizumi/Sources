using System.Threading.Tasks;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Android.Net.Wifi;
using Android.OS;

namespace GetWifi.src {
    [Activity(Label = "GetWifi", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity {
        public static MainActivity mInstance;
        private WifiManager mWifi;
        private IList<ScanResult> mResults;
        private database.DBConfig mDb;
        private string pathToDatabase;
        private string mPlaceName;

        protected override void OnCreate(Bundle bundle) {
            SetContentView(Resource.Layout.Main);
            base.OnCreate(bundle);
            //パスの作成
            var docsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            pathToDatabase = System.IO.Path.Combine(docsFolder, "db_sqlnet.db");
            //DB作成
            mDb = new database.DBConfig(pathToDatabase);
            mDb.createDatabase();
            mInstance = this;
            mWifi = (WifiManager)GetSystemService(WifiService);
        } // OnCreate()

        protected override void OnResume() {
            base.OnResume();
            var tbLayout = FindViewById<TableLayout>(Resource.Id.tableLayout);
            mResults = mWifi.ScanResults;
            // 結果を表示
            var txtView1 = FindViewById<TextView>(Resource.Id.textView1);
            txtView1.Text = "APを" + mResults.Count.ToString() + "件発見しました";

            foreach (var res in mResults) {
                var tb_row = LayoutInflater.Inflate(Resource.Layout.tb_layout, null);
                var ssid = tb_row.FindViewById<TextView>(Resource.Id.rowtext1);
                var bssid = tb_row.FindViewById<TextView>(Resource.Id.rowtext2);
                var level = tb_row.FindViewById<TextView>(Resource.Id.rowtext3);
                ssid.Text = res.Ssid;
                bssid.Text = res.Bssid;
                level.Text = res.Level.ToString();
                tbLayout.AddView(tb_row);
            }

            var autoCompTxtView = FindViewById<AutoCompleteTextView>(Resource.Id.auto_comprete_text_view1);
            var btnInput = FindViewById<Button>(Resource.Id.btnInput); //スキャン開始ボタン
            btnInput.FocusableInTouchMode = true;
            var btnShow = FindViewById<Button>(Resource.Id.btnShowTable); //DATA LISTボタン
            var btnDelete = FindViewById<Button>(Resource.Id.btnDelete); //DELETEボタン
            var btnSave = FindViewById<Button>(Resource.Id.btnSaveFile); //SAVE FILEボタン

            //入力候補の表示
            var places = mDb.getAccessPoints();
            var adapter = new ArrayAdapter<string>(this, Resource.Layout.ArrayAdapterLayout);
            var tempList = new List<string>();
            foreach (var places_item in places) {
                bool distinctFlg = false;//重複フラグ
                foreach (var tempList_item in tempList) {
                    //tempListに同じRoomがあったら重複フラグをtrueに
                    if(tempList_item == places_item.Room) {
                        distinctFlg = true;
                    }
                }
                if (!distinctFlg) {
                    tempList.Add(places_item.Room);
                }
            }
            adapter.AddAll(tempList);
            autoCompTxtView.Adapter = adapter;
            //キーボードのエンターキー検知
            autoCompTxtView.KeyPress += (object sender,View.KeyEventArgs e)=> {
                e.Handled = false;
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter) {
                    var methodManager = (InputMethodManager)GetSystemService(InputMethodService);
                    var currentFocus = Window.CurrentFocus;
                    if (currentFocus != null) {
                        methodManager.HideSoftInputFromWindow(currentFocus.WindowToken, HideSoftInputFlags.None);
                    }
                }
                btnInput.RequestFocus(); //スキャンボタンにフォーカスを移動
            };
            //イベント作成
            btnInput.Click += async delegate {
                btnInput.Enabled = false; //連打防止のためいったん無効化
                mPlaceName = autoCompTxtView.Text;
                if (mPlaceName.Length == 0) {
                    Toast.MakeText(this, "注意:計測場所を入力しないとスキャンできません", ToastLength.Long).Show();
                    return;
                }
                Toast.MakeText(this, string.Format("入力:{0}", mPlaceName), ToastLength.Short).Show();
                //DB内に同じ計測場所があったら上書きするか選ばせる。(TODO:AlertDialogでできるのでは？)
                // スキャン開始
                scanWifi();
                await Task.Run(()=>System.Threading.Thread.Sleep(1000)); //1秒ボタンを無効化
                btnInput.Enabled = true; //ボタンを有効に
            };
            btnDelete.Click += delegate {
                mDb.deleteData();
                Toast.MakeText(this, "テーブルをDELETEしました。", ToastLength.Short).Show();
            };
            btnShow.Click += delegate {
                //TableActivityへ移動
                var intent = new Intent(this, typeof(TableActivity));
                intent.PutExtra("path", pathToDatabase);
                StartActivity(intent);
            };
            btnSave.Click += delegate {
                //DBのデータをファイルに出力
                var save = new SaveFile("/ScanDataLog.csv");
                save.WriteAll(mDb.createCsv());
                string result = save.ReadAll();
                Toast.MakeText(this, string.Format("{0}byte読み込みました",result.Length), ToastLength.Short).Show();
                //save.Delete();
            };
        } //onResume()

        private void scanWifi() {
            //wifi情報を取得
            var intentFilter = new IntentFilter();
            intentFilter.AddAction(WifiManager.ScanResultsAvailableAction);
            var wifiRec = new WifiReceiver(mWifi,mPlaceName);
            RegisterReceiver(wifiRec, intentFilter);
            mWifi.StartScan();
        }

    } // class mainactivity
} // namespace GetWifi