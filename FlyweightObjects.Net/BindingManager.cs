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
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

namespace FlyweightObjects
{
    /// <summary>
    /// Binds data supplied by an <see cref="IDataReader"/> to an enumerable list of objects.
    /// </summary>
    public class BindingManager : IBindingManager
    {
        /// <summary>
        /// Performs the binding of the supplied <see cref="IDataReader"/> to an enuemrable list of objects.
        /// </summary>
        public IEnumerable<T> BindData<T>(IDataReader reader) where T : class, new()
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (reader.IsClosed)
            {
                throw new ArgumentException("The supplied reader cannot be closed to perform this operation.");
            }
            try
            {
                List<T> retVal = new List<T>();
                Type type = typeof(T);
                ConstructorInfo ctor = type.GetConstructor(new Type[0]);
                DataSource dataSource = new DataSource(reader);
                if (typeof(IDataSourceBindable).IsAssignableFrom(type))
                {
                    while (reader.Read())
                    {
                        object obj = ctor.Invoke(new object[0]);
                        ((IDataSourceBindable)obj).BindData(dataSource);
                        retVal.Add((T)obj);
                    }
                }
                else
                {
                    List<DataColumnAttribute> dataColumns = DataAttributeUtilities.GetDataColumnAttributes(typeof(T));
                    while (reader.Read())
                    {
                        object obj = ctor.Invoke(new object[0]);
                        if (type.IsValueType)
                        {
                            obj = reader[0];
                        }
                        else
                        {
                            foreach (DataColumnAttribute dataColumn in dataColumns)
                            {
                                object val = dataSource.GetValue(dataColumn.ColumnName);
                                dataColumn.MappedProperty.SetValue(obj, val);
                            }
                            if (typeof(IDynamicResult).IsAssignableFrom(type))
                            {
                                ((IDynamicResult)obj).Fields = dataSource.GetDynamicFields(type);
                            }
                        }
                    }

                    //MemberInfo[] members = GetDataColumnMemberInfoArray(type, reader);
                    //while (reader.Read())
                    //{
                    //    object obj = ctor.Invoke(new object[0]);
                    //    object[] record = new object[reader.FieldCount];
                    //    reader.GetValues(record);
                    //    if (type.IsValueType)
                    //    {
                    //        obj = record[0];
                    //    }
                    //    else
                    //    {
                    //        for (int i = 0; i < record.Length; i++)
                    //        {
                    //            ProcessMemberBindings(obj, members[i], record[i]);
                    //        }
                    //        //ProcessInterfaceBindings(obj, members, reader);
                    //    }
                    //    retVal.Add((T)obj);
                    //}
                }
            }
            finally
            {
                reader.Close();
            }
            return retVal;
        }

        /// <summary>
        /// Gets a MemberInfo array of all properties of a type which have a DataField attribute.
        /// </summary>
        /// <param name="type">The type of the object.</param>
        /// <param name="reader">The reader to act as the data source.</param>
        private MemberInfo[] GetDataColumnMemberInfoArray(Type type, IDataReader reader)
        {
            MemberInfo[] sortedMembers = new MemberInfo[reader.FieldCount];
            int ordinal = 0;
            MemberInfo[] members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            DataTable schemaTable = reader.GetSchemaTable();
            foreach (MemberInfo member in members)
            {
                DataColumnAttribute[] attrs = member.GetCustomAttributes(typeof(DataColumnAttribute), true) as DataColumnAttribute[];
                if (attrs.Length > 0)
                {
                    try
                    {
                        DataRow[] rows = schemaTable.Select(string.Format("ColumnName='{0}'", attrs[0].ColumnName));
                        if (rows.Length > 0)
                        {
                            ordinal = reader.GetOrdinal(attrs[0].ColumnName);
                            sortedMembers[ordinal] = member;
                        }
                    }
                    catch
                    {
                        ;
                    }
                }
            }
            return sortedMembers;
        }

        /// <summary>
        /// Sets the <see cref="MemberInfo"/> given the object, its property and the value found in the data source.
        /// </summary>
        /// <param name="source">The target domain object.</param>
        /// <param name="member">The <see cref="MemberInfo"/> to set on the object.</param>
        /// <param name="value">The value to set on the <see cref="MemberInfo"/>.</param>
        protected internal void ProcessMemberBindings(object source, MemberInfo member, object value)
        {
            if (member != null && value != DBNull.Value)
            {
                Type memberType = member.MemberType == MemberTypes.Property ? ((PropertyInfo)member).PropertyType : ((FieldInfo)member).FieldType;
                if (memberType.BaseType.Equals("Enum"))
                {
                    value = Enum.Parse(memberType, value.ToString());
                    member.SetValue(source, value);
                }
                else
                {
                    switch (memberType.GetGenericTypeParameter().Name)
                    {
                        case "Byte":
                            member.SetValue(source, Convert.ToByte(value));
                            break;
                        case "Boolean":
                            member.SetValue(source, Convert.ToBoolean(value));
                            break;
                        case "Char":
                            member.SetValue(source, Convert.ToChar(value));
                            break;
                        case "DateTime":
                            member.SetValue(source, Convert.ToDateTime(value));
                            break;
                        case "Decimal":
                            member.SetValue(source, Convert.ToDecimal(value));
                            break;
                        case "Double":
                            member.SetValue(source, Convert.ToDouble(value));
                            break;
                        case "Int16":
                            member.SetValue(source, Convert.ToInt16(value));
                            break;
                        case "Int32":
                            member.SetValue(source, Convert.ToInt32(value));
                            break;
                        case "Int64":
                            member.SetValue(source, Convert.ToInt64(value));
                            break;
                        case "String":
                            if (value is string || value == null)
                                member.SetValue(source, value);
                            else
                                member.SetValue(source, Convert.ToString(value));
                            break;
                        case "Guid":
                            member.SetValue(source, new Guid(value.ToString()));
                            break;
                        default:
                            member.SetValue(source, value);
                            break;
                    }
                }
            }
        }

        ///// <summary>
        ///// Adds unbound columns in the <see cref="IDataReader"/> to the <see cref="IQueryResult"/> object's Fields collection.
        ///// </summary>
        ///// <param name="source">The object to bind the value to.</param>
        ///// <param name="reader">The reader to serve as the data source.</param>
        ///// <param name="memberBindings">A generic list of MemberInfo objects successfully bound by the binding process.</param>
        //public void BindDynamicProperties(object source, IDataReader reader, List<MemberInfo> memberBindings)
        //{
        //    for (int i = 0; i < memberBindings.Count; i++)
        //    {
        //        if (memberBindings[i] == null)
        //        {
        //            string columnName = reader.GetName(i);
        //            ((IQueryResult)source).Fields.Add(new QueryField(columnName, reader[columnName]));
        //        }
        //    }
        //}
    }
}
