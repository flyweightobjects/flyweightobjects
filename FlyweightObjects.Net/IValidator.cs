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
	/// Represents an interface responsible for validating a domain object prior to it being persisted or deleted.
	/// </summary>
	/// <typeparam name="T">The type of object to validate.</typeparam>
	public interface IValidator<T> where T : class, IFlyweight, new()
	{
		/// <summary>
		/// Returns a boolean value indicating whether the domain object can be inserted.
		/// </summary>
		/// <param name="source">The object to validate.</param>
		bool CanInsert(T source);

		/// <summary>
        /// Returns a boolean value indicating whether the domain object can be updated.
		/// </summary>
		/// <param name="source">The object to validate.</param>
		bool CanUpdate(T source);

		/// <summary>
        /// Returns a boolean value indicating whether the domain object can be updated.
		/// </summary>
		/// <param name="source">The object to deleted.</param>
		bool CanDelete(T source);
	}
}
