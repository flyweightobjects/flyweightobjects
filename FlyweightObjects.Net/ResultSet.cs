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
using System.Diagnostics;
using System.Xml.Serialization;
using System.Reflection;
using System.Collections;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents the states of the row.
    /// </summary>
    public enum RowState
    {
        /// <summary>
        /// The row is unchanged.
        /// </summary>
        Unchanged = 0,
        /// <summary>
        /// The row has been created but not yet added to a <see cref="ResultSet.RowCollection"/>.
        /// </summary>
        Detached = 1,
        /// <summary>
        /// The row has been added to a <see cref="ResultSet.RowCollection"/>.
        /// </summary>
        Added = 2,
        /// <summary>
        /// The row has been removed from a <see cref="ResultSet.RowCollection"/>.
        /// </summary>
        Deleted = 4,
        /// <summary>
        /// The row's values have been modified.
        /// </summary>
        Modified = 8,
    }
    
    /// <summary>
    /// Represents a light weight result set.
    /// </summary>
    [Serializable]
    public class ResultSet : ICloneable
    {
        /// <summary>
        /// Represents an event that is fired when a <see cref="ResultSet.Row"/> is added.
        /// </summary>
        [field:NonSerialized]
        public event EventHandler<ItemAddedEventArgs<ResultSet.Row>> RowAdded;

        /// <summary>
        /// Represents an event that is fired when a <see cref="ResultSet.Row"/> is updated.
        /// </summary>
        [field: NonSerialized]
        public event EventHandler<ItemUpdatedEventArgs<ResultSet.Row>> RowUpdated;

        /// <summary>
        /// Represents an event that is fired when a <see cref="ResultSet.Row"/> is deleted.
        /// </summary>
        [field: NonSerialized]
        public event EventHandler<ItemRemovedEventArgs<ResultSet.Row>> RowDeleted;
                
        /// <summary>
        /// Gets the type which uses this <see cref="ResultSet"/> as a data source.
        /// </summary>
        public Type ElementType { get; private set; }
        
        /// <summary>
        /// Gets or sets the name of the <see cref="ResultSet"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the number of records affected by the statement used to populate this <see cref="ResultSet"/>.
        /// </summary>
        public int RecordsAffected { get; private set; }

        /// <summary>
        /// Gets or sets the columnar defintions for the <see cref="ResultSet"/>.
        /// </summary>
        public ColumnCollection Columns { get; set; }

        /// <summary>
        /// Gets or set the row definitions for the <see cref="ResultSet"/>.
        /// </summary>
        public RowCollection Rows { get; set; }

        /// <summary>
        /// Gets the deleted rows for the <see cref="ResultSet"/>.
        /// </summary>
        protected internal RowCollection DeletedRows { get; private set; }

        /// <summary>
        /// Gets a boolean value indicating whether the events are suspended. 
        /// </summary>
        public bool EventsSuspended { get; private set; }

        /// <summary>
        /// Gets a unique Id for the <see cref="ResultSet"/>.
        /// </summary>
        public Guid SchemaGuid { get; private set; }
        
        /// <summary>
        /// Gets a <see cref="HashSet{Guid}"/> representing shemata which have been synchronized.
        /// </summary>
        protected HashSet<Guid> SynchronzedSchemaGuids { get; private set; }

        static Dictionary<Type, ColumnCollection> ColumnCache = new Dictionary<Type, ColumnCollection>();

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        public ResultSet()
        {
            this.SchemaGuid = Guid.NewGuid();
            this.SynchronzedSchemaGuids = new HashSet<Guid>();
            this.Columns = new ColumnCollection(this);
            this.Rows = new RowCollection(this);
            this.DeletedRows = new RowCollection(this);
            this.RecordsAffected = -1;
            this.EventsSuspended = false;
        }

        /// <summary>
        /// Creates a new instance of the class, specifying the type from which to build meta-data.
        /// </summary>
        /// <param name="type">The type that will use this <see cref="ResultSet"/> as a data source.</param>
        public ResultSet(Type type)
            : this()
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this.ElementType = type;
            this.Name = type.Name;
            BuildTypeDefinition();
        }

        /// <summary>
        /// Creates a new instance of the class, specifying the type from which to build meta-data.
        /// </summary>
        /// <param name="columns">The <see cref="ResultSet.ColumnCollection"/> from which to build.</param>
        public ResultSet(ResultSet.ColumnCollection columns)
            : this()
        {
            if (columns == null)
            {
                throw new ArgumentNullException("columns");
            }
            this.Columns = columns;
        }

        /// <summary>
        /// Creates a new instance of the class, specifying the type from which to build meta-data.
        /// </summary>
        /// <param name="type">The type that will use this <see cref="ResultSet"/> as a data source.</param>
        /// <param name="reader">An <see cref="IDataReader"/> from which to build rows.</param>
        public ResultSet(Type type, IDataReader reader)
            : this()
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (reader.IsClosed)
            {
                throw new ArgumentException("The supplied reader cannot be closed.");
            }
            this.ElementType = type;
            this.Name = type.Name;
            this.RecordsAffected = reader.RecordsAffected;
            BuildReaderDefinition(reader);
            BuildTypeDefinition();
        }

        /// <summary>
        /// Suspends all events associated with manipulating row data.
        /// </summary>
        public void SuspendEvents()
        {
            this.EventsSuspended = true;
        }

        /// <summary>
        /// Resumes all events associated with manipulating row data.
        /// </summary>
        public void ResumeEvents()
        {
            this.EventsSuspended = false;
        }

        /// <summary>
        /// Creates a new, detached row.
        /// </summary>
        public Row NewRow()
        {
            object[] itemArray = new object[this.Columns.Count];
            Row row = new Row(this.Rows, itemArray);
            row.RowState = RowState.Detached;
            return row;
        }

        /// <summary>
        /// Creates a new, detached row.
        /// </summary>
        /// <param name="itemArray">The field data for which to create the row.</param>
        public Row NewRow(object[] itemArray)
        {
            if (itemArray.Length != this.Columns.Count)
            {
                throw new InvalidOperationException("The length of the supplied item array does not match that of the target row collection.");
            }
            return new Row(this.Rows, itemArray);
        }

        /// <summary>
        /// Called when a row is added.
        /// </summary>
        /// <param name="row">The added row.</param>
        protected internal void OnRowAdded(Row row)
        {
            if (!this.EventsSuspended && this.RowAdded != null)
            {
                this.RowAdded(this, new ItemAddedEventArgs<Row>(row));
            }
        }

        /// <summary>
        /// Called when a row is updated.
        /// </summary>
        /// <param name="row">The updated row.</param>
        protected internal void OnRowUpdated(Row row)
        {
            if (!this.EventsSuspended && this.RowUpdated != null)
            {
                this.RowUpdated(this, new ItemUpdatedEventArgs<Row>(row));
            }
        }

        /// <summary>
        /// Called when a row is deleted.
        /// </summary>
        /// <param name="row">The deleted row.</param>
        protected internal void OnRowDeleted(Row row)
        {
            if (!this.EventsSuspended) 
            {
                this.DeletedRows.Add(row);   
            }
            if (this.RowDeleted != null)
            {
                this.RowDeleted(this, new ItemRemovedEventArgs<Row>(row));
            }
        }

        /// <summary>
        /// Builds the colimnar defintion for the <see cref="ResultSet"/> using the supplied <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="IDataReader"/> by which to create the columnar definition.</param>
        protected internal void BuildReaderDefinition(IDataReader reader)
        {
            DataTable schemaTable = reader.GetSchemaTable();
            if (schemaTable != null)
            {
                foreach (DataRow row in schemaTable.Rows)
                {
					string columnName = row["ColumnName"].ToString();
					int index = columnName.IndexOf(".");
					if (index > -1 && columnName.Length > 1)
					{
						columnName = columnName.Substring(columnName.IndexOf(".") + 1);
					}
                    if (string.IsNullOrEmpty(columnName))
                    {
                        columnName = string.Format("Column{0}", this.Columns.Count + 1);
                    }
					columnName = columnName.Remove(new char[] { '[', ']' });
					index = 1;
					while (this.Columns.Contains(columnName))
					{
						columnName = string.Format("{0}{1}", columnName, index);
						index++;
					}
					Type type = (Type)(row["DataType"]);
					Column column = new Column(columnName, type);
					this.Columns.Add(column);
                }
            }
        }

        /// <summary>
        /// Builds the columnar defintion for the <see cref="ResultSet"/> using the type.
        /// </summary>
        protected internal void BuildTypeDefinition()
        {
            if (this.ElementType == typeof(QueryResult))
            {
                return;
            }
            ColumnCollection columns;
            if (ColumnCache.TryGetValue(this.ElementType, out columns))
            {
                foreach (Column column in columns)
                {
                    if (!this.Columns.Contains(column.Name))
                    {
                        Column newColumn = column.Clone();
                        this.Columns.Add(newColumn);
                    }
                    else
                    {
                        this.Columns[column.Name].MappedProperty = column.MappedProperty;
                    }
                }
            }
            else
            {
                foreach (PropertyInfo property in this.ElementType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    PropertyStorageAttribute[] propertyStorageAttrs = property.GetCustomAttributes(typeof(PropertyStorageAttribute), true) as PropertyStorageAttribute[];
                    if (propertyStorageAttrs.Length > 0)
                    {
                        Column column = null;
                        if (propertyStorageAttrs[0] is DataColumnAttribute)
                        {
                            DataColumnAttribute dataColumn = propertyStorageAttrs[0] as DataColumnAttribute;
							dataColumn.ColumnName = dataColumn.ColumnName.Remove(new char[] { '[', ']' });
                            if (!this.Columns.Contains(dataColumn.ColumnName))
                            {
                                column = new ResultSet.Column(dataColumn.ColumnName, property.PropertyType);
                                this.Columns.Add(column);
                            }
                            else
                            {
                                column = this.Columns[dataColumn.ColumnName];
                            }
                            column.AllowDBNull = dataColumn.AllowDBNull;
                            column.AutoIncrement = dataColumn.AutoIncrement;
                            column.Caption = dataColumn.Caption;
                            column.ColumnType = dataColumn.ColumnType;
                            column.Computed = dataColumn.Computed;
                            column.HasDefault = dataColumn.HasDefault;
                            column.Identifier = dataColumn.Identifier;
                            column.ReadOnly = dataColumn.ReadOnly;
                            column.SequenceName = dataColumn.SequenceName;
                            column.Size = dataColumn.Size;
                            column.MappedProperty = property;
                        }
                        else if (propertyStorageAttrs[0] is DataRelationAttribute)
                        {
                            DataRelationAttribute dataRelation = propertyStorageAttrs[0] as DataRelationAttribute;
                            if (!this.Columns.Contains(property.Name))
                            {
                                Type type = typeof(DataRelationContainer<>);
                                Type[] parameters = null;
                                if (property.PropertyType.IsGenericType)
                                {
                                    parameters = new Type[] { property.PropertyType.GetGenericTypeParameter() };
                                }
                                else
                                {
                                    parameters = new Type[] { property.PropertyType };
                                }
                                type = type.MakeGenericType(parameters);
                                column = new ResultSet.Column(property.Name, type);
                                this.Columns.Add(column);
                            }
                            else
                            {
                                column = this.Columns[property.Name];
                            }
                            column.IsDataRelation = true;
                            column.AllowPreload = dataRelation.AllowPreload;
                            column.MappedProperty = property;
                        }
                        else if (!this.Columns.Contains(property.Name))
                        {
                            column = new ResultSet.Column(property.Name, property.PropertyType);
                            column.MappedProperty = property;
                            this.Columns.Add(column);
                        }
                        else
                        {
                            this.Columns[property.Name].MappedProperty = property;
                        }
                    }
                }
                lock (ColumnCache)
                {
                    if (!ColumnCache.ContainsKey(this.ElementType))
                    {
                        ColumnCache.Add(this.ElementType, this.Columns.Clone());
                    }
                }
            }
        }

        /// <summary>
        /// Merges the current <see cref="ResultSet"/> column and rows with the supplied <see cref="ResultSet"/>.
        /// </summary>
        /// <param name="resultSet">The <see cref="ResultSet"/> to merge.</param>
        protected internal void Merge(ResultSet resultSet)
        {
            MergeColumns(resultSet.Columns);
            MergeRows(resultSet.Rows);
        }

        /// <summary>
        /// Merges the current <see cref="ResultSet.Row"/> with the supplied <see cref="ResultSet.Row"/>.
        /// </summary>
        /// <param name="row">The <see cref="ResultSet.Row"/> to merge.</param>
        protected internal void Merge(ResultSet.Row row)
        {
            MergeColumns(row.ResultSet.Columns);
            MergeRow(row);
        }

        /// <summary>
        /// Merges the current <see cref="ResultSet"/> with the supplied <see cref="ResultSet.ColumnCollection"/>.
        /// </summary>
        /// <param name="columns">The <see cref="ResultSet.ColumnCollection"/> to merge.</param>
        protected internal void MergeColumns(ColumnCollection columns)
        {
            if (columns == null)
            {
                throw new ArgumentNullException("columns");
            }
            if (columns.ResultSet == null)
            {
                throw new ArgumentNullException("columns.ResultSet");
            }
            if (!SynchronzedSchemaGuids.Contains(columns.ResultSet.SchemaGuid))
            {
                foreach (ResultSet.Column sourceColumn in columns)
                {
                    if (!this.Columns.Contains(sourceColumn.Name))
                    {
                        ResultSet.Column targetColumn = sourceColumn.Clone();
                        this.Columns.Add(targetColumn);
                    }
                }
                SynchronzedSchemaGuids.Add(columns.ResultSet.SchemaGuid);
            }
        }

        /// <summary>
        /// Merges the the supplied <see cref="ResultSet.ColumnCollection"/>.
        /// </summary>
        /// <param name="row">The <see cref="ResultSet.Row"/> to merge.</param>
        protected internal void MergeRow(ResultSet.Row row)
        {
            if (row == null)
            {
                throw new ArgumentNullException("row");
            }
            ResultSet.Row targetRow;
            if (row.ResultSet == this)
            {
                targetRow = this.NewRow(row.ItemArray);
            }
            else
            {
                object[] itemArray = new object[this.Columns.Count];
                targetRow = this.NewRow(itemArray);
                foreach (ResultSet.Column column in this.Columns)
                {
                    if (row.ResultSet.Columns.Contains(column.Name))
                    {
                        targetRow[column.Name] = row[column.Name];
                    }
                }
            }
            this.Rows.Add(targetRow);
        }

        private void MergeRows(ResultSet.RowCollection rows)
        {
            if (rows == null)
            {
                throw new ArgumentNullException("rows");
            }
            foreach (ResultSet.Row sourceRow in rows)
            {
                MergeRow(sourceRow);
            }
        }

        /// <summary>
        /// Represents a column for a given <see cref="ResultSet"/>.
        /// </summary>
        [Serializable, DebuggerDisplay("{Name} {Type}")]
        public class Column
        {
            /// <summary>
            /// Gets the parent <see cref="ResultSet"/> for the column.
            /// </summary>
            public ResultSet ResultSet { get; internal set; }

            /// <summary>
            /// Gets the name of the column.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Gets the type of the data to be stored in the colum.
            /// </summary>
            public Type Type { get; private set; }
            
            /// <summary>
            /// Gets or sets whether the column can except <see cref="DBNull.Value"/>.
            /// </summary>
            public bool AllowDBNull { get; set; }

            /// <summary>
            /// Gets or sets whether the column is automatically incremented by the underlying provider.
            /// </summary>
            public bool AutoIncrement { get; set; }

            /// <summary>
            /// Gets or sets the caption for the column.
            /// </summary>
            public string Caption { get; set; }
            
            /// <summary>
            /// Gets or set the <see cref="DbType"/> for the column.
            /// </summary>
            public DbType ColumnType { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the column is computed by the underlying provider.
            /// </summary>
            public bool Computed { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the column has a default.
            /// </summary>
            public bool HasDefault { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the column particiaptes as a key indentifer.
            /// </summary>
            public bool Identifier { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the column should be considered read only.
            /// </summary>
            public bool ReadOnly { get; set; }

            /// <summary>
            /// Get or sets a value specifying the sequence name underlying provider (Oracle only). 
            /// </summary>
            public string SequenceName { get; set; }

            /// <summary>
            /// Gets or sets a value specifying the size of the column.
            /// </summary>
            public int Size { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the column can be pre-loaded.
            /// </summary>
            public bool AllowPreload { get; set; }

            /// <summary>
            /// Gets a boolean value indicating whether the column represents a <see cref="DataRelationAttribute"/>.
            /// </summary>
            public bool IsDataRelation { get; internal set; }

            /// <summary>
            /// Gets or sets the <see cref="PropertyInfo"/> associated with the column.
            /// </summary>
            [XmlIgnore]
            public PropertyInfo MappedProperty { get; set; }

            /// <summary>
            /// Gets the index of the column.
            /// </summary>
            public int ColumnIndex
            {
                get { return this.ResultSet.Columns.IndexOf(this); }
            }

            /// <summary>
            /// Constructs a new instance of the class.
            /// </summary>
            /// <param name="name">The column name.</param>
            /// <param name="dataType">The type of the column.</param>
            public Column(string name, Type dataType)
            {
                this.Name = name;
                this.Type = dataType;
            }

            /// <summary>
            /// Clones the current column.
            /// </summary>
            public Column Clone()
            {
                return base.MemberwiseClone() as Column; 
            }
        }

        /// <summary>
        /// Represents a row of data found within a <see cref="ResultSet"/>.
        /// </summary>
        [Serializable]
        public class Row
        {
            /// <summary>
            /// Gets the parent <see cref="ResultSet"/> for the row.
            /// </summary>
            protected internal ResultSet ResultSet { get; internal set; }
            
            /// <summary>
            /// Gets or sets the item data for the row.
            /// </summary>
            public object[] ItemArray { get; set; }
            
            /// <summary>
            /// Gets or sets the <see cref="RowState"/> for the row.
            /// </summary>
            public RowState RowState { get; set; }

            /// <summary>
            /// Creates a new instance of the class.
            /// </summary>
            /// <param name="rows">The parent <see cref="RowCollection"/>.</param>
            /// <param name="itemArray">The item data for the row.</param>
            protected internal Row(RowCollection rows, object[] itemArray)
            {
                this.ItemArray = itemArray;
                this.ResultSet = rows.ResultSet;
                this.RowState = RowState.Unchanged;
            }

            /// <summary>
            /// Gets the index of the row.
            /// </summary>
            public int RowIndex
            {
                get 
                { 
                    return this.ResultSet.Rows.IndexOf(this); 
                }
            }

            /// <summary>
            /// Gets a value for the supplied index.
            /// </summary>
            /// <param name="index">The zero-based index of the column.</param>
            protected internal object this[int index]
            {
                get
                {
                    if (index < 0 || index >= this.ResultSet.Columns.Count)
                    {
                        throw new IndexOutOfRangeException(string.Format("The specified column index {0} was not found in the collection.", index));
                    }
                    return this.ItemArray[index];
                }
                set
                {
                    if (index < 0 || index >= this.ResultSet.Columns.Count)
                    {
                        throw new IndexOutOfRangeException(string.Format("The specified column index {0} was not found in the collection.", index));
                    }
                    Column column = this.ResultSet.Columns[index];
                    if (value == null || value == DBNull.Value || !TypeConverter.RequiresConvert(value.GetType(), column.Type))
                    {
                        this.ItemArray[column.ColumnIndex] = value;
                    }
                    else
                    {
                        this.ItemArray[column.ColumnIndex] = TypeConverter.ConvertType(column.Type, value);
                    }
                    if (!this.ResultSet.EventsSuspended && column.Name.ToUpper() != "CHECKSUM")
                    {
                        this.RowState |= RowState.Modified;
                        this.ResultSet.OnRowUpdated(this);
                    }
                }
            }

            /// <summary>
            /// Gets a value for the supplied index.
            /// </summary>
            /// <param name="index">An string specifying the column's name.</param>
            public object this[string index]
            {
                get
                {
                    if (!this.ResultSet.Columns.Contains(index))
                    {
                        throw new IndexOutOfRangeException(string.Format("The specified column index {0} was not found in the collection.", index));
                    }
                    ResultSet.Column column = this.ResultSet.Columns[index];
                    return this.ItemArray[column.ColumnIndex];
                }
                set
                {
                    if (!this.ResultSet.Columns.Contains(index))
                    {
                        throw new IndexOutOfRangeException(string.Format("The specified column index {0} was not found in the collection.", index));
                    }
                    ResultSet.Column column = this.ResultSet.Columns[index];
                    if (value == null || value == DBNull.Value || !TypeConverter.RequiresConvert(value.GetType(), column.Type))
                    {
                        this.ItemArray[column.ColumnIndex] = value;
                    }
                    else
                    {
                        this.ItemArray[column.ColumnIndex] = TypeConverter.ConvertType(column.Type, value);
                    }
                    if (!this.ResultSet.EventsSuspended && column.Name.ToUpper() != "CHECKSUM")
                    {
                        this.RowState |= RowState.Modified;
                        this.ResultSet.OnRowUpdated(this);
                    }
                }
            }

            /// <summary>
            /// Gets a boolean value determining whether the row has been modified.
            /// </summary>
            public bool IsChanged
            {
                get 
                {
                    return ((this.RowState & RowState.Added) == RowState.Added || (this.RowState & RowState.Modified) == RowState.Modified);
                }
            }
        }

        /// <summary>
        /// Represents a collection of <see cref="Column"/>s.
        /// </summary>
        [Serializable, DebuggerDisplay("Count = {Count}")]
        public class ColumnCollection : ICollection<Column>
        {
            /// <summary>
            /// Gets or sets the parent <see cref="ResultSet"/> for the collection of columns.
            /// </summary>
            protected internal ResultSet ResultSet { get; set; }
            
            /// <summary>
            /// Gets or sets the internally stored list of columns.
            /// </summary>
            private List<Column> Columns { get; set; }

            /// <summary>
            /// Constructs a new instance of the class.
            /// </summary>
            /// <param name="resultSet">The parent <see cref="ResultSet"/> for the collection.</param>
            public ColumnCollection(ResultSet resultSet)
            {
                this.Columns = new List<Column>();
                this.ResultSet = resultSet;
            }

            /// <summary>
            /// Gets a <see cref="Column"/> given the supplied index.
            /// </summary>
            /// <param name="index">The zero-based index of the column.</param>
            public Column this[int index]
            {
                get { return this.Columns[index]; }
            }

            /// <summary>
            /// Gets a <see cref="Column"/> given the supplied index.
            /// </summary>
            /// <param name="index">The name of the column.</param>
            public Column this[string index]
            {
                get
                {
                    Column column = this.Columns.FirstOrDefault(c => c.Name.Equals(index, StringComparison.CurrentCultureIgnoreCase));
                    if (column == null)
                    {
                        throw new IndexOutOfRangeException("index");
                    }
                    return column;
                }
            }

            /// <summary>
            /// Creates a new instance of the class.
            /// </summary>
            public ColumnCollection()
            {

            }

            /// <summary>
            /// Gets the index of the supplied <see cref="Column"/>.
            /// </summary>
            /// <param name="column">The column for which to retrieve the index.</param>
            public int IndexOf(Column column)
            {
                return this.Columns.IndexOf(column);
            }

            #region ICollection<Column> Members

            /// <summary>
            /// Adds a new <see cref="Column"/> to the collection.
            /// </summary>
            /// <param name="column">The column to add.</param>
            public void Add(Column column)
            {
                if (column == null)
                {
                    throw new ArgumentNullException();
                }
                if (this.Contains(column.Name))
                {
                    throw new InvalidOperationException("A column with the same name already exists in the collection.");
                }
                this.Columns.Add(column);
                column.ResultSet = this.ResultSet;
                if (this.ResultSet.Rows.Count > 0 && this.ResultSet.Columns.Count > this.ResultSet.Rows[0].ItemArray.Length)
                {
                    for (int i = 0; i < this.ResultSet.Rows.Count; i++)
                    {
                        object[] itemArray = this.ResultSet.Rows[i].ItemArray;
                        Array.Resize<object>(ref itemArray, this.ResultSet.Columns.Count);
                        this.ResultSet.Rows[i].ItemArray = itemArray;
                    }
                }
            }

            /// <summary>
            /// Returns a boolean value indicating whether the column exists.
            /// </summary>
            /// <param name="columnName">The name of the column.</param>
            public bool Contains(string columnName)
            {
                return this.FirstOrDefault(c => c.Name.Equals(columnName, StringComparison.CurrentCultureIgnoreCase)) != null;
            }

            /// <summary>
            /// Clears the collection of all columns.
            /// </summary>
            public void Clear()
            {
                this.Columns.Clear();
            }

            /// <summary>
            /// Returns a boolean value indicating whether the column exists.
            /// </summary>
            /// <param name="column">The <see cref="Column"/> for which to search.</param>
            public bool Contains(Column column)
            {
                return this.Columns.Contains(column);
            }

            /// <summary>
            /// Copies the columns in the collection to the specified array.
            /// </summary>
            /// <param name="array">The target array of columns.</param>
            /// <param name="arrayIndex">The zero-based index by which to start the copy.</param>
            public void CopyTo(Column[] array, int arrayIndex)
            {
                this.Columns.CopyTo(array, arrayIndex);
            }

            /// <summary>
            /// Returns a memberwise clone of the current collection.
            /// </summary>
            internal ColumnCollection Clone()
            {
                return base.MemberwiseClone() as ColumnCollection;
            }

            /// <summary>
            /// Gets the count of the columns within the collection.
            /// </summary>
            public int Count
            {
                get { return this.Columns.Count; ; }
            }

            /// <summary>
            /// Returns a boolean value indicating whether the collection is read only.
            /// </summary>
            public bool IsReadOnly
            {
                get { return false; ; }
            }

            /// <summary>
            /// Removes the supplied <see cref="Column"/> from the list.
            /// </summary>
            /// <param name="column">The column to remove.</param>
            public bool Remove(Column column)
            {
                return this.Columns.Remove(column);
            }

            #endregion

            #region IEnumerable<Column> Members

            /// <summary>
            /// Returns an enumerator for the collection.
            /// </summary>
            public IEnumerator<Column> GetEnumerator()
            {
                return this.Columns.GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            /// <summary>
            /// Returns an enumerator for the collection.
            /// </summary>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            #endregion
        }

        /// <summary>
        /// Represents a collection of <see cref="Row"/>s.
        /// </summary>
        [Serializable, DebuggerDisplay("Count = {Count}")]
        public class RowCollection : ICollection<Row>
        {

            /// <summary>
            /// Gets or sets the parent <see cref="ResultSet"/> for the collection of rows.
            /// </summary>
            public ResultSet ResultSet { get; internal set; }

            /// <summary>
            /// Gets or sets the internally stored list of rows.
            /// </summary>
            private List<Row> Rows { get; set; }
            
            /// <summary>
            /// Gets an object used to synchronize the collection.
            /// </summary>
            public object SyncRoot { get; private set; }

            /// <summary>
            /// Creates a new instance of the class.
            /// </summary>
            protected internal RowCollection(ResultSet resultSet)
            {
                this.Rows = new List<Row>();
                this.ResultSet = resultSet;
            }

            /// <summary>
            /// Gets the <see cref="Row"/> at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index by which to retrieve the row.</param>
            public Row this[int index]
            {
                get { return this.Rows[index]; }
            }

            /// <summary>
            /// Inserts a row at the specified index.
            /// </summary>
            /// <param name="row">The row to insert.</param>
            /// <param name="index">The zero-based index at which to insert the row.</param>
            public void InsertAt(ResultSet.Row row, int index)
            {
                this.Rows.Insert(index, row);
                if (!this.ResultSet.EventsSuspended)
                {
                    row.RowState |= RowState.Added;
                    this.ResultSet.OnRowAdded(row);
                }
            }

            /// <summary>
            /// Removes the row at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index at which to remove the row.</param>
            public void RemoveAt(int index)
            {
                Row row = this.Rows[index];
                if (!this.ResultSet.EventsSuspended)
                {
                    row.RowState |= RowState.Deleted;
                    this.ResultSet.OnRowDeleted(row);
                }
                this.Rows.RemoveAt(index);
            }

            #region ICollection<Row> Members

            /// <summary>
            /// Adds the supplied row to the collection.
            /// </summary>
            /// <param name="row">The <see cref="Row"/> to add.</param>
            public void Add(Row row)
            {
                this.Rows.Add(row);
                if (!this.ResultSet.EventsSuspended)
                {
                    row.RowState |= RowState.Added;
                    this.ResultSet.OnRowAdded(row);
                }
            }

            /// <summary>
            /// Clears the collection of all rows.
            /// </summary>
            public void Clear()
            {
                for (int i = this.Count - 1; i >= 0; i--)
                {
                    this.ResultSet.OnRowDeleted(this.Rows[i]);
                    this.Rows.RemoveAt(i);
                }
            }

            /// <summary>
            /// Returns a boolean value indicating whether the supplied row is contained in the collection.
            /// </summary>
            /// <param name="row">The <see cref="Row"/> for which to search.</param>
            public bool Contains(Row row)
            {
                return this.Rows.Contains(row);
            }

            /// <summary>
            /// Copies the rows in the collection to the specified array.
            /// </summary>
            /// <param name="array">The target array of rows.</param>
            /// <param name="arrayIndex">The zero-based index by which to start the copy.</param>
            public void CopyTo(Row[] array, int arrayIndex)
            {
                this.Rows.CopyTo(array, arrayIndex);
            }

            /// <summary>
            /// Gets the count of rows in the collection.
            /// </summary>
            public int Count
            {
                get { return this.Rows.Count; }
            }

            /// <summary>
            /// Returns a boolean value indicating whether the collection is read only.
            /// </summary>
            public bool IsReadOnly
            {
                get { return false; ; }
            }

            /// <summary>
            /// Removes the specified <see cref="Row"/> from the collection.
            /// </summary>
            /// <param name="row">The <see cref="Row"/> to remove.</param>
            public bool Remove(Row row)
            {
                this.ResultSet.OnRowDeleted(row);
                return this.Rows.Remove(row);
            }

            /// <summary>
            /// Returns the index for the supplied row.
            /// </summary>
            /// <param name="row">The row for which to search for the index.</param>
            public int IndexOf(ResultSet.Row row)
            {
                return this.Rows.IndexOf(row);
            }

            #endregion

            #region IEnumerable<Row> Members

            /// <summary>
            /// Returns an enumerator for the collection.
            /// </summary>
            public IEnumerator<Row> GetEnumerator()
            {
                return this.Rows.GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            /// <summary>
            /// Returns an enumerator for the collection.
            /// </summary>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            #endregion
        }

        /// <summary>
        /// Performs a deep clone of the <see cref="ResultSet"/> including all colums and rows.
        /// </summary>
        public ResultSet Copy()
        {
            return ObjectCloner.Clone<ResultSet>(this);
        }

        #region ICloneable Members

        /// <summary>
        /// Performs a shallow, memberwise clone of the <see cref="ResultSet"/>.
        /// </summary>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
