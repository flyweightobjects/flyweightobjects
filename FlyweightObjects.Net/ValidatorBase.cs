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
	/// Represents a base class responsible to validating a domain object.
	/// </summary>
	/// <typeparam name="T">The type of object to validate.</typeparam>
	public abstract class ValidatorBase<T> : IValidator<T> where T : class, IFlyweight, new()
	{
		private IDataContext _context = null;
		private string _message = string.Empty;

		/// <summary>
		/// Gets an instance of an <see cref="IDataContext"/>.
		/// </summary>
		public IDataContext Context
		{
			get { return _context; }
		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		public ValidatorBase() { }

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="context">An instance of an <see cref="IDataContext"/> used to perform additional queries.</param>
		public ValidatorBase(IDataContext context)
		{
			_context = context;
		}

		/// <summary>
		/// Gets or sets the message associated with the validation operation.
		/// </summary>
		public string Message
		{
			get { return _message; }
			set { _message = value; }
		}

		/// <summary>
		/// Returns whether the domain object can be inserted.
		/// </summary>
		/// <param name="source">The object to validate.</param>
		public virtual bool CanInsert(T source)
		{
			return true;
		}

		/// <summary>
		/// Returns whether the domain object can be updated.
		/// </summary>
		/// <param name="source">The object to validate.</param>
		public virtual bool CanUpdate(T source)
		{
			return true;
		}

		/// <summary>
		/// Returns whether the domain object can be delted.
		/// </summary>
		/// <param name="source">The object to validate.</param>
		public virtual bool CanDelete(T source)
		{
			return true;
		}

		/// <summary>
		/// Returns whether the list of domain objects can be inserted.
		/// </summary>
		/// <param name="source">A list of domain objects to be validated.</param>
		public virtual bool CanInsert(IEnumerable<T> source)
		{
			bool valid = true;
			foreach (T item in source)
			{
				valid &= this.CanInsert(item);
			}
			return valid;
		}

		/// <summary>
		/// Returns whether the list of domain objects can be updated.
		/// </summary>
		/// <param name="source">A list of domain objects to be validated.</param>
		public virtual bool CanUpdate(IEnumerable<T> source)
		{
			bool valid = true;
			foreach (T item in source)
			{
				valid &= this.CanUpdate(item);
			}
			return valid;
		}

		/// <summary>
		/// Returns whether the list of domain objects can be deleted.
		/// </summary>
		/// <param name="source">A list of domain objects to be validated.</param>
		public virtual bool CanDelete(IEnumerable<T> source)
		{
			bool valid = true;
			foreach (T item in source)
			{
				valid &= this.CanDelete(item);
			}
			return valid;
		}
	}
}
