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
	/// Instructs an <see cref="DataContext"/> how to handle concurrency violations.
	/// </summary>
	public enum ConcurrencyViolationMode
	{
		/// <summary>
		/// Overwrite any conflicts.
		/// </summary>
		IgnoreConflict,
		/// <summary>
		/// Throw an exception when a conflict is found.
		/// </summary>
		FailOnConflict
	}
	
	/// <summary>
	/// Determines change conflicts in a relational data store.
	/// </summary>
	public interface IConcurrencyManager
	{
		/// <summary>
		/// Returns a boolean value indicating whether the local copy of the object is newer than the one in storage.
		/// </summary>
		/// <typeparam name="T">The type of object to evaluate.</typeparam>
		/// <param name="source">An instance of T.</param>
		bool IsCurrent<T>(T source) where T : class, IFlyweight, new();

		/// <summary>
        /// Gets a <see cref="IDataContext"/> by which storage operations can be performed to determine concurrency.
		/// </summary>
		IDataContext Context { get; }

		/// <summary>
		/// Gets or sets the <see cref="ConcurrencyViolationMode"/> under which the manager should work.
		/// </summary>
		ConcurrencyViolationMode Mode { get; set; }
	}
}
