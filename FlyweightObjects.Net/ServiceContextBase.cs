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
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
using System.Threading;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Security;

namespace FlyweightObjects
{
    /// <summary>
	/// Represents a base class implementation for server side contexts that can be used to process requests.
	/// </summary>
	public abstract class ServiceContextBase : MarshalByRefObject, IServiceContext
	{
        private string _domainName = string.Empty;
        private Type _defaultLogicType = null;
		private bool _compressPayloads = false;
		private bool _encryptPayloads = false;
		private IDataContext _context = null;
		private byte[] _encryptionKey = null;
		private readonly object _syncRoot = new object();

        /// <summary>
		/// Gets or sets the unique name of the domain which identifies the context with specific object types. Parity must exist between
		/// the value of this property and objects with a <see cref="DataTableAttribute"/>.DomainName in order for deferred loading to work correctly
		/// acrosss multiple domains.
		/// </summary>
		public string DomainName
		{
			get { return _domainName; }
			set { _domainName = value; }
		}

        /// <summary>
        /// Gets the default business logic type for the domain.
        /// </summary>
        public Type DefaultLogicType
        {
            get { return _defaultLogicType; }
        }

		/// <summary>
		/// Gets or sets a boolean value indicating whether message payloads should be compressed.
		/// </summary>
		public bool CompressPayloads
		{
			get { return _compressPayloads; }
			set { _compressPayloads = value; }
		}

		/// <summary>
		/// Gets or sets a boolean value indicating whether message payloads should be encrypted.
		/// </summary>
		public bool EncryptPayloads
		{
			get { return _encryptPayloads; }
			set { _encryptPayloads = value; }
		}

        /// <summary>
        /// Gets or sets the optional 24 byte user defined <see cref="TripleDES.Key"/> to be used when <see cref="EncryptPayloads"/> is set to true.
        /// </summary>
        public byte[] EncryptionKey
        {
            get { return _encryptionKey; }
            set 
            {
                if (value == null || value.Length != 24)
                {
                    throw new ArgumentException(ErrorStrings.InvalidEncryptionKeyLength);
                }
                _encryptionKey = value; 
            }
        }

        /// <summary>
        /// Gets the <see cref="ISecurityPrincipal"/> for the current operation.
        /// </summary>
        public ISecurityPrincipal CurrentPrincipal
        {
            get
            {
                if (Thread.CurrentPrincipal == null || Thread.CurrentPrincipal.Identity == null || !(Thread.CurrentPrincipal is ISecurityPrincipal))
                {
                    Thread.CurrentPrincipal = new SecurityPrincipal(WindowsIdentity.GetCurrent(), new string[0]);
                }
                return Thread.CurrentPrincipal as ISecurityPrincipal;
            }
        }

        /// <summary>
        /// Gets the <see cref="IDataContext"/> for the domain.
        /// </summary>
        protected IDataContext Context
        {
            get { return _context; }
        }

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="connectionString">The connection string to be used to connect to the storage provider.</param>
		/// <param name="providerType">The type of storage to connect.</param>
		public ServiceContextBase(string connectionString, StorageProviderType providerType)
		{
			this.InitializeContext(connectionString, providerType);
		}

		/// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="defaultBusinessLogicType">The default <see cref="BusinessLogicBase{TSource}"/> implementation for the domain.</param>
        public ServiceContextBase(Type defaultBusinessLogicType)
        {
            if (defaultBusinessLogicType == null)
            {
                throw new ArgumentNullException("defaultBusinessLogicType");
            }
            if (defaultBusinessLogicType.BaseType.Name != typeof(BusinessLogicBase<>).Name)
            {
                throw new ArgumentException(string.Format("The supplied type argument {0} must be a subclass of {1}.", defaultBusinessLogicType.Name, typeof(BusinessLogicBase<>).Name), "defaultBusinessLogicType");
            }
            if (defaultBusinessLogicType.IsAbstract)
            {
                throw new ArgumentException(string.Format("The supplied type argument {0} cannot be marked abstract.", defaultBusinessLogicType.Name), "defaultBusinessLogicType");
            }
            if (!defaultBusinessLogicType.HasDefaultConstructor())
            {
                throw new ArgumentException(string.Format("The supplied type argument {0} must have a default constructor.", defaultBusinessLogicType.Name), "defaultBusinessLogicType");
            }
            _defaultLogicType = defaultBusinessLogicType;
			this.InitializeContext(false);
        }

		/// <summary>
		/// Processes a <see cref="DataTransferObject"/> and returns the serialized results.
		/// </summary>
		/// <param name="bytes">A binary serialized instance of a <see cref="DataTransferObject"/>.</param>
		public virtual byte[] ProcessDataTransferObject(byte[] bytes)
		{
			TraceHelper.WriteLine(MethodBase.GetCurrentMethod(), string.Format("[HashCode: {0}, ThreadId: {1}] Processing client request", this.GetHashCode(), Thread.CurrentThread.ManagedThreadId));
			if (bytes == null || bytes.Length == 0)
			{
				return null;
			}
			lock (_syncRoot)
			{
				DataTransferObject dto = null;
				try
				{
					TraceHelper.WriteLine(MethodBase.GetCurrentMethod(), string.Format("[HashCode: {0}, ThreadId: {1}] Deserializing {2}", this.GetHashCode(), Thread.CurrentThread.ManagedThreadId, typeof(DataTransferObject).Name));
					dto = this.DeserializeRequest(bytes);
					if (dto == null)
					{
						throw new Exception(string.Format("A problem was encountered while deserializing the {0} on the host {1}.", typeof(DataTransferObject).FullName, Environment.MachineName));
					}
					if (dto.AssemblyFullName != Assembly.GetExecutingAssembly().FullName)
					{
						throw new InvalidOperationException(string.Format("The calling assembly must be of type {0}. Check the version of the assembly to ensure parity between client and server.", Assembly.GetExecutingAssembly().FullName));
					}
					if (dto.EnforceTransaction)
					{
						((ITransactional)this.Context).BeginTransaction();
					}
					foreach (DataTransferRequest request in dto.Requests.AsEnumerable<DataTransferRequest>())
					{
						TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), string.Format("[HashCode: {0}, ThreadId: {1}] Processing RequestId {2}", this.GetHashCode(), Thread.CurrentThread.ManagedThreadId, request.RequestId));
						Stopwatch timer = new Stopwatch();
						timer.Start();
						try
						{
							MethodInfo methodInfo = Array.Find<MethodInfo>(typeof(IDataContext).GetMethods(), delegate(MethodInfo method)
							{
								return method.ToString() == request.Method.MethodBase.ToString();
							});
							if (methodInfo == null)
							{
								throw new InvalidOperationException(string.Format("Cannot find the method {0} to call for the request id {1}.", methodInfo.Name, request.RequestId));
							}
							if (methodInfo.ContainsGenericParameters)
							{
								Type[] parameterTypes = request.Method.TypeParameters.ToArray();
								methodInfo = methodInfo.MakeGenericMethod(parameterTypes);
							}
							TraceHelper.WriteLine(MethodBase.GetCurrentMethod(), string.Format("[HashCode: {0}, ThreadId: {1}] Invoking method -> {2}", this.GetHashCode(), Thread.CurrentThread.ManagedThreadId, methodInfo.ToString()));
							object[] args = methodInfo.GetParameters().Length > 0 ? request.Method.MethodArguments.ToArray() : null;
							request.ReturnValue = methodInfo.Invoke(this, args);
							request.Method.MethodArguments = args != null ? new ArrayList(args) : new ArrayList();
							request.Response.StatusCode = StatusCode.Success;
                            IResponseMessage message = request.ReturnValue as IResponseMessage;
                            if (message != null)
                            {
                                request.Response = message.Response;
                            }
						}
						catch (Exception e)
						{
							TraceHelper.WriteLine(MethodBase.GetCurrentMethod(), string.Format("[HashCode: {0}, ThreadId: {1}] Exception -> {2}", this.GetHashCode(), Thread.CurrentThread.ManagedThreadId, e.ToString()));
							request.Response.StatusCode = StatusCode.Failure;
							request.Response.Exception = e;
							request.Response.Message = e.InnerException != null ? e.InnerException.Message : e.Message;
							if (dto.EnforceTransaction)
							{
								((ITransactional)_context).RollbackTransaction();
								if (dto.ThrowExceptions)
								{
									throw;
								}
								break;
							}
							if (dto.ThrowExceptions)
							{
								throw;
							}
						}
						finally
						{
							timer.Stop();
							dto.IsProcessed = true;
							request.Response.ServerName = Environment.MachineName;
							request.Response.ExecutionTime = new TimeSpan(timer.ElapsedTicks);
						}
					}
				}
				catch (Exception e)
				{
					TraceHelper.WriteLine(MethodBase.GetCurrentMethod(), string.Format("[HashCode: {0}, ThreadId: {1}] Exception -> {2}", this.GetHashCode(), Thread.CurrentThread.ManagedThreadId, e.ToString()));
					throw;
				}
				finally
				{
					if (dto != null && !dto.HasErrors && dto.EnforceTransaction)
					{
						((ITransactional)this.Context).CommitTransaction(true);
					}
					else
					{
						((ITransactional)this.Context).RollbackTransaction();
					}
					if (this.Context is IDisposable)
					{
						((IDisposable)this.Context).Dispose();
					}
				}
				TraceHelper.WriteLine(MethodBase.GetCurrentMethod(), string.Format("[HashCode: {0}, ThreadId: {1}] Serializing response", this.GetHashCode(), Thread.CurrentThread.ManagedThreadId));
				return this.SerializeResponse(dto);
			}
		}

		private DataTransferObject DeserializeRequest(byte[] bytes)
		{
			if (_compressPayloads)
			{
				bytes = bytes.Decompress();
			}
			if (_encryptPayloads)
			{
                bytes = _encryptionKey != null ? bytes.Decrypt(_encryptionKey) : bytes.Decrypt();
			}
			DataTransferObject dto = BinarySerializer.Deserialize<DataTransferObject>(bytes);
			Thread.CurrentPrincipal = dto.CurrentPrincipal;
            if (!ValidatePrincipal(this.CurrentPrincipal))
            {
                throw new SecurityException("The current principal is not valid for the operation.");
            }
            return dto;
		}

		private byte[] SerializeResponse(DataTransferObject dto)
		{
			byte[] bytes = BinarySerializer.Serialize(dto);
			if (_encryptPayloads)
			{
                bytes = _encryptionKey != null ? bytes.Encrypt(_encryptionKey) : bytes.Encrypt();
			}
			if (_compressPayloads)
			{
				bytes = bytes.Compress();
			}
			return bytes;
		}

        /// <summary>
        /// Returns whether the current <see cref="ISecurityPrincipal"/> is valid for the operation.
        /// </summary>
        /// <param name="principal">The current principal.</param>
		public virtual bool ValidatePrincipal(ISecurityPrincipal principal)
		{
			return true;
		}

		/// <summary>
		/// Creates a new instance of an <see cref="DataContext"/>.
		/// </summary>
		/// <param name="connectionString">The connection string for the StorageProvider.</param>
		/// <param name="providerType">The StorageProviderType enumerated value.</param>
		protected void InitializeContext(string connectionString, StorageProviderType providerType)
		{
			if (connectionString == null)
			{
				throw new ArgumentNullException("connectionString");
			}
			_context = new DataContext(connectionString, providerType);
		}

		/// <summary>
		/// Creates a new instance of a default business logic implementation.
		/// </summary>
		/// <param name="beginTransaction">Determines whether a new transaction should be started.</param>
		protected void InitializeContext(bool beginTransaction)
		{
			_context = BusinessLogicFactory.GetBusinessLogic(this.DefaultLogicType);
			if (beginTransaction)
			{
				((ITransactional)this.Context).BeginTransaction();
			}
		}

        #region IDataContext Members

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// the retrieved object, otherwise it will be its default.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="source">An instance of T.</param>
        public virtual bool TrySelect<T>(out T source) where T : class, IFlyweight, new()
		{
            return this.Context.TrySelect<T>(out source);
		}

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// an IEnumerable of the retrieved objects, otherwise if will be an empty instance of IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
		public virtual bool TrySelect<T>(out IEnumerable<T> source) where T : class, IFlyweight, new()
		{
            return this.Context.TrySelect<T>(out source);
		}

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// the retrieved object, otherwise it will be its default.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="source">An instance of T.</param>
		public virtual bool TrySelect<T>(PropertyExpression<T> whereExpression, out T source) where T : class, IFlyweight, new()
		{
            return this.Context.TrySelect<T>(whereExpression, out source);
		}

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// an IEnumerable of the retrieved objects, otherwise if will be an empty instance of IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> criteria used to query the storage.</param>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
		public virtual bool TrySelect<T>(PropertyExpression<T> whereExpression, out IEnumerable<T> source) where T : class, IFlyweight, new()
		{
            return this.Context.TrySelect<T>(whereExpression, out source);
		}

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// the retrieved object, otherwise it will be its default.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to query the storage.</param>
        /// <param name="source">An instance of T.</param>
		public virtual bool TrySelect<T>(IQueryExpression<T> query, out T source) where T : class, IFlyweight, new()
		{
            return this.Context.TrySelect<T>(query, out source);
		}

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// an IEnumerable of the retrieved objects, otherwise if will be an empty instance of IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to query the storage.</param>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
		public virtual bool TrySelect<T>(IQueryExpression<T> query, out IEnumerable<T> source) where T : class, IFlyweight, new()
		{
            return this.Context.TrySelect<T>(query, out source);
		}

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// the retrieved object, otherwise it will be its default.  Note the value for sql may be any object which implements 
        /// the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> used to query the storage.</param>
        /// <param name="source">An instance of T.</param>
		public virtual bool TrySelect<T>(IStorageCommand command, out T source) where T : class, IFlyweight, new()
		{
            return this.Context.TrySelect<T>(command, out source);
		}

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be an 
        /// IEnumerable of the retrieved objects, otherwise if will be an empty instance of IEnumerable.  Note the value for 
        /// sql may be any object which implements the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> used to query the storage.</param>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
		public virtual bool TrySelect<T>(IStorageCommand command, out IEnumerable<T> source) where T : class, IFlyweight, new()
		{
            return this.Context.TrySelect<T>(command, out source);
		}

        /// <summary>
        /// Returns a boolean value indicating whether the object exists in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="source">An instance of T.</param>
		public virtual bool Exists<T>(T source) where T : class, IFlyweight, new()
		{
            return this.Context.Exists<T>(source);
		}

        /// <summary>
        /// Returns a boolean value indicating whether the object exists in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
		public virtual bool Exists<T>(PropertyExpression<T> whereExpression) where T : class, IFlyweight, new()
		{
            return this.Context.Exists<T>(whereExpression);
		}

        /// <summary>
        /// Returns a boolean value indicating whether the object exists in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to query the storage.</param>
		public virtual bool Exists<T>(IQueryExpression<T> query) where T : class, IFlyweight, new()
		{
            return this.Context.Exists<T>(query);
		}

        /// <summary>
        /// Returns a boolean value indicating whether the object exists in storage.  Note the value for sql may be any 
        /// object which implements the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> used to query the storage.</param>
		public virtual bool Exists<T>(IStorageCommand command) where T : class, IFlyweight, new()
		{
            return this.Context.Exists<T>(command);
		}

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
		public virtual IEnumerable<T> Select<T>() where T : class, IFlyweight, new()
		{
            return this.Context.Select<T>();
		}

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage constrained by the amount as specified
        /// by limit.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
		public virtual IEnumerable<T> Select<T>(int limit) where T : class, IFlyweight, new()
		{
            return this.Context.Select<T>(limit);
		}

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage constrained by the amount as specified
        /// by limit and using the criteria as specified by whereExpression.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
		public virtual IEnumerable<T> Select<T>(int limit, PropertyExpression<T> whereExpression) where T : class, IFlyweight, new()
		{
            return this.Context.Select<T>(limit, whereExpression);
		}

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage using the criteria as specified by whereExpression,
        /// sorted by sortExpressions.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="PropertyExpression{T}"/> by which to sort.</param>
		public virtual IEnumerable<T> Select<T>(PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
		{
            return this.Context.Select<T>(whereExpression, sortExpressions);
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
		public virtual IEnumerable<T> Select<T>(PreloadDepth depth, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
		{
            return this.Context.Select<T>(depth, whereExpression, sortExpressions);
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
		public virtual IEnumerable<T> Select<T>(PreloadOptions<T> options, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            return this.Context.Select<T>(options, whereExpression, sortExpressions);
        }

        /// <summary>
        /// Returns all objects for the specified type T in storage constrained by the amount as specified by limit using the 
        /// criteria as specified by whereExpression, and sorted by sortExpressions.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="PropertyExpression{T}"/> by which to sort.</param>
		public virtual IEnumerable<T> Select<T>(int limit, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
		{
            return this.Context.Select<T>(limit, whereExpression, sortExpressions);
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
		public virtual IEnumerable<T> Select<T>(PreloadDepth depth, int limit, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
		{
            return this.Context.Select<T>(depth, limit, whereExpression, sortExpressions);
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
		public virtual IEnumerable<T> Select<T>(PreloadOptions<T> options, int limit, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            return this.Context.Select<T>(options, limit, whereExpression, sortExpressions);
        }

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IQueryExpression{TSource}"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> by which to retrieve the objects.</param>
		public virtual IEnumerable<T> Select<T>(IQueryExpression<T> query) where T : class, IFlyweight, new()
		{
            return this.Context.Select<T>(query);
		}

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IQueryExpression{TSource}"/>. Additional objects 
        /// will be retrieved in the graph according to their level as specified by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="depth">The depth limit of the object graph.</param>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> by which to retrieve the objects.</param>
		public virtual IEnumerable<T> Select<T>(PreloadDepth depth, IQueryExpression<T> query) where T : class, IFlyweight, new()
		{
            return this.Context.Select<T>(depth, query);
		}

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IQueryExpression{TSource}"/>. Additional objects 
        /// will be retrieved in the graph according to their level as specified by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="options">A <see cref="PreloadOptions{T}"/> specifying additional related objects to retrieve.</param>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> by which to retrieve the objects.</param>
		public virtual IEnumerable<T> Select<T>(PreloadOptions<T> options, IQueryExpression<T> query) where T : class, IFlyweight, new()
        {
            return this.Context.Select<T>(options, query);
        }

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IStorageCommand"/>.  Note the value for 
        /// sql may be any object which implements the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> by which to retrieve the objects.</param>
        public virtual IEnumerable<T> Select<T>(IStorageCommand command) where T : class, IFlyweight, new()
		{
            return this.Context.Select<T>(command);
		}

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IStorageCommand"/>. Additional objects 
        /// will be retrieved in the graph according to their level as specified by depth.  Note the value for sql may be any 
        /// object which implements the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="depth">The depth limit of the object graph.</param>
        /// <param name="command">The <see cref="IStorageCommand"/> by which to retrieve the objects.</param>
		public virtual IEnumerable<T> Select<T>(PreloadDepth depth, IStorageCommand command) where T : class, IFlyweight, new()
		{
            return this.Context.Select<T>(depth, command);
		}

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IQueryExpression{TSource}"/>. Additional objects 
        /// will be retrieved in the graph according to their level as specified by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="options">A <see cref="PreloadOptions{T}"/> specifying additional related objects to retrieve.</param>
        /// <param name="command">The <see cref="IStorageCommand"/> used to query the storage.</param>
		public virtual IEnumerable<T> Select<T>(PreloadOptions<T> options, IStorageCommand command) where T : class, IFlyweight, new()
        {
            return this.Context.Select<T>(options, command);
        }

        /// <summary>
        /// Performs an insert into the target entity with the values as given by the source object's properties.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to insert.</typeparam>
        /// <param name="source">An instance of T.</param>
        public virtual T Insert<T>(T source) where T : class, IFlyweight, new()
		{
            return this.Context.Insert<T>(source);
		}

        /// <summary>
        /// Performs an insert into the target entity using the supplied <see cref="IQueryExpression{TSource}"/>. 
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to insert.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to insert the objects.</param>
        public virtual void Insert<T>(IQueryExpression<T> query) where T : class, IFlyweight, new()
		{
            this.Context.Insert<T>(query);
		}

        /// <summary>
        /// Performs multiple inserts into the target entity using the supplied source.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to insert.</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        public virtual IEnumerable<T> Insert<T>(IEnumerable<T> source) where T : class, IFlyweight, new()
		{
            return this.Context.Insert<T>(source);
		}

        /// <summary>
        /// Performs an update of the target entity with the values as given by the source object's properties.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to update.</typeparam>
        /// <param name="source">An instance of T.</param>
        public virtual T Update<T>(T source) where T : class, IFlyweight, new()
		{
            return this.Context.Update<T>(source);
		}

        /// <summary>
        /// Performs an update of the target entity using the supplied <see cref="IQueryExpression{TSource}"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the object to update.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to insert the object.</param>
        public virtual void Update<T>(IQueryExpression<T> query) where T : class, IFlyweight, new()
		{
            this.Context.Update<T>(query);
		}

        /// <summary>
        /// Performs multiple updates of the target entity using the supplied source.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to update.</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        public virtual IEnumerable<T> Update<T>(IEnumerable<T> source) where T : class, IFlyweight, new()
		{
            return this.Context.Update<T>(source);
		}

        /// <summary>
        /// Performs an update of the target entity with the values as given by the source object's properties.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to update.</typeparam>
        /// <param name="source">An instance of T.</param>
		public virtual T Upsert<T>(T source) where T : class, IFlyweight, new()
        {
            return this.Context.Upsert<T>(source);
		}

        /// <summary>
        /// For each object in source, if the object alrady exists, performs an update of the target entity using 
        /// the supplied source, otherwise an insert is applied.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to either update or insert..</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
		public virtual IEnumerable<T> Upsert<T>(IEnumerable<T> source) where T : class, IFlyweight, new()
		{
            return this.Context.Upsert<T>(source);
		}

        /// <summary>
        /// Deletes the source from storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to delete.</typeparam>
        /// <param name="source">An instance of T.</param>
		public virtual T Delete<T>(T source) where T : class, IFlyweight, new()
		{
            return this.Context.Delete<T>(source);
		}

        /// <summary>
        /// Performs a delete on the target entity given the supplied filter criteria.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to delete.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used as the filter for the delete.</param>
		public virtual void Delete<T>(PropertyExpression<T> whereExpression) where T : class, IFlyweight, new()
		{
            this.Context.Delete<T>(whereExpression);
		}

        /// <summary>
        /// Performs a delete on the target entity using the supplied <see cref="IQueryExpression{TSource}"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the object to update.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to delete the object.</param>
        public virtual void Delete<T>(IQueryExpression<T> query) where T : class, IFlyweight, new()
		{
            this.Context.Delete<T>(query);
		}

        /// <summary>
        /// Deletes all instances contained within source from storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to delete.</typeparam>
        /// <param name="source">An instance of T.</param>
        public virtual IEnumerable<T> Delete<T>(IEnumerable<T> source) where T : class, IFlyweight, new()
		{
            return this.Context.Delete<T>(source);
		}

        /// <summary>
        /// Performs all necessary operations on the supplied <see cref="IFlyweightSet{T}"/> object. Note that all objects which
        /// have been removed from the <see cref="IFlyweightSet{T}"/> will be deleted, while the others will either be inserted
        /// or deleted based upon their status in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of the changed objects to persist.</typeparam>
        /// <param name="source">An instance of an <see cref="FlyweightSet{T}"/>.</param>
		public virtual IEnumerable<T> Persist<T>(FlyweightSet<T> source) where T : class, IFlyweight, new()
        {
            return this.Context.Persist<T>(source);
        }

        /// <summary>
        /// Reloads the source object based upon its identifiers as specified by its <see cref="DataColumnAttribute"/> Identifer properties.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to reload.</typeparam>
        /// <param name="source">An instance of T.</param>
		public virtual T Reload<T>(T source) where T : class, IFlyweight, new()
		{
            return this.Context.Reload<T>(source);
		}

        #endregion
	}
}
