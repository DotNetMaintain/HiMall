using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    public enum WXMsgTemplateType : byte
    {
        /// <summary>
        /// 微信商城
        /// </summary>
        WeiXinShop = 0,
        /// <summary>
        /// 小程序
        /// </summary>
        Applet = 1,
        /// <summary>
        /// O2O小程序
        /// </summary>
        O2OApplet = 2
    }
}
