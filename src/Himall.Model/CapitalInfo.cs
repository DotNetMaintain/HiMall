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
    
    public partial class CapitalInfo:BaseModel
    {
        public CapitalInfo()
        {
            this.Himall_CapitalDetail = new HashSet<CapitalDetailInfo>();
        }
    
        long _id;
        public long Id { get{ return _id; } set{ _id=value;} }
        public long MemId { get; set; }
        public Nullable<decimal> Balance { get; set; }
        public Nullable<decimal> FreezeAmount { get; set; }
        public Nullable<decimal> ChargeAmount { get; set; }
    
        public virtual ICollection<CapitalDetailInfo> Himall_CapitalDetail { get; set; }
    }
}
