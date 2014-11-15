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
using System.Diagnostics;
using System.Security.Principal;
using System.Reflection;
using System.Threading;
using System.Linq.Expressions;
using System.Collections.Specialized;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents a message envelope which encapsulates one or more requests from the server.
    /// </summary>
    /// <example>
    /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
    /// <code>
    /// <![CDATA[
    /// // Create a new TcpClientContext, specifying exposed type, URL and secure channel
    /// TcpClientContext context = new TcpClientContext(typeof(IServiceContext), @"tcp://localhost:9090/AdventureWorksServiceContext", true))
    /// 
    /// // Create a new DTO and instruct it to perform all requests within the same transactional context
    /// DataTransferObject dto = new DataTransferObject();
    /// dto.EnforceTransaction = true;
    ///
    /// // Create a new select QueryExpression
    /// var qe1 = new QueryExpression<Product>()
    ///
    /// .Select(true, 0)
    /// .From<Product>()
    /// .LeftJoin<ProductDocument>(ProductDocument.Properties.ProductID == Product.Properties.ProductID)
    /// .Where(ProductDocument.Properties.ProductID.IsNull() || ProductDocument.Properties.ModifiedDate > DateTime.Now.AddDays(-365));
    ///
    /// // Call Select on the DTO as opposed to the context. Note that we do not assign a return value as it will always be the type's default.
    /// dto.Select<ProductDocument>(qe1);
    /// 
    /// // Create a new update query
    /// var qe2 = new QueryExpression<Product>()
    ///
    /// .Update<Product>()
    /// .Set<Product>(Product.Properties.ModifiedDate == DateTime.Now)
    /// .Where(Product.Properties.ProductID == 355);
    ///
    /// // Call Update on the DTO as opposed to the context Note that we do not assign a return value as it will always be the type's default.
    /// dto.Update<Product>(qe2);
    ///
    /// // Instruct the context to process the DTO
    /// dto = context.ProcessDeferredRequests(dto);
    ///
    /// // Assign variables from the DTO request queue
    /// var p1 = dto.Requests.Dequeue().ReturnValue as IEnumerable<Product>;
    /// var p2 = dto.Requests.Dequeue().ReturnValue as Product;
    /// ]]>
    /// </code>
    /// </example>
    [Serializable]
    public class DataTransferObject : IDataContext
    {
        private Queue<DataTransferRequest> _requests = new Queue<DataTransferRequest>();
        private string _applicationName = string.Empty;
        private string _domainName = string.Empty;
        private string _assemblyFullName = Assembly.GetExecutingAssembly().FullName;
        private bool _isProcessed = false;
        private bool _enforceTransaction = false;
        private bool _throwExceptions = true;
        private HybridDictionary _properties = new HybridDictionary();
        private ISecurityPrincipal _currentPrincipal = new SecurityPrincipal();

        /// <summary>
        /// Gets a generic Queue of requests.
        /// </summary>
        public Queue<DataTransferRequest> Requests
        {
            get { return _requests; }
        }

        /// <summary>
        /// Gets the application name for the instance of the class.
        /// </summary>
        public string ApplicationName
        {
            get { return _applicationName; }
            internal set { _applicationName = value; }
        }

        /// <summary>
        /// Gets the <see cref="TimeSpan"/> representing the sum of all execution times for the requested operations.
        /// </summary>	
        public TimeSpan ExecutionTime
        {
            get
            {
                TimeSpan value = new TimeSpan();
                foreach (DataTransferRequest request in _requests)
                {
                    value = value.Add(request.Response.ExecutionTime);
                }
                return value.Duration();
            }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating whether all submitted requests should be required to participate
        /// in an enforced transaction.
        /// </summary>
        public bool EnforceTransaction
        {
            get { return _enforceTransaction; }
            set { _enforceTransaction = value; }
        }

        /// <summary>
        /// Gets a serializable dictionary of dynamic properties.
        /// </summary>
        public HybridDictionary Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>		
        public DataTransferObject() { }

        /// <summary>
        /// Gets the full name of current executing <see cref="Assembly"/>.
        /// </summary>
        public string AssemblyFullName
        {
            get { return _assemblyFullName; }
        }

        /// <summary>
        /// Gets a boolean value indicating whether the message has been submitted to the server for processing.
        /// </summary>
        public bool IsProcessed
        {
            get { return _isProcessed; }
            internal set { _isProcessed = value; }
        }

        /// <summary>
        /// Returns a boolean value indicating whether each request has been processed.
        /// </summary>
        public bool IsComplete
        {
            get
            {
                if (!_isProcessed)
                {
                    return false;
                }
                foreach (DataTransferRequest request in _requests)
                {
                    if (!request.IsProcessed)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Returns a boolean value indicating whether any of the requests has an error.
        /// </summary>
        public bool HasErrors
        {
            get
            {
                if (!_isProcessed)
                {
                    return false;
                }
                foreach (DataTransferRequest request in _requests)
                {
                    if (request.Response != null && request.Response.StatusCode == StatusCode.Failure)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Gets or sets whether exceptions should be thrown or caught and placed in the <see cref="ResponseMessage"/>
        /// object contained within the <see cref="DataTransferObject"/>.
        /// </summary>
        public bool ThrowExceptions
        {
            get { return _throwExceptions; }
            set { _throwExceptions = value; }
        }

        #region IDataContext Members

        /// <summary>
        /// Gets or sets the unique name of the domain which identifies the context with specific object types. Parity must exist between
        /// the value of this property and objects with a DataTable.DomainName in order for deferred loading to work correctly
        /// acrosss multiple domains.
        /// </summary>
        public string DomainName
        {
            get { return _domainName; }
            set { _domainName = value; }
        }

        /// <summary>
        /// Gets the <see cref="ISecurityPrincipal"/> representing the extended security context of the user.
        /// </summary>
        public ISecurityPrincipal CurrentPrincipal
        {
            get { return _currentPrincipal; }
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// the retrieved object, otherwise it will be its default.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="source">An instance of T.</param>
        public bool TrySelect<T>(out T source) where T : class, IFlyweight, new()
        {
            source = default(T);
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), source);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(bool);
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// an IEnumerable of the retrieved objects, otherwise if will be an empty instance of IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        public bool TrySelect<T>(out IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            source = default(IEnumerable<T>);
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), source);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(bool);
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// the retrieved object, otherwise it will be its default.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="source">An instance of T.</param>
        public bool TrySelect<T>(PropertyExpression<T> whereExpression, out T source) where T : class, IFlyweight, new()
        {
            source = default(T);
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), whereExpression, source);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(bool);
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// an IEnumerable of the retrieved objects, otherwise if will be an empty instance of IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> criteria used to query the storage.</param>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        public bool TrySelect<T>(PropertyExpression<T> whereExpression, out IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            source = default(IEnumerable<T>);
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), whereExpression, source);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(bool);
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// the retrieved object, otherwise it will be its default.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to query the storage.</param>
        /// <param name="source">An instance of T.</param>
        public bool TrySelect<T>(IQueryExpression<T> query, out T source) where T : class, IFlyweight, new()
        {
            source = default(T);
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), query, source);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(bool);
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// an IEnumerable of the retrieved objects, otherwise if will be an empty instance of IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to query the storage.</param>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        public bool TrySelect<T>(IQueryExpression<T> query, out IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            source = default(IEnumerable<T>);
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), query, source);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(bool);
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// the retrieved object, otherwise it will be its default.  Note the value for sql may be any object which implements 
        /// the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> used to query the storage.</param>
        /// <param name="source">An instance of T.</param>
        public bool TrySelect<T>(IStorageCommand command, out T source) where T : class, IFlyweight, new()
        {
            source = default(T);
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), command, source);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(bool);
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be an 
        /// IEnumerable of the retrieved objects, otherwise if will be an empty instance of IEnumerable.  Note the value for 
        /// sql may be any object which implements the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> used to query the storage.</param>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        public bool TrySelect<T>(IStorageCommand command, out IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            source = default(IEnumerable<T>);
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), command, source);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(bool);
        }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// an IEnumerable of the retrieved objects, otherwise if will be an empty instance of IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="whereExpression">An <see cref="Expression"/> that serves as the filter criteria used to query the storage.</param>
        /// <param name="source">An instance of T.</param>
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
        /// <param name="whereExpression">An <see cref="Expression"/> that serves as the filter criteria used to query the storage.</param>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        public bool TrySelect<T>(Expression<Func<T, bool>> whereExpression, out IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            PropertyExpression<T> whereCondition = new PropertyExpressionConverter<T>().Convert(whereExpression);
            return this.TrySelect<T>(whereCondition, out source);
        }

        /// <summary>
        /// Returns a boolean value indicating whether the object exists in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="source">An instance of T.</param>
        public bool Exists<T>(T source) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), source);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(bool);
        }

        /// <summary>
        /// Returns a boolean value indicating whether the object exists in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        public bool Exists<T>(PropertyExpression<T> whereExpression) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), whereExpression);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(bool);
        }

        /// <summary>
        /// Returns a boolean value indicating whether the object exists in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to query the storage.</param>
        public bool Exists<T>(IQueryExpression<T> query) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), query);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(bool);
        }

        /// <summary>
        /// Returns a boolean value indicating whether the object exists in storage.  Note the value for sql may be any 
        /// object which implements the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> used to query the storage.</param>
        public bool Exists<T>(IStorageCommand command) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), command);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(bool);
        }

        /// <summary>
        /// Returns a boolean value indicating whether the object exists in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="whereExpression">An <see cref="Expression"/> that serves as the filter criteria used to query the storage.</param>
        public bool Exists<T>(Expression<Func<T, bool>> whereExpression) where T : class, IFlyweight, new()
        {
            return this.Exists<T>(new PropertyExpressionConverter<T>().Convert(whereExpression));
        }

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        public IEnumerable<T> Select<T>() where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T));
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
        }

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage constrained by the amount as specified
        /// by limit.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
        public IEnumerable<T> Select<T>(int limit) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), limit);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
        }

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage constrained by the amount as specified
        /// by limit and using the criteria as specified by whereExpression.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        public IEnumerable<T> Select<T>(int limit, PropertyExpression<T> whereExpression) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), limit, whereExpression);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
        }

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage using the criteria as specified by whereExpression,
        /// sorted by sortExpressions.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="PropertyExpression{T}"/> by which to sort.</param>
        public IEnumerable<T> Select<T>(PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), whereExpression);
            PropertyExpression<T>[] sortExprs = sortExpressions;
            method.MethodArguments.Add(sortExpressions);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
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
        public IEnumerable<T> Select<T>(PreloadDepth depth, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), depth, whereExpression);
            PropertyExpression<T>[] sortExprs = sortExpressions;
            method.MethodArguments.Add(sortExpressions);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
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
        public IEnumerable<T> Select<T>(PreloadOptions<T> options, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), options, whereExpression);
            PropertyExpression<T>[] sortExprs = sortExpressions;
            method.MethodArguments.Add(sortExpressions);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
        }

        /// <summary>
        /// Returns all objects for the specified type T in storage constrained by the amount as specified by limit using the 
        /// criteria as specified by whereExpression, and sorted by sortExpressions.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="PropertyExpression{T}"/> by which to sort.</param>
        public IEnumerable<T> Select<T>(int limit, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), limit, whereExpression);
            PropertyExpression<T>[] sortExprs = sortExpressions;
            method.MethodArguments.Add(sortExpressions);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
        }

        /// <summary>
        /// Returns all objects for the specified type T in storage constrained by the amount as specified by limit using the 
        /// criteria as specified by whereExpression, and sorted by sortExpressions. Additional objects will be retrieved in the graph 
        /// according to their level as specified by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="depth">The depth of the object graph to prefectch.</param>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="PropertyExpression{T}"/> by which to sort.</param>
        public IEnumerable<T> Select<T>(PreloadDepth depth, int limit, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), depth, limit, whereExpression);
            PropertyExpression<T>[] sortExprs = sortExpressions;
            method.MethodArguments.Add(sortExpressions);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
        }

        /// <summary>
        /// Returns all objects for the specified type T in storage constrained by the amount as specified by limit using the 
        /// criteria as specified by whereExpression, and sorted by sortExpressions. Additional objects will be retrieved in the graph 
        /// according to their level as specified by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="options">A <see cref="PreloadOptions{T}"/> specifying additional related objects to retrieve.</param>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="PropertyExpression{T}"/> by which to sort.</param>
        public IEnumerable<T> Select<T>(PreloadOptions<T> options, int limit, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), options, limit, whereExpression);
            PropertyExpression<T>[] sortExprs = sortExpressions;
            method.MethodArguments.Add(sortExpressions);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
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
        public IEnumerable<T> Select<T>(PreloadOptions<T> options, int limit, Expression<Func<T, bool>> whereExpression, params SortExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), options, limit, whereExpression);
            SortExpression<T>[] sortExprs = sortExpressions;
            method.MethodArguments.Add(sortExpressions);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
        }

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IQueryExpression{TSource}"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> by which to retrieve the objects.</param>
        public IEnumerable<T> Select<T>(IQueryExpression<T> query) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), query);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
        }

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IQueryExpression{TSource}"/>. Additional objects 
        /// will be retrieved in the graph according to their level as specified by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="depth">The depth limit of the object graph.</param>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> by which to retrieve the objects.</param>
        public IEnumerable<T> Select<T>(PreloadDepth depth, IQueryExpression<T> query) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), depth, query);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
        }

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IQueryExpression{TSource}"/>. Additional objects 
        /// will be retrieved in the graph according to their level as specified by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="options">A <see cref="PreloadOptions{T}"/> specifying additional related objects to retrieve.</param>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> by which to retrieve the objects.</param>
        public IEnumerable<T> Select<T>(PreloadOptions<T> options, IQueryExpression<T> query) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), options, query);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
        }

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IStorageCommand"/>.  Note the value for 
        /// sql may be any object which implements the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> by which to retrieve the objects.</param>
        public IEnumerable<T> Select<T>(IStorageCommand command) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), command);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
        }

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IStorageCommand"/>. Additional objects 
        /// will be retrieved in the graph according to their level as specified by depth.  Note the value for sql may be any 
        /// object which implements the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="depth">The depth limit of the object graph.</param>
        /// <param name="command">The <see cref="IStorageCommand"/> by which to retrieve the objects.</param>
        public IEnumerable<T> Select<T>(PreloadDepth depth, IStorageCommand command) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), depth, command);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
        }

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IQueryExpression{TSource}"/>. Additional objects 
        /// will be retrieved in the graph according to their level as specified by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="options">A <see cref="PreloadOptions{T}"/> specifying additional related objects to retrieve.</param>
        /// <param name="command">The <see cref="IStorageCommand"/> used to query the storage.</param>
        public IEnumerable<T> Select<T>(PreloadOptions<T> options, IStorageCommand command) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), options, command);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
        }

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage using the criteria as specified by whereExpression.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="whereExpression">An <see cref="Expression"/> that serves as the filter criteria used to query the storage.</param>
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
        /// <param name="whereExpression">An <see cref="Expression"/> that serves as the filter criteria used to query the storage.</param>
        public IEnumerable<T> Select<T>(int limit, Expression<Func<T, bool>> whereExpression) where T : class, IFlyweight, new()
        {
            return this.Select<T>(limit, new PropertyExpressionConverter<T>().Convert(whereExpression));
        }

        /// <summary>
        /// Performs an insert into the target entity with the values as given by the source object's properties.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to insert.</typeparam>
        /// <param name="source">An instance of T.</param>
        public T Insert<T>(T source) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), source);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(T);
        }

        /// <summary>
        /// Performs an insert into the target entity using the supplied <see cref="IQueryExpression{TSource}"/>. 
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to insert.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to insert the objects.</param>
        public void Insert<T>(IQueryExpression<T> query) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), query);
            this.Requests.Enqueue(new DataTransferRequest(method));
        }

        /// <summary>
        /// Performs multiple inserts into the target entity using the supplied source.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to insert.</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        public IEnumerable<T> Insert<T>(IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), source);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
        }

        /// <summary>
        /// Performs an update of the target entity with the values as given by the source object's properties.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to update.</typeparam>
        /// <param name="source">An instance of T.</param>
        public T Update<T>(T source) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), source);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(T);
        }

        /// <summary>
        /// Performs an update of the target entity using the supplied <see cref="IQueryExpression{TSource}"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the object to update.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to insert the object.</param>
        public void Update<T>(IQueryExpression<T> query) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), query);
            this.Requests.Enqueue(new DataTransferRequest(method));
        }

        /// <summary>
        /// Performs multiple updates of the target entity using the supplied source.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to update.</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        public IEnumerable<T> Update<T>(IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), source);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
        }

        /// <summary>
        /// If the object alrady exists, performs an update of the target entity using the supplied source, otherwise
        /// an insert is applied.
        /// </summary>
        /// <typeparam name="T">The type parameter of the object to either update or insert.</typeparam>
        /// <param name="source">An instance of T.</param>
        public T Upsert<T>(T source) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), source);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(T);
        }

        /// <summary>
        /// For each object in source, if the object alrady exists, performs an update of the target entity using 
        /// the supplied source, otherwise an insert is applied.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to either update or insert..</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        public IEnumerable<T> Upsert<T>(IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), source);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
        }

        /// <summary>
        /// Deletes the source from storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to delete.</typeparam>
        /// <param name="source">An instance of T.</param>
        public T Delete<T>(T source) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), source);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(T);
        }

        /// <summary>
        /// Performs a delete on the target entity given the supplied filter criteria.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to delete.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used as the filter for the delete.</param>
        public void Delete<T>(PropertyExpression<T> whereExpression) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), whereExpression);
            this.Requests.Enqueue(new DataTransferRequest(method));
        }

        /// <summary>
        /// Performs a delete on the target entity given the supplied filter criteria.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to delete.</typeparam>
        /// <param name="whereExpression">An <see cref="Expression"/> that serves as the filter criteria used to query the storage.</param>
        public void Delete<T>(Expression<Func<T, bool>> whereExpression) where T : class, IFlyweight, new()
        {
            this.Delete<T>(new PropertyExpressionConverter<T>().Convert(whereExpression));
        }

        /// <summary>
        /// Performs a delete on the target entity using the supplied <see cref="IQueryExpression{TSource}"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the object to update.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to delete the object.</param>
        public void Delete<T>(IQueryExpression<T> query) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), query);
            this.Requests.Enqueue(new DataTransferRequest(method));
        }

        /// <summary>
        /// Deletes all instances contained within source from storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to delete.</typeparam>
        /// <param name="source">An instance of T.</param>
        public IEnumerable<T> Delete<T>(IEnumerable<T> source) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), source);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
        }

        /// <summary>
        /// Performs all necessary operations on the supplied <see cref="IFlyweightSet{T}"/> object. Note that all objects which
        /// have been removed from the <see cref="IFlyweightSet{T}"/> will be deleted, while the others will either be inserted
        /// or updated based upon whether or not they exist in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of the changed objects to persist.</typeparam>
        /// <param name="source">An instance of a <see cref="FlyweightSet{T}"/>.</param>
        public IEnumerable<T> Persist<T>(FlyweightSet<T> source) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), source);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(IEnumerable<T>);
        }

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage sorted by sortExpression.
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
        /// sorted by sortExpression.
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
        ///     foreach (var p in products)
        ///     {
        ///         Console.WriteLine(p.ProductProductPhotos.Count);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public IEnumerable<T> Select<T>(PreloadOptions<T> options, Expression<Func<T, bool>> whereExpression, params SortExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
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
            return this.Select<T>(options, new PropertyExpressionConverter<T>().Convert(whereExpression), properties.ToArray());
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
        /// Reloads the source object based upon its identifiers as specified by its <see cref="DataColumnAttribute"/> Identifer properties.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to reload.</typeparam>
        /// <param name="source">An instance of T.</param>
        public T Reload<T>(T source) where T : class, IFlyweight, new()
        {
            RuntimeMethod method = new RuntimeMethod(MethodInfo.GetCurrentMethod(), typeof(T), source);
            this.Requests.Enqueue(new DataTransferRequest(method));
            return default(T);
        }

        #endregion
    }
}
