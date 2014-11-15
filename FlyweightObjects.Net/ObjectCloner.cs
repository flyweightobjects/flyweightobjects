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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Collections;

namespace FlyweightObjects
{
    /// <summary>
    /// Provides cloning functionality for serializable objects and lists.
    /// </summary>
    public class ObjectCloner
    {
        /// <summary>
        /// Returns a separate deep clone of an object.
        /// </summary>
        /// <typeparam name="T">The type of the object to return.</typeparam>
        /// <param name="source">The object to clone.</param>
        public static T Clone<T>(T source) where T : new()
        {
            T newObj = default(T);
            if (source != null)
            {
                newObj = Serialize<T>(source);
            }
            return newObj;
        }

        /// <summary>
        /// Returns a separate deep clone of an object.
        /// </summary>
        /// <param name="source">The object to clone.</param>
        public static object Clone(object source) 
        {
            object newObj = default(object);
            if (source != null)
            {
                newObj = Serialize(source);
            }
            return newObj;
        }
        
        /// <summary>
        /// Serializes a generic object and returns a new instance.
        /// </summary>
        /// <typeparam name="T">The generic type to serialize.</typeparam>
        /// <param name="source">The instance object to serialize.</param>
        private static T Serialize<T>(T source)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, source);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Serializes an object and returns a new instance.
        /// </summary>
        /// <param name="source">The instance object to serialize.</param>
        private static object Serialize(object source)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, source);
                stream.Position = 0;
                return formatter.Deserialize(stream);
            }
        }

    }
}
