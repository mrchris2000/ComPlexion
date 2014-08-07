namespace Complexion.Portable.PlexObjects
{
    public class Server : PlexObjectBase<Server>
    {
        public string name { get; set; }
        public string host { get; set; }
        public string address { get; set; }
        public int port { get; set; }
        public string machineIdentifier { get; set; }

        protected override bool OnUpdateFrom(Server newValue)
        {
            var isUpdated = UpdateValue(() => name, newValue);
            isUpdated = UpdateValue(() => host, newValue) | isUpdated;
            isUpdated = UpdateValue(() => address, newValue) | isUpdated;
            isUpdated = UpdateValue(() => port, newValue) | isUpdated;

            return isUpdated;
        }

        public override string Key
        {
            get { return machineIdentifier; }
        }
    }
}
