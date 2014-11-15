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
	/// Represents an abstract command to be processed by a <see cref="IStorageProvider"/>.
	/// </summary>
	public interface IStorageCommand
	{
		/// <summary>
		/// Gets or sets the name of the command.
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Gets the <see cref="ParameterCollection"/> of the command.
		/// </summary>
        ParameterCollection Parameters { get; }

		/// <summary>
		/// Gets or set the <see cref="CommandType"/> of the command.
		/// </summary>
		CommandType CommandType { get; set; }

		/// <summary>
		/// Gets the <see cref="TransactionType"/> of the command.
		/// </summary>
		TransactionType TransactionType { get; }

        /// <summary>
		/// Gets or sets the command text.
		/// </summary>
		string SqlText { get; set; }
	}
}
