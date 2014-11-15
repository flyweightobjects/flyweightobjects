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

namespace FlyweightObjects
{
    /// <summary>
    /// Represents the storage provider which interacts directly with a Microsoft Sql Server Compact Edition database.
    /// </summary>
    public class SqlServerCeStorageProvider : StorageProviderBase
    {
        private const string PARAMETER_PREFIX = "@p";

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="connectionString">The string used to connect to the storage.</param>
        public SqlServerCeStorageProvider(string connectionString)
            : base(connectionString, StorageProviderType.SqlServerCe)
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
            return new SqlServerCeQueryBuilder<T>(this).BuildStorageCommand(query);
        }

        /// <summary>
        /// Executes a select statement against the storage.
        /// </summary>
        /// <typeparam name="T">The type from which to select.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> representing the command.</param>
        public override IEnumerable<T> ProcessSelect<T>(IStorageCommand command)
        {
            return new FlyweightSet<T>(base.DatabaseManager.ExecuteResultSet(typeof(T), command));
        }

        /// <summary>
        /// Executes an insert statement against the storage.
        /// </summary>
        /// <typeparam name="T">The type to insert.</typeparam>
        /// <param name="source">The instance of T to insert.</param>
        public override T ProcessInsert<T>(T source)
        {
            StorageCommand command = this.BuildInsertCommand<T>(source);
            base.DatabaseManager.ExecuteNonQuery(command);
            base.ProcessOutputParameters<T>(source, command.Parameters);
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
        /// Returns an <see cref="StorageCommand"/> representing an insert command based upon the contents of soucre.
        /// </summary>
        /// <typeparam name="T">The type to insert.</typeparam>
        /// <param name="source">An instance of T.</param>
        protected override StorageCommand BuildInsertCommand<T>(T source)
        {
            StorageCommand command = new StorageCommand();
            DataTableAttribute entity = DataAttributeUtilities.GetDataTableAttribute(typeof(T));
            List<DataColumnAttribute> dataColumns = DataAttributeUtilities.GetDataColumnAttributes(typeof(T));
            StringBuilder sb = new StringBuilder(string.Format("INSERT INTO {0} (", entity.TableName));

            foreach (DataColumnAttribute dataColumn in dataColumns)
            {
                if (base.IsValidForInsert<T>(source, dataColumn))
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
                if (base.IsValidForInsert<T>(source, dataColumn))
                {
                    object val = dataColumn.MappedProperty.GetValue(source, null);
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

            DataColumnAttribute autoIncrementedField = DataAttributeUtilities.GetAutoIncrementDataColumn(typeof(T));
            if (autoIncrementedField != null)
            {
                Parameter param = command.CreateParameter(this.ParameterPrefix, autoIncrementedField, DBNull.Value, ParameterDirection.Output);
                param.DataColumnAttribute = autoIncrementedField;
                param.Size = autoIncrementedField.Size == 0 ? int.MaxValue : param.Size;
                sb.Append(string.Format("\r\nSELECT {0} = SCOPE_IDENTITY()", param.Name));
            }
            command.SqlText = sb.ToString();
            return command;
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
