// ***********************************************************************
// 程序集			: NetRube.Data
// 文件名			: WhereBuilder.cs
// 作者				: NetRube
// 创建时间			: 2013-08-05
//
// 最后修改者		: guozan
// 最后修改时间		: 2017-12-21
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
    /// WHERE 构建器
    /// </summary>
    internal class WhereBuilder : ExpressionVisitor
    {
        private STR __sql;
        private Database __db;
        private bool __wtn;

        /// <summary>
        /// 初始化一个新 <see cref="WhereBuilder" /> 实例。
        /// </summary>
        /// <param name="db">数据库实例</param>
        /// <param name="args">参数</param>
        /// <param name="withTableName">指定是否包含表名</param>
        public WhereBuilder(Database db, List<object> args = null, bool withTableName = true)
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
        protected List<object> Params { get; set; }

        /// <summary>
        /// 返回 WHERE 子句内容
        /// </summary>
        /// <returns>WHERE 子句内容</returns>
        public override string ToString()
        {
            return __sql.ToString();
        }

        #region Append
        private WhereBuilder Append(Expression expression, string type)
        {
            if (expression != null)
            {
                if (!__sql.IsEmpty)
                    __sql.Append(type);
                base.Visit(expression);
            }

            return this;
        }
        private WhereBuilder Append(Expression property, QueryOperatorType op, object value, string type)
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
        private WhereBuilder Append(string where, string type)
        {
            if (!where.IsNullOrEmpty_())
            {
                if (!__sql.IsEmpty)
                    __sql.Append(type);
                __sql.Append(where);
            }

            return this;
        }

        /// <summary>添加 WHERE 子句</summary>
        /// <param name="expression">条件表达式</param>
        /// <returns>WHERE 构建器</returns>
        internal WhereBuilder Append(Expression expression)
        {
            return this.Append(expression, " AND ");
        }

        /// <summary>添加 WHERE 子句</summary>
        /// <param name="property">字段栏表达式</param>
        /// <param name="op">比较运算符</param>
        /// <param name="value">值</param>
        /// <returns>WHERE 构建器</returns>
        internal WhereBuilder Append(Expression property, QueryOperatorType op, object value)
        {
            return this.Append(property, op, value, " AND ");
        }

        /// <summary>添加 WHERE 子句</summary>
        /// <param name="where">WHERE 子句</param>
        /// <returns>WHERE 构建器</returns>
        internal WhereBuilder Append(string where)
        {
            return this.Append(where, " AND ");
        }

        /// <summary>添加 WHERE OR 子句</summary>
        /// <param name="expression">条件表达式</param>
        /// <returns>WHERE 构建器</returns>
        internal WhereBuilder AppendOr(Expression expression)
        {
            return this.Append(expression, " OR ");
        }

        /// <summary>添加 WHERE OR 子句</summary>
        /// <param name="property">字段栏表达式</param>
        /// <param name="op">比较运算符</param>
        /// <param name="value">值</param>
        /// <returns>WHERE 构建器</returns>
        internal WhereBuilder AppendOr(Expression property, QueryOperatorType op, object value)
        {
            return this.Append(property, op, value, " OR ");
        }

        /// <summary>添加 WHERE OR 子句</summary>
        /// <param name="where">WHERE 子句</param>
        /// <returns>WHERE 构建器</returns>
        internal WhereBuilder AppendOr(string where)
        {
            return this.Append(where, " OR ");
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
                __sql.Append(this.GetColumnName(expression));
            else
            {
                __sql.AppendFormat("@{0}", this.Params.Count);
                this.Params.Add(this.GetRightValue(expression));
            }

            return expression;
        }

        /// <summary>
        /// 处理常量表达式
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>表达式</returns>
        protected override Expression VisitConstant(ConstantExpression expression)
        {
            __sql.AppendFormat("@{0}", this.Params.Count);
            this.Params.Add(expression.Value);

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
                case "Contains":
                    this.ParseLike(expression, "Contains");
                    break;
                case "StartsWith":
                    this.ParseLike(expression, "StartsWith");
                    break;
                case "EndsWith":
                    this.ParseLike(expression, "EndsWith");
                    break;
                case "ExExists":
                    this.ParseExists(expression);
                    break;
                case "ExIn":
                    this.ParseIn(expression);
                    break;
                case "ExNotIn":
                    this.ParseNotIn(expression);
                    break;
                case "ExIsNull":
                    this.ParseFieldIsNull(expression);
                    break;
                case "ExIsCurrentDay":
                    this.ParseFieldIsCurrentDay(expression);
                    break;
                case "Equals":
                    this.ParseEquals(expression);
                    break;
                case "ToString":
                    this.ParseToString(expression);
                    break;
                default:
                    throw new NotImplementedException("暂时没实现 “{0}” 方法".F(expression.Method.Name));
            }

            return expression;
        }
        #endregion

        #region 内部处理
        protected void AppendToSql(string strsql)
        {
            __sql.Append(strsql);
        }
        protected virtual string GetColumnName(MemberExpression expression)
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
        protected virtual void ParseToString(MethodCallExpression expression)
        {
            __sql.AppendFormat("@{0}", this.Params.Count);
            var value = this.GetRightValue(expression.Object).ToString();
            this.Params.Add(value);
        }
        protected virtual void ParseExists(MethodCallExpression expression)
        {
            var value = this.GetRightValue(expression.Arguments[1]);
            if (value is GetBuilder)
            {
                var val = value as GetBuilder;
                __sql.AppendFormat("(Exists ({0}))", val.GetSql());
            }
        }
        protected virtual void ParseIn(MethodCallExpression expression)
        {
            var column = expression.Arguments[0] as MemberExpression;
            var value = this.GetRightValue(expression.Arguments[1]);
            if (value is IEnumerable)
            {
                var val = value as IEnumerable;
                __sql.AppendFormat("({0} IN (@{1}))", this.GetColumnName(column), this.Params.Count);
                this.Params.Add(val);
            }
            else if (value is GetBuilder)
            {
                var val = value as GetBuilder;
                __sql.AppendFormat("({0} IN ({1}))", this.GetColumnName(column), val.GetSql());
            }
        }
        protected virtual void ParseNotIn(MethodCallExpression expression)
        {
            var column = expression.Arguments[0] as MemberExpression;
            var value = this.GetRightValue(expression.Arguments[1]);
            if (value is IEnumerable)
            {
                var val = value as IEnumerable;
                __sql.AppendFormat("({0} NOT IN (@{1}))", this.GetColumnName(column), this.Params.Count);
                this.Params.Add(val);
            }
            else if (value is GetBuilder)
            {
                var val = value as GetBuilder;
                __sql.AppendFormat("({0} NOT IN ({1}))", this.GetColumnName(column), val.GetSql());
            }
            else
            {
                __sql.AppendFormat("(1 <> 1)");
            }
        }
        protected virtual void ParseFieldIsNull(MethodCallExpression expression)
        {
            var column = expression.Arguments[0] as MemberExpression;
            __sql.AppendFormat("( {0} IS NULL )", this.GetColumnName(column));
        }
        protected virtual void ParseFieldIsCurrentDay(MethodCallExpression expression)
        {
            var column = expression.Arguments[0] as MemberExpression;
            var value = this.GetRightValue(expression.Arguments[1]);

            __sql.AppendFormat("( {0}=0 )", __db._dbType.DATEDIFFSyntax(this.GetColumnName(column), string.Format("@{0}", this.Params.Count)));
            this.Params.Add(value);
        }
        protected virtual void ParseEquals(MethodCallExpression expression)
        {
            var column = expression.Object as MemberExpression;
            var value = this.GetRightValue(expression.Arguments[0]);

            __sql.AppendFormat("( {0} = @{1} )", this.GetColumnName(column), this.Params.Count);
            this.Params.Add(value);
        }
        protected virtual void ParseLike(MethodCallExpression expression, string type)
        {
            var value = this.GetRightValue(expression.Arguments[0]);
            var column = expression.Object as MemberExpression;
            __sql.AppendFormat("({0} LIKE @{1})", this.GetColumnName(column), this.Params.Count);
            switch (type)
            {
                case "Contains":
                    value = "%" + value.ToString().Trim('%') + "%";
                    break;
                case "StartsWith":
                    value = value.ToString().Trim('%') + "%";
                    break;
                case "EndsWith":
                    value = "%" + value.ToString().Trim('%');
                    break;
            }
            this.Params.Add(value);
        }
        #endregion
    }
}