namespace Complexion.Portable.PlexObjects
{
    public class Player : PlexObjectBase<Player>
    {
        public string machineIdentifier { get; set; } 
        public string platform { get; set; } 
        public string product { get; set; } 
        public string state { get; set; } 
        public string title { get; set; }

        public Server Client { get; internal set; }

        protected override bool OnUpdateFrom(Player newValue)
        {
            var isUpdated = UpdateValue(() => title, newValue);
            isUpdated = UpdateValue(() => platform, newValue) | isUpdated;
            isUpdated = UpdateValue(() => product, newValue) | isUpdated;
            isUpdated = UpdateValue(() => state, newValue) | isUpdated;
            isUpdated = UpdateValue(() => title, newValue) | isUpdated;

            return isUpdated;
        }

        public override string Key
        {
            get { return machineIdentifier; }
        }
    }
}
