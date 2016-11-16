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
            //部屋名をAccessPointテーブルから取り出す
            var accessPoints = mDb.getAccessPoints().ToList();
            var roomList = (from ap in accessPoints
                           select new { ap.Room, ap.Date }).Distinct();
            //Console.WriteLine("roomListの中身");
            foreach(var room_item in roomList) {
                //Console.WriteLine("{0}, {1}", room_item.Room, room_item.Date);
                mParams.Add(room_item.Room, room_item.Date);
            }
            
            //パラメータを[部屋名 BSSID：LEVEL...]の形で入れてみる。
            var table = mDb.getScanData().ToList();
            var scanData = table.GroupBy(scandata => scandata.Date);
            var svmParamList = new List<SvmParam>();
            foreach(var roomList_item in roomList) {
                //Dateが等しいものを探してRoomを割り当てる
                foreach (var scanData_item in scanData) {
                    var sameDate = from scan in scanData_item
                                     where scanData_item.Key == roomList_item.Date
                                     orderby scan.ID
                                     select scan;
                    foreach (var sameDate_item in sameDate) {
                        //Dateが等しい＝計測場所が一致するのでSVM用にデータを整理
                        svmParamList.Add(new SvmParam {
                            Lavel = getRoomIndex(roomList_item.Room),
                            paramId = getBssidIndex(sameDate_item.BSSID),
                            param = sameDate_item.Level
                        });
                    }
                }                             
            }
            var svmFormatList = svmParamList.ToLookup(svm => svm.Lavel); //1対多のリストに変換
            outputSvmFile(svmFormatList); //テキストファイルとして出力
            return "出力完了しました。";
        }

        private void outputSvmFile( ILookup<int, SvmParam> paramList) {
            var file = new SaveFile("/LRoomPIDBssidParamLevel.txt", true);
            var sb = new StringBuilder();
            foreach (var svm in paramList) {
                sb.Append(svm.Key);
                //Console.WriteLine("roomID {0} につき {1} 個のカラムがあります", svm.First().Lavel, svm.Count());
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
                Console.WriteLine("該当するRoomIndexが見つかりませんでした");
                return -1;
            }
            return result.ID;
        }

        private int getBssidIndex(string bssid) {
            var result = mDb.getBssidIndex().Where(w => w.BSSID == bssid).FirstOrDefault();
            if(result == null) {
                Console.WriteLine("該当するBSSIDIndexが見つかりませんでした。");
                return -1;
            }
            return result.ID;
        }
    }
}