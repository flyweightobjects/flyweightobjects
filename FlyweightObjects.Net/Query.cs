//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
//  Author:         Marcus Crane, Software Developer / Architect                                        //
//  E-mail:         mailto:marcus_crane@FlyweightObjects                                                //
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
	/// Represents a query to issued to the database/
	/// </summary>
	/// <typeparam name="T">The type the query should return.</typeparam>
	public static class Query<T> where T : class, IFlyweight, new()
	{
		/// <summary>
		/// Returns an <see cref="IQueryExpression{T}"/>.
		/// </summary>
		/// <returns></returns>
		public static IQueryExpression<T> With()
		{
			return new QueryExpression<T>();
		}
	}
}
