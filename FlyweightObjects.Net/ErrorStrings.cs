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

namespace FlyweightObjects
{
	/// <summary>
	/// Represents common error text to be associated with exceptions.
	/// </summary>
	internal static class ErrorStrings
	{
        /// <summary>
        /// Represents a ConcurrencyViolationException error message.
        /// </summary>
        public static readonly string ConcurrencyViolationException = "The operation has been aborted because the object is no longer current.";
		/// <summary>
        /// Represents a SelectTransactionTypeException error message.
		/// </summary>
        public static readonly string SelectTransactionTypeException = "The TransactionType for the supplied query must be of type Select.";
		/// <summary>
		/// Represents a InsertTransactionTypeException error message.
		/// </summary>
		public static readonly string InsertTransactionTypeException = "The TransactionType for the supplied query must be of type Insert.";
		/// <summary>
		/// Represents an UpdateTransactionTypeException error message.
		/// </summary>
		public static readonly string UpdateTransactionTypeException = "The TransactionType for the supplied query must be of type Update.";
		/// <summary>
		/// Represents an DeleteTransactionTypeException error message.
		/// </summary>
		public static readonly string DeleteTransactionTypeException = "The TransactionType for the supplied query must be of type Delete.";
		/// <summary>
		/// Represents an InvalidEnumerableArgumentException error message.
		/// </summary>
		public static readonly string InvalidEnumerableArgumentException = "The specified type argument cannot be enumerable.";
		/// <summary>
		/// Represents an ProviderNotSupportedException error message.
		/// </summary>
		public static readonly string ProviderNotSupportedException = "The specified StorageProvider is not currently supported.";
		/// <summary>
		/// Represents an IEnumerableNotImplementedException error message.
		/// </summary>
		public static readonly string IEnumerableNotImplementedException = "The specified type argument must implement IEnumerable.";
		/// <summary>
		/// Represents an InvalidIEnumerableIListException error message.
		/// </summary>
		public static readonly string InvalidIEnumerableIListException = "The specified generic type must implement IList<T>.";
		/// <summary>
		/// Represents an MissingDefaultConstructorException error message.
		/// </summary>
		public static readonly string MissingDefaultConstructorException = "The specified type must have a default constructor.";
		/// <summary>
		/// Represents an MissingDataTableAttributeException error message.
		/// </summary>
		public static readonly string MissingDataTableAttributeException = "A DataTable could not be found for the specified type.";
		/// <summary>
		/// Represents an MissingColumnAttributeException error message.
		/// </summary>
		public static readonly string MissingColumnAttributeException = "A ColumnAttribute could not be found for the member {0}.";
		/// <summary>
		/// Represents an InvalidPropertyExpressionException error message.
		/// </summary>
		public static readonly string InvalidPropertyExpressionException = "The PropertyExpression specified does not abstract a valid property.";
		/// <summary>
		/// Represents an ValidationFailedException error message.
		/// </summary>
		public static readonly string ValidationFailedException = "The current object failed its validation rules.";
		/// <summary>
        /// Represents an InvalidToCommandConversionOperation error message.
		/// </summary>
		public static readonly string InvalidToCommandConversionOperation = "Cannot convert the current type to an IStorageCommand object.";
		/// <summary>
		/// Represents an InvalidMemberInfoExtensionArgument error message.
		/// </summary>
		public static readonly string InvalidMemberInfoExtensionArgument = "The only valid types for member are FieldInfo and PropertyInfo.";
		/// <summary>
		/// Represents a EmptyDataTransferObjectRequestQueue error message.
		/// </summary>
		public static readonly string EmptyDataTransferObjectRequestQueue = "There are no queued requests to be processed.";
		/// <summary>
		/// Represents an InvalidBeginDeferredRequestCall error message;
		/// </summary>
		public static readonly string InvalidBeginDeferredRequestsCall = "The context is currently in DeferredRequestMode. You must first call CancelDeferredRequests before calling BeginDeferredRequests.";
		/// <summary>
		/// Represents an InvalidProcessDeferredRequestsCall error message.
		/// </summary>
		public static readonly string InvalidProcessDeferredRequestsCall = "Call BeginDeferredRequests first in order to queue multiple calls to be sent to the server.";
        /// <summary>
        /// Represents an InvalidPropertyException error message.
        /// </summary>
        public static readonly string InvalidPropertyNameException = "The property or field name {0} was not found in PropertyStorage. Please ensure that the property definition is adorned with a PropertyStorageAttribute or one of its derivatives, or that the specified field has been selected. If explicitly implementing an interface, specify the type's full name and property name in dot notation syntax.";
        /// <summary>
        ///  Represents an InvalidTableNameException error message.
        /// </summary>
        public static readonly string InvalidTableNameException = "The TableName property of the DataTableAttribute for the class is either invalid or missing.";
        /// <summary>
        ///  Represents an InvalidEncryptionKeyLength error message.
        /// </summary>
        public static readonly string InvalidEncryptionKeyLength = "The length of the encryption key must be 24 bytes.";
        /// <summary>
        /// Represents the error message encountered when an Expression cannot be evaluated as a MemberExpression.
        /// </summary>
        public static readonly string InvalidMemberExpressionOfProperty = "The supplied expression does not refer to a property.";
        /// <summary>
        /// Represents the error message encountered when an empty collection argument has been supplied.
        /// </summary>
        public static readonly string EmptyEnumerableArgumentException = "The supplied argument must have at least one value in the collection.";
	}
}
