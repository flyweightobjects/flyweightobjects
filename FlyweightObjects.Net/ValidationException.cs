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
using System.Runtime.Serialization;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents a validation exception.
	/// </summary>
	[Serializable]
	public class ValidationException : Exception
	{
		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		public ValidationException()
			: base(ErrorStrings.ValidationFailedException)
		{

		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="message">The error message for the exception.</param>
		public ValidationException(string message)
			: base(message)
		{

		}

		/// <summary>
		/// Constructs a new instance of the class specifically for deserialization purposes.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> instance for the deserialization.</param>
		/// <param name="context">The <see cref="StreamingContext"/> for the deserialization.</param>
		protected ValidationException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}
	}
}
