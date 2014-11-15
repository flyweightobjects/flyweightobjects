//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
//  Author:         Marcus Crane, Software Develper / Architect                                         //
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
using System.Runtime.Serialization;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Data.OracleClient;
using System.Reflection;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents a class to assist in database operations.
    /// </summary>
    public class DatabaseManager : IDisposable
    {
        private readonly string _connectionString = string.Empty;
        private readonly string _dataSource = string.Empty;
        private readonly StorageProviderType _providerType;
        private DbConnection _connection;
        private DbTransaction _transaction;
        private int _commandTimeout = 0;
        private bool _disposed = false;
        private static readonly object _syncRoot = new object();

        /// <summary>
        /// Gets the connection string used to construct the <see cref="DatabaseManager"/>.
        /// </summary>
        public string ConnectionString
        {
            get { return _connectionString; }
        }

        /// <summary>
        /// Gets or sets the connection object.
        /// </summary>
        protected internal DbConnection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        /// <summary>
        /// Gets the StorageProviderType used to construct the object.
        /// </summary>
        public StorageProviderType ProviderType
        {
            get { return _providerType; }
        }

        /// <summary>
        /// Gets or sets the timeout for each command executed.
        /// </summary>
        public int CommandTimeout
        {
            get { return _commandTimeout; }
            set { _commandTimeout = value; }
        }

        /// <summary>
        /// Gets whether the current database connection has created a transaction.
        /// </summary>
        public bool IsTransacted
        {
            get { return _transaction != null; }
        }

        /// <summary>
        /// Gets the current transaction if one exists.
        /// </summary>
        protected internal DbTransaction Transaction
        {
            get { return _transaction; }
            set { _transaction = value; }
        }

        /// <summary>
        /// Gets the parsed data source token from the connection string.
        /// </summary>
        public string DataSource
        {
            get { return _dataSource; }
        }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="connectionString">The connection string to the current database.</param>
        /// <param name="providerType">The StorageProviderType of the current database.</param>
        public DatabaseManager(string connectionString, StorageProviderType providerType)
        {
            _connectionString = connectionString;
            _providerType = providerType;
            _dataSource = ParseDataSource();
            PrepareConnection();
        }

        /// <summary>
        /// Creates a new transaction if one is not already present.
        /// </summary>
        public IDbTransaction BeginTransaction()
        {
            if (this.IsTransacted)
            {
                return this.Transaction;
            }
            Connect();
			TraceHelper.WriteLine(MethodBase.GetCurrentMethod(), "Beginning transaction");
            return this.Transaction = this.Connection.BeginTransaction();
        }

        /// <summary>
        /// Creates a new transaction if one is not already present.
        /// </summary>
        /// <param name="isolationLevel">The IsolationLevel for the transaction.</param>
        public IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (this.IsTransacted)
            {
                return this.Transaction;
            }
            Connect();
			TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("Beginning transaction with isolation level {0}", isolationLevel));
            return this.Transaction = this.Connection.BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// Commits all executed operations within the current transaction to the current database.
        /// </summary>
        public void CommitTransaction()
        {
            if (this.IsTransacted && this.Connection.State == ConnectionState.Open)
            {
				TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), "Committing current transaction");
                this.Transaction.Commit();
                this.Transaction = null;
				TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), "Closing database connection");
                this.Connection.Close();
            }
        }

        /// <summary>
        /// Abandons all changes made within the current transaction scope.
        /// </summary>
        public void RollbackTransaction()
        {
            if (this.IsTransacted && this.Connection.State == ConnectionState.Open)
            {
				TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), "Rolling back current transaction");
                this.Transaction.Rollback();
                this.Transaction = null;
				TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), "Closing database connection");
                this.Connection.Close();
            }
        }

        /// <summary>
        /// Prepares a connection for the current database.
        /// </summary>
        protected virtual void PrepareConnection()
        {
            switch (this.ProviderType)
            {
                case StorageProviderType.SqlServer:
					this.Connection = DbProviderFactories.GetFactory(typeof(SqlConnection).Namespace).CreateConnection();
					this.Connection.ConnectionString = this.ConnectionString;
                    break;
                case StorageProviderType.MsJet:
                    this.Connection = new OleDbConnection(this.ConnectionString);
                    break;
                case StorageProviderType.Oracle:
                    this.Connection = new OracleConnection(this.ConnectionString);
                    break;
                case StorageProviderType.MySql:
					this.Connection = DbProviderFactories.GetFactory("MySql.Data.MySqlClient").CreateConnection();
                    this.Connection.ConnectionString = this.ConnectionString;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Creates a new <see cref="IDbCommand"/> from the <see cref="IStorageCommand"/> argument.
        /// </summary>
        /// <param name="command">The SQL from which to create an IDbCommand.</param>
        protected virtual DbCommand CreateCommand(IStorageCommand command)
        {
            DbCommand cmd = _connection.CreateCommand();
            cmd.CommandTimeout = _commandTimeout;
            cmd.CommandText = command.SqlText;
            cmd.CommandType = command.CommandType;
            foreach (Parameter parameter in command.Parameters)
            {
                IDbDataParameter dataParameter = cmd.CreateParameter();
                dataParameter.DbType = parameter.Type;
                dataParameter.Direction = parameter.Direction;
                dataParameter.ParameterName = parameter.Name;
                dataParameter.Size = parameter.Size;
                dataParameter.Value = parameter.Value ?? DBNull.Value;
                cmd.Parameters.Add(dataParameter);
            }
            if (this.IsTransacted)
            {
                cmd.Transaction = _transaction;
            }
            return cmd;
        }

        /// <summary>
        /// Executes an <see cref="IDataReader"/> against the current database.
        /// </summary>
        /// <param name="command">The SQL to execute.</param>
        public virtual IDataReader ExecuteReader(IStorageCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            try
            {
                IDataReader reader = null;
                if (command.IsInitialized())
                {
                    Connect();
                    DbCommand cmd = this.CreateCommand(command);
                    CommandBehavior commandBehavior = this.IsTransacted ? CommandBehavior.Default : CommandBehavior.CloseConnection;
                    TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("SQL => {0}", command.ToString()));
                    reader = cmd.ExecuteReader(commandBehavior);
                    this.ResetParameters(cmd, command);
                }
                return reader; 
            }
            catch (Exception e)
            {
                StorageProviderException ex = new StorageProviderException(this.ProviderType, command, e);
				TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// Executes an <see cref="ResultSet"/> against the current database.
        /// </summary>
        /// <param name="command">The SQL to execute.</param>
        /// <param name="type">The domain type that <see cref="ResultSet"/> represents.</param>
        public virtual ResultSet ExecuteResultSet(Type type, IStorageCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            ResultSet resultSet = null;
            IDataReader reader = null;
            try
            {
                if (command.IsInitialized())
                {
                    Connect();
                    DbCommand cmd = this.CreateCommand(command);
                    CommandBehavior commandBehavior = this.IsTransacted ? CommandBehavior.Default : CommandBehavior.CloseConnection;
                    TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("SQL => {0}", command.ToString()));
                    reader = cmd.ExecuteReader(commandBehavior);
                    resultSet = new ResultSet(type, reader);
                    resultSet.SuspendEvents();
                    while (reader.Read())
                    {
                        object[] values = new object[resultSet.Columns.Count];
                        reader.GetValues(values);
                        resultSet.Rows.Add(resultSet.NewRow(values));
                    }
                }
                return resultSet;
            }
            catch (Exception e)
            {
                StorageProviderException ex = new StorageProviderException(this.ProviderType, command, e);
                TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), ex.ToString());
                throw ex;
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                {
                    reader.Close();
                }
				if (resultSet != null)
				{
					resultSet.ResumeEvents();
				}
                Disconnect();
            }
        }

        /// <summary>
        /// Returns a <see cref="DataTable"/> object using the supplied <see cref="IStorageCommand"/> command.
        /// </summary>
        /// <param name="command">The SQL to execute.</param>
        public virtual DataTable ExecuteDataTable(IStorageCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            try
            {
                DataTable table = new DataTable();
                if (command.IsInitialized())
                {
                    Connect();
                    DbCommand cmd = this.CreateCommand(command);
                    DbDataAdapter adapter = this.CreateDataAdapter();
                    adapter.SelectCommand = cmd;
                    TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("SQL => {0}", command.ToString()));
                    table.RemotingFormat = SerializationFormat.Binary;
                    adapter.Fill(table);
                }
                return table;
            }
            catch (Exception e)
            {
                StorageProviderException ex = new StorageProviderException(this.ProviderType, command, e);
				TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), ex.ToString());
                throw ex;
            }
            finally
            {
                Disconnect();
            }
        }

        /// <summary>
        /// Executes a non-query operation against the current database.
        /// </summary>
        /// <param name="command">The SQL to execute.</param>
        public virtual IStorageCommand ExecuteNonQuery(IStorageCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            try
            {
                if (command.IsInitialized())
                {
                    Connect();
                    DbCommand cmd = this.CreateCommand(command);
                    TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("SQL => {0}", command.ToString()));
                    cmd.ExecuteNonQuery();
                    this.ResetParameters(cmd, command);
                }
                return command; 
            }
            catch (Exception e)
            {
                StorageProviderException ex = new StorageProviderException(this.ProviderType, command, e);
				TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), ex.ToString());
                throw ex;
            }
            finally
            {
                Disconnect();
            }
        }
        
        /// <summary>
        /// Returns an <see cref="IDbDataAdapter"/> for the provider type.
        /// </summary>
        protected virtual DbDataAdapter CreateDataAdapter()
        {
            string providerString = this.GetProviderString();
            DbProviderFactory providerFactory = DbProviderFactories.GetFactory(providerString);
            return providerFactory.CreateDataAdapter();
        }

        /// <summary>
        /// Returns a namespace-based string representing the location of the provider.
        /// </summary>
        protected virtual string GetProviderString()
        {
            switch (this.ProviderType)
            {
                case StorageProviderType.SqlServer:
                    return "System.Data.SqlClient";
                case StorageProviderType.SqlServerCe:
                    return "System.Data.SqlServerCe";
                case StorageProviderType.MsJet:
                    return "System.Data.OleDb";
                case StorageProviderType.Oracle:
                    return "System.Data.OracleClient";
                case StorageProviderType.MySql:
                    return "MySql.Data.MySqlClient";
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Refreshes the <see cref="IStorageCommand"/> parameters collection with values from an IDbCommand.
        /// </summary>
        /// <param name="dbCommand">The IDbCommand containing the parameters.</param>
        /// <param name="storageCommand">The <see cref="IStorageCommand"/> to have its parameters refreshed.</param>
        protected void ResetParameters(IDbCommand dbCommand, IStorageCommand storageCommand)
        {
            if (dbCommand == null)
            {
                throw new ArgumentNullException("dbCommand");
            }
            if (storageCommand == null)
            {
                throw new ArgumentNullException("storageCommand");
            }
            foreach (Parameter param in storageCommand.Parameters)
            {
                param.Value = ((IDataParameter)dbCommand.Parameters[param.Name]).Value;
                IDataReader reader = param.Value as IDataReader;
                if (reader != null)
                {
                    try
                    {
                        DataTable table = new DataTable();
                        table.Load(reader, LoadOption.OverwriteChanges);
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
        }

        private void Connect()
        {
            if (this.Connection.State != ConnectionState.Open)
            {
				TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("Opening connection to {0}", string.IsNullOrEmpty(_dataSource) ? "database" : _dataSource));
				this.Connection.Open();
            }
        }

        private void Disconnect()
        {
            if (!this.IsTransacted && this.Connection != null)
            {
				TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), "Closing database connection");
                this.Connection.Close();
            }
        }

        private string ParseDataSource()
        {
            try
            {
                string[] values = _connectionString.Split(";".ToCharArray());
                foreach (string str in values)
                {
                    if (str.Trim().ToUpper().Contains("DATA SOURCE"))
                    {
                        int idx = str.IndexOf("=");
                        if (str.Trim().Length > idx)
                        {
                            return str.Substring(str.IndexOf("=") + 1).Trim();
                        }
                    }
                }
            }
            catch { }
            return string.Empty;
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
					if (this.Connection != null)
					{
						if (this.IsTransacted)
						{
							TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), "Rolling back current transaction");
							_transaction.Rollback();
						}
						if (this.Connection.State != ConnectionState.Closed)
						{
							TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), "Closing database connection");
							this.Connection.Close();
						}
					}
					_disposed = true;
                }
            }
        }

        /// <summary>
        /// Finalizer for the object.
        /// </summary>
        ~DatabaseManager()
        {
            this.Dispose(false);
        }
    }
}
