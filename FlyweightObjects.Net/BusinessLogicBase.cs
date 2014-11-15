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
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Security.Authentication;
using System.Reflection;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Security;
using System.Collections;

namespace FlyweightObjects
{
    /// <summary>
    /// Serves as the base class for all business logic implementation.
    /// </summary>
    /// <typeparam name="TSource">The domain type for the business logic.</typeparam>
    public abstract class BusinessLogicBase<TSource> : DataContext, IBusinessLogic<TSource>, IBusinessLogic where TSource : class, IFlyweight, new()
    {
        private Type _defaultBusinessLogicType = null;
        private bool _autoDetectBusinessLogic = true;

        /// <summary>
        /// Gets or sets the default type of the business logic.
        /// </summary>
        public Type DefaultBusinessLogicType
        {
            get { return _defaultBusinessLogicType; }
            set { _defaultBusinessLogicType = value; }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating whether the business logic class for a given type should be automatically
        /// detected, instantiated and invoked for an operation.
        /// </summary>
        public bool AutoDetectBusinessLogic
        {
            get { return _autoDetectBusinessLogic; }
            set { _autoDetectBusinessLogic = value; }
        }

        /// <summary>
        /// Gets or sets the IStorageProvider instance for the context.
        /// </summary>
        IStorageProvider IBusinessLogic.StorageProvider
        {
            get { return base.StorageProvider; }
            set { base.StorageProvider = value; }
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
        /// Returns whether the current <see cref="ISecurityPrincipal"/> is valid for the operation.
        /// </summary>
        /// <param name="principal">The current principal.</param>
		public virtual bool ValidatePrincipal(ISecurityPrincipal principal)
		{
			return true;
		}

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="connectionString">The storage's connection string.</param>
        /// <param name="providerType">The StorageProviderType.</param>
        public BusinessLogicBase(string connectionString, StorageProviderType providerType)
            : base(connectionString, providerType)
        {
            if (!ValidatePrincipal(this.CurrentPrincipal))
            {
                throw new SecurityException("The current principal is not valid for the operation.");
            }
        }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="context">The object context from which to construct the new instance.</param>
        public BusinessLogicBase(DataContext context)
            : base(context.ConnectionString, context.StorageProvider.ProviderType)
        {
            this.StorageProvider = context.StorageProvider;
            if (!ValidatePrincipal(this.CurrentPrincipal))
            {
                throw new SecurityException("The current principal is not valid for the operation.");
            }
        }

		/// <summary>
		/// Returns all objects for the specified type of T in storage using the supplied <see cref="IStorageCommand"/>.  Note the value for 
		/// sql may be any object which implements the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
		/// </summary>
		/// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
		/// <param name="command">The <see cref="IStorageCommand"/> by which to retrieve the objects.</param>
		public sealed override IEnumerable<T> Select<T>(IStorageCommand command)
		{
			if (this.AutoDetectBusinessLogic && this.DefaultBusinessLogicType != null && typeof(TSource) != typeof(T))
			{
				return BusinessLogicFactory.GetBusinessLogic<T>(this).Select(command);
			}
			return base.Select<T>(command);
		}

		/// <summary>
		/// Inserts into the target storage the instance of source.
		/// </summary>
		/// <typeparam name="T">The domain type to insert.</typeparam>
		/// <param name="source">The instance of domain type to insert.</param>
		public sealed override T Insert<T>(T source)
		{
			if (source is IEnumerable)
			{
				return base.InvokeListMethod<T>(source, MethodBase.GetCurrentMethod());
			}
			if (this.AutoDetectBusinessLogic && this.DefaultBusinessLogicType != null && typeof(TSource) != typeof(T))
			{
				return BusinessLogicFactory.GetBusinessLogic<T>(this).Insert(source);
			}
			return base.Insert<T>(source);
		}

		/// <summary>
		/// Performs multiple inserts into the target entity using the supplied source.
		/// </summary>
		/// <typeparam name="T">The type parameter of the objects to insert.</typeparam>
		/// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
		public sealed override IEnumerable<T> Insert<T>(IEnumerable<T> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (source.Count<T>() == 0)
			{
				return source;
			}
			IList<T> list = source as FlyweightSet<T>;
			if (list == null)
			{
				list = source.ToList();
			}
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = this.Insert<T>(list[i]);
			}
			return source;
		}

		/// <summary>
		/// Performs multiple inserts into the target entity using the supplied source and batch size. Note that this
		/// functionality is only available for a subset of <see cref="IStorageProvider"/>s.
		/// </summary>
		/// <typeparam name="T">The type parameter of the objects to insert.</typeparam>
		/// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
		/// <param name="batchSize">The number of inserts contained within a batch.</param>
		public sealed override void Insert<T>(IEnumerable<T> source, int batchSize)
		{
			if (this.AutoDetectBusinessLogic)
			{
				throw new InvalidOperationException("Bulk operations cannot be performed when AutoDetectBusinessLogic is set to true.");
			}
			base.Insert<T>(source, batchSize);
		}

		/// <summary>
		/// Performs an insert into the target entity using the supplied <see cref="IQueryExpression{TSource}"/>. 
		/// </summary>
		/// <typeparam name="T">The type parameter of the objects to insert.</typeparam>
		/// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to insert the objects.</param>
		public sealed override void Insert<T>(IQueryExpression<T> query)
		{
			if (this.AutoDetectBusinessLogic && this.DefaultBusinessLogicType != null && typeof(TSource) != typeof(T))
			{
				BusinessLogicFactory.GetBusinessLogic<T>(this).Insert(query);
			}
			base.Insert<T>(query);
		}

		/// <summary>
		/// Updates the target storage with all instances contained within source.
		/// </summary>
		/// <typeparam name="T">The domain type to update.</typeparam>
		/// <param name="source">An instance of T.</param>
		public sealed override T Update<T>(T source)
		{
			if (source is IEnumerable)
			{
				return base.InvokeListMethod<T>(source, MethodBase.GetCurrentMethod());
			}
			if (this.AutoDetectBusinessLogic && this.DefaultBusinessLogicType != null && typeof(TSource) != typeof(T))
			{
				return BusinessLogicFactory.GetBusinessLogic<T>(this).Update(source);
			}
			return base.Update<T>(source);
		}

		/// <summary>
		/// Performs multiple updates of the target entity using the supplied source.
		/// </summary>
		/// <typeparam name="T">The type parameter of the objects to update.</typeparam>
		/// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
		public sealed override IEnumerable<T> Update<T>(IEnumerable<T> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (source.Count<T>() == 0)
			{
				return source;
			}
			IList<T> list = source as FlyweightSet<T>;
			if (list == null)
			{
				list = source.ToList();
			}
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = this.Update<T>(list[i]);
			}
			return source;
		}

		/// <summary>
		/// Performs multiple updates of the target entity using the supplied source and batch size. Note that this
		/// functionality is only available for a subset of <see cref="IStorageProvider"/>s. 
		/// </summary>
		/// <typeparam name="T">The type parameter of the objects to update.</typeparam>
		/// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
		/// <param name="batchSize">The number of updates contained within a batch.</param>
		public sealed override void Update<T>(IEnumerable<T> source, int batchSize)
		{
			if (this.AutoDetectBusinessLogic)
			{
				throw new InvalidOperationException("Bulk operations cannot be performed when AutoDetectBusinessLogic is set to true.");
			}
			base.Update<T>(source, batchSize);
		}

		/// <summary>
        /// Performs an update of the target entity using the supplied <see cref="IQueryExpression{TSource}"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the object to update.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to insert the object.</param>
		public sealed override void Update<T>(IQueryExpression<T> query)
		{
			if (this.AutoDetectBusinessLogic && this.DefaultBusinessLogicType != null && typeof(TSource) != typeof(T))
			{
				BusinessLogicFactory.GetBusinessLogic<T>(this).Update(query);
			}
			base.Update<T>(query);
		}

		/// <summary>
		/// Deletes the source from storage.
		/// </summary>
		/// <typeparam name="T">The domain type to delete.</typeparam>
		/// <param name="source">An instance of T.</param>
		public sealed override T Delete<T>(T source)
		{
			if (source is IEnumerable)
			{
				base.InvokeListMethod<T>(source, MethodBase.GetCurrentMethod());
			}
			if (this.AutoDetectBusinessLogic && this.DefaultBusinessLogicType != null && typeof(TSource) != typeof(T))
			{
				BusinessLogicFactory.GetBusinessLogic<T>(this).Delete(source);
			}
			else
			{
				base.Delete<T>(source);
			}
            return source;
		}

		/// <summary>
		/// Deletes all instances contained within source from storage.
		/// </summary>
		/// <typeparam name="T">The type parameter of the objects to delete.</typeparam>
		/// <param name="source">An instance of T.</param>
		public sealed override IEnumerable<T> Delete<T>(IEnumerable<T> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			IList<T> list = source as FlyweightSet<T>;
			if (list == null)
			{
				list = source.ToList();
			}
			for (int i = 0; i < list.Count; i++)
			{
				this.Delete<T>(list[i]);
			}
            return source;
		}

		/// <summary>
		/// Performs multiple deletes on the target entity using the supplied source and batch size. Note that this
		/// functionality is only available for a subset of <see cref="IStorageProvider"/>s. 
		/// </summary>
		/// <typeparam name="T">The type parameter of the objects to delete.</typeparam>
		/// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
		/// <param name="batchSize">The number of deletes contained within a batch.</param>
		public sealed override void Delete<T>(IEnumerable<T> source, int batchSize)
		{
			if (this.AutoDetectBusinessLogic)
			{
				throw new InvalidOperationException("Bulk operations cannot be performed when AutoDetectBusinessLogic is set to true.");
			}
			base.Delete<T>(source, batchSize);
		}

		/// <summary>
        /// Performs a delete on the target entity using the supplied <see cref="IQueryExpression{TSource}"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the object to update.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to delete the object.</param>
		public sealed override void Delete<T>(IQueryExpression<T> query)
		{
			if (this.AutoDetectBusinessLogic && this.DefaultBusinessLogicType != null && typeof(TSource) != typeof(T))
			{
				BusinessLogicFactory.GetBusinessLogic<T>(this).Delete(query);
			}
			base.Delete<T>(query);
		}

		/// <summary>
		/// Truncates a target entity located in storage.
		/// </summary>
		/// <typeparam name="T">The type abstracting the table to truncate.</typeparam>
		public sealed override void Truncate<T>()
		{
			base.Truncate<T>();
		}

		#region IBusinessLogic<TSource> Members

		/// <summary>
		/// Returns all objects for the specified type of T in storage using the supplied <see cref="IStorageCommand"/>.  Note the value for 
		/// sql may be any object which implements the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
		/// </summary>
		/// <param name="command">The <see cref="IStorageCommand"/> by which to retrieve the objects.</param>
		public virtual IEnumerable<TSource> Select(IStorageCommand command)
		{
			return this.Select<TSource>(command);
		}
		
		/// <summary>
		/// Performs an insert into the target entity with the values as given by the source object's properties.
		/// </summary>
		/// <param name="source">An instance of TSource.</param>
		public virtual TSource Insert(TSource source)
		{
			return this.Insert<TSource>(source);
		}

		/// <summary>
		/// Performs an insert into the target entity using the supplied <see cref="IQueryExpression{TSource}"/>. 
		/// </summary>
		/// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to insert the objects.</param>
		public virtual void Insert(IQueryExpression<TSource> query)
		{
			this.Insert<TSource>(query);
		}

		/// <summary>
		/// Updates the target storage with all instances contained within source.
		/// </summary>
		/// <param name="source">An instance of TSource.</param>
		public virtual TSource Update(TSource source)
		{
			return this.Update<TSource>(source);
		}

		/// <summary>
		/// Performs an update of the target entity using the supplied <see cref="IQueryExpression{TSource}"/>.
		/// </summary>
		/// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to insert the object.</param>
		public virtual void Update(IQueryExpression<TSource> query)
		{
			this.Update<TSource>(query);
		}

		/// <summary>
		/// Deletes the source from storage.
		/// </summary>
		/// <param name="source">An instance of TSource.</param>
		public virtual TSource Delete(TSource source)
		{
			return this.Delete<TSource>(source);
		}

		/// <summary>
		/// Performs a delete on the target entity using the supplied <see cref="IQueryExpression{TSource}"/>.
		/// </summary>
		/// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to delete the object.</param>
		public virtual void Delete(IQueryExpression<TSource> query)
		{
			this.Delete<TSource>(query);
		}

		#endregion
	}
}
