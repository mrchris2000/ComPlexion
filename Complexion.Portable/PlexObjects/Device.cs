using System.Collections.Generic;

namespace Complexion.Portable.PlexObjects
{
    public class Device
    {
        public string name { get; set; }
        public string publicAddress { get; set; }
        public string product { get; set; }
        public string productVersion { get; set; }
        public string platform { get; set; }
        public string platformVersion { get; set; }
        public string device { get; set; }
        public string model { get; set; }
        public string vendor { get; set; }
        public string provides { get; set; }
        public string clientIdentifier { get; set; }
        public string version { get; set; }
        public string id { get; set; }
        public string token { get; set; }
        public string createdAt { get; set; }
        public string lastSeenAt { get; set; }
        public string screenResolution { get; set; }
        public string screenDensity { get; set; }

        public List<Connection> Connections { get; set; }

        public override string ToString()
        {
            return name;
        }
    }
}