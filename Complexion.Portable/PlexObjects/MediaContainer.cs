using System.Collections.Generic;

namespace Complexion.Portable.PlexObjects
{
    public class MediaContainer
    {
        public string publicAddress { get; set; }
        public string friendlyName { get; set; }
        public string platform { get; set; }
        public string machineIdentifier { get; set; }

        public List<Device> Devices { get; set; }

        public List<Video> Videos { get; set; }

        public List<Server> Servers { get; set; }

        public override string ToString()
        {
            return publicAddress;
        }
    }
}
