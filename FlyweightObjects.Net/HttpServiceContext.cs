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
using System.Security.Cryptography;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents server side context that can be used to process requests over the Http protocol.
	/// </summary>
	public abstract class HttpServiceContext : ServiceContextBase
	{
		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="connectionString">The connection string to the storage.</param>
		/// <param name="providerType">The type of storage.</param>
		public HttpServiceContext(string connectionString, StorageProviderType providerType)
			: base(connectionString, providerType)
		{

		}

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="connectionString">The connection string to the storage.</param>
		/// <param name="providerType">The type of storage.</param>
		/// <param name="compressPayloads">Determines whether the message payloads should be compressed.</param>
		/// <param name="encryptPayloads">Determines whether the message payloads should be encrypted.</param>
		public HttpServiceContext(string connectionString, StorageProviderType providerType, bool compressPayloads, bool encryptPayloads)
			: base(connectionString, providerType)
		{
			base.CompressPayloads = compressPayloads;
			base.EncryptPayloads = encryptPayloads;
		}

        /// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		/// <param name="connectionString">The connection string to the storage.</param>
		/// <param name="providerType">The type of storage.</param>
		/// <param name="compressPayloads">Determines whether the message payloads should be compressed.</param>
        /// <param name="key">A valid 24 byte <see cref="TripleDES.Key"/> value to be used when encrypting and decrypting payloads.</param>
		public HttpServiceContext(string connectionString, StorageProviderType providerType, bool compressPayloads, byte[] key)
			: base(connectionString, providerType)
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
        /// <param name="defaultBusinessLogicType">The default <see cref="BusinessLogicBase{TSource}"/> implementation for the domain.</param>
        public HttpServiceContext(Type defaultBusinessLogicType)
            : base(defaultBusinessLogicType)
        {

        }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="defaultBusinessLogicType">The default <see cref="BusinessLogicBase{TSource}"/> implementation for the domain.</param>
        /// <param name="compressPayloads">Determines whether the message payloads should be compressed.</param>
        /// <param name="encryptPayloads">Determines whether the message payloads should be encrypted.</param>
        public HttpServiceContext(Type defaultBusinessLogicType, bool compressPayloads, bool encryptPayloads)
            : base(defaultBusinessLogicType)
        {
            base.CompressPayloads = compressPayloads;
            base.EncryptPayloads = encryptPayloads;
        }
	}
}
