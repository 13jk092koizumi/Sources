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

namespace GetWifi.src.database {
    class Svm {
        private DBConfig mDb;
        private Dictionary<string, DateTime> mParams;
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
            mParams = new Dictionary<string, DateTime>();
        }

        public string createTrainFile() {
            //��������AccessPoint�e�[�u��������o��
            var accessPoints = mDb.getAccessPoints().ToList();
            var roomList = (from ap in accessPoints
                           select new { ap.Room, ap.Date }).Distinct();
            //Console.WriteLine("roomList�̒��g");
            foreach(var room_item in roomList) {
                //Console.WriteLine("{0}, {1}", room_item.Room, room_item.Date);
                mParams.Add(room_item.Room, room_item.Date);
            }
            
            //�p�����[�^��[������ BSSID�FLEVEL...]�̌`�œ���Ă݂�B
            var table = mDb.getScanData().ToList();
            var scanData = table.GroupBy(scandata => scandata.Date);
            var svmParamList = new List<SvmParam>();
            foreach(var roomList_item in roomList) {
                //Date�����������̂�T����Room�����蓖�Ă�
                foreach (var scanData_item in scanData) {
                    var sameDate = from scan in scanData_item
                                     where scanData_item.Key == roomList_item.Date
                                     orderby scan.ID
                                     select scan;
                    foreach (var sameDate_item in sameDate) {
                        //Date�����������v���ꏊ����v����̂�SVM�p�Ƀf�[�^�𐮗�
                        svmParamList.Add(new SvmParam {
                            Lavel = getRoomIndex(roomList_item.Room),
                            paramId = getBssidIndex(sameDate_item.BSSID),
                            param = sameDate_item.Level
                        });
                    }
                }                             
            }
            var svmFormatList = svmParamList.ToLookup(svm => svm.Lavel); //1�Α��̃��X�g�ɕϊ�
            outputSvmFile(svmFormatList); //�e�L�X�g�t�@�C���Ƃ��ďo��
            return "�o�͊������܂����B";
        }

        private void outputSvmFile( ILookup<int, SvmParam> paramList) {
            var file = new SaveFile("/LRoomPIDBssidParamLevel.txt", true);
            var sb = new StringBuilder();
            foreach (var svm in paramList) {
                sb.Append(svm.Key);
                //Console.WriteLine("roomID {0} �ɂ� {1} �̃J����������܂�", svm.First().Lavel, svm.Count());
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
                Console.WriteLine("�Y������RoomIndex��������܂���ł���");
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