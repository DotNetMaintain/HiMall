using Himall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Entities
{
    public partial class FightGroupActiveInfo
    {
        /// <summary>
        /// 店铺名
        /// <para>手动补充</para>
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 商品图片地址
        /// </summary>
        public string ProductImgPath { get; set; }
        /// <summary>
        /// 拼团活动状态
        /// </summary>
        public FightGroupActiveStatus ActiveStatus
        {
            get
            {
                FightGroupActiveStatus result = FightGroupActiveStatus.Ending;
                if (EndTime < DateTime.Now)
                {
                    result = FightGroupActiveStatus.Ending;
                }
                else
                {
                    if (StartTime > DateTime.Now)
                    {
                        result = FightGroupActiveStatus.WillStart;
                    }
                    else
                    {
                        result = FightGroupActiveStatus.Ongoing;
                    }
                }
                return result;
            }
        }
        /// <summary>
        /// 活动项
        /// <para>手动维护</para>
        /// </summary>
        public List<FightGroupActiveItemInfo> ActiveItems { get; set; }
        /// <summary>
        /// 火拼价
        /// </summary>
        public decimal MiniGroupPrice { get; set; }
        /// <summary>
        /// 最低售价
        /// </summary>
        public decimal MiniSalePrice { get; set; }
        /// <summary>
        /// 运费模板
        /// </summary>
        public long FreightTemplateId { get; set; }
        /// <summary>
        /// 商品广告语
        /// </summary>
        public string ProductShortDescription { get; set; }
        /// <summary>
        /// 商品评价数
        /// </summary>
        public int ProductCommentNumber { get; set; }
        /// <summary>
        /// 商品编码
        /// </summary>
        public string ProductCode { get; set; }
        /// <summary>
        /// 商品单位
        /// </summary>
        public string MeasureUnit { get; set; }
        /// <summary>
        /// 商品是否可购买
        /// </summary>
        public bool CanBuy { get; set; }
        /// <summary>
        /// 商品是否还有库存
        /// </summary>
        public bool HasStock { get; set; }

        /// <summary>
        /// 管理审核状态
        /// </summary>
        public FightGroupManageAuditStatus FightGroupManageAuditStatus
        {
            get
            {
                FightGroupManageAuditStatus result = FightGroupManageAuditStatus.Normal;
                if (ManageAuditStatus == -1)
                {
                    result = FightGroupManageAuditStatus.SoldOut;
                }
                return result;
            }
        }
    }
}
