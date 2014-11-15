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
    /// Represents an <see cref="IQueryExpression{TSource}"/> definition which is constructed at runtime.
	/// </summary>
	public interface IRuntimeMethodQuery<TSource> : IQueryExpression<TSource> where TSource : class, IFlyweight, new()
	{
        /// <summary>
        /// Gets a generic Queue of <see cref="RuntimeMethod"/> representing the methods called to construct a query.
        /// </summary>
        Queue<RuntimeMethod> MethodQueue { get; }
        
        /// <summary>
		/// Gets or sets the <see cref="IStorageCommand"/> runtime interpretation.
		/// </summary>
		IStorageCommand Command { get; set; }
	}
}
