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
using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents a grouping of internal utility methods.
	/// </summary>
    internal sealed class DataAttributeUtilities
	{
        private static Dictionary<Type, DataTableAttribute> _dataTableCache = new Dictionary<Type, DataTableAttribute>(); 
        private static Dictionary<Type, List<DataColumnAttribute>> _dataColumnCache = new Dictionary<Type, List<DataColumnAttribute>>();
        private static Dictionary<Type, List<DataColumnAttribute>> _identifierCache = new Dictionary<Type, List<DataColumnAttribute>>();
        private static readonly object _syncLock = new object();
        
        /// <summary>
        /// Returns a <see cref="DataTableAttribute"/> for the specified type argument. 
        /// </summary>
        /// <param name="type">The type to interrogate.</param>
        public static DataTableAttribute GetDataTableAttribute(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            DataTableAttribute retVal = null;
            if (!_dataTableCache.TryGetValue(type, out retVal))
            {
                DataTableAttribute[] dataTableAttrs = type.GetCustomAttributes(typeof(DataTableAttribute), true) as DataTableAttribute[];
                if (dataTableAttrs.Length == 0)
                {
                    throw new ArgumentException(ErrorStrings.MissingDataTableAttributeException);
                }
                retVal = dataTableAttrs[0];
                lock (_syncLock)
                {
                    if (!_dataTableCache.ContainsKey(type))
                    {
                        _dataTableCache.Add(type, retVal);
                    }
                }
            }
            return retVal;
        }

        /// <summary>
        /// Returns a <see cref="DataColumnAttribute"/> for the specified <see cref="PropertyInfo"/> object.
        /// </summary>
        /// <param name="property">The <see cref="PropertyInfo"/> to interrogate.</param>
        public static DataColumnAttribute GetDataColumnAttribute(PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }
            DataColumnAttribute[] dataColumns = property.GetCustomAttributes(typeof(DataColumnAttribute), true) as DataColumnAttribute[];
            if (dataColumns.Length == 0)
            {
                throw new ArgumentException(string.Format(ErrorStrings.MissingColumnAttributeException, property.Name));
            }
            return dataColumns[0];
        }

        /// <summary>
        /// Returns an array of <see cref="DataRelationAttribute"/> attributes for the specified <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="property">The <see cref="PropertyInfo"/> object to interrogate.</param>
        public static DataRelationAttribute[] GetDataRelationAttributes(PropertyInfo property)
        {
            return property.GetCustomAttributes(typeof(DataRelationAttribute), true) as DataRelationAttribute[];
        }

        public static bool TryGetStoragePropertyAttribute(Type type, string propertyName, out PropertyStorageAttribute attribute)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null)
            {
                PropertyStorageAttribute[] storageProperties = property.GetCustomAttributes(typeof(PropertyStorageAttribute), true) as PropertyStorageAttribute[];
                if (storageProperties.Length > 0)
                {
                    attribute = storageProperties[0];
                    return true;
                }
            }
            attribute = null;
            return false;
        }

        public static bool TryGetDataColumnAttribute(Type type, string propertyName, out DataColumnAttribute attribute)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null)
            {
                DataColumnAttribute[] attrs = property.GetCustomAttributes(typeof(DataColumnAttribute), true) as DataColumnAttribute[];
                if (attrs.Length > 0)
                {
                    attribute = attrs[0];
                    return true;
                }
            }
            attribute = null;
            return false;
        }

        /// <summary>
        /// Gets a list of <see cref="DataColumnAttribute"/> from the type using the internal cache.
        /// </summary>
        /// <param name="type">The type to interrogate.</param>
        public static List<DataColumnAttribute> GetDataColumnAttributes(Type type)
        {
            List<DataColumnAttribute> retval = new List<DataColumnAttribute>();
            lock (_syncLock)
            {
                if (_dataColumnCache.ContainsKey(type))
                {
                    retval = ObjectCloner.Clone<List<DataColumnAttribute>>(_dataColumnCache[type]);
                }
                else
                {
                    foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        DataColumnAttribute[] attributes = property.GetCustomAttributes(typeof(DataColumnAttribute), true) as DataColumnAttribute[];
                        if (attributes != null && attributes.Length > 0)
                        {
                            attributes[0].MappedProperty = property;
                            retval.Add(attributes[0]);
                        }
                    }
                    _dataColumnCache.Add(type, retval);
                }
            }
            return retval;
        }

        public static List<DataColumnAttribute> GetDataColumnAttributes(Type type, HashSet<string> properties)
        {
            List<DataColumnAttribute> retval = new List<DataColumnAttribute>();
            foreach (string item in properties)
            {
                PropertyInfo propertyInfo = type.GetProperty(item, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (propertyInfo != null)
                {
					DataColumnAttribute[] attrs = propertyInfo.GetCustomAttributes(typeof(DataColumnAttribute), true) as DataColumnAttribute[];
					if (attrs != null && attrs.Length > 0)
					{
						DataColumnAttribute attr = attrs[0];
						attr.MappedProperty = propertyInfo;
						retval.Add(attr);
					}
                }
            }
            return retval;
        }

        /// <summary>
        /// Gets <see cref="DataColumnAttribute"/> representing an auto-incremented field.
        /// </summary>
        /// <param name="type">The type to interrogate.</param>
        public static DataColumnAttribute GetAutoIncrementDataColumn(Type type)
        {
            DataColumnAttribute autoIncrement = null;
            foreach (DataColumnAttribute dataColumn in DataAttributeUtilities.GetDataColumnAttributes(type))
            {
                if (dataColumn.AutoIncrement == true)
                {
                    autoIncrement = dataColumn;
                    break;
                }
            }
            return autoIncrement;
        }

        /// <summary>
        /// Gets a list <see cref="DataColumnAttribute"/> representing key indentifiers from the type using the internal cache.
        /// </summary>
        /// <param name="type">The type to interrogate.</param>
        public static List<DataColumnAttribute> GetDataColumnIdentifiers(Type type)
        {
            List<DataColumnAttribute> identifiers = new List<DataColumnAttribute>();
            lock (_syncLock)
            {
                if (_identifierCache.ContainsKey(type))
                {
                    identifiers = ObjectCloner.Clone<List<DataColumnAttribute>>(_identifierCache[type]);
                }
                else
                {
                    foreach (DataColumnAttribute dataColumn in DataAttributeUtilities.GetDataColumnAttributes(type))
                    {
                        if (dataColumn.Identifier)
                        {
                            identifiers.Add(dataColumn);
                        }
                    }
                    _identifierCache.Add(type, identifiers);
                }
            }
            if (identifiers.Count == 0)
            {
                throw new InvalidOperationException(string.Format("The supplied type {0} must contain at least one property adorned with a {1} where Identifier is equal to true.", type.FullName, typeof(DataColumnAttribute).FullName));
            }
            return identifiers;
        }
	}
}
