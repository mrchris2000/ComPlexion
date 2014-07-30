using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Complexion.Portable
{
    internal class ObservableCollectionEx<T>: ObservableCollection<T>
    {
        /// <summary> 
        /// Adds the elements of the specified collection to the end of the ObservableCollection(Of T). 
        /// A collection changed notification is raised at the end of the add, not after each item.
        /// </summary> 
        internal void AddRange(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");

            var enumerable = collection as IList<T> ?? collection.ToList();

            foreach (var i in enumerable)
                Items.Add(i);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary> 
        /// Clears then adds the elements of the specified collection to the end of the ObservableCollection(Of T).
        /// A collection change notification is raised at the end of the clear and add, not at each step. 
        /// </summary> 
        internal void ClearAndAddRange(IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");

            var enumerable = collection as IList<T> ?? collection.ToList();

            Items.Clear();

            foreach (var i in enumerable)
                Items.Add(i);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
