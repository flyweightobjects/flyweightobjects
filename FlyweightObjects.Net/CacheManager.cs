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
using System.Timers;
using System.Reflection;
using System.Collections;

namespace FlyweightObjects
{
    /// <summary>
    /// Provides the default caching functionality to be used by the <see cref="DataContext"/>. Note that the domain model must be adorned with a <see cref="DataTableAttribute"/> 
    /// and the attribute itself must have <see cref="DataTableAttribute.EnableCaching"/> set to true.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
	/// [DataTable("Production.Product", EnableCaching=true, CacheTimeout=300)]
    /// public class MyProduct : IRedundancyCheck
    /// {
    ///     [DataColumn("ProductID", Identifier = true)]
    ///     public int ProductID { get; set; }
	///     
    ///     [DataColumn("ModifiedDate")]
    ///     public DateTime ModifiedDate { get; set; }
	///     
	///		[PropertyTorage]
    ///     public string Checksum { get; set; }
    ///     
	///		[Computed]
    ///     public bool IsChanged
    ///     {
    ///         get { return new ChecksumBuilder().BuildChecksum(this) != this.Checksum; }
    ///     }
    /// }
    /// 
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
    ///         {
    ///             context.CacheManager.Enabled = true;
    ///             MyProduct p1 = context.Select<MyProduct>(1).ToSingle();
    ///             Console.Writeline("Elapsed Time: {0}", context.ElapsedMilliseconds);
    ///             MyProduct p2 = context.Select<MyProduct>(1).ToSingle();
    ///             Console.Writeline("Elapsed Time: {0}", context.ElapsedMilliseconds);
    ///         }     
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public class CacheManager : ICacheManager
    {
        /// <summary>
        /// Represents an event that is fired when an item is added to the cache.
        /// </summary>
        public event EventHandler<CachedItemEventArgs> ItemAdded;

        /// <summary>
        /// Represents an event that is fired when an item is removed from the cache.
        /// </summary>
        public event EventHandler<CachedItemEventArgs> ItemRemoved;
        
        private static Dictionary<string, CacheItem> _cache = new Dictionary<string, CacheItem>();
        private readonly object _syncLock = new object();
        private bool _enabled = false;

        /// <summary>
        /// Returns a string representing a key which may used to uniquely identify a command.
        /// </summary>
        /// <typeparam name="T">The type for which the cache key should be built.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> used as the command.</param>
        public string BuildCacheKey<T>(IStorageCommand command) where T : class, IFlyweight, new()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BinarySerializer.Serialize(typeof(T)));
            bytes.AddRange(BinarySerializer.Serialize(command.CommandType));
            bytes.AddRange(BinarySerializer.Serialize(command.SqlText));
            bytes.AddRange(BinarySerializer.Serialize(command.Parameters));
            return bytes.ToArray().ToMD5();
        }

        /// <summary>
        /// Returns a string representing a key which may used to uniquely identify a command.
        /// </summary>
        /// <typeparam name="T">The type for which the cache key should be built.</typeparam>
        /// <param name="depth">The depth by which to interrogate the object graph.</param>
        /// <param name="command">The <see cref="IStorageCommand"/> used as the command.</param>
        public string BuildCacheKey<T>(PreloadDepth depth, IStorageCommand command) where T : class, IFlyweight, new()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BinarySerializer.Serialize(typeof(T)));
            bytes.AddRange(BinarySerializer.Serialize(depth));
            bytes.AddRange(BinarySerializer.Serialize(command.CommandType));
            bytes.AddRange(BinarySerializer.Serialize(command.SqlText));
            bytes.AddRange(BinarySerializer.Serialize(command.Parameters));
            return bytes.ToArray().ToMD5();
        }

        /// <summary>
        /// Returns a string representing a key which may used to uniquely identify a command, including the options associated to its retrieval.
        /// </summary>
        /// <typeparam name="T">The type for which the cache key should be built.</typeparam>
        /// <param name="options">A <see cref="PreloadOptions{T}"/> specifying additional related objects to retrieve.</param>
        /// <param name="command">The <see cref="IStorageCommand"/> used as the command.</param>
        public string BuildCacheKey<T>(PreloadOptions<T> options, IStorageCommand command) where T : class, IFlyweight, new()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BinarySerializer.Serialize(typeof(T)));
            bytes.AddRange(options == null ? new byte[0] : BinarySerializer.Serialize(options));
            bytes.AddRange(BinarySerializer.Serialize(command.CommandType));
            bytes.AddRange(BinarySerializer.Serialize(command.SqlText));
            bytes.AddRange(BinarySerializer.Serialize(command.Parameters));
            return bytes.ToArray().ToMD5();
        }

        /// <summary>
        /// Adds a new object to the cache. Be sure to call the Contains method to circumvent
        /// any key conflicts. This method will hold objects in memory which will self-expire
        /// provided that their time-to-live value is greater than zero.
        /// </summary>
        /// <param name="key">The key that uniquely defines the object.</param>
        /// <param name="timeToLive">The time in seconds that the object should live in the cache.</param>
        /// <param name="source">The object to cache.</param>
        public void AddObject<T>(string key, int timeToLive, IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            lock (_syncLock)
            {
                if (!Contains(key))
                {
                    CacheItem item = new CacheItem(key, DateTime.Now, timeToLive, source);
                    if (timeToLive > 0)
                    {
                        if (ItemAdded != null)
                        {
                            ItemAdded(this, new CachedItemEventArgs(item));
                        }
                        item.TimeElapsed += new EventHandler<CachedItemEventArgs>(item_TimeElapsed);
                    }
                    _cache.Add(key, item);
                }
            }
        }

        /// <summary>
        /// Returns the object from the cache.
        /// </summary>
        /// <param name="key">The key that uniquely defines the object.</param>
        public IEnumerable<T> GetObject<T>(string key) where T : class, IFlyweight, new()
        {
            lock (_syncLock)
            {
                return (IEnumerable<T>)_cache[key].Item;
            }
        }

        /// <summary>
        /// Returns a boolean indicating whether or not the method was successful at retrieving
        /// ths object from the cache.
        /// </summary>
        /// <param name="key">The key that uniquely defines the object.</param>
        /// <param name="result">The object to return if it exists in the cache.</param>
        public bool TryGetObject<T>(string key, out IEnumerable<T> result) where T : class, IFlyweight, new()
        {
            bool retVal = false;
            result = default(IEnumerable<T>);
            lock (_syncLock)
            {
                CacheItem cachedItem = null;
                retVal = _cache.TryGetValue(key, out cachedItem);
                if (retVal)
                {
                    result = (IEnumerable<T>)cachedItem.Item;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Clears the cache of all objects.
        /// </summary>
        public void Flush()
        {
            lock (_syncLock)
            {
                _cache.Clear();
            }
        }

        /// <summary>
        /// Removes the object from the cache.
        /// </summary>
        /// <param name="key">The key that uniquely defines the object.</param>
        public void RemoveObject(string key)
        {
            lock (_syncLock)
            {
                if (Contains(key))
                {
                    if (ItemRemoved != null)
                    {
                        ItemRemoved(this, new CachedItemEventArgs(_cache[key]));
                    }
                    _cache.Remove(key);
                }
            }
        }

        /// <summary>
        /// Returns a boolean indicating whether the object exists in the cache.
        /// </summary>
        /// <param name="key">The key that uniquely defines the object.</param>
        public bool Contains(string key)
        {
            lock (_syncLock)
            {
                return (_cache.ContainsKey(key));
            }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating whether the cache is enabled.
        /// </summary>
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                if (!_enabled)
                {
                    _cache.Clear();
                }
            }
        }

        private void item_TimeElapsed(object sender, CachedItemEventArgs e)
        {
            RemoveObject(e.Item.Key);
        }
    }

    /// <summary>
    /// Provides an encapsulating structure for a cached object.
    /// </summary>
    [Serializable]
    public class CacheItem
    {
        /// <summary>
        /// Represents an event called when an object has expired.
        /// </summary>
        [field:NonSerialized]
        public event EventHandler<CachedItemEventArgs> TimeElapsed;

        private string _key = string.Empty;
        private DateTime _timeCached = DateTime.MinValue;
        private int _ttl = 0;
        private object _item = null;
        [field:NonSerialized]
        private Timer _timer = null;
        private static readonly object _syncLock = new object();

        /// <summary>
        /// Gets the key that corresponds to this cache item.
        /// </summary>
        public string Key
        {
            get { return _key; }
        }

        /// <summary>
        /// Gets the DateTime value the object was placed in cache.
        /// </summary>
        public DateTime TimeCached
        {
            get { return _timeCached; }
        }

        /// <summary>
        /// Gets the duration that the item should be cached.
        /// </summary>
        public int TimeToLive
        {
            get { return _ttl; }
        }

        /// <summary>
        /// Gets the object held in memory.
        /// </summary>
        public object Item
        {
            get { return _item; }
        }

        /// <summary>
        /// Constructs a new CacheItem.
        /// </summary>
        /// <param name="obj">The object to cache.</param>
        public CacheItem(object obj)
        {
            _timeCached = DateTime.Now;
            _item = obj;
        }

        /// <summary>
        /// Constructs a new CacheItem.
        /// </summary>
        /// <param name="key">The unique key that is used to identify this item in the cache.</param>
        /// <param name="timeCached">The date and time the object was placed in cache.</param>
        /// <param name="timeToLive">The duration that the item should be cached.</param>
        /// <param name="obj">The object to cache.</param>
        public CacheItem(string key, DateTime timeCached, int timeToLive, object obj)
        {
            _key = key;
            _timeCached = timeCached;
            _ttl = timeToLive;
            _item = obj;
            if (_ttl > 0)
            {
                _timer = new Timer(_ttl * 1000);
                _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
                _timer.Start();
            }
        }

        /// <summary>
        /// Removes the instance of itself once the timer has expired.
        /// </summary>
        /// <param name="sender">Timer</param>
        /// <param name="e">ElapsedEventArgs</param>
        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_syncLock)
            {
                _timer.Stop();
                if (this.TimeElapsed != null)
                {
                    this.TimeElapsed(this, new CachedItemEventArgs(this));
                }
            }
        }
    }
}
