
namespace Himall.DTO
{
    /// <summary>
    /// 部门信息
    /// </summary>
    public partial class CompanyDepModel
    {
        /// <summary>
        /// 部门标识
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 子公司标识
        /// </summary>
        public long CompanyId { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 编码
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
        /// 创建时间
        /// </summary>
        public string CreateDate { get; set; }
        
        public int MemberNum { get; set; }
        public int MemberNumAuditing { get; set; }
    }
}