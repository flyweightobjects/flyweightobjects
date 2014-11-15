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
    /// Represents a class which can return stongly typed property names.
    /// </summary>
    /// <typeparam name="TSource">The type of the class for which ti find the member.</typeparam>
    public static class PropertyOf<TSource>
    {
        /// <summary>
        /// Returns the name of the specified property.
        /// </summary>
        /// <typeparam name="T">The type that the property must expose.</typeparam>
        /// <param name="expression">The expression for the property.</param>
        public static string Name<T>(Expression<Func<TSource, T>> expression)
        {
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			string propertyName = string.Empty;
			if (expression.Body.NodeType == ExpressionType.MemberAccess)
			{
				MemberExpression memberExpr = expression.Body as MemberExpression;
				propertyName = memberExpr.Member.Name;
			}
			else
			{
				UnaryExpression unaryExpr = expression.Body as UnaryExpression;
				if (unaryExpr == null)
				{
					throw new ArgumentException(string.Format("The supplied expression's body must be a {0}.", typeof(UnaryExpression).FullName));
				}
				MemberExpression memberExpr = unaryExpr.Operand as MemberExpression;
				if (memberExpr == null)
				{
					throw new ArgumentException(string.Format("The supplied expression's operand must be a {0}.", typeof(MemberExpression).FullName));
				}
				propertyName = memberExpr.Member.Name;
			}
            return propertyName;
        }
    }
}
