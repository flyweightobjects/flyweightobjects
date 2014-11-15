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
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Linq.Expressions;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents the non-generic properties internally exposed by a <see cref="DataRelationContainer{T}"/>.
    /// </summary>
    internal interface IDataRelationContainer
    {
        bool IsLoaded { get; set; }
        IEnumerable InnerSource { get; }
    }
    
    /// <summary>
    /// Represents a surrogate object responsible for retrieving related objects on a domain model using load on demand.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve.</typeparam>
    /// <example>
    /// The following code examples demonstrate a manually created MyProduct class from the Microsoft AdventureWorks SQL Server sample database. 
    /// The first shows the definition of the MyProduct class, including a DataRelationContainer&lt;T&gt; instance which abstracts an IFlyweightSet&lt;BillOfMaterial&gt;.
    /// The second shows how to use deferred loading of each retrieved MyProduct in order to display the count of BillOfMaterial related objects. 
    /// <code>
    /// <![CDATA[
    /// [Serializable]
    /// [EditorBrowsable(EditorBrowsableState.Always)]
	/// [DataTable("Production.Product", MemberBindingType = MemberBindingType.Fields, EnableCaching = false, CacheTimeout = 0)]
    /// [DebuggerDisplay("{ToString()}")]
    /// public partial class MyProduct
    /// {
    ///     FlyweightStorage Storage = new FlyweightStorage(typeof(MyProduct));
    ///
    ///     FlyweightStorage IFlyweight.Storage
    ///     {
    ///         get { return this.Storage; }
    ///         set { this.Storage = value; }
    ///     }    
    ///     
    ///     [DataRelation(AllowPreload = true)]
    ///     public FlyweightSet<BillOfMaterial> BillOfMaterials
    ///     {
    ///         get { return this.Storage.GetDataRelation<BillOfMaterial>().GetProperty(BillOfMaterial.Properties.ProductAssemblyID == this.ProductID).ToFlyweightSet(); }
    ///         set { this.Storage.GetDataRelation<BillOfMaterial>().SetProperty(value); }
    ///     }
    ///
    ///     [DataColumn("ProductID", FieldType = DbType.Int32, AllowDBNull = false, Identifier = true, AutoIncrement = true)]
    ///     public virtual int ProductID
    ///     {
    ///         get { return this.Storage.GetProperty<int>(Properties.ProductID.ToString()); }
    ///         set { this.Storage.SetProperty<int>(Properties.ProductID.ToString()); }
    ///     }
    ///
    ///     [DataColumn("Name", FieldType = DbType.AnsiString, AllowDBNull = false)]
    ///     public virtual string Name
    ///     {
    ///         get { return this.Storage.GetProperty<string>(Properties.Name.ToString()); }
    ///         set { this.Storage.SetProperty<string>(Properties.Name.ToString()); }
    ///     }
    ///
    ///     public static class Properties
    ///     {
    ///         public static PropertyExpression<Product> ProductID { get { return new PropertyExpression<Product>("ProductID"); } }
    ///         public static PropertyExpression<Product> Name { get { return new PropertyExpression<Product>("Name"); } }
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// <code>
    /// <![CDATA[
    /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
    /// {
    ///     var qe = new QueryExpression<MyProduct>()
    ///
    ///     .Select(true, 0)
    ///     .From<MyProduct>()
    ///     .LeftJoin<BillOfMaterial>(MyProduct.Properties.ProductID == BillOfMaterial.Properties.ProductAssemblyID)
    ///     .Where(BillOfMaterial.Properties.PerAssemblyQty > 0)
    ///     .OrderBy(MyProduct.Properties.ProductID);
    ///
    ///     context.LoadOnDemand = true;
    ///     foreach (MyProduct product in context.Select<MyProduct>(qe))
    ///     {
    ///         Console.WriteLine("Product Id {0} BOM Count: {1}", product.ProductID, product.BillOfMaterials.Count);
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    [Serializable]
    public class DataRelationContainer<T> : IDataRelationContainer, ILoadOnDemand where T : class, IFlyweight, new()
    {
        private FlyweightSet<T> _innerSource = new FlyweightSet<T>();
        private bool _isLoaded = false;
        private bool _loadOnDemand = true;
        
        /// <summary>
        /// Gets or sets a boolean value indicating whether the contained object has been loaded.
        /// </summary>
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { _isLoaded = value; }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating whether the object should be dynamically loaded when the property is accessed.
        /// </summary>
        public bool LoadOnDemand
        {
            get { return _loadOnDemand; }
            set { _loadOnDemand = value; }
        }

        IEnumerable IDataRelationContainer.InnerSource
        {
            get { return _innerSource; }
        }

        /// <summary>
        /// Retrieves the related objects.
        /// </summary>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to retrieve the objects.</param>
        public IEnumerable<T> GetProperty(IQueryExpression<T> query)
        {
            if (IsLoadable())
            {
                IDataContext context = this.GetDataContext();
                if (context != null)
                {
					if (context is IBusinessLogic)
					{
						_innerSource = BusinessLogicFactory.GetBusinessLogic((IBusinessLogic)context).Select<T>(query) as FlyweightSet<T>;
					}
					else
					{
						_innerSource = context.Select<T>(query) as FlyweightSet<T>;
					}
                    _isLoaded = true;
                }
            }
            return _innerSource;
        }

        /// <summary>
        /// Retrieves the related objects.
        /// </summary>
        /// <param name="command">The <see cref="IStorageCommand"/> used to perform the query.</param>
        public IEnumerable<T> GetProperty(IStorageCommand command)
        {
            if (IsLoadable())
            {
                IDataContext context = this.GetDataContext();
                if (context != null)
                {
					if (context is IBusinessLogic)
					{
						_innerSource = BusinessLogicFactory.GetBusinessLogic((IBusinessLogic)context).Select<T>(command) as FlyweightSet<T>;
					}
					else
					{
						_innerSource = context.Select<T>(command) as FlyweightSet<T>;
					}
                    _isLoaded = true;
                }
            }
            return _innerSource;
        }

        /// <summary>
        /// Retrieves the related objects.
        /// </summary>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to filter the objects to be retrieved.</param>
        public IEnumerable<T> GetProperty(PropertyExpression<T> whereExpression)
        {
            return this.GetProperty(new QueryExpression<T>().Select().From<T>().Where(whereExpression));
        }

        /// <summary>
        /// Retrieves the related objects.
        /// </summary>
        /// <param name="whereExpression">An <see cref="Expression"/> used to filter the objects to be retrieved.</param>
        public IEnumerable<T> GetProperty(Expression<Func<T, bool>> whereExpression)
        {
            return this.GetProperty(new PropertyExpressionConverter<T>().Convert(whereExpression));
        }

        /// <summary>
        /// Sets the related objects.
        /// </summary>
        /// <param name="source">An instance of T.</param>
        public void SetProperty(T source)
        {
            _innerSource = new FlyweightSet<T>();
            _innerSource.Add(source);
            _isLoaded = true;
        }

        /// <summary>
        /// Sets the related objects.
        /// </summary>
        /// <param name="source">An instance of T.</param>
        public void SetProperty(IEnumerable<T> source)
        {
            _innerSource = source.ToFlyweightSet();
            _isLoaded = true;
        }

        private bool IsLoadable()
        {
            return  _loadOnDemand && !_isLoaded && _innerSource.Count<T>() == 0 &&  ((IChangeTrackable<T>)_innerSource).DeletedItems.Count == 0;
        }

        private IDataContext GetDataContext()
        {
            string key = ThreadLocalStorage.BuildContextKey(typeof(T));
            if (ThreadLocalStorage.IsContextRegistered(key))
            {
                return ThreadLocalStorage.GetRegisteredContext(key);
            }
            return null;
        }

    }
}
