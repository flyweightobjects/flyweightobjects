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
using System.Data;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents a stored procedure in storage.
	/// </summary>
    /// <example>
    /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database. In order to run the example, you must create the 
    /// following stored procedure.
    /// <code>
    /// SET ANSI_NULLS ON
    /// GO
    /// SET QUOTED_IDENTIFIER ON
    /// GO
    /// CREATE PROCEDURE GetProductByProductID 
    ///     @p_ProductID int
    /// AS
    /// BEGIN
    ///     SET NOCOUNT ON;
    ///     SELECT      Production.Product.* 
    ///     FROM        Production.Product 
    ///     WHERE       Production.Product.ProductID = @p_ProductID;
    /// END
    /// GO
    ///</code>
    /// <code>
    /// <![CDATA[
    /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
    /// {
    ///     // Create a new StoredProcedure instance, specifying the target's name
    ///     StoredProcedure sp = new StoredProcedure("dbo.GetProductByProductID");
    ///     
    ///     // Add the required parameters
    ///     sp.Parameters.Add(new Parameter("@p_ProductID", 1, DbType.Int32));
    ///     
    ///     // Retrieve a product object and print its name
    ///     Product product = context.Select<Product>(sp).ToSingle();
    ///     Console.WriteLine("Product Name: {0}", product.Name);
    /// }     
    /// ]]>
    /// </code>
    /// </example>
	[Serializable]
	public class StoredProcedure : StorageCommand
	{
		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		public StoredProcedure(string procedureName)
			: base(procedureName, CommandType.StoredProcedure)
		{
			
		}

		/// <summary>
		/// Gets the stored procedure name.
		/// </summary>
		public string ProcedureName
		{
			get { return base._sqlText; }
		}
	}
}
