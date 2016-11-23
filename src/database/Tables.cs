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
        public int Variation { get; set; }
        public DateTime Date { get; set; }
        public int ScanCount { get; set; } = 1;
        public override string ToString() {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", ID, Room, SSID, BSSID, Level, Variation, Date, ScanCount);
        }
    }

    public class ScanDateLog {
        [PrimaryKey,AutoIncrement]
        public int ID { get; set; }
        public string Room { get; set; }
        public DateTime Date { get; set; }
        public override string ToString() {
            return string.Format("{0},{1},{2}", ID, Room, Date);
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

    public class BSSIDIndex {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; } = 1;
        [Unique]
        public string BSSID { get; set; }
        public override string ToString() {
            return string.Format("{0},{1}", ID, BSSID);
        }
    }

    public class RoomIndex {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; } = 1;
        [Unique]
        public string Room { get; set; }
        public override string ToString() {
            return string.Format("{0},{1}", ID, Room);
        }
    }

} //namespace GetWifi