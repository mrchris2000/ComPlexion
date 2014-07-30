namespace Complexion.Portable.PlexObjects
{
    public class Connection
    {
        public string uri { get; set; }

        public override string ToString()
        {
            return uri;
        }
    }
}
