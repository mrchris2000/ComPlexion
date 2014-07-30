namespace Complexion.Portable.PlexObjects
{
    public class PlexUser
    {
        public string username { get; set; }

        public string email { get; set; }

        public int id { get; set; }

        public string thumb { get; set; }

        public string queueEmail { get; set; }

        public string queueUid { get; set; }

        public string cloudSyncDevice { get; set; }

        public string authenticationToken { get; set; }
    }
}