using Himall.CommonModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Entities
{
    public partial class FightGroupInfo
    {
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { get; set; }
        /// <summary>
        /// 商品名称
        ///</summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 商品图片目录
        /// </summary>
        public string ProductImgPath { get; set; }
        /// <summary>
        /// 商品默认图片
        /// </summary>
        public string ProductDefaultImage { get; set; }
        /// <summary>
        /// 团长用户名
        /// </summary>
        public string HeadUserName { get; set; }
        /// <summary>
        /// 团长头像
        /// </summary>
        public string HeadUserIcon { get; set; }
        /// <summary>
        /// 团长头像显示
        /// <para>默认头像值补充</para>
        /// </summary>
        public string ShowHeadUserIcon
        {
            get
            {
                string defualticon = "";
                string result = HeadUserIcon;
                if (string.IsNullOrWhiteSpace(result))
                {
                    result = defualticon;
                }
                return result;
            }
        }
        /// <summary>
        /// 数据状态 成团中  成功   失败
        ///</summary>
        public FightGroupBuildStatus BuildStatus
        {
            get
            {
                return (FightGroupBuildStatus)this.GroupStatus;
            }
        }
        /// <summary>
        /// 拼团订单集
        /// </summary>
        public List<FightGroupOrderInfo> GroupOrders { get; set; }
        /// <summary>
        /// 团组时限（秒）
        /// </summary>
        public int? Seconds { get; set; }
    }
}
