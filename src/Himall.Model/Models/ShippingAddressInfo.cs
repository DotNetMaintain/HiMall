using System.ComponentModel.DataAnnotations.Schema;

namespace Himall.Model
{
    public partial class ShippingAddressInfo
    {
        public ShippingAddressInfo()
        {
            NeedUpdate = true;
        }

        [NotMapped]
        public string RegionFullName { get; set; }

        [NotMapped]
        public string RegionIdPath { set; get; }
        [NotMapped]
        public bool NeedUpdate { get; set; }
        [NotMapped]
        public int IsNeedUpdate { get { return NeedUpdate ? 1 : 0; } }

        public bool CanDelive { get; set; }
        public long? shopBranchId { get; set; }
    }
}
