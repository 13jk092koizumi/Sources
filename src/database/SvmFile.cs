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
    class SvmFile {
        private DBConfig mDb;
        
        public SvmFile() {
            string docsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            string path = System.IO.Path.Combine(docsFolder, "db_sqlnet.db");
            mDb = new DBConfig(path);
        }

        public string createTrainFile() {
            var sb = new StringBuilder();
            var roomList = mDb.getScanDateLog();
            Console.WriteLine("roomListの中身:createTrainFile()");
            foreach(var room_item in roomList) {
                Console.WriteLine("[{0}, {1}]", room_item.Room, room_item.Date);
            }
            //ScanDataテーブルを取得
            var scanData = mDb.getScanData();
            foreach(var room in roomList) {
                //日付が一致する部屋IDを探す
                var sameDateScanData = scanData.Where(w => w.Date == room.Date);
                if (sameDateScanData == null) { Console.WriteLine("ScanDataに{0}と一致するデータがありません！", room.Date); continue; }
                //取り出した1部屋分のスキャンデータを、スキャン回数でグループ化
                foreach(var scanNumGroup in sameDateScanData.GroupBy(g => g.ScanNum)) {                    
                    //ラベルのセット
                    sb.AppendFormat("{0}", getRoomIndex(room.Room));
                    foreach (var one_scan in scanNumGroup) {
                        int bssidIndex = getBssidIndex(one_scan.BSSID);
                        if (bssidIndex > 0) {
                            sb.AppendFormat(" {0}:{1}", bssidIndex, one_scan.Level);
                        }else {
                            sb.Append(" 0");
                        }
                    }
                    sb.AppendLine();
                }
            }
            var file = new SaveFile("/RoomBssidLevel.txt", true);
            file.Write(sb.ToString());

            return "出力完了しました。";
        }

        public void createPredictFile(List<List<Android.Net.Wifi.ScanResult>> results, string fileName) {
            var sb = new StringBuilder();
            var save = new SaveFile(fileName,true);
            int lavel = 0;
            foreach(var res in results) {
                lavel++;
                sb.Append(lavel.ToString());
                foreach(var res_item in res) {
                    sb.AppendFormat(" {0}:{1}", getBssidIndex(res_item.Bssid), res_item.Level);
                }
                sb.AppendLine();
            }
            Console.Write(sb.ToString());
            save.Write(sb.ToString());
        }

        private int getRoomIndex(string room) {
            var result = mDb.getRoomIndex().Where(w => w.Room == room).FirstOrDefault();
            if(result == null) {
                Console.WriteLine("getRoomIndex():該当する{0}のIndexは見つかりませんでした。",room);
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

        private void testLibSvm() {

        }
    }
}