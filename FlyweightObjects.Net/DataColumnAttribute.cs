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
using System.Text;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace FlyweightObjects
{
    /// <summary>
	/// Attributes a domain type's property with information stored in a column.
    /// </summary>
	[Serializable] 
	[AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
    [DebuggerDisplay("{ColumnName}")]
    public sealed class DataColumnAttribute : PropertyStorageAttribute
	{
		private string _columnName = string.Empty;
		private DbType _columnType = DbType.String;
		private bool _identifier = false;
		private bool _autoIncrement = false;
		private bool _allowDBNull = false;
		private bool _computed = false;
		private bool _readOnly = false;
		private int _index = int.MinValue;
        private bool _hasDefault = false;
        private string _sequenceName = string.Empty;
		private int _size = 0;
        private string _caption = string.Empty;
		
        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string ColumnName
        {
            get { return _columnName; }
			set { _columnName = value; }
        }

		/// <summary>
		/// Gets or sets the DbType property of the source table column.
		/// </summary>
		public DbType ColumnType
		{
			get { return _columnType; }
			set { _columnType = value; }
		}

		/// <summary>
		/// Gets or sets whether or not the property is part of a key identifier in the source table.
		/// </summary>
		public bool Identifier
		{
			get { return _identifier; }
			set { _identifier = value; }
		}

		/// <summary>
		/// Gets or sets whether or not the source table column is auto incrementing (i.e., sequence or identity).
		/// </summary>
		public bool AutoIncrement
		{
			get { return _autoIncrement; }
			set { _autoIncrement = value; }
		}

		/// <summary>
		/// Gets or sets whether or not the source column will allow DbNull values.
		/// </summary>
		public bool AllowDBNull
		{
			get { return _allowDBNull; }
			set { _allowDBNull = value; }
		}

		/// <summary>
		/// Gets or sets whether or not the source column is computed.
		/// </summary>
		public bool Computed
		{
			get { return _computed; }
			set { _computed = value; }
		}

		/// <summary>
		/// Gets or sets whether or not the source column is read only.
		/// </summary>
		public bool ReadOnly
		{
			get { return _readOnly; }
			set { _readOnly = value; }
		}
        
        /// <summary>
        /// Gets or sets whether or not the field has a default.
        /// </summary>
        public bool HasDefault
        {
            get { return _hasDefault; }
            set { _hasDefault = value; }
        }

        /// <summary>
        /// Gets or sets the name of the sequence for the field.
        /// </summary>
        public string SequenceName
        {
            get { return _sequenceName; }
            set { _sequenceName = value; }
        }
		
        /// <summary>
        /// Gets or sets the index (ordinal) of the field name.
        /// </summary>
        public int Index
        {
            get { return this._index; }
            set { this._index = value; }
		}

		/// <summary>
		/// Gets or sets the size of the parameter.
		/// </summary>
		public int Size
		{
			get { return _size; }
			set { _size = value; }
		}

        /// <summary>
        /// Gets or sets the caption for the column.
        /// </summary>
        public string Caption
        {
            get 
            {
                if (string.IsNullOrEmpty(_caption))
                {
                    return _columnName;
                }
                return _caption;
            }
            set 
            { 
                _caption = value; 
            }
        }
		
        /// <summary>
        /// Constructs a new DataColumn object.
        /// </summary>
        /// <param name="fieldName">The field name of the DataSource object.</param>
        public DataColumnAttribute(string fieldName)
        {
            this.ColumnName = fieldName;
        }

		/// <summary>
		/// Constructs a new DataColumn object.
		/// </summary>
		/// <param name="fieldName">The field name of the DataSource object</param>
		/// <param name="dbType">The DbType of the DataSource field</param>
		public DataColumnAttribute(string fieldName, DbType dbType)
		{
			this.ColumnName = fieldName;
			this._columnType = dbType;
		}

        /// <summary>
		/// Constructs a new DataColumn object.
		/// </summary>
		/// <param name="fieldName">The field name of the DataSource object.</param>
		/// <param name="dbType">The DbType of the DataSource field.</param>
		/// <param name="identifier">Determines if this property is part of a key identifier in the DataSource.</param>
		/// <param name="allowDbNull">Determines if this property can accept DbNull values.</param>
		public DataColumnAttribute(string fieldName, DbType dbType, bool identifier, bool allowDbNull)
		{
			this.ColumnName = fieldName;
			this._columnType = dbType;
			this._identifier = identifier;
			this._allowDBNull = allowDbNull;
		}
    }
}
