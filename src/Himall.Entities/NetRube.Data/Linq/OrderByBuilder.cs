// ***********************************************************************
// 程序集			: NetRube.Data
// 文件名			: OrderByBuilder.cs
// 作者				: guozan
// 创建时间			: 2017-12-21
//
// 最后修改者		: guozan
// 最后修改时间		: 2017-12-21
// ***********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace NetRube.Data
{
    /// <summary>
    /// Select 构建器
    /// </summary>
    internal class OrderByBuilder<T> : ExpressionVisitor
    {
        private STR __select;
        private bool __wtn;
        private Database __db;
        private bool __needdelimiter = true;

        /// <summary>
        /// 初始化一个新 <see cref="OrderByBuilder&lt;T&gt;" /> 实例。
        /// </summary>
        public OrderByBuilder(Database db, Expression expression, bool withTableName = true)
        {
            __select = new STR();
            __db = db;
            __wtn = withTableName;
            if (expression != null)
            {
                base.Visit(expression);
            }
        }

        /// <summary>
        /// 返回 Select 字段
        /// </summary>
        /// <returns>Select 字段</returns>
        public override string ToString()
        {
            return __select.ToString();
        }

        /// <summary>
        /// 返回 Select 字段是否为空
        /// </summary>
        /// <returns>Select 字段是否为空</returns>
        public bool IsEmpty()
        {
            return __select.IsEmpty;
        }

        #region 重写 ExpressionVisitor

        /// <summary>
        /// 处理构造函数调用表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns></returns>
        protected override Expression VisitNew(NewExpression expression)
        {
            throw new NotSupportedException("未实现本方法");
        }

        /// <summary>
        /// 处理方法调用表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>表达式</returns>
        /// <exception cref="System.NotImplementedException">指定方法未实现</exception>
        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            switch (expression.Method.Name)
            {
                case "ExCount":
                    this.ParseCount(expression);
                    break;
                case "ExSum":
                    this.ParseSum(expression);
                    break;
                case "ExAvg":
                    this.ParseAvg(expression);
                    break;
                case "ExMax":
                    this.ParseMax(expression);
                    break;
                case "ExMin":
                    this.ParseMin(expression);
                    break;
                default:
                    throw new NotImplementedException("暂时没实现 “{0}” 方法".F(expression.Method.Name));
            }

            return expression;
        }

        /// <summary>
        /// 处理字段或属性表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>表达式</returns>
        protected override Expression VisitMemberAccess(MemberExpression expression)
        {
            if (expression.Expression != null && expression.Expression.NodeType == ExpressionType.Parameter)
            {
                if (!__select.IsEmpty && __needdelimiter)
                    __select.Append(", ");
                __select.Append(this.GetColumnName(expression));
            }
            else
            {
                //__select.AppendFormat("@{0}", this.Params.Count);
                //this.Params.Add(this.GetRightValue(expression));
            }

            return expression;
        }

        protected override Expression VisitConstant(ConstantExpression expression)
        {
            if (!__select.IsEmpty && __needdelimiter)
                __select.Append(", ");
            __select.Append(expression.Value);
            return expression;
        }

        #endregion

        #region 内部处理

        private string GetColumnName(MemberExpression expression)
        {
            return __wtn ? __db.GetTableAndColumnName(typeof(T), expression.Member.Name) : __db.GetColumnName(expression.Member.Name);
        }

        private void ParseSum(MethodCallExpression expression)
        {
            if (!__select.IsEmpty && __needdelimiter)
                __select.Append(", ");
            __select.Append(" SUM(");
            var temp = __needdelimiter;
            __needdelimiter = false;
            this.Visit(expression.Arguments[0]);
            __needdelimiter = temp;
            __select.Append(") ");
        }

        private void ParseCount(MethodCallExpression expression)
        {
            if (!__select.IsEmpty && __needdelimiter)
                __select.Append(", ");
            __select.Append(" COUNT(0) ");
        }

        private void ParseAvg(MethodCallExpression expression)
        {
            if (!__select.IsEmpty && __needdelimiter)
                __select.Append(", ");
            __select.Append(" Avg(");
            var temp = __needdelimiter;
            __needdelimiter = false;
            this.Visit(expression.Arguments[0]);
            __needdelimiter = temp;
            __select.Append(") ");
        }

        private void ParseMax(MethodCallExpression expression)
        {
            if (!__select.IsEmpty && __needdelimiter)
                __select.Append(", ");
            __select.Append(" MAX(");
            var temp = __needdelimiter;
            __needdelimiter = false;
            this.Visit(expression.Arguments[0]);
            __needdelimiter = temp;
            __select.Append(") ");
        }

        private void ParseMin(MethodCallExpression expression)
        {
            if (!__select.IsEmpty && __needdelimiter)
                __select.Append(", ");
            __select.Append(" MIN(");
            var temp = __needdelimiter;
            __needdelimiter = false;
            this.Visit(expression.Arguments[0]);
            __needdelimiter = temp;
            __select.Append(") ");
        }

        #endregion
    }
}