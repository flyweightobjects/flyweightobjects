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
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting;
using System.Security.Permissions;
using System.Collections;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;

namespace FlyweightObjects
{
	/// <summary>
	/// Specifies a request mode for interactions with a service.
	/// </summary>
	public enum RequestMode
	{
		/// <summary>
		/// Instructs the context to perform the operation immediately.
		/// </summary>
		NonDeferred,
		/// <summary>
		/// Instructs the context to queue all operations until ProcessDeferredRequests is called.
		/// </summary>
		Deferred
	}

    /// <summary>
	/// Represents the abstract base class for all remoting clients.
	/// </summary>
	public abstract class ClientContextBase : IDataContext, ILoadOnDemand
	{
        /// <summary>
        /// Called prior to processing a <see cref="DataTransferObject"/>.
        /// </summary>
        public event EventHandler<DataTransferEventArgs> Processing;
        
        private DataTransferObject _dto = null;
		private string _url = string.Empty;
		private string _domainName = string.Empty;
		private IServiceContext _service = null;
		private IChannelSender _clientChannel = null;
		private IMessageSink _messageSink = null;
		private string _applicationName = string.Empty;
		private readonly bool _secureChannel = false;
		private bool _compressPayloads = false;
		private bool _encryptPayloads = false;
        private TimeSpan _executionTime = new TimeSpan();
		private bool _enforceTransaction = false;
        private bool _throwExceptions = true;
        private byte[] _encryptionKey = null;

		/// <summary>
		/// Gets the current service to which the client connects.
		/// </summary>
		public virtual IServiceContext Service
		{
			get { return _service; }
		}

        /// <summary>
		/// Gets the <see cref="IChannelSender"/> instance used by the client context.
		/// </summary>
		protected IChannelSender ChannelSender
		{
			get { return _clientChannel; }
		}

		/// <summary>
		/// Gets the <see cref="IMessageSink"/> used by the client context.
		/// </summary>
		protected IMessageSink MessageSink
		{
			get { return _messageSink; }
		}

		/// <summary>
		/// Gets whether or not to use a secure channel for the service.
		/// </summary>
		public bool SecureChannel
		{
			get { return _secureChannel; }
		}

		/// <summary>
		/// Gets the URL for the remoting service.
		/// </summary>
		public string Url
		{
			get { return _url; }
		}

		/// <summary>
		/// Gets the RequestMode that the context is currently in.
		/// </summary>
		public RequestMode RequestMode
		{
			get { return _dto == null ? RequestMode.NonDeferred : RequestMode.Deferred; }
		}

		/// <summary>
		/// Gets the application name for the instance of the class.
		/// </summary>
		public string ApplicationName
		{
			get { return _applicationName; }
		}

		/// <summary>
		/// Gets or sets whether the payload of a request should be compressed.
		/// </summary>
		public bool CompressPayloads
		{
			get { return _compressPayloads; }
			set { _compressPayloads = value; }
		}

		/// <summary>
		/// Gets or sets whether the payload of a request should be encrypted.
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
        /// Gets or sets the peek size of any returned <see cref="FlyweightSet{T}"/> collections for debugging purposes.
        /// The default value for this property is 1000.
        /// </summary>
        public int MaxDebugListSize
        {
            get { return ThreadLocalStorage.GetMaxDebugListSize(); }
            set { ThreadLocalStorage.SetMaxDebugListSize(value); }
        }

        /// <summary>
        /// Gets the <see cref="TimeSpan"/> representing the execution time of the last operation.
        /// </summary>
        public TimeSpan ExecutionTime
        {
            get { return _executionTime; }
        }

		/// <summary>
		/// Gets or sets a boolean value indicating whether all deferred requests should be required to participate in an enforced transaction.
		/// <remarks>
        /// This property only applies to a <see cref="ClientContextBase"/> derivative when BeginDeferredRequests() has been called.
		/// </remarks>
		/// </summary>
		public bool EnforceTransaction
		{
			get { return _enforceTransaction; }
			set { _enforceTransaction = value; }
		}

        /// <summary>
        /// Gets or sets whether exceptions should be thrown or caught on the server and placed in the <see cref="ResponseMessage"/>
        /// object contained within the <see cref="DataTransferObject"/>.
        /// <remarks>
        /// This property only applies to a <see cref="ClientContextBase"/> derivative when BeginDeferredRequests() has been called.
        /// </remarks>
        /// </summary>
        public bool ThrowExceptions
        {
            get { return _throwExceptions; }
            set { _throwExceptions = value; }
        }

		/// <summary>
		/// Creates a new internally held <see cref="DataTransferObject"/> and stores all requests in this member variable. Not until 
		/// ProcessDeferredRequests is called will the requests actually be processed by the server.
		/// </summary>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// // Create a new TcpClientContext, specifying exposed type, URL and secure channel
        /// var context = new TcpClientContext(typeof(IServiceContext), @"tcp://localhost:9090/AdventureWorksServiceContext", true))
        /// 
        /// // Instruct the context to queue the requests instead of processing them immediately
        /// context.BeginDeferredRequests();
        ///
        /// // Create a new select QueryExpression
        /// var qe1 = new QueryExpression<Product>()
        ///
        /// .Select(true, 0)
        /// .From<Product>()
        /// .LeftJoin<ProductDocument>(ProductDocument.Properties.ProductID == Product.Properties.ProductID)
        /// .Where(ProductDocument.Properties.ProductID.IsNull() || ProductDocument.Properties.ModifiedDate > DateTime.Now.AddDays(-365));
        ///
        /// // Call Select on the context. Note that we do not assign a return value as it will always be the type's default.
        /// context.Select<ProductDocument>(qe1);
        /// 
        /// // Create a new update query
        /// var qe2 = new QueryExpression<Product>()
        ///
        /// .Update()
        /// .Set(Product.Properties.ModifiedDate == DateTime.Now)
        /// .Where(Product.Properties.ProductID == 355);
        ///
        /// // Call Update context Note that we do not assign a return value as it will always be the type's default.
        /// context.Update<Product>(qe2);
        ///
        /// // Instruct the context to process the DTO
        /// var dto = context.ProcessDeferredRequests();
        ///
        /// // Assign variables from the DTO request queue
        /// var p1 = dto.Requests.Dequeue().ReturnValue as IEnumerable<Product>;
        /// var p2 = dto.Requests.Dequeue().ReturnValue as Product;
        /// ]]>
        /// </code>
        /// </example>
		public virtual void BeginDeferredRequests()
		{
			if (_dto != null)
			{
				throw new InvalidOperationException(ErrorStrings.InvalidBeginDeferredRequestsCall);
			}
			_dto = new DataTransferObject();
		}

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

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="remotedType">The MarshalByRefObject type exposed by the remote service.</param>
		/// <param name="channelType">The type of remoting channel to use.</param>
		/// <param name="url">The url of the remote service.</param>
		/// <param name="secureChannel">Determines whether the channel should be secured.</param>
		public ClientContextBase(Type remotedType, ChannelType channelType, string url, bool secureChannel)
            : this(remotedType, channelType, url, secureChannel, false, string.Empty, string.Empty, null)
		{
			
		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="remotedType">The MarshalByRefObject type exposed by the remote service.</param>
		/// <param name="channelType">The type of remoting channel to use.</param>
		/// <param name="url">The url of the remote service.</param>
		/// <param name="secureChannel">Determines whether the channel should be secured.</param>
		/// <param name="properties">An <see cref="IDictionary"/> of properties specific for the client channel.</param>
		public ClientContextBase(Type remotedType, ChannelType channelType, string url, bool secureChannel, IDictionary properties)
            : this(remotedType, channelType, url, secureChannel, false, string.Empty, string.Empty, properties)
		{
			
		}

		/// <summary>
		/// Initializes the channels for remote communication.
		/// </summary>
		/// <param name="remotedType">The MarshalByRefObject type exposed by the remote service.</param>
		/// <param name="channelType">The <see cref="ChannelType"/> used to communicate with the service.</param>
		/// <param name="url">The url of the remote service.</param>
		/// <param name="secureChannel">Determines whether the channel should be secured.</param>
		/// <param name="loadOnDemand">A boolean value indicating whether deferred loading of objects should be enabled.</param>
		/// <param name="domainName">A unique name that identifies a domain for which this instance of a context is responsible.</param>
		/// <param name="applicationName">The name of the application which is making the request.</param>
		/// <param name="properties">An instance of an <see cref="IDictionary"/> representing properties for the client channel.</param>
        protected ClientContextBase(Type remotedType, ChannelType channelType, string url, bool secureChannel, bool loadOnDemand, string domainName, string applicationName, IDictionary properties)
		{
            if (string.IsNullOrEmpty(url.TrimNull()))
            {
                throw new ArgumentException("Invalid service location specification.", "url");
            }

            _url = url.Trim();
            _clientChannel = channelType == ChannelType.Http ? new HttpClientChannel(properties, new BinaryClientFormatterSinkProvider()).AsType<IChannelSender>()
				: new TcpClientChannel(properties, new BinaryClientFormatterSinkProvider()).AsType<IChannelSender>();

			if (ChannelServices.GetChannel(_clientChannel.ChannelName) == null)
			{
				ChannelServices.RegisterChannel(_clientChannel, secureChannel);
			}
			
			if (!string.IsNullOrEmpty(applicationName.TrimNull()))
			{
				_applicationName = applicationName;
				RemotingConfiguration.ApplicationName = applicationName;
			}

			WellKnownClientTypeEntry clientTypeEntry = new WellKnownClientTypeEntry(remotedType, _url);
			if (!Array.Exists<WellKnownClientTypeEntry>(RemotingConfiguration.GetRegisteredWellKnownClientTypes(), client => client.ObjectUrl == clientTypeEntry.ObjectUrl))
			{
				RemotingConfiguration.RegisterWellKnownClientType(clientTypeEntry);
			}
			
			string uri;
			_messageSink = _clientChannel.CreateMessageSink(_url, null, out uri);
			_service = Activator.GetObject(remotedType, _url).AsType<IServiceContext>();

			_domainName = domainName;
			this.LoadOnDemand = loadOnDemand;
		}

		/// <summary>
		/// Serializes tbe request as a byte array.
		/// </summary>
		/// <param name="dto">The <see cref="DataTransferObject"/> message wrapper to serialize.</param>
		private byte[] SerializeRequest(DataTransferObject dto)
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
		/// Deserializes the response from a byte array.
		/// </summary>
		/// <param name="bytes">The binary serialized <see cref="DataTransferObject"/> to deserialize.</param>
		private DataTransferObject DeserializeResponse(byte[] bytes)
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
			return dto;
		}

		/// <summary>
		/// Submits the internal <see cref="DataTransferObject"/> member object to the server for processing.
		/// </summary>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// // Create a new TcpClientContext, specifying exposed type, URL and secure channel
        /// var context = new TcpClientContext(typeof(IServiceContext), @"tcp://localhost:9090/AdventureWorksServiceContext", true))
        /// 
        /// // Instruct the context to queue the requests instead of processing them immediately
        /// context.BeginDeferredRequests();
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
        /// context.Select<ProductDocument>(qe1);
        /// 
        /// // Create a new update query
        /// var qe2 = new QueryExpression<Product>()
        ///
        /// .Update()
        /// .Set(Product.Properties.ModifiedDate == DateTime.Now)
        /// .Where(Product.Properties.ProductID == 355);
        ///
        /// // Call Update on the DTO as opposed to the context Note that we do not assign a return value as it will always be the type's default.
        /// context.Update<Product>(qe2);
        ///
        /// // Instruct the context to process the DTO
        /// var dto = context.ProcessDeferredRequests();
        ///
        /// // Assign variables from the DTO request queue
        /// var p1 = dto.Requests.Dequeue().ReturnValue as IEnumerable<Product>;
        /// var p2 = dto.Requests.Dequeue().ReturnValue as Product;
        /// ]]>
        /// </code>
        /// </example>
		public virtual DataTransferObject ProcessDeferredRequests()
		{
			try
			{
                DataTransferObject dto = this.GetDataTransferObject();
                return dto = ProcessDeferredRequests(dto);
			}
			finally
			{
				_dto = null;
			}
		}

		/// <summary>
		/// Cancels the currently queued operations and puts the context back into a non-deferred state.
		/// </summary>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// // Create a new TcpClientContext, specifying exposed type, URL and secure channel
        /// var context = new TcpClientContext(typeof(IServiceContext), @"tcp://localhost:9090/AdventureWorksServiceContext", true))
        /// 
        /// // Cancel all requests that may be queued
        /// if (context.RequestMode == RequestMode.Deferred)
        /// {
        ///     context.CancelDeferredRequests();
        /// }
        /// ]]>
        /// </code>
        /// </example>
		public virtual void CancelDeferredRequests()
		{			
			_dto = null;
		}

		/// <summary>
		/// Submits an instantiated <see cref="DataTransferObject"/> object to the server for processing.
		/// </summary>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// // Create a new TcpClientContext, specifying exposed type, URL and secure channel
        /// var context = new TcpClientContext(typeof(IServiceContext), @"tcp://localhost:9090/AdventureWorksServiceContext", true))
        /// 
        /// // Create a new DTO and instruct it to perform all requests within the same transactional context
        /// var dto = new DataTransferObject();
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
        /// .Update()
        /// .Set(Product.Properties.ModifiedDate == DateTime.Now)
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
		public virtual DataTransferObject ProcessDeferredRequests(DataTransferObject dto)
		{
			Stopwatch timer = new Stopwatch();
			timer.Start();
			if (dto == null)
			{
				throw new ArgumentNullException("dto");
			}
			if (dto.Requests.Count == 0)
			{
				throw new InvalidOperationException(ErrorStrings.EmptyDataTransferObjectRequestQueue);
			}
            try
            {
                dto.ApplicationName = _applicationName;
                dto.DomainName = _domainName;
                DataTransferEventArgs e = new DataTransferEventArgs(dto);
                OnProcessing(e);
                if (!e.Cancel)
                {
                    byte[] bytes = this.Service.ProcessDataTransferObject(SerializeRequest(dto));
                    return this.DeserializeResponse(bytes);
                }
                return dto;
            }
            catch (Exception e)
            {
                TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), e.ToString());
                throw;
            }
			finally
			{
				timer.Stop();
                _executionTime = new TimeSpan(0, 0, 0, 0, (int)timer.ElapsedMilliseconds);
			}
		}

        /// <summary>
        /// Called prior to processing a <see cref="DataTransferObject"/>.
        /// </summary>
        /// <param name="e">The <see cref="DataTransferEventArgs"/> to be processed.</param>
        protected virtual void OnProcessing(DataTransferEventArgs e)
        {
            if (this.Processing != null)
            {
                Processing(this, e);
            }
        }

		/// <summary>
		/// Returns an instance of a <see cref="DataTransferObject"/> based upon the <see cref="RequestMode"/> of the context.
		/// </summary>
		private DataTransferObject GetDataTransferObject()
		{
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				return new DataTransferObject();
			}
			else
			{
				_dto.EnforceTransaction = _enforceTransaction;
                _dto.ThrowExceptions = _throwExceptions;
				return _dto;
			}
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
		public virtual bool TrySelect<T>(out T source) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.TrySelect<T>(out source);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}				
			DataTransferRequest request = dto.Requests.Peek();
			source = request.Method.MethodArguments[0] as T;
			return (bool)request.ReturnValue;
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
		public virtual bool TrySelect<T>(out IEnumerable<T> source) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();	
			dto.TrySelect<T>(out source);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			DataTransferRequest request = dto.Requests.Peek();
			source = request.Method.MethodArguments[0] as IEnumerable<T>;
			return (bool)request.ReturnValue;
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
		public virtual bool TrySelect<T>(PropertyExpression<T> whereExpression, out T source) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.TrySelect<T>(whereExpression, out source);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			DataTransferRequest request = dto.Requests.Peek();
			source = request.Method.MethodArguments[1] as T;
			return (bool)request.ReturnValue;
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
		public virtual bool TrySelect<T>(Expression<Func<T, bool>> whereExpression, out T source) where T : class, IFlyweight, new()
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
		public virtual bool TrySelect<T>(PropertyExpression<T> whereExpression, out IEnumerable<T> source) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.TrySelect<T>(whereExpression, out source);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			DataTransferRequest request = dto.Requests.Peek();
			source = request.Method.MethodArguments[1] as IEnumerable<T>;
			return (bool)request.ReturnValue;
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
		public virtual bool TrySelect<T>(Expression<Func<T, bool>> whereExpression, out IEnumerable<T> source) where T : class, IFlyweight, new()
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
		public virtual bool TrySelect<T>(IQueryExpression<T> query, out T source) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.TrySelect<T>(query, out source);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			DataTransferRequest request = dto.Requests.Peek();
			source = request.Method.MethodArguments[1] as T;
			return (bool)request.ReturnValue;
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
		public virtual bool TrySelect<T>(IQueryExpression<T> query, out IEnumerable<T> source) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.TrySelect<T>(query, out source);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			DataTransferRequest request = dto.Requests.Peek();
			source = request.Method.MethodArguments[1] as IEnumerable<T>;
			return (bool)request.ReturnValue;
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
		public virtual bool TrySelect<T>(IStorageCommand command, out T source) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.TrySelect<T>(command, out source);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			DataTransferRequest request = dto.Requests.Peek();
			source = request.Method.MethodArguments[1] as T;
			return (bool)request.ReturnValue;
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
		public virtual bool TrySelect<T>(IStorageCommand command, out IEnumerable<T> source) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.TrySelect<T>(command, out source);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			DataTransferRequest request = dto.Requests.Peek();
			source = request.Method.MethodArguments[1] as IEnumerable<T>;
			return (bool)request.ReturnValue;
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
		public virtual bool Exists<T>(T source) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Exists<T>(source);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return (bool)dto.Requests.Peek().ReturnValue;
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
		public virtual bool Exists<T>(PropertyExpression<T> whereExpression) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Exists<T>(whereExpression);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return (bool)dto.Requests.Peek().ReturnValue;
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
		public virtual bool Exists<T>(Expression<Func<T, bool>> whereExpression) where T : class, IFlyweight, new()
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
		public virtual bool Exists<T>(IQueryExpression<T> query) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Exists<T>(query);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return (bool)dto.Requests.Peek().ReturnValue;
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
		public virtual bool Exists<T>(IStorageCommand command) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Exists<T>(command);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return (bool)dto.Requests.Peek().ReturnValue;
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
		public virtual IEnumerable<T> Select<T>() where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Select<T>();
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
		public virtual IEnumerable<T> Select<T>(int limit) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Select<T>(limit);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
        ///     var products = context.Select<Product>(p => p.FinishedGoodsFlag == false || p.ModifiedDate > DateTime.Now.AddYears(-10));
        ///     foreach (Product product in products)
        ///     {
        ///         Console.WriteLine("ProductNumber: {0}", product.ProductNumber);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
		public virtual IEnumerable<T> Select<T>(Expression<Func<T, bool>> whereExpression) where T : class, IFlyweight, new()
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
		public virtual IEnumerable<T> Select<T>(int limit, PropertyExpression<T> whereExpression) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Select<T>(limit, whereExpression);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
		public virtual IEnumerable<T> Select<T>(int limit, Expression<Func<T, bool>> whereExpression) where T : class, IFlyweight, new()
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
        ///     var products = context.Select<Product>(Product.Properties.ModifiedDate > DateTime.Now.AddYears(-10), Product.Properties.ProductNumber.Asc());
        ///     foreach (Product product in products)
        ///     {
        ///         Console.WriteLine("ProductNumber: {0}", product.ProductNumber);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
		public virtual IEnumerable<T> Select<T>(PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Select<T>(whereExpression, sortExpressions);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
		public virtual IEnumerable<T> Select<T>(PreloadDepth depth, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Select<T>(depth, whereExpression, sortExpressions);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
		public virtual IEnumerable<T> Select<T>(int limit, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
            dto.Select<T>(limit, whereExpression, sortExpressions);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
		public virtual IEnumerable<T> Select<T>(PreloadDepth depth, int limit, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
            dto.Select<T>(depth, limit, whereExpression, sortExpressions);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
        ///     foreach (var p in products)
        ///     {
        ///         Console.WriteLine(p.ProductProductPhotos.Count);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
		public virtual IEnumerable<T> Select<T>(PreloadOptions<T> options, int limit, Expression<Func<T, bool>> whereExpression, params SortExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            DataTransferObject dto = this.GetDataTransferObject();
            dto.Select<T>(options, limit, whereExpression, sortExpressions);
            if (this.RequestMode == RequestMode.NonDeferred)
            {
                dto = this.ProcessDeferredRequests(dto);
            }
            return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
            DataTransferObject dto = this.GetDataTransferObject();
            dto.Select<T>(options, limit, whereExpression, sortExpressions);
            if (this.RequestMode == RequestMode.NonDeferred)
            {
                dto = this.ProcessDeferredRequests(dto);
            }
            return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
		public virtual IEnumerable<T> Select<T>(PreloadOptions<T> options, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
        {
            DataTransferObject dto = this.GetDataTransferObject();
            dto.Select<T>(options, whereExpression, sortExpressions);
            if (this.RequestMode == RequestMode.NonDeferred)
            {
                dto = this.ProcessDeferredRequests(dto);
            }
            return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
		public virtual IEnumerable<T> Select<T>(IQueryExpression<T> query) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Select<T>(query);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
		public virtual IEnumerable<T> Select<T>(PreloadDepth depth, IQueryExpression<T> query) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Select<T>(depth, query);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
        ///     .Select(true, 10, ProductCategory.Properties.Name)
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
		public virtual IEnumerable<T> Select<T>(PreloadOptions<T> options, IQueryExpression<T> query) where T : class, IFlyweight, new()
        {
            DataTransferObject dto = this.GetDataTransferObject();
            dto.Select<T>(options, query);
            if (this.RequestMode == RequestMode.NonDeferred)
            {
                dto = this.ProcessDeferredRequests(dto);
            }
            return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Select<T>(command);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
		public virtual IEnumerable<T> Select<T>(PreloadDepth depth, IStorageCommand command) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Select<T>(depth, command);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
		public virtual IEnumerable<T> Select<T>(PreloadOptions<T> options, IStorageCommand command) where T : class, IFlyweight, new()
        {
            DataTransferObject dto = this.GetDataTransferObject();
            dto.Select<T>(options, command);
            if (this.RequestMode == RequestMode.NonDeferred)
            {
                dto = this.ProcessDeferredRequests(dto);
            }
            return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
        ///         var product1 = ObjectCloner.Clone<Product>(product);
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
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Insert<T>(source);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return dto.Requests.Peek().ReturnValue as T;
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
        ///     .Insert(Department.Properties.Name, Department.Properties.GroupName, Department.Properties.ModifiedDate)
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
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Insert<T>(query);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
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
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Insert<T>(source);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Update<T>(source);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return dto.Requests.Peek().ReturnValue as T;
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
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Update<T>(query);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
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
        /// ]]>
        /// </code>
        /// </example>
        public virtual IEnumerable<T> Update<T>(IEnumerable<T> source) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Update<T>(source);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
		public virtual T Upsert<T>(T source) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Upsert<T>(source);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return dto.Requests.Peek().ReturnValue as T;
		}

        /// <summary>
        /// For each object in source, if the object alrady exists, performs an update of the target entity using 
        /// the supplied source, otherwise an insert is applied.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to either update or insert..</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        /// <example>
        /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database.
        /// <code>
        /// <![CDATA[
        /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
        /// {
        ///     // Retrieve a list of products
        ///     var products = context.Select<Product>(Product.Properties.ListPrice > 0).ToList();
        ///
        ///     // Change the ModifiedDate of each product
        ///     foreach (Product p in products)
        ///     {
        ///         p.ModifiedDate = DateTime.Now;
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
		public virtual IEnumerable<T> Upsert<T>(IEnumerable<T> source) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Upsert<T>(source);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Delete<T>(source);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}
            return dto.Requests.Peek().ReturnValue as T;
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
		public virtual void Delete<T>(PropertyExpression<T> whereExpression) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Delete<T>(whereExpression);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				this.ProcessDeferredRequests(dto);
			}
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
		public virtual void Delete<T>(Expression<Func<T, bool>> whereExpression) where T : class, IFlyweight, new()
        {
            this.Delete<T>(new PropertyExpressionConverter<T>().Convert(whereExpression));
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
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Delete<T>(query);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				this.ProcessDeferredRequests(dto);
			}	
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
        ///     var = new List<Department>();
        ///
        ///     // Create 10 new departments, set their required properties and add each to the list
        ///     for (int i = 0; i < 10; i++)
        ///     {
        ///         var department = new Department()
        ///         {
        ///             Name = "My New Department",
        ///             GroupName = "My New Department Group",
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
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Delete<T>(source);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}
            return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
        ///     var products = context.Select<Product>(100);
        ///     
        ///     // Update the ModifiedDate property on each product
        ///     foreach (Product p in products)
        ///     {
        ///         p.ModifiedDate = DateTime.Now;
        ///     }
        ///
        ///     // Create a product from a clone
        ///     var product = ObjectCloner.Clone<Product>(products[0]);
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
		public virtual IEnumerable<T> Persist<T>(FlyweightSet<T> source) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Persist<T>(source);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}
            return dto.Requests.Peek().ReturnValue as IEnumerable<T>;
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
		public virtual T Reload<T>(T source) where T : class, IFlyweight, new()
		{
			DataTransferObject dto = this.GetDataTransferObject();
			dto.Reload<T>(source);
			if (this.RequestMode == RequestMode.NonDeferred)
			{
				dto = this.ProcessDeferredRequests(dto);
			}	
			return dto.Requests.Peek().ReturnValue as T;
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
		public virtual IEnumerable<T> Select<T>(params SortExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
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
		public virtual IEnumerable<T> Select<T>(Expression<Func<T, bool>> whereExpression, params SortExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
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
		public virtual IEnumerable<T> Select<T>(PreloadDepth depth, Expression<Func<T, bool>> whereExpression, params SortExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
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
		public virtual IEnumerable<T> Select<T>(PreloadOptions<T> options, Expression<Func<T, bool>> whereExpression, params SortExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
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
		public virtual IEnumerable<T> Select<T>(int limit, Expression<Func<T, bool>> whereExpression, params SortExpression<T>[] sortExpressions) where T : class, IFlyweight, new()
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
		/// Terminates the communication with the remote server.
		/// </summary>
		public void Terminate()
		{
			if (_clientChannel != null)
			{
				try
				{
					ChannelServices.UnregisterChannel(_clientChannel);
					_messageSink = null;
					_clientChannel = null;
				}
				catch (Exception e)
                {
                    TraceHelper.WriteLine(MethodInfo.GetCurrentMethod(), e.ToString());
                }
			}
		}
	}
}
