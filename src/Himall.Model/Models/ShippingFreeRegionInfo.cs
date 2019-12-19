using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public partial class ShippingFreeRegionInfo
    {
        /// <summary>
        /// 省会ID
        /// </summary>
        public int ProvinceId { get; set; }

        /// <summary>
        /// 省会名称
        /// </summary>
        public string ProvinceName { set; get; }
        /// <summary>
        /// 市ID
        /// </summary>
        public Nullable<int> CityId { get; set; }

        /// <summary>
        /// 市区名称
        /// </summary>
        public string CityName { set; get; }
        /// <summary>
        /// 县/区ID
        /// </summary>
        public Nullable<int> CountyId { get; set; }

        /// <summary>
        /// 县区名称
        /// </summary>
        public string CountyName { set; get; }

        /// <summary>
        /// 乡镇ID
        /// </summary>
        public string TownIds { get; set; }

        /// <summary>
        /// 乡镇名称
        /// </summary>
        public string TownNames { set; get; }
        public List<int> RegionSubList { get; set; }
    }
}
