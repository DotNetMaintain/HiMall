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
    
    public partial class ShopAccountItemInfo:BaseModel
    {
        long _id;
        public long Id { get{ return _id; } set{ _id=value;} }
        public long ShopId { get; set; }
        public string ShopName { get; set; }
        public string AccountNo { get; set; }
        public long AccoutID { get; set; }
        public System.DateTime CreateTime { get; set; }
        public decimal Amount { get; set; }
        public Himall.CommonModel.ShopAccountType TradeType { get; set; }
        public bool IsIncome { get; set; }
        public string ReMark { get; set; }
        public string DetailId { get; set; }
        public decimal Balance { get; set; }
        public int SettlementCycle { get; set; }
    
        public virtual ShopAccountInfo Himall_ShopAccount { get; set; }
    }
}
