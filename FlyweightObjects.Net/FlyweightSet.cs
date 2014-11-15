//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
//  Author:         Marcus Crane, Software Developer / Architect                                        //
//  E-mail:         mailto:support@FlyweightObjects.NET                                                 //
//  Company:        FlyweightObjects.NET                                                                //
//  Copyright:      Copyright © FlyweightObjects.NET 2011, All rights reserved.                         //
//  Date Created:   06/04/2008                                                                          //
//                                                                                                      //
//  Disclaimer:                                                                                         //
//  ===========                                                                                         //
//  This code file is provided "as is" with no expressed or implied warranty. The author accepts no     //
//  liability for any damage or loss that the code file may cause as a result of its use. Any           //
//  modification, copying, or reverse engineering of this code file, or the underlying architectural    //
//  foundation it supports, is strictly prohibited without the express written consent of the author.   //
//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents a lightweight set of objects optimized for high speed access.
    /// </summary>
    /// <typeparam name="T">The type parameter of the domain type.</typeparam>
    [Serializable]
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(DebugViewerCollection<>))]
    public class FlyweightSet<T> : IFlyweightSet<T>, IChangeTrackable<T>, IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable, IResponseMessage where T : class, IFlyweight, new()
    {
        /// <summary>
        /// Represents the method that will handle the ItemAdded event of the <see cref="IFlyweightSet{T}"/> class.
        /// </summary>
        [field:NonSerialized]
        public event EventHandler<ItemAddedEventArgs<T>> ItemAdded;

        /// <summary>
        /// Represents the method that will handle the ItemRemoved event of the <see cref="IFlyweightSet{T}"/> class.
        /// </summary>
        [field: NonSerialized]
        public event EventHandler<ItemRemovedEventArgs<T>> ItemRemoved;

        /// <summary>
        /// Represents the method that will handle the ItemsCleared event of the <see cref="IFlyweightSet{T}"/> class.
        /// </summary>
        [field: NonSerialized]
        public event EventHandler<ItemsClearedEventArgs<T>> ItemsCleared;

		/// <summary>
		/// Represents the data source for the collection.
		/// </summary>
        protected internal ResultSet DataSource { get; set; }
        
		/// <summary>
		/// Represents a clone of the original data source for the collection.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected internal ResultSet DataSourceClone { get; set; }

		/// <summary>
		/// Represents the <see cref="IFlyweight"/> instance for the collection.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected T Flyweight { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ResponseMessage"/> for the request.
        /// </summary>
        public ResponseMessage Response { get; set; }

        /// <summary>
        /// Gets the <see cref="IFlyweightSet{T}"/>.DataSource for the collection.
        /// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ResultSet IFlyweightSet<T>.DataSource
        {
            get { return this.DataSource; }
        }

        /// <summary>
        /// Gets the object acting as the Flyweight.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        T IFlyweightSet<T>.Flyweight
        {
            get { return this.Flyweight; }
        }

        /// <summary>
        /// Gets or sets the <see cref="IChangeTrackable{T}"/>.DeletedItems dictionary.
        /// </summary>
        ResultSet.RowCollection IChangeTrackable<T>.DeletedItems
        {
            get { return this.DataSource.DeletedRows; }
        }

		/// <summary>
		/// Gets or sets a boolean value indicating whether the collection will enable the rejection of changes. The
		/// default for this property is false.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public bool EnableRejectChanges { get; set; }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        public FlyweightSet()
        {
			this.Flyweight = new T();
			this.Response = new ResponseMessage();
            this.DataSource = new ResultSet(typeof(T));
        }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        protected internal FlyweightSet(ResultSet resultSet)
        {
			this.Flyweight = new T();
			this.Response = new ResponseMessage();
            if (this.DataSource == null)
            {
                this.DataSource = resultSet;
            }
            else
            {
                this.DataSource.Merge(resultSet);
            }
            if (DataSource.Rows.Count > 0)
            {
                ((IPropertyStorage)this.Flyweight.Storage).DataSource = this.DataSource.Rows[0];
            }
        }

        /// <summary>
        /// Returns the collections back to its original state before any modifications were made to it. Note that 
		/// <see cref="EnableRejectChanges "/> must be set to true prior to calling this method.
        /// </summary>
        public void RejectChanges()
        {
            if (this.DataSourceClone != null)
            {
                try
                {
                    this.DataSource.SuspendEvents();
                    this.DataSource = this.DataSourceClone.Copy();
                    this.DataSource.DeletedRows.Clear();
                }
                finally
                {
                    this.DataSource.ResumeEvents();
                    this.DataSourceClone = null;
                }
            }
        }

        /// <summary>
        /// Creates a deep copy of the collection and all of its contents.
        /// </summary>
        /// <returns></returns>
        public FlyweightSet<T> Clone()
        {
            return ObjectCloner.Clone<FlyweightSet<T>>(this) as FlyweightSet<T>;
        }

        /// <summary>
        /// Determines the index of a specific item in the collection.
        /// </summary>
        /// <param name="item">The object to locate in the collection.</param>
        /// <returns></returns>
        public int IndexOf(T item)
        {
            ResultSet.Row row = ((IPropertyStorage)item.Storage).DataSource;
            return this.DataSource.Rows.IndexOf(row);
        }

        /// <summary>
        /// Inserts an item to the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted. </param>
        /// <param name="item">The object to insert into the collection.</param>
        public void Insert(int index, T item)
        {
            if (index < 0 || index >= this.Count)
            {
                throw new IndexOutOfRangeException();
            }
            this.DataSource.MergeColumns(((IPropertyStorage)item.Storage).DataSource.ResultSet.Columns);
            this.DataSource.Rows.InsertAt(((IPropertyStorage)item.Storage).DataSource, index);
        }

        /// <summary>
        /// Removes the object item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove. </param>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= this.Count)
            {
                throw new IndexOutOfRangeException();
            }
            T item = this[index];
            this.RemoveItemCore(item);
        }

        /// <summary>
        /// Returns a new instance object for the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        public T InstanceAt(int index)
        {
            if (index < 0 || index >= this.Count)
            {
                throw new IndexOutOfRangeException();
            }
            ConstructorInfo ctor = typeof(T).GetConstructor(new Type[] { typeof(PropertyStorage) });
            T item = null;
            if (ctor != null)
            {
                PropertyStorage storage = new PropertyStorage(typeof(T), this.DataSource.Rows[index]);
                item = (T)ctor.Invoke(new object[] { storage });
            }
            else
            {
                item = new T();
                ((IPropertyStorage)item.Storage).DataSource = this.DataSource.Rows[index];
            }
            return item;
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= this.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                ((IPropertyStorage)this.Flyweight.Storage).DataSource = this.DataSource.Rows[index];
                return this.Flyweight;
            }
            set
            {
                if (index < 0 || index >= this.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                T item = this[index];
                ((IPropertyStorage)item.Storage).DataSource.ItemArray = ((IPropertyStorage)value.Storage).DataSource.ItemArray;
                ((IPropertyStorage)value.Storage).DataSource = ((IPropertyStorage)item.Storage).DataSource;
            }
        }

        /// <summary>
        /// Gets the element at the specified index. If instance is true, returns a new instance for the specified index, otherwise returns 
        /// the internally held Flyweight.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <param name="instance">Determines whether the indexer should return a new instance or the internally held Flyweight.</param>
        public T this[int index, bool instance]
        {
            get
            {
                if (index < 0 || index >= this.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                if (instance)
                {
                    return InstanceAt(index);
                }
                else
                {
                    return this[index];
                }
            }
        }

        /// <summary>
        /// Returns an <see cref="IList{T}"/> representation of a limited number items in the current <see cref="FlyweightSet{T}"/> using the current value of
        /// the <see cref="DataContext.MaxDebugListSize"/> property. Note that this property will only be executed if <see cref="Debugger.IsAttached"/> is equal 
        /// to true and should only be used for debugging purposes.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
		IList<T> IFlyweightSet<T>.DebugList
        {
            get
            {
                List<T> list = new List<T>();
                if (Debugger.IsAttached)
                {
                    int size = ThreadLocalStorage.GetMaxDebugListSize();
                    for (int i = 0; i < this.Count && i < size; i++)
                    {
                        T item = InstanceAt(i);
                        list.Add(item);
                    }
                }
                return list;
            }
        }

        /// <summary>
        /// Adda a new object to the collection.
        /// </summary>
		/// <param name="item">The object to add to the collection.</param>
        public void Add(T item)
        {
            this.AddItemCore(item);
        }

        /// <summary>
        /// Clears the collection of all its contents and queues all removed items for deletion.
        /// </summary>
        public void Clear()
        {
            ItemsClearedEventArgs<T> e = new ItemsClearedEventArgs<T>(this);
            CloneDataSource();
            OnItemsCleared(e);
            if (!e.Cancel)
            {
                this.DataSource.Rows.Clear();
            }
        }

        /// <summary>
		/// Determines if the object is already in the collection.
        /// </summary>
        /// <param name="item">The object for which to search.</param>
        public bool Contains(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException();
            }
            return this.IndexOf(item) > -1;
        }

        /// <summary>
		/// Copies the contents of the collection to the supplied array.
        /// </summary>
		/// <param name="array">The target array of the collection.</param>
        /// <param name="arrayIndex">The index of the array to start the copy.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            for (int i = arrayIndex; i < this.Count; i++)
            {
                T item = InstanceAt(i);
                array.SetValue(item, i);
            }
        }

        /// <summary>
		/// Gets the count of objects in the collection.
        /// </summary>
        public int Count
        {
            get { return this.DataSource.Rows.Count; }
        }

        /// <summary>
		/// Returns a boolean indicating whether the collection is read only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return this.DataSource.Rows.IsReadOnly; }
        }

        /// <summary>
        /// Returns a boolean indicating whether any of the contained objects have changed or been removed.
        /// </summary>
        public bool HasChanges
        {
            get 
            {
                return (this.DataSource.DeletedRows.Count > 0) || (this.DataSource.Rows.Where(p => p.IsChanged).Any());
            }
        }

        /// <summary>
		/// Removes the supplied object from the collection.
        /// </summary>
        /// <param name="item">The object to be removed.</param>
        public bool Remove(T item)
        {
            return this.RemoveItemCore(item);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, returns the first occurrence and
        /// removes it from the collection.
        /// </summary>
        /// <param name="match">The delegate that defines the conditions of the elements to search for.</param>
        public T Remove(Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            for (int i = this.Count - 1; i >= 0; i--)
            {
                if (match(this[i]))
                {
                    T source = this.InstanceAt(i);
                    this.RemoveAt(i);
                    return source;
                }
            }
            return default(T);
        }

        /// <summary>
        /// Retrieves the all the elements that match the conditions defined by the specified predicate and then removes them from
        /// the collection.
        /// </summary>
        /// <param name="match">The delegate that defines the conditions of the elements to search for.</param>
        public FlyweightSet<T> RemoveAll(Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            ResultSet resultSet = new ResultSet(this.DataSource.Columns);
            FlyweightSet<T> collection = new FlyweightSet<T>(resultSet);
            for (int i = this.Count - 1; i >= 0; i--)
            {
                if (match(this[i]))
                {
                    collection.Add(this[i]);
                    this.RemoveAt(i);
                }
            }
            return collection;
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate and returns the first occurrence.
        /// </summary>
        /// <param name="match">The delegate that defines the conditions of the elements to search for.</param>
        public T Find(Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            for (int i = 0; i < this.Count; i++)
            {
                if (match(this[i]))
                {
                    return this[i];
                }
            }
            return default(T);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate and returns a bool indicating
        /// whether or not the obejct exists.
        /// </summary>
        /// <param name="match">The delegate that defines the conditions of the elements to search for.</param>
        public bool Exists(Predicate<T> match)
        {
            return this.Find(match) != null;
        }

        /// <summary>
        /// Retrieves the all the elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="match">The delegate that defines the conditions of the elements to search for.</param>
        public FlyweightSet<T> FindAll(Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            FlyweightSet<T> collection = new FlyweightSet<T>();
            for (int i = 0; i < this.Count; i++)
            {
                if (match(this[i]))
                {
                    collection.Add(this[i]);
                }
            }
            return collection;
        }

        /// <summary>
        /// Returns a new <see cref="FlyweightSet{T}"/> containing only the new, modified and removed items
        /// found in the source collection.
        /// </summary>
        public FlyweightSet<T> GetChangeSet()
        {
            FlyweightSet<T> retVal = new FlyweightSet<T>(new ResultSet(this.DataSource.Columns));
            foreach (ResultSet.Row row in ((IChangeTrackable<T>)this).DeletedItems)
            {
                retVal.DataSource.DeletedRows.Add(row);
            }
            foreach (T item in this)
            {
                if (item.IsChanged())
                {
                    retVal.Add(item);
                }
            }
            return retVal;
        }
        
        /// <summary>
		/// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (ResultSet.Row row in this.DataSource.Rows)
            {
                if (row.RowState != RowState.Detached && row.RowState != RowState.Deleted)
                {
                    ((IPropertyStorage)this.Flyweight.Storage).DataSource = row;
                    yield return this.Flyweight;
                }
            }
        }

        /// <summary>
		/// Returns an enumerator that iterates through the collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Raises the ItemAdded event.
        /// </summary>
        /// <param name="item">The added object.</param>
        protected void OnItemAdded(T item)
        {
            if (this.ItemAdded != null)
            {
                ItemAdded(this, new ItemAddedEventArgs<T>(item));
            }
        }

        /// <summary>
        /// Raises the ItemAdded event.
        /// </summary>
        /// <param name="item">The resmoved item.</param>
        protected void OnItemRemoved(T item)
        {
            if (this.ItemRemoved != null)
            {
                ItemRemoved(this, new ItemRemovedEventArgs<T>(item));
            }
        }

        /// <summary>
        /// Raises the ItemsCleared event.
        /// </summary>
        protected void OnItemsCleared(ItemsClearedEventArgs<T> args)
        {
            if (this.ItemsCleared != null)
            {
                ItemsCleared(this, args);
            }
        }

        private void CloneDataSource()
        {
            if (this.EnableRejectChanges && this.DataSourceClone == null)
            {
                this.DataSourceClone = this.DataSource.Copy();
            }
        }

        private int AddItemCore(T item)
        {
			CloneDataSource();
            this.DataSource.Merge(((IPropertyStorage)item.Storage).DataSource);
            OnItemAdded(item);
            int index = this.DataSource.Rows.Count - 1;
            return index;
        }

        private bool RemoveItemCore(T item)
        {
            bool retVal = false;
            int index = this.DataSource.Rows.IndexOf(((IPropertyStorage)item.Storage).DataSource);
            if (index > -1)
            {
                CloneDataSource();
                OnItemRemoved(item);
                this.DataSource.Rows.RemoveAt(index);
                retVal = true;
            }
            return retVal;
        }

        int IList.Add(object value)
        {
            if (value.GetType() != typeof(T))
            {
                throw new ArgumentException(string.Format("The argument value is not of type {0}", typeof(T).FullName));
            }
            return this.AddItemCore((T)value);
        }

        bool IList.Contains(object value)
        {
            if (value.GetType() != typeof(T))
            {
                throw new ArgumentException(string.Format("The argument value is not of type {0}", typeof(T).FullName));
            }
            return Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            if (value.GetType() != typeof(T))
            {
                throw new ArgumentException(string.Format("The argument value is not of type {0}", typeof(T).FullName));
            }
            return this.IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool IList.IsFixedSize
        {
            get { return false; }
        }

        void IList.Remove(object value)
        {
            if (value.GetType() != typeof(T))
            {
                throw new ArgumentException(string.Format("The argument value is not of type {0}", typeof(T).FullName));
            }
            this.RemoveItemCore((T)value);
        }

        object IList.this[int index]
        {
            get
            {
                return this[index] as T;
            }
            set
            {
                if (value.GetType() != typeof(T))
                {
                    throw new ArgumentException(string.Format("The argument value is not of type {0}", typeof(T).FullName));
                }
                this[index] = (T)value;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.CopyTo((T[])array, index);
        }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot
        {
            get { return this.DataSource.Rows.SyncRoot; }
        }

		/// <summary>
		/// Performs the specified action on each element in the <see cref="FlyweightSet{T}"/>.
		/// </summary>
		/// <param name="action">The <see cref="Action{T}"/> delegate to perform on each element.</param>
		protected internal void ForEach(Action<T> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException("action");
			}
			for (int i = 0; i < this.Count; i++)
			{
				action(this[i]);
			}
		}

		internal class DebugViewerCollection<T1> where T1 : class, IFlyweight, new()
		{
			private readonly IList<T1> _collection;

			public DebugViewerCollection(IList<T1> value)
			{
				_collection = value;
			}

			[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
			public IList<T1> Collection
			{
				get { return _collection; }
			}
		}
    }
}