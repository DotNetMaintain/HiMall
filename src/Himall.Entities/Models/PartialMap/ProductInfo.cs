using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Entities
{
    public partial class ProductInfo
    {
        /// <summary>
        /// 销售状态
        /// </summary>
        public enum ProductSaleStatus
        {
            /// <summary>
            /// 原始状态
            /// <para>此状态不可入库，需要取出原数据的销售状态重新补充数据</para>
            /// </summary>
            [Description("原始状态")]
            RawState = 0,
            /// <summary>
            /// 出售中
            /// </summary>
            [Description("出售中")]
            OnSale = 1,

            /// <summary>
            /// 仓库中
            /// </summary>
            [Description("仓库中")]
            InStock = 2,
            /// <summary>
            /// 草稿箱
            /// </summary>
            [Description("草稿箱")]
            InDraft = 3
        }
        /// <summary>
        /// 审核状态
        /// </summary>
        public enum ProductAuditStatus
        {
            /// <summary>
            /// 待审核
            /// </summary>
            [Description("待审核")]
            WaitForAuditing = 1,

            /// <summary>
            /// 销售中
            /// </summary>
            [Description("销售中")]
            Audited,

            /// <summary>
            /// 未通过(审核失败)
            /// </summary>
            [Description("未通过")]
            AuditFailed,

            /// <summary>
            /// 违规下架
            /// </summary>
            [Description("违规下架")]
            InfractionSaleOff,

            /// <summary>
            /// 未审核
            /// </summary>
            [Description("未审核")]
            UnAudit
        }
    }
}
