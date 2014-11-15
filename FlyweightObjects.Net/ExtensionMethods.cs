//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
//  Author:         Marcus Crane, Software Developer / Architect                                        //
//  E-mail:         mailto:marcus_crane@FlyweightObjects                                                //
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
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Data;
using System.Linq.Expressions;
using System.Xml;
using System.Diagnostics;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents a compendium of static extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        private static readonly string EncryptionKey = "A190B45E1E4D4DACA1ABCFDA";

        /// <summary>
        /// Returns the default contstructor for the given type if one exists.
        /// </summary>
        /// <param name="source">The type of to interrogate.</param>
        internal static ConstructorInfo GetDefaultConstructor(this Type source)
        {
            ConstructorInfo ctor = null;
            if (source.IsClass)
            {
                ctor = source.GetConstructor(new Type[] { });
            }
            return ctor;
        }

        /// <summary>
        /// Returns a boolean value indicating whether the type contains a default constructor.
        /// </summary>
        /// <param name="source">The type of to interrogate.</param>
        internal static bool HasDefaultConstructor(this Type source)
        {
            return source.GetDefaultConstructor() != null;
        }

        /// <summary>
        /// Returns a boolean indicating whether or not the source can be enumerated.
        /// </summary>
        /// <param name="source">The type of to interrogate.</param>
        public static bool IsEnumerable(this Type source)
        {
            return typeof(IEnumerable).IsAssignableFrom(source);
        }

        /// <summary>
        /// Returns a boolean value indicating whether the supplied type is a generic list.
        /// </summary>
        /// <param name="source">The type of to interrogate.</param>
        internal static bool IsGenericList(this Type source)
        {
            return typeof(IList).IsAssignableFrom(source) && source.IsGenericType && source.GetGenericArguments().Length > 0;
        }

        /// <summary>
        /// Returns a boolean value indicating whether the supplied type's base class is a generic list.
        /// </summary>
        /// <param name="source">The type of to interrogate.</param>
        internal static bool IsGenericListDerived(this Type source)
        {
            return typeof(IList).IsAssignableFrom(source) && source.BaseType.IsGenericType && source.BaseType.IsEnumerable();
        }

        /// <summary>
        /// Returns a type that represents the generic argument of the generic class instance.
        /// </summary>
        /// <param name="source">The type of to interrogate.</param>
        public static Type GetGenericTypeParameter(this Type source)
        {
            return source.GetGenericArguments().Length > 0 ? source.GetGenericArguments()[0] : source;
        }

        /// <summary>
        /// Returns a boolean value indicating whether the type can be cached.
        /// </summary>
        /// <param name="source">The type of to interrogate.</param>
        internal static bool IsCacheable(this Type source)
        {
            int cacheTimeOut = 0;
            return source.IsCacheable(out cacheTimeOut);
        }

        /// <summary>
        /// Returns a boolean value indicating whether the type can be cached, and if it can, the associated time out value.
        /// </summary>
        /// <param name="source">The type of to interrogate.</param>
        /// <param name="cacheTimeOut">The time out value for the type.</param>
        internal static bool IsCacheable(this Type source, out int cacheTimeOut)
        {
            cacheTimeOut = 0;
            DataTableAttribute[] dataTableAttrs = source.GetCustomAttributes(typeof(DataTableAttribute), true) as DataTableAttribute[];
            if (dataTableAttrs.Length > 0 && dataTableAttrs[0].EnableCaching)
            {
                cacheTimeOut = dataTableAttrs[0].CacheTimeout;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the type exposed by the <see cref="MemberInfo"/> subtype implementation.
        /// </summary>
        /// <param name="member">The <see cref="MemberInfo"/> object.</param>
        internal static Type GetMemberInfoType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException(ErrorStrings.InvalidMemberInfoExtensionArgument);
            }
        }

        /// <summary>
        /// Returns a <see cref="TransactionType"/> enum value based upon the parsed <see cref="IStorageCommand"/> SqlText property.
        /// </summary>
        /// <param name="source">The <see cref="IStorageCommand"/> object to interrogate.</param>
        public static TransactionType GetTransactionType(this IStorageCommand source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            string sql = string.Format(" {0}", source.SqlText.ToUpper());
            if (sql.Contains(" INSERT INTO "))
            {
                return TransactionType.Insert;
            }
            if (sql.ToUpper().Contains(" UPDATE "))
            {
                return TransactionType.Update;
            }
            if (sql.ToUpper().Contains(" DELETE "))
            {
                return TransactionType.Delete;
            }
            if (sql.ToUpper().Contains(" SELECT "))
            {
                return TransactionType.Select;
            }
            if (sql.ToUpper().Contains(" TRUNCATE "))
            {
                return TransactionType.Truncate;
            }
            return TransactionType.Unknown;
        }

        /// <summary>
        /// Returns the first instance in source or T's default value if source is empty.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="source">An instance of the type parameter.</param>
        public static T ToSingle<T>(this IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            return source.FirstOrDefault<T>();
        }

        /// <summary>
        /// Converts an <see cref="IEnumerable{T}"/> to an instance of a <see cref="FlyweightSet{T}"/>.
        /// </summary>
        /// <typeparam name="T">the type parameter of the object.</typeparam>
        /// <param name="source">The source list of objects to convert.</param>
        public static FlyweightSet<T> ToFlyweightSet<T>(this IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            if (source is FlyweightSet<T>)
            {
                return source as FlyweightSet<T>;
            }
            if (source == null || source.Count() == 0)
            {
                return new FlyweightSet<T>();
            }
            else
            {
                IFlyweight flyweight = source.ElementAt(0);
                ResultSet sourceResultSet = ((IPropertyStorage)flyweight.Storage).DataSource.ResultSet;
                ResultSet targetResultSet = new ResultSet(sourceResultSet.Columns);
                FlyweightSet<T> collection = new FlyweightSet<T>(targetResultSet);
                foreach (var item in source)
                {
                    collection.Add(item);
                }
                return collection;
            }
        }

        /// <summary>
        /// Converts source to a new <see cref="Collection{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="source">An instance of the type parameter.</param>
        public static Collection<T> ToCollection<T>(this IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            return new Collection<T>(source.ToList<T>());
        }

        /// <summary>
        /// Converts source to a new <see cref="BindingList{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="source">An instance of the type parameter.</param>
        public static BindingList<T> ToBindingList<T>(this IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            return new BindingList<T>(source.ToList<T>());
        }

        /// <summary>
        /// Converts source to a new <see cref="Queue{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="source">An instance of the type parameter.</param>
        public static Queue<T> ToQueue<T>(this IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            return new Queue<T>(source);
        }

        /// <summary>
        /// Converts source to a new <see cref="Stack{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="source">An instance of the type parameter.</param>
        public static Stack<T> ToStack<T>(this IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            return new Stack<T>(source);
        }

        /// <summary>
        /// Returns a boolean indicating whether the provided string can be interpreted as a number.
        /// </summary>
        /// <param name="source">The string to interrogate.</param>
        public static bool IsNumeric(this string source)
        {
            double number = 0D;
            return double.TryParse(source, out number);
        }

        /// <summary>
        /// Gets the entire <see cref="Stream"/> object as an array of bytes.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        public static byte[] GetBytes(this Stream stream)
        {
            byte[] buffer = new byte[4096];
            using (MemoryStream ms = new MemoryStream())
            {
                int bytesRead = 0;
                do
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        ms.Write(buffer, 0, bytesRead);
                    }
                } while (bytesRead > 0);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Compresses the byte array using Gzip.
        /// </summary>
        /// <param name="bytes">The byte array to compress.</param>
        public static byte[] Compress(this byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    gzip.Write(bytes, 0, bytes.Length);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Decompresses the byte array using Gzip.
        /// </summary>
        /// <param name="bytes">The byte array to decomress.</param>
        public static byte[] Decompress(this byte[] bytes)
        {
            using (GZipStream gzip = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress))
            {
                return gzip.GetBytes();
            }
        }

        /// <summary>
        /// Determines if an object has been changed.
        /// </summary>
        /// <typeparam name="T">The type of object to interrogate.</typeparam>
        /// <param name="source">An instance of T.</param>
        public static bool IsChanged<T>(this T source) where T : class, IFlyweight, new()
        {
            if (source == null)
            {
                return false;
            }
            IRedundancyCheck obj = source as IRedundancyCheck;
            if (obj != null)
            {
                if (string.IsNullOrEmpty(obj.Checksum))
                {
                    return false;
                }
                return new ChecksumBuilder().BuildChecksum(obj) != obj.Checksum;
            }
            return source.Storage.IsChanged;
        }

        /// <summary>
        /// Determines whether or not the provided object is changed.
        /// </summary>
        /// <param name="source">The <see cref="IEnumerable{T}"/> object to interrogate.</param>
        public static bool IsChanged<T>(this IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            IChangeTrackable<T> obj1 = source as IChangeTrackable<T>;
            if (obj1 != null && obj1.DeletedItems.Count > 0)
            {
                return true;
            }
            foreach (var item in source)
            {
                if (item.IsChanged<T>())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Encrypts the byte array using the <see cref="TripleDESCryptoServiceProvider"/> provider.
        /// </summary>
        /// <param name="bytes">The byte array to encrypt.</param>
        /// <param name="key">A valid <see cref="TripleDES.Key"/> value.</param>
        public static byte[] Encrypt(this byte[] bytes, byte[] key)
        {
            if (key == null || key.Length != 24)
            {
                throw new ArgumentException(ErrorStrings.InvalidEncryptionKeyLength);
            }
            using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
            {
                tdes.Key = key;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;
                ICryptoTransform transfrom = tdes.CreateEncryptor();
                return transfrom.TransformFinalBlock(bytes, 0, bytes.Length);
            }
        }

        /// <summary>
        /// Encrypts the byte array using the <see cref="TripleDESCryptoServiceProvider"/> provider.
        /// </summary>
        /// <param name="bytes">The byte array to encrypt.</param>
        internal static byte[] Encrypt(this byte[] bytes)
        {
            return bytes.Encrypt(UTF8Encoding.UTF8.GetBytes(EncryptionKey));
        }

        /// <summary>
        /// Decryptes the byte array using the <see cref="TripleDESCryptoServiceProvider"/> provider.
        /// </summary>
        /// <param name="bytes">The byte array to decrypt.</param>
        /// <param name="key">A valid <see cref="TripleDES.Key"/> value.</param>
        public static byte[] Decrypt(this byte[] bytes, byte[] key)
        {
            if (key == null || key.Length != 24)
            {
                throw new ArgumentException(ErrorStrings.InvalidEncryptionKeyLength);
            }
            using (TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider())
            {
                tdes.Key = key;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;
                ICryptoTransform transform = tdes.CreateDecryptor();
                return transform.TransformFinalBlock(bytes, 0, bytes.Length);
            }
        }

        /// <summary>
        /// Returns an MD5 hash of the byte array using the <see cref="MD5CryptoServiceProvider"/>.
        /// </summary>
        /// <param name="bytes">The byte array to hash.</param>
        internal static string ToMD5(this byte[] bytes)
        {
            using (MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider())
            {
                byte[] hash = hashProvider.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte hashByte in hash)
                {
                    sb.Append(string.Format("{0:X1}", hashByte));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Decryptes the byte array using the <see cref="TripleDESCryptoServiceProvider"/> provider.
        /// </summary>
        /// <param name="bytes">The byte array to decrypt.</param>
        internal static byte[] Decrypt(this byte[] bytes)
        {
            return bytes.Decrypt(UTF8Encoding.UTF8.GetBytes(EncryptionKey));
        }

        /// <summary>
        /// Returns a boolean value indicating whether the current type implements the supplied interface type.
        /// </summary>
        /// <param name="currentType">The type being evaluated.</param>
        /// <param name="interfaceType">The interface type being checked.</param>
        internal static bool IsImplementationOf(this Type currentType, Type interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException("The interfaceType argument must be an interface.");
            }
            return currentType.GetInterface(interfaceType.FullName) != null;
        }

        /// <summary>
        /// Returns a zero-length string if the source is null, otherwise the string trimmed.
        /// </summary>
        /// <param name="source">The string to be trimmed.</param>
        public static string TrimNull(this string source)
        {
            if (source == null)
                return string.Empty;
            else
                return source.Trim();
        }

		/// <summary>
		/// Performs the specified action on each element in the <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="T">The type of objects contained in the collection.</typeparam>
		/// <param name="source">The collection of objects to iterate.</param>
		/// <param name="action">The <see cref="Action{T}"/> delegate to perform on each element.</param>
		public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) where T : class, IFlyweight, new()
		{
			if (source is FlyweightSet<T>)
			{
				((FlyweightSet<T>)source).ForEach(action);
			}
			else
			{
				source.ToList<T>().ForEach(action);
			}
		}

		/// <summary>
		/// Performs an ascending sort operation on the supplied <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="T">The type of objects contained in the collection.</typeparam>
		/// <param name="source">The collection of objects to sort.</param>
		/// <param name="expression">An expression representing the property name by which to sort.</param>
		public static IEnumerable<T> Sort<T>(this IEnumerable<T> source, Expression<Func<T, object>> expression)
		{
			return source.Sort(expression, ListSortDirection.Ascending);
		}

		/// <summary>
		/// Performs a sort operation on the supplied <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="T">The type of objects contained in the collection.</typeparam>
		/// <param name="source">The collection of objects to sort.</param>
		/// <param name="expression">An expression representing the property name by which to sort.</param>
		/// <param name="direction">The direction by which to sort.</param>
		public static IEnumerable<T> Sort<T>(this IEnumerable<T> source, Expression<Func<T, object>> expression, ListSortDirection direction)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			string propertyName = PropertyOf<T>.Name(expression);
			return source.Sort(propertyName, direction);
		}

		/// <summary>
		/// Performs an ascending sort operation on the supplied <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="T">The type of objects contained in the collection.</typeparam>
		/// <param name="source">The collection of objects to sort.</param>
		/// <param name="propertyName">The property name by which to sort.</param>
		public static IEnumerable<T> Sort<T>(this IEnumerable<T> source, string propertyName)
		{
			return source.Sort(propertyName, ListSortDirection.Ascending);
		}

		/// <summary>
		/// Performs a sort operation on the supplied <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <typeparam name="T">The type of objects contained in the collection.</typeparam>
		/// <param name="source">The collection of objects to sort.</param>
		/// <param name="propertyName">The property name by which to sort.</param>
		/// <param name="direction">The direction by which to sort.</param>
		public static IEnumerable<T> Sort<T>(this IEnumerable<T> source, string propertyName, ListSortDirection direction)
		{
			if (propertyName == null)
			{
				throw new ArgumentNullException(propertyName);
			}
			PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (propertyInfo == null)
			{
				throw new InvalidOperationException(string.Format("The property name {0} could not be found in the class {1}", propertyName, typeof(T).FullName));
			}
			if (!(propertyInfo.PropertyType == typeof(string) || propertyInfo.PropertyType.IsValueType))
			{
				throw new InvalidOperationException("The property type specified can only be a value type or string.");
			}
			List<T> list = source as List<T>;
			if (list == null)
			{
				list = new List<T>(source);
			}
			if (typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType))
			{
				list.Sort(delegate(T objA, T objB)
				{
					var a = (IComparable)propertyInfo.GetValue(objA, null);
					var b = (IComparable)propertyInfo.GetValue(objB, null);
					if (direction == ListSortDirection.Ascending)
					{
						if (a != null)
							return a.CompareTo(b);
						else
							return 0;
					}
					else
					{
						if (b != null)
							return b.CompareTo(a);
						else
							return 1;
					}
				});
			}
			else
			{
				list.Sort(delegate(T objA, T objB)
				{
					var a = propertyInfo.GetValue(objA, null).ToString();
					var b = propertyInfo.GetValue(objB, null).ToString();
					if (direction == ListSortDirection.Ascending)
					{
						if (a != null)
							return a.CompareTo(b);
						else
							return a.CompareTo(b);
					}
					else
					{
						if (b != null)
							return b.CompareTo(a);
						else
							return b.CompareTo(a);
					}
				});
			}
			return list;
		}

		/// <summary>
		/// Returns a string with the removed characters found in the supplied character array.
		/// </summary>
		/// <param name="source">The string to search.</param>
		/// <param name="chars">The characters to remove.</param>
		public static string Remove(this string source, char[] chars)
		{
			string retval = source;
			foreach (char chr in chars)
			{
				retval = retval.Replace(chr.ToString(), string.Empty);
			}
			return retval;
		}

		/// <summary>
		/// Gets the value held in local <see cref="PropertyStorage"/> for the specified index.
		/// </summary>
		/// <typeparam name="T">The type of the value to get.</typeparam>
		/// <param name="source">An instance of an <see cref="IFlyweight"/>.</param>
		/// <param name="index">The property or field name held in <see cref="PropertyStorage"/>.</param>
		public static T GetValue<T>(this IFlyweight source, string index)
		{
            return source.Storage.GetProperty<T>(index);
		}

        /// <summary>
        /// If it can be found, returns the value for index, otherwise the default of T.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="source">The <see cref="IFlyweight"/> to interrogate.</param>
        /// <param name="index">The property or field name held in <see cref="PropertyStorage"/>.</param>
        public static T GetValueOrDefault<T>(this IFlyweight source, string index)
        {
            if (source.Storage.PropertyExists(index))
            {
                return source.Storage.GetProperty<T>(index);
            }
            return default(T);
        }

		/// <summary>
		/// Sets the value held in local <see cref="PropertyStorage"/> for the specified index.
		/// </summary>
		/// <typeparam name="T">The type of the value to set.</typeparam>
		/// <param name="source">An instance of an <see cref="IFlyweight"/>.</param>
		/// <param name="index">The property or field name held in <see cref="PropertyStorage"/>.</param>
		/// <param name="value">The value to set.</param>
		public static void SetValue<T>(this IFlyweight source, string index, T value)
		{
            source.Storage.SetProperty<T>(index, value);
		}

        /// <summary>
        /// Returns the inner most <see cref="Exception"/> for the provided exception.
        /// </summary>
        /// <param name="source">The <see cref="Exception"/> to interrogate.</param>
        public static Exception ToRoot(this Exception source)
        {
            Exception ex = source;
            while (ex != null && ex.InnerException != null)
            {
                ex = ex.InnerException;
            }
            return ex;
        }

        /// <summary>
        /// Performs conversions between compatible reference types. If the cast is not successful, 
        /// null will be returned.
        /// </summary>
        /// <typeparam name="T">The type to cast to.</typeparam>
        /// <param name="source">The <see cref="object"/> to cast.</param>
        public static T AsType<T>(this object source) where T : class
        {
            return source as T;
        }

		/// <summary>
		/// Returns true if the object was successfully casted to the type of T, otherwise false. If successful,
		/// the supplied out parameter will be set to the casted type, otherwise its default will be set.
		/// </summary>
		/// <typeparam name="T">The type to cast to.</typeparam>
		/// <param name="source">The <see cref="object"/> to cast.</param>
		/// <param name="value">A out parameter to set for the casted type.</param>
		public static bool TryCast<T>(this object source, out T value)
		{
            value = default(T);
            if (source != null && typeof(T).IsAssignableFrom(source.GetType()))
            {
                value = (T)source;
                return true;
            }
			return false;
		}

        /// <summary>
        /// Determines whether the values of the two objects are the same for all of its members, public or otherwise.
        /// </summary>
        /// <typeparam name="T">The instance to compare.</typeparam>
        /// <param name="source">The source object to compare.</param>
        /// <param name="target">The target object to compare.</param>
        public static bool EqualTo<T>(this T source, T target) where T : class, IFlyweight, IRedundancyCheck
        {
            ChecksumBuilder cb = new ChecksumBuilder();
            string sourceVal = cb.BuildChecksum(source);
            string targetVal = cb.BuildChecksum(target);
            return sourceVal == targetVal;
        }

        internal static bool IsInitialized(this IStorageCommand command)
        {
            return !string.IsNullOrEmpty(command.SqlText);
        }

        internal static bool IsNullable(this Type source)
        {
            return source.IsClass || (source.IsGenericType && source.GetGenericTypeDefinition() == typeof(Nullable<>)) ;
        }

        internal static object Default(this Type source)
        {
            object retVal = null;
            if (source.IsNullable())
            {
                retVal = null;
            }
            else
            {
                switch (source.GetGenericTypeParameter().Name)
                {
                    case "Byte":
                        retVal = default(Byte);
                        break;
                    case "SByte":
                        retVal = default(SByte);
                        break;
                    case "Int16":
                        retVal = default(Int16);
                        break;
                    case "Int32":
                        retVal = default(Int32);
                        break;
                    case "Int64":
                        retVal = default(Int64);
                        break;
                    case "UInt16":
                        retVal = default(UInt16);
                        break;
                    case "UInt32":
                        retVal = default(UInt32);
                        break;
                    case "UInt64":
                        retVal = default(UInt64);
                        break;
                    case "Single":
                        retVal = default(Single);
                        break;
                    case "Double":
                        retVal = default(Double);
                        break;
                    case "Boolean":
                        retVal = default(Boolean);
                        break;
                    case "Char":
                        retVal = default(Char);
                        break;
                    case "Decimal":
                        retVal = default(Decimal);
                        break;
                    case "DateTime":
                        retVal = default(DateTime);
                        break;
                    case "String":
                        retVal = default(String);
                        break;
                    case "Guid":
                        retVal = default(Guid);
                        break;
                    default:
                        break;
                }
            }
            return retVal;
        }

		/// <summary>
		/// Executes the query given an instance of a <see cref="DataContext"/>.
		/// </summary>
		/// <typeparam name="T">The type to return.</typeparam>
		/// <param name="source">An instance of an <see cref="IQueryExpression{T}"/>.</param>
		/// <param name="context">An instance of a <see cref="DataContext"/>.</param>
		/// <returns></returns>
		public static IEnumerable<T> Execute<T>(this IQueryExpression<T> source, IDataContext context)
			where T : class , IFlyweight, new()
		{
			if (ReferenceEquals(context, null))
				throw new ArgumentNullException("context");
			if (ReferenceEquals(source, null))
				throw new ArgumentNullException("context");
			return context.Select<T>(source);
		}
    }
}
