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
	/// Represents a request operation to be processed by the server.
	/// </summary>
	[Serializable]
	[DebuggerDisplay("{DisplayName}")]
	public class DataTransferRequest : IResponseMessage
	{
		private string _requestId = Guid.NewGuid().ToString();
		private RuntimeMethod _method = null;
		private object _returnValue = null;
		private ResponseMessage _responseMessage = new ResponseMessage();

		/// <summary>
		/// Gets the unique identifier for the request.
		/// </summary>
		public string RequestId
		{
			get { return _requestId; }
			set { _requestId = value; }
		}

		/// <summary>
		/// Gets the method meta data associated with the request.
		/// </summary>
		public RuntimeMethod Method
		{
			get { return _method; }
			internal set { _method = value; }
		}

		/// <summary>
		/// Gets the return value for the request.
		/// </summary>
		public object ReturnValue
		{
			get { return _returnValue; }
			internal set { _returnValue = value; }
		}

		/// <summary>
		/// Gets a boolean value indicating whether the request has been processed.
		/// </summary>
		public bool IsProcessed
		{
			get { return _responseMessage.ExecutionTime.TotalMilliseconds > 0; }
		}

		/// <summary>
		/// Gets or sets the <see cref="ResponseMessage"/> associated with the request.
		/// </summary>
        public ResponseMessage Response
		{
			get { return _responseMessage; }
			set { _responseMessage = value; }
		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		DataTransferRequest() { }

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="method">The method meta data for the request.</param>
		public DataTransferRequest(RuntimeMethod method)
		{
			_method = method;
		}

		/// <summary>
		/// Gets the name to display in the debugger.
		/// </summary>
		object DisplayName
		{
			get
			{
				if (this.Method != null)
				{
					return this.Method.MethodBase.ToString();
				}
				return this.GetType();
			}
		}
	}
}
