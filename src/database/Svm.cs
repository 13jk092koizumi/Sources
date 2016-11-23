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

namespace GetWifi.src.database {
    class Svm {
        private DBConfig mDb;
        //private Dictionary<string, DateTime> mParams;
        private class SvmParam {
            public int Lavel { get; set; }
            public int paramId { get; set; }
            public int param { get; set; }
            public override string ToString() {
                return string.Format(" {0}:{1}", paramId, param);
            }
        };
        
        public Svm() {
            string docsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            string path = System.IO.Path.Combine(docsFolder, "db_sqlnet.db");
            mDb = new DBConfig(path);
            //mParams = new Dictionary<string, DateTime>();
        }

        public string createTrainFile() {
            //��������AccessPoint�e�[�u��������o��
            var roomList = mDb.getScanDateLog();
            Console.WriteLine("roomList�̒��g:createTrainFile()");
            foreach(var room_item in roomList) {
                Console.WriteLine("[{0}, {1}]", room_item.Room, room_item.Date);
            }
            
            //�p�����[�^��[������ BSSID�FLEVEL...]�̌`�œ���Ă݂�B
            var scanData = mDb.getScanData().ToList();
            var svmParamList = new List<SvmParam>();
            foreach (var room_item in roomList) {
                //room_item��Date�Ɠ������J���������o���B
                var sameDateScanData = scanData.Where(w => w.Date == room_item.Date);
                if (sameDateScanData == null) { Console.WriteLine("ScanData��ScanDateLog�̓��t�ƈ�v����f�[�^������܂���I"); continue; }
                foreach (var scan_item in sameDateScanData) {
                    svmParamList.Add(new SvmParam {
                        Lavel = getRoomIndex(room_item.Room),
                        paramId = getBssidIndex(scan_item.BSSID),
                        param = scan_item.Level
                    });
                }
            }
            var svmFormatList = svmParamList.ToLookup(svm => svm.Lavel); //1�Α��̃��X�g�ɕϊ�
            outputSvmFile(svmFormatList); //�e�L�X�g�t�@�C���Ƃ��ďo��
            
            return "�o�͊������܂����B";
        }

        private void outputSvmFile( ILookup<int, SvmParam> paramList) {
            var file = new SaveFile("/RoomBssidLevel.txt", true);
            var sb = new StringBuilder();
            foreach (var svm in paramList) {
                sb.Append(svm.Key);
                Console.WriteLine("roomID {0} �ɂ� {1} �̃J����������܂�", svm.First().Lavel, svm.Count());
                foreach(var svm_item in svm) {
                    sb.Append(svm_item.ToString());
                }
                sb.AppendLine();
                //Console.Write(sb.ToString());
            }
            file.Write(sb.ToString());
        }

        private int getRoomIndex(string room) {
            var result = mDb.getRoomIndex().Where(w => w.Room == room).FirstOrDefault();
            if(result == null) {
                Console.WriteLine("getRoomIndex():�Y������RoomIndex��������܂���ł���");
                return -1;
            }
            return result.ID;
        }

        private int getBssidIndex(string bssid) {
            var result = mDb.getBssidIndex().Where(w => w.BSSID == bssid).FirstOrDefault();
            if(result == null) {
                Console.WriteLine("�Y������BSSIDIndex��������܂���ł����B");
                return -1;
            }
            return result.ID;
        }
    }
}