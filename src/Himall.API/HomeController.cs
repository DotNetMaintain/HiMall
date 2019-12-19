using Himall.Application;
using Himall.Core;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Himall.API
{
    public class HomeController : BaseApiController
    {
        //APP首页配置共用于安卓和IOS，这里的平台类型写的为IOS，安卓调用首页接口数据时平台类型也选IOS
        public APPHome Get(int pageNo, int pageSize)
        {
            var slideImageSettings = ServiceProvider.Instance<ISlideAdsService>.Create.GetSlidAds(0, SlideAdInfo.SlideAdType.IOSShopHome);
            //var slides = slideImageSettings.ToArray().Select(item => new HomeSlides { ImageUrl = "http://" + Url.Request.RequestUri.Host + item.ImageUrl, Url = item.Url });
            //var slides = slideImageSettings.ToArray().Select(item => new HomeSlides { ImageUrl = Core.HimallIO.GetRomoteImagePath(item.ImageUrl), Url = item.Url });


            var images = ServiceProvider.Instance<ISlideAdsService>.Create.GetImageAds(0, Himall.CommonModel.ImageAdsType.APPSpecial).ToList();
            //var images = ServiceProvider.Instance<ISlideAdsService>.Create.GetImageAds(0).Take(5).ToList();
            //var homeImage = images.Select(item => new HomeImage
            //    {
            //        //ImageUrl = "http://" + Url.Request.RequestUri.Host + item.ImageUrl,
            //        ImageUrl = Core.HimallIO.GetRomoteImagePath(item.ImageUrl),
            //        Url = item.Url
            //    });
            var mhproser = ServiceProvider.Instance<IMobileHomeProductsService>.Create;
            var totalProducts = mhproser.GetMobileHomePageProducts(0, Core.PlatformType.IOS).Count();
            var homeProducts = mhproser.GetMobileHomePageProducts(0, Core.PlatformType.IOS)
                .OrderBy(item => item.Sequence).ThenBy(d => d.Id).Skip((pageNo - 1) * pageSize).Take(pageSize);
            decimal discount = 1M;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            var products = new List<HomeProduct>();
            if (homeProducts != null)
            {
                var limitService = ServiceProvider.Instance<ILimitTimeBuyService>.Create;
                var fight = FightGroupApplication.GetFightGroupPrice();
                foreach (var item in homeProducts.ToArray())
                {
                    var limitBuy = limitService.GetLimitTimeMarketItemByProductId(item.ProductId);
                    decimal minSalePrice = item.Himall_Products.Himall_Shops.IsSelf ? item.Himall_Products.MinSalePrice * discount : item.Himall_Products.MinSalePrice;
                    var isValidLimitBuy = "false";
                    if (limitBuy != null)
                    {
                        minSalePrice = limitBuy.MinPrice; //限时购不打折
                    }
                    var isFight = fight.Where(r => r.ProductId == item.ProductId).FirstOrDefault();
                    long activeId = 0;
                    if (isFight != null)
                    {
                        minSalePrice = isFight.ActivePrice;
                        activeId = isFight.ActiveId;
                    }
                    products.Add(new HomeProduct()
                    {
                        Id = item.ProductId.ToString(),
                        //    //ImageUrl = "http://" + Url.Request.RequestUri.Host + item.Himall_Products.GetImage(ProductInfo.ImageSize.Size_220),
                        ImageUrl = Core.HimallIO.GetRomoteProductSizeImage(item.Himall_Products.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_220),
                        Name = item.Himall_Products.ProductName,
                        MarketPrice = item.Himall_Products.MarketPrice.ToString(),
                        SalePrice = minSalePrice.ToString("f2"),
                        Discount = (minSalePrice / item.Himall_Products.MarketPrice).ToString("0.0"),
                        //Url = "http://" + Url.Request.RequestUri.Host + "/m-ios/product/detail/" + item.ProductId
                        Url = Core.HimallIO.GetRomoteImagePath("/m-ios/product/detail/" + item.ProductId),
                        FightGroupId = activeId
                    });
                }
            }
            //var products = homeProducts.ToArray().Select(item => new HomeProduct
            //{ //CommentsCount=item.Himall_Products.Himall_Shops.IsSelf
            //    Id = item.ProductId.ToString(),
            //    //ImageUrl = "http://" + Url.Request.RequestUri.Host + item.Himall_Products.GetImage(ProductInfo.ImageSize.Size_220),
            //    ImageUrl = Core.HimallIO.GetRomoteProductSizeImage(item.Himall_Products.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_220),
            //    Name = item.Himall_Products.ProductName,
            //    MarketPrice = item.Himall_Products.MarketPrice.ToString(),
            //    //  SalePrice = item.Himall_Products.MinSalePrice.ToString(),
            //    SalePrice = item.Himall_Products.Himall_Shops.IsSelf ? item.Himall_Products.MinSalePrice * discount.ToString() : item.Himall_Products.MinSalePrice.ToString(),

            //    Discount = (item.Himall_Products.MinSalePrice / item.Himall_Products.MarketPrice).ToString("0.0"),
            //    //Url = "http://" + Url.Request.RequestUri.Host + "/m-ios/product/detail/" + item.ProductId
            //    Url = Core.HimallIO.GetRomoteImagePath("/m-ios/product/detail/" + item.ProductId)
            //});

            var iconSettings = ServiceProvider.Instance<ISlideAdsService>.Create.GetSlidAds(0, SlideAdInfo.SlideAdType.APPIcon);
            var icon = iconSettings.ToArray().Select(item => new HomeSlides { Desc = item.Description, ImageUrl = Core.HimallIO.GetRomoteImagePath(item.ImageUrl), Url = item.Url });

            var services = CustomerServiceApplication.GetPlatformCustomerService(true, true);
            var meiqia = CustomerServiceApplication.GetPlatformCustomerService(true, false).FirstOrDefault(p => p.Tool == CustomerServiceInfo.ServiceTool.MeiQia);
            if (meiqia != null)
                services.Insert(0, meiqia);

            APPHome appHome = new APPHome();
            appHome.success = true;
            //2017年9月1号 商城首页接口修改（把原广告图片的去掉，只保留商品）
            appHome.TotalProduct = totalProducts;
            appHome.Icon = icon;
            //appHome.Slide = slides;//轮播图数组
            //appHome.Topic = homeImage;//专题数组
            appHome.Product = products;
            appHome.CustomerServices = services;
            return appHome;
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
            if (string.IsNullOrWhiteSpace(siteSetting.AppVersion))
            {
                siteSetting.AppVersion = "0.0.0";
            }
            var downLoadUrl = "";
            Version v1 = new Version(siteSetting.AppVersion), v2 = new Version(appVersion);
            if (v1 > v2)
            {
                if (type == (int)PlatformType.IOS)
                {
                    if (string.IsNullOrWhiteSpace(siteSetting.IOSDownLoad))
                    {
                        return ErrorResult("站点未设置IOS下载地址", 10004);
                    }
                    downLoadUrl = siteSetting.IOSDownLoad;
                }
                if (type == (int)PlatformType.Android)
                {
                    if (string.IsNullOrWhiteSpace(siteSetting.AndriodDownLoad))
                    {
                        return ErrorResult("站点未设置Andriod下载地址", 10003);
                    }
                    string str = siteSetting.AndriodDownLoad.Substring(siteSetting.AndriodDownLoad.LastIndexOf("/"), siteSetting.AndriodDownLoad.Length - siteSetting.AndriodDownLoad.LastIndexOf("/"));
                    var curProjRootPath = System.Web.Hosting.HostingEnvironment.MapPath("~/app") + str;
                    if (!File.Exists(curProjRootPath))
                    {
                        return ErrorResult("站点未上传app安装包", 10002);
                    }
                    downLoadUrl = siteSetting.AndriodDownLoad;
                }
            }
            else
            {
                return ErrorResult("当前为最新版本", 10001);
            }
            dynamic result = SuccessResult();
            result.code = 10000;
            result.DownLoadUrl = downLoadUrl;
            result.Description = siteSetting.AppUpdateDescription;

            return result;
        }

        /// <summary>
        /// 获取App引导页图片
        /// </summary>
        /// <returns></returns>
        public List<Himall.DTO.SlideAdModel> GetAppGuidePages()
        {
            var result = SlideApplication.GetGuidePages();
            foreach (var item in result)
            {
                item.ImageUrl = HimallIO.GetRomoteImagePath(item.ImageUrl);
            }
            if (result == null)
            {
                result = new List<DTO.SlideAdModel>();
            }
            return result;
        }


        public object GetAboutUs()
        {
            var appModel = SystemAgreementApplication.GetAgreement(AgreementInfo.AgreementTypes.APP);
            var content = string.Empty;
            if (appModel != null)
                content = appModel.AgreementContent.Replace("src=\"/Storage/", "src=\"" + Core.HimallIO.GetRomoteImagePath("/Storage/"));
            return SuccessResult(content);
        }
    }
}
