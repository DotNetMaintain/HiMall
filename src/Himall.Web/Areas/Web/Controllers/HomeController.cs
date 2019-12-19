using Himall.CommonModel;
using Himall.Core.Plugins.OAuth;
using Himall.DTO;
using Himall.DTO.QueryModel;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.Web.Models;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Himall.Application;
using Himall.Core;
using Himall.Web.Models;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;

namespace Himall.Web.Areas.Web.Controllers
{
    public class HomeController : BaseWebController
    {
        private IMemberService _iMemberService;
        private ISlideAdsService _iSlideAdsService;
        private IFloorService _iFloorService;
        private IArticleCategoryService _iArticleCategoryService;
        private IArticleService _iArticleService;
        private IBrandService _iBrandService;
        private ILimitTimeBuyService _iLimitTimeBuyService;
        private ISiteSettingService _iSiteSettingService;
        const string _themesettings = "~/Areas/Admin/Views/PageSettings/themesetting.json";
        const string _templatesettings = "~/Areas/Admin/Views/PageSettings/templatesettings.json";

        public HomeController(
            IMemberService iMemberService,
            ISlideAdsService iSlideAdsService,
            IFloorService iFloorService,
            IArticleCategoryService iArticleCategoryService,
            IArticleService iArticleService,
            IBrandService iBrandService,
            ILimitTimeBuyService iLimitTimeBuyService,
            ISiteSettingService iSiteSettingService
            )
        {
            _iMemberService = iMemberService;
            _iSlideAdsService = iSlideAdsService;
            _iFloorService = iFloorService;
            _iArticleCategoryService = iArticleCategoryService;
            _iArticleService = iArticleService;
            _iBrandService = iBrandService;
            _iLimitTimeBuyService = iLimitTimeBuyService;
            _iSiteSettingService = iSiteSettingService;
        }
        private bool IsInstalled()
        {
            var t = ConfigurationManager.AppSettings["IsInstalled"];
            return null == t || bool.Parse(t);
        }

        //#if !DEBUG
        //               [OutputCache(Duration = ConstValues.PAGE_CACHE_DURATION)]
        //#endif
        //[OutputCache(Duration = ConstValues.PAGE_CACHE_DURATION)]
        [HttpGet]
        public ActionResult Index()
        {
            if (!IsInstalled())
            {
                return RedirectToAction("Agreement", "Installer");
            }
            return File(Server.MapPath("~/Areas/Web/Views/Home/index1.html"), "text/html");
            //var ser_user = _iMemberService;

            #region 初始化首页数据
            //var homePageModel = new HomePageModel();

            //if (CurrentSiteSetting.AdvertisementState)
            //{
            //	homePageModel.AdvertisementUrl = CurrentSiteSetting.AdvertisementUrl;
            //	homePageModel.AdvertisementImagePath = CurrentSiteSetting.AdvertisementImagePath;
            //}

            ////获取信任登录插件需要在首页head中填充的验证内容
            //ViewBag.OAuthValidateContents = GetOAuthValidateContents();
            //homePageModel.SiteName = CurrentSiteSetting.SiteName;
            //homePageModel.Title = string.IsNullOrWhiteSpace(CurrentSiteSetting.Site_SEOTitle) ? "商城首页" : CurrentSiteSetting.Site_SEOTitle;
            //var view = ViewEngines.Engines.FindView(ControllerContext, "Index", null);
            //List<HomeFloorModel> floorModels = new List<HomeFloorModel>();
            //homePageModel.handImage = _iSlideAdsService.GetHandSlidAds().ToList();
            //var silder = _iSlideAdsService.GetSlidAds(0, SlideAdInfo.SlideAdType.PlatformHome).ToList();
            //homePageModel.slideImage = silder;
            //var imageAds = _iSlideAdsService.GetImageAds(0).ToList();
            ////人气单品
            //homePageModel.imageAds = imageAds.Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.Single).ToList();
            ////banner右侧广告
            //homePageModel.imageAdsTop = imageAds.Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.BannerAds).ToList();

            //homePageModel.CenterAds = imageAds.Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.Customize).ToList();
            //homePageModel.ShopAds = imageAds.Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.BrandsAds).ToList();

            //if (homePageModel.imageAds.Count == 0)
            //{
            //	homePageModel.imageAds = imageAds.Take(8).ToList();
            //}
            //if (homePageModel.imageAdsTop.Count == 0)
            //{
            //	homePageModel.imageAdsTop = imageAds.Take(2).ToList();
            //}
            //if (homePageModel.CenterAds.Count == 0)
            //{
            //	homePageModel.CenterAds = imageAds.Take(3).ToList();
            //}
            //if (homePageModel.ShopAds.Count == 0)
            //{
            //	homePageModel.ShopAds = imageAds.Take(2).ToList();
            //}
            ///*没地方用，先去掉
            //var articleService = ServiceApplication.Create<IArticleService>();
            //ViewBag.ArticleTabs = new List<IQueryable<ArticleInfo>>()
            //{   articleService.GetTopNArticle<ArticleInfo>(8, 4),
            //	articleService.GetTopNArticle<ArticleInfo>(8, 5),
            //	articleService.GetTopNArticle<ArticleInfo>(8, 6),
            //	articleService.GetTopNArticle<ArticleInfo>(8, 7)
            //};
            //*/

            ////楼层数据
            //var floors = _iFloorService.GetHomeFloors().ToList();
            //foreach (var f in floors)
            //{
            //	var model = new HomeFloorModel();
            //	var texts = f.FloorTopicInfo.Where(a => a.TopicType == Position.Top).ToList();
            //	var products = f.FloorTopicInfo.Where(a => a.TopicType != Position.Top).ToList();
            //	var productModules = f.FloorProductInfo.ToList();
            //	var brands = f.FloorBrandInfo.Take(10).ToList();
            //	model.Name = f.FloorName;
            //	model.SubName = f.SubName;
            //	model.StyleLevel = f.StyleLevel;
            //	model.DefaultTabName = f.DefaultTabName;

            //	//文本设置
            //	foreach (var s in texts)
            //	{
            //		model.TextLinks.Add(new HomeFloorModel.WebFloorTextLink()
            //		{
            //			Id = s.Id,
            //			Name = s.TopicName,
            //			Url = s.Url
            //		});
            //	}

            //	//广告设置
            //	foreach (var s in products)
            //	{
            //		model.Products.Add(new HomeFloorModel.WebFloorProductLinks
            //		{
            //			Id = s.Id,
            //			ImageUrl = s.TopicImage,
            //			Url = s.Url,
            //			Type = s.TopicType
            //		});
            //	}

            //	//推荐品牌
            //	foreach (var s in brands)
            //	{
            //		model.Brands.Add(new WebFloorBrand
            //		{
            //			Id = s.BrandInfo.Id,
            //			Img = s.BrandInfo.Logo,
            //			Url = "",
            //			Name = s.BrandInfo.Name
            //		});
            //	}

            //	//推荐商品
            //	foreach (var s in productModules)
            //	{
            //		model.ProductModules.Add(new HomeFloorModel.ProductModule
            //		{
            //			Id = s.Id,
            //			ProductId = s.ProductId,
            //			MarketPrice = s.ProductInfo.MarketPrice,
            //			price = s.ProductInfo.MinSalePrice,
            //			productImg = Himall.Core.HimallIO.GetProductSizeImage(s.ProductInfo.ImagePath, 1, (int)ImageSize.Size_350),
            //			productName = s.ProductInfo.ProductName,
            //			Tab = s.Tab
            //		});
            //	}

            //             if (model.StyleLevel == 1 || model.StyleLevel == 4 || model.StyleLevel == 5 || model.StyleLevel == 6 || model.StyleLevel == 7)
            //	{
            //		model.Tabs = f.Himall_FloorTabls.OrderBy(p => p.Id).Select(p => new Himall.Web.Areas.Web.Models.HomeFloorModel.Tab()
            //		 {
            //			 Name = p.Name,
            //			 Detail = p.Himall_FloorTablDetails.ToList()
            //			 .Where(d => d.Himall_Products.AuditStatus == ProductInfo.ProductAuditStatus.Audited
            //			 && d.Himall_Products.SaleStatus == ProductInfo.ProductSaleStatus.OnSale)
            //			 .Select(d => new Himall.Web.Areas.Web.Models.HomeFloorModel.ProductDetail()
            //			 {
            //				 ProductId = d.Himall_Products.Id,
            //				 ImagePath = Himall.Core.HimallIO.GetProductSizeImage(d.Himall_Products.ImagePath, 1, (int)ImageSize.Size_350),
            //				 Price = d.Himall_Products.MinSalePrice,
            //				 Name = d.Himall_Products.ProductName
            //			 }).ToList()
            //		 }).ToList();

            //		model.Scrolls = model.Products.Where(p => p.Type == Position.ScrollOne || p.Type == Position.ScrollTwo
            //																	|| p.Type == Position.ScrollThree || p.Type == Position.ScrollFour).ToList();

            //		model.RightTops = model.Products.Where(p => p.Type == Position.ROne || p.Type == Position.RTwo
            //																	|| p.Type == Position.RThree || p.Type == Position.RFour).ToList();

            //		model.RightBottons = model.Products.Where(p => p.Type == Position.RFive || p.Type == Position.RSix
            //																	|| p.Type == Position.RSeven || p.Type == Position.REight).ToList();
            //	}
            //	floorModels.Add(model);
            //}
            //homePageModel.floorModels = floorModels;

            ////全部品牌
            //HomeBrands homeBrands = new HomeBrands();
            //var listBrands = Application.BrandApplication.GetBrands(null);
            //foreach (var item in listBrands)
            //{
            //	homeBrands.listBrands.Add(new WebFloorBrand
            //	{
            //		Id = item.Id,
            //		Img = item.Logo,
            //		Url = "",
            //		Name = item.Name
            //	});
            //}
            //homePageModel.brands = homeBrands;

            ////限时购
            //var setting = _iSiteSettingService.GetSiteSettings();
            //if (setting.Limittime)
            //	homePageModel.FlashSaleModel = _iLimitTimeBuyService.GetRecentFlashSale();
            //else
            //{
            //	homePageModel.FlashSaleModel = new List<FlashSaleModel>();
            //}

            //return View(homePageModel);
            #endregion
        }

        public ActionResult Index1()
        {
            return File(Server.MapPath("~/Areas/Web/Views/Home/index1.html"), "text/html");
        }

        public ActionResult Test()
        {
            return View();
        }

        /// <summary>n
        /// 用于响应SLB，直接返回
        /// </summary>
        /// <returns></returns>
        [HttpHead]
        public ContentResult Index(string s)
        {
            return Content("");
        }


        IEnumerable<string> GetOAuthValidateContents()
        {
            var oauthPlugins = Core.PluginsManagement.GetPlugins<IOAuthPlugin>(true);
            return oauthPlugins.Select(item => item.Biz.GetValidateContent());
        }

        [HttpPost]
        public JsonResult GetProducts(long[] ids)
        {
            var products = ProductManagerApplication.GetProductByIds(ids).ToList().Select(item => new
            {
                item.Id,
                item.ProductName,
                item.MarketPrice,
                item.MinSalePrice,
                item.SaleStatus,
                item.ImagePath
            });

            return Json(products, true);
        }

        /// <summary>
        /// 获取限时购商品
        /// </summary>
        /// <param name="ids">商品ID集合</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetLimitedProducts(List<long> ids)
        {
            var result = LimitTimeApplication.GetPriceByProducrIds(ids).ToList();
            var productIds = result.Select(p => p.ProductId);
            var skuInfos = SKUApplication.GetByProductIds(productIds);
            var products = result.Select(item => new
            {
                MinPrice = item.MinPrice,
                ProductId = item.ProductId,
                Count = skuInfos.Where(a => a.ProductId == item.ProductId).Sum(b => b.Stock)//每个限时购商品取该所有SKU的库存之和
            });

            return Json(products, true);
        }

        /// <summary>
        /// 首页动态获取我的积分、优惠券、已领取的优惠券
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Coupon()
        {
            int memberIntegral = 0; List<Coupon> baseCoupons = new List<Coupon>();
            long shopId = CurrentSellerManager != null ? CurrentSellerManager.ShopId : 0;
            if (CurrentUser != null)
            {
                memberIntegral = MemberIntegralApplication.GetMemberIntegral(CurrentUser.Id).AvailableIntegrals;

                //优惠卷
                var coupons = CouponApplication.GetAllUserCoupon(CurrentUser.Id).ToList();
                coupons = coupons == null ? new List<UserCouponInfo>() : coupons;
                if (coupons != null)
                {
                    baseCoupons.AddRange(coupons.Select(p => new Coupon()
                    {
                        BasePrice = p.BasePrice,
                        BaseShopId = p.BaseShopId,
                        BaseShopName = p.BaseShopName,
                        BaseType = p.BaseType,
                        OrderAmount = p.OrderAmount
                    }));
                }

                //红包
                var shopBonus = ShopBonusApplication.GetCanUseDetailByUserId(CurrentUser.Id);
                shopBonus = shopBonus == null ? new List<ShopBonusReceiveInfo>() : shopBonus;
                if (shopBonus != null)
                {
                    baseCoupons.AddRange(shopBonus.Select(p => new Coupon()
                    {
                        BasePrice = p.BasePrice,
                        BaseShopId = p.BaseShopId,
                        BaseShopName = p.BaseShopName,
                        BaseType = p.BaseType,
                        UseState = p.Himall_ShopBonusGrant.Himall_ShopBonus.UseState,
                        UsrStatePrice = p.Himall_ShopBonusGrant.Himall_ShopBonus.UsrStatePrice
                    }));
                }
            }
            return Json(new
            {
                memberIntegral = memberIntegral,
                baseCoupons = baseCoupons,
                shopId = shopId
            }, true);
        }

        // GET: Web/Home
        public ActionResult Index2()
        {
            BranchShopDayFeatsQuery query = new BranchShopDayFeatsQuery();
            query.StartDate = DateTime.Now.Date.AddDays(-10);
            query.EndDate = DateTime.Now.Date;
            query.ShopId = 288;
            query.BranchShopId = 21;
            var model = Himall.Application.OrderAndSaleStatisticsApplication.GetDayAmountSale(query);
            ViewBag.Keyword = CurrentSiteSetting.Keyword;
            return View();
        }

        [HttpGet]
        public JsonResult GetFoot()
        {
            var articleCategoryService = _iArticleCategoryService;
            var articleService = _iArticleService;
            //服务文章
            var pageFootServiceCategory = articleCategoryService.GetSpecialArticleCategory(SpecialCategory.PageFootService);
            if (pageFootServiceCategory == null)
            {
                return Json(new List<PageFootServiceModel>(), JsonRequestBehavior.AllowGet);
            }
            var pageFootServiceSubCategies = articleCategoryService.GetArticleCategoriesByParentId(pageFootServiceCategory.Id);
            var pageFootService = pageFootServiceSubCategies.ToArray().Select(item =>
                 new PageFootServiceModel()
                 {
                     CateogryName = item.Name,
                     Articles = articleService.GetArticleByArticleCategoryId(item.Id).Where(t => t.IsRelease)
                 }
                );
            var PageFootService = pageFootService;
            return Json(PageFootService, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TestLogin()
        {
            ViewBag.Keyword = CurrentSiteSetting.Keyword;
            return View();
        }


        /// <summary>
        /// 获取主题配色json
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetThemeSettingJson()
        {
            string currentTempdate = System.IO.File.ReadAllText(this.Server.MapPath(_templatesettings));//读取当前应用的模板
            TemplateSetting curTemplateObj = ParseFormJson<TemplateSetting>(currentTempdate);
            if (curTemplateObj != null)
            {
                if (System.IO.File.Exists(this.Server.MapPath(_themesettings)))
                {
                    string currentTheme = System.IO.File.ReadAllText(this.Server.MapPath(_themesettings));//读取当前模板应用的主题配色
                    List<ThemeSetting> curThemeObjs = ParseFormJson<List<ThemeSetting>>(currentTheme);
                    if (curThemeObjs != null && curThemeObjs.Count > 0)
                    {
                        var info = curThemeObjs.FirstOrDefault(a => a.templateId == curTemplateObj.Id);
                        if (null != info)
                        {
                            return Json(info, true);
                        }
                    }
                }
            }
            return Json(null, true);
        }


        [HttpPost]
        public JsonResult GetFootNew()
        {
            //页脚
            var articleCategoryService = _iArticleCategoryService;
            var articleService = _iArticleService;
            //服务文章
            var pageFootServiceCategory = articleCategoryService.GetSpecialArticleCategory(SpecialCategory.PageFootService);
            if (pageFootServiceCategory == null) { return Json(null); }
            var pageFootServiceSubCategies = articleCategoryService.GetArticleCategoriesByParentId(pageFootServiceCategory.Id).ToList();
            dynamic noticeInfo = new System.Dynamic.ExpandoObject();
            var allArticle = articleService.GetArticleByArticleCategoryIds(pageFootServiceSubCategies.Select(a => a.Id).ToList()).Where(p => p.IsRelease).ToList();
            FootNoticeModel info = null;
            List<FootNoticeModel> resultList = new List<FootNoticeModel>();
            pageFootServiceSubCategies.ForEach(p =>
            {
                info = new FootNoticeModel()
                {
                    CateogryName = p.Name,
                    List = allArticle.Where(x => x.CategoryId == p.Id).Select(y => new ArticleInfo
                    {
                        Id = y.Id,
                        Title = y.Title
                    }).ToList()
                };
                resultList.Add(info);
            });
            noticeInfo.PageFootService = resultList;
            //页脚
            noticeInfo.PageFoot = CurrentSiteSetting.PageFoot;
            noticeInfo.QRCode = CurrentSiteSetting.QRCode;
            noticeInfo.SiteName = CurrentSiteSetting.SiteName;
            noticeInfo.Logo = CurrentSiteSetting.Logo;
            noticeInfo.PCBottomPic = CurrentSiteSetting.PCBottomPic;
            return Json(noticeInfo, true);
        }


        [HttpPost]
        public JsonResult GetNotice()
        {
            var specialArticleInfo = ObjectContainer.Current.Resolve<IArticleCategoryService>().GetSpecialArticleCategory(SpecialCategory.InfoCenter);
            if (specialArticleInfo != null)
            {
                var result = ObjectContainer.Current.Resolve<IArticleService>().GetTopNArticle<ArticleInfo>(7, specialArticleInfo.Id);
                var notice = result.Select(p => new
                {
                    url = "/Article/Details/" + p.Id,
                    title = p.Title
                });
                return Json(notice, true);
            }
            return Json(null);
        }

        public static T ParseFormJson<T>(string szJson)
        {
            T obj = Activator.CreateInstance<T>();
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(szJson)))
            {
                DataContractJsonSerializer dcj = new DataContractJsonSerializer(typeof(T));
                return (T)dcj.ReadObject(ms);
            }
        }


        [HttpPost]
        public JsonResult GetBrands()
        {
            var result = BrandApplication.GetBrands("", 1, int.MaxValue).Models;
            var brands = result.Select(item => new
            {
                BrandName = item.Name,
                BrandLogo = Core.HimallIO.GetImagePath(item.Logo),
                Id = item.Id
            });
            return Json(brands, true);
        }
    }
}