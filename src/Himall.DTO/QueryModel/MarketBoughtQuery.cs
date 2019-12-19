using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;
using Himall.CommonModel;

namespace Himall.DTO.QueryModel
{
    public partial class MarketBoughtQuery : QueryBase
    {
        /// <summary>
        /// 购买类型
        /// </summary>
        public MarketType? MarketType { get; set; }

        /// <summary>
        /// 店铺名称
        /// </summary>
        public string ShopName { get; set; }

        /// <summary>
        /// 购买记录ID
        /// </summary>
        public long Id { set; get; }

    }
}
