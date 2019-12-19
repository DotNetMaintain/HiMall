using Himall.Core;
using Himall.DTO;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.O2O.Model
{
    public class HomeGetStoreListModel : HomePageShopBranch
    {
        public decimal CommentScore { get; set; }
        public long SaleCount { get; set; }
        public long SaleCountByMonth { get; set; }
        public long CartQuantity { get; set; }
        public List<HomeGetStoreListProductModel> ShowProducts { get; set; }
        public int ProductCount { get; set; }
    }

    public class HomeGetStoreListProductModel
    {
        public long Id { get; set; }
        public string ProductName { get; set; }
        public decimal MarketPrice { get; set; }
        public decimal MinSalePrice { get; set; }
        public bool HasSKU { get; set; }
        public long SaleCount { get; set; }
        public int HighCommentCount { get; set; }

        /// <summary>
        /// 默认图片
        /// </summary>
        public string DefaultImage
        {
            get;
            set;
        }
        
    }
}
