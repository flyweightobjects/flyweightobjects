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
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Data;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Collections;
using System.Linq.Expressions;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents a surrogate container for private member fields of an <see cref="IFlyweight"/> object.
    /// </summary>
    [Serializable]
    public class PropertyStorage : IPropertyStorage, INotifyPropertyChanging, INotifyPropertyChanged
    {
        /// <summary>
        /// Represents the method that will handle the PropertyChanging event of the <see cref="INotifyPropertyChanging"/> class.
        /// </summary>
        [field:NonSerialized]
        public event PropertyChangingEventHandler PropertyChanging;
        
        /// <summary>
        /// Represents the method that will handle the PropertyChanging event of the <see cref="INotifyPropertyChanged"/> class.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Represents the method that will handle the RowSourceChanged event when the underlying RowSource has been changed.
        /// </summary>
        [field:NonSerialized]
        public event EventHandler DataSourceChanged;

        /// <summary>
        /// Gets or sets the type represented by <see cref="PropertyStorage"/>.
        /// </summary>
        protected internal Type ElementType     { get; private set; }

        /// <summary>
        /// Gets or sets the data source for the <see cref="PropertyStorage"/>.
        /// </summary>
        protected internal ResultSet.Row Row    { get; private set; }
        
        /// <summary>
        /// Gets or sets the type of domain object for which properties are stored.
        /// </summary>
        Type IPropertyStorage.ElementType
        {
            get { return this.ElementType; }
        }

        /// <summary>
        /// Gets or sets the underlying <see cref="ResultSet.Row"/> wich serves as the data source.
        /// </summary>
        ResultSet.Row IPropertyStorage.DataSource
        {
            get { return this.Row; }
            set
            {
                this.Row = value;
                OnDataSourceChanged();
            }
        }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="elementType">The type of domain object for which to store properties.</param>
        public PropertyStorage(Type elementType)
        {
            this.ElementType = elementType;
            if (this.ElementType != typeof(QueryResult))
            {
                ResultSet resultSet = new ResultSet(this.ElementType);
                this.Row = resultSet.NewRow();
                resultSet.Rows.Add(this.Row);
            }
            OnDataSourceChanged();
        }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="elementType">The type of domain object for which to store properties.</param>
        /// <param name="dataSource">The <see cref="ResultSet.Row"/> acting as the datasource.</param>
        internal PropertyStorage(Type elementType, ResultSet.Row dataSource)
        {
            this.ElementType = elementType;
            this.Row = dataSource;
            OnDataSourceChanged();
        }

        /// <summary>
        /// Gets or sets the property value in local storage at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        public object this[int index]
        {
            get { return this.Row[index]; }
            set { this.Row[index] = value; }
        }

        /// <summary>
        /// Gets or sets the property value in local storage at the specified index.
        /// </summary>
        /// <param name="index">The name of the property.</param>
        public object this[string index]
        {
            get 
            {
                string columnName = index;
                if (!TryGetColumnName(index, out columnName))
                {
                    throw new InvalidPropertyNameException(index);
                }
                return this.Row[columnName]; 
            }
            set 
            {
                string columnName = index;
                if (!TryGetColumnName(index, out columnName))
                {
                    throw new InvalidPropertyNameException(index);
                }
                this.Row[columnName] = value;
            }
        }

        /// <summary>
        /// Gets the property value held in local storage.
        /// </summary>
        /// <typeparam name="T">The type of the property to retrieve from local storage.</typeparam>
        /// <param name="propertyName">The property name held in local storage.</param>
        public T GetProperty<T>(string propertyName)
        {
            string columnName = propertyName;
            if (!TryGetColumnName(propertyName, out columnName))
            {
                throw new InvalidPropertyNameException(propertyName);
            }
            T retVal = default(T);
            object val = this.Row[columnName];
            if (val != null && val != DBNull.Value)
            {
                if (!TypeConverter.RequiresConvert(typeof(T), val.GetType()))
                {
                    retVal = (T)val;
                }
                else
                {
                    retVal = (T)TypeConverter.ConvertType<T>(val);
                }
            }
            if (retVal == null && typeof(IEnumerable).IsAssignableFrom(typeof(T)) && typeof(T).HasDefaultConstructor())
            {
				retVal = Activator.CreateInstance<T>();
                this.Row[columnName] = retVal;
            }
            return retVal;
        }

        /// <summary>
        /// Gets the property value held in local storage. If the value equals the default(T), returns the supplied defaultValue instead.
        /// </summary>
        /// <typeparam name="T">The type of the property to retrieve from local storage.</typeparam>
        /// <param name="propertyName">The property name held in local storage.</param>
        /// <param name="defaultValue">The default value to return.</param>
        public T GetProperty<T>(string propertyName, T defaultValue)
        {
            T retVal = this.GetProperty<T>(propertyName);
            if (retVal == null || retVal.Equals(default(T)))
            {
                retVal = defaultValue;
            }
            return retVal;
        }

        /// <summary>
        /// Gets the property value held in local storage.
        /// </summary>
        /// <typeparam name="TSource">The type expected in local storage.</typeparam>
        /// <typeparam name="T">The type of the property to retrieve from local storage.</typeparam>
        /// <param name="property">The expression representing the property held in local storage.</param>
        public T GetProperty<TSource, T>(Expression<Func<TSource, T>> property) where TSource : class, IFlyweight, new()
        {
            string propertyName = PropertyOf<TSource>.Name<T>(property);
            return GetProperty<T>(propertyName);
        }

        /// <summary>
        /// Gets the property value held in local storage.
        /// </summary>
        /// <typeparam name="TSource">The type expected in local storage.</typeparam>
        /// <typeparam name="T">The type of the property to retrieve from local storage.</typeparam>
        /// <param name="property">The expression representing the property held in local storage.</param>
        /// <param name="defaultValue">The default value to return.</param>
        public T GetProperty<TSource, T>(Expression<Func<TSource, T>> property, T defaultValue) where TSource : class, IFlyweight, new()
        {
            string propertyName = PropertyOf<TSource>.Name<T>(property);
            return GetProperty<T>(propertyName, defaultValue);
        }

        /// <summary>
        /// Sets the property value held in local storage.
        /// </summary>
        /// <typeparam name="T">The type of the property to set in local storage</typeparam>
        /// <param name="propertyName">The property name held in local storage.</param>
        /// <param name="value">The value to set for the property.</param>
        public void SetProperty<T>(string propertyName, T value)
        {
            string columnName = propertyName;
            if (!TryGetColumnName(propertyName, out columnName))
            {
                throw new InvalidPropertyNameException(propertyName);
            }
            OnPropertyChanging(propertyName);
            this.Row[columnName] = value;
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Sets the property value held in local storage.
        /// </summary>
        /// <typeparam name="TSource">The type expected in local storage.</typeparam>
        /// <typeparam name="T">The type of the property to set in local storage.</typeparam>
        /// <param name="property">The expression representing the property held in local storage.</param>
        /// <param name="value">The value to set for the property.</param>
        public void SetProperty<TSource, T>(Expression<Func<TSource, T>> property, T value) where TSource : class, IFlyweight, new()
        {
            string propertyName = PropertyOf<TSource>.Name<T>(property);
            SetProperty<T>(propertyName, value);
        }

        /// <summary>
        /// Returns a <see cref="DataRelationContainer{T}"/> container held in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of the related object.</typeparam>
        /// <param name="propertyName">The name of the related property.</param>
        public DataRelationContainer<T> GetDataRelation<T>(string propertyName) where T : class, IFlyweight, new()
        {
            string columnName = propertyName;
            if (!TryGetColumnName(propertyName, out columnName))
            {
                throw new InvalidPropertyNameException(propertyName);
            }
            if (this.Row.ResultSet.Columns[columnName].Type != typeof(DataRelationContainer<T>))
            {
                throw new ArgumentNullException(string.Format("The specified propertyName is not a {0}.", typeof(DataRelationContainer<T>).FullName));
            }
            DataRelationContainer<T> container = this.Row[columnName].AsType<DataRelationContainer<T>>();
            if (container == null)
            {
                container = new DataRelationContainer<T>();
                this.Row[columnName] = container;
            }
            return container;
        }

        /// <summary>
        /// Returns a <see cref="DataRelationContainer{T}"/> container held in storage.
        /// </summary>
        /// <param name="property">An expression which represents the property name.</param>
        public DataRelationContainer<TRelation> GetDataRelation<TSource, TRelation>(Expression<Func<TSource, object>> property) where TRelation : class, IFlyweight, new()
        {
            string propertyName = PropertyOf<TSource>.Name(property);
            return GetDataRelation<TRelation>(propertyName);
        }

        /// <summary>
        /// Determines if the specified property name exist in local storage.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        public bool PropertyExists(string propertyName)
        {
            string columnName = null;
            return TryGetColumnName(propertyName, out columnName);
        }

        /// <summary>
        /// Returns an <see cref="IList{PropertyDefinition}"/> objects which are being held in local storage.
        /// </summary>
        public IList<PropertyDefinition> PropertyDefinitions
        {
            get
            {
                List<PropertyDefinition> vals = new List<PropertyDefinition>();
                foreach (ResultSet.Column column in this.Row.ResultSet.Columns)
                {
                    string propertyName = null;
                    if (TryGetPropertyName(column.Name, out propertyName))
                    {
                        vals.Add(new PropertyDefinition(propertyName, column.Name, column.Type));
                    }
                    else
                    {
                        vals.Add(new PropertyDefinition(column.Name, column.Name, column.Type));
                    }
                }
                return vals;
            }
        }

        /// <summary>
        /// Determines if any property has been changed.
        /// </summary>
        public bool IsChanged
        {
            get { return this.Row.IsChanged; }
        }

        /// <summary>
        /// Raises the PropertyChanging event.
        /// </summary>
        protected virtual void OnPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        /// <summary>
        /// Raises the RowSourceChanged event.
        /// </summary>
        protected virtual void OnDataSourceChanged()
        {
            if (DataSourceChanged != null)
            {
                DataSourceChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Attempts to retrieve the column name in local storage representing the property name specified. If successful, returns true 
        /// with the name of the column, otherwise false.
        /// </summary>
        /// <param name="propertyName">The property name on the object.</param>
        /// <param name="columnName">The underlying column name of the object.</param>
        protected internal bool TryGetColumnName(string propertyName, out string columnName)
        {
			if (propertyName == null)
			{
				throw new ArgumentNullException("propertyName");
			}
			
			bool retVal = this.Row.ResultSet.Columns.Contains(propertyName);
            columnName = propertyName;
            if (!retVal)
            {
                DataColumnAttribute attr = null;
                if (DataAttributeUtilities.TryGetDataColumnAttribute(this.ElementType, propertyName, out attr))
                {
					columnName = attr.ColumnName.Remove(new char[] { '[', ']' });
                    retVal = true;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Attempts to retrieve the property name representing specified column name in local storage. If successful, returns true 
        /// with the name of the property, otherwise false.
        /// </summary>
        /// <param name="columnName">The underlying column name of the object.</param>
        /// <param name="propertyName">The property name on the object.</param>
        protected internal bool TryGetPropertyName(string columnName, out string propertyName)
        {
			if (columnName == null)
			{
				throw new ArgumentNullException("columnName");
			}

			columnName = columnName.Remove(new char[] { '[', ']' });
			bool retVal = false;
            propertyName = null;
            foreach (PropertyInfo property in this.ElementType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                DataColumnAttribute[] dataColumns = property.GetCustomAttributes(typeof(DataColumnAttribute), true) as DataColumnAttribute[];
				if (dataColumns.Length > 0 && dataColumns[0].ColumnName.Remove(new char[] { '[', ']' }).Equals(columnName, StringComparison.CurrentCultureIgnoreCase))
                {
                    propertyName = property.Name;
                    retVal = true;
                    break;
                }
            }
            return retVal;
        }
    }
}
