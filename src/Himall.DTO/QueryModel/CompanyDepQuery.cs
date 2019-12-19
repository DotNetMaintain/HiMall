using Himall.Model;
using System;
using System.Collections.Generic;

namespace Himall.DTO.QueryModel
{
    public partial class CompanyDepQuery : QueryBase
    {
        /// <summary>
        /// 标识
        /// </summary>
        public long? Id { get; set; }
        /// <summary>
        /// 公司标识
        /// </summary>
        public long? CompanyId { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 公司编码
        /// </summary>
        public string CompanyCode { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 部门编码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndDate { get; set; }

    }
}
