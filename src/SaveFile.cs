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

namespace GetWifi.src {
    class SaveFile {
        private string mFilePath;
        
        /// <summary>
        /// 作りたいファイル名を引数とする
        /// </summary>
        /// <param name="fileName">"/ファイル名.csv"などとすること</param>
        public SaveFile(string fileName) {
            var folderPath = Android.OS.Environment.ExternalStorageDirectory;
            mFilePath = folderPath + "/Android/data/GetWifi.GetWifi/files" + fileName; // "/data/ooo.txt"
            //Console.WriteLine(mFilePath);
        }

        public SaveFile(string fileName, bool extendNameFlag) {
            if (extendNameFlag) {
                int lastIndex = fileName.LastIndexOf('.');
                string now = DateTime.Now.ToString("yyMMdd");
                fileName = fileName.Insert(lastIndex, now);
                Console.WriteLine(fileName);
            }
            var folderPath = Android.OS.Environment.ExternalStorageDirectory;
            mFilePath = folderPath + "/Android/data/GetWifi.GetWifi/files" + fileName; // "/data/ooo.txt"
            //Console.WriteLine(mFilePath);
        }

        public void Write(string content) {
            try {
                using(var sw = new System.IO.StreamWriter(mFilePath)) {
                    sw.Write(content);
                    sw.Close();
                }
            }catch(Exception e) {
                Console.WriteLine("failed save string line");
                throw new Exception(e.Message);
            }
        }

        public void WriteAll(List<string> contentList) {
            try {
                //Console.WriteLine(Android.OS.Environment.ExternalStorageState);
                    using (var sw = new System.IO.StreamWriter(mFilePath)) {
                        foreach (var item in contentList) {
                            sw.WriteLine(item);
                        }
                    sw.Close();
                    }
            }catch(Exception e) {
                Console.WriteLine(e.Message + " :failed to WRITE file!");
                throw;
            }
        }

        public string ReadAll() {
            try {
                var str = System.IO.File.ReadAllText(mFilePath);
                return str;
            }catch(Exception e) {
                Console.WriteLine(e.Message + " :failed to READ file!");
                throw;
            }
        }

        public void Delete() {
            try {
                System.IO.File.Delete(mFilePath);
            }catch(Exception e) {
                Console.WriteLine(e.Message + " :failed to DELETE file!");
                throw;
            }
        }
    }
}