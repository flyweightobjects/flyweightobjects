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
using System.Data;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents transactional functionality.
	/// </summary>
	public interface ITransactional
	{
		/// <summary>
		/// Starts a new <see cref="IDbTransaction"/> if one is not already present, otherwise it will enlist.
		/// </summary>
		IDbTransaction BeginTransaction();
		
		/// <summary>
		/// Rolls back all changes to the current <see cref="IDbTransaction"/>.
		/// </summary>
		void RollbackTransaction();

		/// <summary>
		/// Commits the current transaction to storage if the current implementing class is the owner.
		/// </summary>
		void CommitTransaction();

        /// <summary>
        /// Commits the current transaction to storage. If force is equal to true, the transaction will be commited even though the current instance may not be the owner.
        /// </summary>
        /// <param name="force">A value indicating whether to force the commit regardless of ownership.</param>
        void CommitTransaction(bool force);

		/// <summary>
		/// Gets the current active transaction if one if present, otherwise returns null.
		/// </summary>
		IDbTransaction ActiveTransaction { get; }
	}
}
