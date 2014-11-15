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
using System.Data.Common;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents the storage provider which interacts directly with a relational database.
	/// </summary>
	public interface IStorageProvider : ITransactional, IDisposable
	{
        /// <summary>
		/// Gets the prefix characters to prepend to a parameter.
		/// </summary>
		string ParameterPrefix { get; }

        /// <summary>
        /// Gets or sets a value indicating whether empty string values are allowed for string-based data types. If false and 
        /// the underlying column accepts nulls, a <see cref="DBNull"/> value is applied.
        /// </summary>
        bool AllowEmptyString { get; set; }
		
		/// <summary>
        /// Converts an <see cref="IRuntimeMethodQuery{TSource}"/> into an <see cref="IStorageCommand"/>.
        /// </summary>
        /// <typeparam name="T">The target type of the query.</typeparam>
        /// <param name="query">The query to convert.</param>
        IStorageCommand BuildStorageCommand<T>(IRuntimeMethodQuery<T> query) where T : class, IFlyweight, new();
        
		/// <summary>
		/// Executes a select statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type from which to select.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> representing the command.</param>
        IEnumerable<T> ProcessSelect<T>(IStorageCommand command) where T : class, IFlyweight, new();
		
		/// <summary>
		/// Executes an insert statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to insert.</typeparam>
		/// <param name="source">The instance of T to insert.</param>
		T ProcessInsert<T>(T source) where T : class, IFlyweight, new();
		
		/// <summary>
		/// Executes an insert statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to insert.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> representing the command.</param>
        T ProcessInsert<T>(IStorageCommand command) where T : class, IFlyweight, new();
		
		/// <summary>
		/// Executes a series of batched insert statments against the storage.
		/// </summary>
		/// <typeparam name="T">The type to insert.</typeparam>
		/// <param name="source">The collection of T to insert.</param>
		/// <param name="batchSize">The size of each batch.</param>
		void ProcessInsert<T>(IEnumerable<T> source, int batchSize) where T : class, IFlyweight, new();
				
		/// <summary>
		///  Executes an update statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to update.</typeparam>
		/// <param name="source">The instance of T to update.</param>
		T ProcessUpdate<T>(T source) where T : class, IFlyweight, new();

		/// <summary>
		/// Executes an update statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to update.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> representing the command.</param>
        T ProcessUpdate<T>(IStorageCommand command) where T : class, IFlyweight, new();

		/// <summary>
		/// Executes a series of batched update statments against the storage.
		/// </summary>
		/// <typeparam name="T">The type to update.</typeparam>
		/// <param name="source">The collection of T to update.</param>
		/// <param name="batchSize">The size of each batch.</param>
		void ProcessUpdate<T>(IEnumerable<T> source, int batchSize) where T : class, IFlyweight, new();
				
		/// <summary>
		/// Executes a delete statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to delete.</typeparam>
		/// <param name="source">The instance of T to delete.</param>
		void ProcessDelete<T>(T source) where T : class, IFlyweight, new();
		
		/// <summary>
		/// Executes a delete statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to delete.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> representing the command.</param>
        void ProcessDelete<T>(IStorageCommand command) where T : class, IFlyweight, new();

		/// <summary>
		/// Executes a series of batched delete statments against the storage.
		/// </summary>
		/// <typeparam name="T">The type to delete.</typeparam>
		/// <param name="source">The collection of T to delete.</param>
		/// <param name="batchSize">The size of each batch.</param>
		void ProcessDelete<T>(IEnumerable<T> source, int batchSize) where T : class, IFlyweight, new();

		/// <summary>
		/// Returns a boolean indicating whether source exists.
		/// </summary>
		/// <typeparam name="T">The type to search.</typeparam>
		/// <param name="source">The instance of T for which to search.</param>
		bool ProcessExists<T>(T source) where T : class, IFlyweight, new();
		
		/// <summary>
		/// Returns source after being refreshed from the storage.
		/// </summary>
		/// <typeparam name="T">The type to reload.</typeparam>
		/// <param name="source">An instance of T to reload.</param>
		T ProcessReload<T>(T source) where T : class, IFlyweight, new();

        /// <summary>
        /// Truncates a target entity located in storage.
        /// </summary>
        /// <typeparam name="T">The type abstracting the table to truncate.</typeparam>
        void ProcessTruncate<T>() where T : class, IFlyweight, new();
		
		/// <summary>
		/// Gets or sets a connection to the storage.
		/// </summary>
		DbConnection Connection { get; set; }
		
		/// <summary>
		/// Gets or sets the string used to open the storage.
		/// </summary>
		string ConnectionString { get; }
		
		/// <summary>
		/// Gets the type of storage provider for the connection.
		/// </summary>
		StorageProviderType ProviderType { get; }
		
		/// <summary>
		/// Gets or sets the timeout value.
		/// </summary>
		int CommandTimeout { get; set; }

		/// <summary>
		/// Gets or sets the transactional isolation level for commands executes against the storage.
		/// </summary>
		IsolationLevel IsolationLevel { get; set; }

        /// <summary>
        /// Executes an <see cref="IStorageCommand"/> against storage.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        IStorageCommand ProcessExecute(IStorageCommand command);
	}
}
