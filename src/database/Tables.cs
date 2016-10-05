using SQLite;

namespace GetWifi.src.database {
    public class AccessPoint {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Room { get; set; }
        public string SSID { get; set; }
        public string BSSID { get; set; }
        //public string Capabilities { get; set; }
        //public int Frequency { get; set; }
        public int Level { get; set; }
        public int Dispersion { get; set; }

        public override string ToString() {
            return string.Format("\n[{0}: Room={1},SSID={2},BSSID={3},Level={4},Dispersion={5}]",ID,Room,SSID,BSSID,Level,Dispersion);
        }
    }

    public class ScanData {
        [PrimaryKey,AutoIncrement]
        public int ID { get; set; }
        [Indexed]
        public string BSSID { get; set; }
        public int Level { get; set; }

        public override string ToString() {
            return string.Format("\n[{0}: BSSID={1},Level={2}]", ID, BSSID, Level);
        }
    }

} //namespace GetWifi