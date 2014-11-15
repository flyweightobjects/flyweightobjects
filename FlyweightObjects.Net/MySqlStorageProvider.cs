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
using System.Data;
using System.Text;
using System.Reflection;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents the storage provider which interacts directly with a MySQL database.
	/// </summary>
	public class MySqlStorageProvider : StorageProviderBase, IStorageProvider
	{
		private const string PARAMETER_PREFIX = "?p";

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="cnnString">The string used to connect to the storage.</param>
		public MySqlStorageProvider(string cnnString)
			: base(cnnString, StorageProviderType.MySql)
		{
			
		}

		/// <summary>
		/// Gets the prefix characters to prepend to a parameter.
		/// </summary>
		public override string ParameterPrefix
		{
			get { return PARAMETER_PREFIX; }
		}

		/// <summary>
		/// Gets the prefix characters to prepend to a parameter.
		/// </summary>
		public static string GetParameterPrefix()
		{
			return PARAMETER_PREFIX;
		}

		/// <summary>
        /// Converts an <see cref="IQueryExpression{TSource}"/> into an <see cref="IStorageCommand"/>.
        /// </summary>
        /// <typeparam name="T">The target type of the query.</typeparam>
        /// <param name="query">The query to convert.</param>
        public override IStorageCommand BuildStorageCommand<T>(IRuntimeMethodQuery<T> query)
        {
            return new MySqlQueryBuilder<T>(this).BuildStorageCommand(query);
        }

		/// <summary>
		/// Executes a select statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type from which to select.</typeparam>
		/// <param name="comand">The <see cref="IStorageCommand"/> representing the command.</param>
		public override IEnumerable<T> ProcessSelect<T>(IStorageCommand comand)
		{
            return new FlyweightSet<T>(base.DatabaseManager.ExecuteResultSet(typeof(T), comand));
		}

		/// <summary>
		/// Executes an insert statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to insert.</typeparam>
		/// <param name="source">The instance of T to insert.</param>
		public override T ProcessInsert<T>(T source)
		{
			DataTableAttribute entity = DataAttributeUtilities.GetDataTableAttribute(typeof(T).GetGenericTypeParameter());
			StorageCommand command = this.BuildInsertCommand<T>(source);
			base.DatabaseManager.ExecuteNonQuery(command);
			base.ProcessOutputParameters<T>(source, command.Parameters);
			DataColumnAttribute field = DataAttributeUtilities.GetAutoIncrementDataColumn(typeof(T));
			if (field != null && field.MappedProperty != null && field.ColumnType != DbType.Guid)
			{
				IStorageCommand identitySql = new StorageCommand(string.Format("SELECT LAST_INSERT_ID() AS {0}", field.ColumnName));
				IDataReader reader = base.DatabaseManager.ExecuteReader(identitySql);
				try
				{
					if (reader.Read())
					{
                        source.Storage[field.ColumnName] = reader.GetValue(0);
					}
				}
				finally
				{
					if (!reader.IsClosed)
					{
						reader.Close();
					}
				}
			}
			return source;
		}

		/// <summary>
		/// Executes an insert statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to insert.</typeparam>
		/// <param name="command">The <see cref="IStorageCommand"/> representing the command.</param>
		public override T ProcessInsert<T>(IStorageCommand command)
		{
            ResultSet resultSet = this.DatabaseManager.ExecuteResultSet(typeof(T), command);
            T source = new T();
            if (resultSet != null && resultSet.Rows.Count > 0)
            {
                ((IPropertyStorage)source.Storage).DataSource = resultSet.Rows[0];
            }
            return base.ProcessOutputParameters<T>(source, command.Parameters);
		}

		/// <summary>
		/// Executes a series of batched insert statments against the storage.
		/// </summary>
		/// <typeparam name="T">The type to insert.</typeparam>
		/// <param name="source">The collection of T to insert.</param>
		/// <param name="batchSize">The size of each batch.</param>
		public override void ProcessInsert<T>(IEnumerable<T> source, int batchSize)
		{
			base.ProcessBatch<T>(source, batchSize, TransactionType.Insert);
		}

		/// <summary>
		///  Executes an update statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to update.</typeparam>
		/// <param name="source">The instance of T to update.</param>
		public override T ProcessUpdate<T>(T source)
		{
			StorageCommand command = base.BuildUpdateCommand<T>(source);
			base.DatabaseManager.ExecuteNonQuery(command);
			return source;
		}

		/// <summary>
		/// Executes an update statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to update.</typeparam>
		/// <param name="command">The <see cref="IStorageCommand"/> representing the command.</param>
		public override T ProcessUpdate<T>(IStorageCommand command)
		{
            ResultSet resultSet = this.DatabaseManager.ExecuteResultSet(typeof(T), command);
            T source = new T();
            if (resultSet != null && resultSet.Rows.Count > 0)
            {
                ((IPropertyStorage)source.Storage).DataSource = resultSet.Rows[0];
            }
            return base.ProcessOutputParameters<T>(source, command.Parameters);
		}

		/// <summary>
		/// Executes a series of batched update statments against the storage.
		/// </summary>
		/// <typeparam name="T">The type to update.</typeparam>
		/// <param name="source">The collection of T to update.</param>
		/// <param name="batchSize">The size of each batch.</param>
		public override void ProcessUpdate<T>(IEnumerable<T> source, int batchSize)
		{
			base.ProcessBatch<T>(source, batchSize, TransactionType.Update);
		}

		/// <summary>
		/// Executes a delete statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to delete.</typeparam>
		/// <param name="source">The instance of T to delete.</param>
		public override void ProcessDelete<T>(T source)
		{
			StorageCommand command = base.BuildDeleteCommand<T>(source);
			base.DatabaseManager.ExecuteNonQuery(command);
		}

		/// <summary>
		/// Executes a delete statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to delete.</typeparam>
		/// <param name="command">The <see cref="IStorageCommand"/> representing the command.</param>
		public override void ProcessDelete<T>(IStorageCommand command)
		{
			base.DatabaseManager.ExecuteNonQuery(command);
		}

		/// <summary>
		/// Executes a series of batched delete statments against the storage.
		/// </summary>
		/// <typeparam name="T">The type to delete.</typeparam>
		/// <param name="source">The collection of T to delete.</param>
		/// <param name="batchSize">The size of each batch.</param>
		public override void ProcessDelete<T>(IEnumerable<T> source, int batchSize)
		{
			base.ProcessBatch<T>(source, batchSize, TransactionType.Delete);
		}

		/// <summary>
		/// Returns a boolean indicating whether source exists.
		/// </summary>
		/// <typeparam name="T">The type to search.</typeparam>
		/// <param name="source">The instance of T for which to search.</param>
		public override bool ProcessExists<T>(T source)
		{
            T val = this.ProcessReload<T>(source);
			return val != null;
		}

		/// <summary>
		/// Returns source after being refreshed from the storage.
		/// </summary>
		/// <typeparam name="T">The type to reload.</typeparam>
		/// <param name="source">An instance of T to reload.</param>
		public override T ProcessReload<T>(T source)
		{
            StorageCommand command = base.BuildReloadCommand<T>(source);
            return this.ProcessSelect<T>(command).ToSingle<T>();
           
		}

        /// <summary>
        /// Truncates a target entity located in storage.
        /// </summary>
        /// <typeparam name="T">The type abstracting the table to truncate.</typeparam>
        public override void ProcessTruncate<T>()
        {
            StorageCommand command = base.BuildTruncateCommand<T>();
            base.DatabaseManager.ExecuteNonQuery(command);
        }
	}
}