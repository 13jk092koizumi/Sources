using System;
using System.Collections.Generic;

using SQLite;

namespace GetWifi.src.database {

    class DBConfig {
        private  SQLiteConnection connection;
        private string path;
        private const int ListSizeMax = 10; //�X�L��������AP�̕ۑ����

        public DBConfig(string db_path) {
            path = db_path;
            if(path != null)
            connection = new SQLiteConnection(this.path);
        }

        ~DBConfig() {
            connection.Close();
        }

        public string createDatabase() {
            try {
                connection.CreateTable<AccessPoint>();
                connection.CreateTable<ScanData>();
                return "Database created";
            }
            catch (SQLiteException ex) {
                return ex.Message;
            }
        }

        public void insertAccessPoints(IList<Android.Net.Wifi.ScanResult> scanResult, string placeName) {
            //TODO:100�񕪂̃X�L�������ʂ�������ScanData�e�[�u������A������ScanResults��AP����ɂ��ĕ��ρA���U�����߂�AccessPoint�e�[�u���ɕۑ�����B
            var wList = new List<AccessPoint>();
            //�e�[�u���ɒl����
            for (int i=0; i<scanResult.Count && i<ListSizeMax-1; ++i) {
                var scanDataTable = connection.Query<ScanData>("select * from ScanData where BSSID == ?",scanResult[i].Bssid);
                int average = 0;
                int dispersion = 0;
                int count = scanDataTable.Count;
                //BSSID���Ƃɕ��ςƕ��U���v�Z����B
                foreach (var tb_item in scanDataTable) {
                    average += tb_item.Level;
                }
                average /= count; //���ώZ�o
                foreach(var tb_item in scanDataTable) {
                    int element = tb_item.Level - average;
                    dispersion += element * element;
                }
                dispersion /= count; //���U�Z�o
                wList.Add(new AccessPoint {
                    Room            =   placeName,
                    SSID            =   scanResult[i].Ssid,
                    BSSID           =   scanResult[i].Bssid,
                    Level           =   average,
                    Dispersion      =   dispersion
                });
            }
            try {
                connection.InsertAll(wList);
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        } //insertAccessPoints()

        public void insertScanData(IList<Android.Net.Wifi.ScanResult> scanResult) {
            try {
                //�d�g���x�̋�������ListSizeMax�ۑ�
                var list = new List<ScanData>();
                for(int i= 0; i<scanResult.Count && i<ListSizeMax-1; ++i) {
                    list.Add(new ScanData {
                        BSSID = scanResult[i].Bssid,
                        Level = scanResult[i].Level
                    });
                }
                //foreach (var item in list) { Console.WriteLine(string.Format("{0}:{1}",item.ID,item.BSSID)); }
                connection.InsertAll(list);
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        } //insertScanData()

        public void dropTable() {
            try {
                connection.DropTable<AccessPoint>();
                connection.DropTable<ScanData>();
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        }

        public void deleteData() {
            try {
                connection.DeleteAll<AccessPoint>();
                connection.DeleteAll<ScanData>();
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        } //deleteData

        /// <summary>
        /// ap��AccessPoint�e�[�u���Ascan��ScanData�e�[�u�����폜
        /// </summary>
        /// <param name="tb_name">"ap" or "scan"</param>
        public void dropTable(string tb_name) {
            if(tb_name == "ap") {
                connection.DropTable<AccessPoint>();
            }else if (tb_name == "scan") {
                connection.DropTable<ScanData>();
            } else { Console.WriteLine("\t�e�[�u�����폜�ł��܂���ł����B\n"); }
        } //dropTable()

        public int findNumberRecords() {
            try {
                //���ׂẴ��R�[�h���J�E���g���܂��B �Ȃ̂Œx���ł��B
                var count = connection.ExecuteScalar<int>("SELECT Count(*) FROM AccessPoint");
                return count;
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        } //findNumberRecords()

        public TableQuery<AccessPoint> getAccessPoints() {
            try {
                var table = connection.Table<AccessPoint>();
                return table;
            }catch(SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        }

        public TableQuery<AccessPoint> getAccessPoints(ref int colums) {
            try {
                var table = connection.Table<AccessPoint>();
                colums = findNumberRecords();
                return table;
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        } //getAccessPoints()

        public TableQuery<ScanData> getScanData() {
            try {
                return connection.Table<ScanData>();
            }
            catch (SQLiteException ex) {
                Console.WriteLine(ex);
                throw;
            }
        } //getScanData
    }
}