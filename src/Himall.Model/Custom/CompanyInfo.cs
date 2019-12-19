using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    /// <summary>
    /// 子公司实体
    /// </summary>
    [TableName("Custom_CompanyInfos")]
    public partial class CompanyInfo
    {
        /// <summary>
        /// 子公司标识
        /// </summary>
        [FieldType(FieldType.IncrementField)]
        [FieldType(FieldType.KeyField)]
        public long Id { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        [FieldType(FieldType.CommonField)]
        public string Name { get; set; }
        /// <summary>
        /// 编码
        /// </summary>
        [FieldType(FieldType.CommonField)]
        public string Code { get; set; }
        /// <summary>
        /// 公司联系人
        /// </summary>
        [FieldType(FieldType.CommonField)]
        public string Contacts { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        [FieldType(FieldType.CommonField)]
        public string Phone { get; set; }
        /// <summary>
        /// 公司地址标识
        /// </summary>
        [FieldType(FieldType.CommonField)]
        public int RegionId { get; set; }
        /// <summary>
        /// 公司地址标识链
        /// </summary>
        [FieldType(FieldType.CommonField)]
        public string Regionlink { get; set; }
        /// <summary>
        /// 具体街道信息
        /// </summary>
        [FieldType(FieldType.CommonField)]
        public string Address { get; set; }
        /// <summary>
        /// 地址详情(楼栋-门牌)
        /// </summary>
        [FieldType(FieldType.CommonField)]
        public string AddressDetail { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [FieldType(FieldType.CommonField)]
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 部门数量
        /// </summary>
        public int DepNum { get; set; }
        /// <summary>
        /// 员工数量（审核通过）
        /// </summary>
        public int MemNum { get; set; }
        /// <summary>
        /// 员工数量（审核中）
        /// </summary>
        public int MemNumAuditing { get; set; }
    }
}
