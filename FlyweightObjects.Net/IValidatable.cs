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
    /// Represents an interface used to validate the state of an object prior to performing an insert or update
    /// operation.
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        /// Returns a boolean indicating whether the current state of the object is valid. If an objeect implements
        /// this interface, the <see cref="DataContext"/> will validate the object prior to performing an insert
        /// or update operation. Returning false will cause a <see cref="ValidationException"/> to be thrown be the
        /// <see cref="DataContext"/>.
        /// </summary>
		/// <param name="context">The <see cref="IDataContext"/> used to query ancillary objects if necessary.</param>
        /// <param name="transactionType">The transaction type that will occur post successful validation.</param>
        /// <param name="message">The message to be displayed.</param>
        bool TryValidate(IDataContext context, TransactionType transactionType, out string message);
    }
}
