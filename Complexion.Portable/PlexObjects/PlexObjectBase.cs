using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Complexion.Portable.PlexObjects
{
    public abstract class PlexObjectBase<T>
    {
        public event EventHandler Updated;

        protected virtual void OnUpdated()
        {
            var handler = Updated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        protected abstract bool OnUpdateFrom(T newValue);

        public bool UpdateFrom(T newValue)
        {
            var updated = OnUpdateFrom(newValue);

            if (updated) OnUpdated();

            return updated;
        }

        public abstract string Key { get; }

        protected bool UpdateValue<TValue>(Expression<Func<TValue>> propertyExpression, T newValue)
        {
            var memberExpression = (MemberExpression)propertyExpression.Body;
            var propertyInfo = (PropertyInfo)memberExpression.Member;

            var value = propertyInfo.GetValue(newValue);

            if (propertyInfo.GetValue(this).Equals(value))
                return false;

            propertyInfo.SetValue(this, value);
            return true;
        }
    }
}
