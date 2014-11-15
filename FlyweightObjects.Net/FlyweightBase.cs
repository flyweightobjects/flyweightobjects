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
using System.ComponentModel;
using System.Linq.Expressions;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents a concrete base class from which other types can be derived.
    /// </summary>
    public class FlyweightBase : IFlyweight, IRedundancyCheck, IValidatable, INotifyPropertyChanging, INotifyPropertyChanged, IPropertyChangedTrackable
    {
        /// <summary>
        /// Represents the <see cref="PropertyStorage"/> which manages manages state of member fields.
        /// </summary>
        protected PropertyStorage Storage { get; set; }

		/// <summary>
		/// Constructs a new instance of the class.
		/// </summary>
		public FlyweightBase()
		{
			this.Storage = new PropertyStorage(this.GetType());
		}

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="storage">The <see cref="PropertyStorage"/> which manages state of member fields.</param>
        public FlyweightBase(PropertyStorage storage)
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

        #region IRedundancyCheck Members

		/// <summary>
		/// Gets or sets a hash on the object in order to determine if an instance has changed.
		/// </summary>
        [PropertyStorage]
        public string Checksum
        {
            get { return this.Storage.GetProperty<string>(PropertyOf<FlyweightBase>.Name(p => p.Checksum)); }
			set { this.Storage.SetProperty<string>(PropertyOf<FlyweightBase>.Name(p => p.Checksum), value); }
        }

        #endregion

        #region IValidatable Members

        /// <summary>
        /// Returns a boolean indicating whether the current state of the object is valid. If an objeect implements
        /// this interface, the <see cref="DataContext"/> will validate the object prior to performing an insert
        /// or update operation. Returning false will cause a <see cref="ValidationException"/> to be thrown be the
        /// <see cref="DataContext"/>.
        /// </summary>
		/// <param name="context">The <see cref="IDataContext"/> used to query ancillary objects if necessary.</param>
        /// <param name="transactionType">The transaction type that will occur post successful validation.</param>
        /// <param name="message">The message to be displayed.</param>
        public virtual bool TryValidate(IDataContext context, TransactionType transactionType, out string message)
        {
            message = string.Empty;
            return true;
        }

        #endregion

        #region INotifyPropertyChanging Members

        /// <summary>
        /// Represents the method that will handle the PropertyChanged event raised when a property is changing on an object. 
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;

        /// <summary>
        /// Notifies clients that a property value is changing. 
        /// </summary>
        /// <typeparam name="T">The type which exposes the property.</typeparam>
        /// <param name="property">An expression which represents the property.</param>
        protected void SendPropertyChanging<T>(Expression<Func<T, object>> property)
        {
            string propertyName = PropertyOf<T>.Name(property);
            SendPropertyChanging(propertyName);
        }

        /// <summary>
        /// Notifies clients that a property value is changing.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected void SendPropertyChanging(string propertyName)
        {
            if (propertyName != PropertyOf<FlyweightBase>.Name(p => p.Checksum))
            {
                if (string.IsNullOrEmpty(((IRedundancyCheck)this).Checksum))
                {
                    ((IRedundancyCheck)this).Checksum = new ChecksumBuilder().BuildChecksum(this);
                }
                if (this.PropertyChanging != null)
                {
                    this.PropertyChanging(this, new PropertyChangingEventArgs(string.Empty));
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Represents the method that will handle the PropertyChanged event raised when a property is changed on an object. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies clients that a property value has changed. 
        /// </summary>
        /// <typeparam name="T">The type which exposes the property.</typeparam>
        /// <param name="property">An expression which represents the property.</param>
        protected void SendPropertyChanged<T>(Expression<Func<T, object>> property)
        {
            string propertyName = PropertyOf<T>.Name(property);
            SendPropertyChanged(propertyName);
        }

        /// <summary>
        /// Notifies clients that a property value is changing.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected void SendPropertyChanged(string propertyName)
        {
			if (propertyName != PropertyOf<FlyweightBase>.Name(p => p.Checksum))
            {
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
                ((IPropertyChangedTrackable)this).ChangedProperties.Add(propertyName);
            }
        }

        #endregion

        #region IPropertyChangedTrackable Members

		/// <summary>
		/// Gets the list of properties that have been changed.
		/// </summary>
        [PropertyStorage]
        public HashSet<string> ChangedProperties
        {
            get { return this.Storage.GetProperty<HashSet<string>>(PropertyOf<FlyweightBase>.Name(p => ChangedProperties)); }
        }

        #endregion
    }
}
