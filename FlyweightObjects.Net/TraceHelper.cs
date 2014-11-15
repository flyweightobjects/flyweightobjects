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
using System.Reflection;
using System.Diagnostics;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents a helper class to assist in tracing.
	/// </summary>
	public sealed class TraceHelper
	{
		private static string _machineName = Environment.MachineName;
        private static bool _enabled = true;

        /// <summary>
        /// Gets or sets a boolean value to enable or disable the <see cref="TraceHelper"/>.
        /// </summary>
        public static bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }
		
		/// <summary>
		/// Writes a message to the trace listeners in the Listeners collection.
		/// </summary>
		/// <param name="method">The method to be traced.</param>
		public static void WriteLine(MethodBase method) 
		{
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
            if (_enabled)
            {
                Trace.WriteLine(string.Format("[Machine: {0}] [{1}.{2}]", _machineName, method.DeclaringType.FullName, ParseMethod(method)));
            }
		}
		
		/// <summary>
		/// Writes a message to the trace listeners in the Listeners collection.
		/// </summary>
		/// <param name="method">The method to be traced.</param>
		/// <param name="message">The message to write.</param>
		public static void WriteLine(MethodBase method, string message)
		{
			if (method == null)
			{
				throw new ArgumentNullException("method");
			}
			if (message == null)
			{
				throw new ArgumentNullException("message");
			}
            if (_enabled)
            {
                Trace.WriteLine(string.Format("[Machine: {0}] [{1}.{2}] {3}", _machineName, method.DeclaringType.FullName, ParseMethod(method), message));
            }
		}

		private static string ParseMethod(MethodBase method)
		{
			string retVal = string.Empty;
			string[] strings = method.ToString().Split(" ".ToCharArray());
			for (int i = 1; i < strings.Length; i++)
			{
				retVal += strings[i];
			}
			return retVal == string.Empty ? method.Name : retVal;
		}
	}
}
