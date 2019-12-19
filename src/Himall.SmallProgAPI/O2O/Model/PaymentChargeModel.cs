using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.O2O.Model
{
    public class PaymentChargeModel
    {
        public string openId { get; set; }
        public string typeId { get; set; }
        public decimal amount { get; set; }
    }
}
