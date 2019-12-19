// ***********************************************************************
// 程序集			: NetRube.Data
// 文件名			: HavingBuilder.cs
// 作者				: guozan
// 创建时间			: 2017-12-21
//
// 最后修改者		: guozan
// 最后修改时间		: 2017-12-21
// ***********************************************************************

using Himall.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NetRube.Data
{
    /// <summary>
    /// Join 构建器
    /// </summary>
    internal class JoinBuilder : WhereBuilder
    {
        /// <summary>
        /// 初始化一个新 <see cref="JoinBuilder" /> 实例。
        /// </summary>
        /// <param name="db">数据库实例</param>
        /// <param name="args">参数</param>
        /// <param name="withTableName">指定是否包含表名</param>
        public JoinBuilder(Database db, List<object> args = null, bool withTableName = true)
            : base(db, args, withTableName)
        {

        }

        #region 重写 ExpressionVisitor

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
                case "": break;
                default:
                    throw new NotImplementedException("暂时没实现 “{0}” 方法".F(expression.Method.Name));
            }

            return expression;
        }

        #endregion
    }
}