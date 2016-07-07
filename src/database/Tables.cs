using SQLite;

namespace GetWifi.src.database {
    public class Wifi_tb {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Room { get; set; }
        public string SSID { get; set; }
        public string BSSID { get; set; }
        public string Capabilities { get; set; }
        public int Frequency { get; set; }
        public int Level { get; set; }

        public override string ToString() {
            return string.Format("\n[{0}: Room={1},SSID={2},BSSID={3},Capabilities={4},Frequency={5},Level={6}]",ID,Room,SSID,BSSID,Capabilities,Frequency,Level);
        }
    }

} //namespace GetWifi