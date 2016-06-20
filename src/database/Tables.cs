using SQLite;

namespace GetWifi {
    public class RN_tb {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Room { get; set; }

        public override string ToString() {
            return string.Format("[RN_tb: ID={0},Room={1}]\n", ID, Room);
        }
    } //class RN_tb

    public class WifiState_tb {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string BSSID { get; set; }
        public string Capabilities { get; set; }
        public string Frequency { get; set; }
        public string Level { get; set; }
        public string SSID { get; set; }
        public int Timestamp { get; set; }

        public override string ToString() {
            return string.Format("[WifiState_tb: ID={0},BSSID={1},Capabilities={2},Frequency={3},Level={4},SSID={5},Timestamp={6}]\n",
                                                ID, BSSID, Capabilities, Frequency, Level, SSID, Timestamp);
        }
    } //class Tables
} //namespace GetWifi