//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Himall.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class LimitTimeMarketInfo:BaseModel
    {
        long _id;
        public long Id { get{ return _id; } set{ _id=value;} }
        public string Title { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public LimitTimeMarketAuditStatus AuditStatus { get; set; }
        public System.DateTime AuditTime { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public decimal Price { get; set; }
        public decimal RecentMonthPrice { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
        public int Stock { get; set; }
        public int SaleCount { get; set; }
        public string CancelReson { get; set; }
        public int MaxSaleCount { get; set; }
        public string ProductAd { get; set; }
        public decimal MinPrice { get; set; }
        public string ImagePath { get; set; }
    }
}
