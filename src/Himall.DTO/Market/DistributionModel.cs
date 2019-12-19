using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    public class DistributionModel
    {
        public string RealName { set; get; }
        public int TopRegionId { get; set; }
        public int RegionId { get; set; }
        public string Address { get; set; }

        public string Mobile { set; get; }

        public string ShopName { set; get; }

        public long UserId { set; get; }
    }
}
