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
            
        } //OnCreate()
        
        private void swap(System.Collections.Generic.IList<ScanResult> list, int indexA, int indexB) {
            ScanResult temp= list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = temp;
        } //swap()

        /*resultsのリスト配列を電波強度(Level)でソート*/
        private void sortScanResult(System.Collections.Generic.IList<ScanResult> list) {
            int size = list.Count; //配列の大きさ取得
            int i, j; //for文用
            for (i = 0; i < size; ++i) {
                for (j = size - 1; j > i; --j) {
                    if (list[j - 1].Level < list[j].Level) { //電波強度が前の要素より大きかったら
                        swap(list, j - 1, j); //入れ替え
                    }
                }
            } //sortScanResult()

        }

    }
}

