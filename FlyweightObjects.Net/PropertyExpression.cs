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
using System.Reflection;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq.Expressions;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents a column of an object constrained to a type which can be used in creating <see cref="QueryExpression{TSource}"/> queries.
	/// </summary>
	/// <typeparam name="T">The parent type which exposes the property representing a column.</typeparam>
	[Serializable]
	public sealed class Column<T> : PropertyExpression<T> where T : class, IFlyweight, new()
	{
		/// <summary>
		/// Creates an instance of the class.
		/// </summary>
		public Column() { }

		/// <summary>
		/// Creates an instance of the class.
		/// </summary>
		/// <param name="name">The property name that is adorned with a <see cref="DataColumnAttribute"/>.</param>
		public Column(string name)
			: base(name) { }

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="property">An expression which represents the property which is adorned with a <see cref="DataColumnAttribute"/>.</param>
		public Column(Expression<Func<T, object>> property)
			: base(property) { }
	}
	
	/// <summary>
	/// Represents a property of an object constrained to a type which can be used in creating <see cref="QueryExpression{TSource}"/> queries.
	/// </summary>
	/// <typeparam name="T">The parent type which exposes the property.</typeparam>
	[Serializable]
	public class PropertyExpression<T> : PropertyExpression where T : class, IFlyweight, new()
	{
		/// <summary>
		/// Creates an instance of the class.
		/// </summary>
		protected internal PropertyExpression() 
        { 
        
        }
		
		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
        /// <param name="propertyName">The property name that is adorned with a <see cref="DataColumnAttribute"/>.</param>
		public PropertyExpression(string propertyName)
            : base(typeof(T), propertyName)
		{

		}

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="property">An expression which represents the property which is adorned with a <see cref="DataColumnAttribute"/>.</param>
        public PropertyExpression(Expression<Func<T, object>> property)
            : base(typeof(T), PropertyOf<T>.Name(property))
        {
            
        }

		/// <summary>
		/// Represents an unspecified expression.
		/// </summary>
		public static PropertyExpression<T> Empty
		{
			get { return new PropertyExpression<T>(); }
		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="propertyName">The name of the property.</param>
		/// <returns></returns>
		public static PropertyExpression<T> Create(string propertyName)
		{
			return new PropertyExpression<T>(propertyName);
		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="property">An <see cref="Expression"/> representing the name of the property.</param>
		/// <returns></returns>
		public static PropertyExpression<T> Create(Expression<Func<T, object>> property)
		{
			return new PropertyExpression<T>(property);
		}

        /// <summary>
        /// Overrides the addition operator.
        /// </summary>
        /// <param name="expression">The expression to be added to.</param>
        /// <param name="value">The value to add.</param>
        public static PropertyExpression<T> operator +(PropertyExpression<T> expression, double value)
        {
            return expression.Add(value);
        }

        /// <summary>
        /// Overrides the substraction operator.
        /// </summary>
        /// <param name="expression">The expression from which to subtract.</param>
        /// <param name="value">The value to subtract.</param>
        public static PropertyExpression<T> operator -(PropertyExpression<T> expression, double value)
        {
            return expression.Subtract(value);
        }

        /// <summary>
        /// Overrides the multiplication operator.
        /// </summary>
        /// <param name="expression">The expression to multiply.</param>
        /// <param name="value">The value to multiply.</param>
        public static PropertyExpression<T> operator *(PropertyExpression<T> expression, double value)
        {
            return expression.Multiply(value);
        }

        /// <summary>
        /// Overrides the division operator.
        /// </summary>
        /// <param name="expression">The expression to divide.</param>
        /// <param name="value">The value by which to divide.</param>
        public static PropertyExpression<T> operator /(PropertyExpression<T> expression, double value)
        {
            return expression.Divide(value);
        }

		/// <summary>
		/// Overrides the equal to operator.
		/// </summary>
		/// <param name="expression">The first expression in the comparison.</param>
		/// <param name="value">The object to be compared.</param>
		public static PropertyExpression<T> operator ==(PropertyExpression<T> expression, object value)
		{
			return expression.EqualTo(value);
		}

		/// <summary>
		/// Overrides the not equal to operator.
		/// </summary>
		/// <param name="expression">The first expression in the comparison.</param>
		/// <param name="value">The object to be compared.</param>
		public static PropertyExpression<T> operator !=(PropertyExpression<T> expression, object value)
		{
			return expression.NotEqualTo(value);
		}

		/// <summary>
		/// Overrides the greater than operator.
		/// </summary>
		/// <param name="expression">The first expression in the comparison.</param>
		/// <param name="value">The object to be compared.</param>
		public static PropertyExpression<T> operator >(PropertyExpression<T> expression, object value)
		{
			return expression.GreaterThan(value);
		}

		/// <summary>
		/// Overrides the less than operator.
		/// </summary>
		/// <param name="expression">The first expression in the comparison.</param>
		/// <param name="value">The object to be compared.</param>
		public static PropertyExpression<T> operator <(PropertyExpression<T> expression, object value)
		{
			return expression.LessThan(value);
		}

		/// <summary>
		/// Overrides the greater than or equal to operator.
		/// </summary>
		/// <param name="expression">The first expression in the comparison.</param>
		/// <param name="value">The object to be compared.</param>
		public static PropertyExpression<T> operator >=(PropertyExpression<T> expression, object value)
		{
			return expression.GreaterThanOrEqualTo(value);
		}

		/// <summary>
		/// Overrides the less than or equal to operator.
		/// </summary>
		/// <param name="expression">The first expression in the comparison.</param>
		/// <param name="value">The object to be compared.</param>
		public static PropertyExpression<T> operator <=(PropertyExpression<T> expression, object value)
		{
			return expression.LessThanOrEqualTo(value);
		}

		/// <summary>
		/// Overrides the And operator.
		/// </summary>
		/// <param name="expression1">The original expression to receive the and-ing.</param>
		/// <param name="expression2">The additional expression to be and-ed.</param>
		public static PropertyExpression<T> operator &(PropertyExpression<T> expression1, PropertyExpression<T> expression2)
		{
			expression1.And(expression2);
			return expression1;
		}

		/// <summary>
		/// Overrides the Or operator.
		/// </summary>
		/// <param name="expression1">The original expression to receive the or-ing.</param>
		/// <param name="expression2">The additional expression to be or-ed.</param>
		public static PropertyExpression<T> operator |(PropertyExpression<T> expression1, PropertyExpression<T> expression2)
		{
			expression1.Or(expression2);
			return expression1;
		}

		/// <summary>
		/// Overrides the true operator.
		/// </summary>
		/// <param name="expression">The expression to be true-ed.</param>
		public static bool operator true(PropertyExpression<T> expression)
		{
			return false;
		}

		/// <summary>
		/// Overrides the false operator.
		/// </summary>
		/// <param name="expression">The expression to be false-ed.</param>
		public static bool operator false(PropertyExpression<T> expression)
		{
			return false;
		}

        /// <summary>
        /// Represents the addition operator.
        /// </summary>
        /// <param name="value">The value to add.</param>
        public new PropertyExpression<T> Add(double value)
        {
            base.Add(value);
            return this;
        }

        /// <summary>
        /// Represents the subtraction operator.
        /// </summary>
        /// <param name="value">The value to subtract.</param>
        public new PropertyExpression<T> Subtract(double value)
        {
            base.Subtract(value);
            return this;
        }

        /// <summary>
        /// Represents the multiplication operator.
        /// </summary>
        /// <param name="value">The value to multiply.</param>
        public new PropertyExpression<T> Multiply(double value)
        {
            base.Multiply(value);
            return this;
        }

        /// <summary>
        /// Represents the division operator.
        /// </summary>
        /// <param name="value">The value by which to divide.</param>
        public new PropertyExpression<T> Divide(double value)
        {
            base.Divide(value);
            return this;
        }

		/// <summary>
		/// Represents the equal operator.
		/// </summary>
		/// <param name="value">The value to be evaluated or the source of the set.</param>
        public new PropertyExpression<T> EqualTo(object value)
		{
            base.EqualTo(value);
			return this;
		}

		/// <summary>
		/// Represents the not equal to operator.
		/// </summary>
		/// <param name="value">The value to be evaluated.</param>
		public new PropertyExpression<T> NotEqualTo(object value)
		{
            base.NotEqualTo(value);
			return this;
		}

		/// <summary>
		/// Represents the greater than operator.
		/// </summary>
		/// <param name="value">The value to be evaluated.</param>
        public new PropertyExpression<T> GreaterThan(object value)
		{
            base.GreaterThan(value);
			return this;
		}

		/// <summary>
		/// Represents the less than operator.
		/// </summary>
		/// <param name="value">The value to be evaluated.</param>
        public new PropertyExpression<T> LessThan(object value)
		{
            base.LessThan(value);
			return this;
		}

		/// <summary>
		/// Represents a Data Manipulation Language (DML) IN keyword.
		/// </summary>
		/// <param name="values">The values to search for.</param>
		public new PropertyExpression<T> In(params object[] values)
		{
            base.In(values);
			return this;
		}

		/// <summary>
		/// Represents a Data Manipulation Language (DML) NOT IN keywords.
		/// </summary>
		/// <param name="values">The values to search for.</param>
		public new PropertyExpression<T> NotIn(params object[] values)
		{
            base.NotIn(values);
			return this;
		}

		/// <summary>
		/// Represents a Data Manipulation Language (DML) IN keyword.
		/// </summary>
		/// <param name="values">An <see cref="IEnumerable"/> of values.</param>
		public new PropertyExpression<T> In(IEnumerable values)
		{
            base.In(values);
			return this;
		}

		/// <summary>
		/// Represents a Data Manipulation Language (DML) NOT IN keywords.
		/// </summary>
		/// <param name="values">An <see cref="IEnumerable"/> of values.</param>
		public new PropertyExpression<T> NotIn(IEnumerable values)
		{
            base.NotIn(values);
			return this;
		}

        /// <summary>
        /// Represents a Data Manipulation Language (DML) IN keyword.
        /// </summary>
        /// <param name="query">An <see cref="IQueryExpression{TSource}"/> serving as a subquery.</param>
        public PropertyExpression<T> In(IQueryExpression<T> query)
        {
            base.In(query);
            return this;
        }

        /// <summary>
        /// Represents a Data Manipulation Language (DML) IN keyword.
        /// </summary>
        /// <param name="query">An <see cref="IQueryExpression{TSource}"/> serving as a subquery.</param>
        public PropertyExpression<T> NotIn(IQueryExpression<T> query)
        {
            base.NotIn(query);
            return this;
        }

        /// <summary>
        /// Represents a Data Manipulation Language (DML) IN keyword.
        /// </summary>
        /// <param name="command">An <see cref="IStorageCommand"/> serving as a subquery.</param>
        public PropertyExpression<T> In(IStorageCommand command)
        {
            base.In(command);
            return this;
        }

        /// <summary>
        /// Represents a Data Manipulation Language (DML) IN keyword.
        /// </summary>
        /// <param name="command">An <see cref="IStorageCommand"/> serving as a subquery.</param>
        public PropertyExpression<T> NotIn(IStorageCommand command)
        {
            base.NotIn(command);
            return this;
        }

		/// <summary>
		/// Represents the greater than or equal to operator.
		/// </summary>
		/// <param name="value">The value to be evaluated.</param>
        public new PropertyExpression<T> GreaterThanOrEqualTo(object value)
		{
            base.GreaterThanOrEqualTo(value);
			return this;
		}

		/// <summary>
		/// Represents the less than or equal to operator.
		/// </summary>
		/// <param name="value">The value to be evaluated.</param>
        public new PropertyExpression<T> LessThanOrEqualTo(object value)
		{
            base.LessThanOrEqualTo(value);
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) LIKE keyword.
		/// </summary>
		/// <param name="value">The value to be evaluated.</param>
		public new PropertyExpression<T> Like(string value)
		{
            base.Like(value);
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) NOT LIKE keywords.
		/// </summary>
		/// <param name="value">The value to be evaluated.</param>
		public new PropertyExpression<T> NotLike(string value)
		{
            base.NotLike(value);
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) Like operator with wildcards on both sides of value. 
		/// </summary>
		/// <param name="value">The value to search for.</param>
		public new PropertyExpression<T> Contains(string value)
		{
            base.Contains(value);
			return this;
		}

        /// <summary>
        /// Determines if the beginning of the string in storage matches the specified string.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        public new PropertyExpression<T> StartsWith(string value)
        {
            base.StartsWith(value);
            return this;
        }

        /// <summary>
        /// Determines if the ending of the string in storage matches the specified string.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        public new PropertyExpression<T> EndsWith(string value)
        {
            base.EndsWith(value);
            return this;
        }

		/// <summary>
		/// Represents the Data Manipulation Language (DML) Not Like operator with wildcards on both sides of value. 
		/// </summary>
		/// <param name="value">The value to search for.</param>
		public new PropertyExpression<T> NotContains(string value)
		{
            base.NotContains(value);
			return this;
		}

        /// <summary>
		/// Represents the Data Manipulation Language (DML) IS NULL keywords.
		/// </summary>
		public new PropertyExpression<T> IsNull()
		{
            base.IsNull();
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) IS NOT NULL keywords.
		/// </summary>
		public new PropertyExpression<T> IsNotNull()
		{
            base.IsNotNull();
			return this;
		}

		/// <summary>
		///  Represents the Data Manipulation Language (DML) BETWEEN keyword.
		/// </summary>
		/// <param name="value1">The lowest value.</param>
		/// <param name="value2">The highest value.</param>
		public new PropertyExpression<T> Between(object value1, object value2)
		{
            base.Between(value1, value2);
			return this;
		}

		/// <summary>
		///  Represents the Data Manipulation Language (DML) NOT BETWEEN keywords.
		/// </summary>
		/// <param name="value1">The lowest value.</param>
		/// <param name="value2">The highest value.</param>
		public new PropertyExpression<T> NotBetween(object value1, object value2)
		{
            base.NotBetween(value1, value2);
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) AS keyword.
		/// </summary>
		/// <param name="alias">The alias name.</param>
		public new PropertyExpression<T> As(string alias)
		{
            base.As(alias);
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) COUNT aggregate function.
		/// </summary>
		public new PropertyExpression<T> Count()
		{
			base.Count();
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) COUNT DISTINCT aggregate function.
		/// </summary>
		public new PropertyExpression<T> Count(bool distinct)
		{
			base.Count(distinct);
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) AVG aggregate function.
		/// </summary>
		public new PropertyExpression<T> Avg()
		{
			base.Avg();
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) MIN aggregate function.
		/// </summary>
		public new PropertyExpression<T> Min()
		{
			base.Min();
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) MAX aggregate function.
		/// </summary>
		public new PropertyExpression<T> Max()
		{
			base.Max();
			return this;
		}

		/// <summary>
		/// Converts the value to upper case.
		/// </summary>
		public new PropertyExpression<T> ToUpper()
		{
			base.ToUpper();
			return this;
		}

		/// <summary>
		/// Converts the value to lower case.
		/// </summary>
		public new PropertyExpression<T> ToLower()
		{
			base.ToLower();
			return this;
		}

		/// <summary>
		/// Removes beginning and trailing white space.
		/// </summary>
		public new PropertyExpression<T> Trim()
		{
			base.Trim();
			return this;
		}

		/// <summary>
		/// Removes beginning white space.
		/// </summary>
		public new PropertyExpression<T> TrimStart()
		{
			base.TrimStart();
			return this;
		}

		/// <summary>
		/// Removes trailing white space.
		/// </summary>
		public new PropertyExpression<T> TrimEnd()
		{
			base.TrimEnd();
			return this;
		}

		/// <summary>
		///  Represents the Data Manipulation Language (DML) SUBSTR function.
		/// </summary>
		/// <param name="startIndex">The starting first character in the string.</param>
		public new PropertyExpression<T> Substring(int startIndex)
		{
			base.Substring(startIndex);
			return this;
		}

		/// <summary>
		///  Represents the Data Manipulation Language (DML) SUBSTR function.
		/// </summary>
		/// <param name="startIndex">The starting first character in the string.</param>
		/// <param name="length">Determines how many characters to return in the string.</param>
		public new PropertyExpression<T> Substring(int startIndex, int length)
		{
			base.Substring(startIndex, length);
			return this;
		}

        /// <summary>
        /// Represents the Data Manipulation Language (DML) SUM aggregate function.
        /// </summary>
        /// <returns></returns>
        public new PropertyExpression<T> Sum()
        {
            base.Sum();
            return this;
        }

		/// <summary>
		/// Represents the Data Manipulation Language (DML) AND keyword to be used as part of a WHERE condition.
		/// </summary>
		/// <param name="expression">The additional criteria to serve as the search criteria.</param>
		public PropertyExpression<T> And(PropertyExpression<T> expression)
		{
			this.ChildExpressions.Add(expression);
			expression.LogicalExpression = LogicalOperator.And;
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) OR operator to be used as part of a WHERE condition.
		/// </summary>
		/// <param name="expression">The additional criteria to serve as the search criteria.</param>
		public PropertyExpression<T> Or(PropertyExpression<T> expression)
		{
			this.ChildExpressions.Add(expression);
			expression.LogicalExpression = LogicalOperator.Or;
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) ASC keyword.
		/// </summary>
		public new PropertyExpression<T> Asc()
		{
            base.Asc();
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) DESC operator.
		/// </summary>
		public new PropertyExpression<T> Desc()
		{
            base.Desc();
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) Length function.
		/// </summary>
		public new PropertyExpression<T> Length()
		{
			base.Length();
			return this;
		}

		/// <summary>
		/// Determines whether the current object equals obj.
		/// </summary>
		/// <param name="obj">The object to compare.</param>
		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		/// <summary>
		/// Serves as a hash function.
		/// </summary>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// Returns a <see cref="PropertyExpression{T}"/> specifiying all properties to be selected.
		/// </summary>
		public static PropertyExpression<T> All()
		{
			return new PropertyExpression<T>("*");
		}
	}
	
	/// <summary>
	/// Represents a property for an object which can be used in creating <see cref="QueryExpression{TSource}"/> queries.
	/// </summary>
	[Serializable]
	public abstract class PropertyExpression
	{
		private Type _parentType = null;
		private string _propertyName = null;
		private DataTableAttribute _dataTableAttribute = null;
		private DataColumnAttribute _dataColumnAttribute = null;
		private string _operatorExpression = string.Empty;
        private object _argumentData = null;
		private string _propertyAlias = string.Empty;
		private List<PropertyExpression> _childExpressions = new List<PropertyExpression>();
		private string _logicalOperatorExpression = string.Empty;
        private string _unaryOperatorExpression = string.Empty;
        private string _sortingExpression = string.Empty;
		private List<Function> _functions = new List<Function>();
        private bool _recurseInitFlag = false;
		
		/// <summary>
		/// Gets a boolean value indicating whether or not the <see cref="PropertyExpression"/> has been initialized.
        /// </summary>
        public virtual bool IsEmpty
		{
			get { return _dataColumnAttribute == null && _propertyName != "*"; }
		}

		/// <summary>
		/// Gets or sets the alias for the selected property.
		/// </summary>
		protected internal string PropertyAlias
		{
			get { return _propertyAlias; }
			set { _propertyAlias = value; }
		}

		/// <summary>
		/// Gets the type which exposes the property being expressed.
		/// </summary>
		protected internal Type ParentType
		{
			get { return _parentType; }
		}

		/// <summary>
		/// Gets the name of the property being expressed.
		/// </summary>
		protected internal string Name
		{
			get { return _propertyName; }
		}

		/// <summary>
		/// Gets the <see cref="DataColumnAttribute"/> for the currrent <see cref="PropertyExpression"/>.
		/// </summary>
		protected internal DataColumnAttribute DataColumnAttribute
		{
			get { return _dataColumnAttribute; }
		}

		/// <summary>
		/// Gets the <see cref="DataTableAttribute"/> of the expressed property's <see cref="DataColumnAttribute"/>.
		/// </summary>
		protected internal DataTableAttribute DataTableAttribute
		{
			get { return _dataTableAttribute; }
		}

		/// <summary>
		/// Gets the name of the column for the currrent <see cref="PropertyExpression"/>.
		/// </summary>
		protected internal string DataColumnName
		{
			get
			{
				return string.Format("{0}.{1}", _dataTableAttribute.TableName, _dataColumnAttribute == null ? _propertyName : _dataColumnAttribute.ColumnName);
			}
		}

		/// <summary>
		/// Gets or sets the operator expression for the currrent <see cref="PropertyExpression"/>.
		/// </summary>
		protected internal string OperatorExpression
		{
			get { return _operatorExpression; }
			set { _operatorExpression = value; }
		}

		/// <summary>
		/// Gets or sets the arguments specified for the currrent <see cref="PropertyExpression"/>.
		/// </summary>
		protected internal object ArgumentData
		{
			get { return _argumentData; }
			set { _argumentData = value; }
		}
		
		/// <summary>
		/// Gets or sets the functions for the currrent <see cref="PropertyExpression"/>.
		/// </summary>
		protected internal List<Function> Functions
		{
			get { return _functions; }
			set { _functions = value; }
		}

		/// <summary>
		/// Gets or sets the child expressions for the currrent <see cref="PropertyExpression"/>.
		/// </summary>
		protected internal List<PropertyExpression> ChildExpressions
		{
			get { return _childExpressions; }
			set { _childExpressions = value; }
		}

		/// <summary>
		/// Gets or sets the logical expression for the currrent <see cref="PropertyExpression"/>.
		/// </summary>
		protected internal string LogicalExpression
		{
			get { return _logicalOperatorExpression; }
			set { _logicalOperatorExpression = value; }
		}

		/// <summary>
		/// Gets or sets the unary expressions for the currrent <see cref="PropertyExpression"/>.
		/// </summary>
		protected internal string UnaryExpression
        {
            get { return _unaryOperatorExpression; }
            set { _unaryOperatorExpression = value; }
        }

		/// <summary>
		/// Gets or sets the sorting expressions for the currrent <see cref="PropertyExpression"/>.
		/// </summary>
		protected internal string SortingExpression
		{
			get { return _sortingExpression; }
			set { _sortingExpression = value; }
		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		protected internal PropertyExpression() 
        { 
        
        }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="parentType">The class that contains the specified property.</param>
        /// <param name="propertyName">The property name that is adorned with a <see cref="DataColumnAttribute"/>.</param>
		public PropertyExpression(Type parentType, string propertyName)
		{
            Initialize(parentType, propertyName);
		}

        private void Initialize(Type parentType, string propertyName)
        {
            _propertyName = propertyName;
            _parentType = parentType;
            _dataTableAttribute = DataAttributeUtilities.GetDataTableAttribute(_parentType);
            if (_propertyName == "*")
            {
                return;   
            }
            PropertyInfo property = _parentType.GetProperty(_propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property == null)
            {
                throw new ArgumentException(string.Format("The property name {0} does not exist on the type {1} as a {2}.", _propertyName, _parentType.Name, this.GetType().FullName));
            }
            DataColumnAttribute[] dataColumns = property.GetCustomAttributes(typeof(DataColumnAttribute), true) as DataColumnAttribute[];
            if (dataColumns.Length == 0)
            {
                if (_recurseInitFlag)
                {
                    throw new InvalidOperationException(string.Format("Cannot find the {0} for the member name {1}.", typeof(DataColumnAttribute).Name, propertyName));
                }
                _recurseInitFlag = true;
                string fieldName = this.ToFieldName(_propertyName);
                Initialize(parentType, fieldName);
            }
            else
            {
                _dataColumnAttribute = dataColumns[0];
            }
        }

        private string ToFieldName(string propertyName)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }
            if (propertyName.Length < 2)
            {
                return propertyName;
            }
            return "_" + propertyName.Substring(0, 1).ToLower() + propertyName.Substring(1);
        }

        /// <summary>
        /// Overrides the addition operator.
        /// </summary>
        /// <param name="expression">The expression to be added to.</param>
        /// <param name="value">The value to add.</param>
        public static PropertyExpression operator +(PropertyExpression expression, double value)
        {
            return expression.Add(value);
        }

        /// <summary>
        /// Overrides the substraction operator.
        /// </summary>
        /// <param name="expression">The expression from which to subtract.</param>
        /// <param name="value">The value to subtract.</param>
        public static PropertyExpression operator -(PropertyExpression expression, double value)
        {
            return expression.Subtract(value);
        }

        /// <summary>
        /// Overrides the multiplication operator.
        /// </summary>
        /// <param name="expression">The expression to multiply.</param>
        /// <param name="value">The value to multiply.</param>
        public static PropertyExpression operator *(PropertyExpression expression, double value)
        {
            return expression.Multiply(value);
        }

        /// <summary>
        /// Overrides the division operator.
        /// </summary>
        /// <param name="expression">The expression to divide.</param>
        /// <param name="value">The value by which to divide.</param>
        public static PropertyExpression operator /(PropertyExpression expression, double value)
        {
            return expression.Divide(value);
        }
        
		/// <summary>
		/// Overrides the equal to operator.
		/// </summary>
		/// <param name="expression">The first expression in the comparison.</param>
		/// <param name="value">The object to be compared.</param>
		public static PropertyExpression operator ==(PropertyExpression expression, object value)
		{
			return expression.EqualTo(value);
		}

		/// <summary>
		/// Overrides the not equal to operator.
		/// </summary>
		/// <param name="expression">The first expression in the comparison.</param>
		/// <param name="value">The object to be compared.</param>
		public static PropertyExpression operator !=(PropertyExpression expression, object value)
		{
			return expression.NotEqualTo(value);
		}

		/// <summary>
		/// Overrides the greater than operator.
		/// </summary>
		/// <param name="expression">The first expression in the comparison.</param>
		/// <param name="value">The object to be compared.</param>
		public static PropertyExpression operator >(PropertyExpression expression, object value)
		{
			return expression.GreaterThan(value);
		}

		/// <summary>
		/// Overrides the less than operator.
		/// </summary>
		/// <param name="expression">The first expression in the comparison.</param>
		/// <param name="value">The object to be compared.</param>
		public static PropertyExpression operator <(PropertyExpression expression, object value)
		{
			return expression.LessThan(value);
		}

		/// <summary>
		/// Overrides the greater than or equal to operator.
		/// </summary>
		/// <param name="expression">The first expression in the comparison.</param>
		/// <param name="value">The object to be compared.</param>
		public static PropertyExpression operator >=(PropertyExpression expression, object value)
		{
			return expression.GreaterThanOrEqualTo(value);
		}
                
		/// <summary>
		/// Overrides the less than or equal to operator.
		/// </summary>
		/// <param name="expression">The first expression in the comparison.</param>
		/// <param name="value">The object to be compared.</param>
		public static PropertyExpression operator <=(PropertyExpression expression, object value)
		{
			return expression.LessThanOrEqualTo(value);
		}

		/// <summary>
		/// Overrides the Or operator.
		/// </summary>
		/// <param name="expression1">The original expression to receive the and-ing.</param>
		/// <param name="expression2">The additional expression to be and-ed.</param>
		public static PropertyExpression operator &(PropertyExpression expression1, PropertyExpression expression2)
		{
			expression1.And(expression2);
			return expression1;
		}

		/// <summary>
		/// Overrides the And operator.
		/// </summary>
		/// <param name="expression1">The original expression to receive the or-ing.</param>
		/// <param name="expression2">The additional expression to be or-ed.</param>
		public static PropertyExpression operator |(PropertyExpression expression1, PropertyExpression expression2)
		{
			expression1.Or(expression2);
			return expression1;
		}

		/// <summary>
		/// Overrides the true operator.
		/// </summary>
		/// <param name="expression">The expression to be true-ed.</param>
		public static bool operator true(PropertyExpression expression)
		{
			return false;
		}

		/// <summary>
		/// Overrides the false operator.
		/// </summary>
		/// <param name="expression">The expression to be false-ed.</param>
		public static bool operator false(PropertyExpression expression)
		{
			return false;
		}

		/// <summary>
		/// Determines whether the current object equals obj.
		/// </summary>
		/// <param name="obj">The object to compare.</param>
		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		/// <summary>
		/// Serves as a hash function.
		/// </summary>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// Returns the abstracted member name of the expression if applicable, otherwise the type name.
		/// </summary>
		public override string ToString()
		{
			string retVal = base.ToString();
			if (!string.IsNullOrEmpty(_propertyName))
			{
				retVal = _propertyName;
			}
			if (!string.IsNullOrEmpty(_propertyAlias))
			{
				retVal = _propertyAlias;
			}
			return retVal;
		}

        /// <summary>
        /// Represents the addition operator.
        /// </summary>
        /// <param name="value">The value to add.</param>
        public PropertyExpression Add(double value)
        {
            this.OperatorExpression = string.Format("{0} {1} {2}", this.OperatorExpression, ArithmeticOperator.Add, value.ToString()).TrimStart();
            return this;
        }

        /// <summary>
        /// Represents the subtraction operator.
        /// </summary>
        /// <param name="value">The value to subtract.</param>
        public PropertyExpression Subtract(double value)
        {
            this.OperatorExpression = string.Format("{0} {1} {2}", this.OperatorExpression, ArithmeticOperator.Subtract, value.ToString()).TrimStart();
            return this;
        }

        /// <summary>
        /// Represents the multiplication operator.
        /// </summary>
        /// <param name="value">The value to multiply.</param>
        public PropertyExpression Multiply(double value)
        {
            this.OperatorExpression = string.Format("{0} {1} {2}", this.OperatorExpression, ArithmeticOperator.Multiply, value.ToString()).TrimStart();
            return this;
        }

        /// <summary>
        /// Represents the division operator.
        /// </summary>
        /// <param name="value">The value by which to divide.</param>
        public PropertyExpression Divide(double value)
        {
            this.OperatorExpression = string.Format("{0} {1} {2}", this.OperatorExpression, ArithmeticOperator.Divide, value.ToString()).TrimStart();
            return this;
        }

		/// <summary>
		/// Represents the equal operator.
		/// </summary>
		/// <param name="value">The value to be evaluated or the source of the set.</param>
		public PropertyExpression EqualTo(object value)
        {
            this.OperatorExpression = string.Format("{0} {1}", this.OperatorExpression, RelationalOperator.EqualTo).TrimStart();
			this.ArgumentData = value;
			return this;
		}

		/// <summary>
		/// Represents the not equal to operator.
		/// </summary>
		/// <param name="value">The value to be evaluated.</param>
        public PropertyExpression NotEqualTo(object value)
		{
            this.OperatorExpression = string.Format("{0} {1}", this.OperatorExpression, RelationalOperator.NotEqualTo).TrimStart();
			this.ArgumentData = value;
			return this;
		}

		/// <summary>
		/// Represents the greater than operator.
		/// </summary>
		/// <param name="value">The value to be evaluated.</param>
        public PropertyExpression GreaterThan(object value)
		{
            this.OperatorExpression = string.Format("{0} {1}", this.OperatorExpression, RelationalOperator.GreaterThan).TrimStart();
			this.ArgumentData = value;
			return this;
		}

		/// <summary>
		/// Represents the less than operator.
		/// </summary>
        public PropertyExpression LessThan(object value)
		{
            this.OperatorExpression = string.Format("{0} {1}", this.OperatorExpression, RelationalOperator.LessThan).TrimStart();
			this.ArgumentData = value;
			return this;
		}

		/// <summary>
		/// Represents a Data Manipulation Language (DML) IN keyword.
		/// </summary>
		/// <param name="values">The values to search for.</param>
		public PropertyExpression In(params object[] values)
		{
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            if (values.Count() == 0)
            {
                throw new ArgumentException(ErrorStrings.EmptyEnumerableArgumentException);
            }
            this.OperatorExpression = string.Format("{0} {1}", this.OperatorExpression, RelationalOperator.In).TrimStart();
			this.ArgumentData = values;
			return this;
		}

		/// <summary>
		/// Represents a Data Manipulation Language (DML) NOT IN keywords.
		/// </summary>
		/// <param name="values">The values to search for.</param>
		public PropertyExpression NotIn(params object[] values)
		{
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            if (values.Count() == 0)
            {
                throw new ArgumentException(ErrorStrings.EmptyEnumerableArgumentException);
            }
            this.OperatorExpression = string.Format("{0} {1}", this.OperatorExpression, RelationalOperator.NotIn).TrimStart();
			this.ArgumentData = values;
			return this;
		}

		/// <summary>
		/// Represents a Data Manipulation Language (DML) IN keyword.
		/// </summary>
		/// <param name="values">An <see cref="IEnumerable"/> of values.</param>
		public PropertyExpression In(IEnumerable values)
		{
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            this.OperatorExpression = string.Format("{0} {1}", this.OperatorExpression, RelationalOperator.In).TrimStart();
			List<string> strings = new List<string>();
			foreach (var item in values) 
            { 
                strings.Add(item.ToString()); 
            }
            if (strings.Count > 0)
            {
                this.ArgumentData = strings.ToArray();
            }
            else
            {
                throw new ArgumentException(ErrorStrings.EmptyEnumerableArgumentException);
            }
			return this;
		}

		/// <summary>
		/// Represents a Data Manipulation Language (DML) NOT IN keywords.
		/// </summary>
		/// <param name="values">An <see cref="IEnumerable"/> of values.</param>
		public PropertyExpression NotIn(IEnumerable values)
		{
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }
            this.OperatorExpression = string.Format("{0} {1}", this.OperatorExpression, RelationalOperator.NotIn).TrimStart();
			List<string> strings = new List<string>();
			foreach (var item in values) 
            { 
                strings.Add(item.ToString()); 
            }
            if (strings.Count > 0)
            {
                this.ArgumentData = strings.ToArray();
            }
            else
            {
                throw new ArgumentException(ErrorStrings.EmptyEnumerableArgumentException);
            }
			return this;
		}

		/// <summary>
		/// Represents the greater than or equal to operator.
		/// </summary>
		/// <param name="value">The value to be evaluated.</param>
        public PropertyExpression GreaterThanOrEqualTo(object value)
		{
            this.OperatorExpression = string.Format("{0} {1}", this.OperatorExpression, RelationalOperator.GreaterThanOrEqualTo).TrimStart();
			this.ArgumentData = value;
			return this;
		}

        /// <summary>
		/// Represents the less than or equal to operator.
		/// </summary>
		/// <param name="value">The value to be evaluated.</param>
        public PropertyExpression LessThanOrEqualTo(object value)
		{
            this.OperatorExpression = string.Format("{0} {1}", this.OperatorExpression, RelationalOperator.LessThanOrEqualTo).TrimStart();
			this.ArgumentData = value;
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) LIKE keyword. Note you must provide a wild-card character when
        /// calling this method.
		/// </summary>
		/// <param name="value">The value to be evaluated.</param>
		public PropertyExpression Like(string value)
		{
            this.OperatorExpression = string.Format("{0} {1}", this.OperatorExpression, RelationalOperator.Like).TrimStart();
            this.ArgumentData = value;
			return this;
		}

		/// <summary>
        /// Represents the Data Manipulation Language (DML) NOT LIKE keywords. Note you must provide a wild-card character when
        /// calling this method.
		/// </summary>
		/// <param name="value">The value to be evaluated.</param>
		public PropertyExpression NotLike(string value)
		{
			this.OperatorExpression = string.Format("{0} {1}", this.OperatorExpression, RelationalOperator.NotLike).TrimStart();
            this.ArgumentData = value;
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) Like operator with wildcards on both sides of value. 
		/// </summary>
		/// <param name="value">The value to search for.</param>
		public PropertyExpression Contains(string value)
		{
			this.OperatorExpression = string.Format("{0} {1}", this.OperatorExpression, RelationalOperator.Like).TrimStart();
			this.ArgumentData = string.Format("%{0}%", value);
			return this;
		}

        /// <summary>
        /// Determines if the beginning of the string in storage matches the specified string.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        public PropertyExpression StartsWith(string value)
        {
            value = value == null ? string.Empty : value;
            this.Like(value + "%");
            return this;
        }

        /// <summary>
        /// Determines if the ending of the string in storage matches the specified string.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        public PropertyExpression EndsWith(string value)
        {
            value = value == null ? string.Empty : value;
            this.Like("%" + value);
            return this;
        }

		/// <summary>
		/// Represents the Data Manipulation Language (DML) Not Like operator with wildcards on both sides of value. 
		/// </summary>
		/// <param name="value">The value to search for.</param>
		public PropertyExpression NotContains(string value)
		{
			this.OperatorExpression = string.Format("{0} {1}", this.OperatorExpression, RelationalOperator.NotLike).TrimStart();
			this.ArgumentData = string.Format("%{0}%", value);
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) IS NULL keywords.
		/// </summary>
		public PropertyExpression IsNull()
		{
			this.OperatorExpression = string.Format("{0} {1}", this.OperatorExpression, RelationalOperator.IsNull).TrimStart();
			this.ArgumentData = string.Empty;
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) IS NOT NULL keywords.
		/// </summary>
		public PropertyExpression IsNotNull()
		{
			this.OperatorExpression = string.Format("{0} {1}", this.OperatorExpression, RelationalOperator.IsNotNull).TrimStart();
			this.ArgumentData = string.Empty;
			return this;
		}

		/// <summary>
		///  Represents the Data Manipulation Language (DML) BETWEEN keyword.
		/// </summary>
		/// <param name="value1">The lowest value.</param>
		/// <param name="value2">The highest value.</param>
		public PropertyExpression Between(object value1, object value2)
		{
            this.OperatorExpression = string.Format("{0} {1}", this.OperatorExpression, RelationalOperator.Between).TrimStart();
			this.ArgumentData = new object[2] { value1, value2 };
			return this;
		}

		/// <summary>
		///  Represents the Data Manipulation Language (DML) NOT BETWEEN keywords.
		/// </summary>
		/// <param name="value1">The lowest value.</param>
		/// <param name="value2">The highest value.</param>
		public PropertyExpression NotBetween(object value1, object value2)
		{
            this.OperatorExpression = string.Format("{0} {1}", this.OperatorExpression, RelationalOperator.NotBetween).TrimStart();
			this.ArgumentData = new object[2] { value1, value2 };
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) AS keyword.
		/// </summary>
		/// <param name="alias">The alias name.</param>
		public PropertyExpression As(string alias)
		{
			this.PropertyAlias = alias;
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) COUNT aggregate function.
		/// </summary>
		public PropertyExpression Count()
		{
			this.Functions.Add(new Function(FunctionType.Count, null));
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) COUNT DISTINCT aggregate function.
		/// </summary>
		public PropertyExpression Count(bool distinct)
		{
			FunctionType type = distinct ? FunctionType.CountDistinct : FunctionType.Count;
			this.Functions.Add(new Function(type, null));
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) AVG aggregate function.
		/// </summary>
		public PropertyExpression Avg()
		{
			this.Functions.Add(new Function(FunctionType.Avg, null));
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) MIN aggregate function.
		/// </summary>
		public PropertyExpression Min()
		{
            this.Functions.Add(new Function(FunctionType.Min, null));
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) MAX aggregate function.
		/// </summary>
		public PropertyExpression Max()
		{
			this.Functions.Add(new Function(FunctionType.Max, null));
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) Length function.
		/// </summary>
		public PropertyExpression Length()
		{
			this.Functions.Add(new Function(FunctionType.Len, null));
			return this;
		}

		/// <summary>
		/// Converts the value to upper case.
		/// </summary>
		public PropertyExpression ToUpper()
		{
			this.Functions.Add(new Function(FunctionType.Upper, null));
			return this;
		}

		/// <summary>
		/// Converts the value to lower case.
		/// </summary>
		public PropertyExpression ToLower()
		{
			this.Functions.Add(new Function(FunctionType.Lower, null));
			return this;
		}

		/// <summary>
		/// Removes beginning and trailing white space.
		/// </summary>
		public PropertyExpression Trim()
		{
			this.Functions.Add(new Function(FunctionType.Trim, null));
			return this;
		}

		/// <summary>
		/// Removes beginning white space.
		/// </summary>
		public PropertyExpression TrimStart()
		{
			this.Functions.Add(new Function(FunctionType.LTrim, null));
			return this;
		}

		/// <summary>
		/// Removes trailing white space.
		/// </summary>
		public PropertyExpression TrimEnd()
		{
			this.Functions.Add(new Function(FunctionType.RTrim, null));
			return this;
		}

		/// <summary>
		///  Represents the Data Manipulation Language (DML) SUBSTR function.
		/// </summary>
		/// <param name="startIndex">The starting first character in the string.</param>
		public PropertyExpression Substring(int startIndex)
		{
			this.Functions.Add(new Function(FunctionType.Substr, startIndex));
			return this;
		}

		/// <summary>
		///  Represents the Data Manipulation Language (DML) SUBSTR function.
		/// </summary>
		/// <param name="startIndex">The starting first character in the string.</param>
		/// <param name="length">Determines how many characters to return in the string.</param>
		public PropertyExpression Substring(int startIndex, int length)
		{
			this.Functions.Add(new Function(FunctionType.Substr, startIndex, length));
			return this;
		}

        /// <summary>
        /// Represents the Data Manipulation Language (DML) SUM aggregate function.
        /// </summary>
        /// <returns></returns>
        public PropertyExpression Sum()
        {
            this.Functions.Add(new Function(FunctionType.Sum, null));
            return this;
        }

		/// <summary>
		/// Represents the Data Manipulation Language (DML) AND keyword to be used as part of a WHERE condition.
		/// </summary>
		/// <param name="expression">The additional criteria to serve as the search criteria.</param>
		public PropertyExpression And(PropertyExpression expression)
		{
			this.ChildExpressions.Add(expression);
			expression.LogicalExpression = LogicalOperator.And;
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) OR operator to be used as part of a WHERE condition.
		/// </summary>
		/// <param name="expression">The additional criteria to serve as the search criteria.</param>
		public PropertyExpression Or(PropertyExpression expression)
		{
			this.ChildExpressions.Add(expression);
			expression.LogicalExpression = LogicalOperator.Or;
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) ASC keyword.
		/// </summary>
		public PropertyExpression Asc()
		{
			this.SortingExpression = SortingOperator.Asc;
			return this;
		}

		/// <summary>
		/// Represents the Data Manipulation Language (DML) DESC operator.
		/// </summary>
		public PropertyExpression Desc()
		{
			this.SortingExpression = SortingOperator.Desc;
			return this;
		}

		/// <summary>
		/// Gets the <see cref="PropertyExpressionType"/> for the current <see cref="PropertyExpression"/>.
		/// </summary>
        protected internal PropertyExpressionType ExpressionType
        {
            get 
            {
                if (this.ArgumentData is PropertyExpression)
                {
                    return PropertyExpressionType.Join;
                }
                if (this.Functions.Count > 0)
                {
                    return PropertyExpressionType.Function;
                }
                return PropertyExpressionType.Operator;
            }
        }
	}

	[Serializable]
	internal class RelationalOperator
	{
        public const string EqualTo = "=";
        public const string NotEqualTo = "<>";
        public const string GreaterThan = ">";
        public const string LessThan = "<";
        public const string GreaterThanOrEqualTo = ">=";
        public const string LessThanOrEqualTo = "<=";
        public const string In = "IN";
        public const string NotIn = "NOT IN";
        public const string Between = "BETWEEN";
        public const string NotBetween = "NOT BETWEEN";
        public const string IsNull = "IS NULL";
        public const string IsNotNull = "IS NOT NULL";
        public const string Like = "LIKE";
        public const string NotLike = "NOT LIKE";

        public static string Parse(string value)
        {
            string retVal = string.Empty;
            Match match = Regex.Match(value, "[^*/+-0-9]+");
            while (match.Success)
            {
                retVal += string.Format(" {0}", match.Value.Trim());
                match = match.NextMatch();
            }
            return retVal.Trim();
        }
	}

	/// <summary>
	/// Represents a supported Data Manipulation Language (DML) function.
	/// </summary>
	public enum FunctionType
	{
		/// <summary>
		/// Avg aggregate function.
		/// </summary>
		Avg,
		/// <summary>
		/// Min aggregate function.
		/// </summary>
		Min,
		/// <summary>
		/// Max aggregate function.
		/// </summary>
		Max,
		/// <summary>
		/// Count aggregate function.
		/// </summary>
		Count,
		/// <summary>
		/// Count dstinct aggregate function.
		/// </summary>
		CountDistinct,
		/// <summary>
		/// Length function.
		/// </summary>
		Len,
		/// <summary>
		/// Upper case function.
		/// </summary>
		Upper,
		/// <summary>
		/// Lower case function.
		/// </summary>
		Lower,
		/// <summary>
		/// Trim function.
		/// </summary>
		Trim,
		/// <summary>
		/// Left trim function.
		/// </summary>
		LTrim,
		/// <summary>
		/// Right trim function.
		/// </summary>
		RTrim,
		/// <summary>
		/// Substring function.
		/// </summary>
		Substr,
        /// <summary>
        /// Sum aggregate function.
        /// </summary>
        Sum
	}

	/// <summary>
	/// Represents the interpreted type of <see cref="PropertyExpression"/>.
	/// </summary>
    public enum PropertyExpressionType
    {
        /// <summary>
        /// A Join expression.
        /// </summary>
		Join,
		/// <summary>
		/// An Operator expression.
		/// </summary>
        Operator,
		/// <summary>
		/// A function expression.
		/// </summary>
        Function
    }

	/// <summary>
	/// Represents an abstraction of a database function.
	/// </summary>
	[Serializable]
	[DebuggerDisplay("{FunctionName}")]
	public class Function
	{
		private FunctionType _functionType;
		private object[] _parameters = new object[0];
		
		/// <summary>
		/// Gets the <see cref="FunctionType"/> for the current <see cref="Function"/>.
		/// </summary>
		public FunctionType FunctionType 
		{
			get { return _functionType; }
		}
		
		/// <summary>
		/// Gets the parameters of the current <see cref="Function"/>.
		/// </summary>
		public object[] Parameters 
		{
			get { return _parameters; }
		}
		
		/// <summary>
		/// Gets the name of the current <see cref="Function"/>.
		/// </summary>
		public string FunctionName 
		{ 
			get { return this.FunctionType.ToString().ToUpper(); } 
		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="functionType">The type of function.</param>
		/// <param name="parameters">The parameters for the function.</param>
		public Function(FunctionType functionType, params object[] parameters)
		{
			_functionType = functionType;
			if (parameters != null)
			{
				_parameters = parameters;
			}
		}
	}

    [Serializable]
    internal class ArithmeticOperator
    {
        public const string Add = "+";
        public const string Subtract = "-";
        public const string Multiply = "*";
        public const string Divide = "/";
    }

	[Serializable]
	internal class LogicalOperator
	{
        public const string And = "AND";
        public const string Or = "OR";
	}

	[Serializable]
	internal class SortingOperator
	{
        public const string Asc = "ASC";
        public const string Desc = "DESC";
	}
	
}
