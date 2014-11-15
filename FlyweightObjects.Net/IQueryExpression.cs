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
	/// Represents a provider independent query.
	/// </summary>
	public interface IQueryExpression<TSource> where TSource : class, IFlyweight, new()
	{
        /// <summary>
        /// Gets or sets  the <see cref="Pagination"/> for the <see cref="IQueryExpression{T}"/>.
        /// </summary>
        Pagination Pagination { get; set; }
        
        /// <summary>
		/// Represents the SELECT keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to select.</param>
        IQueryExpression<TSource> Select(params PropertyExpression[] properties);

        /// <summary>
        /// Represents the SELECT keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="limit">The maximum number of objects to return.</param>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to select.</param>
        IQueryExpression<TSource> Select(int limit, params PropertyExpression[] properties);
        
        /// <summary>
        /// Represents the SELECT keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="distinct">Determines whether a distinct set should be returned.</param>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to select.</param>
        IQueryExpression<TSource> Select(bool distinct, params PropertyExpression[] properties);

        /// <summary>
		/// Represents the SELECT keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="distinct">Determines whether a distinct set should be returned.</param>
		/// <param name="limit">The maximum number of objects to return.</param>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to select.</param>
		IQueryExpression<TSource> Select(bool distinct, int limit, params PropertyExpression[] properties);
		
		/// <summary>
        /// Represents the INSERT INTO keywords of a Data Manipulation Language (DML) statement.
        /// </summary>
        IQueryExpression<TSource> Insert();

		/// <summary>
		/// Represents the INSERT INTO keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="properties">An array of <see cref="PropertyExpression"/> members to be inserted.</param>
        IQueryExpression<TSource> Insert(params PropertyExpression<TSource>[] properties);
		
		/// <summary>
		/// Represents the UPDATE keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
        IQueryExpression<TSource> Update();
		
		/// <summary>
		/// Represents the DELETE keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
        IQueryExpression<TSource> Delete();
		
		/// <summary>
		/// Represents the FROM keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <typeparam name="T">The type to serve as the source of From.</typeparam>
		IQueryExpression<TSource> From<T>() where T : class, IFlyweight, new();
		
		/// <summary>
		/// Represents the SET keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="properties">An array of generic <see cref="PropertyExpression{T}"/> members whose values should be set.</param>
		IQueryExpression<TSource> Set(params PropertyExpression<TSource>[] properties);
		
		/// <summary>
		/// Represents the VALUES keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="values">The values to be inserted.</param>
		IQueryExpression<TSource> Values(params object[] values);

		/// <summary>
		/// Represents the INNER JOIN keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <typeparam name="T">The type to join.</typeparam>
		/// <param name="joinExpression">A property expression in the form of A=B.</param>
		IQueryExpression<TSource> Join<T>(PropertyExpression joinExpression) where T : class, IFlyweight, new();
		
		/// <summary>
		/// Represents the LEFT OUTER JOIN keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <typeparam name="T">The type to join.</typeparam>
		/// <param name="joinExpression">A property expression in the form of A=B.</param>
		IQueryExpression<TSource> LeftJoin<T>(PropertyExpression joinExpression) where T : class, IFlyweight, new();
		
		/// <summary>
		/// Represents the RIGHT OUTER JOIN keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="joinExpression">A property expression in the form of A=B.</param>
		IQueryExpression<TSource> RightJoin<T>(PropertyExpression joinExpression) where T : class, IFlyweight, new();
		
		/// <summary>
		/// Represents the FULL OUTER JOIN keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <typeparam name="T">The type to join.</typeparam>
		/// <param name="joinExpression">A property expression in the form of A=B.</param>
		IQueryExpression<TSource> FullJoin<T>(PropertyExpression joinExpression) where T : class, IFlyweight, new();

        /// <summary>
        /// Represents the CROSS JOIN keywords of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <typeparam name="T">The type to join.</typeparam>
        IQueryExpression<TSource> CrossJoin<T>() where T : class, IFlyweight, new();

		/// <summary>
		/// Represents the WHERE keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="whereExpression">The crtieria used to limit the results of the query.</param>
		IQueryExpression<TSource> Where(PropertyExpression whereExpression);
		
		/// <summary>
		/// Represents the GROUP BY keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="properties">An array of <see cref="PropertyExpression"/> members by which to group.</param>
		IQueryExpression<TSource> GroupBy(params PropertyExpression[] properties);
		
		/// <summary>
		/// Represents the HAVING keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="havingExpression">The expression which limits an aggregate function.</param>
		IQueryExpression<TSource> Having(PropertyExpression havingExpression);
		
		/// <summary>
		/// Represents the UNION keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		IQueryExpression<TSource> Union();
		
		/// <summary>
		/// Represents the ORDER BY keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="sortExpressions">An array of <see cref="PropertyExpression"/> members by which to sort.</param>
		IQueryExpression<TSource> OrderBy(params PropertyExpression[] sortExpressions);
		
		/// <summary>
		/// Represents the ORDER BY keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="ordinal">The ordinal postiion by which to sort.</param>
		IQueryExpression<TSource> OrderBy(int ordinal);
		
		/// <summary>
		/// Represents the ORDER BY keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="ordinals">An array of ordinals by which to sort.</param>
		IQueryExpression<TSource> OrderBy(params int[] ordinals);
		
		/// <summary>
		/// Represents the ORDER BY keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="alias">The alias by which to sort.</param>
		IQueryExpression<TSource> OrderBy(string alias);
		
		/// <summary>
		/// Represents the DESC keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		IQueryExpression<TSource> Desc();
		
		/// <summary>
		/// Represents the ASC keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		IQueryExpression<TSource> Asc();
		
		/// <summary>
		/// Represents the AND keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="whereExpression">The crtieria used to limit the statement.</param>
		/// <seealso cref="Where"/>
		IQueryExpression<TSource> And(PropertyExpression whereExpression);
		
		/// <summary>
		/// Represents the OR keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="whereExpression">The crtieria used to limit the statement.</param>
		/// <seealso cref="Where"/>
		IQueryExpression<TSource> Or(PropertyExpression whereExpression);

        /// <summary>
        /// Returns an instance of an <see cref="IStorageCommand"/>.
        /// </summary>
        IStorageCommand ToCommand();
	}
}
