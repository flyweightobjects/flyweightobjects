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
using System.Data;

namespace FlyweightObjects
{
	/// <summary>
	/// Represents a query builder to build MySQL specific queries.
	/// </summary>
	[Serializable]
    public class MySqlQueryBuilder<TSource> : QueryBuilderBase<TSource> where TSource : class, IFlyweight, new()
	{
		private int _limitValue;

        /// <summary>
		/// Gets the prefix characters to prepend to a parameter.
		/// </summary>
		public override string ParameterPrefix
		{
			get { return MySqlStorageProvider.GetParameterPrefix(); }
		}

        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="provider">The associated <see cref="IStorageProvider"/>.</param>
        public MySqlQueryBuilder(IStorageProvider provider)
            : base(provider)
        {

        }

        /// <summary>
        /// Represents the SELECT keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="limit">The maximum number of objects to return.</param>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to select.</param>
        public override IQueryExpression<TSource> Select(int limit, params PropertyExpression[] properties)
        {
            base.Select(properties);                            
            _limitValue = limit;
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
            base.Select(distinct, properties);
            _limitValue = limit;
			return this;
		}

		/// <summary>
		/// Represents the DELETE keyword of a Data Manipulation Language (DML) statement.
		/// </summary>
		public override IQueryExpression<TSource> Delete()
		{
			string entityName = DataAttributeUtilities.GetDataTableAttribute(typeof(TSource).GetGenericTypeParameter()).TableName;
			base.Command.SqlText += string.Format(" DELETE FROM {0}", entityName);
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
			if (command.SqlText.Contains("LEN("))
			{
				command.SqlText = command.SqlText.Replace("LEN(", "LENGTH(");
			}
			if (_limitValue > 0)
			{
				command.SqlText += string.Format(" LIMIT {0}", _limitValue);
			}
			return command;
		}
	}
}