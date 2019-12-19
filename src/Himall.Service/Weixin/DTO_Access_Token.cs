using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Service.Weixin
{
    public class DTO_Access_Token
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
    }
}
