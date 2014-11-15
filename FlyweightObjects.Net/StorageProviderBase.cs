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
using System.Data.Common;
using System.Collections;
using System.Reflection;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents an abstract base class implementation of a storage provider which interacts directly with a relational database.
	/// </summary>
	public abstract class StorageProviderBase : IStorageProvider, IDisposable
	{
		private DatabaseManager _databaseManager = null;
		private IDbTransaction _activeTransaction = null;
		private IsolationLevel _isolationLevel = IsolationLevel.Unspecified;
		private bool _allowEmptyString = true;
		private static readonly object _syncLock = new object();

		/// <summary>
		/// Gets the prefix characters to prepend to a parameter.
		/// </summary>
		public abstract string ParameterPrefix { get; }

        /// <summary>
        /// Converts an <see cref="IRuntimeMethodQuery{TSource}"/> into an <see cref="IStorageCommand"/>.
        /// </summary>
        /// <typeparam name="T">The target type of the query.</typeparam>
        /// <param name="query">The query to convert.</param>
        public abstract IStorageCommand BuildStorageCommand<T>(IRuntimeMethodQuery<T> query) where T : class, IFlyweight, new();

		/// <summary>
		/// Executes a select statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type from which to select.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> representing the command.</param>
		public abstract IEnumerable<T> ProcessSelect<T>(IStorageCommand command) where T : class, IFlyweight, new();

		/// <summary>
		/// Executes an insert statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to insert.</typeparam>
		/// <param name="source">The instance of T to insert.</param>
		public abstract T ProcessInsert<T>(T source) where T : class, IFlyweight, new();

		/// <summary>
		/// Executes an insert statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to insert.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> representing the command.</param>
        public abstract T ProcessInsert<T>(IStorageCommand command) where T : class, IFlyweight, new();

		/// <summary>
		/// Executes a series of batched insert statments against the storage.
		/// </summary>
		/// <typeparam name="T">The type to insert.</typeparam>
		/// <param name="source">The collection of T to insert.</param>
		/// <param name="batchSize">The size of each batch.</param>
		public abstract void ProcessInsert<T>(IEnumerable<T> source, int batchSize) where T : class, IFlyweight, new();

		/// <summary>
		///  Executes an update statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to update.</typeparam>
		/// <param name="source">The instance of T to update.</param>
		public abstract T ProcessUpdate<T>(T source) where T : class, IFlyweight, new();

		/// <summary>
		/// Executes an update statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to update.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> representing the command.</param>
        public abstract T ProcessUpdate<T>(IStorageCommand command) where T : class, IFlyweight, new();

		/// <summary>
		/// Executes a series of batched update statments against the storage.
		/// </summary>
		/// <typeparam name="T">The type to update.</typeparam>
		/// <param name="source">The collection of T to update.</param>
		/// <param name="batchSize">The size of each batch.</param>
		public abstract void ProcessUpdate<T>(IEnumerable<T> source, int batchSize) where T : class, IFlyweight, new();

		/// <summary>
		/// Executes a delete statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to delete.</typeparam>
		/// <param name="source">The instance of T to delete.</param>
		public abstract void ProcessDelete<T>(T source) where T : class, IFlyweight, new();

		/// <summary>
		/// Executes a delete statement against the storage.
		/// </summary>
		/// <typeparam name="T">The type to delete.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> representing the command.</param>
        public abstract void ProcessDelete<T>(IStorageCommand command) where T : class, IFlyweight, new();

		/// <summary>
		/// Executes a series of batched delete statments against the storage.
		/// </summary>
		/// <typeparam name="T">The type to delete.</typeparam>
		/// <param name="source">The collection of T to delete.</param>
		/// <param name="batchSize">The size of each batch.</param>
		public abstract void ProcessDelete<T>(IEnumerable<T> source, int batchSize) where T : class, IFlyweight, new();

		/// <summary>
		/// Returns a boolean indicating whether source exists.
		/// </summary>
		/// <typeparam name="T">The type to search.</typeparam>
		/// <param name="source">The instance of T for which to search.</param>
		public abstract bool ProcessExists<T>(T source) where T : class, IFlyweight, new();

		/// <summary>
		/// Returns source after being refreshed from the storage.
		/// </summary>
		/// <typeparam name="T">The type to reload.</typeparam>
		/// <param name="source">An instance of T to reload.</param>
		public abstract T ProcessReload<T>(T source) where T : class, IFlyweight, new();

        /// <summary>
        /// Truncates a target entity located in storage.
        /// </summary>
        /// <typeparam name="T">The type abstracting the table to truncate.</typeparam>
        public abstract void ProcessTruncate<T>() where T : class, IFlyweight, new();

        /// <summary>
        /// Executes an <see cref="IStorageCommand"/> against storage.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public virtual IStorageCommand ProcessExecute(IStorageCommand command)
        {
            return this.DatabaseManager.ExecuteNonQuery(command);
        }

        /// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="connectionString">The string used to connect to the storage.</param>
		/// <param name="providerType">The <see cref="StorageProviderType"/> of the storage.</param>
		public StorageProviderBase(string connectionString, StorageProviderType providerType)
		{
			_databaseManager = new DatabaseManager(connectionString, providerType);
		}

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="databaseManager">The <see cref="DatabaseManager "/> that will execute the operations against storage.</param>
        public StorageProviderBase(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

		/// <summary>
		/// Gets or sets the connection object.
		/// </summary>
		public DbConnection Connection
		{
			get { return _databaseManager.Connection; }
			set { _databaseManager.Connection = value; }
		}

		/// <summary>
		/// Gets the current active transaction if one exists.
		/// </summary>
		public IDbTransaction ActiveTransaction
		{
			get { return _activeTransaction; }
		}

		/// <summary>
		/// Gets the string used to connect to the storage.
		/// </summary>
		public string ConnectionString
		{
			get { return _databaseManager.ConnectionString; }
		}

		/// <summary>
		/// Gets the <see cref="StorageProviderType"/> of the storage.
		/// </summary>
		public StorageProviderType ProviderType
		{
			get { return _databaseManager.ProviderType; }
		}

		/// <summary>
		/// Gets or sets the timeout of each command executed against the storage.
		/// </summary>
		public int CommandTimeout
		{
			get { return _databaseManager.CommandTimeout; }
			set { _databaseManager.CommandTimeout = value; }
		}

		/// <summary>
		/// Gets or sets the transaction <see cref="IsolationLevel"/> of exach executed command.
		/// </summary>
		public IsolationLevel IsolationLevel
		{
			get { return _isolationLevel; }
			set { _isolationLevel = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether empty string values are allowed for string-based data types. If false and 
        /// the underlying column accepts nulls, a <see cref="DBNull"/> value is applied.
		/// </summary>
		public bool AllowEmptyString
		{
			get { return _allowEmptyString; }
			set { _allowEmptyString = value; }
		}

		/// <summary>
		/// Gets the <see cref="DatabaseManager"/> used to communicate with the storage.
		/// </summary>
		public DatabaseManager DatabaseManager
		{
			get { return _databaseManager; }
		}

		/// <summary>
		/// Starts a new <see cref="IDbTransaction"/> if one is not already present, otherwise it will enlist.
		/// </summary>
		public IDbTransaction BeginTransaction()
		{
            return _activeTransaction = _isolationLevel != System.Data.IsolationLevel.Unspecified ?
                _databaseManager.BeginTransaction(_isolationLevel) :
                _databaseManager.BeginTransaction();
		}

		/// <summary>
		/// Commits the current transaction to storage assuming that the current instance is the owner.
		/// </summary>
		public void CommitTransaction()
		{
			_databaseManager.CommitTransaction();
			_activeTransaction = null;
		}

        /// <summary>
        /// Commits the current transaction to storage. If force is equal to true, the transaction will be commited even though the current instance may not be the owner.
        /// </summary>
        /// <param name="force">A value indicating whether to force the commit regardless of ownership.</param>
        public void CommitTransaction(bool force)
        {
            CommitTransaction();
        }

		/// <summary>
		/// Gets the current active transaction if one if present, otherwise returns null.
		/// </summary>
		public void RollbackTransaction()
		{
			_databaseManager.RollbackTransaction();
			_activeTransaction = null;
		}

        /// <summary>
		/// Binds the returned out put parameters on the object if applicable.
		/// </summary>
		/// <typeparam name="T">The type to bind the outout parameters to.</typeparam>
		/// <param name="source">An instance of T.</param>
		/// <param name="parameters">A <see cref="ParameterCollection"/> of parameters.</param>
        public virtual T ProcessOutputParameters<T>(T source, ParameterCollection parameters) where T : class, IFlyweight, new()
		{
			foreach (Parameter parameter in parameters)
			{
				if (parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Output)
				{
					if (parameter.Value != DBNull.Value)
					{
						if (parameter.DataColumnAttribute != null && parameter.DataColumnAttribute.MappedProperty != null)
						{
							parameter.DataColumnAttribute.MappedProperty.SetValue(source, parameter.Value, null);
						}
					}
				}
			}
			return source;
		}

		/// <summary>
		/// Interates throgh a collection of objects, creates the command texts and then executes the entire batch as one transaction.
		/// </summary>
		/// <typeparam name="T">The type to process.</typeparam>
		/// <param name="source">A collection of T.</param>
		/// <param name="batchSize">The size of the batch.</param>
		/// <param name="transactionType">The transaction type for the batch.</param>
		public virtual void ProcessBatch<T>(IEnumerable<T> source, int batchSize, TransactionType transactionType) where T : class, IFlyweight, new()
		{
			if (source == null)
			{
				throw new ArgumentException("The supplied list argument cannot be null.");
			}
			if (batchSize <= 0 || batchSize > source.Count<T>())
			{
				batchSize = source.Count<T>();
			}
            IList<T> collection = source as FlyweightSet<T>;
            if (collection == null)
            {
                collection = source.ToList();
            }
			int processedCount = 0;
			while (processedCount < source.Count<T>())
			{
				StringBuilder sb = new StringBuilder();
				StorageCommand command = new StorageCommand();
				if (processedCount + batchSize > source.Count<T>())
				{
					batchSize = source.Count<T>() - processedCount;
				}
				for (int i = 0; i < batchSize; i++)
				{
					StorageCommand sql = null;
					switch (transactionType)
					{
						case TransactionType.Delete:
							sql = this.BuildDeleteCommand<T>(collection[processedCount + i]);
							break;
						case TransactionType.Insert:
							sql = this.BuildInsertCommand<T>(collection[processedCount + i]);
							break;
						case TransactionType.Update:
							sql = this.BuildUpdateCommand<T>(collection[processedCount + i]);
							break;
						default: break;
					}
					if (!sql.SqlText.Trim().EndsWith(";"))
					{
						sql.SqlText += ";";
					}
					sb.Append(string.Format("{0}\r\n", sql.SqlText));
					command.Parameters.AddRange(sql.Parameters);
				}
				command.SqlText = sb.ToString();
				this.DatabaseManager.ExecuteNonQuery(command);
				processedCount += batchSize;
			}
		}

		/// <summary>
		/// Returns an insert command based upon the supplied object.
		/// </summary>
		/// <typeparam name="T">The type to insert.</typeparam>
		/// <param name="source">An instance of T to insert.</param>
        protected virtual StorageCommand BuildInsertCommand<T>(T source) where T : class, IFlyweight, new()
		{
			StorageCommand command = new StorageCommand();
			DataTableAttribute entity = DataAttributeUtilities.GetDataTableAttribute(typeof(T));
			List<DataColumnAttribute> dataColumns = DataAttributeUtilities.GetDataColumnAttributes(typeof(T));
			StringBuilder sb = new StringBuilder(string.Format("INSERT INTO {0} (", entity.TableName));
			foreach (DataColumnAttribute dataColumn in dataColumns)
			{
				if (IsValidForInsert<T>(source, dataColumn))
				{
					sb.Append(dataColumn.ColumnName);
					if (dataColumns.IndexOf(dataColumn) < dataColumns.Count - 1)
					{
						sb.Append(", ");
					}
				}
			}
			if (sb.ToString().EndsWith(", "))
			{
				sb.Remove(sb.Length - 2, 2);
			}
			sb.Append(") VALUES (");
			foreach (DataColumnAttribute dataColumn in dataColumns)
			{
				object val = dataColumn.MappedProperty.GetValue(source, null);
				if (IsValidForInsert<T>(source, dataColumn))
				{
					Parameter param = command.CreateParameter(this.ParameterPrefix, dataColumn, val, ParameterDirection.Input); 
					sb.Append(param.Name);
					if (dataColumns.IndexOf(dataColumn) < dataColumns.Count - 1)
					{
						sb.Append(", ");
					}
				}
			}
			if (sb.ToString().EndsWith(", "))
			{
				sb.Remove(sb.Length - 2, 2);
			}
			sb.Append(")");
			command.SqlText = sb.ToString();
			return command;
		}

		/// <summary>
		/// Returns an update command based upon the supplied object.
		/// </summary>
		/// <typeparam name="T">The type to update.</typeparam>
		/// <param name="source">An instance of T to update.</param>
        protected virtual StorageCommand BuildUpdateCommand<T>(T source) where T : class, IFlyweight, new()
		{
			StorageCommand command = new StorageCommand();
			DataTableAttribute entity = DataAttributeUtilities.GetDataTableAttribute(typeof(T));
			StringBuilder sb = new StringBuilder(string.Format("UPDATE {0} SET ", entity.TableName));
            List<DataColumnAttribute> dataColumns;
            IPropertyChangedTrackable obj = source as IPropertyChangedTrackable;
            if (obj != null && obj.ChangedProperties != null && obj.ChangedProperties.Count > 0)
            {
                dataColumns = DataAttributeUtilities.GetDataColumnAttributes(typeof(T), obj.ChangedProperties);
            }
            else
            {
                dataColumns = DataAttributeUtilities.GetDataColumnAttributes(typeof(T));
            }
            dataColumns.RemoveAll(p => p.Identifier || p.AutoIncrement || p.Computed || p.ReadOnly);
            if (dataColumns.Count == 0)
            {
                return command;
            }
            foreach (DataColumnAttribute dataColumn in dataColumns)
			{
				object val = dataColumn.MappedProperty.GetValue(source, null);
                if (val == null || (val.ToString() == string.Empty && !_allowEmptyString))
				{
					val = DBNull.Value;
				}
				Parameter param = command.CreateParameter(this.ParameterPrefix, dataColumn, val, ParameterDirection.Input);
				sb.Append(string.Format("{0} = {1}", dataColumn.ColumnName, param.Name));
				if (dataColumns.IndexOf(dataColumn) < dataColumns.Count - 1)
				{
					sb.Append(", ");
				}
			}
			if (sb.ToString().EndsWith(", "))
			{
				sb.Remove(sb.Length - 2, 2);
			}
			sb.Append(" WHERE ");
            List<DataColumnAttribute> identifiers = DataAttributeUtilities.GetDataColumnIdentifiers(typeof(T));
			foreach (DataColumnAttribute dataColumn in identifiers)
			{
				object val = dataColumn.MappedProperty.GetValue(source, null);
				Parameter param = command.CreateParameter(this.ParameterPrefix, dataColumn, val, ParameterDirection.Input);
				sb.Append(string.Format("{0} = {1}", dataColumn.ColumnName, param.Name));
				if (identifiers.IndexOf(dataColumn) < identifiers.Count - 1)
				{
					sb.Append(" AND ");
				}
			}
			command.SqlText = sb.ToString();
			return command;
		}

		/// <summary>
		/// Returns a delete command based upon the supplied object.
		/// </summary>
		/// <typeparam name="T">The type to delete.</typeparam>
		/// <param name="source">An instance of T to delete.</param>
        protected virtual StorageCommand BuildDeleteCommand<T>(T source) where T : class, IFlyweight, new()
		{
			StorageCommand command = new StorageCommand();
			DataTableAttribute dataTable = DataAttributeUtilities.GetDataTableAttribute(typeof(T));
			StringBuilder sb = new StringBuilder(string.Format("DELETE FROM {0}", dataTable.TableName));
			sb.Append(" WHERE ");
            List<DataColumnAttribute> identifiers = DataAttributeUtilities.GetDataColumnIdentifiers(typeof(T));
			foreach (DataColumnAttribute dataColumn in identifiers)
			{
				object val = dataColumn.MappedProperty.GetValue(source, null);
				Parameter param = command.CreateParameter(this.ParameterPrefix, dataColumn, val, ParameterDirection.Input);
				sb.Append(string.Format("{0} = {1}", dataColumn.ColumnName, param.Name));
				if (identifiers.IndexOf(dataColumn) < identifiers.Count - 1)
				{
					sb.Append(" AND ");
				}
			}
			command.SqlText = sb.ToString();
			return command;
		}

        /// <summary>
        /// Builds a truncate statement for a type abstracting a table in storage.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected virtual StorageCommand BuildTruncateCommand<T>() where T : class, IFlyweight, new()
        {
            DataTableAttribute dataTable = DataAttributeUtilities.GetDataTableAttribute(typeof(T));
            if (string.IsNullOrEmpty(dataTable.TableName))
            {
                throw new InvalidOperationException(ErrorStrings.InvalidTableNameException);
            }
            return new StorageCommand(string.Format("TRUNCATE TABLE {0}", dataTable.TableName.Trim()));
        }

		/// <summary>
		/// Builds a select command used to reload the object.
		/// </summary>
		/// <typeparam name="T">The type to reload.</typeparam>
		/// <param name="source">An instance of T to reload.</param>
        protected virtual StorageCommand BuildReloadCommand<T>(T source) where T : class, IFlyweight, new()
		{
			StringBuilder sb = new StringBuilder();
			StorageCommand command = new StorageCommand();
            DataTableAttribute dataTable = DataAttributeUtilities.GetDataTableAttribute(typeof(T));
            sb.Append(string.Format("SELECT {0}.* FROM {1}", dataTable.TableName, dataTable.TableName));
			sb.Append(" WHERE ");
            List<DataColumnAttribute> identifiers = DataAttributeUtilities.GetDataColumnIdentifiers(typeof(T));
			foreach (DataColumnAttribute dataColumn in identifiers)
			{
				object val = dataColumn.MappedProperty.GetValue(source, null);
				Parameter param = command.CreateParameter(this.ParameterPrefix, dataColumn, val, ParameterDirection.Input);
				sb.Append(string.Format("{0}.{1} = {2}", dataTable.TableName, dataColumn.ColumnName, param.Name));
				if (identifiers.IndexOf(dataColumn) < identifiers.Count - 1)
				{
					sb.Append(" AND ");
				}
			}
			command.SqlText = sb.ToString();
			return command;
		}
        
		/// <summary>
		/// Returns a boolean value indicating whether the supplied <see cref="DataColumnAttribute"/> can be inserted.
		/// </summary>
		/// <typeparam name="T">The type to interrogate.</typeparam>
		/// <param name="source">An instance of T.</param>
		/// <param name="dataColumn">A <see cref="DataColumnAttribute"/> representing the meta data about the underlying field.</param>
        public virtual bool IsValidForInsert<T>(T source, DataColumnAttribute dataColumn) where T : class, IFlyweight, new()
		{
			bool retVal = !dataColumn.AutoIncrement & !dataColumn.Computed & !dataColumn.ReadOnly;
			if (retVal && dataColumn.HasDefault)
			{
				object val = dataColumn.MappedProperty.GetValue(source, null);
                Type type = dataColumn.MappedProperty.PropertyType;
                if (val == null || val.ToString().Equals(string.Empty) || val.Equals(type.Default()))
				{
					retVal = false;
				}
				else if (typeof(DateTime).IsAssignableFrom(val.GetType()) && ((DateTime)val == DateTime.MinValue || (DateTime)val == DateTime.MaxValue))
				{
					retVal = false;
				}
                else if (dataColumn.MappedProperty.GetMemberInfoType().IsValueType)
                {
                    int value = -1;
                    if (int.TryParse(val.ToString(), out value) && value == 0)
                    {
                        retVal = false;
                    }
                }
			}
			return retVal;
		}

		/// <summary>
		/// Disposes the object.
		/// </summary>
		public void Dispose()
		{
			_databaseManager.Dispose();
		}
	}
}
