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
using System.Diagnostics;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents the status code of a <see cref="IResponseMessage"/>.
	/// </summary>
	public enum StatusCode
	{
		/// <summary>
		/// No status has been assigned.
		/// </summary>
		Unassigned,
		/// <summary>
		/// The operation was successful.
		/// </summary>
		Success,
		/// <summary>
		/// The operation failed.
		/// </summary>
		Failure,
        /// <summary>
        /// A warning regarding the current operation.
        /// </summary>
        Warning
	}
	
	/// <summary>
	/// Represents a message regarding th result of an operation.
	/// </summary>
	public interface IResponseMessage
	{
		/// <summary>
		/// Gets or sets a <see cref="ResponseMessage"/>.
		/// </summary>
		ResponseMessage Response { get; set; }
	}

}
