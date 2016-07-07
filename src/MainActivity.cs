using System.Threading.Tasks;
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

        private IList<ScanResult> results;
        private database.DBConfig db;
        private string pathToDatabase;
        private string room;

        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            Toast.MakeText(this, "計測場所を入力してください。", ToastLength.Short).Show();
            var tbLayout = FindViewById<TableLayout>(Resource.Id.tableLayout);
            // スキャン結果の取得
            results = getWifi();
            // 結果を表示
            var txtView1 = FindViewById<TextView>(Resource.Id.textView1);
            var apNum = "APを" + results.Count + "件見つけました。";
            txtView1.Text = apNum;
            foreach(var res in results) {
                var tb_row  = LayoutInflater.Inflate(Resource.Layout.tb_layout, null);

                var ssid      =   tb_row.FindViewById<TextView>(Resource.Id.rowtext1);
                var bssid     =   tb_row.FindViewById<TextView>(Resource.Id.rowtext2);
                var level     =   tb_row.FindViewById<TextView>(Resource.Id.rowtext3);
                var frequency =   tb_row.FindViewById<TextView>(Resource.Id.rowtext4);
                ssid.Text       =   res.Ssid;
                bssid.Text      =   res.Bssid;
                level.Text      =   res.Level.ToString();
                frequency.Text  =   res.Frequency.ToString();

                tbLayout.AddView(tb_row);
            }
            
        } // OnCreate()

        protected override void OnResume() {
            base.OnResume();

            //データベースに保存する
            // create variables for onscreen widgets
            var edit = FindViewById<EditText>(Resource.Id.input_editTxt);
            var btnInput = FindViewById<Button>(Resource.Id.btnInput);
            var btnAddData = FindViewById<Button>(Resource.Id.btnAddData);
            var btnShow = FindViewById<Button>(Resource.Id.btnShowTable);
            var btnDelete = FindViewById<Button>(Resource.Id.btnDelete);

            //パスの作成
            var docsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            pathToDatabase = System.IO.Path.Combine(docsFolder, "db_sqlnet.db");
            //DB作成
            db = new database.DBConfig(pathToDatabase);
            btnAddData.Enabled = btnShow.Enabled = btnInput.Enabled = false;
            db.createDatabase();
            
            //イベント作成
            edit.AfterTextChanged += (_, __) => btnInput.Enabled = true;
            btnInput.Click += delegate {
                btnAddData.Enabled = btnShow.Enabled = true;
                room = edit.Text;
                Toast.MakeText(this, string.Format("入力:{0}", room), ToastLength.Short).Show();
            };
            btnDelete.Click += delegate {
                db.deleteData(pathToDatabase);
                Toast.MakeText(this, "テーブルをDELETEしました。", ToastLength.Short).Show();
                btnAddData.Enabled = btnShow.Enabled = false;
            };
            btnAddData.Click += delegate {
                db.insertScanResult(results, edit.Text);
                Toast.MakeText(this, string.Format("{0}件追加しました。\n", results.Count), ToastLength.Short).Show();
            };
            btnShow.Click += delegate {
                //TableActivityへ移動
                var intent = new Intent(this, typeof(TableActivity));
                var bundle = new Bundle();
                intent.PutExtra("path", pathToDatabase);
                StartActivity(intent);
            };
        } //onResume()
        
        private IList<ScanResult> getWifi() {
            //wifi情報を取得
            var wifi = (WifiManager)GetSystemService(WifiService);
            //アクセスポイントのスキャン
            
            var sr = wifi.ScanResults;
            sortScanResult(sr);
            return sr;
        }

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