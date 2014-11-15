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
using System.Reflection;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents the storage provider which interacts directly with a Microsoft Jet database.
	/// </summary>
	public class MsJetStorageProvider : StorageProviderBase, IStorageProvider
	{
		private const string PARAMETER_PREFIX = "@p";

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="connectionString">The connection string used to communicate with the storage.</param>
		public MsJetStorageProvider(string connectionString)
			: base(connectionString, StorageProviderType.MsJet)
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
        /// Converts an <see cref="IRuntimeMethodQuery{TSource}"/> into an <see cref="IStorageCommand"/>.
        /// </summary>
        /// <typeparam name="T">The target type of the query.</typeparam>
        /// <param name="query">The query to convert.</param>
        public override IStorageCommand BuildStorageCommand<T>(IRuntimeMethodQuery<T> query)
        {
            return new MsJetQueryBuilder<T>(this).BuildStorageCommand(query);
        }

		/// <summary>
		/// Executes a select statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type from which to select.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> representing the command.</param>
        public override IEnumerable<T> ProcessSelect<T>(IStorageCommand command)
		{
			DataTableAttribute entity = DataAttributeUtilities.GetDataTableAttribute(typeof(T).GetGenericTypeParameter());
			if (!string.IsNullOrEmpty(entity.TableName) && !this.IsBracketed(entity.TableName))
			{
                this.BracketEntityNames(command, entity.TableName);
			}
            return new FlyweightSet<T>(base.DatabaseManager.ExecuteResultSet(typeof(T), command));
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
			if (!string.IsNullOrEmpty(entity.TableName) && !this.IsBracketed(entity.TableName))
			{
				this.BracketEntityNames(command, entity.TableName);
			}
			base.DatabaseManager.ExecuteNonQuery(command);
			base.ProcessOutputParameters<T>(source, command.Parameters);
			DataColumnAttribute field = DataAttributeUtilities.GetAutoIncrementDataColumn(typeof(T));
			if (field != null && field.MappedProperty != null && field.ColumnType != DbType.Guid)
			{
				IStorageCommand identitySql = new StorageCommand("SELECT @@IDENTITY");
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
			DataTableAttribute entity = DataAttributeUtilities.GetDataTableAttribute(typeof(T).GetGenericTypeParameter());
			if (!string.IsNullOrEmpty(entity.TableName) && !this.IsBracketed(entity.TableName))
			{
                this.BracketEntityNames(command, entity.TableName);
			}
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
			throw new NotSupportedException(string.Format("Batch inserts are not supported by the {0}", this.GetType().Name));
		}

		/// <summary>
		///  Executes an update statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to update.</typeparam>
		/// <param name="source">The instance of T to update.</param>
		public override T ProcessUpdate<T>(T source)
		{
			StorageCommand command = base.BuildUpdateCommand<T>(source);
			DataTableAttribute entity = DataAttributeUtilities.GetDataTableAttribute(typeof(T).GetGenericTypeParameter());
			if (!string.IsNullOrEmpty(entity.TableName) && !this.IsBracketed(entity.TableName))
			{
				this.BracketEntityNames(command, entity.TableName);
			}
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
			DataTableAttribute entity = DataAttributeUtilities.GetDataTableAttribute(typeof(T).GetGenericTypeParameter());
			if (!string.IsNullOrEmpty(entity.TableName) && !this.IsBracketed(entity.TableName))
			{
                this.BracketEntityNames(command, entity.TableName);
			}
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
			throw new NotSupportedException(string.Format("Batch updates are not supported by the {0}", this.GetType().Name));
		}

		/// <summary>
		/// Executes a delete statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to delete.</typeparam>
		/// <param name="source">The instance of T to delete.</param>
		public override void ProcessDelete<T>(T source)
		{
			StorageCommand command = base.BuildDeleteCommand<T>(source);
			DataTableAttribute entity = DataAttributeUtilities.GetDataTableAttribute(typeof(T).GetGenericTypeParameter());
			if (!string.IsNullOrEmpty(entity.TableName) && !this.IsBracketed(entity.TableName))
			{
				this.BracketEntityNames(command, entity.TableName);
			}
			base.DatabaseManager.ExecuteNonQuery(command);
		}

		/// <summary>
		/// Executes a delete statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to delete.</typeparam>
		/// <param name="command">The <see cref="IStorageCommand"/> representing the command.</param>
		public override void ProcessDelete<T>(IStorageCommand command)
		{
			DataTableAttribute entity = DataAttributeUtilities.GetDataTableAttribute(typeof(T).GetGenericTypeParameter());
			if (!string.IsNullOrEmpty(entity.TableName) && !this.IsBracketed(entity.TableName))
			{
				this.BracketEntityNames(command, entity.TableName);
			}
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
			throw new NotSupportedException(string.Format("Batch deletes are not supported by the {0}", this.GetType().Name));
		}

		/// <summary>
		/// Returns a boolean indicating whether source exists.
		/// </summary>
		/// <typeparam name="T">The type to search.</typeparam>
		/// <param name="source">The instance of T for which to search.</param>
		public override bool ProcessExists<T>(T source)
		{
			StorageCommand command = base.BuildReloadCommand<T>(source);
			DataTableAttribute entity = DataAttributeUtilities.GetDataTableAttribute(typeof(T).GetGenericTypeParameter());
			if (!string.IsNullOrEmpty(entity.TableName) && !this.IsBracketed(entity.TableName))
			{
				this.BracketEntityNames(command, entity.TableName);
			}
            return base.DatabaseManager.ExecuteDataTable(command).Rows.Count > 0;
		}

		/// <summary>
		/// Returns source after being refreshed from the storage.
		/// </summary>
		/// <typeparam name="T">The type to reload.</typeparam>
		/// <param name="source">An instance of T to reload.</param>
		public override T ProcessReload<T>(T source)
		{
			StorageCommand command = base.BuildReloadCommand<T>(source);
			DataTableAttribute entity = DataAttributeUtilities.GetDataTableAttribute(typeof(T).GetGenericTypeParameter());
			if (!string.IsNullOrEmpty(entity.TableName) && !this.IsBracketed(entity.TableName))
			{
				this.BracketEntityNames(command, entity.TableName);
			}
            return this.ProcessSelect<T>(command).ToSingle<T>();
		}

        /// <summary>
        /// Truncates a target entity located in storage.
        /// </summary>
        /// <typeparam name="T">The type abstracting the table to truncate.</typeparam>
        public override void ProcessTruncate<T>()
        {
            throw new NotSupportedException(string.Format("Truncates are not supported by the {0}", this.GetType().Name));
        }

		private bool IsBracketed(string value)
		{
			if (!string.IsNullOrEmpty(value.Trim()))
			{
				if (value.TrimStart().StartsWith("[") && value.TrimEnd().EndsWith("]"))
				{
					return true;
				}
			}
			return false;
		}

		private void BracketEntityNames(IStorageCommand command, string entityName)
		{
			command.SqlText += " ";
			command.SqlText = command.SqlText.Replace(string.Format(" {0} ", entityName), string.Format(" [{0}] ", entityName));
			command.SqlText = command.SqlText.Replace(string.Format(" ({0} ", entityName), string.Format(" ([{0}] ", entityName));
			command.SqlText = command.SqlText.Replace(string.Format(" {0}) ", entityName), string.Format(" [{0}]) ", entityName));
			command.SqlText = command.SqlText.Replace(string.Format("{0}.", entityName), string.Format("[{0}].", entityName));
		}
	}
}
