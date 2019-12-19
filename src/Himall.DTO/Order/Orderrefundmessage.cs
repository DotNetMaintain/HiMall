using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    public class Orderrefundmessage
    {
        public long RefundId { get; set; }
        public decimal Amount { get; set; }
        public string remark { get; set; }
        public string operatorname { get; set; }
    }
}
