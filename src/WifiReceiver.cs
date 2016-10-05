using System;
using System.Threading;
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
        string mPlaceName;           //scaned place name
        const int mLoopMax = 10;     //how many scan
        int mLoopCount;
        public WifiReceiver() { }

        public WifiReceiver(WifiManager wifi_manager, string place) {
            mWifiMng = wifi_manager;
            mPlaceName = place;
            //�v���O���X�_�C�A���O�̏�����
            mProgDialog = new ProgressDialog(MainActivity.Instance);
            mProgDialog.SetMessage("�X�L������");
            mProgDialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
            mProgDialog.Max = mLoopMax;
            mLoopCount = 1;
            var eventTxt = MainActivity.Instance.FindViewById<TextView>(Resource.Id.EventText);
            eventTxt.Text = "";
        }

        public override void OnReceive(Context context, Intent intent) {
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
            var docsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            var pathToDB = System.IO.Path.Combine(docsFolder, "db_sqlnet.db");
            var db = new DBConfig(pathToDB);
            if (size > 0) {
                int index = 0;
                foreach (var sr in mResults) {
                    ++index;
                }
                db.insertScanData(mResults);//DB�ɂ�������ۑ�
                mProgDialog.Progress = mLoopCount;
                ++mLoopCount;
                if (mLoopCount <= mLoopMax) {
                    mWifiMng.StartScan();
                } else {
                    var builder = new AlertDialog.Builder(MainActivity.Instance);
                    builder.SetMessage("�X�L�������I�����܂���");
                    var dialog = builder.Create();
                    dialog.Show();
                    mProgDialog.Dismiss();
                    //�f�[�^�̒ǉ�                    
                    db.insertAccessPoints(mResults, mPlaceName);
                        
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
        /// results�̃��X�g�z���d�g���x(Level)�ō~���\�[�g
        /// </summary>
        private IList<ScanResult> sortScanResult() {
            var list = mWifiMng.ScanResults;
            int size = list.Count; //�z��̑傫���擾
            int i, j; //for���p
            for (i = 0; i < size; ++i) {
                for (j = size - 1; j > i; --j) {
                    if (list[j - 1].Level < list[j].Level) { //�O�̗v�f�̕���������������
                        swap(list, j - 1, j); //����ւ�
                    }
                }
            }
            return list;
        } // sortScanResult()
    }
}