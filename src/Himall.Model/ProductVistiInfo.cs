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
    
    public partial class ProductVistiInfo:BaseModel
    {
        long _id;
        public long Id { get{ return _id; } set{ _id=value;} }
        public long ProductId { get; set; }
        public System.DateTime Date { get; set; }
        public long VistiCounts { get; set; }
        public long SaleCounts { get; set; }
        public decimal SaleAmounts { get; set; }
        public Nullable<long> OrderCounts { get; set; }
        public long ShopId { get; set; }
        public long VisitUserCounts { get; set; }
        public long PayUserCounts { get; set; }
        public bool StatisticFlag { get; set; }
    
        public virtual ProductInfo ProductInfo { get; set; }
    }
}
