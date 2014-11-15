//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
//  Author:         Marcus Crane, Software Developer / Architect                                        //
//  E-mail:         mailto:support@FlyweightObjects.NET                                                 //
//  Company:        FlyweightObjects.NET, LLC                                                           //
//  Copyright:      Copyright © FlyweightObjects.NET 2009, All rights reserved.                         //
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
using System.Collections;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents a class which can serve as a data source for an object.
    /// </summary>
    internal sealed class DataSource : IDataSource
    {
        private List<DataSource.FieldInfo> Fields { get; set; }
        private IDataRecord Record { get; set; }
        private List<FieldInfo> DynamicFields { get; set; }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="reader">An open <see cref="IDataReader"/> implementation.</param>
        public DataSource(IDataReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (reader.IsClosed)
            {
                throw new ArgumentException(string.Format("Cannot create a new instance of a {0} using a closed {1}.", this.GetType().FullName, reader.GetType().FullName));
            }
            this.Record = reader;
            this.Fields = new List<DataSource.FieldInfo>();
            for (int index = 0; index < reader.FieldCount; index++)
            {
                this.Fields.Add(new DataSource.FieldInfo(reader.GetName(index), index, reader.GetFieldType(index)));
            }
        }

        /// <summary>
        /// Returns a Boolean value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a Boolean will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        public bool GetBoolean(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            bool retVal = default(bool);
            this.TryGetBoolean(fieldName, out retVal);
            return retVal;
        }

        /// <summary>
        /// Returns a Boolean value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public bool GetBoolean(string fieldName, bool defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            bool value = default(bool);
            if (this.TryGetBoolean(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a nullable Boolean value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public bool? GetBoolean(string fieldName, bool? defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            bool value = default(bool);
            if (this.TryGetBoolean(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        public bool TryGetBoolean(string fieldName, out bool value)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            bool retVal = false;
            value = default(bool);
            FieldInfo field = this.Fields.Find(p => p.FieldName == fieldName);
            if (field != null && !this.Record.IsDBNull(field.Ordinal))
            {
                if (field.FieldType == typeof(Boolean))
                {
                    value = this.Record.GetBoolean(field.Ordinal);
                    retVal = true;
                }
                else
                {
                    retVal = bool.TryParse(this.Record.GetValue(field.Ordinal).ToString(), out value);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Returns a Byte value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a Boolean will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        public byte GetByte(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            byte retVal = default(byte);
            this.TryGetByte(fieldName, out retVal);
            return retVal;
        }

        /// <summary>
        /// Returns a Byte value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public byte GetByte(string fieldName, byte defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            byte value = default(byte);
            if (this.TryGetByte(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a nullable Byte value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public byte? GetByte(string fieldName, byte? defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            byte value = default(byte);
            if (this.TryGetByte(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        public bool TryGetByte(string fieldName, out byte value)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            bool retVal = false;
            value = default(byte);
            FieldInfo field = this.Fields.Find(p => p.FieldName == fieldName);
            if (field != null && !this.Record.IsDBNull(field.Ordinal))
            {
                if (field.FieldType == typeof(bool))
                {
                    value = this.Record.GetByte(field.Ordinal);
                    retVal = true;
                }
                else
                {
                    retVal = byte.TryParse(this.Record.GetValue(field.Ordinal).ToString(), out value);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Returns a byte array value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a byte array will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        public byte[] GetBytes(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            byte[] retVal = default(byte[]);
            FieldInfo field = this.Fields.Find(p => p.FieldName == fieldName);
            if (field != null && !this.Record.IsDBNull(field.Ordinal))
            {
                if (field.FieldType == typeof(byte[]))
                {
                    retVal = this.Record.GetValue(field.Ordinal) as byte[];
                }
            }
            return retVal;
        }

        /// <summary>
        /// Reads a stream of bytes from the specified column offset into the buffer as an array, starting at the given buffer offset.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="fieldOffset">The index within the field from which to start the read operation.</param>
        /// <param name="buffer">The buffer into which to read the stream of bytes.</param>
        /// <param name="bufferOffSet">The index within the field from which to start the read operation.</param>
        /// <param name="length">The number of bytes to read. </param>
        public long GetBytes(string fieldName, long fieldOffset, byte[] buffer, int bufferOffSet, int length)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            long retVal = default(long);
            FieldInfo field = this.Fields.Find(p => p.FieldName == fieldName);
            if (field != null && !this.Record.IsDBNull(field.Ordinal))
            {
                if (field.FieldType == typeof(byte[]))
                {
                    retVal = this.Record.GetBytes(field.Ordinal, fieldOffset, buffer, bufferOffSet, length);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Returns a Char value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a Boolean will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        public char GetChar(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            char retVal = default(char);
            this.TryGetChar(fieldName, out retVal);
            return retVal;
        }

        /// <summary>
        /// Returns a Char value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public char GetChar(string fieldName, char defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            char value = default(char);
            if (this.TryGetChar(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a nullable Char value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public char? GetChar(string fieldName, char? defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            char value = default(char);
            if (this.TryGetChar(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        public bool TryGetChar(string fieldName, out char value)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            bool retVal = false;
            value = default(char);
            FieldInfo field = this.Fields.Find(p => p.FieldName == fieldName);
            if (field != null && !this.Record.IsDBNull(field.Ordinal))
            {
                if (field.FieldType == typeof(char))
                {
                    value = this.Record.GetChar(field.Ordinal);
                    retVal = true;
                }
                else
                {
                    retVal = char.TryParse(this.Record.GetValue(field.Ordinal).ToString(), out value);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Returns a Char array value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a char array will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        public char[] GetChars(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            char[] retVal = default(char[]);
            FieldInfo field = this.Fields.Find(p => p.FieldName == fieldName);
            if (field != null && !this.Record.IsDBNull(field.Ordinal))
            {
                if (field.FieldType == typeof(char[]))
                {
                    retVal = this.Record.GetValue(field.Ordinal) as char[];
                }
            }
            return retVal;
        }

        /// <summary>
        /// Reads a stream of characters from the specified column offset into the buffer as an array, starting at the given buffer offset.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="fieldOffset">The index within the field from which to start the read operation.</param>
        /// <param name="buffer">The buffer into which to read the stream of characters.</param>
        /// <param name="bufferOffSet">The index within the field from which to start the read operation.</param>
        /// <param name="length">The number of characters to read. </param>
        public long GetChars(string fieldName, long fieldOffset, char[] buffer, int bufferOffSet, int length)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            long retVal = default(long);
            FieldInfo field = this.Fields.Find(p => p.FieldName == fieldName);
            if (field != null && !this.Record.IsDBNull(field.Ordinal))
            {
                if (field.FieldType == typeof(char[]))
                {
                    retVal = this.Record.GetChars(field.Ordinal, fieldOffset, buffer, bufferOffSet, length);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Returns a string value from the <see cref="DataSource"/>. If the field name does not exist, the default value for string will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        public string GetString(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            string retVal = default(string);
            this.TryGetString(fieldName, out retVal);
            return retVal;
        }

        /// <summary>
        /// Returns a string value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public string GetString(string fieldName, string defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            string value = default(string);
            if (this.TryGetString(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        public bool TryGetString(string fieldName, out string value)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            bool retVal = false;
            value = default(string);
            FieldInfo field = this.Fields.Find(p => p.FieldName == fieldName);
            if (field != null && !this.Record.IsDBNull(field.Ordinal))
            {
                if (field.FieldType == typeof(string))
                {
                    value = this.Record.GetString(field.Ordinal);
                }
                else
                {
                    value = this.Record.GetValue(field.Ordinal).ToString();
                }
                retVal = true;
            }
            return retVal;
        }

        /// <summary>
        /// Returns a DateTime value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a DateTime will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        public DateTime GetDateTime(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            DateTime retVal = default(DateTime);
            this.TryGetDateTime(fieldName, out retVal);
            return retVal;
        }

        /// <summary>
        /// Returns a DateTime value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public DateTime GetDateTime(string fieldName, DateTime defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            DateTime value = default(DateTime);
            if (this.TryGetDateTime(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a nullable DateTime value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public DateTime? GetDateTime(string fieldName, DateTime? defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            DateTime value = default(DateTime);
            if (this.TryGetDateTime(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        public bool TryGetDateTime(string fieldName, out DateTime value)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            bool retVal = false;
            value = default(DateTime);
            FieldInfo field = this.Fields.Find(p => p.FieldName == fieldName);
            if (field != null && !this.Record.IsDBNull(field.Ordinal))
            {
                if (field.FieldType == typeof(DateTime))
                {
                    value = this.Record.GetDateTime(field.Ordinal);
                    retVal = true;
                }
                else
                {
                    retVal = DateTime.TryParse(this.Record.GetValue(field.Ordinal).ToString(), out value);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Returns a Decimal value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a DateTime will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        public decimal GetDecimal(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            decimal retVal = default(decimal);
            this.TryGetDecimal(fieldName, out retVal);
            return retVal;
        }

        /// <summary>
        /// Returns a Decimal value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public decimal GetDecimal(string fieldName, decimal defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            decimal value = default(decimal);
            if (this.TryGetDecimal(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a nullable Decimal value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public decimal? GetDecimal(string fieldName, decimal? defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            decimal value = default(decimal);
            if (this.TryGetDecimal(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        public bool TryGetDecimal(string fieldName, out decimal value)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            bool retVal = false;
            value = default(decimal);
            FieldInfo field = this.Fields.Find(p => p.FieldName == fieldName);
            if (field != null && !this.Record.IsDBNull(field.Ordinal))
            {
                if (field.FieldType == typeof(decimal))
                {
                    value = this.Record.GetDecimal(field.Ordinal);
                    retVal = true;
                }
                else
                {
                    retVal = decimal.TryParse(this.Record.GetValue(field.Ordinal).ToString(), out value);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Returns a Double value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a DateTime will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        public double GetDouble(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            double retVal = default(double);
            this.TryGetDouble(fieldName, out retVal);
            return retVal;
        }

        /// <summary>
        /// Returns a Double value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public double GetDouble(string fieldName, double defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            double value = default(double);
            if (this.TryGetDouble(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a nullable Double value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public double? GetDouble(string fieldName, double? defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            double value = default(double);
            if (this.TryGetDouble(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        public bool TryGetDouble(string fieldName, out double value)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            bool retVal = false;
            value = default(double);
            FieldInfo field = this.Fields.Find(p => p.FieldName == fieldName);
            if (field != null && !this.Record.IsDBNull(field.Ordinal))
            {
                if (field.FieldType == typeof(double))
                {
                    value = this.Record.GetDouble(field.Ordinal);
                    retVal = true;
                }
                else
                {
                    retVal = double.TryParse(this.Record.GetValue(field.Ordinal).ToString(), out value);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Returns a Single value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a DateTime will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        public float GetFloat(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            float retVal = default(float);
            this.TryGetFloat(fieldName, out retVal);
            return retVal;
        }

        /// <summary>
        /// Returns a Single value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public float GetFloat(string fieldName, float defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            float value = default(float);
            if (this.TryGetFloat(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a nullable Single value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public float? GetFloat(string fieldName, float? defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            float value = default(float);
            if (this.TryGetFloat(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        public bool TryGetFloat(string fieldName, out float value)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            bool retVal = false;
            value = default(float);
            FieldInfo field = this.Fields.Find(p => p.FieldName == fieldName);
            if (field != null && !this.Record.IsDBNull(field.Ordinal))
            {
                if (field.FieldType == typeof(float))
                {
                    value = this.Record.GetFloat(field.Ordinal);
                    retVal = true;
                }
                else
                {
                    retVal = float.TryParse(this.Record.GetValue(field.Ordinal).ToString(), out value);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Returns a Guid value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a Guid will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        public Guid GetGuid(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            Guid retVal = default(Guid);
            FieldInfo field = this.Fields.Find(p => p.FieldName == fieldName);
            if (field != null && !this.Record.IsDBNull(field.Ordinal))
            {
                if (field.FieldType == typeof(Guid))
                {
                    retVal = this.Record.GetGuid(field.Ordinal);
                }
                else
                {
                    object val = this.Record.GetValue(field.Ordinal);
                    retVal = new Guid(val.ToString());
                }
            }
            return retVal;
        }

        /// <summary>
        /// Returns a Int16 value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a DateTime will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        public Int16 GetInt16(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            Int16 retVal = default(Int16);
            this.TryGetInt16(fieldName, out retVal);
            return retVal;
        }

        /// <summary>
        /// Returns a Int16 value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public Int16 GetInt16(string fieldName, Int16 defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            Int16 value = default(Int16);
            if (this.TryGetInt16(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a nullable Int16 value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public Int16? GetInt16(string fieldName, Int16? defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            Int16 value = default(Int16);
            if (this.TryGetInt16(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        public bool TryGetInt16(string fieldName, out Int16 value)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            bool retVal = false;
            value = default(Int16);
            FieldInfo field = this.Fields.Find(p => p.FieldName == fieldName);
            if (field != null && !this.Record.IsDBNull(field.Ordinal))
            {
                if (field.FieldType == typeof(Int16))
                {
                    value = this.Record.GetInt16(field.Ordinal);
                    retVal = true;
                }
                else
                {
                    retVal = Int16.TryParse(this.Record.GetValue(field.Ordinal).ToString(), out value);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Returns a Int32 value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a Boolean will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        public Int32 GetInt32(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            Int32 retVal = default(Int32);
            this.TryGetInt32(fieldName, out retVal);
            return retVal;
        }

        /// <summary>
        /// Returns a Int32 value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public Int32 GetInt32(string fieldName, Int32 defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            Int32 value = default(Int32);
            if (this.TryGetInt32(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a nullable Int32 value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public Int32? GetInt32(string fieldName, Int32? defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            Int32 value = default(Int32);
            if (this.TryGetInt32(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        public bool TryGetInt32(string fieldName, out Int32 value)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            bool retVal = false;
            value = default(Int32);
            FieldInfo field = this.Fields.Find(p => p.FieldName == fieldName);
            if (field != null && !this.Record.IsDBNull(field.Ordinal))
            {
                if (field.FieldType == typeof(Int32))
                {
                    value = this.Record.GetInt32(field.Ordinal);
                    retVal = true;
                }
                else
                {
                    retVal = Int32.TryParse(this.Record.GetValue(field.Ordinal).ToString(), out value);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Returns a Int64 value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a DateTime will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        public Int64 GetInt64(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            Int64 retVal = default(Int64);
            this.TryGetInt64(fieldName, out retVal);
            return retVal;
        }

        /// <summary>
        /// Returns a Int64 value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public Int64 GetInt64(string fieldName, Int64 defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            Int64 value = default(Int64);
            if (this.TryGetInt64(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a nullable Int64 value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public Int64? GetInt64(string fieldName, Int64? defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            Int64 value = default(Int64);
            if (this.TryGetInt64(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        public bool TryGetInt64(string fieldName, out Int64 value)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            bool retVal = false;
            value = default(Int64);
            FieldInfo field = this.Fields.Find(p => p.FieldName == fieldName);
            if (field != null && !this.Record.IsDBNull(field.Ordinal))
            {
                if (field.FieldType == typeof(Int64))
                {
                    value = this.Record.GetInt64(field.Ordinal);
                    retVal = true;
                }
                else
                {
                    retVal = Int64.TryParse(this.Record.GetValue(field.Ordinal).ToString(), out value);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Returns an object value from the <see cref="DataSource"/>. If the field name does not exist, the default value for an object will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        public object GetValue(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            object retval = default(object);
            FieldInfo field = this.Fields.Find(p => p.FieldName == fieldName);
            if (field != null && !this.Record.IsDBNull(field.Ordinal))
            {
                retval = this.Record.GetValue(field.Ordinal);
            }
            return retval;
        }

        /// <summary>
        /// Returns a T value from the <see cref="DataSource"/>. If the field name does not exist, the default value for T will be returned.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="fieldName">The name of the field.</param>
        public T GetValue<T>(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            T retVal = default(T);
            this.TryGetValue<T>(fieldName, out retVal);
            return retVal;
        }

        /// <summary>
        /// Returns a T value from the <see cref="DataSource"/>. If the field name does not exist, the default value for T will be returned.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        public T GetValue<T>(string fieldName, T defaultValue)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            T value = default(T);
            if (this.TryGetValue<T>(fieldName, out value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        public bool TryGetValue<T>(string fieldName, out T value)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }
            bool retVal = false;
            value = default(T);
            FieldInfo field = this.Fields.Find(p => p.FieldName == fieldName);
            if (field != null && !this.Record.IsDBNull(field.Ordinal))
            {
                if (field.FieldType == typeof(T).GetGenericTypeParameter())
                {
                    value = (T)this.Record.GetValue(field.Ordinal);
                    retVal = true;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Returns a <see cref="DynamicFieldCollection "/> of dyanamic, or unbound, fields for the specified type supplied.
        /// </summary>
        /// <param name="type">The type to interrogate for unbound fields.</param>
        public DynamicFieldCollection GetDynamicFields(Type type)
        {
            DynamicFieldCollection fields = new DynamicFieldCollection();
            if (this.DynamicFields == null)
            {
                this.DynamicFields = new List<FieldInfo>();
                List<DataColumnAttribute> dataFields = DataAttributeUtilities.GetDataColumnAttributes(type);
                foreach (FieldInfo fieldInfo in this.Fields)
                {
                    if (dataFields.Find(p => p.ColumnName == fieldInfo.FieldName) == null)
                    {
                        this.DynamicFields.Add(fieldInfo);
                    }
                }
            }
            foreach (FieldInfo fieldInfo in this.DynamicFields)
            {
                DynamicField field = new DynamicField(fieldInfo.FieldName, null);
                if (!this.Record.IsDBNull(fieldInfo.Ordinal))
                {
                    field.Value = this.Record.GetValue(fieldInfo.Ordinal);
                }
                fields.Add(field);
            }
            return fields;
        }
        
        /// <summary>
        /// Represents meta data information regarding the <see cref="IDataReader"/> upon which the instance of the class was constructed.
        /// </summary>
        [DebuggerDisplay("{FieldName}, {FieldType}")]
        internal class FieldInfo
        {
            public string FieldName { get; set; }
            public int Ordinal { get; set; }
            public Type FieldType { get; set; }

            public FieldInfo(string fieldName, int ordinal, Type fieldType)
            {
                this.FieldName = fieldName;
                this.Ordinal = ordinal;
                this.FieldType = fieldType;
            }
        }
    }
}
