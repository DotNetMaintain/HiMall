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
    
    public partial class ProductDescriptionTemplateInfo:BaseModel
    {
        long _id;
        public long Id { get{ return _id; } set{ _id=value;} }
        public long ShopId { get; set; }
        public string Name { get; set; }
        public ProductDescriptionTemplateInfo.TemplatePosition Position { get; set; }
        public string Content { get; set; }
        public string MobileContent { get; set; }
    }
}
