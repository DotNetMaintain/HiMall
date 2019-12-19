using System.ComponentModel;

namespace Himall.Core
{
    /// <summary>
    /// 平台类型
    /// </summary>
    public enum PlatformType
    {
        /// <summary>
        /// PC
        /// </summary>
        [Description("PC")]
        PC = 0,

        /// <summary>
        /// 微信
        /// </summary>
        [Description("微信")]
        WeiXin = 4,

        /// <summary>
        /// 安卓
        /// </summary>
        [Description("Android")]
        Android = 2,

        /// <summary>
        /// 苹果
        /// </summary>
        [Description("IOS")]
        IOS = 3,


        /// <summary>
        /// 触屏
        /// </summary>
        [Description("触屏")]
        Wap=4,
        /// <summary>
        /// 微信小程序
        /// </summary>
        [Description("微信小程序")]
        WeiXinSmallProg = 5,
        /// <summary>
        /// 微信多门店小程序
        /// </summary>
        [Description("微信多门店小程序")]
        WeiXinO2OSmallProg = 6,
        /// <summary>
        /// 移动端
        /// </summary>
        [Description("移动端")]
        Mobile = 99,
    }
}
