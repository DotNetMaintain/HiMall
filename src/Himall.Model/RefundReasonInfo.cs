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
    
    public partial class RefundReasonInfo:BaseModel
    {
        int _id;
        public int Id { get{ return _id; } set{ _id=value;} }
        public string AfterSalesText { get; set; }
        public Nullable<int> Sequence { get; set; }
    }
}
