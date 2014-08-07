using System;
using System.Collections.Generic;
using System.Linq;

namespace Complexion.Portable
{
    internal static class ListExtensions
    {
        public static  bool UpdateToMatch<T, TKey>(this IList<T> list, ICollection<T> collection, Func<T, TKey> keyFunc, Func<T, T, bool> updateAction = null)
        {
            var updated = false;

            var newKeys = new HashSet<TKey>(collection.Select(keyFunc));
            var oldKeys = new HashSet<TKey>(list.Select(keyFunc));

            var toDelete = new HashSet<TKey>(oldKeys);
            toDelete.ExceptWith(newKeys);

            var toAdd = new HashSet<TKey>(newKeys);
            toAdd.ExceptWith(oldKeys);

            foreach (var item in list.Where(i => toDelete.Contains(keyFunc(i))).ToList())
            {
                list.Remove(item);
                updated = true;
            }

            foreach (var item in collection.Where(i => toAdd.Contains(keyFunc(i))).ToList())
            {
                list.Add(item);
                updated = true;
            }

            if (updateAction == null)
                return updated;

            var toUpdate = new HashSet<TKey>(oldKeys);
            toUpdate.IntersectWith(newKeys);

            foreach (var key in toUpdate)
            {
                var oldItem = list.FirstOrDefault(i => Equals(keyFunc(i), key));
                var newItem = collection.FirstOrDefault(i => Equals(keyFunc(i), key));

                if (!Equals(oldItem, default(T)) && !Equals(newItem, default(T)))
                    updated = updateAction(oldItem, newItem) | updated;
            }

            return updated;
        }

        public static bool UpdateToMatch<T, TKey>(this IList<T> list, T item, Func<T, TKey> keyFunc, Func<T, T, bool> updateAction = null)
        {
            return UpdateToMatch(list, new List<T> { item }, keyFunc, updateAction);
        }
    }
}
