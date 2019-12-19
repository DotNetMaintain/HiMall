using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Entities
{
    public partial class FightGroupActiveItemInfo
    {
        /// <summary>
        /// 规格名称
        /// </summary>
        public string SkuName { get; set; }
        /// <summary>
        /// 商品售价
        /// </summary>
        public decimal ProductPrice { get; set; }
        /// <summary>
        /// 商品成本价
        /// </summary>
        public decimal ProductCostPrice { get; set; }
        /// <summary>
        /// 已售
        ///</summary>
        public long ProductStock { get; set; }
        /// <summary>
        /// 颜色
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// 尺码
        /// </summary>
        public string Size { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// 显示图片
        /// <para>颜色独有</para>
        /// </summary>
        public string ShowPic { get; set; }
    }
}
