
namespace Himall.DTO
{
    /// <summary>
    /// 子公司信息
    /// </summary>
    public partial class CompanyModel
    {
        /// <summary>
        /// 子公司标识
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        
        public string Name { get; set; }
        /// <summary>
        /// 编码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 公司联系人
        /// </summary>
        public string Contacts { get; set; }
        /// <summary>
        /// 具体街道信息
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateDate { get; set; }

        public int DepNum { get; set; }
        public int MemberNum { get; set; }
        public int MemberNumAuditing { get; set; }
    }
}