using Himall.Core;
using Himall.Core.Helper;
using Himall.Core.Plugins.OAuth;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Himall.API.Model.ParamsModel;
using Himall.API;
using Himall.DTO.QueryModel;
using Himall.Application;
using System.IO;

namespace Himall.API
{
    public class ShopHomeController : BaseShopApiController
    {
        public object GetShopHome()
        {
			CheckUserLogin();

            DateTime nowDt = DateTime.Now;
            //三个月内订单
            OrderQuery query = new OrderQuery() { ShopId=this.CurrentUser.ShopId, StartDate = nowDt.Date.AddDays(-nowDt.Day).AddMonths(-2), EndDate = nowDt };
            var orders = OrderApplication.GetOrdersNoPage(query);
            var threeMonthAmounht = orders.Sum(e => e.ActualPayAmount);
            //从三个月的数据中统计本周的
            DateTime weekStartDt = nowDt.Date.AddDays(-(int)nowDt.DayOfWeek);
            var weekAmount = orders.Where(e => e.OrderDate >= weekStartDt).Sum(e => e.ActualPayAmount);
            //从三个月的数据中统计当天的
            var todayAmount = orders.Where(e => e.OrderDate.Date == nowDt.Date).Sum(e => e.ActualPayAmount);


            //近三天发布商品数
            ProductQuery productQuery = new ProductQuery();

            productQuery.AuditStatus = new []{ProductInfo.ProductAuditStatus.Audited};
            productQuery.StartDate = nowDt.Date.AddDays(-2);
            productQuery.EndDate = nowDt;
            productQuery.PageNo=1;
            productQuery.PageSize=int.MaxValue;

            var products = ProductManagerApplication.GetProducts(productQuery).Models;
            var productCount = products.Select(e =>e.Id).Count();

            RefundQuery refundQuery = new RefundQuery()
            {
                AuditStatus = OrderRefundInfo.OrderRefundAuditStatus.WaitAudit,
                PageNo = 1,
                PageSize = int.MaxValue
            };
            var refunds = RefundApplication.GetOrderRefunds(refundQuery);
            var refundCount = refunds.Total;
            return new
            {
                success = true,
                data = new
                {
                    shopName =CurrentShop.ShopName,
                    todayAmount = todayAmount,
                    weekAmount = weekAmount,
                    threeMonthAmounht = threeMonthAmounht,
                    createProductCount = productCount,
                    refundCount = refundCount
                }
            };
        }
        
        public object GetUpdateApp(string appVersion, int type)
        {
            var siteSetting = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings();

            if (string.IsNullOrWhiteSpace(appVersion) || (3 < type && type < 2))
            {
                return ErrorResult("版本号不能为空或者平台类型错误", 10006);
            }
            Version ver = null;
            try
            {
                ver = new Version(appVersion);
            }
            catch (Exception)
            {
                return ErrorResult("错误的版本号", 10005);
            }
            if (string.IsNullOrWhiteSpace(siteSetting.ShopAppVersion))
            {
                siteSetting.ShopAppVersion = "0.0.0";
            }
            var downLoadUrl = "";
            Version v1 = new Version(siteSetting.ShopAppVersion), v2 = new Version(appVersion);
            if (v1 > v2)
            {
                if (type == (int)PlatformType.IOS)
                {
                    if (string.IsNullOrWhiteSpace(siteSetting.ShopIOSDownLoad))
                    {
                        return ErrorResult("站点未设置IOS下载地址", 10004);
                    }
                    downLoadUrl = siteSetting.ShopIOSDownLoad;
                }
                if (type == (int)PlatformType.Android)
                {
                    if (string.IsNullOrWhiteSpace(siteSetting.ShopAndriodDownLoad))
                    {
                        return ErrorResult("站点未设置Andriod下载地址", 10003);
                    }
                    string str = siteSetting.ShopAndriodDownLoad.Substring(siteSetting.ShopAndriodDownLoad.LastIndexOf("/"), siteSetting.ShopAndriodDownLoad.Length - siteSetting.ShopAndriodDownLoad.LastIndexOf("/"));
                    var curProjRootPath = System.Web.Hosting.HostingEnvironment.MapPath("~/app") + str;
                    if (!File.Exists(curProjRootPath))
                    {
                        return ErrorResult("站点未上传app安装包", 10002);
                    }
                    downLoadUrl = siteSetting.ShopAndriodDownLoad;
                }
            }
            else
            {
                return ErrorResult("当前为最新版本", 10001);
            }

            return new { success = true, code = 10000, DownLoadUrl = downLoadUrl, Description = siteSetting.AppUpdateDescription };
        }
        /// <summary>
        /// 获取未读消息数
        /// </summary>
        /// <returns></returns>
        public object GetNoReadMessageCount()
        {
            CheckUserLogin();
            long shopid = CurrentUser.ShopId;
            int count = AppMessageApplication.GetShopNoReadMessageCount(shopid);
            return new { success = true, count = count };
        }
    }
}
