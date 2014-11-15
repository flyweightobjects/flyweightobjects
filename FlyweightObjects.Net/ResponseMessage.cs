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
	/// Represents the facility that fulfilled the request.
	/// </summary>
	public enum StorageFacility
	{
		/// <summary>
		/// The request was fulfilled by a <see cref="IStorageProvider"/>.
		/// </summary>
		StorageProvider,
		/// <summary>
		/// The request was fulfilled by a <see cref="ICacheManager"/>.
		/// </summary>
		CacheManager,
	}

	/// <summary>
	/// Represents a statuc message for a given operation or domain model.
	/// </summary>
	[Serializable]
	[DebuggerDisplay("{DisplayText}")]
	public class ResponseMessage
	{
		/// <summary>
		/// Gets or sets the server name associated with processing a request.
		/// </summary>
		public string ServerName { get; set; }

		/// <summary>
		/// Gets the <see cref="TimeSpan"/> representing the execution time of the last operation.
		/// </summary>
		public TimeSpan ExecutionTime { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="StatusCode"/> of the message.
		/// </summary>
		public StatusCode StatusCode { get; set; }

		/// <summary>
		/// Gets or sets the message text.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Gets or sets an <see cref="Exception"/> object if an error has been thrown.
		/// </summary>
		public Exception Exception { get; set; }

		/// <summary>
		/// Represents the facility that fulfilled the request.
		/// </summary>
		public StorageFacility StorageFacility { get; protected internal set; }

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		public ResponseMessage()
		{
			this.ServerName = string.Empty;
			this.ExecutionTime = new TimeSpan();
			this.StatusCode = StatusCode.Unassigned;
			this.Message = string.Empty;
			this.StorageFacility = StorageFacility.StorageProvider;
		}

		private string DisplayText
		{
			get
			{
				string val = string.Format("{0}: {1}", this.StatusCode.ToString(), this.ExecutionTime);
				if (this.StatusCode == StatusCode.Failure)
				{
					val = string.Format("{0}: {1}", this.StatusCode.ToString(), this.Message);
				}
				return val;
			}
		}
	}
}
