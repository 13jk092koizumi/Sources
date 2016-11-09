using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Net.Wifi;

using SQLite;

using GetWifi.src.database;

namespace GetWifi.src {
    [BroadcastReceiver]
    public class WifiReceiver : BroadcastReceiver {
        WifiManager mWifiMng;        //wifiManager
        IList<ScanResult> mResults;  //ScanResult List
        ProgressDialog mProgDialog;
        DateTime mDate;              //start scan DateTime
        DBConfig mDb;                //database
        string mPlaceName;           //scaned place name
        const int mLoopMax = 200;     //how many scan
        int mLoopCount;
        public WifiReceiver() { }

        public WifiReceiver(WifiManager wifi_manager, string place) {
            mWifiMng = wifi_manager;
            mPlaceName = place;
            mDate = DateTime.Now;
            //DBセット
            var docsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            var pathToDB = System.IO.Path.Combine(docsFolder, "db_sqlnet.db");
            mDb = new DBConfig(pathToDB);
            //プログレスダイアログの初期化
            mProgDialog = new ProgressDialog(MainActivity.mInstance);
            
            mProgDialog.SetMessage("スキャン中");
            mProgDialog.SetButton("キャンセル", (s,e)=> {
                mDb.resetScanData();//中途半端なカラムを削除
                MainActivity.mInstance.UnregisterReceiver(this); //BroadcastReceiver解除
                Toast.MakeText(MainActivity.mInstance, "キャンセルしました。", ToastLength.Long).Show();
            });
            mProgDialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
            mProgDialog.Max = mLoopMax;
            mLoopCount = 1;
            var eventTxt = MainActivity.mInstance.FindViewById<TextView>(Resource.Id.EventText);
            eventTxt.Text = "";
        }

        public async override void OnReceive(Context context, Intent intent) {
            if (mLoopCount == 1) {
                mProgDialog.Show();
            }
            var action = intent.Action;
            if (!action.Equals(WifiManager.ScanResultsAvailableAction)) {
                Console.WriteLine("FAIL!!");
                throw new ApplicationException("error! can't get broadcast");
            }

            mResults = sortScanResult();
            var size = mResults.Count;
            if (size > 0) {
                mDb.insertScanData(mResults, mDate);//DBにいったん保存
                await Task.Run(() => updateProgress());
                mProgDialog.Progress = mLoopCount;
                ++mLoopCount;
                if (mLoopCount <= mLoopMax) {
                    mWifiMng.StartScan();
                } else {
                    var builder = new AlertDialog.Builder(MainActivity.mInstance);
                    builder.SetMessage("スキャンが終了しました");
                    var dialog = builder.Create();
                    mProgDialog.Dismiss();
                    dialog.Show();
                    //データの追加                    
                    mDb.insertAccessPoints(mResults, mPlaceName, mDate);
                        
                    context.UnregisterReceiver(this);
                }
            } else {
                context.UnregisterReceiver(this);
                Console.WriteLine("SCAN FAILED!!");
            }
            
        }

        private void swap(IList<ScanResult> list, int indexA, int indexB) {
            ScanResult temp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = temp;
        } // swap()

        /// <summary>
        /// resultsのリスト配列を電波強度(Level)で降順ソート
        /// </summary>
        private IList<ScanResult> sortScanResult() {
            var list = mWifiMng.ScanResults;
            int size = list.Count; //配列の大きさ取得
            int i, j; //for文用
            for (i = 0; i < size; ++i) {
                for (j = size - 1; j > i; --j) {
                    if (list[j - 1].Level < list[j].Level) { //前の要素の方が小さかったら
                        swap(list, j - 1, j); //入れ替え
                    }
                }
            }
            return list;
        } // sortScanResult()

        private void updateProgress() {
            int time = calcWaitTime();
            string message = string.Format("スキャン中　残り{0}秒", time);
            new Handler(Looper.MainLooper).Post(() => mProgDialog.SetMessage(message));
            System.Threading.Thread.Sleep(300);
        }

        private int calcWaitTime() {
            var nowTime = DateTime.Now;
            var ts = nowTime - mDate;
            double waitTime = (ts.TotalSeconds / mLoopCount) * (mLoopMax - mLoopCount);
            return (int)waitTime;
        } //calcWaitTime()
    }
}