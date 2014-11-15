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
using System.Text;

namespace FlyweightObjects
{
	/// <summary>
	/// Attributes a compositional property within a domain type in order to describe a relationship. 
	/// </summary>
	[Serializable]
	[AttributeUsage(AttributeTargets.Property, AllowMultiple=true, Inherited=true)]
	public class DataRelationAttribute : PropertyStorageAttribute
	{
		private bool _allowPreload = true;

		/// <summary>
		/// Gets or sets a value indicating whether the related object or objects will be returned when the parent is retrieved and PreloadDepth is greater than zero. 
		/// Setting this value to false means that the relation can only be retrieved using load on demand.
		/// </summary>
		public bool AllowPreload
		{
			get { return _allowPreload; }
			set { _allowPreload = value; }
		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		public DataRelationAttribute() { }

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="allowPreload">A value indicating that the object can be retrieved automatically when its parent is retrieved.</param>
		public DataRelationAttribute(bool allowPreload)
		{
			_allowPreload = allowPreload;
		}
		
	}
}