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

namespace FlyweightObjects
{
    /// <summary>
    /// Represents an interface which can serve as a data source for an object.
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        /// Returns a Boolean value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a Boolean will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        bool GetBoolean(string fieldName);

        /// <summary>
        /// Returns a Boolean value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        bool GetBoolean(string fieldName, bool defaultValue);

        /// <summary>
        /// Returns a nullable Boolean value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        bool? GetBoolean(string fieldName, bool? defaultValue);

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        bool TryGetBoolean(string fieldName, out bool value);

        /// <summary>
        /// Returns a Byte value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a Boolean will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        byte GetByte(string fieldName);

        /// <summary>
        /// Returns a Byte value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        byte GetByte(string fieldName, byte defaultValue);

        /// <summary>
        /// Returns a nullable Byte value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        byte? GetByte(string fieldName, byte? defaultValue);

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        bool TryGetByte(string fieldName, out byte value);

        /// <summary>
        /// Returns a byte array value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a byte array will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        byte[] GetBytes(string fieldName);

        /// <summary>
        /// Reads a stream of bytes from the specified column offset into the buffer as an array, starting at the given buffer offset.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="fieldOffset">The index within the field from which to start the read operation.</param>
        /// <param name="buffer">The buffer into which to read the stream of bytes.</param>
        /// <param name="bufferOffSet">The index within the field from which to start the read operation.</param>
        /// <param name="length">The number of bytes to read. </param>
        long GetBytes(string fieldName, long fieldOffset, byte[] buffer, int bufferOffSet, int length);

        /// <summary>
        /// Returns a Char value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a Boolean will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        char GetChar(string fieldName);

        /// <summary>
        /// Returns a Char value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        char GetChar(string fieldName, char defaultValue);

        /// <summary>
        /// Returns a nullable Char value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        char? GetChar(string fieldName, char? defaultValue);

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        bool TryGetChar(string fieldName, out char value);

        /// <summary>
        /// Returns a Char array value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a char array will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        char[] GetChars(string fieldName);


        /// <summary>
        /// Reads a stream of characters from the specified column offset into the buffer as an array, starting at the given buffer offset.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="fieldOffset">The index within the field from which to start the read operation.</param>
        /// <param name="buffer">The buffer into which to read the stream of characters.</param>
        /// <param name="bufferOffSet">The index within the field from which to start the read operation.</param>
        /// <param name="length">The number of characters to read. </param>
        long GetChars(string fieldName, long fieldOffset, char[] buffer, int bufferOffSet, int length);

        /// <summary>
        /// Returns a string value from the <see cref="DataSource"/>. If the field name does not exist, the default value for string will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        string GetString(string fieldName);

        /// <summary>
        /// Returns a string value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        string GetString(string fieldName, string defaultValue);

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        bool TryGetString(string fieldName, out string value);

        /// <summary>
        /// Returns a DateTime value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a DateTime will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        DateTime GetDateTime(string fieldName);

        /// <summary>
        /// Returns a DateTime value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        DateTime GetDateTime(string fieldName, DateTime defaultValue);

        /// <summary>
        /// Returns a nullable DateTime value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        DateTime? GetDateTime(string fieldName, DateTime? defaultValue);

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        bool TryGetDateTime(string fieldName, out DateTime value);

        /// <summary>
        /// Returns a Decimal value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a DateTime will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        decimal GetDecimal(string fieldName);

        /// <summary>
        /// Returns a Decimal value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        decimal GetDecimal(string fieldName, decimal defaultValue);

        /// <summary>
        /// Returns a nullable Decimal value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        decimal? GetDecimal(string fieldName, decimal? defaultValue);

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        bool TryGetDecimal(string fieldName, out decimal value);

        /// <summary>
        /// Returns a Double value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a DateTime will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        double GetDouble(string fieldName);

        /// <summary>
        /// Returns a Double value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        double GetDouble(string fieldName, double defaultValue);

        /// <summary>
        /// Returns a nullable Double value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        double? GetDouble(string fieldName, double? defaultValue);

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        bool TryGetDouble(string fieldName, out double value);

        /// <summary>
        /// Returns a Single value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a DateTime will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        float GetFloat(string fieldName);

        /// <summary>
        /// Returns a Single value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        float GetFloat(string fieldName, float defaultValue);

        /// <summary>
        /// Returns a nullable Single value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        float? GetFloat(string fieldName, float? defaultValue);

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        bool TryGetFloat(string fieldName, out float value);

        /// <summary>
        /// Returns a Guid value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a Guid will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        Guid GetGuid(string fieldName);

        /// <summary>
        /// Returns a Int16 value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a DateTime will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        Int16 GetInt16(string fieldName);

        /// <summary>
        /// Returns a Int16 value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        Int16 GetInt16(string fieldName, Int16 defaultValue);

        /// <summary>
        /// Returns a nullable Int16 value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        Int16? GetInt16(string fieldName, Int16? defaultValue);

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        bool TryGetInt16(string fieldName, out Int16 value);

        /// <summary>
        /// Returns a Int32 value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a Boolean will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        Int32 GetInt32(string fieldName);

        /// <summary>
        /// Returns a Int32 value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        Int32 GetInt32(string fieldName, Int32 defaultValue);

        /// <summary>
        /// Returns a nullable Int32 value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        Int32? GetInt32(string fieldName, Int32? defaultValue);

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        bool TryGetInt32(string fieldName, out Int32 value);

        /// <summary>
        /// Returns a Int64 value from the <see cref="DataSource"/>. If the field name does not exist, the default value for a DateTime will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        Int64 GetInt64(string fieldName);

        /// <summary>
        /// Returns a Int64 value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        Int64 GetInt64(string fieldName, Int64 defaultValue);

        /// <summary>
        /// Returns a nullable Int64 value from the <see cref="DataSource"/>. If the field name does not exist, the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        Int64? GetInt64(string fieldName, Int64? defaultValue);

        /// <summary>
        /// Returns a Boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter. If the value could not be retrieved, then the supplied defaultValue will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        bool TryGetInt64(string fieldName, out Int64 value);

        /// <summary>
        /// Returns an object value from the <see cref="DataSource"/>. If the field name does not exist, the default value for an object will be returned.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        object GetValue(string fieldName);

        /// <summary>
        /// Returns a T value from the <see cref="DataSource"/>. If the field name does not exist, the default value for T will be returned.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="fieldName">The name of the field.</param>
        T GetValue<T>(string fieldName);

        /// <summary>
        /// Returns a T value from the <see cref="DataSource"/>. If the field name does not exist, the default value for T will be returned.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="defaultValue">The default value to be returned.</param>
        T GetValue<T>(string fieldName, T defaultValue);

        /// <summary>
        /// Returns a boolean value indicating whether or not the value could successfully be retrieved from the <see cref="DataSource"/>. The value for 
        /// the field will be given by the out value parameter.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to be set.</param>
        bool TryGetValue<T>(string fieldName, out T value);
    }
}
