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
using System.Data;
using System.Diagnostics;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents an abstract common parameter.
    /// </summary>
	[Serializable]
    [DebuggerDisplay("Name:{Name}, Value:{Value}")]
	public class Parameter
    {
        private string _name = string.Empty;
		private object _value = DBNull.Value;
		private ParameterDirection _direction = ParameterDirection.Input;
		private DbType _type = DbType.AnsiString;
		private int _size = 0;
		private string _sourceColumnName = string.Empty;
		private DataColumnAttribute _dataColumn = null;
        private bool _isResultSet = false;

		/// <summary>
		/// Gets or sets the name of the parameter.
		/// </summary>
        public String Name
        {
            get { return _name; }
			internal set { _name = value; }
        }

		/// <summary>
		/// Gets or sets the value of the parameter.
		/// </summary>
        public virtual object Value
        {
            get { return _value; }
            set { _value = value; }
        }

		/// <summary>
		/// Gets or sets the <see cref="ParameterDirection"/> of the parameter.
		/// </summary>
        public ParameterDirection Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

		/// <summary>
		/// Gets or sets the <see cref="DbType"/> of the parameter.
		/// </summary>
        public DbType Type
        {
            get { return _type; }
            set { _type = value; }
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
		/// Gets or sets the name of the source column for the parameter.
		/// </summary>
        public string SourceColumnName
        {
            get { return _sourceColumnName; }
            set { _sourceColumnName = value; }
        }

		/// <summary>
		/// Gets or sets the name of the <see cref="DataColumnAttribute"/> for the parameter.
		/// </summary>
		internal DataColumnAttribute DataColumnAttribute
		{
			get { return _dataColumn; }
			set { _dataColumn = value; }
		}

        /// <summary>
        /// Gets or sets whether the <see cref="Parameter"/> represents a result set.
        /// </summary>
        public bool IsResultSet
        {
            get { return _isResultSet; }
            set 
            { 
                _isResultSet = value;
                _type = DbType.Object;
            }
        }
	
		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
        public Parameter() { }

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="value">The value of the parameter.</param>
		public Parameter(string name, object value)
		{
			_name = name;
			_value = value;
		}
		
		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="value">The value of the parameter.</param>
		/// <param name="direction">The <see cref="ParameterDirection"/> of the parameter.</param>
		/// <param name="dbType">The <see cref="DbType"/> of the parameter.</param>
		/// <param name="size">The size of the parameter.</param>
		public Parameter(string name, object value, ParameterDirection direction, DbType dbType, int size)
        {
            _name = name;
            _value = value;
            _direction = direction;
            _type = dbType;
            _size = size;
        }

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="value">The value of the parameter.</param>
		/// <param name="direction">The <see cref="ParameterDirection"/> of the parameter.</param>
        public Parameter(string name, object value, ParameterDirection direction)
        {
            _name = name;
            _value = value;
            _direction = direction;
        }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> of the parameter.</param>
        /// <param name="isResultSet">Indicates whether the parameter represents a result set.</param>
        public Parameter(string name, object value, ParameterDirection direction, bool isResultSet)
        {
            _name = name;
            _value = value;
            _direction = direction;
            _isResultSet = isResultSet;
            if (_isResultSet)
            {
                _type = DbType.Object;
            }
        }


		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="value">The value of the parameter.</param>
		/// <param name="dbType">The <see cref="DbType"/> of the parameter.</param>
        public Parameter(string name, object value, DbType dbType)
        {
            _name = name;
            _value = value;
            _type = dbType;
        }

		/// <summary>
		///  Constructs a new instance of the class.
		/// </summary>
		/// <param name="value">The value of the parameter.</param>
		/// <param name="direction">The <see cref="ParameterDirection"/> of the parameter.</param>
		/// <param name="dbType">The <see cref="DbType"/> of the parameter.</param>
		internal Parameter(object value, ParameterDirection direction, DbType dbType)
		{
			_value = value;
			_type = dbType;
			_direction = direction;
		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="value">The value of the parameter.</param>
		/// <param name="direction">The <see cref="ParameterDirection"/> of the parameter.</param>
		/// <param name="dbType">The <see cref="DbType"/> of the parameter.</param>
        public Parameter(string name, object value, ParameterDirection direction, DbType dbType)
        {
            _name = name;
            _value = value;
            _type = dbType;
            _direction = direction;
        }

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="value">The value of the parameter.</param>
		/// <param name="dbType">The <see cref="DbType"/> of the parameter.</param>
		/// <param name="size">The size of the parameter.</param>
        public Parameter(string name, object value, DbType dbType, int size)
        {
            _name = name;
            _value = value;
            _type = dbType;
            _size = size;
        }

		/// <summary>
		///  Constructs a new instance of the class.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="dbType">The <see cref="DbType"/> of the parameter.</param>
		/// <param name="size">The size of the parameter.</param>
        public Parameter(string name, DbType dbType, int size)
        {
            _name = name;
            _type = dbType;
            _size = size;
        }
    }
}
