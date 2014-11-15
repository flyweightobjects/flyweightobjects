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
using System.Security.Principal;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents the required members for a generic business logic class.
	/// <typeparam name="TSource">The type of object for which the buisness logic implementation represents.</typeparam>
    /// </summary>
    public interface IBusinessLogic<TSource> : IBusinessLogic where TSource  : class, IFlyweight, new()
    {
		/// <summary>
		/// Returns all objects for the specified type of T in storage using the supplied <see cref="IStorageCommand"/>.  Note the value for 
		/// sql may be any object which implements the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
		/// </summary>
        /// <param name="command">The <see cref="IStorageCommand"/> by which to retrieve the objects.</param>
		IEnumerable<TSource> Select(IStorageCommand command);

		/// <summary>
		/// Performs an insert into the target entity with the values as given by the source object's properties.
		/// </summary>
		/// <param name="source">An instance of TSource.</param>
		TSource Insert(TSource source);

		/// <summary>
		/// Performs an insert into the target entity using the supplied <see cref="IQueryExpression{TSource}"/>. 
		/// </summary>
		/// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to insert the objects.</param>
		void Insert(IQueryExpression<TSource> query);
		
		/// <summary>
		/// Updates the target storage with all instances contained within source.
		/// </summary>
		/// <param name="source">An instance of TSource.</param>
		TSource Update(TSource source);

		/// <summary>
		/// Performs an update of the target entity using the supplied <see cref="IQueryExpression{TSource}"/>.
		/// </summary>
		/// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to insert the object.</param>
		void Update(IQueryExpression<TSource> query);

		/// <summary>
		/// Deletes the source from storage.
		/// </summary>
		/// <param name="source">An instance of TSource.</param>
		TSource Delete(TSource source);

		/// <summary>
		/// Performs a delete on the target entity using the supplied <see cref="IQueryExpression{TSource}"/>.
		/// </summary>
		/// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to delete the object.</param>
		void Delete(IQueryExpression<TSource> query);
    }
    
    /// <summary>
    /// Represents the required members for a business logic class.
	/// </summary>
    public interface IBusinessLogic : IDataContext, ITransactional
	{
        /// <summary>
        /// Gets the <see cref="ISecurityPrincipal"/> for the current operation.
        /// </summary>
        ISecurityPrincipal CurrentPrincipal { get; }

        /// <summary>
        /// Gets or sets the default type of the business logic.
        /// </summary>
        Type DefaultBusinessLogicType { get; set; }

        /// <summary>
        /// Returns whether the current <see cref="ISecurityPrincipal"/> is valid for the operation.
        /// </summary>
        /// <param name="principal">The current principal.</param>
        bool ValidatePrincipal(ISecurityPrincipal principal);

        /// <summary>
        /// Gets or sets the IStorageProvider instance for the context.
        /// </summary>
		IStorageProvider StorageProvider { get; set; }
	}
}
	