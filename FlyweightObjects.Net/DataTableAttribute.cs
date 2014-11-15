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
using System.Diagnostics;

namespace FlyweightObjects
{
	/// <summary>
	/// Attributes a domain type with information about an entity contained within storage.
	/// </summary>
	[Serializable]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [DebuggerDisplay("{TableName}")]
	public sealed class DataTableAttribute : Attribute
	{
		private string _tableName = string.Empty;
		private bool _enableCaching = false;
		private int _cacheTimeOut = 0;
		private bool _allowSelect = true;
		private bool _allowInsert = true;
		private bool _allowUpdate = true;
		private bool _allowDelete = true;
        private bool _allowTruncate = true;
		private string _domainName = string.Empty;

        /// <summary>
        /// Constructs a new DataTable instance.
        /// </summary>
        public DataTableAttribute()
        {

        }

		/// <summary>
		/// Constructs a new DataTable instance.
		/// </summary>
		/// <param name="tableName"></param>
		public DataTableAttribute(string tableName)
		{
			_tableName = tableName;
		}

		/// <summary>
		/// Gets or sets the name of the table for which the class applies.
		/// </summary>
		public string TableName
		{
			get { return _tableName; }
			set { _tableName = value; }
		}

		/// <summary>
		/// Gets or sets whether the domain type can be retrieved from storage.
		/// </summary>
		public bool AllowSelect
		{
			get { return _allowSelect; }
			set { _allowSelect = value; }
		}

		/// <summary>
		/// Gets or sets whether the domain type can be inserted into storage.
		/// </summary>
		public bool AllowInsert
		{
			get { return _allowInsert; }
			set { _allowInsert = value; }
		}

		/// <summary>
		/// Gets or sets whether the domain type can be updated in storage.
		/// </summary>
		public bool AllowUpdate
		{
			get { return _allowUpdate; }
			set { _allowUpdate = value; }
		}

		/// <summary>
		/// Gets or sets whether the domain type can be deleted from storage.
		/// </summary>
		public bool AllowDelete
		{
			get { return _allowDelete; }
			set { _allowDelete = value; }
		}

        /// <summary>
        /// Gets or sets whether the domain type can be truncated in storage.
        /// </summary>
        public bool AllowTruncate
        {
            get { return _allowTruncate; }
            set { _allowTruncate = value; }
        }

		/// <summary>
		/// Gets or sets whether the object can be cached. Use CacheTimeout to specify the
		/// length of time in seconds to keep the object in memory. If the EnableCaching 
		/// value is set to true and the CacheTimeout value is set to zero, the object will be
		/// held in memory for the lifetime of the assembly, otherwise it will be removed from
		/// the cache once the CacheTimeout property has expired.
		/// </summary>
		public bool EnableCaching
		{
			get { return _enableCaching; }
			set { _enableCaching = value; }
		}

		/// <summary>
		/// Gets or sets the number of seconds to hold the object in cache. Setting this
		/// property to a value other than zero will have no effect unless EnableCaching
		/// is set to true.
		/// </summary>
		public int CacheTimeout
		{
			get { return _cacheTimeOut; }
			set { _cacheTimeOut = value; }
		}

		/// <summary>
		/// Gets or sets the unique name of the domain which identifies this object with a specific instance of a <see cref="DataContext"/>.
		/// </summary>
		public string DomainName
		{
			get { return _domainName; }
			set { _domainName = value; }
		}
	}
}
