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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Data;

namespace FlyweightObjects
{
    /// <summary>
    /// Provides a surrogate object to hold unbound query results. This class may be used in lieu of a strongly typed
    /// domain class by exposing a serializable dictionary of name value pairs representing columns returned in 
    /// a select statement.
    /// </summary>
    /// <example>
    /// The following example uses a Product and ProductCategory class from the Microsoft AdventureWorks SQL Server sample database.
    /// <code>
    /// <![CDATA[
    /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
    /// {
    ///     var qe = new QueryExpression<QueryResult>()
    ///
    ///     .Select(Product.Properties.ProductID, ProductInventory.Properties.Quantity.Max().As("MaxInventoryQty"))
    ///     .From<Product>()
    ///     .Join<ProductInventory>(Product.Properties.ProductID == ProductInventory.Properties.ProductID)
    ///     .Where(Product.Properties.DiscontinuedDate.IsNull())
    ///     .GroupBy(Product.Properties.ProductID)
    ///     .Having(ProductInventory.Properties.LocationID.Count() > 0)
    ///     .OrderBy(ProductInventory.Properties.Quantity.Max().Desc());
    ///
    ///     var qr = context.Select<QueryResult>(qe);
    ///     Console.WriteLine("Maximum inventory quantity is {0}", qr[0]["MaxInventoryQty"]);
    /// }
    /// ]]>
    /// </code>
    /// </example>
    [Serializable]
    [DataTable(AllowSelect = true, AllowInsert = false, AllowUpdate = false, AllowDelete = false)]
    public class QueryResult : IFlyweight
    {
        /// <summary>
        /// Gets the <see cref="PropertyStorage"/> which manages manages state of member fields.
        /// </summary>
        protected PropertyStorage Storage { get; private set; }

        /// <summary>
        /// Represents the <see cref="PropertyStorage"/> which manages manages state of member fields.
        /// </summary>
        public QueryResult()
        {
            this.Storage = new PropertyStorage(this.GetType());
        }

        /// <summary>
        /// /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="storage"></param>
        public QueryResult(PropertyStorage storage)
        {
            if (storage == null)
            {
                throw new ArgumentNullException("storage");
            }
            this.Storage = storage;
        }

        /// <summary>
        /// Gets or sets the <see cref="PropertyStorage"/>  which manages state for member fields.
        /// </summary>
        PropertyStorage IFlyweight.Storage
        {
            get { return Storage; }
            set { Storage = value; }
        }

        /// <summary>
        /// Gets or sets the value at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        public object this[int index]
        {
            get { return this.Storage[index]; }
            set { this.Storage[index] = value; }
        }

        /// <summary>
        /// Gets or sets the value using the specified field name.
        /// </summary>
        /// <param name="index">The name of the field.</param>
        /// <returns></returns>
        public object this[string index]
        {
            get {  return this.Storage[index]; } 
            set {  this.Storage[index] = value; }
        }
    }
}