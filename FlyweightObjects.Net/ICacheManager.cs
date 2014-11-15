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
using System.Collections;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents all required functionality to manage a cache of objects.
	/// </summary>
	public interface ICacheManager
	{
        /// <summary>
        /// Represents an event that is fired when an item is added to the cache.
        /// </summary>
        event EventHandler<CachedItemEventArgs> ItemAdded;

        /// <summary>
        /// Represents an event that is fired when an item is removed from the cache.
        /// </summary>
        event EventHandler<CachedItemEventArgs> ItemRemoved;

        /// <summary>
        /// Returns a string representing a key which may used to uniquely identify a command, including the depth by which it was retrieved.
        /// </summary>
        /// <typeparam name="T">The type for which the cache key should be built.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> used as the command.</param>
        string BuildCacheKey<T>(IStorageCommand command) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns a string representing a key which may used to uniquely identify a command, including the depth by which it was retrieved.
		/// </summary>
        /// <typeparam name="T">The type for which the cache key should be built.</typeparam>
		/// <param name="depth">The depth by which to interrogate the object graph.</param>
        /// <param name="command">The <see cref="IStorageCommand"/> used as the command.</param>
        string BuildCacheKey<T>(PreloadDepth depth, IStorageCommand command) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns a string representing a key which may used to uniquely identify a command, including the options associated to its retrieval.
        /// </summary>
        /// <typeparam name="T">The type for which the cache key should be built.</typeparam>
        /// <param name="options">A <see cref="PreloadOptions{T}"/> specifying additional related objects to retrieve.</param>
        /// <param name="command">The <see cref="IStorageCommand"/> used as the command.</param>
        string BuildCacheKey<T>(PreloadOptions<T> options, IStorageCommand command) where T : class, IFlyweight, new();

        /// <summary>
		/// Adds the supplied object to the cache.
		/// </summary>
		/// <typeparam name="T">The type to add.</typeparam>
		/// <param name="key">The unique key to give to the item.</param>
		/// <param name="timeToLive">The length of time the object should live in the cache.</param>
        /// <param name="source">The instance object to cache.</param>
        void AddObject<T>(string key, int timeToLive, IEnumerable<T> source) where T : class, IFlyweight, new();

        /// <summary>
		/// Returns a boolean value indicating whether teh object exists in the cache. If it does, result
		/// be set to the object, otherwise if will its default value.
		/// </summary>
		/// <typeparam name="T">The type to search.</typeparam>
		/// <param name="key">The unique key for the item.</param>
		/// <param name="result">The item if it exists in the cache, otherwise its default value.</param>
		/// <returns></returns>
        bool TryGetObject<T>(string key, out IEnumerable<T> result) where T : class, IFlyweight, new();
		
		/// <summary>
		/// Returns the object from the cache.
		/// </summary>
		/// <typeparam name="T">The type to retrieve.</typeparam>
		/// <param name="key">The unique key for the item.</param>
        IEnumerable<T> GetObject<T>(string key) where T : class, IFlyweight, new();

		/// <summary>
		/// Removes the object from the cache using the provided key.
		/// </summary>
		/// <param name="key">The unique key for the item.</param>
		void RemoveObject(string key);

		/// <summary>
		/// Returns a boolean value indicating whether the item exists in the cache based upon the supplied key.
		/// </summary>
		/// <param name="key">The unique key for the item.</param>
		/// <returns></returns>
		bool Contains(string key);
		
		/// <summary>
		/// Clears the cache of all items.
		/// </summary>
		void Flush();

		/// <summary>
		/// Gets or sets a value indicating whether the <see cref="ICacheManager"/> should be enabled.
		/// </summary>
		bool Enabled { get; set; }
	}
}
