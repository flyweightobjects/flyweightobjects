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
    /// Represents a provider in the interpretation of an <see cref="IRuntimeMethodQuery{TSource}"/> object.
	/// </summary>
    public interface IQueryBuilder<TSource> : IQueryExpression<TSource> where TSource : class, IFlyweight, new()
	{
		/// <summary>
		/// Gets the prefix characters to prepend to a parameter.
		/// </summary>
		string ParameterPrefix { get; }

		/// <summary>
		/// Gets a proper function name by <see cref="FunctionType"/>.
		/// </summary>
		/// <param name="functionType">The type of supported function.</param>
		string GetFunctionName(FunctionType functionType);

		/// <summary>
        /// Iterates a <see cref="IRuntimeMethodQuery{TSource}"/> instance and returns an <see cref="IStorageCommand"/>.
		/// </summary>
        /// <param name="query">The query to be processed.</param>
		IStorageCommand BuildStorageCommand(IRuntimeMethodQuery<TSource> query);
	}
}
