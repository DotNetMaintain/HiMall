using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.O2O.Model
{
    public class MemberCapitalApplyWithDrawModel
    {
        public MemberCapitalApplyWithDrawModel()
        {
            this.applyType = 1;
        }
        public string openId { get; set; }
        public string nickname { get; set; }
        public decimal amount { get; set; }
        public string pwd { get; set; }
        public int applyType { get; set; }
    }
}
