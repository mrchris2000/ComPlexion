namespace Complexion.Portable.PlexObjects
{
    public abstract class IdTagObjectBase<T> : PlexObjectBase<IdTagObjectBase<T>>
    {
        public long id { get; set; }
        public string tag { get; set; }

        protected override bool OnUpdateFrom(IdTagObjectBase<T> newValue)
        {
            return UpdateValue(() => tag, newValue);
        }

        public override string Key
        {
            get { return id.ToString(); }
        }
    }
}
