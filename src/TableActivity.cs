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

namespace GetWifi.src {
    [Activity]
    public class TableActivity : Activity {

        private string path;
        private database.DBConfig db;

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            var intent = this.Intent;
            if (intent != null && intent.HasExtra("path")) {
                path = intent.GetStringExtra("path") as string;
                Console.WriteLine(string.Format("get {0} by intent", path));
            }
            db = new database.DBConfig(path);

            SetContentView(Resource.Layout.TableActivityLayout);

            //結果をtextviewに代入
            var colums = 0;
            var txtView = FindViewById<TextView>(Resource.Id.tb_TextView1);
            var tb_layout = FindViewById<TableLayout>(Resource.Id.tb_tableLayout);
            var ap_tb = db.getAccessPoints(ref colums);
            txtView.Text = string.Format("DBから{0}件のデータを取得しました.", colums);
            foreach(var ap in ap_tb) {
                //Inflatorでレイアウトを追加
                var tb_row = LayoutInflater.Inflate(Resource.Layout.tb_layout2, null);

                var id          =   tb_row.FindViewById<TextView>(Resource.Id.rowtext1);
                var room        =   tb_row.FindViewById<TextView>(Resource.Id.rowtext2);
                var ssid        =   tb_row.FindViewById<TextView>(Resource.Id.rowtext3);
                var bssid       =   tb_row.FindViewById<TextView>(Resource.Id.rowtext4);
                var level       =   tb_row.FindViewById<TextView>(Resource.Id.rowtext5);
                var dispersion  =   tb_row.FindViewById<TextView>(Resource.Id.rowtext6);
                

                id.Text             =    ap.ID.ToString();
                room.Text           =    ap.Room;
                ssid.Text           =    ap.SSID;
                bssid.Text          =    ap.BSSID;
                level.Text          =    ap.Level.ToString();
                dispersion.Text     =    ap.Dispersion.ToString();

                tb_layout.AddView(tb_row);
            }

            var btnBack = FindViewById<Button>(Resource.Id.tb_btnBack);
            btnBack.Click += delegate {
                var intentToMain = new Intent(this, typeof(MainActivity));
                StartActivity(intentToMain);
            };
        }
    }
}