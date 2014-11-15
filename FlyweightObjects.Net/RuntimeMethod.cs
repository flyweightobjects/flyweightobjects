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
using System.Collections;
using System.Reflection;
using System.Diagnostics;

namespace FlyweightObjects
{
	/// <summary>
	/// Contains information about the calling method.
	/// </summary>
	[Serializable]
	[DebuggerDisplay("{MethodBase}")]
	public sealed class RuntimeMethod
	{
		private List<Type> _typeParameters = new List<Type>();
		private ArrayList _methodArguments = new ArrayList();
		private MethodBase _methodInfo = null;
		
		/// <summary>
		/// Gets the captured method.
		/// </summary>
		public MethodBase MethodBase
		{
			get { return _methodInfo; }
		}

		/// <summary>
		/// Gets the type parameters associated with a generic method.
		/// </summary>
		public List<Type> TypeParameters
		{
			get { return _typeParameters; }
		}
		
		/// <summary>
		/// Gets or internally sets the arguments associated with a captured method.
		/// </summary>
		public ArrayList MethodArguments
		{
			get { return _methodArguments; }
			internal set { _methodArguments = value; }
		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="method">The captured method.</param>
		/// <param name="typeParam">The parameter type of a generic method.</param>
		public RuntimeMethod(MethodBase method, Type typeParam)
		{
			_methodInfo = method;
			_typeParameters.Add(typeParam);
		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="method">The captured method.</param>
		/// <param name="methodArgs">The captured arguments.</param>
		public RuntimeMethod(MethodBase method, params object[] methodArgs)
		{
			_methodInfo = method;
			_methodArguments.AddRange(methodArgs);
		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="method">The captured method.</param>
		/// <param name="typeParam">The parameter type of a generic method.</param>
		/// <param name="methodArgs">The captured arguments.</param>
		public RuntimeMethod(MethodBase method, Type typeParam, params object[] methodArgs)
		{
			_methodInfo = method;
			_typeParameters.Add(typeParam);
			_methodArguments.AddRange(methodArgs);
		}
	}
}
