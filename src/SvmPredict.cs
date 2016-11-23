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
    class SvmPredict {
        private Context context;

        public SvmPredict(Context context) {
            this.context = context;
        }

        public void predict(int scanNum) {
            var wifiMng = (WifiManager)context.GetSystemService(Context.WifiService);
            var receiver = new PredictReceiver(wifiMng, scanNum);
            var filter = new IntentFilter();
            filter.AddAction(WifiManager.ScanResultsAvailableAction);
            context.RegisterReceiver(receiver, filter);
            wifiMng.StartScan();
        }

        private class PredictReceiver : BroadcastReceiver {
            private int mScanNum;
            private int mLoop;
            private ProgressDialog mProg;
            private WifiManager mWifiMng;
            private List<List<ScanResult>> mResultsList;

            public PredictReceiver(WifiManager wifiMng, int scanNum) {
                mScanNum = scanNum;
                mLoop = 1;
                mWifiMng = wifiMng;
                mResultsList = new List<List<ScanResult>>();
            }
            public async override void OnReceive(Context context, Intent intent) {
                if (mLoop == 1) {
                    mProg = new ProgressDialog(context);
                    mProg.SetProgressStyle(ProgressDialogStyle.Horizontal);
                    mProg.SetMessage("スキャン中です…");
                    mProg.Max = mScanNum;
                    mProg.Progress = 0;
                    mProg.Show();
                }
                if (mLoop <= mScanNum) {
                    await Task.Run(() => doProcess());
                    mProg.Progress = mLoop;
                    ++mLoop;
                    mWifiMng.StartScan();
                } else {
                    var svm = new database.Svm();
                    Android.Util.Log.Debug("PredictBroadcast", "スキャン終了");
                    mProg.Dismiss();
                    Toast.MakeText(context, "テストデータを出力しました。", ToastLength.Short).Show();
                    int counter = 0;
                    foreach (var result in mResultsList) {
                        Android.Util.Log.Debug("PredictBroadcast:", "スキャン{0}回目　要素数{1}", ++counter, result.Count);
                    }
                    context.UnregisterReceiver(this);
                }
            }
            private void doProcess() {
                var svm = new database.Svm();
                var sb = new StringBuilder();
                var results = mWifiMng.ScanResults;
                var sortedResult = (from sorted in results
                                    orderby sorted.Level descending
                                    select sorted).ToList().Take(10);
                var tempList = new List<ScanResult>();
                foreach (var item in sortedResult) {
                    tempList.Add(item);
                }
                mResultsList.Add(tempList);
            }
        }
    }
}