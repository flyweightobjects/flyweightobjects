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
using System.ComponentModel;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents the generic EventArgs used when a DML (Data Manipulation Language) operation is performed.
	/// </summary>
	public class DataOperationEventArgs<T> : DataOperationEventArgs where T : class, IFlyweight, new()
	{
		/// <summary>
		/// Gets the source object used in the operation.
		/// </summary>
		public new T Source { get; set; }

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="transactionType">The type of transaction.</param>
		/// <param name="source">The source object used in the operation.</param>
		public DataOperationEventArgs(TransactionType transactionType, T source)
			: base(transactionType, source)
		{
			this.Source = source;
		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="transactionType">The type of transaction.</param>
		/// <param name="command">The <see cref="IStorageCommand"/> provided for the operation.</param>
		public DataOperationEventArgs(TransactionType transactionType, IStorageCommand command)
			: base(transactionType, command)
		{

		}
	}
	
	/// <summary>
    /// Represents the EventArgs used when a DML (Data Manipulation Language) operation is performed.
    /// </summary>
    [Serializable]
    public class DataOperationEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Gets the <see cref="TransactionType"/> specified for operation.
        /// </summary>
        public TransactionType TransactionType { get; private set; }
        
        /// <summary>
        /// Gets the source object used in the operation.
        /// </summary>
        public object Source { get; private set; }
        
        /// <summary>
        /// Gets the <see cref="IStorageCommand"/> used in the operation.
        /// </summary>
        public IStorageCommand Command { get; private set; }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="transactionType">The type of transaction.</param>
        /// <param name="source">The source object used in the operation.</param>
        public DataOperationEventArgs(TransactionType transactionType, object source)
        {
            this.TransactionType = transactionType;
            this.Source = source;
        }

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="transactionType">The type of transaction.</param>
		/// <param name="command">The <see cref="IStorageCommand"/> provided for the operation.</param>
		public DataOperationEventArgs(TransactionType transactionType, IStorageCommand command)
		{
			this.TransactionType = transactionType;
			this.Command = command;
		}
    }

    /// <summary>
	/// Represents an event argument associated with caching.
	/// </summary>
    [Serializable]
	public class CachedItemEventArgs : EventArgs
	{
		/// <summary>
		/// Gets or sets the CacheItem object.
		/// </summary>
		public CacheItem Item { get; set; }
		
		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="item">The CacheItem object which has been cached.</param>
		public CachedItemEventArgs(CacheItem item)
		{
			this.Item = item;
		}
	}

    /// <summary>
    /// Represents the base class event arguments for when an item is changed.
    /// </summary>
    /// <typeparam name="T">The type which has been changed.</typeparam>
    [Serializable]
    public abstract class ItemChangedEventArgs<T> : EventArgs
    {
        private T _item = default(T);

        /// <summary>
        /// Gets the item that has been changed.
        /// </summary>
        public T Item
        {
            get { return _item; }
        }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="item">The changed item.</param>
        public ItemChangedEventArgs(T item)
        {
            _item = item;
        }
    }

    /// <summary>
    /// Represents the event arguments for when an item is added.
    /// </summary>
    /// <typeparam name="T">The type which has been added.</typeparam>
    [Serializable]
    public class ItemAddedEventArgs<T> : ItemChangedEventArgs<T>
    {
        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="item">The added item.</param>
        public ItemAddedEventArgs(T item)
            : base(item)
        {

        }
    }

    /// <summary>
    /// Represents the event arguments for when an item is updated.
    /// </summary>
    /// <typeparam name="T">The type which has been updated.</typeparam>
    [Serializable]
    public class ItemUpdatedEventArgs<T> : ItemChangedEventArgs<T>
    {
        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="item">The updated item.</param>
        public ItemUpdatedEventArgs(T item)
            : base(item)
        {
            
        }
    }

    /// <summary>
    /// Represents the event arguments for when an item is removed.
    /// </summary>
    /// <typeparam name="T">The type which has been removed.</typeparam>
    [Serializable]
    public class ItemRemovedEventArgs<T> : ItemChangedEventArgs<T>
    {
        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="item">The removed item.</param>
        public ItemRemovedEventArgs(T item)
            : base(item)
        {
            
        }
    }

    /// <summary>
    /// Represents the event arguments for when all items are cleared.
    /// </summary>
    /// <typeparam name="T">The type of the items which have been cleared.</typeparam>
    [Serializable]
    public class ItemsClearedEventArgs<T> : CancelEventArgs
    {
        /// <summary>
        /// The collection of cleared items.
        /// </summary>
        public IEnumerable<T> Items { get; set; }
        
        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="items">The cleared items.</param>
        public ItemsClearedEventArgs(IEnumerable<T> items)
        {
            this.Items = items;
        }
    }

    /// <summary>
    /// Represents event arguments pertaining to <see cref="DataTransferObject"/> related requests.
    /// </summary>
    public class DataTransferEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Gets the <see cref="DataTransferObject"/> associated with the requests.
        /// </summary>
        public DataTransferObject DataTransferObject { get; private set; }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="dto">The <see cref="DataTransferObject"/> which is being processed.</param>
        public DataTransferEventArgs(DataTransferObject dto)
        {
            this.DataTransferObject = dto;
        }
    }
}
