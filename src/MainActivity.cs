using System.Threading.Tasks;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Android.Net.Wifi;
using Android.OS;
using System;

namespace GetWifi.src {
    [Activity(Label = "GetWifi", MainLauncher = true, Icon = "@drawable/icon",ScreenOrientation =Android.Content.PM.ScreenOrientation.Portrait)]
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
            //結果を表示
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
                        if (tempList_item == places_item.Room) {
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
            autoCompTxtView.KeyPress += (object sender, View.KeyEventArgs e) => {
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
                        await Task.Run(() => System.Threading.Thread.Sleep(1000)); //1秒ボタンを無効化
                        btnInput.Enabled = true; //ボタンを有効に
                        return;
                    }
                    Toast.MakeText(this, string.Format("入力:{0}", mPlaceName), ToastLength.Short).Show();
                    //スキャン開始
                    scanWifi();
                    await Task.Run(() => System.Threading.Thread.Sleep(1000)); //1秒ボタンを無効化
                    btnInput.Enabled = true; //ボタンを有効に
                };
            btnDelete.Click += delegate {
                mDb.deleteData();
                Toast.MakeText(this, "テーブルをDELETEしました。", ToastLength.Long).Show();
            };
            btnShow.Click += delegate {
                //TableActivityへ移動
                var intent = new Intent(this, typeof(TableActivity));
                intent.PutExtra("path", pathToDatabase);
                StartActivity(intent);
            };
            btnSave.Click += delegate {
                var menu = new PopupMenu(this, btnSave);
                menu.Inflate(Resource.Menu.IOFileMenu);
                menu.Show();
                //メニュー選択時のイベント
                menu.MenuItemClick += (s, arg) => {
                    if (arg == null) {
                        Console.WriteLine("arg was null!!"); return;
                    }
                    switch (arg.Item.ItemId) {
                        case Resource.Id.io_file_menu_item01:
                            outCsvFile(); break;
                        case Resource.Id.io_file_menu_item02:
                            menu.Dismiss();
                            outSvmFile(); break;
                        default:
                            menu.Dismiss(); break;
                    }
               };
            };
            FindViewById<Button>(Resource.Id.btnReset).Click += delegate {
                var message = mDb.resetScanData();
                Toast.MakeText(this, message, ToastLength.Short).Show();
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

        private void outCsvFile() {
            //DBのデータをファイルに出力
            var save = new SaveFile("/ScanDataLog.csv", true);
            save.WriteAll(mDb.createCsv());
            string result = save.ReadAll();
            Toast.MakeText(this, string.Format("{0}byteのファイルを出力しました。",result.Length), ToastLength.Long).Show();
            //save.Delete();
        }

        private void outSvmFile() {
            /*string ID = null;
            string param = null;
            var dlg = setdialogView( ID,  param);
            dlg.SetNegativeButton("キャンセル", (s, arg) => { });
            dlg.Create().Show();
            */
            var svm = new database.Svm();
            string message = svm.createTrainFile();
            Toast.MakeText(this, message, ToastLength.Long).Show();
        }

        private AlertDialog.Builder setdialogView(string ID, string param) {
            var items = new[] { "BSSID", "LEVEL", "LEVEL(AVERAGE)" };
            var dlg = new AlertDialog.Builder(this);
            dlg.SetTitle("パラメータを選択");
            //ビュー作成
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };
            layout.SetGravity(GravityFlags.Left);
            var adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, items);
            var tvID = new TextView(this) { Text = "パラメータ番号" };
            var spinnerID = new Spinner(this) { Adapter = adapter };
            var tvParam = new TextView(this) { Text = "パラメータ値" };
            var spinnerParam = new Spinner(this) { Adapter = adapter };
            layout.AddView(tvID);
            layout.AddView(spinnerID);
            layout.AddView(tvParam);
            layout.AddView(spinnerParam);
            //ビューをダイアログにセット
            dlg.SetView(layout);
            //選択された要素の取得
            dlg.SetPositiveButton("OK", (s, arg) => {
                ID = (string)spinnerID.SelectedItem;
                param = (string)spinnerParam.SelectedItem;
            });
            return dlg;
        }

    } // class mainactivity
} // namespace GetWifi