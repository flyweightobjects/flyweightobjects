//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
//  Author:         Marcus Crane, Software Developer / Architect                                        //
//  E-mail:         mailto:support@FlyweightObjects.NET                                                 //
//  Company:        FlyweightObjects.NET                                                                //
//  Copyright:      Copyright © FlyweightObjects.NET 2011, All rights reserved.                         //
//  Date Created:   04/02/2010                                                                          //
//                                                                                                      //
//  Disclaimer:                                                                                         //
//  ===========                                                                                         //
//  This code file is provided "as is" with no expressed or implied warranty. The author accepts no     //
//  liability for any damage or loss that the code file may cause as a result of its use. Any           //
//  modification, copying, or reverse engineering of this code file, or the underlying architectural    //
//  foundation it supports, is strictly prohibited without the express written consent of the author.   //
//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
using System;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents a query builder to build Microsoft Sql Server Compact Edition specific queries.
    /// </summary>
    [Serializable]
    public class SqlServerCeQueryBuilder<TSource> : QueryBuilderBase<TSource> where TSource : class, IFlyweight, new()
        {
        /// <summary>
        /// Gets the prefix characters to prepend to a parameter.
        /// </summary>
        public override string ParameterPrefix
        {
            get { return SqlServerStorageProvider.GetParameterPrefix(); }
        }

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="provider">The associated <see cref="IStorageProvider"/>.</param>
        public SqlServerCeQueryBuilder(IStorageProvider provider)
            : base(provider)
        {

        }

        /// <summary>
        /// Gets a proper function name by <see cref="FunctionType"/>.
        /// </summary>
        /// <param name="functionType">The type of supported function.</param>
        public override string GetFunctionName(FunctionType functionType)
        {
            switch (functionType)
            {
                case FunctionType.Trim:
                    return "LTRIM(RTRIM";
                case FunctionType.Substr:
                    return "SUBSTRING";
                default:
                    return base.GetFunctionName(functionType);
            }
        }

        /// <summary>
        /// Represents the SELECT keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="limit">The maximum number of objects to return.</param>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to select.</param>
        public override IQueryExpression<TSource> Select(int limit, params PropertyExpression[] properties)
        {
            base.Command.SqlText += string.Format
                (" SELECT {0} {1} ", limit > 0 ? 
                    string.Format("TOP ({0})", limit) : 
                    null, 
                    properties != null && properties.Length > 0 ? 
                    GetSelectedProperties(properties) : 
                    GetSelectedProperties(typeof(TSource))).Replace("SELECT  ", "SELECT "
                );
            return this;
        }

        /// <summary>
        /// Represents the SELECT keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="distinct">Determines whether a distinct set should be returned.</param>
        /// <param name="limit">The maximum number of objects to return.</param>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to select.</param>
        public override IQueryExpression<TSource> Select(bool distinct, int limit, params PropertyExpression[] properties)
        {
            base.Command.SqlText += string.Format
                (" SELECT {0} {1} {2} ", distinct ? 
                    "DISTINCT" : 
                    null, 
                    limit > 0 ? 
                    string.Format("TOP ({0})", limit) : 
                    null, 
                    properties != null && properties.Length > 0 ? 
                    GetSelectedProperties(properties) : 
                    GetSelectedProperties(typeof(TSource))).Replace("SELECT  ", "SELECT "
                );
            return this;
        }

        /// <summary>
        /// Returns an instance of an <see cref="IStorageCommand"/> IExecutableSql object.
        /// </summary>
        public override IStorageCommand ToCommand()
        {
            if (this.Pagination != null)
            {
                throw new NotSupportedException(string.Format("Paging is not currently supported by the {0}.", this.GetType().Name));
            }
            return base.ToCommand();
        }
    }
}
