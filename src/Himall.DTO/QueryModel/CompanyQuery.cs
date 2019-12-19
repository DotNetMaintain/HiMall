using Himall.Model;
using System;
using System.Collections.Generic;

namespace Himall.DTO.QueryModel
{
    public partial class CompanyQuery : QueryBase
    {
        /// <summary>
        /// 标识
        /// </summary>
        public long? Id { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 公司编码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string Contacts { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public int? RegionId { get; set; }
        /// <summary>
        /// 地址详情(楼栋-门牌)
        /// </summary>
        public string AddressDetail { get; set; }
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
