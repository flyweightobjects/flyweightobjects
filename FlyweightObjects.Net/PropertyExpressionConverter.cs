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
using System.Reflection;
using System.Collections;
using System.ComponentModel;

namespace FlyweightObjects
{
	/// <summary>
	/// Converts an <see cref="Expression"/> to a <see cref="ExpressionConverter{T}"/>
	/// </summary>
	public static class ExpressionConverter<T> where T : class, IFlyweight, new()
    {
	    /// <summary>
		/// Converts the supplied <see cref="Expression"/> to a <see cref="ExpressionConverter{T}"/>
	    /// </summary>
	    /// <param name="expression"></param>
	    /// <returns></returns>
		public static PropertyExpression<T> Convert(Expression expression)
	    {
		    return new PropertyExpressionConverter<T>().Convert(expression);
	    }
    }
	
	internal class PropertyExpressionConverter<T> : ExpressionVisitor where T : class, IFlyweight, new()
    {
        private PropertyExpression<T> _propertyExpression;

        public PropertyExpression<T> Convert(Expression expression)
        {
            _propertyExpression = PropertyExpression<T>.Empty;
            if (expression != null)
            {
                this.Visit(ExpressionEvaluator.PartialEval(expression));
                this.ShiftLogicalExpressions();
            }
            return _propertyExpression;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            m = base.VisitMemberAccess(m) as MemberExpression;
            PropertyExpression<T> propertyExpr = this.GetPropertyExpression(m.Member);
            return m;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            this.Visit(b.Left);
            switch (b.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    this.GetPropertyExpression().OperatorExpression = ArithmeticOperator.Add;
                    break;
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    this.GetPropertyExpression().OperatorExpression = ArithmeticOperator.Subtract;
                    break;
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    this.GetPropertyExpression().OperatorExpression = ArithmeticOperator.Multiply;
                    break;
                case ExpressionType.Divide:
                    this.GetPropertyExpression().OperatorExpression = ArithmeticOperator.Divide;
                    break;
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    this.GetPropertyExpression().LogicalExpression = LogicalOperator.And;
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    this.GetPropertyExpression().LogicalExpression = LogicalOperator.Or;
                    break;
                case ExpressionType.Equal:
                    this.GetPropertyExpression().OperatorExpression = RelationalOperator.EqualTo;
                    break;
                case ExpressionType.NotEqual:
                    this.GetPropertyExpression().OperatorExpression = RelationalOperator.NotEqualTo;
                    break;
                case ExpressionType.LessThan:
                    this.GetPropertyExpression().OperatorExpression = RelationalOperator.LessThan;
                    break;
                case ExpressionType.LessThanOrEqual:
                    this.GetPropertyExpression().OperatorExpression = RelationalOperator.LessThanOrEqualTo;
                    break;
                case ExpressionType.GreaterThan:
                    this.GetPropertyExpression().OperatorExpression = RelationalOperator.GreaterThan;
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    this.GetPropertyExpression().OperatorExpression = RelationalOperator.GreaterThanOrEqualTo;
                    break;
                default:
                    throw new NotSupportedException(string.Format("The {0} operator is not supported", b.NodeType));
            }
            this.Visit(b.Right);
            return b;
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            this.GetPropertyExpression().PropertyAlias = p.Name;
            return p;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            PropertyExpression<T> propertyExpr = this.GetPropertyExpression();
            if (c.Value == null)
            {
                if (propertyExpr.OperatorExpression.Trim() == "<>")
                {
                    propertyExpr.OperatorExpression = string.Empty;
                    propertyExpr.IsNotNull();
                }
                else if (propertyExpr.OperatorExpression.Trim() == "=")
                {
                    propertyExpr.OperatorExpression = string.Empty;
                    propertyExpr.IsNull();
                }
            }
            propertyExpr.ArgumentData = c.Value;
            return c;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            m = base.VisitMethodCall(m) as MethodCallExpression;
            ArrayList args = new ArrayList();
            foreach (Expression e in m.Arguments)
            {
                switch (e.NodeType)
                {
                    case ExpressionType.Constant:
                        args.Add(((ConstantExpression)e).Value);
                        break;
                    default:
                        throw new NotSupportedException(string.Format("The expression argument type {0} is not supported.", e.NodeType));
                }
            }
            this.InvokeMethod(m.Method.Name, args.ToArray());
            return m;
        }

        private PropertyExpression<T> GetPropertyExpression(MemberInfo member)
        {
            if (member.DeclaringType.IsAssignableFrom(typeof(T)))
            {
                if (_propertyExpression.IsEmpty)
                {
                    _propertyExpression = new PropertyExpression<T>(member.Name);
                }
                else
                {
                    var childExpr = new PropertyExpression<T>(member.Name);
                    _propertyExpression.ChildExpressions.Add(childExpr);
                    return childExpr;
                }
            }
            else if ((member.DeclaringType.IsValueType || member.DeclaringType == typeof(string)) && member.MemberType == MemberTypes.Property)
            {
                this.InvokeMethod(member.Name, null);
            }
            return this.GetPropertyExpression();
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            u = base.VisitUnary(u) as UnaryExpression;
            if (u.NodeType == ExpressionType.Not)
            {
                PropertyExpression<T> propertyExpr = this.GetPropertyExpression();
                if (propertyExpr.OperatorExpression.Trim() == "LIKE") 
                {
                    propertyExpr.OperatorExpression = string.Empty;
                    propertyExpr.NotLike(propertyExpr.ArgumentData.ToString());
                }
                else if (propertyExpr.OperatorExpression.Trim() == "=")
                {
                    propertyExpr.OperatorExpression = string.Empty;
                    propertyExpr.NotEqualTo(propertyExpr.ArgumentData);
                }
                else
                {
                    _propertyExpression.UnaryExpression = "NOT";
                }
            }
            return u;
        }

        private void InvokeMethod(string memberName, object[] args)
        {
            switch (memberName)
            {
                case "Average":
                    this.GetPropertyExpression().Avg();
                    break;
                case "Min":
                    this.GetPropertyExpression().Min();
                    break;
                case "Max":
                    this.GetPropertyExpression().Max();
                    break;
                case "Count":
                    this.GetPropertyExpression().Count();
                    break;
                case "Length":
                    this.GetPropertyExpression().Length();
                    break;
                case "ToUpper":
                    this.GetPropertyExpression().ToUpper();
                    break;
                case "ToLower":
                    this.GetPropertyExpression().ToLower();
                    break;
                case "Trim":
                    this.GetPropertyExpression().Trim();
                    break;
                case "TrimStart":
                    this.GetPropertyExpression().TrimStart();
                    break;
                case "TrimEnd":
                    this.GetPropertyExpression().TrimEnd();
                    break;
                case "Contains":
                    if (args == null)
                    {
                        throw new InvalidOperationException(string.Format("Not enough arguments to call or specify {0}.", memberName));
                    }
                    this.GetPropertyExpression().Contains(args[0].ToString());
                    break;
                case "Substring":
                    if (args == null)
                    {
                        throw new InvalidOperationException(string.Format("Not enough arguments to call or specify {0}.", memberName));
                    }
                    if (args.Length == 1)
                    {
                        this.GetPropertyExpression().Substring((int)args[0]);
                    }
                    else
                    {
                        this.GetPropertyExpression().Substring((int)args[0], (int)args[1]);
                    }
                    break;
                default:
                    throw new NotSupportedException(string.Format("The call to {0} is not supported.", memberName));
            }
        }

        private PropertyExpression<T> GetPropertyExpression()
        {
            if (_propertyExpression.ChildExpressions.Count == 0)
            {
                return _propertyExpression;
            }
            int index = _propertyExpression.ChildExpressions.Count - 1;
            return _propertyExpression.ChildExpressions[index] as PropertyExpression<T>;
        }

        private void ShiftLogicalExpressions()
        {
            if (!string.IsNullOrEmpty(_propertyExpression.LogicalExpression) && _propertyExpression.ChildExpressions.Count > 0)
            {
                string[] logicalExpressions = new string[_propertyExpression.ChildExpressions.Count];
                logicalExpressions[0] = _propertyExpression.LogicalExpression;
                _propertyExpression.LogicalExpression = string.Empty;
                for (int i = 0; i < _propertyExpression.ChildExpressions.Count - 1; i++)
                {
                    logicalExpressions[i + 1] = _propertyExpression.ChildExpressions[i].LogicalExpression;
                    _propertyExpression.ChildExpressions[i].LogicalExpression = string.Empty;
                }
                for (int i = 0; i < _propertyExpression.ChildExpressions.Count; i++)
                {
                    _propertyExpression.ChildExpressions[i].LogicalExpression = logicalExpressions[i];
                }
            }
        }
    }
}
