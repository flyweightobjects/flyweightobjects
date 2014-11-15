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
using System.Reflection;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections;

namespace FlyweightObjects
{
    /// <summary>
    /// Represents a join type for a <see cref="QueryExpression{T}"/>.
    /// </summary>
    public enum JoinType
    {
        /// <summary>
        /// An INNER JOIN expression.
        /// </summary>
        InnerJoin,
        /// <summary>
        /// A LEFT OUTER JOIN expression.
        /// </summary>
        LeftOuterJoin,
        /// <summary>
        /// A RIGHT OUTER JOIN expression.
        /// </summary>
        RightOuterJoin,
        /// <summary>
        /// A FULL OUTER JOIN expression.
        /// </summary>
        FullOuterJoin,
        /// <summary>
        /// A CROSS JOIN expression.
        /// </summary>
        CrossJoin
    }

    /// <summary>
    /// Represents a query builder to build relational database specific queries.
    /// </summary>
    [Serializable]
    public abstract class QueryBuilderBase<TSource> : IQueryBuilder<TSource> where TSource : class, IFlyweight, new()
    {
        private static Dictionary<Type, string> _selectStatementCache = new Dictionary<Type, string>();
        private List<PropertyExpression> _selectedProperties = new List<PropertyExpression>();
        private IStorageCommand _command = new StorageCommand();
        private Queue<RuntimeMethod> _methodQueue = new Queue<RuntimeMethod>();
        private static object _syncLock = new object();
        private IStorageProvider _provider = null;
        private Pagination _pagination = null;

        /// <summary>
        /// Gets or sets the <see cref="Pagination"/> for the <see cref="IQueryExpression{TSource}"/>.
        /// </summary>
        public Pagination Pagination 
        {
            get { return _pagination; }
            set { _pagination = value; }
        }

        /// <summary>
        /// Gets the <see cref="TransactionType"/> for the query.
        /// </summary>
        public TransactionType TransactionType { get; protected set; }
        
        /// <summary>
        /// Gets or sets the collection of <see cref="PropertyExpression"/> found in the query.
        /// </summary>
        protected List<PropertyExpression> SelectedProperties
        {
            get { return _selectedProperties; }
            set { _selectedProperties = value; }
        }

        /// <summary>
        /// Gets a cache of Select statements for the given Type.
        /// </summary>
        protected Dictionary<Type, string> SelectStatementCache
        {
            get { return _selectStatementCache; }
        }

        /// <summary>
        /// Gets a generic Queue of <see cref="RuntimeMethod"/> representing the methods called to construct a query.
        /// </summary>
        public Queue<RuntimeMethod> MethodQueue
        {
            get { return _methodQueue; }
            private set { _methodQueue = value; }
        }

        /// <summary>
        /// Gets the <see cref="IStorageCommand"/> created by an <see cref="IQueryBuilder{TSource}"/> instance after interpreting the call stack.
        /// </summary>
        public IStorageCommand Command
        {
            get { return _command; }
            set { _command = value; }
        }

        /// <summary>
        /// Gets the <see cref="IStorageProvider"/> associated with this <see cref="IQueryBuilder{TSource}"/>.
        /// </summary>
        protected IStorageProvider StorageProvider
        {
            get { return _provider; }
        }

        /// <summary>
        /// Gets the prefix characters to prepend to a parameter.
        /// </summary>
        public abstract string ParameterPrefix { get; }

        /// <summary>
        /// Represents the SELECT keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to select.</param>
        public virtual IQueryExpression<TSource> Select(params PropertyExpression[] properties)
        {
			_command.SqlText += string.Format(" SELECT {0} ", properties != null && properties.Length > 0 ? GetSelectedProperties(properties) : GetSelectedProperties(typeof(TSource))).Replace("SELECT  ", "SELECT ");
            return this;
        }
        
        /// <summary>
        /// Represents the SELECT keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="limit">The maximum number of objects to return.</param>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to select.</param>
        public abstract IQueryExpression<TSource> Select(int limit, params PropertyExpression[] properties);

        /// <summary>
        /// Represents the SELECT keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="distinct">Determines whether a distinct set should be returned.</param>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to select.</param>
        public virtual IQueryExpression<TSource> Select(bool distinct, params PropertyExpression[] properties)
        {
            _command.SqlText += string.Format(" SELECT {0} {1} ", distinct ? "DISTINCT" : null, properties != null && properties.Length > 0 ? GetSelectedProperties(properties) : GetSelectedProperties(typeof(TSource))).Replace("SELECT  ", "SELECT ");
            return this;
        }

        /// <summary>
        /// Represents the SELECT keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="distinct">Determines whether a distinct set should be returned.</param>
        /// <param name="limit">The maximum number of objects to return.</param>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to select.</param>
        public abstract IQueryExpression<TSource> Select(bool distinct, int limit, params PropertyExpression[] properties);
        
        /// <summary>
        /// Constructs a new instance of the class.
        /// </summary>
        /// <param name="provider">The <see cref="IStorageProvider"/> for the domain.</param>
        public QueryBuilderBase(IStorageProvider provider)
        {
            _provider = provider;
            _command.Name = Guid.NewGuid().ToString().ToUpper();
        }

        /// <summary>
        /// Gets a proper function name by <see cref="FunctionType"/>.
        /// </summary>
        /// <param name="functionType">The type of supported function.</param>
        public virtual string GetFunctionName(FunctionType functionType)
        {
            return functionType.ToString().ToUpper();
        }

        /// <summary>
        /// Represents the INSERT INTO keywords of a Data Manipulation Language (DML) statement.
        /// </summary>
        public virtual IQueryExpression<TSource> Insert()
        {
            if (typeof(TSource) == typeof(QueryResult))
            {
                throw new InvalidOperationException(string.Format("Cannot perform an insert operation using a {0} as the generic type parameter.", typeof(QueryResult).FullName));
            }
            string entityName = DataAttributeUtilities.GetDataTableAttribute(typeof(TSource).GetGenericTypeParameter()).TableName;
            _command.SqlText += string.Format(" INSERT INTO {0}", entityName);
            return this;
        }

        /// <summary>
        /// Represents the INSERT INTO keywords of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members to be inserted.</param>
        public virtual IQueryExpression<TSource> Insert(params PropertyExpression<TSource>[] properties)
        {
            if (typeof(TSource) == typeof(QueryResult))
            {
                throw new InvalidOperationException(string.Format("Cannot perform an insert operation using a {0} as the generic type parameter.", typeof(QueryResult).FullName));
            }
            if (properties != null && properties.Length > 0)
            {
                string entityName = DataAttributeUtilities.GetDataTableAttribute(typeof(TSource).GetGenericTypeParameter()).TableName;
                string[] strings = Array.ConvertAll<PropertyExpression<TSource>, string>(properties, delegate(PropertyExpression<TSource> obj)
                {
                    return obj.DataColumnAttribute.ColumnName;
                });
                foreach (PropertyExpression<TSource> property in properties)
                {
                    this.CreateParameter(property);
                }
                _command.SqlText += string.Format(" INSERT INTO {0} ({1})", entityName, string.Join(", ", strings));
            }
            return this;
        }

        /// <summary>
        /// Represents the UPDATE keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        public virtual IQueryExpression<TSource> Update()
        {
            if (typeof(TSource) == typeof(QueryResult))
            {
                throw new InvalidOperationException(string.Format("Cannot perform an update operation using a {0} as the generic type parameter.", typeof(QueryResult).FullName));
            }
            string entityName = DataAttributeUtilities.GetDataTableAttribute(typeof(TSource).GetGenericTypeParameter()).TableName;
            _command.SqlText += string.Format(" UPDATE {0}", entityName);
            return this;
        }

        /// <summary>
        /// Represents the DELETE keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        public virtual IQueryExpression<TSource> Delete()
        {
            if (typeof(TSource) == typeof(QueryResult))
            {
                throw new InvalidOperationException(string.Format("Cannot perform a delete operation using a {0} as the generic type parameter.", typeof(QueryResult).FullName)); 
            }
            string entityName = DataAttributeUtilities.GetDataTableAttribute(typeof(TSource).GetGenericTypeParameter()).TableName;
            _command.SqlText += string.Format(" DELETE FROM {0}", entityName);
            return this;
        }

        /// <summary>
        /// Represents the FROM keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <typeparam name="T">The type to serve as the source of From.</typeparam>
        public virtual IQueryExpression<TSource> From<T>() where T : class, IFlyweight, new()
        {
            if (_command.TransactionType != TransactionType.Delete)
            {
                string entityName = DataAttributeUtilities.GetDataTableAttribute(typeof(T).GetGenericTypeParameter()).TableName;
                _command.SqlText = _command.SqlText.TrimEnd() + string.Format(" FROM {0}", entityName);
            }
            return this;
        }

        /// <summary>
        /// Represents the SET keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="properties">An array of generic <see cref="PropertyExpression{T}"/> members whose values should be set.</param>
        public virtual IQueryExpression<TSource> Set(params PropertyExpression<TSource>[] properties)
        {
            if (properties != null && properties.Length > 0)
            {
                _command.SqlText += " SET ";
                foreach (PropertyExpression<TSource> property in properties)
                {
                    if (!property.IsEmpty)
                    {
                        if (property.ArgumentData is PropertyExpression<TSource>)
                        {
                            PropertyExpression<TSource> propertyExpr = (PropertyExpression<TSource>)property.ArgumentData;
                            if (!string.IsNullOrEmpty(propertyExpr.OperatorExpression))
                            {
                                _command.SqlText += string.Format("{0} = {1} {2}, ", property.DataColumnName, propertyExpr.DataColumnName, propertyExpr.OperatorExpression);
                            }
                            else
                            {
                                _command.SqlText += string.Format("{0} = {1}, ", property.DataColumnName, propertyExpr.DataColumnName);
                            }
                        }
                        else
                        {
                            Parameter param = CreateParameter(property);
                            param.Value = property.ArgumentData;
                            _command.SqlText += string.Format("{0} = {1}, ", property.DataColumnName, param.Name);
                        }
                    }
                }
                if (_command.SqlText.EndsWith(", "))
                {
                    _command.SqlText = _command.SqlText.Remove(_command.SqlText.Length - 2, 2);
                }
            }
            return this;
        }

        /// <summary>
        /// Represents the VALUES keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="values">The values to be inserted.</param>
        public virtual IQueryExpression<TSource> Values(params object[] values)
        {
            if (!_command.SqlText.ToUpper().Contains("INSERT"))
            {
                throw new InvalidOperationException("Cannot call Values() prior to calling Insert<T>()");
            }
            if (values.Length != _command.Parameters.Count)
            {
                throw new Exception("The number of values must match the number of properties specified within Insert<T>().");
            }

            string val = string.Empty;
            for (int i = 0; i < values.Length; i++)
            {
                _command.Parameters[i].Value = values[i];
                val += string.Format("{0}, ", _command.Parameters[i].Name);
            }
            if (val.EndsWith(", "))
            {
                val = val.Remove(val.Length - 2, 2);
            }
            _command.SqlText += string.Format(" VALUES ({0})", val);
            return this;
        }

        /// <summary>
        /// Represents the INNER JOIN keywords of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <typeparam name="T">The type to join.</typeparam>
        /// <param name="joinExpression">A property expression in the form of A=B.</param>
        public virtual IQueryExpression<TSource> Join<T>(PropertyExpression joinExpression) where T : class, IFlyweight, new()
        {
            return this.JoinInternal<T>(JoinType.InnerJoin, joinExpression);
        }

        /// <summary>
        /// Represents the LEFT OUTER JOIN keywords of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <typeparam name="T">The type to join.</typeparam>
        /// <param name="joinExpression">A property expression in the form of A=B.</param>
        public virtual IQueryExpression<TSource> LeftJoin<T>(PropertyExpression joinExpression) where T : class, IFlyweight, new()
        {
            return this.JoinInternal<T>(JoinType.LeftOuterJoin, joinExpression);
        }

        /// <summary>
        /// Represents the RIGHT OUTER JOIN keywords of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="joinExpression">A property expression in the form of A=B.</param>
        public virtual IQueryExpression<TSource> RightJoin<T>(PropertyExpression joinExpression) where T : class, IFlyweight, new()
        {
            return this.JoinInternal<T>(JoinType.RightOuterJoin, joinExpression);
        }

        /// <summary>
        /// Represents the FULL OUTER JOIN keywords of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <typeparam name="T">The type to join.</typeparam>
        /// <param name="joinExpression">A property expression in the form of A=B.</param>
        public virtual IQueryExpression<TSource> FullJoin<T>(PropertyExpression joinExpression) where T : class, IFlyweight, new()
        {
            return this.JoinInternal<T>(JoinType.FullOuterJoin, joinExpression);
        }

        /// <summary>
        /// Represents the CROSS JOIN keywords of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <typeparam name="T">The type to join.</typeparam>
        public virtual IQueryExpression<TSource> CrossJoin<T>() where T : class, IFlyweight, new()
        {
            return this.JoinInternal<T>(JoinType.CrossJoin, PropertyExpression<T>.Empty);
        }

        private IQueryExpression<TSource> JoinInternal<T>(JoinType joinType, PropertyExpression joinExpression)
        {
            string joinTypeExpr = GetJoinTypeExpression(joinType);
            string entityName = DataAttributeUtilities.GetDataTableAttribute(typeof(T).GetGenericTypeParameter()).TableName;
            string joinClause = string.Empty;
            if (joinType != JoinType.CrossJoin)
            {
                joinClause = string.Format(" {0} {1} ON ", joinTypeExpr, entityName);
            }
            else
            {
                _command.SqlText = string.Format("{0} {1} {2}", _command.SqlText.Trim(), joinTypeExpr, entityName);
                return this;
            }
            switch (joinExpression.ExpressionType)
            {
                case PropertyExpressionType.Join:
                    joinClause += GetJoinExpression(joinExpression);
                    break;
                case PropertyExpressionType.Operator:
                    joinClause += GetOperatorExpression(joinExpression);
                    break;
                case PropertyExpressionType.Function:
                    joinClause += GetFunctionExpression(joinExpression);
                    break;
                default:
                    break;
            }
            _command.SqlText += joinClause;
            foreach (PropertyExpression expr in joinExpression.ChildExpressions)
            {
                string funcExpr = this.GetFunctionExpression(expr);
                string fieldExpr = !string.IsNullOrEmpty(funcExpr) ? funcExpr : expr.DataColumnName;
                if (expr.ArgumentData is PropertyExpression)
                {
                    _command.SqlText += string.Format(" {0} {1} = {2}", expr.LogicalExpression, fieldExpr, ((PropertyExpression)expr.ArgumentData).DataColumnName);
                    if (!string.IsNullOrEmpty(((PropertyExpression)expr.ArgumentData).OperatorExpression))
                    {
                        _command.SqlText += string.Format(" {0}", ((PropertyExpression)expr.ArgumentData).OperatorExpression);
                    }
                }
                else if (expr.ArgumentData == null && !string.IsNullOrEmpty(expr.OperatorExpression))
                {
                    _command.SqlText += string.Format(" {0} {1} {2} {3}", expr.LogicalExpression, fieldExpr, expr.OperatorExpression, "NULL");
                }
                else if (expr.ArgumentData.GetType() != typeof(string) && expr.ArgumentData.ToString().IsNumeric())
                {
                    _command.SqlText += string.Format(" {0} {1} {2} {3}", expr.LogicalExpression, fieldExpr, expr.OperatorExpression, expr.ArgumentData);
                }
                else
                {
                    _command.SqlText += string.Format(" {0} {1} {2}", expr.LogicalExpression, fieldExpr, GetOperatorExpression(expr).Replace(fieldExpr, string.Empty).Trim());
                }
            }
            return this;
        }

        /// <summary>
        /// Represents the GROUP BY keywords of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members by which to group.</param>
        public virtual IQueryExpression<TSource> GroupBy(params PropertyExpression[] properties)
        {
            if (properties != null && properties.Length > 0)
            {
                _command.SqlText += " GROUP BY ";
                foreach (PropertyExpression property in properties)
                {
                    if (!property.IsEmpty)
                    {
                        string funcExpr = this.GetFunctionExpression(property);
                        string fieldExpr = !string.IsNullOrEmpty(funcExpr) ? funcExpr : property.DataColumnName;
                        _command.SqlText += string.Format("{0}, ", fieldExpr);
                    }
                }
                if (_command.SqlText.EndsWith(", "))
                {
                    _command.SqlText = _command.SqlText.Remove(_command.SqlText.Length - 2, 2);
                }
            }
            return this;
        }

        /// <summary>
        /// Represents the Group By All keywords of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> members by which to group.</param>
        public virtual IQueryExpression<TSource> GroupByAll(params PropertyExpression[] properties)
        {
            if (properties != null && properties.Length > 0)
            {
                _command.SqlText += " GROUP BY ALL ";
                foreach (PropertyExpression property in properties)
                {
                    if (!property.IsEmpty)
                    {
                        string funcExpr = this.GetFunctionExpression(property);
                        string fieldExpr = !string.IsNullOrEmpty(funcExpr) ? funcExpr : property.DataColumnName;
                        _command.SqlText += string.Format("{0}, ", fieldExpr);
                    }
                }
                if (_command.SqlText.EndsWith(", "))
                {
                    _command.SqlText = _command.SqlText.Remove(_command.SqlText.Length - 2, 2);
                }
            }
            return this;
        }

        /// <summary>
        /// Represents the HAVING keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="havingExpression">The expression which limits an aggregate function.</param>
        public virtual IQueryExpression<TSource> Having(PropertyExpression havingExpression)
        {
            if (!havingExpression.IsEmpty)
            {
                if (!_command.SqlText.Contains("HAVING"))
                {
                    _command.SqlText += " HAVING";
                }
                if (!string.IsNullOrEmpty(havingExpression.LogicalExpression))
                {
                    _command.SqlText += string.Format(" {0}", havingExpression.LogicalExpression);
                }
                string having = GetSelectedProperties(havingExpression);
                _command.SqlText += string.Format(" {0} {1}", having, havingExpression.ArgumentData);
                foreach (PropertyExpression property in havingExpression.ChildExpressions)
                {
                    Having(property);
                }
            }
            return this;
        }

        /// <summary>
        /// Represents the UNION keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        public virtual IQueryExpression<TSource> Union()
        {
            _command.SqlText += " UNION ";
            return this;
        }

        /// <summary>
        /// Represents the Union All keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        public virtual IQueryExpression<TSource> UnionAll()
        {
            _command.SqlText += " UNION ALL ";
            return this;
        }

        /// <summary>
        /// Represents the WHERE keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="whereExpression">The crtieria used to limit the results of the query.</param>
        public virtual IQueryExpression<TSource> Where(PropertyExpression whereExpression)
        {
            Where(whereExpression, false);
            while (_command.SqlText.Split("(".ToCharArray()).Length > _command.SqlText.Split(")".ToCharArray()).Length)
            {
                _command.SqlText += ")";
            }
            return this;
        }

        /// <summary>
        /// Represents the WHERE keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="whereExpression">The crtieria used to limit the results of the query.</param>
        /// <param name="recursive">Determines whether the method has been called by itself.</param>
        protected virtual IQueryExpression<TSource> Where(PropertyExpression whereExpression, bool recursive)
        {
            if (whereExpression.IsEmpty || whereExpression.Name.ToString() == "*")
            {
                return this;
            }
            string whereExpr = string.Empty;
            if (!_command.SqlText.Contains("WHERE (") || (_command.SqlText.Contains("UNION") && !(_command.SqlText.Contains("OR (") || _command.SqlText.Contains("AND ("))))
            {
                whereExpr += string.Format(" WHERE (");
            }
            if (!string.IsNullOrEmpty(whereExpression.UnaryExpression))
            {
                whereExpr += string.Format("{0} (", whereExpression.UnaryExpression);
            }
            if (!string.IsNullOrEmpty(whereExpression.LogicalExpression))
            {
				whereExpr += string.Format(" {0} ", whereExpression.LogicalExpression.TrimNull());
            }
            if (whereExpression.Functions.Count > 0)
            {
                string funcExpr = this.GetFunctionExpression(whereExpression);
                whereExpr += funcExpr;
            }
			string operatorExpr = GetOperatorExpression(whereExpression);
			whereExpr += operatorExpr;
			_command.SqlText = string.Format("{0} {1}", _command.SqlText.TrimNull(), whereExpr.TrimNull());
            foreach (PropertyExpression property in whereExpression.ChildExpressions)
            {
                Where(property, true);
            }
            return this;
        }

        /// <summary>
        /// Represents the ORDER BY keywords of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="sortExpressions">An array of <see cref="PropertyExpression"/> members by which to sort.</param>
        public virtual IQueryExpression<TSource> OrderBy(params PropertyExpression[] sortExpressions)
        {
            if (sortExpressions != null && sortExpressions.Length > 0)
            {
                _command.SqlText += GetOrderByExpression(new List<PropertyExpression>(sortExpressions));
            }
            return this;
        }

        /// <summary>
        /// Returns a string representing the sorting expression of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="sortingExpressions">A collection of <see cref="PropertyExpression"/>s.</param>
        protected string GetOrderByExpression(List<PropertyExpression> sortingExpressions)
        {
            string retVal = string.Empty;
            foreach (PropertyExpression property in sortingExpressions)
            {
                if (!property.IsEmpty)
                {
                    if (!retVal.Contains(" ORDER BY "))
                    {
                        retVal = " ORDER BY ";
                    }
                    string funcExpr = this.GetFunctionExpression(property);
                    retVal += string.Format("{0} ", !string.IsNullOrEmpty(funcExpr) ? funcExpr : property.DataColumnName);
                    if (!string.IsNullOrEmpty(property.OperatorExpression))
                    {
                        retVal += string.Format("{0} ", property.OperatorExpression);
                    }
                    if (!string.IsNullOrEmpty(property.SortingExpression))
                    {
                        retVal += string.Format("{0}, ", property.SortingExpression);
                    }
                    else
                    {
                        retVal = retVal.TrimEnd() + ", ";
                    }
                }
            }
            if (retVal.EndsWith(", "))
            {
                retVal = retVal.Remove(retVal.Length - 2, 2);
            }
            return retVal;
        }
        /// <summary>
        /// Represents the ORDER BY keywords of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="ordinal">The ordinal postiion by which to sort.</param>
        public virtual IQueryExpression<TSource> OrderBy(int ordinal)
        {
            _command.SqlText += string.Format(" ORDER BY {0}", ordinal);
            return this;
        }

        /// <summary>
        /// Represents the ORDER BY keywords of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="ordinals">An array of ordinals by which to sort.</param>
        public virtual IQueryExpression<TSource> OrderBy(params int[] ordinals)
        {
            _command.SqlText += string.Format(" ORDER BY ");
            foreach (int i in ordinals)
            {
                _command.SqlText += string.Format("{0}, ", i);
            }
            if (_command.SqlText.EndsWith(", "))
            {
                _command.SqlText = _command.SqlText.Remove(_command.SqlText.Length - 2, 2);
            }
            return this;
        }

        /// <summary>
        /// Represents the ORDER BY keywords of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="alias">The alias by which to sort.</param>
        public virtual IQueryExpression<TSource> OrderBy(string alias)
        {
            _command.SqlText += string.Format(" ORDER BY {0}", alias);
            return this;
        }

        /// <summary>
        /// Represents the DESC keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        public virtual IQueryExpression<TSource> Desc()
        {
            _command.SqlText += " DESC";
            return this;
        }

        /// <summary>
        /// Represents the ASC keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        public virtual IQueryExpression<TSource> Asc()
        {
            _command.SqlText += " ASC";
            return this;
        }

        /// <summary>
        /// Represents the AND keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="whereExpression">The crtieria used to limit the statement.</param>
        public virtual IQueryExpression<TSource> And(PropertyExpression whereExpression)
        {
            if (!whereExpression.IsEmpty)
            {
                _command.SqlText += string.Format(" AND (");
                Where(whereExpression);
            }
            return this;
        }

        /// <summary>
        /// Represents the OR keyword of a Data Manipulation Language (DML) statement.
        /// </summary>
        /// <param name="whereExpression">The crtieria used to limit the statement.</param>
        public virtual IQueryExpression<TSource> Or(PropertyExpression whereExpression)
        {
            if (!whereExpression.IsEmpty)
            {
                _command.SqlText += string.Format(" OR (");
                Where(whereExpression);
            }
            return this;
        }

        /// <summary>
        /// Returns an instance of an <see cref="IStorageCommand"/>.
        /// </summary>
        public virtual IStorageCommand ToCommand()
        {
            _command.SqlText = _command.SqlText.Trim();
            return _command;
        }

        /// <summary>
        /// Returns a comma-separated string of selected properties.
        /// </summary>
        /// <param name="type">The type to interrogate.</param>
        protected virtual string GetSelectedProperties(Type type)
        {
            StringBuilder sb = new StringBuilder();
            lock (_syncLock)
            {
                if (this.SelectStatementCache.ContainsKey(type))
                {
                    sb.Append(this.SelectStatementCache[type]);
                }
                else
                {
                    DataTableAttribute dataTable = DataAttributeUtilities.GetDataTableAttribute(type);
                    List<DataColumnAttribute> dataColumns = DataAttributeUtilities.GetDataColumnAttributes(type);
                    if (string.IsNullOrEmpty(dataTable.TableName) || dataColumns.Count == 0)
                    {
                        sb.Append("*");
                    }
                    else
                    {
                        foreach (DataColumnAttribute dataColumn in dataColumns)
                        {
                            sb.Append(string.Format("{0}.{1}", dataTable.TableName, dataColumn.ColumnName));
                            if (dataColumns.IndexOf(dataColumn) < dataColumns.Count - 1)
                            {
                                sb.Append(", ");
                            }
                        }
                    }
                    this.SelectStatementCache.Add(type, sb.ToString());
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns a comma-separated string of selected properties.
        /// </summary>
        /// <param name="properties">An array of <see cref="PropertyExpression"/> objects.</param>
        protected virtual string GetSelectedProperties(params PropertyExpression[] properties)
        {
            ValidateSelection(properties);
            string retVal = string.Empty;
            foreach (PropertyExpression property in properties)
            {
                if (!property.IsEmpty)
                {
                    _selectedProperties.Add(property);
                    if (property.Functions.Count > 0)
                    {
                        string funcExpr = this.GetFunctionExpression(property);
                        retVal += funcExpr;
                    }
					else if (!(property is ILiteral))
					{
						retVal += string.Format("{0} ", property.DataColumnName);
					}
					else
					{
						if (((ILiteral)property).Constant == null)
						{
							retVal += "NULL ";
						}
						else
						{
							retVal += string.Format("'{0}' ", ((ILiteral)property).Constant);
						}
					}
                    if (!string.IsNullOrEmpty(property.OperatorExpression))
                    {
                        retVal += string.Format("{0} ", property.OperatorExpression);
                    }
                    if (!string.IsNullOrEmpty(property.PropertyAlias))
                    {
                        retVal = retVal.Trim();
                        retVal += string.Format(" AS {0} ", property.PropertyAlias);
                    }
                    retVal = retVal.TrimEnd() + ", ";
                }
            }
            if (retVal.EndsWith(", "))
            {
                retVal = retVal.Remove(retVal.Length - 2, 2);
            }
            return retVal;
        }

        /// <summary>
        /// Returns a join type string expression for the supplied <see cref="JoinType"/>.
        /// </summary>
        /// <param name="joinType">The <see cref="JoinType"/> to express as a string.</param>
        protected virtual string GetJoinTypeExpression(JoinType joinType)
        {
            string joinTypeExpr = string.Empty;
            switch (joinType)
            {
                default:
                case JoinType.InnerJoin: joinTypeExpr = "INNER JOIN";
                    break;
                case JoinType.LeftOuterJoin: joinTypeExpr = "LEFT OUTER JOIN";
                    break;
                case JoinType.RightOuterJoin: joinTypeExpr = "RIGHT OUTER JOIN";
                    break;
                case JoinType.FullOuterJoin: joinTypeExpr = "FULL OUTER JOIN";
                    break;
                case JoinType.CrossJoin: joinTypeExpr = "CROSS JOIN";
                    break;
            }
            return joinTypeExpr;
        }

        /// <summary>
        /// Retrieves a string representing the join expression.
        /// </summary>
        /// <param name="joinExpression">The <see cref="PropertyExpression"/> representing the join.</param>
        protected virtual string GetJoinExpression(PropertyExpression joinExpression)
        {
            if (joinExpression.ExpressionType != PropertyExpressionType.Join)
            {
                throw new InvalidOperationException("The expression must represent a join expression between two entities.");
            }
            PropertyExpression joinedProperty = (PropertyExpression)joinExpression.ArgumentData;
            string joinExpr = string.Format("{0} {1} {2}", joinExpression.DataColumnName, joinExpression.OperatorExpression, joinedProperty.DataColumnName);
            if (!string.IsNullOrEmpty(joinedProperty.OperatorExpression))
            {
                joinExpr += string.Format(" {0}", joinedProperty.OperatorExpression);
            }
            return joinExpr;
        }

        /// <summary>
        /// Returns the composited function expression represented as a string.
        /// </summary>
        /// <param name="expression">The <see cref="PropertyExpression"/> containing the function.</param>
        protected virtual string GetFunctionExpression(PropertyExpression expression)
        {
            string field = expression.DataColumnName.Contains("*") ? "*" : expression.DataColumnName;
            string funcExpr = string.Empty;
            foreach (Function function in expression.Functions)
            {
                string funcName = function.FunctionType == FunctionType.CountDistinct ? "COUNT(DISTINCT " : this.GetFunctionName(function.FunctionType);
                if (expression.Functions.IndexOf(function) == 0)
                {
                    if (function.FunctionType == FunctionType.CountDistinct)
                    {
                        funcExpr = funcExpr.Insert(0, string.Format("{0}{1}", funcName, field));
                    }
                    else
                    {
                        funcExpr = funcExpr.Insert(0, string.Format("{0}({1}", funcName, field));
                    }
                }
                else
                {
                    funcExpr = funcExpr.Insert(0, string.Format("{0}(", funcName));
                }
                foreach (var item in function.Parameters)
                {
                    funcExpr += string.Format(", {0}", item);
                }
                foreach (Char c in this.GetFunctionName(function.FunctionType))
                {
                    if (c == '(')
                    {
                        funcExpr += ")";
                    }
                }
                funcExpr += ")";
            }
            return funcExpr;
        }

        /// <summary>
        /// Returns the operator expression represented as a string.
        /// </summary>
        /// <param name="expression"></param>
        protected virtual string GetOperatorExpression(PropertyExpression expression)
        {
            string expr = string.Empty;
            switch (RelationalOperator.Parse(expression.OperatorExpression))
            {
                case RelationalOperator.Between:
                case RelationalOperator.NotBetween:
                    {
                        object[] values = expression.ArgumentData as object[];
                        Parameter param1 = CreateParameter(expression);
                        param1.Value = values[0];
                        Parameter param2 = CreateParameter(expression);
                        param2.Value = values[1];
                        if (expression.Functions.Count == 0)
                        {
                            expr += string.Format("{0} {1} {2} AND {3}", expression.DataColumnName, expression.OperatorExpression, param1.Name, param2.Name);
                        }
                        else
                        {
                            expr += string.Format(" {0} {1} AND {2}", expression.OperatorExpression, param1.Name, param2.Name);
                        }
                        break;
                    }
                case RelationalOperator.In:
                case RelationalOperator.NotIn:
                    {
                        StringBuilder values = new StringBuilder();
                        foreach (object item in expression.ArgumentData as IEnumerable)
                        {
                            if (item.GetType().IsImplementationOf(typeof(IQueryExpression<>)))
                            {
                                if (item.GetType().IsImplementationOf(typeof(IRuntimeMethodQuery<>)))
                                {
                                    MethodInfo methodInfo = this.StorageProvider.GetType().GetMethod("BuildStorageCommand");
                                    Type parameterType = item.GetType().GetGenericArguments()[0];
                                    methodInfo =  methodInfo.MakeGenericMethod(new Type[] { parameterType });
                                    IStorageCommand command = methodInfo.Invoke(this.StorageProvider, new object[] { item }) as IStorageCommand;
                                    values.Append(command.SqlText);
                                    MergeParameters(command);
                                }
                                else
                                {
                                    MethodInfo methodInfo = item.GetType().GetMethod("ToCommand", new Type[0]);
                                    IStorageCommand command = methodInfo.Invoke(item, null) as IStorageCommand;
                                    values.Append(command.SqlText);
                                    MergeParameters(command);
                                }
                            }
                            else if (item is IStorageCommand)
                            {
                                IStorageCommand command = item as IStorageCommand;
                                values.Append(command.SqlText);
                                MergeParameters(command);
                            }
                            else
                            {
                                Parameter param = expression.Functions.Count == 0 ? CreateParameter(expression) : CreateParameter();
                                param.Value = item;
                                values.Append(string.Format("{0}, ", param.Name));
                            }
                        }
                        string paramNames = values.ToString().EndsWith(", ") ? values.ToString().Remove(values.Length - 2, 2) : values.ToString();
                        if (expression.Functions.Count == 0)
                        {
                            expr += string.Format("{0} {1} ({2})", expression.DataColumnName, expression.OperatorExpression, paramNames);
                        }
                        else
                        {
                            expr += string.Format(" {0} ({1})", expression.OperatorExpression, paramNames);
                        }
                        break;
                    }
                case RelationalOperator.IsNull:
                case RelationalOperator.IsNotNull:
                    {
						if (expression.Functions.Count == 0)
						{
							expr += string.Format("{0} {1}", expression.DataColumnName, expression.OperatorExpression);
						}
						else
						{
							expr += string.Format("{0}", expression.OperatorExpression);
						}
                        break;
                    }
                case RelationalOperator.Like:
                case RelationalOperator.NotLike:
                    {
                        if (expression.Functions.Count == 0)
                        {
                            Parameter param = CreateParameter(expression);
                            param.Value = expression.ArgumentData;
                            expr += string.Format("{0} {1} {2}", expression.DataColumnName, expression.OperatorExpression, param.Name);
                        }
                        else
                        {
                            Parameter param = CreateParameter();
                            param.Value = expression.ArgumentData;
                            expr += string.Format(" {0} {1}", expression.OperatorExpression, param.Name);
                        }
                        break;
                    }
                default:
                    {
                        if (expression.Functions.Count == 0)
                        {
                            Parameter param = CreateParameter(expression);
                            param.Value = expression.ArgumentData;
                            expr += string.Format("{0} {1} {2}", expression.DataColumnName, expression.OperatorExpression, param.Name);
                        }
                        else
                        {
                            Parameter param = CreateParameter();
                            param.Value = expression.ArgumentData;
                            expr += string.Format("{0} {1}", expression.OperatorExpression, param.Name);
                        }
                        break;
                    }
            }
            return expr;
        }

        /// <summary>
        /// Creates a <see cref="Parameter"/> object given the supplied expression.
        /// </summary>
        /// <param name="expression">The <see cref="PropertyExpression"/>.</param>
        protected virtual Parameter CreateParameter(PropertyExpression expression)
        {
            Parameter param = ((StorageCommand)_command).CreateParameter(this.ParameterPrefix, expression.DataColumnAttribute, DBNull.Value, ParameterDirection.Input);
            param.Type = expression.DataColumnAttribute.ColumnType;
            param.Size = expression.DataColumnAttribute.Size;
            return param;
        }

        /// <summary>
        /// Creates a <see cref="Parameter"/> object given the supplied expression.
        /// </summary>
        protected virtual Parameter CreateParameter()
        {
            Parameter param = new Parameter();
            param.Name = string.Format("{0}{1}", this.ParameterPrefix, _command.Parameters.Count + 1);
            _command.Parameters.Add(param);
            return param;
        }

        /// <summary>
        /// Iterates a <see cref="IRuntimeMethodQuery{TSource}"/> instance and returns an <see cref="IStorageCommand"/>.
        /// </summary>
        /// <param name="query">The query to be processed.</param>
        public IStorageCommand BuildStorageCommand(IRuntimeMethodQuery<TSource> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException();
            }
            _methodQueue = query.MethodQueue;
            _pagination = query.Pagination;
            foreach (RuntimeMethod runtimeMethod in this.MethodQueue)
            {
                string methodKey = ParseGenericArgs(runtimeMethod.MethodBase.ToString());
                MethodInfo methodInfo = Array.Find<MethodInfo>(this.GetType().GetMethods(), delegate(MethodInfo method)
                {
                    return methodKey == ParseGenericArgs(method.ToString());
                });
                if (methodInfo == null)
                {
                    throw new InvalidOperationException(string.Format("The {0} cannot find the method {1} in the supplied {2}.", this.GetType().Name, runtimeMethod.MethodBase, typeof(IQueryExpression<TSource>).Name));
                }
                if (methodInfo.ContainsGenericParameters)
                {
                    Type[] parameterTypes = runtimeMethod.TypeParameters.ToArray();
                    methodInfo = methodInfo.MakeGenericMethod(parameterTypes);
                }
                if (methodInfo.GetParameters().Length > 0)
                {
                    object[] parameters = runtimeMethod.MethodArguments.ToArray();
                    methodInfo.Invoke(this, parameters);
                }
                else
                {
                    methodInfo.Invoke(this, null);
                }
            }
            return this.ToCommand();
        }

        private void MergeParameters(IStorageCommand command)
        {
            if (this.Command.CommandType != CommandType.Text || command.CommandType != CommandType.Text)
            {
                throw new InvalidOperationException("Cannot merge commands for any CommandType other than CommandType.Text.");
            }
            foreach (Parameter item in command.Parameters)
            {
                Parameter parameter = this.Command.Parameters.OfType<Parameter>().Where(p => p.Name == item.Name).FirstOrDefault();
                if (parameter != null)
                {
                    string name = parameter.Name;
                    parameter.Name = string.Format("{0}{1}", this.ParameterPrefix, this.Command.Parameters.Count + 1);
                    this.Command.SqlText = this.Command.SqlText.Replace(name, parameter.Name);
                }
                this.Command.Parameters.Add(item);
            }
        }

        private string ParseGenericArgs(string value)
        {
            StringBuilder sb = new StringBuilder();
            string[] values = value.Split(' ');
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].Contains('[') && values[i].Contains(']'))
                {
                    int idx1 = values[i].IndexOf('[', 0);
                    int idx2 = values[i].IndexOf(']', idx1);
                    values[i] = values[i].Remove(idx1 + 1, (idx2 - idx1 - 1));
                }
                sb.Append(string.Format("{0} ", values[i].Trim()));
            }
            return sb.ToString().Trim();
        }

        private void ValidateSelection(PropertyExpression[] properties)
        {
            foreach (PropertyExpression property in properties)
            {
                if (property.DataTableAttribute != null && !property.DataTableAttribute.AllowSelect)
                {
                    throw new InvalidOperationException(string.Format("Cannot perform the requested operation because the {0}.AllowSelect property for one or more of the types in the select clause is set to false.", typeof(DataTableAttribute).Name));
                }
            }
        }

        /// <summary>
        /// Returns a String that represents the current Object.
        /// </summary>
        public override string ToString()
        {
            if (_command != null && _command.SqlText.Trim().Length > 0)
            {
                return _command.SqlText;
            }
            return base.ToString();
        }
    }
}