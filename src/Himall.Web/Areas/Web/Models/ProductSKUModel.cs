using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Web.Models
{
    public class ProductSKUModel
    {
        public string SkuId { get; set; }
        public decimal Price { get; set; }
        public long Stock { get; set; }
        public int cartCount { get; set; }

        public int minMath { get; set; }
    }
}