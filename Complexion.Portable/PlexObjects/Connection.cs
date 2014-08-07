namespace Complexion.Portable.PlexObjects
{
    public class Connection : PlexObjectBase<Connection>
    {
        public string uri { get; set; }

        public override string ToString()
        {
            return uri;
        }

        protected override bool OnUpdateFrom(Connection newValue)
        {
            return false;
        }

        public override string Key
        {
            get { return uri; }
        }
    }
}
