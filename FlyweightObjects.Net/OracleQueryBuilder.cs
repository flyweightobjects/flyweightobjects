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
	/// Represents a query builder to build Oracle specific queries.
	/// </summary>
	[Serializable]
    public class OracleQueryBuilder<TSource> : QueryBuilderBase<TSource> where TSource : class, IFlyweight, new()
	{
		private int _limitValue = 0;

        /// <summary>
		/// Gets the prefix characters to prepend to a parameter.
		/// </summary>
		public override string ParameterPrefix
		{
			get { return OracleStorageProvider.GetParameterPrefix(); }
		}

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="provider">The associated <see cref="IStorageProvider"/>.</param>
        public OracleQueryBuilder(IStorageProvider provider)
            : base(provider)
        {

        }

        /// <summary>
        /// Represents the SELECT keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to select.</param>
		public override IQueryExpression<TSource> Select(params PropertyExpression[] properties)
        {
            PrepareInlineView();
            return base.Select(properties);
        }

        /// <summary>
        /// Represents the SELECT keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="distinct">Determines whether a distinct set should be returned.</param>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to select.</param>
        public override IQueryExpression<TSource> Select(bool distinct, params PropertyExpression[] properties)
        {
            PrepareInlineView();
            return base.Select(distinct, properties);
        }
        
        /// <summary>
        /// Represents the SELECT keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="limit">The maximum number of objects to return.</param>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to select.</param>
        public override IQueryExpression<TSource> Select(int limit, params PropertyExpression[] properties)
        {
            _limitValue = limit;
            PrepareInlineView();
            base.Select(properties);
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
            _limitValue = limit;
            PrepareInlineView();
            base.Select(distinct, properties);
            return this;
        }

        private void PrepareInlineView()
        {
            if (typeof(TSource) != typeof(QueryResult))
            {
                DataTableAttribute entity = DataAttributeUtilities.GetDataTableAttribute(typeof(TSource).GetGenericTypeParameter());
                if (entity != null && base.Command.SqlText.Contains("UPDATE "))
                {
                    base.Command.SqlText = base.Command.SqlText.Replace(string.Format("UPDATE {0}", entity.TableName), "UPDATE (");
                }
            }
        }

        /// <summary>
        /// Represents the INSERT INTO keywords of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to be inserted.</param>
        public override IQueryExpression<TSource> Insert(params PropertyExpression<TSource>[] properties)
        {
            throw new NotSupportedException(string.Format("Direct inserts using a {0} is not supported by the {1}.", typeof(QueryExpression<TSource>).Name, typeof(OracleQueryBuilder<TSource>).Name));
        }

		/// <summary>
		/// Represents the SET keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		/// <param name="properties">An array of generic <see cref="PropertyExpression"/> members whose values should be set.</param>
		public override IQueryExpression<TSource> Set(params PropertyExpression<TSource>[] properties)
		{
			base.Command.SqlText += " SET ";
			foreach (PropertyExpression property in properties)
			{
				if (!property.IsEmpty)
				{
                    if (property.ArgumentData is PropertyExpression<TSource>)
                    {
                        PropertyExpression<TSource> propertyExpr = (PropertyExpression<TSource>)property.ArgumentData;
                        if (!string.IsNullOrEmpty(propertyExpr.OperatorExpression))
                        {
                            base.Command.SqlText += string.Format("{0} = {1} {2}, ", property.DataColumnName, propertyExpr.DataColumnName, propertyExpr.OperatorExpression);
                        }
                        else
                        {
                            base.Command.SqlText += string.Format("{0} = {1}, ", property.DataColumnName, propertyExpr.DataColumnName);
                        }
                    }
                    else
                    {
                        Parameter param = CreateParameter(property);
                        param.Value = property.ArgumentData;
                        base.Command.SqlText += string.Format("{0} = {1}, ", property.DataColumnAttribute.ColumnName, param.Name);
                    }
				}
			}
			if (base.Command.SqlText.EndsWith(", "))
			{
				base.Command.SqlText = base.Command.SqlText.Remove(base.Command.SqlText.Length - 2, 2);
			}
			return this;
		}

		/// <summary>
		/// Returns an instance of an <see cref="IStorageCommand"/>.
		/// </summary>
        public override IStorageCommand ToCommand()
        {
            if (this.Pagination != null)
            {
                throw new NotSupportedException(string.Format("Paging is not currently supported by the {0}.", this.GetType().Name));
            }
            IStorageCommand command = base.ToCommand();
            if (_limitValue > 0)
            {
                if (command.SqlText.Contains("WHERE (") && command.TransactionType != TransactionType.Update)
                {
                    command.SqlText = command.SqlText.Replace("WHERE (", string.Format("WHERE (ROWNUM <= {0} AND ", _limitValue));
                }
                else if (command.SqlText.Contains("GROUP BY"))
                {
                    command.SqlText = command.SqlText.Insert(command.SqlText.IndexOf("GROUP BY") - 1, string.Format(" WHERE (ROWNUM <= {0})", _limitValue));
                }
                else if (command.SqlText.Contains("ORDER BY"))
                {
                    command.SqlText = command.SqlText.Insert(command.SqlText.IndexOf("ORDER BY") - 1, string.Format(" WHERE (ROWNUM <= {0})", _limitValue));
                }
                else
                {
                    command.SqlText += string.Format(" WHERE (ROWNUM <= {0})", _limitValue);
                }
            }
            if (command.SqlText.Contains("UPDATE ( SELECT"))
            {
                command.SqlText = command.SqlText.Replace("UPDATE ( SELECT", "UPDATE (SELECT");
            }
            if (command.SqlText.Contains("LEN("))
            {
                command.SqlText = command.SqlText.Replace("LEN(", "LENGTH(");
            }
            command.SqlText.Trim();
            return command;
        }
	}
}
