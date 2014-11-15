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
	/// Manages concurrency by comparing the original checksum of the object against that which is
	/// currenty held in storage. If the cheksums are different, a ConcurrencyException is thrown.
	/// Note that in order to for this manager to compare check sum values, the object must implement
	/// IRedundancyCheck.
	/// </summary>
    /// <example>
    /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
    /// <code>
    /// <![CDATA[
    /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
    /// {
    ///     // Create a reusable QueryExpression, selecting a product by ProductID
    ///     var qe = new QueryExpression<Product>().Select().From<Product>().Where(Product.Properties.ProductID == 355);
    ///     
    ///     // Retrieve the product into two separate instances
    ///     Product p1 = context.Select<Product>(qe).ToSingle();
    ///     Product p2 = context.Select<Product>(qe).ToSingle();
    ///     
    ///     // Set the ConcurrencyManager mode so that it will fail on concurrency issues
    ///     context.ConcurrencyManager.Mode = ConcurrencyViolationMode.FailOnConflict;
    ///
    ///     try
    ///     {
    ///         // Set the ModifiedDate of the first product and update it
    ///         p1.ModifiedDate = DateTime.Now;
    ///         context.Update<Product>(p1);
    ///         
    ///         // Set the ModifiedDate of the second product and attempt to update it
    ///         p2.ModifiedDate = DateTime.Now;
    ///         context.Update<Product>(p2);
    ///     }
    ///     catch (ConcurrencyException e)
    ///     {
    ///         // Display the exception and throw
    ///         Console.WriteLine("Exception: {0}", e.ToString());
    ///         throw;
    ///     }
    /// }     
    /// ]]>
    /// </code>
    /// </example>
	public class ConcurrencyManager : IConcurrencyManager
	{
		private ChecksumBuilder _checkSumBuilder = new ChecksumBuilder();
		private ConcurrencyViolationMode _concurrencyViolationMode;
		private IDataContext _context;
		
		/// <summary>
		/// Gets or sets the ConcurrencyViolationMode for the ConcurrencyManager.
		/// </summary>
		public ConcurrencyViolationMode Mode 
		{
			get { return _concurrencyViolationMode; }
			set { _concurrencyViolationMode = value; }
		}

		/// <summary>
		/// Gets the IDataContext from which the ConcurrencyManager was constructed.
		/// </summary>
		public IDataContext Context
		{
			get { return _context; }
		}
		
		/// <summary>
		/// COnstructs a new instance of the class.
		/// </summary>
		/// <param name="context">The IDataContext instance used to retrieve data from storage.</param>
		public ConcurrencyManager(IDataContext context)
		{
			_context = context;
			_concurrencyViolationMode = ConcurrencyViolationMode.IgnoreConflict;
		}

		/// <summary>
		/// Determines if the current object is current by comparing the check sum values of it and the one
		/// currently held in storage.
		/// </summary>
		/// <typeparam name="T">The type of the domain object.</typeparam>
		/// <param name="source">The instance of the domain object.</param>
		public bool IsCurrent<T>(T source) where T : class, IFlyweight, new()
		{
			if (this.Mode == ConcurrencyViolationMode.IgnoreConflict)
			{
				return true;
			}
			IRedundancyCheck obj = source as IRedundancyCheck;
			if (obj != null && !string.IsNullOrEmpty(obj.Checksum))
			{
				T val = this.Context.Reload<T>(source);
				if (val != null) 
				{
					string checkSum = _checkSumBuilder.BuildChecksum(val);
					if (checkSum != obj.Checksum)
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
