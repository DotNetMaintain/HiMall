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
    
    public partial class FloorTopicInfo:BaseModel
    {
        long _id;
        public long Id { get{ return _id; } set{ _id=value;} }
        public long FloorId { get; set; }
        public Himall.CommonModel.Position TopicType { get; set; }
        private string topicImage { get; set; }
        public string TopicName { get; set; }
        public string Url { get; set; }
    
        public virtual HomeFloorInfo HomeFloorInfo { get; set; }
    }
}
