using Himall.Core;
using Himall.DTO;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Application
{
    public class MobileHomeTopicApplication
    {
        private static IMobileHomeTopicService _iMobileHomeTopicService = ObjectContainer.Current.Resolve<IMobileHomeTopicService>();
        /// <summary>
        /// 获取移动端首页专题设置
        /// </summary>
        /// <param name="platformType">平台类型</param>
        /// <param name="shopId">店铺Id</param>
        /// <returns></returns>
        public static IList<MobileHomeTopicsInfo> GetMobileHomeTopicInfos(PlatformType platformType, long shopId = 0)
        {
            var topicInfos= _iMobileHomeTopicService.GetMobileHomeTopicInfos(platformType, shopId).ToList();
            return topicInfos;
        }
    }
}
