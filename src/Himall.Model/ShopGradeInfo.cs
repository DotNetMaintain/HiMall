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
    
    public partial class ShopGradeInfo:BaseModel
    {
        long _id;
        public long Id { get{ return _id; } set{ _id=value;} }
        public string Name { get; set; }
        public int ProductLimit { get; set; }
        public int ImageLimit { get; set; }
        public int TemplateLimit { get; set; }
        public decimal ChargeStandard { get; set; }
        public string Remark { get; set; }
    }
}
