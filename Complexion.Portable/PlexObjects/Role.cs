namespace Complexion.Portable.PlexObjects
{
    public class Role : IdTagObjectBase<Role>
    {
        public string role { get; set; }
        public string thumb { get; set; }

        protected override bool OnUpdateFrom(IdTagObjectBase<Role> newValue)
        {
            var isUpdated = base.OnUpdateFrom(newValue);
            isUpdated = UpdateValue(() => role, newValue) | isUpdated;
            isUpdated = UpdateValue(() => thumb, newValue) | isUpdated;

            return isUpdated;
        }

        public override string Key
        {
            get { return id.ToString(); }
        }
    }
}
