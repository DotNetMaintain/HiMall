// ***********************************************************************
// 程序集			: NetRube.Data
// 文件名			: SelectBuilder.cs
// 作者				: guozan
// 创建时间			: 2017-12-19
//
// 最后修改者		: guozan
// 最后修改时间		: 2017-12-19
// ***********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace NetRube.Data
{
    /// <summary>
    /// Select 构建器
    /// </summary>
    internal class SelectBuilder : ExpressionVisitor
    {
        private STR __select;
        private bool __wtn;
        private Database __db;
        private bool __needdelimiter = true;

        /// <summary>
        /// 初始化一个新 <see cref="SelectBuilder" /> 实例。
        /// </summary>
        public SelectBuilder(Database db, Expression expression, bool withTableName = true)
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
            var members = expression.Members;
            var arguments = expression.Arguments;
            var len = members.Count < arguments.Count ? members.Count : arguments.Count;
            for (int i = 0; i < len; i++)
            {
                this.Visit(arguments[i]);
                if (arguments[i].NodeType == ExpressionType.MemberAccess && (arguments[i] as MemberExpression).Member.Name == members[i].Name) continue;
                __select.Append(__db._dbType.EscapeNewName(members[i].Name));
            }
            return expression;
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
                case "ExIfNull":
                    this.ParseIfNull(expression);
                    break;
                case "ExResolve":
                    var value = this.GetRightValue(expression.Arguments[0]);
                    if (value is GetBuilder)
                    {
                        this.ParseContinue(value as GetBuilder);
                    }
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
                __select.Append("");
                var value = this.GetRightValue(expression);
                if (value is GetBuilder)
                {
                    __select.Append((value as GetBuilder).GetSql());
                }
            }

            return expression;
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            this.Visit(expression.Left);
            __select.Append(this.GetOperator(expression.NodeType));
            var temp = __needdelimiter;
            __needdelimiter = false;
            this.Visit(expression.Right);
            __needdelimiter = temp;
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
            var et = expression.Expression.Type;
            var mm = expression.Member;
            PropertyInfo pi = null;
            if (mm.ReflectedType == et)
                pi = mm as PropertyInfo;
            else
                pi = et.GetProperty(mm.Name);
            return __wtn ? __db.GetTableAndColumnName(pi.ReflectedType, mm.Name) : __db.GetColumnName(mm.Name);
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

        private void ParseContinue(GetBuilder gb)
        {
            if (!__select.IsEmpty && __needdelimiter)
                __select.Append(", ");
            __select.Append(" (" + gb.GetSql() + ") ");
        }

        private void ParseIfNull(MethodCallExpression expression)
        {
            if (!__select.IsEmpty && __needdelimiter)
                __select.Append(", ");
            __select.Append(" IFNULL(");
            var temp = __needdelimiter;
            __needdelimiter = false;
            this.Visit(expression.Arguments[0]);
            __needdelimiter = temp;
            __select.Append(", ");
            __select.Append(this.GetRightValue(expression.Arguments[1]));
            __select.Append(") ");
        }

        #endregion
    }
}