using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model.Models
{
    public class MyMemberOpenId
    {
        long _id;
        public long Id { get { return _id; } set { _id = value; } }
        public long UserId { get; set; }
        public string OpenId { get; set; }
        public string ServiceProvider { get; set; }
        public MemberOpenIdInfo.AppIdTypeEnum AppIdType { get; set; }
        public string UnionId { get; set; }
        public string UnionOpenId { get; set; }
    }
}
