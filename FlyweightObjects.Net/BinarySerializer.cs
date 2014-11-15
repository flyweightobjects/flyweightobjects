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
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace FlyweightObjects
{
    /// <summary>
    /// Performs binary serialization routines.
    /// </summary>
    /// <example>
    /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
    /// <code>
    /// <![CDATA[
    /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
    /// {
    ///     // Retrieve a product from storage
    ///     var p1 = context.Select<Product>(1).ToSingle();
    ///     Console.WriteLine("ProductID: {0}", p1.Name);
    ///            
    ///     // Serialize the object as a byte array
    ///     var[] bytes = BinarySerializer.Serialize(p1);
    ///     
    ///     // Deserialize the product 
    ///     Product p2 = BinarySerializer.Deserialize<Product>(bytes);
    ///     Console.WriteLine("ProductID: {0}", p2.Name);
    /// }
    /// ]]>
    /// </code>
    /// </example>   
    public sealed class BinarySerializer
    {
        /// <summary>
        /// Serializes the object into a byte array.
        /// </summary>
        /// <param name="source">The object to serialize.</param>
        public static byte[] Serialize(object source)
        {
            if (source == null)
            {
                return null;
            }
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, source);
                return stream.GetBuffer();
            }
        }

        /// <summary>
        /// Deserializes an object into the type of T.
        /// </summary>
        /// <typeparam name="T">The type of object to be deserialized.</typeparam>
        /// <param name="bytes">The byte array of the object.</param>
        public static T Deserialize<T>(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return default(T);
            }
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                BinaryFormatter serializer = new BinaryFormatter();
                return (T)serializer.Deserialize(stream);
            }
        }
    }
}