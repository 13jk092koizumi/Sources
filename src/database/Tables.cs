using System;
using SQLite;

namespace GetWifi.src.database {
    public class AccessPoint {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Room { get; set; }
        public string SSID { get; set; }
        public string BSSID { get; set; }
        public int Level { get; set; }
        public int Dispersion { get; set; }
        public DateTime Date { get; set; }
        public int ScanCount { get; set; } = 1;
        public override string ToString() {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7}\n", ID, Room, SSID, BSSID, Level, Dispersion, Date, ScanCount);
        }
    }

    public class ScanData {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        [Indexed]
        public string BSSID { get; set; }
        public int Level { get; set; }
        public DateTime Date { get; set; }
        public override string ToString() {
            return string.Format("{0},{1},{2},{3}", ID, BSSID, Level, Date);
        }
    }

} //namespace GetWifi