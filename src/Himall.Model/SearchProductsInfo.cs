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
    
    public partial class SearchProductsInfo:BaseModel
    {
        long _id;
        public long Id { get{ return _id; } set{ _id=value;} }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public Nullable<long> BrandId { get; set; }
        public string BrandName { get; set; }
        public string BrandLogo { get; set; }
        public long FirstCateId { get; set; }
        public string FirstCateName { get; set; }
        public long SecondCateId { get; set; }
        public string SecondCateName { get; set; }
        public long ThirdCateId { get; set; }
        public string ThirdCateName { get; set; }
        public string AttrValues { get; set; }
        public int Comments { get; set; }
        public int SaleCount { get; set; }
        public decimal SalePrice { get; set; }
        public Nullable<System.DateTime> OnSaleTime { get; set; }
        public string ImagePath { get; set; }
        public bool CanSearch { get; set; }
    }
}
