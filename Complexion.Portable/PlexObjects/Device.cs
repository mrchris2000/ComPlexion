using System.Collections.Generic;
using System.Linq;

namespace Complexion.Portable.PlexObjects
{
    public class Device : PlexObjectBase<Device>
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
        public long createdAt { get; set; }
        public long lastSeenAt { get; set; }
        public string screenResolution { get; set; }
        public string screenDensity { get; set; }

        public List<Connection> Connections { get; set; }

        public override string ToString()
        {
            return name;
        }

        protected override bool OnUpdateFrom(Device newDevice)
        {
            var isUpdated = UpdateValue(() => createdAt, newDevice);
            isUpdated = UpdateValue(() => device, newDevice) | isUpdated;
            isUpdated = UpdateValue(() => id, newDevice) | isUpdated;
            isUpdated = UpdateValue(() => lastSeenAt, newDevice) | isUpdated;
            isUpdated = UpdateValue(() => name, newDevice) | isUpdated;
            isUpdated = UpdateValue(() => platform, newDevice) | isUpdated;
            isUpdated = UpdateValue(() => platformVersion, newDevice) | isUpdated;
            isUpdated = UpdateValue(() => productVersion, newDevice) | isUpdated;
            isUpdated = UpdateValue(() => provides, newDevice) | isUpdated;
            isUpdated = UpdateValue(() => publicAddress, newDevice) | isUpdated;
            isUpdated = UpdateValue(() => screenDensity, newDevice) | isUpdated;
            isUpdated = UpdateValue(() => screenResolution, newDevice) | isUpdated;
            isUpdated = UpdateValue(() => token, newDevice) | isUpdated;
            isUpdated = UpdateValue(() => vendor, newDevice) | isUpdated;
            isUpdated = UpdateValue(() => version, newDevice) | isUpdated;

            foreach (var connection in Connections.ToList().Where(con => newDevice.Connections.All(c => c.uri != con.uri)))
            {
                Connections.Remove(connection);
                isUpdated = true;
            }

            foreach (var connection in newDevice.Connections.Where(con => Connections.All(c => c.uri != con.uri)))
            {
                Connections.Add(connection);
                isUpdated = true;
            }
            
            return isUpdated;
        }

        public override string Key
        {
            get { return clientIdentifier; }
        }
    }
}