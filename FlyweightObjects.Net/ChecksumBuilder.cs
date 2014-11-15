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
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Diagnostics;

namespace FlyweightObjects
{
    /// <summary>
    /// Creates and evaluates checksums as an MD5 hash.
    /// </summary>
    /// <remarks>
    /// The ChecksumBuilder class creates an MD5 hash of the object based upon the values of its members which have been adorned with a <see cref="DataColumnAttribute"/>.
    /// When a class implements <see cref="IRedundancyCheck"/>, the <see cref="DataContext"/> automatically computes the checksum before returning the object. An 
    /// object can be aware of whether or not it has changes by simply comparing a new computed checksum against the original one given by the <see cref="DataContext"/>. 
    /// Note that if an object uses the ChecksumBuilder in a public property, you must mark the property with a <see cref="ComputedAttribute"/> to ensure that an infinite loop
    /// is not encoutered when the hash is created.
    /// </remarks>
    /// <example>
    /// The following example uses a manually created MyProduct class, abstracting Product from the Microsoft AdventureWorks SQL Server sample database. When
    /// the ModifiedDate property is changed, the IsChanged property will change its state from false to true because the newly calculated checksum is now different
    /// than the one supplied by the <see cref="DataContext"/>.
    /// <code>
    /// <![CDATA[
	/// [DataTable("Production.Product", EnableCaching=true, CacheTimeout=300)]
	/// public class MyProduct : IRedundancyCheck
	/// {
	///     [DataColumn("ProductID", Identifier = true)]
	///     public int ProductID { get; set; }
	///     
	///     [DataColumn("ModifiedDate")]
	///     public DateTime ModifiedDate { get; set; }
	///     
	///		[PropertyTorage]
	///     public string Checksum { get; set; }
	///     
	///		[Computed]
	///     public bool IsChanged
	///     {
	///         get { return new ChecksumBuilder().BuildChecksum(this) != this.Checksum; }
	///     }
	/// }
    ///
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
    ///         {
    ///             var p = context.Select<MyProduct>(1).ToSingle();
    ///             Console.WriteLine("Is Changed: {0}", p.IsChanged);
    ///
    ///             p.ModifiedDate = DateTime.Now;
    ///             Console.WriteLine("Is Changed: {0}", p.IsChanged);
    ///         }
    ///    }
    ///}
    /// ]]>
    /// </code>
    /// </example> 
    public class ChecksumBuilder
    {
        private static Dictionary<Type, PropertyInfo[]> _propertyInfoCache = new Dictionary<Type, PropertyInfo[]>();

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        public ChecksumBuilder() { }

        /// <summary>
        /// Creates a new MD5 hash from the supplied object.
        /// </summary>
        /// <param name="obj">The object from which to compute the checksum.</param>
        public string BuildChecksum(object obj)
        {
            StringBuilder values = new StringBuilder();
            if (obj != null)
            {
                foreach (PropertyInfo property in GetSortedPropertyInfos(obj.GetType()))
                {
                    object memberVal = property.GetValue(obj, null);
                    if (typeof(ValueType).IsAssignableFrom(property.GetMemberInfoType()) || typeof(string).IsAssignableFrom(property.GetMemberInfoType()))
                    {
                        values.Append(memberVal == null ? "/0" : memberVal.ToString());
                    }
                    else if (property.GetMemberInfoType().IsArray)
                    {
                        if (memberVal != null)
                        {
                            IEnumerator enumerator = ((IEnumerable)memberVal).GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                if (typeof(ValueType).IsAssignableFrom(enumerator.Current.GetType()) || typeof(string).IsAssignableFrom(enumerator.Current.GetType()))
                                {
                                    values.Append(enumerator.Current == null ? "/0" : enumerator.Current.ToString());
                                }
                            }
                        }
                        else
                        {
                            values.Append("/0");
                        }
                    }
                }
            }
            return ComputeHash(values.ToString());
        }

        /// <summary>
        /// Determines if two checksum strings are equal.
        /// </summary>
        /// <param name="checksum1">The first checksum value.</param>
        /// <param name="checksum2">The second checksum value.</param>
        public bool EqualsChecksum(string checksum1, string checksum2)
        {
            return checksum1.Equals(checksum2);
        }

        /// <summary>
        /// Returns the object's <see cref="PropertyInfo"/> array either from cache or from the type itself. If the <see cref="PropertyInfo"/> array 
        /// is not found in the cache, then it is added after it has been sorted.
        /// </summary>
        /// <param name="type">The type to interrogate for its properties.</param>
        /// <returns>A sorted <see cref="PropertyInfo"/> array.</returns>
        private PropertyInfo[] GetSortedPropertyInfos(Type type)
        {
            PropertyInfo[] propertyInfos = null;
            if (_propertyInfoCache.ContainsKey(type))
            {
                propertyInfos = _propertyInfoCache[type];
            }
            else
            {
                List<PropertyInfo> properties = new List<PropertyInfo>(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                for (int i = properties.Count - 1; i >= 0; i--)
                {
                    if (properties[i].GetCustomAttributes(typeof(DataColumnAttribute), true).Length == 0 || properties[i].GetCustomAttributes(typeof(ComputedAttribute), true).Length > 0)
                    {
                        properties.Remove(properties[i]);
                    }
                }
                properties.Sort(delegate(PropertyInfo objA, PropertyInfo objB)
                {
                    return objA.Name.CompareTo(objB.Name);
                });
                propertyInfos = properties.ToArray();
                lock (_propertyInfoCache)
                {
                    if (!_propertyInfoCache.ContainsKey(type))
                    {
                        _propertyInfoCache.Add(type, propertyInfos);
                    }
                }
            }
            return propertyInfos;
        }

        /// <summary>
        /// Computes a hash using an MD5 algorithm and converts it to a hexidicimal string. This method allows very
        /// large objects to be hashed using a very small string-based check sum value.
        /// </summary>
        /// <param name="memberValues">An appended string interpretation of the object's members.</param>
        /// <returns>A hexidecimal string hashed value.</returns>
        protected internal string ComputeHash(string memberValues)
        {
            using (MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider())
            {
                StringBuilder sb = new StringBuilder();
                UnicodeEncoding encoding = new UnicodeEncoding();
                byte[] hash = hashProvider.ComputeHash(encoding.GetBytes(memberValues));
                foreach (byte hashByte in hash)
                {
                    sb.Append(string.Format("{0:X1}", hashByte));
                }
                return sb.ToString();
            } 
        }
    }
}

