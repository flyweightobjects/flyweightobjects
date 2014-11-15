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
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Collections;
using System.Security.Cryptography;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents the generic class for clients using the Tcp protocol.
    /// </summary>
    /// <typeparam name="TService">The custom <see cref="IServiceContext"/> implementation exposed by the remote service.</typeparam>
    public class TcpClientContext<TService> : TcpClientContext where TService : class, IServiceContext
    {
        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="url">The url of the remote service.</param>
        /// <param name="secureChannel">Determines whether the channel should be secured.</param>
        public TcpClientContext(string url, bool secureChannel)
            : base(typeof(TService), url, secureChannel)
        {

        }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="url">The url of the remote service.</param>
        /// <param name="secureChannel">Determines whether the channel should be secured.</param>
        /// <param name="compressPayloads">Determines whether the request and response payloads should be compressed.</param>
        /// <param name="encryptPayloads">Determines whether the request and response payloads should be encrypted.</param>
        public TcpClientContext(string url, bool secureChannel, bool compressPayloads, bool encryptPayloads)
            : base(typeof(TService), url, secureChannel, compressPayloads, encryptPayloads)
        {

        }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="url">The url of the remote service.</param>
        /// <param name="secureChannel">Determines whether the channel should be secured.</param>
        /// <param name="compressPayloads">Determines whether the request and response payloads should be compressed.</param>
        /// <param name="key">A valid 24 byte <see cref="TripleDES.Key"/> value to be used when encrypting and decrypting payloads.</param>
        public TcpClientContext(string url, bool secureChannel, bool compressPayloads, byte[] key)
            : base(typeof(TService), url, secureChannel, compressPayloads, key)
        {

        }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="url">The url of the remote service.</param>
        /// <param name="secureChannel">Determines whether the channel should be secured.</param>
        /// <param name="properties">An <see cref="IDictionary"/> of properties specific for the remoting channel.</param>
        public TcpClientContext(string url, bool secureChannel, IDictionary properties)
            : base(typeof(TService), url, secureChannel, properties)
        {

        }

        /// <summary>
        /// Gets the current service to which the client connects.
        /// </summary>
        public new TService Service
        {
            get { return base.Service.AsType<TService>(); }
        }
    }
    
    /// <summary>
	/// Represents the class for clients using the Tcp protocol.
	/// </summary>
	public class TcpClientContext : ClientContextBase, IDataContext
	{
		/// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="remotedType">The MarshalByRefObject type exposed by the remote service.</param>
        /// <param name="url">The url of the remote service.</param>
        /// <param name="secureChannel">Determines whether the channel should be secured.</param>
		public TcpClientContext(Type remotedType, string url, bool secureChannel)
			: base(remotedType, ChannelType.Tcp, url, secureChannel)
        {
			
        }

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="remotedType">The MarshalByRefObject type exposed by the remote service.</param>
		/// <param name="url">The url of the remote service.</param>
		/// <param name="secureChannel">Determines whether the channel should be secured.</param>
		/// <param name="compressPayloads">Determines whether the request and response payloads should be compressed.</param>
		/// <param name="encryptPayloads">Determines whether the request and response payloads should be encrypted.</param>
		public TcpClientContext(Type remotedType, string url, bool secureChannel, bool compressPayloads, bool encryptPayloads)
			: base(remotedType, ChannelType.Http, url, secureChannel)
		{
			base.CompressPayloads = compressPayloads;
			base.EncryptPayloads = encryptPayloads;
		}

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="remotedType">The MarshalByRefObject type exposed by the remote service.</param>
        /// <param name="url">The url of the remote service.</param>
        /// <param name="secureChannel">Determines whether the channel should be secured.</param>
        /// <param name="compressPayloads">Determines whether the request and response payloads should be compressed.</param>
        /// <param name="key">A valid 24 byte <see cref="TripleDES.Key"/> value to be used when encrypting and decrypting payloads.</param>
        public TcpClientContext(Type remotedType, string url, bool secureChannel, bool compressPayloads, byte[] key)
            : base(remotedType, ChannelType.Tcp, url, secureChannel)
        {
            if (key == null || key.Length != 24)
            {
                throw new ArgumentException(ErrorStrings.InvalidEncryptionKeyLength);
            }
            base.CompressPayloads = compressPayloads;
            base.EncryptPayloads = true;
            base.EncryptionKey = key;
        }

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="remotedType">The MarshalByRefObject type exposed by the remote service.</param>
		/// <param name="url">The url of the remote service.</param>
		/// <param name="secureChannel">Determines whether the channel should be secured.</param>
		/// <param name="properties">An <see cref="IDictionary"/> of properties specific for the remoting channel.</param>
		public TcpClientContext(Type remotedType, string url, bool secureChannel, IDictionary properties)
			: base(remotedType, ChannelType.Tcp, url, secureChannel, properties)
		{
			
		}
	}
}
