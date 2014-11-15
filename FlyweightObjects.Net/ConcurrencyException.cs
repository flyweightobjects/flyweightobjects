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
	/// Represents an exception whenever a concurrency conflict has occurred.
	/// </summary>
	[Serializable]
	public class ConcurrencyException : Exception
	{
		private object _sourceObject = null;
		
		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		public ConcurrencyException()
			: base(ErrorStrings.ConcurrencyViolationException)
		{

		}

		/// <summary>
		/// Gets the object that is no longer current.
		/// </summary>
		public object SourceObject 
		{
			get { return _sourceObject; }
		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		public ConcurrencyException(object source)
			: base(ErrorStrings.ConcurrencyViolationException)
		{
			_sourceObject = source;
		}

		/// <summary>
		/// Constructs a new instance of the class specifically for deserialization purposes.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> instance for the deserialization.</param>
		/// <param name="context">The <see cref="StreamingContext"/> for the deserialization.</param>
		protected ConcurrencyException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}
	}
}
