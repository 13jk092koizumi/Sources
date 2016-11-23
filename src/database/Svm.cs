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
            //部屋名をAccessPointテーブルから取り出す
            var roomList = mDb.getScanDateLog();
            Console.WriteLine("roomListの中身:createTrainFile()");
            foreach(var room_item in roomList) {
                Console.WriteLine("[{0}, {1}]", room_item.Room, room_item.Date);
            }
            
            //パラメータを[部屋名 BSSID：LEVEL...]の形で入れてみる。
            var scanData = mDb.getScanData().ToList();
            var svmParamList = new List<SvmParam>();
            foreach (var room_item in roomList) {
                //room_itemのDateと等しいカラムを取り出す。
                var sameDateScanData = scanData.Where(w => w.Date == room_item.Date);
                if (sameDateScanData == null) { Console.WriteLine("ScanDataにScanDateLogの日付と一致するデータがありません！"); continue; }
                foreach (var scan_item in sameDateScanData) {
                    svmParamList.Add(new SvmParam {
                        Lavel = getRoomIndex(room_item.Room),
                        paramId = getBssidIndex(scan_item.BSSID),
                        param = scan_item.Level
                    });
                }
            }
            var svmFormatList = svmParamList.ToLookup(svm => svm.Lavel); //1対多のリストに変換
            outputSvmFile(svmFormatList); //テキストファイルとして出力
            
            return "出力完了しました。";
        }

        private void outputSvmFile( ILookup<int, SvmParam> paramList) {
            var file = new SaveFile("/RoomBssidLevel.txt", true);
            var sb = new StringBuilder();
            foreach (var svm in paramList) {
                sb.Append(svm.Key);
                Console.WriteLine("roomID {0} につき {1} 個のカラムがあります", svm.First().Lavel, svm.Count());
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
                Console.WriteLine("getRoomIndex():該当するRoomIndexが見つかりませんでした");
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