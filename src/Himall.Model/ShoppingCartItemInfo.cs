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
    
    internal partial class ShoppingCartItemInfo:BaseModel
    {
        long _id;
        public long Id { get{ return _id; } set{ _id=value;} }
        public long UserId { get; set; }
        public Nullable<long> ShopBranchId { get; set; }
        public long ProductId { get; set; }
        public string SkuId { get; set; }
        public int Quantity { get; set; }
        public System.DateTime AddTime { get; set; }
    
        public virtual UserMemberInfo MemberInfo { get; set; }
        public virtual ProductInfo ProductInfo { get; set; }
    }
}
