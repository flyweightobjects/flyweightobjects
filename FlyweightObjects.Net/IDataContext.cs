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
using System.Collections;
using System.Data;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents a depth determining how deeply objects should be retrieved from their graph when a single call is made.
	/// </summary>
	public enum PreloadDepth
	{
		/// <summary>
		/// A flat object and no degree of relative separation from itself exposed by its graph.
		/// </summary>
		FlatObject,
		/// <summary>
		/// A flat object plus one degree of relative separation from itself exposed by its graph.
		/// </summary>
		FirstRelatives,
		/// <summary>
		/// A flat object plus two degrees of relative separation from itself exposed by its graph.
		/// </summary>
		SecondRelatives,
		/// <summary>
		/// A flat object plus three degrees of relative separation from iself exposed by its graph.
		/// </summary>
		ThirdRelatives,
		/// <summary>
		/// A flat object plus four degrees of relative separation from iself exposed by its graph.
		/// </summary>
		FourthRelatives,
		/// <summary>
		/// A flat object plus five degrees of relative separation from iself exposed by its graph.
		/// </summary>
		FifthRelatives
	}
	
	/// <summary>
	/// Represents a context by which storage operations can be performed.
	/// </summary>
    public interface IDataContext
    {
        /// <summary>
        /// Gets or sets the unique name of the domain which identifies the context with specific object types. Parity must exist between
        /// the value of this property and objects with a <see cref="DataTable"/>.DomainName in order for deferred loading to work correctly
        /// acrosss multiple domains.
        /// </summary>
        string DomainName { get; set; }

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// the retrieved object, otherwise it will be its default.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="source">An instance of T.</param>
        bool TrySelect<T>(out T source) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// an IEnumerable of the retrieved objects, otherwise if will be an empty instance of IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        bool TrySelect<T>(out IEnumerable<T> source) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// the retrieved object, otherwise it will be its default.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="source">An instance of T.</param>
        bool TrySelect<T>(PropertyExpression<T> whereExpression, out T source) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// an IEnumerable of the retrieved objects, otherwise if will be an empty instance of IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> criteria used to query the storage.</param>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        bool TrySelect<T>(PropertyExpression<T> whereExpression, out IEnumerable<T> source) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// the retrieved object, otherwise it will be its default.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to query the storage.</param>
        /// <param name="source">An instance of T.</param>
        bool TrySelect<T>(IQueryExpression<T> query, out T source) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// an IEnumerable of the retrieved objects, otherwise if will be an empty instance of IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to query the storage.</param>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        bool TrySelect<T>(IQueryExpression<T> query, out IEnumerable<T> source) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be
        /// the retrieved object, otherwise it will be its default.  Note the value for sql may be any object which implements 
        /// the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> used to query the storage.</param>
        /// <param name="source">An instance of T.</param>
        bool TrySelect<T>(IStorageCommand command, out T source) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns a boolean value indicating whether or not the source exists in storage. If true, the source will be an 
        /// IEnumerable of the retrieved objects, otherwise if will be an empty instance of IEnumerable.  Note the value for 
        /// sql may be any object which implements the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> used to query the storage.</param>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        bool TrySelect<T>(IStorageCommand command, out IEnumerable<T> source) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns a boolean value indicating whether the object exists in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="source">An instance of T.</param>
        bool Exists<T>(T source) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns a boolean value indicating whether the object exists in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        bool Exists<T>(PropertyExpression<T> whereExpression) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns a boolean value indicating whether the object exists in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to query the storage.</param>
        bool Exists<T>(IQueryExpression<T> query) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns a boolean value indicating whether the object exists in storage.  Note the value for sql may be any 
        /// object which implements the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of source.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> used to query the storage.</param>
        bool Exists<T>(IStorageCommand command) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        IEnumerable<T> Select<T>() where T : class, IFlyweight, new();

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage constrained by the amount as specified
        /// by limit.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
        IEnumerable<T> Select<T>(int limit) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage constrained by the amount as specified
        /// by limit and using the criteria as specified by whereExpression.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        IEnumerable<T> Select<T>(int limit, PropertyExpression<T> whereExpression) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage using the criteria as specified by whereExpression,
        /// sorted by sortExpressions.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="PropertyExpression{T}"/> by which to sort.</param>
        IEnumerable<T> Select<T>(PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage using the criteria as specified by whereExpression,
        /// sorted by sortExpressions. Additional objects will be retrieved in the graph according to their level as specified
        /// by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="depth">The depth limit of the object graph.</param>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="PropertyExpression{T}"/> by which to sort.</param>
        IEnumerable<T> Select<T>(PreloadDepth depth, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns all objects for the specified type of T contained in storage using the criteria as specified by whereExpression,
        /// sorted by sortExpressions. Additional objects will be retrieved in the graph according to their level as specified
        /// by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="options">A <see cref="PreloadOptions{T}"/> specifying additional related objects to retrieve.</param>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="PropertyExpression{T}"/> by which to sort.</param>
        IEnumerable<T> Select<T>(PreloadOptions<T> options, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns all objects for the specified type T in storage constrained by the amount as specified by limit using the 
        /// criteria as specified by whereExpression, and sorted by sortExpressions.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="limit">The maximum number of objects to retrieve.</param>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used to query the storage.</param>
        /// <param name="sortExpressions">A parameter array of <see cref="PropertyExpression{T}"/> by which to sort.</param>
        IEnumerable<T> Select<T>(int limit, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new();

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
        IEnumerable<T> Select<T>(PreloadOptions<T> options, int limit, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new();

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
        IEnumerable<T> Select<T>(PreloadDepth depth, int limit, PropertyExpression<T> whereExpression, params PropertyExpression<T>[] sortExpressions) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IQueryExpression{TSource}"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> by which to retrieve the objects.</param>
        IEnumerable<T> Select<T>(IQueryExpression<T> query) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IQueryExpression{TSource}"/>. Additional objects 
        /// will be retrieved in the graph according to their level as specified by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="depth">The depth limit of the object graph.</param>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> by which to retrieve the objects.</param>
        IEnumerable<T> Select<T>(PreloadDepth depth, IQueryExpression<T> query) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IQueryExpression{TSource}"/>. Additional objects 
        /// will be retrieved in the graph according to their level as specified by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="options">A <see cref="PreloadOptions{T}"/> specifying additional related objects to retrieve.</param>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> by which to retrieve the objects.</param>
        IEnumerable<T> Select<T>(PreloadOptions<T> options, IQueryExpression<T> query) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IStorageCommand"/>.  Note the value for 
        /// sql may be any object which implements the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="command">The <see cref="IStorageCommand"/> by which to retrieve the objects.</param>
        IEnumerable<T> Select<T>(IStorageCommand command) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IStorageCommand"/>. Additional objects 
        /// will be retrieved in the graph according to their level as specified by depth.  Note the value for sql may be any 
        /// object which implements the <see cref="IStorageCommand"/> interface including <see cref="StoredProcedure"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="depth">The depth limit of the object graph.</param>
        /// <param name="command">The <see cref="IStorageCommand"/> by which to retrieve the objects.</param>
        IEnumerable<T> Select<T>(PreloadDepth depth, IStorageCommand command) where T : class, IFlyweight, new();

        /// <summary>
        /// Returns all objects for the specified type of T in storage using the supplied <see cref="IQueryExpression{TSource}"/>. Additional objects 
        /// will be retrieved in the graph according to their level as specified by depth.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to retrieve.</typeparam>
        /// <param name="options">A <see cref="PreloadOptions{T}"/> specifying additional related objects to retrieve.</param>
        /// <param name="command">The <see cref="IStorageCommand"/> used to query the storage.</param>
        IEnumerable<T> Select<T>(PreloadOptions<T> options, IStorageCommand command) where T : class, IFlyweight, new();

        /// <summary>
        /// Performs an insert into the target entity with the values as given by the source object's properties.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to insert.</typeparam>
        /// <param name="source">An instance of T.</param>
        T Insert<T>(T source) where T : class, IFlyweight, new();

        /// <summary>
        /// Performs an insert into the target entity using the supplied <see cref="IQueryExpression{TSource}"/>. 
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to insert.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to insert the objects.</param>
        void Insert<T>(IQueryExpression<T> query) where T : class, IFlyweight, new();

        /// <summary>
        /// Performs multiple inserts into the target entity using the supplied source.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to insert.</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        IEnumerable<T> Insert<T>(IEnumerable<T> source) where T : class, IFlyweight, new();

        /// <summary>
        /// Performs an update of the target entity with the values as given by the source object's properties.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to update.</typeparam>
        /// <param name="source">An instance of T.</param>
        T Update<T>(T source) where T : class, IFlyweight, new();

        /// <summary>
        /// Performs an update of the target entity using the supplied <see cref="IQueryExpression{TSource}"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the object to update.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to insert the object.</param>
        void Update<T>(IQueryExpression<T> query) where T : class, IFlyweight, new();

        /// <summary>
        /// Performs multiple updates of the target entity using the supplied source.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to update.</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        IEnumerable<T> Update<T>(IEnumerable<T> source) where T : class, IFlyweight, new();

        /// <summary>
        /// If the object exiss, performs an update of the target entity with the values as given by the source 
        /// object's properties, otherwise performs an insert.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to update.</typeparam>
        /// <param name="source">An instance of T.</param>
        T Upsert<T>(T source) where T : class, IFlyweight, new();

        /// <summary>
        /// For each object in source, if the object alrady exists, performs an update of the target entity using 
        /// the supplied source, otherwise an insert is applied.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to either update or insert.</typeparam>
        /// <param name="source">An object which implements <see cref="IEnumerable{T}"/>.</param>
        IEnumerable<T> Upsert<T>(IEnumerable<T> source) where T : class, IFlyweight, new();

        /// <summary>
        /// Deletes the source from storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to delete.</typeparam>
        /// <param name="source">An instance of T.</param>
        T Delete<T>(T source) where T : class, IFlyweight, new();

        /// <summary>
        /// Deletes all instances contained within source from storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to delete.</typeparam>
        /// <param name="source">An instance of T.</param>
        IEnumerable<T> Delete<T>(IEnumerable<T> source) where T : class, IFlyweight, new();

        /// <summary>
        /// Performs a delete on the target entity given the supplied filter criteria.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to delete.</typeparam>
        /// <param name="whereExpression">The <see cref="PropertyExpression{T}"/> used as the filter for the delete.</param>
        void Delete<T>(PropertyExpression<T> whereExpression) where T : class, IFlyweight, new();

        /// <summary>
        /// Performs a delete on the target entity using the supplied <see cref="IQueryExpression{TSource}"/>.
        /// </summary>
        /// <typeparam name="T">The type parameter of the object to update.</typeparam>
        /// <param name="query">The <see cref="IQueryExpression{TSource}"/> used to delete the object.</param>
        void Delete<T>(IQueryExpression<T> query) where T : class, IFlyweight, new();

        /// <summary>
        /// Performs all necessary operations on the supplied <see cref="IFlyweightSet{T}"/> object. Note that all objects which
        /// have been removed from the <see cref="IFlyweightSet{T}"/> will be deleted, while the others will either be inserted
        /// or deleted based upon their status in storage.
        /// </summary>
        /// <typeparam name="T">The type parameter of the changed objects to persist.</typeparam>
        /// <param name="source">An instance of a <see cref="FlyweightSet{T}"/>.</param>
        IEnumerable<T> Persist<T>(FlyweightSet<T> source) where T : class, IFlyweight, new();

        /// <summary>
        /// Reloads the source object based upon its identifiers as specified by its <see cref="DataColumnAttribute"/> Identifer properties.
        /// </summary>
        /// <typeparam name="T">The type parameter of the objects to reload.</typeparam>
        /// <param name="source">An instance of T.</param>
        T Reload<T>(T source) where T : class, IFlyweight, new();
    }
}
