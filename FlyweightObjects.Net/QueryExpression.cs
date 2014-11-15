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
using System.Reflection;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents a provider independent query.
	/// </summary>
    /// <example>
    /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
    /// <code>
    /// <![CDATA[
    /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
    /// {
    ///     var qe = new QueryExpression<Product>()
    ///
    ///     .Select(true, 0, ProductCategory.Properties.Name)
    ///     .From<Product>()
    ///     .Join<ProductCategory>(Product.Properties.ProductSubcategoryID == ProductCategory.Properties.ProductCategoryID)
    ///     .Where(Product.Properties.ProductID.Between(350, 360))
    ///     .OrderBy(ProductCategory.Properties.Name);
    ///
    ///     var products = context.Select<Product>(qe);
    ///     Console.WriteLine("Count is {0}", products.Count<Product>());
    /// }
    /// ]]>
    /// </code>
    /// </example>
	[Serializable]
    public class QueryExpression<TSource> : IQueryExpression<TSource>, IRuntimeMethodQuery<TSource> where TSource : class, IFlyweight, new()
	{
		private Queue<RuntimeMethod> _methodQueue = new Queue<RuntimeMethod>();
        private IStorageCommand _command = null;
        private Pagination _pagination = null;

        /// <summary>
        /// Gets or sets the pagination details for the query.
        /// </summary>
        public Pagination Pagination
        {
            get { return _pagination; }
            set { _pagination = value; }
        }

        /// <summary>
        /// Gets a generic Queue of <see cref="RuntimeMethod"/> representing the methods called to construct a query.
        /// </summary>
        Queue<RuntimeMethod> IRuntimeMethodQuery<TSource>.MethodQueue 
        {
            get { return _methodQueue; }
        }

        /// <summary>
        /// Gets or sets the <see cref="IStorageCommand"/> runtime interpretation.
        /// </summary>
        IStorageCommand IRuntimeMethodQuery<TSource>.Command
        {
            get { return _command; }
            set { _command = value; }
        }

        /// <summary>
        /// Represents the SELECT keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to select.</param>
        public IQueryExpression<TSource> Select(params PropertyExpression[] properties)
        {
            RuntimeMethod method = new RuntimeMethod(MethodBase.GetCurrentMethod());
            method.TypeParameters.Add(typeof(TSource));
            PropertyExpression[] propertyExpressions = properties;
            method.MethodArguments.Add(propertyExpressions);
            _methodQueue.Enqueue(method);
            return this;
        }

        /// <summary>
        /// Represents the SELECT keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="limit">The maximum number of objects to return.</param>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to select.</param>
        public IQueryExpression<TSource> Select(int limit, params PropertyExpression[] properties)
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), limit);
            PropertyExpression[] propertyExpressions = properties;
            method.MethodArguments.Add(propertyExpressions);
            _methodQueue.Enqueue(method);
            return this;
        }

        /// <summary>
        /// Represents the SELECT keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="distinct">Determines whether a distinct set should be returned.</param>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to select.</param>
        public IQueryExpression<TSource> Select(bool distinct, params PropertyExpression[] properties)
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), distinct);
            PropertyExpression[] propertyExpressions = properties;
            method.MethodArguments.Add(propertyExpressions);
            _methodQueue.Enqueue(method);
            return this;
        }

        /// <summary>
        /// Represents the SELECT keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="distinct">Determines whether a distinct set should be returned.</param>
        /// <param name="limit">The maximum number of objects to return.</param>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to select.</param>
        public IQueryExpression<TSource> Select(bool distinct, int limit, params PropertyExpression[] properties)
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), distinct, limit);
            PropertyExpression[] propertyExpressions = properties;
            method.MethodArguments.Add(propertyExpressions);
            _methodQueue.Enqueue(method);
            return this;
        }
		
        /// <summary>
        /// Represents the INSERT INTO keywords of a Data Manipulation Language (DML) statement.
        /// </summary>
        public IQueryExpression<TSource> Insert()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod());
            method.TypeParameters.Add(typeof(TSource));
            _methodQueue.Enqueue(method);
            return this;
        }

		/// <summary>
		/// Represents the INSERT INTO keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="properties">An array of <see cref="PropertyExpression"/> members to be inserted.</param>
		public IQueryExpression<TSource> Insert(params PropertyExpression<TSource>[] properties)
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod());
			PropertyExpression<TSource>[] propertyExpressions = properties;
			method.MethodArguments.Add(propertyExpressions);
			method.TypeParameters.Add(typeof(TSource));
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the UPDATE keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		public IQueryExpression<TSource> Update()
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod());
			method.TypeParameters.Add(typeof(TSource));
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the DELETE keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		public IQueryExpression<TSource> Delete()
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod());
			method.TypeParameters.Add(typeof(TSource));
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the FROM keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <typeparam name="T">The type to serve as the source of From.</typeparam>
		public IQueryExpression<TSource> From<T>() where T : class, IFlyweight, new()
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod());
			method.TypeParameters.Add(typeof(T));
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the SET keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="properties">An array of generic <see cref="PropertyExpression"/> members whose values should be set.</param>
		public IQueryExpression<TSource> Set(params PropertyExpression<TSource>[] properties)
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod());
            PropertyExpression<TSource>[] propertyExpressions = properties;
			method.MethodArguments.Add(propertyExpressions);
			method.TypeParameters.Add(typeof(TSource));
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the VALUES keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="values">The values to be inserted.</param>
		public IQueryExpression<TSource> Values(params object[] values)
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod());
			object[] parameters = values;
			method.MethodArguments.Add(parameters);
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the INNER JOIN keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <typeparam name="T">The type to join.</typeparam>
		/// <param name="joinExpression">A property expression in the form of A=B.</param>
		public IQueryExpression<TSource> Join<T>(PropertyExpression joinExpression) where T : class, IFlyweight, new()
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), joinExpression);
			method.TypeParameters.Add(typeof(T));
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the LEFT OUTER JOIN keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <typeparam name="T">The type to join.</typeparam>
		/// <param name="joinExpression">A property expression in the form of A=B.</param>
		public IQueryExpression<TSource> LeftJoin<T>(PropertyExpression joinExpression) where T : class, IFlyweight, new()
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), joinExpression);
			method.TypeParameters.Add(typeof(T));
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the RIGHT OUTER JOIN keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="joinExpression">A property expression in the form of A=B.</param>
		public IQueryExpression<TSource> RightJoin<T>(PropertyExpression joinExpression) where T : class, IFlyweight, new()
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), joinExpression);
			method.TypeParameters.Add(typeof(T));
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the FULL OUTER JOIN keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <typeparam name="T">The type to join.</typeparam>
		/// <param name="joinExpression">A property expression in the form of A=B.</param>
		public IQueryExpression<TSource> FullJoin<T>(PropertyExpression joinExpression) where T : class, IFlyweight, new()
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), joinExpression);
			method.TypeParameters.Add(typeof(T));
			_methodQueue.Enqueue(method);
			return this;
		}

        /// <summary>
        /// Represents the CROSS JOIN keywords of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <typeparam name="T">The type to join.</typeparam>
        public IQueryExpression<TSource> CrossJoin<T>() where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T));
            _methodQueue.Enqueue(method);
            return this;
        }

		/// <summary>
		/// Represents the GROUP BY keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="properties">An array of <see cref="PropertyExpression"/> members by which to group.</param>
		public IQueryExpression<TSource> GroupBy(params PropertyExpression[] properties)
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod());
			PropertyExpression[] propertyExpressions = properties;
			method.MethodArguments.Add(propertyExpressions);
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the Group By All keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="properties">An array of <see cref="PropertyExpression"/> members by which to group.</param>
		public IQueryExpression<TSource> GroupByAll(params PropertyExpression[] properties)
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), properties);
			PropertyExpression[] propertyExpressions = properties;
			method.MethodArguments.Add(propertyExpressions);
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the HAVING keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="havingExpression">The expression which limits an aggregate function.</param>
		public IQueryExpression<TSource> Having(PropertyExpression havingExpression)
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), havingExpression);
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the UNION keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		public IQueryExpression<TSource> Union()
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod());
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the Union All keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		public IQueryExpression<TSource> UnionAll()
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod());
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the WHERE keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="whereExpression">The crtieria used to limit the results of the query.</param>
		public IQueryExpression<TSource> Where(PropertyExpression whereExpression)
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), whereExpression);
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the ORDER BY keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="sortExpressions">An array of <see cref="PropertyExpression"/> members by which to sort.</param>
		public IQueryExpression<TSource> OrderBy(params PropertyExpression[] sortExpressions)
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod());
			PropertyExpression[] propertyExpressions = sortExpressions;
			method.MethodArguments.Add(propertyExpressions);
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the ORDER BY keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="ordinal">The ordinal postiion by which to sort.</param>
		public IQueryExpression<TSource> OrderBy(int ordinal)
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), ordinal);
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the ORDER BY keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="ordinals">An array of ordinals by which to sort.</param>
		public IQueryExpression<TSource> OrderBy(params int[] ordinals)
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod());
			int[] values = ordinals;
			method.MethodArguments.Add(values);
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the ORDER BY keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="alias">The alias by which to sort.</param>
		public IQueryExpression<TSource> OrderBy(string alias)
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), alias);
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the DESC keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		public IQueryExpression<TSource> Desc()
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod());
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the ASC keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		public IQueryExpression<TSource> Asc()
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod());
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the AND keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="whereExpression">The crtieria used to limit the statement.</param>
		public IQueryExpression<TSource> And(PropertyExpression whereExpression)
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), whereExpression);
			_methodQueue.Enqueue(method);
			return this;
		}

		/// <summary>
		/// Represents the OR keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="whereExpression">The crtieria used to limit the statement.</param>
		/// <seealso cref="Where"/>
		public IQueryExpression<TSource> Or(PropertyExpression whereExpression)
		{
			RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), whereExpression);
			_methodQueue.Enqueue(method);
			return this;
		}

        /// <summary>
        /// Returns an instance of an <see cref="IStorageCommand"/>.
        /// </summary>
        public IStorageCommand ToCommand()
        {
            throw new InvalidOperationException(ErrorStrings.InvalidToCommandConversionOperation);
        }

        /// <summary>
        /// Returns an instance of an <see cref="IStorageCommand"/>.
        /// </summary>
        /// <param name="queryBuilder">The <see cref="IQueryBuilder{TSource}"/> used to interpret the query.</param>
        public IStorageCommand ToCommand(IQueryBuilder<TSource> queryBuilder)
        {
            if (queryBuilder == null)
            {
                throw new ArgumentNullException("queryBuilder");
            }
            return queryBuilder.BuildStorageCommand((IRuntimeMethodQuery<TSource>)this);
        }

        /// <summary>
        /// Returns a String that represents the current Object.
        /// </summary>
        public override string ToString()
        {
            if (_command != null)
			{
				return _command.SqlText;
			}
            return base.ToString();
        }
	}
}
