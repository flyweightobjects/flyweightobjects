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
using System.Linq.Expressions;

namespace FlyweightObjects
{
    /// <summary>
    /// The direction of a sort.
    /// </summary>
    public enum SortDirection
    {
        /// <summary>
        /// Sort in ascending order.
        /// </summary>
        Asc,
        
        /// <summary>
        /// Sort in descending order.
        /// </summary>
        Desc
    }

    /// <summary>
    /// Represents an expression that also has a <see cref="SortDirection"/> associated to it.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SortExpression<T>
    {
        private Expression _expression;
        private SortDirection _direction;

        /// <summary>
        /// Gets or sets the direction of the sort order.
        /// </summary>
        public SortDirection Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Expression"/> for the sorting.
        /// </summary>
        public Expression Expression
        {
            get { return _expression; }
            set { _expression = value; }
        }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="expression">An expression specifying the property by which to sort.</param>
        public SortExpression(Expression<Func<T, object>> expression)
        {
            _expression = expression;
            _direction = SortDirection.Asc;
        }
        
        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="expression">An expression specifying the property by which to sort.</param>
        /// <param name="direction">The direction of the sort order.</param>
        public SortExpression(Expression<Func<T, object>> expression, SortDirection direction)
        {
            _expression = expression;
            _direction = direction;
        }
        
        /// <summary>
        /// Specifies the property that should sorted.
        /// </summary>
        /// <param name="expression">An expression specifying the property by which to sort.</param>
        public static SortExpression<T> Sort(Expression<Func<T, object>> expression)
        {
            return new SortExpression<T>(expression);
        }

        /// <param name="expression">An expression specifying the property by which to sort.</param>
        /// <param name="direction">The direction of the sort order.</param>
        public static SortExpression<T> Sort(Expression<Func<T, object>> expression, SortDirection direction)
        {
            return new SortExpression<T>(expression, direction);
        }
    }
}
