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
using System.Linq;
using System.Text;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents a class responsible for converting one type to another.
    /// </summary>
    [Serializable]
    internal static class TypeConverter
    {
        public static object ConvertType(Type type, object value)
        {
            object retVal = value;
            if (type.BaseType.Equals("Enum"))
            {
                retVal = Enum.Parse(type, value.ToString());
            }
            else
            {
                switch (type.GetGenericTypeParameter().Name)
                {
                    case "Byte":
                        retVal = Convert.ToByte(value);
                        break;
                    case "SByte":
                        retVal = Convert.ToSByte(value);
                        break;
                    case "Int16":
                        retVal = Convert.ToInt16(value);
                        break;
                    case "Int32":
                        retVal = Convert.ToInt32(value);
                        break;
                    case "Int64":
                        retVal = Convert.ToInt64(value);
                        break;
                    case "UInt16":
                        retVal = Convert.ToUInt16(value);
                        break;
                    case "UInt32":
                        retVal = Convert.ToUInt32(value);
                        break;
                    case "UInt64":
                        retVal = Convert.ToUInt64(value);
                        break;
                    case "Single":
                        retVal = Convert.ToSingle(value);
                        break;
                    case "Double":
                        retVal = Convert.ToDouble(value);
                        break;
                    case "Boolean":
                        retVal = Convert.ToBoolean(value);
                        break;
                    case "Char":
                        retVal = Convert.ToChar(value);
                        break;
                    case "Decimal":
                        retVal = Convert.ToDecimal(value);
                        break;
                    case "DateTime":
                        retVal = Convert.ToDateTime(value);
                        break;
                    case "String":
                        if (value is string || value == null)
                            retVal = value;
                        else
                            retVal = Convert.ToString(value);
                        break;
                    case "Guid":
                        retVal = new Guid(value.ToString());
                        break;
                    default:
                        retVal = value;
                        break;
                }
            }
            return retVal;
        }

        public static T ConvertType<T>(object value)
        {
            return (T)ConvertType(typeof(T), value);
        }

        public static bool RequiresConvert(Type typeA, Type typeB)
        {
            return typeA != typeB || (Nullable.GetUnderlyingType(typeA) != null && Nullable.GetUnderlyingType(typeA) != typeB);
        }
    }
}
