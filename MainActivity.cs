using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Net.Wifi;
using Android.OS;

namespace GetWifi {
    [Activity(Label = "GetWifi", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity {

        protected override void OnCreate(Bundle bundle) {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            var linearLayout = FindViewById<LinearLayout>(Resource.Id.LLayout);
            
            //TextView生成
            var tv = new TextView(this);

            //wifi情報を取得
            var wifi = (WifiManager)GetSystemService(WifiService);
            //アクセスポイントのスキャン
            wifi.StartScan();
            //結果の取得
            var results = wifi.ScanResults;
            //結果をTextViewにセット
            var sb = new System.Text.StringBuilder();

            for (int i=0; i<results.Count; ++i) {
                sb.Append(results[i].ToString());
                sb.Append("\n");                
            }
            tv.SetTextSize(Android.Util.ComplexUnitType.Dip, 20);
            tv.Text = sb.ToString();
            linearLayout.AddView(tv);
            
        }
    }
}

