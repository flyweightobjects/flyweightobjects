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
using System.Runtime.Serialization;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents an exception thrown when processing a command, exposing the <see cref="IStorageCommand"/> which was 
    /// issued to the provider.
    /// </summary>
    /// <example>
    /// The following example uses a Product class from the Microsoft AdventureWorks SQL Server sample database. Because the SqlText of the <see cref="StorageCommand"/>
    /// is malformed, a <see cref="StorageProviderException"/> will be thrown. Note that if you are calling a command from across process boundaries, you will need to 
    /// catch a base <see cref="Exception"/> and cast its inner exception as a <see cref="StorageProviderException"/> to see applicable data about the command.
    /// <code>
    /// <![CDATA[
    /// using (var context = new DataContext(@"Integrated Security=SSPI;Initial Catalog=AdventureWorks;Data Source=localhost;", StorageProviderType.SqlServer))
    /// {
    ///     try
    ///     {
    ///         Product product = null;
    ///         if (context.TrySelect<Product>(new StorageCommand("SELECT * Production.Product WHERE ProductID = 355"), out product))
    ///         {
    ///             Console.WriteLine("Product name is {0}", product.Name);
    ///         }
    ///     }
    ///     catch (StorageProviderException e)
    ///     {
    ///         Console.WriteLine(e.ToString());
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    [Serializable]
    public class StorageProviderException : Exception
    {
        private StorageProviderType _providerType;
        private IStorageCommand _command = null;

        /// <summary>
        /// Gets the <see cref="StorageProviderType"/> associated with the exception.
        /// </summary>
        public StorageProviderType ProviderType
        {
            get { return _providerType; }
        }

        /// <summary>
        /// Gets the <see cref="IStorageCommand"/> that threw the exception.
        /// </summary>
        public IStorageCommand Command
        {
            get { return _command; }
        }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="providerType">The <see cref="StorageProviderType"/> associated with the exception.</param>
        /// <param name="command">The <see cref="IStorageCommand"/> which caused the exception.</param>
        /// <param name="innerException">The inner exception thrown by the ADO.NET provider.</param>
        public StorageProviderException(StorageProviderType providerType, IStorageCommand command, Exception innerException)
            : base("An error ocurred while processing the following command: " + command.ToString() + ". See the inner exception for more details.", innerException)
        {
            _providerType = providerType;
            _command = command;
        }

        /// <summary>
        /// Constructs a new instance of the class specifically for deserialization purposes.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> instance for the deserialization.</param>
        /// <param name="context">The <see cref="StreamingContext"/> for the deserialization.</param>
        protected StorageProviderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _command = info.GetValue("_sql", typeof(IStorageCommand)) as IStorageCommand;
        }

        /// <summary>
        /// Adds the extended member data for serialization purposes.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that contains the class meta data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> used during serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_sql", _command);
        }
    }
}
