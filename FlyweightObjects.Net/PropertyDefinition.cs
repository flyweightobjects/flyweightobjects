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
using System.Diagnostics;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents a property being stored in <see cref="PropertyStorage"/>.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{PropertyName} {DataColumnType}")]
    public class PropertyDefinition
    {
        private string _propertyName;
        private string _dataColumnName;
        private Type _dataColumnType;

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="dataColumnName">The <see cref="DataColumnAttribute.ColumnName"/> value.</param>
        /// <param name="dataColumnType">The type used to store the property in <see cref="PropertyStorage"/>.</param>
        public PropertyDefinition(string propertyName, string dataColumnName, Type dataColumnType)
        {
            _propertyName = propertyName;
            _dataColumnName = dataColumnName;
            _dataColumnType = dataColumnType;
        }

        /// <summary>
        /// Gets the name of the property as exposed by the domain object.
        /// </summary>
        public string PropertyName
        {
            get { return _propertyName; }
        }

        /// <summary>
        /// Gets the name of the underlying column.
        /// </summary>
        public string DataColumnName
        {
            get { return _dataColumnName; }
        }

        /// <summary>
        /// Gets the type of the underlying column.
        /// </summary>
        public Type DataColumnType
        {
            get { return _dataColumnType; }
        }

        string DisplayName
        {
            get 
            {
                return string.Format("{0} {1}", this.PropertyName, this.DataColumnType);
            }
        }
    }
}
