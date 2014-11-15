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
using System.Data;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents a query builder to build Microsoft Jet specific queries.
	/// </summary>
	[Serializable]
    public class MsJetQueryBuilder<TSource> : QueryBuilderBase<TSource> where TSource : class, IFlyweight, new()
	{
        /// <summary>
		/// Gets the prefix characters to prepend to a parameter.
		/// </summary>
		public override string ParameterPrefix
		{
			get { return MsJetStorageProvider.GetParameterPrefix(); }
		}

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="provider">The associated <see cref="IStorageProvider"/>.</param>
        public MsJetQueryBuilder(IStorageProvider provider)
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
				case FunctionType.Upper:
					return "UCASE";
				case FunctionType.Lower:
					return "LCASE";
				case FunctionType.Substr:
					return "MID";
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
			base.Command.SqlText += string.Format(" SELECT {0} {1} ", limit > 0 ? string.Format("TOP {0}", limit) : null, properties != null && properties.Length > 0 ? GetSelectedProperties(properties) : GetSelectedProperties(typeof(TSource))).Replace("SELECT  ", "SELECT ");
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
            base.Command.SqlText += string.Format(" SELECT {0} {1} {2} ", distinct ? "DISTINCT" : null, limit > 0 ? string.Format("TOP {0}", limit) : null, properties != null && properties.Length > 0 ? GetSelectedProperties(properties) : GetSelectedProperties(typeof(TSource)));
            return this;
        }

		/// <summary>
		/// Represents the INNER JOIN keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <typeparam name="T">The type to join.</typeparam>
		/// <param name="joinExpression">A property expression in the form of A=B.</param>
		public override IQueryExpression<TSource> Join<T>(PropertyExpression joinExpression)
		{
			if (base.Command.TransactionType == TransactionType.Select)
			{
				base.Command.SqlText = base.Command.SqlText.Replace(" FROM ", " FROM (");
				base.Join<T>(joinExpression);
				base.Command.SqlText += ")";
			}
			else
			{
				base.Join<T>(joinExpression);
			}
			return this;
		}

		/// <summary>
		/// Represents the Left Outer keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <typeparam name="T">The type to join.</typeparam>
		/// <param name="joinExpression">A property expression in the form of A=B.</param>
		public override IQueryExpression<TSource> LeftJoin<T>(PropertyExpression joinExpression)
		{
			if (base.Command.TransactionType == TransactionType.Select)
			{
				base.Command.SqlText = base.Command.SqlText.Replace(" FROM ", " FROM (");
				base.LeftJoin<T>(joinExpression);
				base.Command.SqlText += ")";
			}
			else
			{
				base.LeftJoin<T>(joinExpression);
			}
			return this;
		}

		/// <summary>
		/// Represents the RIGHT OUTER JOIN keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <typeparam name="T">The type to join.</typeparam>
		/// <param name="joinExpression">A property expression in the form of A=B.</param>
		public override IQueryExpression<TSource> RightJoin<T>(PropertyExpression joinExpression)
		{
			if (base.Command.TransactionType == TransactionType.Select)
			{
				base.Command.SqlText = base.Command.SqlText.Replace(" FROM ", " FROM (");
				base.RightJoin<T>(joinExpression);
				base.Command.SqlText += ")";
			}
			else
			{
				base.RightJoin<T>(joinExpression);
			}
			return this;
		}

		/// <summary>
		/// Represents the FULL OUTER JOIN keywords of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <typeparam name="T">The type to join.</typeparam>
		/// <param name="joinExpression">A property expression in the form of A=B.</param>
		public override IQueryExpression<TSource> FullJoin<T>(PropertyExpression joinExpression)
		{
			if (base.Command.TransactionType == TransactionType.Select)
			{
				base.Command.SqlText = base.Command.SqlText.Replace(" FROM ", " FROM (");
				base.FullJoin<T>(joinExpression);
				base.Command.SqlText += ")";
			}
			else
			{
				base.FullJoin<T>(joinExpression);
			}
			return this;
		}

        /// <summary>
        /// Represents the CROSS JOIN keywords of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <typeparam name="T">The type to join.</typeparam>
        public override IQueryExpression<TSource> CrossJoin<T>()
        {
            string entityName = DataAttributeUtilities.GetDataTableAttribute(typeof(T).GetGenericTypeParameter()).TableName;
            base.Command.SqlText = string.Format("{0}, {1}", base.Command.SqlText.TrimEnd(), entityName);
            return this;
        }

		/// <summary>
		/// Not supported.
		/// </summary>
		public override IQueryExpression<TSource> Union()
		{
			throw new NotSupportedException(string.Format("The UNION keyword is not supported by the {0}.", this.GetType().FullName));
		}

		/// <summary>
		/// Not supported.
		/// </summary>
		public override IQueryExpression<TSource> UnionAll()
		{
			throw new NotSupportedException(string.Format("The UNION ALL keyword is not supported by the {0}.", this.GetType().FullName));
		}

        /// <summary>
        /// Returns an instance of an <see cref="IStorageCommand"/>.
        /// </summary>
        public override IStorageCommand ToCommand()
        {
            if (this.Pagination != null)
            {
                throw new NotSupportedException(string.Format("Paging is not supported by the {0}.", this.GetType().Name));
            }
            return base.ToCommand();
        }
	}
}
