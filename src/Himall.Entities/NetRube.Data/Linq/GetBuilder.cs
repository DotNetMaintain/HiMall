// ***********************************************************************
// 程序集			: NetRube.Data
// 文件名			: GetBuilder.cs
// 作者				: NetRube
// 创建时间			: 2013-08-05
//
// 最后修改者		: guozan
// 最后修改时间		: 2017-12-21
// ***********************************************************************

using Himall.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NetRube.Data
{
    /// <summary>
    /// 查询构建器基类。内部使用，不能直接初始化此类。
    /// </summary>
    public abstract class GetBuilder
    {
        internal abstract string GetSql();
    }

    /// <summary>
    /// 查询构建器
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class GetBuilder<T> : GetBuilder where T : IModel
    {
        private Database __db;
        private string __table;
        bool removeParams = true;

        /// <summary>
        /// 初始化一个新 <see cref="GetBuilder&lt;T&gt;" /> 实例。
        /// </summary>
        /// <param name="db">数据库实例</param>
        /// <param name="args">参数</param>
        public GetBuilder(Database db, List<object> args = null)
        {
            __db = db;
            __table = db.GetTableName<T>();
            this.Params = args ?? new List<object>();
        }

        /// <summary>
        /// 获取参数
        /// </summary>
        /// <value>参数集合</value>
        public List<object> Params { get; private set; }

        #region JOIN
        /// <summary>
        /// 内连其它实体
        /// </summary>
        /// <typeparam name="TJoin">连接的实体类型</typeparam>
        /// <param name="expression">连接表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> InnerJoin<TJoin>(Expression<Func<T, TJoin, bool>> expression) where TJoin : IModel
        {
            return this.InternalJoin<TJoin>("INNER JOIN", expression);
        }

        /// <summary>
        /// 内连其它实体
        /// </summary>
        /// <typeparam name="TJoin">连接的实体类型</typeparam>
        /// <typeparam name="TEntity">设置关联的实体类型</typeparam>
        /// <param name="expression">连接表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> InnerJoin<TJoin, TEntity>(Expression<Func<TJoin, TEntity, bool>> expression)
            where TJoin : IModel
            where TEntity : IModel
        {
            return this.InternalJoin<TJoin>("INNER JOIN", expression);
        }

        /// <summary>
        /// 左连其它实体
        /// </summary>
        /// <typeparam name="TJoin">连接的实体类型</typeparam>
        /// <param name="expression">连接表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> LeftJoin<TJoin>(Expression<Func<T, TJoin, bool>> expression) where TJoin : IModel
        {
            return this.InternalJoin<TJoin>("LEFT JOIN", expression);
        }

        /// <summary>
        /// 左连其它实体
        /// </summary>
        /// <typeparam name="TJoin">连接的实体类型</typeparam>
        /// <typeparam name="TEntity">设置关联的实体类型</typeparam>
        /// <param name="expression">连接表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> LeftJoin<TJoin, TEntity>(Expression<Func<TJoin, TEntity, bool>> expression)
            where TJoin : IModel
            where TEntity : IModel
        {
            return this.InternalJoin<TJoin>("LEFT JOIN", expression);
        }

        /// <summary>
        /// 右连其它实体
        /// </summary>
        /// <typeparam name="TJoin">连接的实体类型</typeparam>
        /// <param name="expression">连接表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> RightJoin<TJoin>(Expression<Func<T, TJoin, bool>> expression) where TJoin : IModel
        {
            return this.InternalJoin<TJoin>("RIGHT JOIN", expression);
        }

        /// <summary>
        /// 右连其它实体
        /// </summary>
        /// <typeparam name="TJoin">连接的实体类型</typeparam>
        /// <typeparam name="TEntity">设置关联的实体类型</typeparam>
        /// <param name="expression">连接表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> RightJoin<TJoin, TEntity>(Expression<Func<TJoin, TEntity, bool>> expression)
            where TJoin : IModel
            where TEntity : IModel
        {
            return this.InternalJoin<TJoin>("RIGHT JOIN", expression);
        }

        private STR __join;
        private GetBuilder<T> InternalJoin<TJoin>(string join, Expression expression) where TJoin : IModel
        {
            if (__join.IsNull_()) __join = new STR();
            __join.AppendFormat(" {0} {1} ON {2} ",
                join, __db.GetTableName<TJoin>(),
                new JoinBuilder(__db, this.Params).Append(expression).ToString());

            return this;
        }

        #endregion

        #region WHERE
        private WhereBuilder __where;
        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Where(Expression expression)
        {
            if (__where == null)
                __where = new WhereBuilder(__db, this.Params);
            __where.Append(expression);

            return this;
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="property">字段栏表达式</param>
        /// <param name="op">比较运算符</param>
        /// <param name="value">值</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Where(Expression property, QueryOperatorType op, dynamic value)
        {
            if (__where == null)
                __where = new WhereBuilder(__db, this.Params);
            __where.Append(property, op, value);

            return this;
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="property">字段栏表达式</param>
        /// <param name="op">比较运算符</param>
        /// <param name="value">值</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Where(Expression<Func<T, dynamic>> property, QueryOperatorType op, dynamic value)
        {
            return this.Where((Expression)property, op, value);
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <param name="property">字段栏表达式</param>
        /// <param name="op">比较运算符</param>
        /// <param name="value">值</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Where<T1>(Expression<Func<T1, dynamic>> property, QueryOperatorType op, dynamic value) where T1 : IModel
        {
            return this.Where((Expression)property, op, value);
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Where(Expression<Func<T, bool>> expression)
        {
            return this.Where((Expression)expression);
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <typeparam name="T1">实体类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Where<T1>(Expression<Func<T1, bool>> expression) where T1 : IModel
        {
            return this.Where((Expression)expression);
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <typeparam name="T1">实体类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Where<T1>(Expression<Func<T, T1, bool>> expression) where T1 : IModel
        {
            return this.Where((Expression)expression);
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <typeparam name="T1">实体类型 1</typeparam>
        /// <typeparam name="T2">实体类型 2</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Where<T1, T2>(Expression<Func<T1, T2, bool>> expression)
            where T1 : IModel
            where T2 : IModel
        {
            return this.Where((Expression)expression);
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <typeparam name="T1">实体类型 1</typeparam>
        /// <typeparam name="T2">实体类型 2</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Where<T1, T2>(Expression<Func<T, T1, T2, bool>> expression)
            where T1 : IModel
            where T2 : IModel
        {
            return this.Where((Expression)expression);
        }
        #endregion

        #region WHERE OR
        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> WhereOr(Expression expression)
        {
            if (__where == null)
                __where = new WhereBuilder(__db, this.Params);
            __where.AppendOr(expression);

            return this;
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="property">字段栏表达式</param>
        /// <param name="op">比较运算符</param>
        /// <param name="value">值</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> WhereOr(Expression property, QueryOperatorType op, dynamic value)
        {
            if (__where == null)
                __where = new WhereBuilder(__db, this.Params);
            __where.AppendOr(property, op, value);

            return this;
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="property">字段栏表达式</param>
        /// <param name="op">比较运算符</param>
        /// <param name="value">值</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> WhereOr(Expression<Func<T, dynamic>> property, QueryOperatorType op, dynamic value)
        {
            return this.WhereOr((Expression)property, op, value);
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <param name="property">字段栏表达式</param>
        /// <param name="op">比较运算符</param>
        /// <param name="value">值</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> WhereOr<T1>(Expression<Func<T1, dynamic>> property, QueryOperatorType op, dynamic value) where T1 : IModel
        {
            return this.WhereOr((Expression)property, op, value);
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> WhereOr(Expression<Func<T, bool>> expression)
        {
            return this.WhereOr((Expression)expression);
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <typeparam name="T1">实体类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> WhereOr<T1>(Expression<Func<T1, bool>> expression) where T1 : IModel
        {
            return this.WhereOr((Expression)expression);
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <typeparam name="T1">实体类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> WhereOr<T1>(Expression<Func<T, T1, bool>> expression) where T1 : IModel
        {
            return this.WhereOr((Expression)expression);
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <typeparam name="T1">实体类型 1</typeparam>
        /// <typeparam name="T2">实体类型 2</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> WhereOr<T1, T2>(Expression<Func<T1, T2, bool>> expression)
            where T1 : IModel
            where T2 : IModel
        {
            return this.WhereOr((Expression)expression);
        }

        /// <summary>
        /// 查询条件
        /// </summary>
        /// <typeparam name="T1">实体类型 1</typeparam>
        /// <typeparam name="T2">实体类型 2</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> WhereOr<T1, T2>(Expression<Func<T, T1, T2, bool>> expression)
            where T1 : IModel
            where T2 : IModel
        {
            return this.WhereOr((Expression)expression);
        }
        #endregion

        #region Group By

        private GroupByBuilder __groupby;

        /// <summary>
        /// 分组条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>分组构建器</returns>
        public GetBuilder<T> GroupBy(Expression expression)
        {
            if (__groupby == null)
                __groupby = new GroupByBuilder(__db, this.Params);
            __groupby.Append(expression);

            return this;
        }

        /// <summary>
        /// 分组条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>分组构建器</returns>
        public GetBuilder<T> GroupBy(Expression<Func<T, dynamic>> expression)
        {
            return this.GroupBy((Expression)expression);
        }

        /// <summary>
        /// 分组条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>分组构建器</returns>
        public GetBuilder<T> GroupBy<T1>(Expression<Func<T1, dynamic>> expression)
        {
            return this.GroupBy((Expression)expression);
        }

        #endregion

        #region Having

        private HavingBuilder __having;

        /// <summary>
        /// 聚合条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Having(Expression expression)
        {
            if (__having == null)
                __having = new HavingBuilder(__db, this.Params);
            __having.Append(expression);

            return this;
        }

        /// <summary>
        /// 聚合条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>聚合条件构建器</returns>
        public GetBuilder<T> Having(Expression<Func<T, bool>> expression)
        {
            return this.Having((Expression)expression);
        }

        /// <summary>
        /// 聚合条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <typeparam name="T1">实体类型 1</typeparam>
        /// <returns>聚合条件构建器</returns>
        public GetBuilder<T> Having<T1>(Expression<Func<T1, bool>> expression)
            where T1 : IModel
        {
            return this.Having((Expression)expression);
        }

        /// <summary>
        /// 聚合条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <typeparam name="T1">实体类型 1</typeparam>
        /// <returns>聚合条件构建器</returns>
        public GetBuilder<T> Having<T1>(Expression<Func<T, T1, bool>> expression)
            where T1 : IModel
        {
            return this.Having((Expression)expression);
        }

        /// <summary>
        /// 聚合条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <typeparam name="T1">实体类型 1</typeparam>
        /// <typeparam name="T2">实体类型 2</typeparam>
        /// <returns>聚合条件构建器</returns>
        public GetBuilder<T> Having<T1, T2>(Expression<Func<T1, T2, bool>> expression)
            where T1 : IModel
            where T2 : IModel
        {
            return this.Having((Expression)expression);
        }

        /// <summary>
        /// 聚合条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <typeparam name="T1">实体类型 1</typeparam>
        /// <typeparam name="T2">实体类型 2</typeparam>
        /// <returns>聚合条件构建器</returns>
        public GetBuilder<T> Having<T1, T2>(Expression<Func<T, T1, T2, bool>> expression)
            where T1 : IModel
            where T2 : IModel
        {
            return this.Having((Expression)expression);
        }

        #endregion

        #region HavingOr

        /// <summary>
        /// 聚合条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> HavingOr(Expression expression)
        {
            if (__having == null)
                __having = new HavingBuilder(__db, this.Params);
            __having.AppendOr(expression);

            return this;
        }

        /// <summary>
        /// 聚合条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>聚合条件构建器</returns>
        public GetBuilder<T> HavingOr(Expression<Func<T, bool>> expression)
        {
            return this.HavingOr((Expression)expression);
        }

        /// <summary>
        /// 聚合条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <typeparam name="T1">实体类型 1</typeparam>
        /// <returns>聚合条件构建器</returns>
        public GetBuilder<T> HavingOr<T1>(Expression<Func<T1, bool>> expression)
            where T1 : IModel
        {
            return this.HavingOr((Expression)expression);
        }

        /// <summary>
        /// 聚合条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <typeparam name="T1">实体类型 1</typeparam>
        /// <returns>聚合条件构建器</returns>
        public GetBuilder<T> HavingOr<T1>(Expression<Func<T, T1, bool>> expression)
            where T1 : IModel
        {
            return this.HavingOr((Expression)expression);
        }

        /// <summary>
        /// 聚合条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <typeparam name="T1">实体类型 1</typeparam>
        /// <typeparam name="T2">实体类型 2</typeparam>
        /// <returns>聚合条件构建器</returns>
        public GetBuilder<T> HavingOr<T1, T2>(Expression<Func<T1, T2, bool>> expression)
            where T1 : IModel
            where T2 : IModel
        {
            return this.HavingOr((Expression)expression);
        }

        /// <summary>
        /// 聚合条件
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <typeparam name="T1">实体类型 1</typeparam>
        /// <typeparam name="T2">实体类型 2</typeparam>
        /// <returns>聚合条件构建器</returns>
        public GetBuilder<T> HavingOr<T1, T2>(Expression<Func<T, T1, T2, bool>> expression)
            where T1 : IModel
            where T2 : IModel
        {
            return this.HavingOr((Expression)expression);
        }

        #endregion

        #region SELECT
        private STR __select;
        private GetBuilder<T> Select(string columnName)
        {
            if (__select.IsNull_() || __select[0] == '*') __select = new STR();
            if (!__select.IsEmpty)
                __select.Append(", ");
            __select.Append(columnName);

            return this;
        }
        private GetBuilder<T> Select(string tableName, string columnName)
        {
            return Select("{0}.{1}".F(tableName, columnName));
        }


        /// <summary>
        /// 获取所有字段
        /// </summary>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> SelectAll()
        {
            __select = "*";

            return this;
        }

        /// <summary>
        /// 添加获取 <typeparamref name="T" /> 类型的所有字段
        /// </summary>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select()
        {
            return this.Select(__table, "*");
        }

        /// <summary>
        /// 添加获取 <typeparamref name="TEntity" /> 类型的所有字段
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity>() where TEntity : IModel
        {
            return this.Select(__db.GetTableName<TEntity>(), "*");
        }

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select(Expression expression)
        {
            return this.Select(new SelectBuilder(__db, expression).ToString());
        }

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select(params Expression<Func<T, dynamic>>[] expression)
        {
            expression.ForEach_(e => this.Select(new SelectBuilder(__db, e).ToString()));

            return this;
        }

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity>(params Expression<Func<TEntity, dynamic>>[] expression)
            where TEntity : IModel
        {
            expression.ForEach_(e => this.Select(new SelectBuilder(__db, e).ToString()));

            return this;
        }

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select(Expression<Func<T, dynamic>> expression)
        {
            this.Select(new SelectBuilder(__db, expression).ToString());
            return this;
        }

        /// <summary>
        /// 添加要获取的字段
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Select<TEntity>(Expression<Func<TEntity, dynamic>> expression)
            where TEntity : IModel
        {
            this.Select(new SelectBuilder(__db, expression).ToString());
            return this;
        }

        #endregion

        #region FROM
        private GetBuilder<T> __from;
        /// <summary>
        /// 从指定查询构建器里查询数据，如“FROM (SQL 语句)”
        /// </summary>
        /// <param name="from">查询构建器</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> From(GetBuilder<T> from)
        {
            __from = from;
            return this;
        }
        #endregion

        #region LIMIT
        private int __skip;
        /// <summary>
        /// 跳过第几条记录
        /// </summary>
        /// <param name="count">跳过记录数量</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Skip(int count)
        {
            __skip = count;
            return this;
        }

        private int __take;
        /// <summary>
        /// 取连续记录数
        /// </summary>
        /// <param name="count">获取记录数</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Take(int count)
        {
            __take = count;
            return this;
        }
        #endregion

        #region ORDER BY

        /// <summary>
        /// 添加排序字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> OrderBy(Expression<Func<T, dynamic>> expression)
        {
            return this.OrderBy(false, new OrderByBuilder<T>(__db, expression).ToString());
        }

        /// <summary>
        /// 添加排序字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> OrderBy<TEntity>(Expression<Func<TEntity, dynamic>> expression)
            where TEntity : IModel
        {
            return this.OrderBy(false, new OrderByBuilder<TEntity>(__db, expression).ToString());
        }

        /// <summary>
        /// 添加排序字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> OrderBy(params Expression<Func<T, dynamic>>[] expression)
        {
            expression.ForEach_(
                e => this.OrderBy(false, new OrderByBuilder<T>(__db, e).ToString())
                );

            return this;
        }

        /// <summary>
        /// 添加排序字段
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> OrderBy<TEntity>(params Expression<Func<TEntity, dynamic>>[] expression)
            where TEntity : IModel
        {
            expression.ForEach_(
                e => this.OrderBy(false, new OrderByBuilder<TEntity>(__db, e).ToString())
                );

            return this;
        }

        /// <summary>
        /// 添加降序排序字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> OrderByDescending(Expression<Func<T, dynamic>> expression)
        {
            return this.OrderBy(true, new OrderByBuilder<T>(__db, expression).ToString());
        }

        /// <summary>
        /// 添加降序排序字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> OrderByDescending<TEntity>(Expression<Func<TEntity, dynamic>> expression)
            where TEntity : IModel
        {
            return this.OrderBy(true, new OrderByBuilder<TEntity>(__db, expression).ToString());
        }

        /// <summary>
        /// 添加降序排序字段
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> OrderByDescending(params Expression<Func<T, dynamic>>[] expression)
        {
            expression.ForEach_(
                e => this.OrderBy(true, new OrderByBuilder<T>(__db, e).ToString())
                );

            return this;
        }

        /// <summary>
        /// 添加降序排序字段
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> OrderByDescending<TEntity>(params Expression<Func<TEntity, dynamic>>[] expression)
            where TEntity : IModel
        {
            expression.ForEach_(
                e => this.OrderBy(true, new OrderByBuilder<TEntity>(__db, e).ToString())
                );

            return this;
        }

        private GetBuilder<T> OrderBy(bool desc, string tableName, string columnName)
        {
            return OrderBy(desc, "{0}.{1}".F(tableName, columnName));
        }

        private STR __orderby;

        private GetBuilder<T> OrderBy(bool desc, string orderby)
        {
            if (__orderby.IsNull_()) __orderby = new STR();
            if (string.IsNullOrEmpty(orderby)) return this;

            if (!__orderby.IsEmpty)
                __orderby.Append(", ");
            __orderby.Append(orderby);
            if (desc)
                __orderby.Append(" DESC ");

            return this;
        }

        #endregion

        #region DISTINCT
        private bool __dist;
        /// <summary>
        /// 返回非重复记录
        /// </summary>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Distinct()
        {
            __dist = true;
            return this;
        }
        #endregion

        #region Map
        private Delegate __mapCallback;
        private Type[] __mapType;
        private bool __hasMap;
        private GetBuilder<T> InternalMap(Type[] type, Delegate cb)
        {
            __mapType = type;
            __mapCallback = cb;
            __hasMap = true;
            return this;
        }

        /// <summary>
        /// 多实体关联映射
        /// </summary>
        /// <typeparam name="T1">实体类型 1</typeparam>
        /// <typeparam name="T2">实体类型 2</typeparam>
        /// <param name="cb">实体间的关联转换器</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Map<T1, T2>(Func<T1, T2, T> cb)
            where T1 : IModel
            where T2 : IModel
        {
            return InternalMap(
                new Type[] {
                    typeof(T1),
                    typeof(T2)
                },
                cb);
        }

        /// <summary>
        /// 多实体关联映射
        /// </summary>
        /// <typeparam name="T1">实体类型 1</typeparam>
        /// <typeparam name="T2">实体类型 2</typeparam>
        /// <typeparam name="T3">实体类型 3</typeparam>
        /// <param name="cb">实体间的关联转换器</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Map<T1, T2, T3>(Func<T1, T2, T3, T> cb)
            where T1 : IModel
            where T2 : IModel
            where T3 : IModel
        {
            return InternalMap(
                new Type[] {
                    typeof(T1),
                    typeof(T2),
                    typeof(T3)
                },
                cb);
        }

        /// <summary>
        /// 多实体关联映射
        /// </summary>
        /// <typeparam name="T1">实体类型 1</typeparam>
        /// <typeparam name="T2">实体类型 2</typeparam>
        /// <typeparam name="T3">实体类型 3</typeparam>
        /// <typeparam name="T4">实体类型 4</typeparam>
        /// <param name="cb">实体间的关联转换器</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Map<T1, T2, T3, T4>(Func<T1, T2, T3, T4, T> cb)
            where T1 : IModel
            where T2 : IModel
            where T3 : IModel
            where T4 : IModel
        {
            return InternalMap(
                new Type[] {
                    typeof(T1),
                    typeof(T2),
                    typeof(T3),
                    typeof(T4)
                },
                cb);
        }

        /// <summary>
        /// 多实体关联映射
        /// </summary>
        /// <typeparam name="T1">实体类型 1</typeparam>
        /// <typeparam name="T2">实体类型 2</typeparam>
        /// <typeparam name="T3">实体类型 3</typeparam>
        /// <typeparam name="T4">实体类型 4</typeparam>
        /// <typeparam name="T5">实体类型 5</typeparam>
        /// <param name="cb">实体间的关联转换器</param>
        /// <returns>查询构建器</returns>
        public GetBuilder<T> Map<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, T> cb)
            where T1 : IModel
            where T2 : IModel
            where T3 : IModel
            where T4 : IModel
            where T5 : IModel
        {
            return InternalMap(
                new Type[] {
                    typeof(T1),
                    typeof(T2),
                    typeof(T3),
                    typeof(T4),
                    typeof(T5)
                },
                cb);
        }
        #endregion

        #region 内部操作
        private string GetColumnName(MemberExpression expression, bool wtn = true)
        {
            var et = expression.Expression.Type;
            var mm = expression.Member;
            PropertyInfo pi = null;
            if (mm.ReflectedType == et)
                pi = mm as PropertyInfo;
            else
                pi = et.GetProperty(mm.Name);
            return wtn ? __db.GetTableAndColumnName(pi.ReflectedType, mm.Name) : __db.GetColumnName(mm.Name);
        }

        private string GetSelect()
        {
            if (__select.IsNull_() || __select.IsEmpty) return "*";
            return __select.ToString();
        }
        private string GetFrom()
        {
            if (__from.IsNull_()) return __table;
            return "({0}) {1}".F(__from.GetSql(), __table);
        }
        private string GetJoin()
        {
            if (__join.IsNull_() || __join.IsEmpty) return string.Empty;
            return __join.ToString();
        }
        private string GetWhere(bool prefix = true)
        {
            if (__where.IsNull_()) return string.Empty;
            if (prefix)
                return " WHERE " + __where.ToString();
            return __where.ToString();
        }
        private object[] GetParams()
        {
            return this.Params.ToArray();
        }
        private string GetOrderBy(bool prefix = true)
        {
            if (__orderby.IsNull_() || __orderby.IsEmpty) return string.Empty;
            if (prefix)
                return " ORDER BY " + __orderby.ToString();
            return __orderby.ToString();
        }
        private string GetGroupBy()
        {
            if (__groupby.IsNull_() || __groupby.IsEmpty()) return string.Empty;
            return " Group By " + __groupby.ToString();
        }
        private string GetHaving(bool prefix = true)
        {
            if (__groupby.IsNull_() || __groupby.IsEmpty() || __having.IsNull_()) return string.Empty;
            if (prefix)
                return " HAVING " + __having.ToString();
            return __having.ToString();
        }
        #endregion

        #region 生成 SQL 语句
        internal override string GetSql()
        {
            if (__take > 0)
            {
                if (__skip > 0)
                    return this.GetSqlPaged();
                return this.GetSqlTake();
            }
            var dist = __dist ? "DISTINCT " : string.Empty;
            return "SELECT {0}{1} FROM {2}{3} {4}{5}{6}{7}".F(dist, this.GetSelect(), this.GetFrom(), this.GetJoin(), this.GetWhere(), this.GetGroupBy(), this.GetHaving(), this.GetOrderBy());
        }
        private string GetSqlTake()
        {
            return __db._dbType.BuildTopSql(__take, __dist, this.GetSelect(), this.GetFrom(), this.GetJoin(), this.GetWhere(), this.GetGroupBy(), this.GetHaving(), this.GetOrderBy(), this.Params);
        }
        private string GetSqlPaged()
        {
            return __db._dbType.BuildPagedSql(__skip, __take, __dist, this.GetSelect(), this.GetFrom(), this.GetJoin(), this.GetWhere(), this.GetGroupBy(), this.GetHaving(), this.GetOrderBy(), this.Params);
        }
        #endregion

        #region 执行
        #region Count
        /// <summary>
        /// 获取符合相关条件的记录数量
        /// </summary>
        /// <returns>返回符合相关条件的记录数量</returns>
        public int Count()
        {
            return this.Count<int>();
        }
        /// <summary>
        /// 获取符合相关条件的记录数量
        /// </summary>
        /// <typeparam name="TResult">返回的数据类型</typeparam>
        /// <returns>以指定的数据类型返回符合相关条件的记录数量</returns>
        public TResult Count<TResult>()
        {
            var ret = default(TResult);
            var strGroupBy = this.GetGroupBy();
            if (string.IsNullOrEmpty(strGroupBy))
            {
                if (__dist)
                {
                    ret = __db.ExecuteScalar<TResult>("SELECT COUNT(0) FROM (SELECT DISTINCT {0} FROM {1} {2} {3})"
                        .F(this.GetSelect(), this.GetFrom(), this.GetJoin(), this.GetWhere()), this.GetParams());
                }
                else
                {
                    ret = __db.ExecuteScalar<TResult>("SELECT COUNT(0) FROM {0} {1} {2} "
                        .F(this.GetFrom(), this.GetJoin(), this.GetWhere()), this.GetParams());
                }
            }
            else
            {
                var strHaving = this.GetHaving();
                ret = __db.ExecuteScalar<TResult>("SELECT COUNT(0) FROM (SELECT {0} FROM {1} {2} {3} {4} {5} ) aa"
                     .F(__dist ? "DISTINCT {0}".F(this.GetSelect()) : "1", this.GetFrom(), this.GetJoin(), this.GetWhere(), strGroupBy, strHaving), this.GetParams());
            }
            if (removeParams)
            {
                Params = null;
                __db.Params = null;
            }
            return ret;
        }
        #endregion

        #region Exist
        /// <summary>
        /// 获取是否存在相关条件的记录
        /// </summary>
        /// <returns>返回是否存在相关条件的记录</returns>
        public bool Exist()
        {
            var ret = Count<long>() > 0; //__db.Exists<T>(GetWhere(false), this.GetParams());
            Params = null;
            __db.Params = null;
            return ret;
        }
        #endregion

        #region Sum

        /// <summary>
        /// 获取指定字段的和
        /// </summary>
        /// <typeparam name="TResult">返回的数据类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>以指定的数据类型返回符合相关条件的和</returns>
        public TResult Sum<TResult>(Expression<Func<T, dynamic>> expression)
        {
            var ret = __db.ExecuteScalar<TResult>("SELECT SUM({0}) FROM {1}{2} {3}{4}{5}".F(new SelectBuilder(__db, expression).ToString(), this.GetFrom(), this.GetJoin(), this.GetWhere(), this.GetGroupBy(), this.GetHaving()), this.GetParams());
            Params = null;
            __db.Params = null;
            return ret;
        }

        /// <summary>
        /// 获取指定字段的和
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <typeparam name="TResult">返回的数据类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>以指定的数据类型返回符合相关条件的和</returns>
        public TResult Sum<TEntity, TResult>(Expression<Func<TEntity, dynamic>> expression)
        {
            var ret = __db.ExecuteScalar<TResult>("SELECT SUM({0}) FROM {1}{2} {3}{4}{5}".F(new SelectBuilder(__db, expression).ToString(), this.GetFrom(), this.GetJoin(), this.GetWhere(), this.GetGroupBy(), this.GetHaving()), this.GetParams());
            Params = null;
            __db.Params = null;
            return ret;
        }
        #endregion

        #region Max

        /// <summary>
        /// 获取指定字段的最大值
        /// </summary>
        /// <typeparam name="TResult">返回的数据类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>以指定的数据类型返回符合相关条件的最大值</returns>
        public TResult Max<TResult>(Expression<Func<T, dynamic>> expression)
        {
            var ret = __db.ExecuteScalar<TResult>("SELECT MAX({0}) FROM {1}{2} {3}{4}{5}".F(new SelectBuilder(__db, expression).ToString(), this.GetFrom(), this.GetJoin(), this.GetWhere(), this.GetGroupBy(), this.GetHaving()), this.GetParams());
            Params = null;
            __db.Params = null;
            return ret;
        }

        /// <summary>
        /// 获取指定字段的最大值
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <typeparam name="TResult">返回的数据类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>以指定的数据类型返回符合相关条件的最大值</returns>
        public TResult Max<TEntity, TResult>(Expression<Func<TEntity, dynamic>> expression)
        {
            var ret = __db.ExecuteScalar<TResult>("SELECT MAX({0}) FROM {1}{2} {3}{4}{5}".F(new SelectBuilder(__db, expression).ToString(), this.GetFrom(), this.GetJoin(), this.GetWhere(), this.GetGroupBy(), this.GetHaving()), this.GetParams());
            Params = null;
            __db.Params = null;
            return ret;
        }
        #endregion

        #region Min

        /// <summary>
        /// 获取指定字段的最小值
        /// </summary>
        /// <typeparam name="TResult">返回的数据类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>以指定的数据类型返回符合相关条件的最小值</returns>
        public TResult Min<TResult>(Expression<Func<T, dynamic>> expression)
        {
            var ret = __db.ExecuteScalar<TResult>("SELECT MIN({0}) FROM {1}{2} {3}{4}{5}".F(new SelectBuilder(__db, expression).ToString(), this.GetFrom(), this.GetJoin(), this.GetWhere(), this.GetGroupBy(), this.GetHaving()), this.GetParams());
            Params = null;
            __db.Params = null;
            return ret;
        }

        /// <summary>
        /// 获取指定字段的最小值
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <typeparam name="TResult">返回的数据类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>以指定的数据类型返回符合相关条件的最小值</returns>
        public TResult Min<TEntity, TResult>(Expression<Func<TEntity, dynamic>> expression)
        {
            var ret = __db.ExecuteScalar<TResult>("SELECT MIN({0}) FROM {1}{2} {3}{4}{5}".F(new SelectBuilder(__db, expression).ToString(), this.GetFrom(), this.GetJoin(), this.GetWhere(), this.GetGroupBy(), this.GetHaving()), this.GetParams());
            Params = null;
            __db.Params = null;
            return ret;
        }

        #endregion

        #region Avg

        /// <summary>
        /// 获取指定字段的平均值
        /// </summary>
        /// <typeparam name="TResult">返回的数据类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>以指定的数据类型返回符合相关条件的平均值</returns>
        public TResult Avg<TResult>(Expression<Func<T, dynamic>> expression)
        {
            var ret = __db.ExecuteScalar<TResult>("SELECT AVG({0}) FROM {1}{2} {3}{4}{5}".F(new SelectBuilder(__db, expression).ToString(), this.GetFrom(), this.GetJoin(), this.GetWhere(), this.GetGroupBy(), this.GetHaving()), this.GetParams());
            Params = null;
            __db.Params = null;
            return ret;
        }

        /// <summary>
        /// 获取指定字段的平均值
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <typeparam name="TResult">返回的数据类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>以指定的数据类型返回符合相关条件的平均值</returns>
        public TResult Avg<TEntity, TResult>(Expression<Func<TEntity, dynamic>> expression)
        {
            var ret = __db.ExecuteScalar<TResult>("SELECT AVG({0}) FROM {1}{2} {3}{4}{5}".F(new SelectBuilder(__db, expression).ToString(), this.GetFrom(), this.GetJoin(), this.GetWhere(), this.GetGroupBy(), this.GetHaving()), this.GetParams());
            Params = null;
            __db.Params = null;
            return ret;
        }

        #endregion

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns>符合相关条件的实体列表</returns>
        public List<T> ToList()
        {
            var ret = new List<T>();
            if (__hasMap)
            {
                ret = __db.Query<T>(__mapType, __mapCallback, this.GetSql(), this.GetParams()).ToList();
                Params = null;
                __db.Params = null;
                return ret;
            }
            ret = __db.Query<T>(this.GetSql(), this.GetParams()).ToList();
            Params = null;
            __db.Params = null;
            return ret;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns>符合相关条件的实体列表</returns>
        public List<TResult> ToList<TResult>()
        {
            var ret = new List<TResult>();
            if (__hasMap)
            {
                ret = __db.Query<TResult>(__mapType, __mapCallback, this.GetSql(), this.GetParams()).ToList();
                Params = null;
                __db.Params = null;
                return ret;
            }
            ret = __db.Query<TResult>(this.GetSql(), this.GetParams()).ToList();
            Params = null;
            __db.Params = null;
            return ret;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns>符合相关条件的实体列表</returns>
        public List<TResult> ToList<TResult>(Func<dynamic, TResult> selector)
        {
            if (__hasMap)
            {
                var retMap = __db.Query<dynamic>(__mapType, __mapCallback, this.GetSql(), this.GetParams());
                Params = null;
                __db.Params = null;
                return retMap.Select(selector).ToList();
            }
            var ret = __db.Query<dynamic>(this.GetSql(), this.GetParams());
            Params = null;
            __db.Params = null;
            return ret.Select(selector).ToList();
        }

        /// <summary>
        /// 获取满足条件的第一项或默认内容
        /// </summary>
        /// <returns>符合相关条件的单个实体</returns>
        public T FirstOrDefault()
        {
            var ret = default(T);
            if (__hasMap)
            {
                ret = __db.Query<T>(__mapType, __mapCallback, this.GetSql(), this.GetParams()).FirstOrDefault();
                Params = null;
                __db.Params = null;
                return ret;
            }
            __take = 1;
            ret = __db.Query<T>(this.GetSql(), this.GetParams()).FirstOrDefault();
            Params = null;
            __db.Params = null;
            return ret;
        }

        /// <summary>
        /// 获取满足条件的第一项或默认内容
        /// </summary>
        /// <returns>符合相关条件的单个实体</returns>
        public TResult FirstOrDefault<TResult>()
        {
            var ret = default(TResult);
            if (__hasMap)
            {
                ret = __db.Query<TResult>(__mapType, __mapCallback, this.GetSql(), this.GetParams()).FirstOrDefault();
                Params = null;
                __db.Params = null;
                return ret;
            }
            __take = 1;
            ret = __db.Query<TResult>(this.GetSql(), this.GetParams()).FirstOrDefault();
            Params = null;
            __db.Params = null;
            return ret;
        }

        /// <summary>
        /// 获取满足条件的第一项或默认内容
        /// </summary>
        /// <returns>符合相关条件的单个实体</returns>
        public TResult FirstOrDefault<TResult>(Func<dynamic, TResult> selector)
        {
            if (__hasMap)
            {
                var retMap = __db.Query<dynamic>(__mapType, __mapCallback, this.GetSql(), this.GetParams());
                Params = null;
                __db.Params = null;
                return retMap.Select(selector).FirstOrDefault();
            }
            __take = 1;
            var ret = __db.Query<dynamic>(this.GetSql(), this.GetParams());
            Params = null;
            __db.Params = null;
            return ret.Select(selector).FirstOrDefault();
        }

        #region 分页
        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>符合相关条件的分页列表</returns>
        public PagedList<T> ToPagedList(int pageIndex, int pageSize)
        {
            IList<T> ls = null;
            removeParams = false;
            var count = this.Count();
            removeParams = true;
            if (pageSize > 0)
                __take = pageSize;
            if (count > 0)
            {
                var pages = Utils.GetPages_(count, pageSize);
                pageIndex = pageIndex.NotGreaterThan_(pages);
                if (pageIndex > 1)
                    __skip = (pageIndex - 1) * pageSize;
                ls = this.ToList();
            }

            return new PagedList<T>(ls, pageIndex, pageSize, count);
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>符合相关条件的分页列表</returns>
        public PagedList<TEntity> ToPagedList<TEntity>(int pageIndex, int pageSize)
        {
            IList<TEntity> ls = null;
            removeParams = false;
            var count = this.Count();
            removeParams = true;
            if (pageSize > 0)
                __take = pageSize;
            if (count > 0)
            {
                var pages = Utils.GetPages_(count, pageSize);
                pageIndex = pageIndex.NotGreaterThan_(pages);
                if (pageIndex > 1)
                    __skip = (pageIndex - 1) * pageSize;
                ls = this.ToList<TEntity>();
            }

            return new PagedList<TEntity>(ls, pageIndex, pageSize, count);
        }

        /// <summary>
        /// 获取分页列表
        /// </summary>
        /// <returns>符合相关条件的分页列表</returns>
        public PagedList<T> ToPagedList()
        {
            IList<T> ls = null;
            var index = 1;
            removeParams = false;
            var count = this.Count();
            removeParams = true;
            if (count > 0)
            {
                ls = this.ToList();
                double recs = __take.NotGreaterThan_(count);
                index = Math.Ceiling(recs / __skip).ToInt_();
            }

            return new PagedList<T>(ls, index, __take, count);
        }

        /// <summary>
        /// 获取分页列表，返回 PetaPoco 的分页格式
        /// </summary>
        /// <param name="pageIndex">当前页索引</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>符合相关条件的 PetaPoco 分页格式列表</returns>
        //public Page<T> ToPage(int pageIndex, int pageSize)
        //{
        //    var ls = this.ToPagedList(pageIndex, pageSize);
        //    return new Page<T>
        //    {
        //        CurrentPage = ls.CurrentPageIndex,
        //        ItemsPerPage = ls.PageSize,
        //        TotalItems = ls.TotalRecordCount,
        //        TotalPages = ls.TotalPageCount,
        //        Items = ls
        //    };
        //}

        /// <summary>
        /// 获取分页列表，返回 PetaPoco 的分页格式
        /// </summary>
        /// <returns>符合相关条件的 PetaPoco 分页格式列表</returns>
        //public Page<T> ToPage()
        //{
        //    var ls = this.ToPagedList();
        //    return new Page<T>
        //    {
        //        CurrentPage = ls.CurrentPageIndex,
        //        ItemsPerPage = ls.PageSize,
        //        TotalItems = ls.TotalRecordCount,
        //        TotalPages = ls.TotalPageCount,
        //        Items = ls
        //    };
        //}
        #endregion

        #region 子查询
        /// <summary>
        /// 建立子查询条件
        /// </summary>
        public GetBuilder<T> Continue()
        {
            __db.Params = this.Params;
            return this;
        }

        #endregion

        #endregion
    }
}