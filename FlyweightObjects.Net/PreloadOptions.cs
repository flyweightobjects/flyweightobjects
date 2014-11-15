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
using System.Linq.Expressions;
using System.Reflection;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents additional loading options that can be specified when querying relational storage.
    /// </summary>
    /// <typeparam name="T">The type parameter of primary object.</typeparam>
    [Serializable]
    public class PreloadOptions<T> where T : class, IFlyweight, new()
    {
        private List<PropertyInfo> _properties = new List<PropertyInfo>();

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        public PreloadOptions(params Expression<Func<T, object>>[] expressions)
        {
            foreach (var item in expressions)
            {
                PropertyInfo property = (item.Body as MemberExpression).Member as PropertyInfo;
                if (property != null)
                {
                    ValidateProperty(property);
                    _properties.Add(property);
                }
            }
        }

        /// <summary>
        /// Specifies the data relations that should be selected with T.
        /// </summary>
        /// <param name="expressions"></param>
        public static PreloadOptions<T> LoadWith(params Expression<Func<T, object>>[] expressions)
        {
            return new PreloadOptions<T>(expressions);
        }

        /// <summary>
        /// Gets a list of <see cref="PropertyInfo"/> objects specified when the class was constructed.
        /// </summary>
        public List<PropertyInfo> Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Determines whether the specified property is a valid data relation.
        /// </summary>
        protected internal void ValidateProperty(PropertyInfo property)
        {
            if (DataAttributeUtilities.GetDataRelationAttributes(property).Length == 0)
            {
                throw new ArgumentException(string.Format("The specified property {0} in the type {1} is not a {2}.", property.Name, typeof(T).FullName, typeof(DataRelationAttribute).FullName));
            }
        }
    }
}
