using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    /// <summary>
    /// 部门实体
    /// </summary>
    [TableName("Custom_CompanyDepInfos")]
    public partial class CompanyDepInfo
    {
        /// <summary>
        /// 部门标识
        /// </summary>
        [FieldType(FieldType.IncrementField)]
        [FieldType(FieldType.KeyField)]
        public long Id { get; set; }
        /// <summary>
        /// 所属公司
        /// </summary>
        [FieldType(FieldType.CommonField)]
        public long CompanyId { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        [FieldType(FieldType.CommonField)]
        public string Name { get; set; }
        /// <summary>
        /// 编码
        /// </summary>
        [FieldType(FieldType.CommonField)]
        public string Code { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [FieldType(FieldType.CommonField)]
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 公司编码
        /// </summary>
        public string CompanyCode { get; set; }
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
