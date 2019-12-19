using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public partial class ShippingFreeGroupInfo
    {
        public virtual ICollection<ShippingFreeRegionInfo> Himall_ShippingFreeRegions { get; set; }
    }
}
