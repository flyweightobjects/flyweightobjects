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
	/// Represents the interface for a literal value to be specified in a SELECT statement.
	/// </summary>
	internal interface ILiteral
	{
		/// <summary>
		/// The constant value of the selection.
		/// </summary>
		object Constant { get; }
	}

	/// <summary>
	/// Represents a class for a literal value to be specified in a SELECT statement.
	/// </summary>
	/// <typeparam name="T">The primitive type for the constant value which should be selected.</typeparam>
	[Serializable]
	public class Literal<T> : PropertyExpression, ILiteral
	{
		/// <summary>
		/// The constant value of the selection.
		/// </summary>
		public T Constant { get; set; }

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="value">The primitive type for the constant value which should be selected.</param>
		public Literal(T value)
		{
			this.Constant = value;
			base.PropertyAlias = string.Format("Column{0}", this.GetHashCode());
		}

		/// <summary>
		/// Gets a boolean value indicating whether or not the <see cref="Literal{t}"/> has been initialized.
		/// </summary>
		public override bool IsEmpty
		{
			get { return false; }
		}

		/// <summary>
		/// 
		/// </summary>
		object ILiteral.Constant
		{
			get { return this.Constant; }
		}

		/// <summary>
		/// Specifies the literal value which should be selected.
		/// </summary>
		/// <param name="value">The constant value which should be selected.</param>
		public static Literal<T> Value(T value)
		{
			Literal<T> retVal = new Literal<T>(value);
			return retVal;
		}
	}
}
