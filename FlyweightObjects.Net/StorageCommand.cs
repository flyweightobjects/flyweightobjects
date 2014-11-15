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
using System.Data;
using System;
using System.Diagnostics;
using System.Text;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents a transaction type to be executed against the storage.
	/// </summary>
	public enum TransactionType
	{
		/// <summary>
		/// The default setting.
		/// </summary>
		Unknown,
		
		/// <summary>
		/// A select statment.
		/// </summary>
		Select,
		
		/// <summary>
		/// An insert statement.
		/// </summary>
		Insert,

		/// <summary>
		/// An update statement.
		/// </summary>
		Update,
		
        /// <summary>
        /// A truncate statement
        /// </summary>
        Truncate,

		/// <summary>
		/// A delete statement.
		/// </summary>
		Delete,
		
		/// <summary>
		/// A persist statement.
		/// </summary>
		Persist
	}

	/// <summary>
	/// Represents a structured query language command to be executed against the storage.
	/// </summary>
    [Serializable]
	[DebuggerDisplay("{SqlText}")]
	public class StorageCommand : IStorageCommand
	{
		/// <summary>
		/// The name of the command.
		/// </summary>
		protected string _name = string.Empty;
		/// <summary>
		/// The command text or procedure name of the command.
		/// </summary>
		protected string _sqlText = string.Empty;
		/// <summary>
		/// The <see cref="ParameterCollection"/> of the command.
		/// </summary>
		protected ParameterCollection _parameters;
		/// <summary>
		/// The <see cref="CommandType"/> of the command.
		/// </summary>
		private CommandType _commandType = CommandType.Text;
        		
 		/// <summary>
 		/// The name of the command.
 		/// </summary>
		public string Name 
		{
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// Gets or sets the SQL command text to be run on the storage.
		/// </summary>
		public virtual string SqlText 
		{
			get { return _sqlText; }
			set { _sqlText = value; }
		}

		/// <summary>
		/// Gets or sets the <see cref="CommandType"/>.
		/// </summary>
		public virtual CommandType CommandType
		{
			get { return _commandType; }
			set { _commandType = value; }
		}

		/// <summary>
		/// Gets the <see cref="TransactionType"/>.
		/// </summary>
		public virtual TransactionType TransactionType
		{
			get 
			{
				return this.GetTransactionType();
			}
		}
		
		/// <summary>
		/// Gets or sets the collection of parameters to be used when querying the storage.
		/// </summary>
		public ParameterCollection Parameters 
		{
			get { return _parameters; }
			set { _parameters = value; }
		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
        protected internal StorageCommand() 
        {
            _parameters = new ParameterCollection();
        }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        protected internal StorageCommand(string sqlText, CommandType commandType)
            : this()
        {
            _sqlText = sqlText;
            _commandType = commandType;
        }

        /// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		public StorageCommand(string sqlText)
            : this()
		{
			_sqlText = sqlText;
		}

        /// <summary>
		/// Returns a <see cref="Parameter"/> to be used in the command.
		/// </summary>
		/// <param name="parameterPrefix">The prefix to prepend to the name.</param>
		/// <param name="dataColumn">The <see cref="DataColumnAttribute"/> used in creation.</param>
		/// <param name="value">The value for the Parameter.</param>
		/// <param name="direction">The <see cref="ParameterDirection"/> of the Parameter.</param>
		protected internal Parameter CreateParameter(string parameterPrefix, DataColumnAttribute dataColumn, object value, ParameterDirection direction)
		{
			Parameter param = new Parameter(value == null ? DBNull.Value : value, direction, dataColumn.ColumnType);
            param.Name = string.Format("{0}{1}", parameterPrefix, param.GetHashCode());
            if (this.TransactionType == TransactionType.Select)
            {
                param.Name = string.Format("{0}{1}", parameterPrefix, this.Parameters.Count + 1);
            }
			param.Size = dataColumn.Size;
			param.SourceColumnName = dataColumn.ColumnName;
			param.DataColumnAttribute = dataColumn;
            this.Parameters.Add(param);
            return param;
		}

		/// <summary>
        /// Returns a string representation of the <see cref="StorageCommand"/>.
		/// </summary>
		public override string ToString()
		{
            StringBuilder sb = new StringBuilder(this.SqlText.Trim());
            if (this.Parameters.Count > 0)
            {
                sb.Append(" [");
                foreach (Parameter param in this.Parameters)
                {
                    sb.Append(string.Format("{0}={1}, ", param.Name, param.Value));
                }
                if (sb.ToString().EndsWith(", "))
                {
                    sb.Remove(sb.Length - 2, 2);
                }
                sb.Append("]");
            }
			return sb.ToString();
		}
		
	}
}
	