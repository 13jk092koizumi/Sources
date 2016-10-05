using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Net.Wifi;

namespace GetWifi.src {
    [Activity(Label = "OpenAppActivity", MainLauncher = false)]
    public class OpenAppActivity : Activity {

        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.FirstMenu);
            var button1 = FindViewById<Button>(Resource.Id.button1);
            var tv = FindViewById<TextView>(Resource.Id.ThreadProcess);
            button1.Click += async (sender, e) => {
                int index = 0;

                while (index < 10) {
                    await Task.Run(() => {
                        var wifiMng = (WifiManager)GetSystemService(WifiService);
                        wifiMng.StartScan();
                        System.Threading.Thread.Sleep(500);
                        
                    }); // ワーカースレッド
                    if (index < 10) {
                        int num = index + 1;
                        tv.Text = string.Format("スキャン中({0}回目)", num); // UIスレッド
                    }
                    index++;
                }
                tv.Text = "スキャン完了";

            };
            // Create your application here
        }
    }
}