namespace Complexion.Portable.PlexObjects
{
    public class Role : PlexObjectBase<Role>
    {
        public long id { get; set; }
        public string role { get; set; }
        public string tag { get; set; }
        public string thumb { get; set; }

        protected override bool OnUpdateFrom(Role newValue)
        {
            var isUpdated = UpdateValue(() => role, newValue);
            isUpdated = UpdateValue(() => tag, newValue) | isUpdated;
            isUpdated = UpdateValue(() => thumb, newValue) | isUpdated;

            return isUpdated;
        }

        public override string Key
        {
            get { return id.ToString(); }
        }
    }
}
