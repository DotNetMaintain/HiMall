// ***********************************************************************
// 程序集			: NetRube.Data
// 文件名			: GroupByBuilder.cs
// 作者				: guozan
// 创建时间			: 2017-10-16
//
// 最后修改者		: guozan
// 最后修改时间		: 2017-10-16
// ***********************************************************************

using Himall.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace NetRube.Data
{
    /// <summary>
    /// GroupBy 构建器
    /// </summary>
    internal class GroupByBuilder : ExpressionVisitor
    {
        private STR __sql;
        private Database __db;
        private bool __wtn;

        /// <summary>
        /// 初始化一个新 <see cref="GroupByBuilder" /> 实例。
        /// </summary>
        /// <param name="db">数据库实例</param>
        /// <param name="args">参数</param>
        /// <param name="withTableName">指定是否包含表名</param>
        public GroupByBuilder(Database db, List<object> args = null, bool withTableName = true)
        {
            __sql = new STR();
            __db = db;
            __wtn = withTableName;
            this.Params = args ?? new List<object>();
        }

        /// <summary>
        /// 获取参数
        /// </summary>
        /// <value>参数集合</value>
        public List<object> Params { get; private set; }

        /// <summary>
        /// 返回 Group By 子句内容
        /// </summary>
        /// <returns>Group By 子句内容</returns>
        public override string ToString()
        {
            return __sql.ToString();
        }

        /// <summary>
        /// 返回 Group By 子句是否为空
        /// </summary>
        /// <returns>返回 Group By 子句是否为空</returns>
        public bool IsEmpty()
        {
            return __sql.IsEmpty;
        }

        #region Append

        private GroupByBuilder Append(Expression property, QueryOperatorType op, object value, string type)
        {
            if (property != null && value != null)
            {
                if (!__sql.IsEmpty)
                    __sql.Append(type);

                __sql.Append("(");
                __sql.Append(__db.GetColumnName(property));
                __sql.Append(this.GetOperator(op));
                __sql.AppendFormat("@{0}", this.Params.Count);
                __sql.Append(")");

                switch (op)
                {
                    case QueryOperatorType.Contains:
                        value = "%" + value.ToString().Trim('%') + "%";
                        break;
                    case QueryOperatorType.StartsWith:
                        value = value.ToString().Trim('%') + "%";
                        break;
                    case QueryOperatorType.EndsWith:
                        value = "%" + value.ToString().Trim('%');
                        break;
                }
                this.Params.Add(value);
            }

            return this;
        }

        /// <summary>添加 Group By 子句</summary>
        /// <param name="expression">条件表达式</param>
        /// <returns>Group By 构建器</returns>
        internal GroupByBuilder Append(Expression expression)
        {
            if (expression != null)
            {
                base.Visit(expression);
            }

            return this;
        }

        #endregion

        #region 重写 ExpressionVisitor
        /// <summary>
        /// 处理二元运算表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>表达式</returns>
        protected override Expression VisitBinary(BinaryExpression expression)
        {
            __sql.Append("(");
            this.Visit(expression.Left);
            __sql.Append(this.GetOperator(expression.NodeType));
            this.Visit(expression.Right);
            __sql.Append(")");

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
                if (!__sql.IsEmpty)
                    __sql.Append(", ");
                __sql.Append(this.GetColumnName(expression));
            }
            else
            {
                __sql.AppendFormat("@{0}", this.Params.Count);
                this.Params.Add(this.GetRightValue(expression));
            }

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

        #endregion
    }
}