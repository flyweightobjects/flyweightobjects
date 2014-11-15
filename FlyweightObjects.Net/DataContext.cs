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
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Linq.Expressions;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents the class by which all storage operations are performed.
    /// </summary>
    /// <exception cref="StorageProviderException"/>
    public class DataContext : IDataContext, ILoadOnDemand, ITransactional, IDisposable
    {
		/// <summary>
		///Returns true to specify distinct values.
		/// </summary>
		protected const bool DISTINCT = true;
		/// <summary>
		/// Returns the integer specified as input.
		/// </summary>
		protected static Func<int, int> TOP = (value) => value;
		
		/// <summary>
        /// Called prior to performing a select operation.
        /// </summary>
        public event EventHandler<DataOperationEventArgs> Selecting;
        /// <summary>
        /// Called prior to performing an insert operation.
        /// </summary>
        public event EventHandler<DataOperationEventArgs> Inserting;
        /// <summary>
        /// Called prior to performing an update operation.
        /// </summary>
        public event EventHandler<DataOperationEventArgs> Updating;
        /// <summary>
        /// Called prior to performing a delete operation.
        /// </summary>
        public event EventHandler<DataOperationEventArgs> Deleting;
        /// <summary>
        /// Called prior to performing a truncate operation.
        /// </summary>
        public event EventHandler<DataOperationEventArgs> Truncating;
        /// <summary>
        /// Called prior to executing an <see cref="IStorageCommand"/>.
        /// </summary>
        public event EventHandler<DataOperationEventArgs> Executing;

        private bool _autoCloneObjects = false;
        private TimeSpan _executionTime = new TimeSpan();
        private IStorageProvider _storageProvider = null;
        private Stopwatch _timer = new Stopwatch();
        private bool _isPreloading = false;
        private IConcurrencyManager _concurrencyManager = null;
        private ICacheManager _cacheManager = null;
        private bool _isTransactionOwner = false;
        private string _domainName = string.Empty;
        private volatile Lazy<string> _machineName;
        private MethodBase _transactionMethodOwner = null;
        private bool _updateUnchangedObjects = false;

        /// <summary>
        /// Gets the string used to connect to the current <see cref="StorageProvider"/>.
        /// </summary>
        public string ConnectionString
        {
            get { return _storageProvider.ConnectionString; }
        }

        /// <summary>
        /// Gets or sets whether objects retrieved from the <see cref="StorageProvider"/> should be cloned before returning to the caller.
        /// </summary>
        public bool AutoCloneObjects
        {
            get { return _autoCloneObjects; }
            set { _autoCloneObjects = value; }
        }

        /// <summary>
        /// Gets the <see cref="TimeSpan"/> representing the execution time of the last operation.
        /// </summary>
        public TimeSpan ExecutionTime
        {
            get { return _executionTime; }
        }

        /// <summary>
        /// Gets the IDbTransaction for the current context if one exists.
        /// </summary>
        public IDbTransaction ActiveTransaction
        {
            get { return this.StorageProvider.ActiveTransaction; }
        }

        /// <summary>
        /// Gets whether the current context is the initiator of the current transaction.
        /// </summary>
        protected bool IsTransactionOwner
        {
            get { return _isTransactionOwner; }
        }

        /// <summary>
        /// Gets or sets the unique name of the domain which identifies the context with specific object types. Parity must exist between
        /// the value of this property and objects with a <see cref="DataTable"/>.DomainName in order for deferred loading to work correctly
        /// acrosss multiple domains.
        /// </summary>
        public string DomainName
        {
            get { return _domainName; }
            set { _domainName = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether updates should occur for objects which have not been changed.
        /// </summary>
        public bool UpdateUnchangedObjects
        {
            get { return _updateUnchangedObjects; }
            set { _updateUnchangedObjects = value; }
        }

        /// <summary>
        /// Gets or sets the peek size of any returned <see cref="FlyweightSet{T}"/> collections for debugging purposes.
        /// The default value for this property is 1000.
        /// </summary>
        public int MaxDebugListSize
        {
            get { return ThreadLocalStorage.GetMaxDebugListSize(); }
            set { ThreadLocalStorage.SetMaxDebugListSize(value); }
        }

        /// <summary>
        /// Gets the local machine name for the instance of the context.
        /// </summary>
        protected internal string MachineName
        {
            get
            {
                if (!_machineName.IsValueCreated)
                {
                    _machineName = new Lazy<string>(() => Environment.MachineName);
                }
                return _machineName.Value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IStorageProvider"/> for the current context.
        /// </summary>
        public IStorageProvider StorageProvider
        {
            get { return _storageProvider; }
            protected internal set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                _storageProvider = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ICacheManager"/> for the context.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     context.CacheManager.Enabled = true;
        /// }     
        /// ]]>
        /// </code>
        /// </example>
        public ICacheManager CacheManager
        {
            get { return _cacheManager; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                _cacheManager = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IConcurrencyManager"/> for the context.
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
        public IConcurrencyManager ConcurrencyManager
        {
            get { return _concurrencyManager; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                _concurrencyManager = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the deferred loading of objects should be enabled.
        /// </summary>
        public bool LoadOnDemand
        {
            get
            {
                return ThreadLocalStorage.IsContextRegistered(this);
            }
            set
            {
                lock (this)
                {
                    if (value)
                    {
                        ThreadLocalStorage.RegisterContext(this);
                    }
                    else
                    {
                        ThreadLocalStorage.UnRegisterContext(this);
                    }
                }
            }
        }

        /// <summary>
        /// Starts a new transaction for the current context.
        /// </summary>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Begin a new transaction
        ///     context.BeginTransaction();
        ///     try
        ///     {
        ///         // Retrieve a collection of products
        ///         var products = context.Select<Product>(100).ToFlyweightSet();
        ///         
        ///         // Set the ModifiedDate of each product
        ///         foreach (Product product in products)
        ///         {
        ///             product.ModifiedDate = DateTime.Now;
        ///         }
        ///         
        ///         // Update the target entity and commit changes
        ///         context.Update<Product>(products);
        ///         context.CommitTransaction();
        ///     }
        ///     catch (Exception e)
        ///     {
        ///         // Rollback the changes if one is present
        ///         context.RollbackTransaction();
        ///         Console.WriteLine("Exception: {0}", e.ToString());
        ///         throw;
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IDbTransaction BeginTransaction()
        {
            if (this.StorageProvider.ActiveTransaction == null)
            {
                this.StorageProvider.BeginTransaction();
                _isTransactionOwner = true;
                _transactionMethodOwner = new StackTrace().GetFrame(1).GetMethod();
            }
            return this.StorageProvider.ActiveTransaction;
        }

        /// <summary>
        /// Commits the current transaction if one is present.
        /// </summary>
        /// <remarks>
        /// Note that calling RollbackTransaction() is an innocuous event if BeginTransaction() has not been called. Therefore, it is not necessary to examine the <see cref="ActiveTransaction"/>
        /// property prior to calling this method.
        /// </remarks>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Begin a new transaction
        ///     context.BeginTransaction();
        ///     try
        ///     {
        ///         // Retrieve a collection of products
        ///         var products = context.Select<Product>(100).ToFlyweightSet();
        ///         
        ///         // Set the ModifiedDate of each product
        ///         foreach (Product product in products)
        ///         {
        ///             product.ModifiedDate = DateTime.Now;
        ///         }
        ///         
        ///         // Update the target entity and commit changes
        ///         context.Update<Product>(products);
        ///         context.CommitTransaction();
        ///     }
        ///     catch (Exception e)
        ///     {
        ///         // Rollback the changes if one is present
        ///         context.RollbackTransaction();
        ///         Console.WriteLine("Exception: {0}", e.ToString());
        ///         throw;
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public void CommitTransaction()
        {
            if (this.StorageProvider.ActiveTransaction != null && _isTransactionOwner)
            {
                if (new StackTrace().GetFrame(1).GetMethod().Equals(_transactionMethodOwner))
                {
                    this.StorageProvider.CommitTransaction();
                    _isTransactionOwner = false;
                    _transactionMethodOwner = null;
                }
            }
        }

        /// <summary>
        /// Commits the current transaction to storage. If force is equal to true, the transaction will be commited even though the current instance may not be the owner.
        /// </summary>
        /// <param name="force">A value indicating whether to force the commit regardless of ownership.</param>
        public void CommitTransaction(bool force)
        {
            if (this.StorageProvider.ActiveTransaction != null)
            {
                this.StorageProvider.CommitTransaction(true);
                _isTransactionOwner = false;
                _transactionMethodOwner = null;
            }
        }

        /// <summary>
        /// Rolls back the current transaction if one is present.
        /// </summary>
        /// <remarks>
        /// Note that calling RollbackTransaction() is an innocuous event if BeginTransaction() has not been called. Therefore, it is not necessary to examine the <see cref="ActiveTransaction"/>
        /// property prior to calling this method.
        /// </remarks>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Begin a new transaction
        ///     context.BeginTransaction();
        ///     try
        ///     {
        ///         // Retrieve a collection of products
        ///         var products = context.Select<Product>(100).ToFlyweightSet();
        ///         
        ///         // Set the ModifiedDate of each product
        ///         foreach (Product product in products)
        ///         {
        ///             product.ModifiedDate = DateTime.Now;
        ///         }
        ///         
        ///         // Update the target entity and commit changes
        ///         context.Update<Product>(products);
        ///         context.CommitTransaction();
        ///     }
        ///     catch (Exception e)
        ///     {
        ///         // Rollback the changes if one is present
        ///         context.RollbackTransaction();
        ///         Console.WriteLine("Exception: {0}", e.ToString());
        ///         throw;
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public void RollbackTransaction()
        {
            if (this.StorageProvider.ActiveTransaction != null)
            {
                this.StorageProvider.RollbackTransaction();
                _isTransactionOwner = false;
                _transactionMethodOwner = null;
            }
        }

        /// <summary>
        /// Constructs a new instance of a <see cref="DataContext"/>.
        /// </summary>
        DataContext()
        {
            _concurrencyManager = new ConcurrencyManager(this);
            _cacheManager = new CacheManager();
            _machineName = new Lazy<string>();
        }

        /// <summary>
        /// Constructs a new instance of a <see cref="DataContext"/>.
        /// </summary>
        /// <param name="connectionString">The connection string for the StorageProvider.</param>
        /// <param name="providerType">The StorageProviderType enumerated value.</param>
        public DataContext(string connectionString, StorageProviderType providerType)
            : this()
        {
            _storageProvider = GetStorageProvider(connectionString, providerType);
        }

        /// <summary>
        /// Constructs a new instance of a <see cref="DataContext"/>.
        /// </summary>
        /// <param name="connectionString">The connection string for the StorageProvider.</param>
        /// <param name="providerType">The StorageProviderType enumerated value.</param>
        /// <param name="loadOnDemand">A boolean value indicating whether deferred loading of objects should be enabled.</param>
        public DataContext(string connectionString, StorageProviderType providerType, bool loadOnDemand)
            : this(connectionString, providerType)
        {
            this.LoadOnDemand = loadOnDemand;
        }

        /// <summary>
        /// Constructs a new instance of a <see cref="DataContext"/>.
        /// </summary>
        /// <param name="connectionString">The connection string for the StorageProvider.</param>
        /// <param name="providerType">The StorageProviderType enumerated value.</param>
        /// <param name="loadOnDemand">A boolean value indicating whether deferred loading of objects should be enabled.</param>
        /// <param name="domainName">A unique name that identifies a domain for which this instance of the <see cref="DataContext"/> is responsible.</param>
        public DataContext(string connectionString, StorageProviderType providerType, bool loadOnDemand, string domainName)
            : this(connectionString, providerType)
        {
            this.DomainName = domainName;
            this.LoadOnDemand = loadOnDemand;
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// the retrieved object, otherwise it will be its default.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="source">An instance of T.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     Product product = null;
        ///     if (context.TrySelect<Product>(out product))
        ///     {
        ///         Console.WriteLine("First returned name is {0}", product.Name);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public bool TrySelect<T>(out T source) where T : class, IFlyweight, new()
        {
            source = this.Select<T>(1).ToSingle<T>();
            return source != null;
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// an IEnumerable of the retrieved objects, otherwise if will be an empty instance of IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     IEnumerable<Product> products = null;
        ///     if (context.TrySelect<Product>(out products))
        ///     {
        ///         Console.WriteLine("Count is {0}", products.Count<Product>());
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public bool TrySelect<T>(out IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            source = this.Select<T>(0);
            return source.Any<T>();
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// the retrieved object, otherwise it will be its default.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="source">An instance of T.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     Product product = null;
        ///     if (context.TrySelect<Product>(Product.Properties.ProductID == 355, out product))
        ///     {
        ///         Console.WriteLine("Product name is {0}", product.Name);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public bool TrySelect<T>(PropertyExpression<T> whereExpression, out T source) where T : class, IFlyweight, new()
        {
            source = this.Select<T>(1, whereExpression).ToSingle<T>();
            return source != null;
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// an IEnumerable of the retrieved objects, otherwise if will be an empty instance of IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="whereExpression">An <see cref="Expression"/> that serves as the filter criteria used to query the storage.</param>
        /// <param name="source">An instance of T.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     Product product = null;
        ///     if (context.TrySelect<Product>(p => p.ProductID == 355, out product))
        ///     {
        ///         Console.WriteLine("Product name is {0}", product.Name);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public bool TrySelect<T>(Expression<Func<T, bool>> whereExpression, out T source) where T : class, IFlyweight, new()
        {
            PropertyExpression<T> whereCondition = new PropertyExpressionConverter<T>().Convert(whereExpression);
            return this.TrySelect<T>(whereCondition, out source);
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// an IEnumerable of the retrieved objects, otherwise if will be an empty instance of IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> criteria used to query the storage.</param>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     IEnumerable<Product> products = null;
        ///     if (context.TrySelect<Product>(Product.Properties.ProductLine.Trim().Length() > 0 && Product.Properties.ListPrice > 0, out products))
        ///     {
        ///         Console.WriteLine("Count is {0}", products.Count<Product>());
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public bool TrySelect<T>(PropertyExpression<T> whereExpression, out IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            source = this.Select<T>(0, whereExpression);
            return source.Any<T>();
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// an IEnumerable of the retrieved objects, otherwise if will be an empty instance of IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="whereExpression">An <see cref="Expression"/> that serves as the filter criteria used to query the storage.</param>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     IEnumerable<Product> products = null;
        ///     if (context.TrySelect<Product>(p => p.ProductLine.Trim().Length() > 0 && p.ListPrice > 0, out products))
        ///     {
        ///         Console.WriteLine("Count is {0}", products.Count<Product>());
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public bool TrySelect<T>(Expression<Func<T, bool>> whereExpression, out IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            PropertyExpression<T> whereCondition = new PropertyExpressionConverter<T>().Convert(whereExpression);
            return this.TrySelect<T>(whereCondition, out source);
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// the retrieved object, otherwise it will be its default.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to query the storage.</param>
        /// <param name="source">An instance of T.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     Product product = null;
        ///     if (context.TrySelect<Product>(new QueryExpression<Product>().Select().From<Product>().Where(Product.Properties.ProductID == 355), out product))
        ///     {
        ///         Console.WriteLine("Product name is {0}", product.Name);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public bool TrySelect<T>(IQueryExpression<T> query, out T source) where T : class, IFlyweight, new()
        {
            source = this.Select<T>(query).ToSingle<T>();
            return source != null;
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// an IEnumerable of the retrieved objects, otherwise if will be an empty instance of IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to query the storage.</param>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     IEnumerable<Product> products = null;
        ///     if (context.TrySelect<Product>(new QueryExpression<Product>().Select().From<Product>().Where(Product.Properties.ProductLine.Trim().Length() > 0), out products))
        ///     {
        ///         Console.WriteLine("Count is {0}", products.Count<Product>());
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public bool TrySelect<T>(IQueryExpression<T> query, out IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            source = this.Select<T>(query);
            return source.Any<T>();
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// the retrieved object, otherwise it will be its default.  Note the value for sql may be any object which implements 
        /// the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> used to query the storage.</param>
        /// <param name="source">An instance of T.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     Product product = null;
        ///     if (context.TrySelect<Product>(new StorageCommand("SELECT * FROM Production.Product WHERE ProductID = 355"), out product))
        ///     {
        ///         Console.WriteLine("Product name is {0}", product.Name);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public bool TrySelect<T>(IStorageCommand command, out T source) where T : class, IFlyweight, new()
        {
            source = this.Select<T>(command).ToSingle<T>();
            return source != null;
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be an 
        /// IEnumerable of the retrieved objects, otherwise if will be an empty instance of IEnumerable.  Note the value for 
        /// sql may be any object which implements the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> used to query the storage.</param>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     IEnumerable<Product> products = null;
        ///     if (context.TrySelect<Product>(new StorageCommand("SELECT * FROM Production.Product WHERE ProductLine IS NOT NULL"), out products))
        ///     {
        ///         Console.WriteLine("Count is {0}", products.Count<Product>());
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public bool TrySelect<T>(IStorageCommand command, out IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            source = this.Select<T>(command);
            return source.Any<T>();
        }

        /// <summary>
        /// Returns a boolean value indicating whether the object exists in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="source">An instance of T.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     Product product = context.Select<Product>(Product.Properties.ProductID == 355).ToSingle();
        ///     if (context.Exists<Product>(product))
        ///     {
        ///         Console.WriteLine("Product exists");
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public bool Exists<T>(T source) where T : class, IFlyweight, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (typeof(T).IsEnumerable())
            {
                throw new InvalidOperationException(ErrorStrings.InvalidEnumerableArgumentException);
            }
            Stopwatch timer = StartTimer();
            ValidateType<T>();
            ValidateOperation<T>(TransactionType.Select);
            bool retVal = this.StorageProvider.ProcessExists<T>(source);
            StopTimer(timer);
            return retVal;
        }

        /// <summary>
        /// Returns a boolean value indicating whether the object exists in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     if (context.Exists<Product>(Product.Properties.ProductID == 355))
        ///     {
        ///         Console.WriteLine("Product exists");
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public bool Exists<T>(PropertyExpression<T> whereExpression) where T : class, IFlyweight, new()
        {
            return this.Select<T>(whereExpression).Any<T>();
        }

        /// <summary>
        /// Returns a boolean value indicating whether the object exists in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="whereExpression">An <see cref="Expression"/> that serves as the filter criteria used to query the storage.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     if (context.Exists<Product>(p => p.ProductID == 355))
        ///     {
        ///         Console.WriteLine("Product exists");
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public bool Exists<T>(Expression<Func<T, bool>> whereExpression) where T : class, IFlyweight, new()
        {
            return this.Exists<T>(new PropertyExpressionConverter<T>().Convert(whereExpression));
        }

        /// <summary>
        /// Returns a boolean value indicating whether the object exists in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to query the storage.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     if (context.Exists<Product>(new QueryExpression<Product>().Select().From<Product>().Where(Product.Properties.ProductID == 355)))
        ///     {
        ///         Console.WriteLine("Product exists");
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public bool Exists<T>(IQueryExpression<T> query) where T : class, IFlyweight, new()
        {
            return this.Select<T>(query).Any<T>();
        }

        /// <summary>
        /// Returns a boolean value indicating whether the object exists in storage.  Note the value for sql may be any 
        /// object which implements the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> used to query the storage.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     if (context.Exists<Product>(new StorageCommand("SELECT * FROM Production.Product WHERE ProductID = 355")))
        ///     {
        ///         Console.WriteLine("Product exists");
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public bool Exists<T>(IStorageCommand command) where T : class, IFlyweight, new()
        {
            return this.Select<T>(command).Any<T>();
        }

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var products = context.Select<Product>();
        ///     foreach (Product product in products)
        ///     {
        ///         Console.WriteLine("ProductNumber: {0}", product.ProductNumber);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>() where T : class, IFlyweight, new()
        {
            return this.Select<T>(new QueryExpression<T>().Select().From<T>());
        }

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage constrained by the amount as specified
        /// by limit.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var products = context.Select<Product>(100);
        ///     foreach (Product product in products)
        ///     {
        ///         Console.WriteLine("ProductNumber: {0}", product.ProductNumber);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(int limit) where T : class, IFlyweight, new()
        {
            return this.Select<T>(new QueryExpression<T>().Select(limit).From<T>());
        }

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage using the criteria as specified by whereExpression.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="whereExpression">An <see cref="Expression"/> that serves as the filter criteria used to query the storage.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var products = context.Select<Product>(p => p.ModifiedDate > DateTime.Now.AddYears(-10));
        ///     foreach (Product product in products)
        ///     {
        ///         Console.WriteLine("ProductNumber: {0}", product.ProductNumber);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(Expression<Func<T, bool>> whereExpression) where T : class, IFlyweight, new()
        {
            return this.Select<T>(new PropertyExpressionConverter<T>().Convert(whereExpression));
        }

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage constrained by the amount as specified
        /// by limit and using the criteria as specified by whereExpression.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var products = context.Select<Product>(100, Product.Properties.ProductLine.Trim().Length() > 0);
        ///     foreach (Product product in products)
        ///     {
        ///         Console.WriteLine("ProductNumber: {0}", product.ProductNumber);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(int limit, PropertyExpression<T> whereExpression) where T : class, IFlyweight, new()
        {
            return this.Select<T>(new QueryExpression<T>().Select(limit).From<T>().Where(whereExpression));
        }

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage constrained by the amount as specified
        /// by limit and using the criteria as specified by whereExpression. 
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
        /// <param name="whereExpression">An <see cref="Expression"/> that serves as the filter criteria used to query the storage.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var products = context.Select<Product>(100, p => p.ProductLine.Trim().Length() > 0);
        ///     foreach (Product product in products)
        ///     {
        ///         Console.WriteLine("ProductNumber: {0}", product.ProductNumber);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(int limit, Expression<Func<T, bool>> whereExpression) where T : class, IFlyweight, new()
        {
            return this.Select<T>(limit, new PropertyExpressionConverter<T>().Convert(whereExpression));
        }

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage using the criteria as specified by whereExpression,
        /// sorted by sortExpressions.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="PropertyExpression{T}"/> by which to sort.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var products = context.Select<Product>(Product.Properties.ModifiedDate > DateTime.Now.AddYears(-10), Product.Properties.ProductNumber.Desc());
        ///     foreach (Product product in products)
        ///     {
        ///         Console.WriteLine("ProductNumber: {0}", product.ProductNumber);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            return this.Select<T>(new QueryExpression<T>().Select().From<T>().Where(whereExpression).OrderBy(sortExpressions));
        }

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage using the criteria as specified by whereExpression,
        /// sorted by sortExpressions. Additional objects will be retrieved in the graph according to their level as specified
        /// by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="depth">The depth limit of the object graph.</param>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="PropertyExpression{T}"/> by which to sort.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var products = context.Select<Product>(PreloadDepth.FirstRelatives, Product.Properties.ProductID.Between(350, 375), Product.Properties.ProductID.Asc());
        ///     foreach (Product product in products)
        ///     {
        ///         foreach (ProductInventory productInventory in product.ProductInventories)
        ///         {
        ///             Console.WriteLine("ProductID: {0}, LocationID: {1}, Quantity: {2}", product.ProductID, productInventory.LocationID, productInventory.Quantity);
        ///         }
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(PreloadDepth depth, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            return this.Select<T>(depth, new QueryExpression<T>().Select().From<T>().Where(whereExpression).OrderBy(sortExpressions));
        }

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage using the criteria as specified by whereExpression,
        /// sorted by sortExpressions. Additional objects will be retrieved in the graph according to their level as specified
        /// by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="options">A <see cref="PreloadOptions{T}"/> specifying additional related objects to retrieve.</param>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="PropertyExpression{T}"/> by which to sort.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var products = context.Select<Product>(PreloadOptions<Product>.LoadWith(p => p.ProductProductPhoto), Product.Properties.ProductID.Between(350, 375), Product.Properties.ProductID.Asc());
        ///     foreach (Product product in products)
        ///     {
        ///         foreach (ProductInventory productInventory in product.ProductInventories)
        ///         {
        ///             Console.WriteLine("ProductID: {0}, LocationID: {1}, Quantity: {2}", product.ProductID, productInventory.LocationID, productInventory.Quantity);
        ///         }
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(PreloadOptions<T> options, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            Stopwatch timer = StartTimer();
            IEnumerable<T> source = this.Select<T>(PreloadDepth.FlatObject, whereExpression, sortExpressions);
            ProcessDataRelations<T>(source, options);
            StopTimer(timer);
            return source;
        }

        /// <summary>
        /// Returns all objects for the specified type T in storage constrained by the amount as specified by limit using the 
        /// criteria as specified by whereExpression, and sorted by sortExpressions.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="PropertyExpression{T}"/> by which to sort.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var products = context.Select<Product>(PreloadDepth.FirstRelatives, Product.Properties.ProductID.Between(350, 375), Product.Properties.ProductID.Asc());
        ///     foreach (Product product in products)
        ///     {
        ///         Console.WriteLine("ProductID: {0}", product.ProductID);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(int limit, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            return this.Select<T>(new QueryExpression<T>().Select(limit).From<T>().Where(whereExpression).OrderBy(sortExpressions));
        }

        /// <summary>
        /// Returns all objects for the specified type T in storage constrained by the amount as specified by limit using the 
        /// criteria as specified by whereExpression, and sorted by sortExpressions. Additional objects will be retrieved in the graph 
        /// according to their level as specified by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
        /// <param name="depth">The depth of the object graph to prefectch.</param>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="PropertyExpression{T}"/> by which to sort.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var products = context.Select<Product>(true, 10, PreloadDepth.FirstRelatives, Product.Properties.ProductLine.Trim().Length() > 0, Product.Properties.ModifiedDate.Desc())
        ///     foreach (Product product in products)
        ///     {
        ///         foreach (ProductInventory productInventory in product.ProductInventories)
        ///         {
        ///             Console.WriteLine("ProductID: {0}, LocationID: {1}, Quantity: {2}", product.ProductID, productInventory.LocationID, productInventory.Quantity);
        ///         }
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(PreloadDepth depth, int limit, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            return this.Select<T>(depth, new QueryExpression<T>().Select(limit).From<T>().Where(whereExpression).OrderBy(sortExpressions));
        }

        /// <summary>
        /// Returns all objects for the specified type T in storage constrained by the amount as specified by limit using the 
        /// criteria as specified by whereExpression, and sorted by sortExpressions. Additional objects will be retrieved in the graph 
        /// according to their level as specified by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
        /// <param name="options">A <see cref="PreloadOptions{T}"/> specifying additional related objects to retrieve.</param>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="PropertyExpression{T}"/> by which to sort.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var products = context.Select<Product>(true, 10, PreloadOptions<Product>.LoadWith(p => p.ProductProductPhotos), Product.Properties.ProductLine.Trim().Length() > 0, Product.Properties.ModifiedDate.Desc())
        ///     foreach (Product product in products)
        ///     {
        ///         foreach (ProductInventory productInventory in product.ProductInventories)
        ///         {
        ///             Console.WriteLine("ProductID: {0}, LocationID: {1}, Quantity: {2}", product.ProductID, productInventory.LocationID, productInventory.Quantity);
        ///         }
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(PreloadOptions<T> options, int limit, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            Stopwatch timer = StartTimer();
            IEnumerable<T> source = this.Select<T>(PreloadDepth.FlatObject, limit, whereExpression, sortExpressions);
            ProcessDataRelations<T>(source, options);
            StopTimer(timer);
            return source;
        }

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IQueryExpression{TSource}"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> by which to retrieve the objects.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var qe = new QueryExpression<Product>()
        ///
        ///     .Select(true, 10, ProductCategory.Properties.Name)
        ///     .From<Product>()
        ///     .LeftJoin<ProductCategory>(Product.Properties.ProductSubcategoryID == ProductCategory.Properties.ProductCategoryID)
        ///     .Where(Product.Properties.ProductID.Between(100, 500))
        ///     .OrderBy(ProductCategory.Properties.Name);
        ///
        ///     var products = context.Select<Product>(qe);
        ///     Console.WriteLine("Count is {0}", products.Count<Product>());
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(IQueryExpression<T> query) where T : class, IFlyweight, new()
        {
            return this.Select<T>(PreloadDepth.FlatObject, query);
        }

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IQueryExpression{TSource}"/>. Additional objects 
        /// will be retrieved in the graph according to their level as specified by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="depth">The depth limit of the object graph.</param>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> by which to retrieve the objects.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var qe = new QueryExpression<Product>()
        ///
        ///     .Select(true, 0, ProductCategory.Properties.Name)
        ///     .From<Product>()
        ///     .Join<ProductCategory>(Product.Properties.ProductSubcategoryID == ProductCategory.Properties.ProductCategoryID)
        ///     .Where(Product.Properties.ProductID.Between(350, 360))
        ///     .OrderBy(ProductCategory.Properties.Name);
        ///
        ///     var products = context.Select<Product>(PreloadDepth.FirstRelatives, qe);
        ///     foreach (Product product in products)
        ///     {
        ///         Console.WriteLine("ProductNumber: {0}, ProductSubcategory Name: {1}", product.ProductNumber, product.ProductSubcategory.Name);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(PreloadDepth depth, IQueryExpression<T> query) where T : class, IFlyweight, new()
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            IStorageCommand command = null;
            IRuntimeMethodQuery<T> runtime = query as IRuntimeMethodQuery<T>;
            if (runtime != null)
            {
                command = this.StorageProvider.BuildStorageCommand<T>(runtime);
                runtime.Command = command;
            }
            else
            {
                command = query.ToCommand();
            }
            return this.Select<T>(depth, command);
        }

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IQueryExpression{TSource}"/>. Additional objects 
        /// will be retrieved in the graph according to their level as specified by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="options">A <see cref="PreloadOptions{T}"/> specifying additional related objects to retrieve.</param>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> by which to retrieve the objects.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var qe = new QueryExpression<Product>()
        ///
        ///     .Select(true, 0, ProductCategory.Properties.Name)
        ///     .From<Product>()
        ///     .Join<ProductCategory>(Product.Properties.ProductSubcategoryID == ProductCategory.Properties.ProductCategoryID)
        ///     .Where(Product.Properties.ProductID.Between(350, 360))
        ///     .OrderBy(ProductCategory.Properties.Name);
        ///
        ///     var products = context.Select<Product>(PreloadOptions<Product>.LoadWith(p => p.ProductProductPhotos), qe);
        ///     foreach (Product product in products)
        ///     {
        ///         Console.WriteLine("ProductNumber: {0}, ProductSubcategory Name: {1}", product.ProductNumber, product.ProductSubcategory.Name);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(PreloadOptions<T> options, IQueryExpression<T> query) where T : class, IFlyweight, new()
        {
            Stopwatch timer = StartTimer();
            IEnumerable<T> source = this.Select<T>(PreloadDepth.FlatObject, query);
            ProcessDataRelations<T>(source, options);
            StopTimer(timer);
            return source;
        }

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IStorageCommand"/>.  Note the value for 
        /// sql may be any object which implements the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> by which to retrieve the objects.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var product = context.Select<Product>(new StorageCommand("SELECT * FROM Production.Product WHERE ProductID = 355")).ToSingle();
        ///     Console.WriteLine("ProductNumber is {0}", product.ProductNumber);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public virtual IEnumerable<T> Select<T>(IStorageCommand command) where T : class, IFlyweight, new()
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (string.IsNullOrEmpty(command.SqlText))
            {
                throw new InvalidOperationException(string.Format("There is no SqlText associated with the {0} object.", typeof(IStorageCommand).Name));
            }
            if (!typeof(StoredProcedure).IsAssignableFrom(command.GetType()) && command.GetTransactionType() != TransactionType.Select)
            {
                throw new ArgumentException(ErrorStrings.SelectTransactionTypeException);
            }
            Stopwatch timer = StartTimer();
            ValidateOperation<T>(TransactionType.Select);
            IEnumerable<T> source;
            string key = null;
            if (!TryGetCachedObject<T>(command, out key, out source))
            {
                DataOperationEventArgs<T> e = new DataOperationEventArgs<T>(TransactionType.Select, command);
                OnSelecting<T>(e);
                if (!e.Cancel)
                {
                    source = this.StorageProvider.ProcessSelect<T>(command);
                    SetCachedObject<T>(key, source);
                }
            }
            AutoCloneSource<T>((FlyweightSet<T>)source);
            StopTimer(timer);
			SetResponseMessage<T>((IResponseMessage)source);
			TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("Execution Time: {0}", this.ExecutionTime));
            return source;
        }

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IStorageCommand"/>. Additional objects 
        /// will be retrieved in the graph according to their level as specified by depth.  Note the value for sql may be any 
        /// object which implements the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="depth">The depth limit of the object graph.</param>
        /// <param name="command">The <see cref="IStorageCommand"/> by which to retrieve the objects.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var product = context.Select<Product>(PreloadDepth.FirstRelatives, new StorageCommand("SELECT * FROM Production.Product WHERE ProductID = 355")).ToSingle();
        ///     Console.WriteLine("Inventory location count is {0}", product.ProductInventories.Count);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(PreloadDepth depth, IStorageCommand command) where T : class, IFlyweight, new()
        {
            Stopwatch timer = StartTimer();
            IEnumerable<T> source;
            string key = null;
            if (!TryGetCachedObject<T>(depth, command, out key, out source))
            {
                source = this.Select<T>(command);
                ProcessDataRelations<IEnumerable<T>>(source, depth);
                SetCachedObject<T>(key, source);
            }
            AutoCloneSource<T>((FlyweightSet<T>)source);
            StopTimer(timer);
			SetResponseMessage<T>((IResponseMessage)source);
            return source;
        }

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IQueryExpression{TSource}"/>. Additional objects 
        /// will be retrieved in the graph according to their level as specified by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="options">A <see cref="PreloadOptions{T}"/> specifying additional related objects to retrieve.</param>
        /// <param name="command">The <see cref="IStorageCommand"/> used to query the storage.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var products = context.Select<Product>(PreloadOptions<Product>.LoadWith(p => p.ProductProductPhotos), new StorageCommand("SELECT * FROM Production.Product"));
        ///     foreach (Product product in products)
        ///     {
        ///         Console.WriteLine("ProductNumber: {0}, ProductSubcategory Name: {1}", product.ProductNumber, product.ProductSubcategory.Name);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(PreloadOptions<T> options, IStorageCommand command) where T : class, IFlyweight, new()
        {
            Stopwatch timer = StartTimer();
            IEnumerable<T> source;
            string key = null;
            if (!TryGetCachedObject<T>(options, command, out key, out source))
            {
                source = this.Select<T>(command);
                ProcessDataRelations<T>(source, options);
                SetCachedObject<T>(key, source);
            }
            AutoCloneSource<T>((FlyweightSet<T>)source);
			StopTimer(timer);
			SetResponseMessage<T>((IResponseMessage)source);
            return source;
        }

        /// <summary>
        /// Reloads the source object based upon its identifiers as specified by its <see cref="DataColumnAttribute"/> Identifer properties.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to reload.</typeparam>
        /// <param name="source">An instance of T.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Retrieve a product and change its ModifiedDate value
        ///     var product = context.Select<Product>(Product.Properties.ProductID == 355).ToSingle();
        ///     product.ModifiedDate = DateTime.Now;
        ///     Console.WriteLine("Product ModifiedDate is {0}", product.ModifiedDate);
        ///     
        ///     // Abandon the change and refresh the object
        ///     product = context.Reload<Product>(product);
        ///     Console.WriteLine("Product ModifiedDate is {0}", product.ModifiedDate);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public T Reload<T>(T source) where T : class, IFlyweight, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (typeof(T).IsEnumerable())
            {
                throw new InvalidOperationException(ErrorStrings.InvalidEnumerableArgumentException);
            }
            try
            {
                Stopwatch timer = StartTimer();
                ValidateType<T>();
                ValidateOperation<T>(TransactionType.Select);
                T obj = this.StorageProvider.ProcessReload<T>(source);
                AutoCloneObject<T>(obj);
                StopTimer(timer);
                SetResponseMessage<T>(source);
                return obj;
            }
            catch (Exception e)
            {
                SetResponseMessage<T>(source, e);
                throw;
            }
        }

        /// <summary>
        /// Executes an <see cref="IStorageCommand"/> against storage.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public virtual IStorageCommand Execute(IStorageCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            Stopwatch timer = StartTimer();
            DataOperationEventArgs e = new DataOperationEventArgs(TransactionType.Unknown, command);
            OnExecuting(e);
            if (!e.Cancel)
            {
                command = this.StorageProvider.ProcessExecute(command);
            }
            StopTimer(timer);
			TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("Execution Time: {0}", this.ExecutionTime));
            return command;
        }

        /// <summary>
        /// Performs an insert into the target entity with the values as given by the source object's properties.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to insert.</typeparam>
        /// <param name="source">An instance of T.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Retrieve a product by ProductID
        ///     var product = context.Select<Product>(Product.Properties.ProductID == 355).ToSingle();
        ///     
        ///     // Ensure that the retrieved product exists
        ///     if (product != null)
        ///     {
        ///         // Clone the product and change its properties to satisfy the constraints in the underlying entity
        ///         Product product1 = ObjectCloner.Clone<Product>(product);
        ///         product1.ProductID = 0;
        ///         product1.ProductNumber = Guid.NewGuid().ToString().Substring(0, 5);
        ///         product1.Name = "My New Product";
        ///         product1.rowguid = Guid.NewGuid();
        ///         
        ///         // Insert the new product and print its surrogate primary key
        ///         context.Insert<Product>(product1);
        ///         Console.WriteLine("ProductID is {0}", product1.ProductID);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public virtual T Insert<T>(T source) where T : class, IFlyweight, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            try
            {
                Stopwatch timer = StartTimer();
                ValidateType<T>();
                ValidateOperation<T>(TransactionType.Insert);
                if (typeof(T).IsEnumerable())
                {
                    source = this.InvokeListMethod<T>(source, MethodBase.GetCurrentMethod());
                }
                else
                {
                    DataOperationEventArgs<T> e = new DataOperationEventArgs<T>(TransactionType.Insert, source);
                    OnInserting<T>(e);
                    if (!e.Cancel)
                    {
                        ValidateObject<T>(TransactionType.Insert, source);
                        CheckConcurrency<T>(source);
                        source = this.StorageProvider.ProcessInsert<T>(source);
                        ResetChecksum<T>(source);
                        ResetChangedProperties<T>(source);
                    }
                }
                StopTimer(timer);
                SetResponseMessage<T>(source);
                TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("Execution Time: {0}", this.ExecutionTime));
                return source;
            }
            catch (Exception e)
            {
                SetResponseMessage<T>(source, e);
                throw;
            }
        }

        /// <summary>
        /// Performs multiple inserts into the target entity using the supplied source.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to insert.</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Create a new empty list of products to be inserted
        ///     var list = new List<Product>();
        ///
        ///     // Loop through a collection of products
        ///     foreach (Product p in context.Select<Product>(Product.Properties.ProductID.Between(300, 349)))
        ///     {
        ///         // Create a new product from a clone and add it to the list
        ///         Product p1 = ObjectCloner.Clone<Product>(product);
        ///         p1.ProductID = 0;
        ///         p1.ProductNumber = Guid.NewGuid().ToString().Substring(0, 5);
        ///         p1.Name = product.ProductNumber;
        ///         p1.rowguid = Guid.NewGuid();
        ///         list.Add(p1);
        ///     }
        ///
        ///     // Insert the new list of products and print their surrogate primary keys
        ///     context.Insert<Product>(list);
        ///     foreach (Product p in list)
        ///     {
        ///         Console.WriteLine("ProductID: {0}", p.ProductID);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public virtual IEnumerable<T> Insert<T>(IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!source.Any<T>())
            {
                return source;
            }
            Stopwatch timer = StartTimer();
			IList<T> list = source as FlyweightSet<T>;
			if (list == null)
			{
				list = source.ToList();
			}
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = this.Insert<T>(list[i]);
            }
            StopTimer(timer);
			TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("Execution Time: {0}", this.ExecutionTime));
            return source;
        }

        /// <summary>
        /// Performs an insert into the target entity using the supplied <see cref="IQueryExpression{TSource}"/>. 
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to insert.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to insert the objects.</param>
        /// <example>
        /// The following example uses a Department class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Construct an insert QueryExpression object
        ///     var qe = new QueryExpression<Department>()
        ///
        ///     .Insert<Department>(Department.Properties.Name, Department.Properties.GroupName, Department.Properties.ModifiedDate)
        ///     .Values("My New Department", "My New Group", DateTime.Now);
        ///     
        ///     // Insert the values
        ///     context.Insert<Department>(qe);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public virtual void Insert<T>(IQueryExpression<T> query) where T : class, IFlyweight, new()
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            Stopwatch timer = StartTimer();
            ValidateType<T>();
            ValidateOperation<T>(TransactionType.Insert);
            IStorageCommand command = null;
            IRuntimeMethodQuery<T> runtime = query as IRuntimeMethodQuery<T>;
            if (runtime != null)
            {
                command = this.StorageProvider.BuildStorageCommand<T>(runtime);
                runtime.Command = command;
            }
            else
            {
                command = query.ToCommand();
            }
            if (command.TransactionType != TransactionType.Insert)
            {
                throw new InvalidOperationException(string.Format("The {0} for the {1} must be of type Insert.", typeof(TransactionType).Name, query.GetType().Name));
            }
            DataOperationEventArgs<T> e = new DataOperationEventArgs<T>(TransactionType.Insert, command);
            OnInserting<T>(e);
            if (!e.Cancel)
            {
                this.StorageProvider.ProcessInsert<T>(command);
            }
			TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("Execution Time: {0}", this.ExecutionTime));
            StopTimer(timer);
        }

        /// <summary>
        /// Performs multiple inserts into the target entity using the supplied source and batch size. Note that this
        /// functionality is only available for a subset of <see cref="IStorageProvider"/>s. 
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to insert.</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        /// <param name="batchSize">The number of inserts contained within a batch.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Create a new empty list of products to be inserted
        ///     var list = new List<Product>();
        ///
        ///     // Loop through a collection of products
        ///     foreach (Product p in context.Select<Product>(Product.Properties.ProductID.Between(300, 349)))
        ///     {
        ///         // Create a new product from a clone and add it to the list
        ///         Product p1 = ObjectCloner.Clone<Product>(product);
        ///         p1.ProductID = 0;
        ///         p1.ProductNumber = Guid.NewGuid().ToString().Substring(0, 5);
        ///         p1.Name = product.ProductNumber;
        ///         p1.rowguid = Guid.NewGuid();
        ///         list.Add(p1);
        ///     }
        ///
        ///     // Insert the new list of products by batches of 10 and print their surrogate primary keys
        ///     context.Insert<Product>(list, 10);
        ///     foreach (Product p in list)
        ///     {
        ///         Console.WriteLine("ProductID: {0}", p.ProductID);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public virtual void Insert<T>(IEnumerable<T> source, int batchSize) where T : class, IFlyweight, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!source.Any<T>())
            {
                return;
            }
            Stopwatch timer = StartTimer();
            ValidateType<T>();
            ValidateOperation<T>(TransactionType.Insert);
            bool cancel = false;
            foreach (T item in source)
            {
                DataOperationEventArgs<T> e = new DataOperationEventArgs<T>(TransactionType.Insert, item);
                OnInserting<T>(e);
                cancel = e.Cancel;
                if (cancel)
                {
                    break;
                }
            }
            if (!cancel)
            {
				ValidateObject<IEnumerable<T>>(TransactionType.Insert, source);
				this.StorageProvider.ProcessInsert<T>(source, batchSize);
                ResetChecksum<IEnumerable<T>>(source);
                ResetChangedProperties<IEnumerable<T>>(source);
            }
            StopTimer(timer);
			TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("Execution Time (IEnumerable): {0}", this.ExecutionTime));
        }

        /// <summary>
        /// Performs an update of the target entity using the supplied <see cref="IQueryExpression{TSource}"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the object to update.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to insert the object.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Construct a QueryExpression object with the criteria for the update
        ///     var qe = new QueryExpression<Product>()
        ///
        ///     .Update()
        ///     .Set(Product.Properties.ModifiedDate == DateTime.Now, Product.Properties.MakeFlag == true)
        ///     .From<Product>()
        ///     .Join<ProductInventory>(ProductInventory.Properties.ProductID == Product.Properties.ProductID)
        ///     .Where(ProductInventory.Properties.Quantity > 0);
        ///
        ///     // Update the products directly without retrieving them
        ///     context.Update<Product>(qe);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public virtual void Update<T>(IQueryExpression<T> query) where T : class, IFlyweight, new()
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            Stopwatch timer = StartTimer();
            ValidateType<T>();
            ValidateOperation<T>(TransactionType.Update);
            IStorageCommand command = null;
            IRuntimeMethodQuery<T> runtime = query as IRuntimeMethodQuery<T>;
            if (runtime != null)
            {
                command = this.StorageProvider.BuildStorageCommand<T>(runtime);
                runtime.Command = command;
            }
            else
            {
                command = query.ToCommand();
            }
            if (command.TransactionType != TransactionType.Update)
            {
                throw new InvalidOperationException(string.Format("The {0} for the {1} must be of type Update.", typeof(TransactionType).Name, query.GetType().Name));
            }
            DataOperationEventArgs<T> e = new DataOperationEventArgs<T>(TransactionType.Update, command);
            OnUpdating<T>(e);
            if (!e.Cancel)
            {
                this.StorageProvider.ProcessUpdate<T>(command);
            }
            StopTimer(timer);
			TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("Execution Time: {0}", this.ExecutionTime));
        }

        /// <summary>
        /// Performs an update of the target entity with the values as given by the source object's properties.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to update.</typeparam>
        /// <param name="source">An instance of T.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var product = context.Select<Product>(Product.Properties.ProductID == 355).ToSingle();
        ///     product.ModifiedDate = DateTime.Now;
        ///     context.Update<Product>(product);
        ///     Console.WriteLine("Product ModifiedDate is {0}", product.ModifiedDate);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public virtual T Update<T>(T source) where T : class, IFlyweight, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            try
            {
                Stopwatch timer = StartTimer();
                ValidateType<T>();
                ValidateOperation<T>(TransactionType.Update);
                if (typeof(T).IsEnumerable())
                {
                    source = InvokeListMethod<T>(source, MethodBase.GetCurrentMethod());
                }
                else
                {
                    if (source.IsChanged<T>() || this.UpdateUnchangedObjects)
                    {
                        DataOperationEventArgs<T> e = new DataOperationEventArgs<T>(TransactionType.Update, source);
                        OnUpdating<T>(e);
                        if (!e.Cancel)
                        {
                            ValidateObject<T>(TransactionType.Update, source);
                            CheckConcurrency<T>(source);
                            source = this.StorageProvider.ProcessUpdate<T>(source);
                            ResetChecksum<T>(source);
                            ResetChangedProperties<T>(source);
                        }
                    }
                    else
                    {
                        TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("No changes were found in the supplied {0} object. The update has been aborted.", typeof(T).FullName));
                    }
                }
                StopTimer(timer);
                SetResponseMessage<T>(source);
                TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("Execution Time: {0}", this.ExecutionTime));
                return source;
            }
            catch (Exception e)
            {
                SetResponseMessage<T>(source, e);
                throw;
            }
        }

        /// <summary>
        /// Performs multiple updates of the target entity using the supplied source.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to update.</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Retrieve the top 100 products
        ///     var products = context.Select<Product>(100);
        ///     Console.WriteLine("Count is {0}", products.Count<Product>());
        ///     
        ///     // Change the ModifiedDate of each product
        ///     foreach (product product in products)
        ///     {
        ///         products.ModifiedDate = DateTime.Now;
        ///     }
        ///        
        ///     // Update the products
        ///     context.Update<Product>(products);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public virtual IEnumerable<T> Update<T>(IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!source.Any<T>())
            {
                return source;
            }
            Stopwatch timer = StartTimer();
			IList<T> list = source as FlyweightSet<T>;
			if (list == null)
			{
				list = source.ToList();
			}
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = this.Update<T>(list[i]);
            }
            StopTimer(timer);
			TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("Execution Time (IEnumerable): {0}", this.ExecutionTime));
            return source;
        }

        /// <summary>
        /// Performs multiple updates of the target entity using the supplied source and batch size. Note that this
        /// functionality is only available for a subset of <see cref="IStorageProvider"/>s. 
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to update.</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        /// <param name="batchSize">The number of updates contained within a batch.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Retrieve the top 100 products
        ///     var products = context.Select<Product>(100);
        ///     
        ///     // Change the ModifiedDate of each product
        ///     foreach (Product product in products)
        ///     {
        ///         product.ModifiedDate = DateTime.Now;
        ///     }
        ///
        ///     // Update the products in batches of 20
        ///     context.Update<Product>(products, 20);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public virtual void Update<T>(IEnumerable<T> source, int batchSize) where T : class, IFlyweight, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!source.Any<T>())
            {
                return;
            }
            Stopwatch timer = StartTimer();
            ValidateType<T>();
            ValidateOperation<T>(TransactionType.Update);
            ValidateObject<IEnumerable<T>>(TransactionType.Update, source);
            FlyweightSet<T> list = new FlyweightSet<T>();
            bool cancel = false;
            foreach (T item in source)
            {
                if (item.IsChanged<T>() || this.UpdateUnchangedObjects)
                {
                    DataOperationEventArgs<T> e = new DataOperationEventArgs<T>(TransactionType.Update, item);
                    OnUpdating<T>(e);
                    cancel = e.Cancel;
                    if (cancel)
                    {
                        break;
                    }
                    list.Add(item);
                }
            }
            if (!cancel)
            {
                this.StorageProvider.ProcessUpdate<T>(list, batchSize);
                ResetChecksum<IEnumerable<T>>(source);
                ResetChangedProperties<IEnumerable<T>>(source);
            }
            StopTimer(timer);
			TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("Execution Time (IEnumerable): {0}", this.ExecutionTime));
        }

        /// <summary>
        /// If the object alrady exists, performs an update of the target entity using the supplied source, otherwise
        /// an insert is applied.
        /// </summary>
        /// <typeparam name="T">The type parameter of the object to either update or insert.</typeparam>
        /// <param name="source">An instance of T.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Create a new product and set required properties
        ///     var product = new Product()
        ///     {
        ///         Name = Guid.NewGuid().ToString().Substring(0, 5),
        ///         ProductNumber = product.Name,
        ///         SafetyStockLevel = 1,
        ///         ReorderPoint = 1
        ///     };
        ///     
        ///     // Perform an upsert on the target entity using the newly created product
        ///     context.Upsert<Product>(product);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public T Upsert<T>(T source) where T : class, IFlyweight, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if ((((IPropertyStorage)source.Storage).DataSource.RowState & RowState.Added) == RowState.Added && !this.Exists<T>(source))
            {
                this.Insert<T>(source);
            }
            else
            {
                this.Update<T>(source);
            }    
            return source;
        }

        /// <summary>
        /// For each object in source, if the object alrady exists, performs an update of the target entity using 
        /// the supplied source, otherwise an insert is applied.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to either update or insert.</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Retrieve a list of products
        ///     var products = context.Select<Product>(Product.Properties.ListPrice > 0).ToFlyweightSet();
        ///
        ///     // Change the ModifiedDate of each product
        ///     foreach (var product in products)
        ///     {
        ///         product.ModifiedDate = DateTime.Now;
        ///     }
        ///        
        ///     // Create a new product and set required properties
        ///     var product = new Product()
        ///     {
        ///         Name = Guid.NewGuid().ToString().Substring(0, 5),
        ///         ProductNumber = product.Name,
        ///         SafetyStockLevel = 1,
        ///         ReorderPoint = 1
        ///     };
        ///     
        ///     // Add the new product to the list
        ///     products.Add(product);
        ///       
        ///     // Update the target entity with the list of products
        ///     context.Upsert<Product>(products);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Upsert<T>(IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!source.Any<T>())
            {
                return source;
            }
            Stopwatch timer = StartTimer();
			IList<T> list = source as FlyweightSet<T>;
			if (list == null)
			{
				list = source.ToList();
			}
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = this.Upsert<T>(list[i]);
            }
            StopTimer(timer);
            return source;
        }

        /// <summary>
        /// Performs a delete on the target entity using the supplied <see cref="IQueryExpression{TSource}"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the object to update.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to delete the object.</param>
        /// <example>
        /// The following example uses a Department class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Create a new department and set required properties
        ///     var department = new Department()
        ///     {
        ///         Name = "My New Department",
        ///         GroupName = "My New Department Group",
        ///         ModifiedDate = DateTime.Now
        ///     };
        ///
        ///     // Insert the new department
        ///     context.Insert<Department>(department);
        ///
        ///     // Delete the department using a QueryExpression, specifying the newly created department
        ///     context.Delete<Department>(new QueryExpression<Department>().Delete<Department>().Where(Department.Properties.DepartmentID == department.DepartmentID));
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public virtual void Delete<T>(IQueryExpression<T> query) where T : class, IFlyweight, new()
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            Stopwatch timer = StartTimer();
            ValidateType<T>();
            ValidateOperation<T>(TransactionType.Delete);
            IStorageCommand command = null;
            IRuntimeMethodQuery<T> runtime = query as IRuntimeMethodQuery<T>;
            if (runtime != null)
            {
                command = this.StorageProvider.BuildStorageCommand<T>(runtime);
                runtime.Command = command;
            }
            else
            {
                command = query.ToCommand();
            }
            if (command.TransactionType != TransactionType.Delete)
            {
                throw new InvalidOperationException(string.Format("The {0} for the {1} must be of type Delete.", typeof(TransactionType).Name, query.GetType().Name));
            }
            DataOperationEventArgs<T> e = new DataOperationEventArgs<T>(TransactionType.Delete, command);
            OnDeleting<T>(e);
            if (!e.Cancel)
            {
                this.StorageProvider.ProcessDelete<T>(command);
            }
            StopTimer(timer);
			TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("Execution Time: {0}", this.ExecutionTime));
        }

        /// <summary>
        /// Deletes the source from storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to delete.</typeparam>
        /// <param name="source">An instance of T.</param>
        /// <example>
        /// The following example uses a Department class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Create a new department and set required properties
        ///     var department = new Department()
        ///     {
        ///         Name = "My New Department",
        ///         GroupName = "My New Department Group",
        ///         ModifiedDate = DateTime.Now,
        ///         ModifiedDate = DateTime.Now
        ///     };
        ///
        ///     // Insert the new department
        ///     context.Insert<Department>(department);
        ///
        ///     // Delete the department
        ///     context.Delete<Department>(department);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public virtual T Delete<T>(T source) where T : class, IFlyweight, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            try
            {
                Stopwatch timer = StartTimer();
                ValidateType<T>();
                ValidateOperation<T>(TransactionType.Delete);
                if (typeof(T).IsEnumerable())
                {
                    this.InvokeListMethod<T>(source, MethodBase.GetCurrentMethod());
                }
                else
                {
                    DataOperationEventArgs<T> e = new DataOperationEventArgs<T>(TransactionType.Delete, source);
                    OnDeleting<T>(e);
                    if (!e.Cancel)
                    {
                        this.StorageProvider.ProcessDelete<T>(source);
                        ResetChecksum<T>(source);
                        ResetChangedProperties<T>(source);
                    }
                }
                StopTimer(timer);
                SetResponseMessage<T>(source);
                TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("Execution Time: {0}", this.ExecutionTime));
                return source;
            }
            catch (Exception e)
            {
                SetResponseMessage<T>(source, e);
                throw;
            }
        }

        /// <summary>
        /// Performs a delete on the target entity given the supplied filter criteria.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to delete.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used as the filter for the delete.</param>
        /// <example>
        /// The following example uses a Department class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Create a new department and set required properties
        ///     var department = new Department()
        ///     {
        ///         Name = "My New Department",
        ///         GroupName = "My New Department Group",
        ///         ModifiedDate = DateTime.Now,
        ///         ModifiedDate = DateTime.Now
        ///     };
        ///
        ///     // Insert the new department
        ///     context.Insert<Department>(department);
        ///
        ///     // Delete the department
        ///     context.Delete<Department>(Department.Properties.DepartmentID == department.DepartmentID);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public void Delete<T>(PropertyExpression<T> whereExpression) where T : class, IFlyweight, new()
        {
            this.Delete<T>(new QueryExpression<T>().Delete().Where(whereExpression));
        }

        /// <summary>
        /// Performs a delete on the target entity given the supplied filter criteria.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to delete.</typeparam>
        /// <param name="whereExpression">An <see cref="Expression"/> that serves as the filter criteria used to query the storage.</param>
        /// <example>
        /// The following example uses a Department class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Create a new department and set required properties
        ///     var department = new Department()
        ///     {
        ///         Name = "My New Department",
        ///         GroupName = "My New Department Group",
        ///         ModifiedDate = DateTime.Now,
        ///         ModifiedDate = DateTime.Now
        ///     };
        ///
        ///     // Insert the new department
        ///     context.Insert<Department>(department);
        ///
        ///     // Delete the department
        ///     context.Delete<Department>(d => d.DepartmentID == department.DepartmentID);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public void Delete<T>(Expression<Func<T, bool>> whereExpression) where T : class, IFlyweight, new()
        {
            this.Delete<T>(new PropertyExpressionConverter<T>().Convert(whereExpression));
        }

        /// <summary>
        /// Deletes all instances contained within source from storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to delete.</typeparam>
        /// <param name="source">An instance of T.</param>
        /// <example>
        /// The following example uses a Department class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Create a list of departments
        ///     var departments = new List<Department>();
        ///
        ///     // Create 10 new departments, set their required properties and add each to the list
        ///     for (int i = 0; i < 10; i++)
        ///     {
        ///         var department = new Department()
        ///         {
        ///             Name = "My New Department",
        ///             GroupName = "My New Department Group",
        ///             ModifiedDate = DateTime.Now,
        ///             ModifiedDate = DateTime.Now
        ///         };
        ///         departments.Add(department);
        ///     }
        ///
        ///     // create the new departments
        ///     context.Insert<Department>(departments);
        ///     
        ///     // Delete the newly created departments
        ///     context.Delete<Department>(departments);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public virtual IEnumerable<T> Delete<T>(IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Stopwatch timer = StartTimer();
			IList<T> list = source as FlyweightSet<T>;
			if (list == null)
			{
				list = source.ToList();
			}
            for (int i = 0; i < list.Count; i++)
            {
                this.Delete<T>(list[i]);
            }
            ClearDeletedItems<T>(source);
            StopTimer(timer);
			TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("Execution Time (IEnumerable): {0}", this.ExecutionTime));
            return source;
        }

        /// <summary>
        /// Performs multiple deletes on the target entity using the supplied source and batch size. Note that this
        /// functionality is only available for a subset of <see cref="IStorageProvider"/>s. 
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to delete.</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        /// <param name="batchSize">The number of deletes contained within a batch.</param>
        /// <example>
        /// The following example uses a Department class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Create a list of departments
        ///     var departments = new List<Department>();
        ///
        ///     // Create 50 new departments, set their required properties and add each to the list
        ///     for (int i = 0; i < 50; i++)
        ///     {
        ///         var department = new Department()
        ///         {
        ///             Name = "My New Department",
        ///             GroupName = "My New Department Group",
        ///             ModifiedDate = DateTime.Now,
        ///             ModifiedDate = DateTime.Now
        ///         };
        ///         departments.Add(department);
        ///     }
        ///
        ///     // create the new departments
        ///     context.Insert<Department>(departments);
        ///     
        ///     // Delete the newly created departments in bacthes of 10
        ///     context.Delete<Department>(departments, 10);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public virtual void Delete<T>(IEnumerable<T> source, int batchSize) where T : class, IFlyweight, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (!source.Any<T>())
            {
                return;
            }
            Stopwatch timer = StartTimer();
            ValidateType<T>();
            ValidateOperation<T>(TransactionType.Delete);
            foreach (T item in source)
            {
                this.StorageProvider.ProcessDelete<T>(source, batchSize);
                ResetChecksum<IEnumerable<T>>(source);
                ResetChangedProperties<IEnumerable<T>>(source);
                ClearDeletedItems<T>(source);
            }
            StopTimer(timer);
			TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("Execution Time (IEnumerable): {0}", this.ExecutionTime));
        }

        /// <summary>
        /// Truncates a target entity located in storage.
        /// </summary>
        /// <typeparam name="T">The type abstracting the table to truncate.</typeparam>
        /// <example>
        /// The following example uses a ErrrorLog class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Truncate the table
        ///     context.Truncate<ErrorLog>();
        ///     
        ///     // Get the count for the ErrorLog table
        ///     var qe = new QueryExpression<ErrorLog>()
        ///
        ///     .Select(ErrorLog.Properties.ErrorLogID.Count().As("Count"))
        ///     .From<ErrorLog>();
        ///
        ///     // Ensure that the table has been truncated
        ///     int count = this.Context.Select<QueryResult>(qe).ToList()[0].GetField<int>("Count");
        ///     Console.WriteLine("Count is {0}", count);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public virtual void Truncate<T>() where T : class, IFlyweight, new()
        {
            ValidateOperation<T>(TransactionType.Truncate);
            Stopwatch timer = StartTimer();
            DataOperationEventArgs<T> e = new DataOperationEventArgs<T>(TransactionType.Truncate, new T());
            OnTruncating<T>(e);
            if (!e.Cancel)
            {
                this.StorageProvider.ProcessTruncate<T>();
            }
            StopTimer(timer);
        }

        /// <summary>
        /// Performs all necessary operations on the supplied <see cref="IFlyweightSet{T}"/> object. Note that all objects which
        /// have been removed from the <see cref="IFlyweightSet{T}"/> will be deleted, while the others will either be inserted
        /// or updated based upon whether or not they exist in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of the changed objects to persist.</typeparam>
        /// <param name="source">An instance of a <see cref="FlyweightSet{T}"/>.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Retrieve an IEnumerable<Product>
        ///     var products = context.Select<Product>(100).ToFlyweightSet();
        ///     
        ///     // Update the ModifiedDate property on each product
        ///     foreach (Product p in products)
        ///     {
        ///         p.ModifiedDate = DateTime.Now;
        ///     }
        ///
        ///     // Create a product from a clone
        ///     Product product = ObjectCloner.Clone<Product>(products[0]);
        ///     product.ProductID = 0;
        ///     product.ProductNumber = Guid.NewGuid().ToString().Substring(0, 5);
        ///     product.Name = "My New Product";
        ///     product.rowguid = Guid.NewGuid();
        ///     
        ///     // Add the product to the IFlyweightSet
        ///     products.Add(product);
        ///     
        ///     // Persist changes
        ///     context.Persist<Product>(products);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Persist<T>(FlyweightSet<T> source) where T : class, IFlyweight, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Stopwatch timer = StartTimer();
            foreach (ResultSet.Row row in ((IChangeTrackable<T>)source).DeletedItems)
            {
                T obj = new T();
                ((IPropertyStorage)obj.Storage).DataSource = row;
                this.Delete<T>(obj);
            }
            this.Upsert<T>(source);
			StopTimer(timer);
			SetResponseMessage<T>(source);
            return source;
        }

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage sorted by sortExpressions.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="sortExpressions">An <see cref="Expression"/> used to qualify the property and direction of the sort.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var products = context.Select<Product>new SortExpression<Product>(p => p.ProductModelId, SortDirection.Desc));
        ///     foreach (Product product in products)
        ///     {
        ///         Console.WriteLine("ProductNumber: {0}", product.ProductNumber);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(params SortExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            return this.Select<T>(PreloadDepth.FlatObject, null, sortExpressions);
        }

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage sorted by sortExpression.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="whereExpression">An <see cref="Expression"/> that serves as the filter criteria used to query the storage.</param>
        /// <param name="sortExpressions">An <see cref="Expression"/> used to qualify the property and direction of the sort.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var products = context.Select<Product>new SortExpression<Product>(p => p.ModifiedDate > DateTime.Now.AddYears(-10), new SortExpression<Product>(p => p.ProductModelId));
        ///     foreach (Product product in products)
        ///     {
        ///         Console.WriteLine("ProductNumber: {0}", product.ProductNumber);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(Expression<Func<T, bool>> whereExpression, params SortExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            return this.Select<T>(PreloadDepth.FlatObject, whereExpression, sortExpressions);
        }

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage using the criteria as specified by whereExpression,
        /// sorted by sortExpressions.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="depth">The depth limit of the object graph.</param>
        /// <param name="whereExpression">An <see cref="Expression"/> that serves as the filter criteria used to query the storage.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="SortExpression{T}"/> used to qualify the properties and sorting directions.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var products = context.Select<Product>(p => p.ModifiedDate > DateTime.Now.AddYears(-10), new SortExpression<Product>(p => p.ProductModelId, SortDirection.Desc));
        ///     foreach (Product product in products)
        ///     {
        ///         Console.WriteLine("ProductNumber: {0}", product.ProductNumber);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(PreloadDepth depth, Expression<Func<T, bool>> whereExpression, params SortExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            List<PropertyExpression<T>> properties = new List<PropertyExpression<T>>();
            if (sortExpressions == null || sortExpressions.Length == 0)
            {
                properties.Add(PropertyExpression<T>.Empty);
            }
            else
            {
                foreach (SortExpression<T> item in sortExpressions)
                {
                    PropertyExpression<T> property = new PropertyExpressionConverter<T>().Convert(item.Expression);
                    if (item.Direction == SortDirection.Desc)
                    {
                        property.Desc();
                    }
                    properties.Add(property);
                }
            }
            return this.Select<T>(depth, new PropertyExpressionConverter<T>().Convert(whereExpression), properties.ToArray());
        }

        /// <summary>
        /// Returns all objects for the specified type T in storage constrained by the amount as specified by limit using the 
        /// criteria as specified by whereExpression, and sorted by sortExpressions.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
        /// <param name="whereExpression">An <see cref="Expression"/> used to filter the objects to be retrieved.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="SortExpression{T}"/> used to qualify the properties and sorting directions.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var products = context.Select<Product>(true, 100, p => p.ModifiedDate > DateTime.Now.AddYears(-10), new SortExpression<Product>(p => p.ProductModelId));
        ///     foreach (Product product in products)
        ///     {
        ///         Console.WriteLine("ProductID: {0}", product.ProductID);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(int limit, Expression<Func<T, bool>> whereExpression, params SortExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            List<PropertyExpression<T>> properties = new List<PropertyExpression<T>>();
            if (sortExpressions == null || sortExpressions.Length == 0)
            {
                properties.Add(PropertyExpression<T>.Empty);
            }
            else
            {
                foreach (SortExpression<T> item in sortExpressions)
                {
                    PropertyExpression<T> property = new PropertyExpressionConverter<T>().Convert(item.Expression);
                    if (item.Direction == SortDirection.Desc)
                    {
                        property.Desc();
                    }
                    properties.Add(property);
                }
            }
            return this.Select<T>(limit, new PropertyExpressionConverter<T>().Convert(whereExpression), properties.ToArray());
        }

        /// <summary>
        /// Returns all objects for the specified type T in storage constrained by the amount as specified by limit using the 
        /// criteria as specified by whereExpression, and sorted by sortExpressions.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="depth">The depth of the object graph to prefectch.</param>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
        /// <param name="whereExpression">An <see cref="Expression"/> used to filter the objects to be retrieved.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="SortExpression{T}"/> used to qualify the properties and sorting directions.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var products = context.Select<Product>(true, 100, PreloadDepth.FirstRelatives, p => p.ModifiedDate > DateTime.Now.AddYears(-10), new SortExpression<Product>(p => p.ProductModelId));
        ///     foreach (Product product in products)
        ///     {
        ///         Console.WriteLine("ProductID: {0}", product.ProductID);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(PreloadDepth depth, int limit, Expression<Func<T, bool>> whereExpression, params SortExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            List<PropertyExpression<T>> properties = new List<PropertyExpression<T>>();
            if (sortExpressions == null || sortExpressions.Length == 0)
            {
                properties.Add(PropertyExpression<T>.Empty);
            }
            else
            {
                foreach (SortExpression<T> item in sortExpressions)
                {
                    PropertyExpression<T> property = new PropertyExpressionConverter<T>().Convert(item.Expression);
                    if (item.Direction == SortDirection.Desc)
                    {
                        property.Desc();
                    }
                    properties.Add(property);
                }
            }
            return this.Select<T>(depth, limit, new PropertyExpressionConverter<T>().Convert(whereExpression), properties.ToArray());
        }

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage using the criteria as specified by whereExpression,
        /// sorted by sortExpressions.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="options">A <see cref="PreloadOptions{T}"/> specifying additional related objects to retrieve.</param>
        /// <param name="whereExpression">An <see cref="Expression"/> that serves as the filter criteria used to query the storage.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="SortExpression{T}"/> used to qualify the properties and sorting directions.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var products = this.Context.Select<Product>(PreloadOptions<Product>.LoadWith(p => p.ProductProductPhotos), p => p.ProductID > 900, SortExpression<Product>.Sort(p => p.ProductID));
        ///     foreach (var product in products)
        ///     {
        ///         Console.WriteLine(product.ProductProductPhotos.Count);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(PreloadOptions<T> options, Expression<Func<T, bool>> whereExpression, params SortExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            Stopwatch timer = StartTimer();
            IEnumerable<T> source = this.Select<T>(whereExpression, sortExpressions).ToFlyweightSet();
            ProcessDataRelations<T>(source, options);
            StopTimer(timer);
            return source;
        }

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage using the criteria as specified by whereExpression,
        /// sorted by sortExpressions.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="options">A <see cref="PreloadOptions{T}"/> specifying additional related objects to retrieve.</param>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
        /// <param name="whereExpression">An <see cref="Expression"/> that serves as the filter criteria used to query the storage.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="SortExpression{T}"/> used to qualify the properties and sorting directions.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     var products = this.Context.Select<Product>(true, 10, PreloadOptions<Product>.LoadWith(p => p.ProductProductPhotos), p => p.ProductID > 900, SortExpression<Product>.Sort(p => p.ProductID));
        ///     foreach (var product in products)
        ///     {
        ///         Console.WriteLine(product.ProductProductPhotos.Count);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(PreloadOptions<T> options, int limit, Expression<Func<T, bool>> whereExpression, params SortExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            Stopwatch timer = StartTimer();
            IEnumerable<T> source = this.Select<T>(limit, whereExpression, sortExpressions).ToFlyweightSet();
            ProcessDataRelations<T>(source, options);
            StopTimer(timer);
            return source;
        }

        private IStorageProvider GetStorageProvider(string connectionString, StorageProviderType providerType)
        {
            IStorageProvider provider = null;
            switch (providerType)
            {
                case StorageProviderType.SqlServer:
                    provider = new SqlServerStorageProvider(ParseProviderToken(connectionString));
                    break;
                case StorageProviderType.SqlServerCe:
                    provider = new SqlServerCeStorageProvider(ParseProviderToken(connectionString));
                    break;
                case StorageProviderType.MsJet:
                    provider = new MsJetStorageProvider(connectionString);
                    break;
                case StorageProviderType.Oracle:
                    provider = new OracleStorageProvider(ParseProviderToken(connectionString));
                    break;
                case StorageProviderType.MySql:
                    provider = new MySqlStorageProvider(ParseProviderToken(connectionString));
                    break;
                default:
                    throw new NotSupportedException(ErrorStrings.ProviderNotSupportedException);
            }
            return provider;
        }

        private string ParseProviderToken(string connectionString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string item in connectionString.Split(";".ToCharArray()))
            {
                if (!item.Replace(" ", string.Empty).Contains("Provider="))
                {
                    sb.AppendFormat("{0};", item);
                }
            }
            return sb.ToString();
        }

        private void ValidateType<T>()
        {
            if (typeof(T).IsEnumerable())
            {
                if (!(typeof(T).IsGenericList() || typeof(T).IsGenericListDerived()))
                {
                    throw new ArgumentException(ErrorStrings.InvalidIEnumerableIListException);
                }
                else if (typeof(T).IsGenericList())
                {
                    Type containedType = typeof(T).GetGenericArguments()[0];
                    if (containedType.GetDefaultConstructor() == null)
                    {
                        throw new ArgumentException(ErrorStrings.MissingDefaultConstructorException);
                    }
                }
            }
        }

        private void ValidateOperation<T>(TransactionType transactionType)
        {
            Type type = typeof(T).GetGenericTypeParameter();
            if (typeof(T).IsGenericListDerived())
            {
                type = typeof(T).BaseType.GetGenericTypeParameter();
            }
            DataTableAttribute dataTable = DataAttributeUtilities.GetDataTableAttribute(type);
            switch (transactionType)
            {
                case TransactionType.Select:
                    if (!dataTable.AllowSelect)
                    {
                        throw new InvalidOperationException(string.Format("Cannot perform the requested operation because the {0}.AllowSelect property for the type {1} is set to false.", typeof(DataTableAttribute).Name, typeof(T).Name));
                    }
                    break;
                case TransactionType.Insert:
                    if (!dataTable.AllowInsert)
                    {
                        throw new InvalidOperationException(string.Format("Cannot perform the requested operation because the {0}.AllowInsert property for the type {1} is set to false.", typeof(DataTableAttribute).Name, typeof(T).Name));
                    }
                    break;
                case TransactionType.Update:
                    if (!dataTable.AllowUpdate)
                    {
                        throw new InvalidOperationException(string.Format("Cannot perform the requested operation because the {0}.AllowUpdate property for the type {1} is set to false.", typeof(DataTableAttribute).Name, typeof(T).Name));
                    }
                    break;
                case TransactionType.Delete:
                    if (!dataTable.AllowDelete)
                    {
                        throw new InvalidOperationException(string.Format("Cannot perform the requested operation because the {0}.AllowDelete property for the type {1} is set to false.", typeof(DataTableAttribute).Name, typeof(T).Name));
                    }
                    break;
                case TransactionType.Truncate:
                    if (!dataTable.AllowTruncate)
                    {
                        throw new InvalidOperationException(string.Format("Cannot perform the requested operation because the {0}.AllowTruncate property for the type {1} is set to false.", typeof(DataTableAttribute).Name, typeof(T).Name));
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Starts a new timer used to calculate response times and resets the last ExecutionTime value.
        /// </summary>
        protected Stopwatch StartTimer()
        {
            _executionTime = TimeSpan.Zero;
            return Stopwatch.StartNew();
        }

        /// <summary>
        /// Stops the supplied and sets the current ExecutionTime. 
        /// </summary>
        protected void StopTimer(Stopwatch timer)
        {
            timer.Stop();
            _executionTime = new TimeSpan(0, 0, 0, 0, (int)timer.ElapsedMilliseconds);
        }

        /// <summary>
        /// Sets the <see cref="ResponseMessage"/> for an object that implements <see cref="IResponseMessage"/>.
        /// </summary>
        protected void SetResponseMessage<T>(IResponseMessage source) where T : class, IFlyweight, new()
        {
            if (source == null)
            {
                return;
            }
            if (source.Response == null)
            {
                source.Response = new ResponseMessage();
            }
            source.Response.ServerName = this.MachineName;
            source.Response.ExecutionTime = this.ExecutionTime;
            source.Response.StatusCode = StatusCode.Success;
        }

        /// <summary>
        /// Sets the <see cref="ResponseMessage"/> for an object that implements <see cref="IResponseMessage"/>.
        /// </summary>
        protected void SetResponseMessage<T>(T source) where T : class, IFlyweight, new()
        {
            if (source == null)
            {
                return;
            }
            IResponseMessage obj = source as IResponseMessage;
            if (obj != null)
            {
                obj.Response.ServerName = this.MachineName;
                obj.Response.ExecutionTime = this.ExecutionTime;
                obj.Response.StatusCode = StatusCode.Success;
            }
        }

        /// <summary>
        /// Sets the <see cref="ResponseMessage"/> for an object that implements <see cref="IResponseMessage"/>.
        /// </summary>
        protected void SetResponseMessage<T>(T source, Exception e) where T : class, IFlyweight, new()
        {
            if (source == null)
            {
                return;
            }
            IResponseMessage obj = source as IResponseMessage;
            if (obj != null)
            {
                obj.Response.ServerName = this.MachineName;
                obj.Response.StatusCode = StatusCode.Failure;
                obj.Response.Exception = e;
            }
        }

        private void ClearDeletedItems<T>(IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            IChangeTrackable<T> list = source as IChangeTrackable<T>;
            if (list != null)
            {
                list.DeletedItems.Clear();
            }
        }

        private bool TryGetCachedObject<T>(IStorageCommand command, out string key, out IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            source = default(IEnumerable<T>);
            key = null;
            if (this.CacheManager.Enabled && typeof(T).GetGenericTypeParameter().IsCacheable())
            {
                key = this.CacheManager.BuildCacheKey<T>(command);
                lock (this.CacheManager)
                {
                    if (this.CacheManager.Contains(key))
                    {
                        source = this.CacheManager.GetObject<T>(key) as IEnumerable<T>;
                        IResponseMessage message = source as IResponseMessage;
                        if (message != null)
                        {
                            message.Response.StorageFacility = StorageFacility.CacheManager;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TryGetCachedObject<T>(PreloadDepth depth, IStorageCommand command, out string key, out IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            source = default(IEnumerable<T>);
            key = null;
            if (this.CacheManager.Enabled && typeof(T).GetGenericTypeParameter().IsCacheable())
            {
                key = this.CacheManager.BuildCacheKey<T>(depth, command);
                lock (this.CacheManager)
                {
                    if (this.CacheManager.Contains(key))
                    {
                        source = this.CacheManager.GetObject<T>(key) as IEnumerable<T>;
                        IResponseMessage message = source as IResponseMessage;
                        if (message != null)
                        {
                            message.Response.StorageFacility = StorageFacility.CacheManager;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TryGetCachedObject<T>(PreloadOptions<T> options, IStorageCommand command, out string key, out IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            source = default(IEnumerable<T>);
            key = null;
            if (this.CacheManager.Enabled && typeof(T).GetGenericTypeParameter().IsCacheable())
            {
                key = this.CacheManager.BuildCacheKey<T>(options, command);
                lock (this.CacheManager)
                {
                    if (this.CacheManager.Contains(key))
                    {
                        source = this.CacheManager.GetObject<T>(key) as IEnumerable<T>;
                        IResponseMessage message = source as IResponseMessage;
                        if (message != null)
                        {
                            message.Response.StorageFacility = StorageFacility.CacheManager;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private void SetCachedObject<T>(string key, IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            if (this.CacheManager.Enabled && !string.IsNullOrEmpty(key))
            {
                int cacheTimeOut = 0;
                if (typeof(T).GetGenericTypeParameter().IsCacheable(out cacheTimeOut))
                {
                    lock (this.CacheManager)
                    {
                        if (!this.CacheManager.Contains(key))
                        {
                            this.CacheManager.AddObject<T>(key, cacheTimeOut, source);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Automatically interates a collection if T is an <see cref="IEnumerable"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the source.</typeparam>
        /// <param name="source">An instance of T.</param>
        /// <param name="targetMethod">The target <see cref="MethodBase"/> to invoke.</param>
        protected internal T InvokeListMethod<T>(T source, MethodBase targetMethod)
        {
            if (!typeof(T).IsEnumerable())
            {
                throw new InvalidOperationException(ErrorStrings.IEnumerableNotImplementedException);
            }
            MethodInfo methodInfo = this.GetType().GetMethods().Where<MethodInfo>(x => x.ToString() == targetMethod.ToString()).SingleOrDefault<MethodInfo>();
            if (methodInfo == null)
            {
                throw new InvalidOperationException(string.Format("The method {0} cannot be invoked because it does not exist in the {1}.", targetMethod.Name, this.GetType().Name));
            }
            Type parameterType = typeof(T).GetGenericArguments()[0];
            if (methodInfo != null && methodInfo.IsGenericMethodDefinition)
            {
                methodInfo = methodInfo.MakeGenericMethod(new Type[] { parameterType });
            }
            IList list = source as IList;
            for (int i = 0; i < list.Count; i++)
            {
                if (methodInfo.ReturnType != null)
                {
                    list[i] = methodInfo.Invoke(this, new object[] { list[i] });
                }
                else
                {
                    methodInfo.Invoke(this, new object[] { list[i] });
                }
            }
            return source;
        }

        private IEnumerable<T> AutoCloneSource<T>(FlyweightSet<T> source) where T : class, IFlyweight, new()
        {
            if (_autoCloneObjects && source != null)
            {
                source = ObjectCloner.Clone(source) as FlyweightSet<T>;
            }
            return source;
        }

        private T AutoCloneObject<T>(T source) where T : class, IFlyweight, new()
        {
            if (_autoCloneObjects && source != null)
            {
                source = ObjectCloner.Clone<T>(source);
            }
            return source;
        }

        private void CheckConcurrency<T>(T source) where T : class, IFlyweight, new()
        {
            if (source != null && !this.ConcurrencyManager.IsCurrent<T>(source))
            {
                throw new ConcurrencyException(source);
            }
        }

        private void ProcessDataRelations<T>(IEnumerable<T> source, PreloadOptions<T> options) where T : class, IFlyweight, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            bool flag = this.LoadOnDemand;
            try
            {
                foreach (T local in source)
                {
                    foreach (PropertyInfo info in options.Properties)
                    {
                        this._isPreloading = true;
                        this.LoadOnDemand = true;
                        info.GetValue(local, null);
                    }
                }
            }
            finally
            {
                this.LoadOnDemand = flag;
                this._isPreloading = false;
            }
        }

        private void ProcessDataRelations<T>(T source, PreloadDepth depth)
        {
            if (source != null && depth > PreloadDepth.FlatObject && !_isPreloading)
            {
                this.ProcessDataRelationsCore(source, (int)depth);
            }
        }

        private void ProcessDataRelationsCore(object source, int depth)
        {
            if (source == null)
            {
                return;
            }
            Type type = source.GetType();
            if (type.IsEnumerable())
            {
                foreach (object item in (IList)source)
                {
                    ProcessDataRelationsCore(item, depth);
                }
            }
            bool loadOnDemand = this.LoadOnDemand;
            try
            {
                foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    DataRelationAttribute[] FlyweightAssociations = DataAttributeUtilities.GetDataRelationAttributes(propertyInfo);
                    if (FlyweightAssociations.Length > 0 && FlyweightAssociations[0].AllowPreload)
                    {
						MethodInfo methodInfo = propertyInfo.GetGetMethod(true);
						_isPreloading = true;
						this.LoadOnDemand = true;
						object child = methodInfo.Invoke(source, null);
                        int newDepth = depth - 1;
                        if (newDepth > 0)
                        {
                            ProcessDataRelationsCore(child, newDepth);
                        }
                    }
                }
            }
            finally
            {
                this.LoadOnDemand = loadOnDemand;
                _isPreloading = false;
            }
        }

        private void ValidateObject<T>(TransactionType transactionType, T source)
        {
            if (source == null)
            {
                return;
            }
            if (typeof(IValidatable).IsAssignableFrom(typeof(T).GetGenericTypeParameter()) && typeof(T).IsEnumerable())
            {
                foreach (var item in (IEnumerable)source)
                {
                    ValidateObject(transactionType, item);
                }
            }
            IValidatable obj = source as IValidatable;
            if (obj != null)
            {
                string message = string.Empty;
                if (!obj.TryValidate(this, transactionType, out message))
                {
                    throw new ValidationException(message);
                }
            }
        }

        private void ResetChecksum<T>(T source)
        {
            if (source == null)
            {
                return;
            }
            if (typeof(IRedundancyCheck).IsAssignableFrom(typeof(T).GetGenericTypeParameter()) && typeof(T).IsEnumerable())
            {
                foreach (var item in (IEnumerable)source)
                {
                    ResetChecksum(item);
                }
            }
            IRedundancyCheck obj = source as IRedundancyCheck;
            if (obj != null)
            {
                obj.Checksum = string.Empty;
            }
        }

        private void ResetChangedProperties<T>(T source)
        {
            if (source == null)
            {
                return;
            }
            if (typeof(IPropertyChangedTrackable).IsAssignableFrom(typeof(T).GetGenericTypeParameter()) && typeof(T).IsEnumerable())
            {
                foreach (var item in (IEnumerable)source)
                {
                    ResetChangedProperties(item);
                }
            }
            IPropertyChangedTrackable obj = source as IPropertyChangedTrackable;
            if (obj != null)
            {
                obj.ChangedProperties.Clear();
            }
        }

        /// <summary>
        /// Called prior to a select operation.
        /// </summary>
        /// <typeparam name="T">The type to be selected.</typeparam>
        /// <param name="e">The <see cref="DataOperationEventArgs{T}"/> for the operation.</param>
        protected virtual void OnSelecting<T>(DataOperationEventArgs<T> e) where T : class, IFlyweight, new()
        {
            if (this.Selecting != null)
            {
                this.Selecting(this, e);
            }
        }

        /// <summary>
        /// Called prior to performing an insert operation.
        /// </summary>
        /// <typeparam name="T">The type to be inserted.</typeparam>
        /// <param name="e">The <see cref="DataOperationEventArgs{T}"/> for the operation.</param>
        protected virtual void OnInserting<T>(DataOperationEventArgs<T> e) where T : class, IFlyweight, new()
        {
            if (this.Inserting != null)
            {
                this.Inserting(this, e);
            }
        }

        /// <summary>
        /// Called prior to performing an update operation.
        /// </summary>
        /// <typeparam name="T">The type to be updated.</typeparam>
        /// <param name="e">The <see cref="DataOperationEventArgs{T}"/> for the operation.</param>
        protected virtual void OnUpdating<T>(DataOperationEventArgs<T> e) where T : class, IFlyweight, new()
        {
            if (this.Updating != null)
            {
                this.Updating(this, e);
            }
        }

        /// <summary>
        /// Called prior to performing a delete operation.
        /// </summary>
        /// <typeparam name="T">The type to be deleted.</typeparam>
        /// <param name="e">The <see cref="DataOperationEventArgs{T}"/> for the operation.</param>
        protected virtual void OnDeleting<T>(DataOperationEventArgs<T> e) where T : class, IFlyweight, new()
        {
            if (this.Deleting != null)
            {
                this.Deleting(this, e);
            }
        }

        /// <summary>
        /// Called prior to performing a truncate operation.
        /// </summary>
        /// <typeparam name="T">The type to be truncated.</typeparam>
        /// <param name="e">The <see cref="DataOperationEventArgs{T}"/> for the operation.</param>
        protected virtual void OnTruncating<T>(DataOperationEventArgs<T> e) where T : class, IFlyweight, new()
        {
            if (this.Truncating != null)
            {
                this.Truncating(this, e);
            }
        }

        /// <summary>
        /// Called prior to performing an Execute operation.
        /// </summary>
        /// <param name="e">The <see cref="DataOperationEventArgs"/> for the operation.</param>
        protected virtual void OnExecuting(DataOperationEventArgs e)
        {
            if (this.Executing != null)
            {
                this.Executing(this, e);
            }
        }

        /// <summary>
        /// Disposes of the object.
        /// </summary>
        public void Dispose()
        {
			this.LoadOnDemand = false;
			this.StorageProvider.Dispose();
        }
    }
}