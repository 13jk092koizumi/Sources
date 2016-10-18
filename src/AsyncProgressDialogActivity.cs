using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Net.Wifi;


namespace GetWifi.src {
    [Activity(Label = "AsyncProgressDialogActivity", MainLauncher = false)] //c#��'����'�ł��Bjava�ł͂���܂���
    public class AsyncProgressDialogActivity : Activity {
        public const string MyScanAction = "MY_SCAN_ACTION"; //�萔�̕�����Bconst = final
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ProgressDialogLayout);
            //ProgressDialog�̒�`
            ProgressDialog progressDialog = new ProgressDialog(this);
            progressDialog.SetTitle("�X�L������");
            progressDialog.SetMessage("Message"); //��ŕς��̂łȂ�ł�����
            progressDialog.SetProgressStyle(ProgressDialogStyle.Horizontal); //�啶����������java�ƈႢ�܂����d�l�ł�
            Button button = FindViewById<Button>(Resource.Id.scan_button);

            //�{�^���̃C�x���g�Bjava�ł���onClick()�ł��B
            button.Click += delegate {
                //BroadCastReceiver�̃u���[�h�L���X�g(�A�N�V����)�����삵�Ă��܂��B�֗������Ȃ̂ł���Ă݂܂���
                IntentFilter filter = new IntentFilter();
                filter.AddAction(MyScanAction); //������ScanResulutsAvailableAction�ɕς��Ă�������
                ScanReceiver receiver = new ScanReceiver(progressDialog);
                RegisterReceiver(receiver, filter);
                SendBroadcast(new Intent(MyScanAction)); //Broadcast���M(����̃A�N�V�����̂���)
            };
            
        }

        public class ScanReceiver : BroadcastReceiver {
            Thread thread;
            ProgressDialog progressDialog;
            Context context;
            public ScanReceiver(ProgressDialog dialog) {
                progressDialog = dialog;
            }
            public override void OnReceive(Context context, Intent intent) {
                this.context = context;
                exeThread(); //�񓯊��������s
                context.UnregisterReceiver(this); //�o�^������K����������
            }

            //�񓯊������B�O�ɏo�����̂ɐ[���Ӗ��͖����ł�
            private void exeThread() {
                thread = new Thread(() => {
                    Handler handler = new Handler(context.MainLooper); //UI�X���b�h�ɃA�N�Z�X���邽��Handler���g���܂�

                    handler.Post(() => { progressDialog.Show(); }); //java����Runnable()���K�v���Ǝv���܂�
                    WifiManager wifi = (WifiManager)context.GetSystemService(WifiService);
                    
                    //i��X�L��������J��Ԃ�����
                    for (int i = 1; i <= 10; i++) {
                        handler.Post(() => {
                            progressDialog.Progress = i * 10; progressDialog.SetMessage(string.Format("����{0}��", i * 10)); //�񓯊��̂������œ��I��Message��ς����܂��B
                        });
                        wifi.StartScan();

                        /*DB�փf�[�^��ǉ����鏈���Ȃǂ�������*/

                        Thread.Sleep(1000);//���U�Ńv���O�������I������Ⴄ�̂�
                    }
                    
                    handler.Post(() => { progressDialog.Dismiss(); });
                    handler.Post(() => { Toast.MakeText(context, "�X�L�����I��", ToastLength.Short).Show(); });
                });
                thread.Start();
            }
        }
    }
}