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
    
    public partial class InviteRecordInfo:BaseModel
    {
        long _id;
        public long Id { get{ return _id; } set{ _id=value;} }
        public string UserName { get; set; }
        public string RegName { get; set; }
        public int InviteIntegral { get; set; }
        public Nullable<int> RegIntegral { get; set; }
        public Nullable<System.DateTime> RegTime { get; set; }
        public Nullable<long> UserId { get; set; }
        public Nullable<long> RegUserId { get; set; }
        public Nullable<System.DateTime> RecordTime { get; set; }
    
        public virtual UserMemberInfo InviteMember { get; set; }
        public virtual UserMemberInfo RegMember { get; set; }
    }
}
