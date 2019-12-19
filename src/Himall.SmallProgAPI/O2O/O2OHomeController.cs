using AutoMapper;
using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.DTO;
using Himall.DTO.QueryModel;
using Himall.Model;
using Himall.SmallProgAPI.O2O.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;

namespace Himall.SmallProgAPI
{
    public class O2OHomeController : BaseO2OApiController
    {
        /// <summary>
        /// 多门店首页
        /// </summary>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetIndexData()
        {
            CheckOpenStore();

            var model = SlideApplication.GetShopBranchListSlide();
            var defaultImage = new Himall.DTO.SlideAdModel { };
            var adimgs = model.Where(e => e.TypeId == Himall.Model.SlideAdInfo.SlideAdType.NearShopBranchSpecial);
            var siteinfo = SiteSettingApplication.GetSiteSettings();
            dynamic result = new ExpandoObject();
            result.QQMapKey = CommonConst.QQMapKey;
            if (siteinfo.O2OApplet_IsUseTopSlide)
            {
                result.TopSlide = model.Where(e => e.TypeId == Himall.Model.SlideAdInfo.SlideAdType.NearShopBranchHome).ToList(); //顶部轮播图
            }
            else
            {
                result.TopSlide = null;
            }
            if (siteinfo.O2OApplet_IsUseIconArea)
            {
                result.Menu = model.Where(e => e.TypeId == Himall.Model.SlideAdInfo.SlideAdType.NearShopBranchIcon).ToList(); //菜单图
            }
            else
            {
                result.Menu = null;
            }
            if (siteinfo.O2OApplet_IsUseAdArea)
            {
                result.ADImg1 = adimgs.Count() > 0 ? adimgs.ElementAt(0) : defaultImage;//广告图1
                result.ADImg2 = adimgs.Count() > 1 ? adimgs.ElementAt(1) : defaultImage;//广告图2
                result.ADImg3 = adimgs.Count() > 2 ? adimgs.ElementAt(2) : defaultImage;//广告图3
                result.ADImg4 = adimgs.Count() > 3 ? adimgs.ElementAt(3) : defaultImage;//广告图4
                result.ADImg5 = adimgs.Count() > 4 ? adimgs.ElementAt(4) : defaultImage;//广告图5
            }
            else
            {
                result.ADImg1 = null;
                result.ADImg2 = null;
                result.ADImg3 = null;
                result.ADImg4 = null;
                result.ADImg5 = null;
            }
            if (siteinfo.O2OApplet_IsUseMiddleSlide)
            {
                result.MiddleSlide = model.Where(e => e.TypeId == Himall.Model.SlideAdInfo.SlideAdType.NearShopBranchHome2).ToList(); //中间轮播图
            }
            else
            {
                result.MiddleSlide = null;
            }
            return JsonResult<dynamic>(result);
        }

        #region 门店列表
        /// <summary>
        /// 门店列表
        /// </summary>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetStoreList(string fromLatLng, string keyWords = "", long? tagsId = null, long? shopId = null, int pageNo = 1, int pageSize = 10)
        {
            CheckOpenStore();
            ShopBranchQuery query = new ShopBranchQuery();
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            query.Status = ShopBranchStatus.Normal;
            query.ShopBranchName = keyWords.Trim();
            query.ShopBranchTagId = tagsId;
            query.CityId = -1;
            query.FromLatLng = fromLatLng;
            query.OrderKey = 2;
            query.OrderType = true;
            query.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
            if (query.FromLatLng.Split(',').Length != 2)
            {
                throw new HimallException("无法获取您的当前位置，请确认是否开启定位服务！");
            }

            string address = "", province = "", city = "", district = "", street = "";
            string currentPosition = string.Empty;//当前详情地址，优先顺序：建筑、社区、街道
            Region cityInfo = new Region();
            if (shopId.HasValue)//如果传入了商家ID，则只取商家下门店
            {
                query.ShopId = shopId.Value;
                if (query.ShopId <= 0)
                {
                    throw new HimallException("无法定位到商家！");
                }
            }
            else//否则取用户同城门店
            {
                var addressObj = ShopbranchHelper.GetAddressByLatLng(query.FromLatLng, ref address, ref province, ref city, ref district, ref street);
                if (string.IsNullOrWhiteSpace(city))
                {
                    city = province;
                }
                if (string.IsNullOrWhiteSpace(city))
                {
                    throw new HimallException("无法定位到城市！");
                }
                cityInfo = RegionApplication.GetRegionByName(city, Region.RegionLevel.City);
                if (cityInfo == null)
                {
                    throw new HimallException("无法定位到城市！");
                }
                if (cityInfo != null)
                {
                    query.CityId = cityInfo.Id;
                }
                //处理当前地址
                currentPosition = street;
            }
            var shopBranchs = ShopBranchApplication.SearchNearShopBranchs(query);
            //组装首页数据
            //补充门店活动数据
            var homepageBranchs = ProcessBranchHomePageData(shopBranchs.Models);
            AutoMapper.Mapper.CreateMap<HomePageShopBranch, HomeGetStoreListModel>();
            var homeStores = AutoMapper.Mapper.Map<List<HomePageShopBranch>, List<HomeGetStoreListModel>>(homepageBranchs);
            long userId = 0;
            if (CurrentUser != null)
            {//如果已登陆取购物车数据
                //memberCartInfo = CartApplication.GetShopBranchCart(CurrentUser.Id);
                userId = CurrentUser.Id;
            }
            //统一处理门店购物车数量
            var cartItemCount = ShopBranchApplication.GetShopBranchCartItemCount(userId, homeStores.Select(e => e.ShopBranch.Id));
            foreach (var item in homeStores)
            {
                //商品
                ShopBranchProductQuery proquery = new ShopBranchProductQuery();
                proquery.PageSize = 4;
                proquery.PageNo = 1;
                proquery.OrderKey = 3;
                if (!string.IsNullOrWhiteSpace(keyWords))
                {
                    proquery.KeyWords = keyWords;
                }
                proquery.shopBranchId = item.ShopBranch.Id;
                proquery.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
                var pageModel = ShopBranchApplication.GetShopBranchProducts(proquery);
                var dtNow = DateTime.Now;
                var saleCountByMonth = OrderApplication.GetSaleCount(dtNow.AddDays(-30).Date, dtNow, shopBranchId: proquery.shopBranchId.Value);
                item.SaleCount = OrderApplication.GetSaleCount(shopBranchId: proquery.shopBranchId.Value);
                item.SaleCountByMonth = saleCountByMonth;
                item.ShowProducts = pageModel.Models.Select(p =>
                {
                    var comcount = CommentApplication.GetProductHighCommentCount(productId: p.Id, shopBranchId: proquery.shopBranchId.Value);
                    return new HomeGetStoreListProductModel
                    {
                        Id = p.Id,
                        DefaultImage = HimallIO.GetRomoteProductSizeImage(p.ImagePath, 1, ImageSize.Size_150.GetHashCode()),
                        MinSalePrice = p.MinSalePrice,
                        ProductName = p.ProductName,
                        HasSKU = p.HasSKU,
                        MarketPrice = p.MarketPrice,
                        SaleCount = OrderApplication.GetSaleCount(shopBranchId: proquery.shopBranchId.Value, productId: p.Id),
                        HighCommentCount = comcount,
                    };
                }).ToList();
                item.ProductCount = pageModel.Total;
                if (cartItemCount != null)
                {
                    item.CartQuantity = cartItemCount.ContainsKey(item.ShopBranch.Id) ? cartItemCount[item.ShopBranch.Id] : 0;
                }
                //评分
                item.CommentScore = ShopBranchApplication.GetServiceMark(item.ShopBranch.Id).ComprehensiveMark;
            }
            return JsonResult<dynamic>(new
            {
                Total = shopBranchs.Total,
                CityInfo = new { Id = cityInfo.Id, Name = cityInfo.Name },
                CurrentAddress = currentPosition,
                Stores = homeStores
            });
        }

        /// <summary>
        /// 根据商品查找门店
        /// </summary>
        /// <param name="fromLatLng"></param>
        /// <param name="productId"></param>
        /// <param name="shopId"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetStoresByProduct(string fromLatLng, long productId, long? shopId = null, int pageNo = 1, int pageSize = 10)
        {
            CheckOpenStore();
            ShopBranchQuery query = new ShopBranchQuery();
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            query.Status = ShopBranchStatus.Normal;
            query.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
            query.ProductIds = new long[] { productId };
            query.CityId = -1;
            query.FromLatLng = fromLatLng;
            query.OrderKey = 2;
            query.OrderType = true;
            if (query.FromLatLng.Split(',').Length != 2)
            {
                throw new HimallException("无法获取您的当前位置，请确认是否开启定位服务！");
            }

            string address = "", province = "", city = "", district = "", street = "";
            string currentPosition = string.Empty;//当前详情地址，优先顺序：建筑、社区、街道
            Region cityInfo = new Region();
            if (shopId.HasValue)//如果传入了商家ID，则只取商家下门店
            {
                query.ShopId = shopId.Value;
                if (query.ShopId <= 0)
                {
                    throw new HimallException("无法定位到商家！");
                }
            }
            else//否则取用户同城门店
            {
                var addressObj = ShopbranchHelper.GetAddressByLatLng(query.FromLatLng, ref address, ref province, ref city, ref district, ref street);
                if (string.IsNullOrWhiteSpace(city))
                {
                    throw new HimallException("无法定位到城市！");
                }
                cityInfo = RegionApplication.GetRegionByName(city, Region.RegionLevel.City);
                if (cityInfo != null)
                {
                    query.CityId = cityInfo.Id;
                }
                //处理当前地址

                currentPosition = street;
            }
            var shopBranchs = ShopBranchApplication.StoreByProductNearShopBranchs(query);
            //组装首页数据
            //补充门店活动数据
            var homepageBranchs = ProcessBranchHomePageData(shopBranchs.Models);
            AutoMapper.Mapper.CreateMap<HomePageShopBranch, HomeGetStoreListModel>();
            var homeStores = AutoMapper.Mapper.Map<List<HomePageShopBranch>, List<HomeGetStoreListModel>>(homepageBranchs);
            long userId = 0;
            if (CurrentUser != null)
            {
                //如果已登陆取购物车数据
                //memberCartInfo = CartApplication.GetShopBranchCart(CurrentUser.Id);
                userId = CurrentUser.Id;
            }

            var cartItemCount = ShopBranchApplication.GetShopBranchCartItemCount(userId, homeStores.Select(e => e.ShopBranch.Id));
            foreach (var item in homeStores)
            {
                //商品
                ShopBranchProductQuery proquery = new ShopBranchProductQuery();
                proquery.PageSize = 4;
                proquery.PageNo = 1;
                proquery.OrderKey = 3;
                proquery.shopBranchId = item.ShopBranch.Id;
                proquery.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
                var pageModel = ShopBranchApplication.GetShopBranchProducts(proquery);
                if (productId > 0)
                {
                    var product = pageModel.Models.FirstOrDefault(n => n.Id == productId);
                    pageModel.Models.Remove(product);
                    var models = pageModel.Models.OrderByDescending(p => p.SaleCounts).ThenByDescending(p => p.Id).Take(3).ToList();
                    if (null != product)
                    {
                        models.Insert(0, product);
                    }
                    pageModel.Models = models;
                }
                var dtNow = DateTime.Now;
                var saleCountByMonth = OrderApplication.GetSaleCount(dtNow.AddDays(-30).Date, dtNow, shopBranchId: proquery.shopBranchId.Value);
                item.SaleCount = OrderApplication.GetSaleCount(shopBranchId: proquery.shopBranchId.Value);
                item.SaleCountByMonth = saleCountByMonth;
                item.ShowProducts = pageModel.Models.Select(p => new HomeGetStoreListProductModel
                {
                    Id = p.Id,
                    DefaultImage = HimallIO.GetRomoteProductSizeImage(p.ImagePath, 1, ImageSize.Size_150.GetHashCode()),
                    MinSalePrice = p.MinSalePrice,
                    ProductName = p.ProductName,
                    HasSKU = p.HasSKU,
                    MarketPrice = p.MarketPrice
                }).ToList();
                item.ProductCount = pageModel.Total;
                if (cartItemCount != null)
                {
                    item.CartQuantity = cartItemCount.ContainsKey(item.ShopBranch.Id) ? cartItemCount[item.ShopBranch.Id] : 0;
                }

                //评分
                item.CommentScore = ShopBranchApplication.GetServiceMark(item.ShopBranch.Id).ComprehensiveMark;
            }
            return JsonResult<dynamic>(new
            {
                Total = shopBranchs.Total,
                CityInfo = new { Id = cityInfo.Id, Name = cityInfo.Name },
                CurrentAddress = currentPosition,
                Stores = homeStores
            });
        }

        private List<HomePageShopBranch> ProcessBranchHomePageData(List<ShopBranch> list, bool isAllCoupon = false)
        {
            var shopIds = list.Select(e => e.ShopId).Distinct();
            var homepageBranchs = list.Select(e => new HomePageShopBranch
            {
                ShopBranch = e
            }).ToList();
            foreach (var sid in shopIds)
            {
                ShopActiveList actives = new ShopActiveList();
                //优惠券
                List<CouponInfo> couponList;
                var couponsql = CouponApplication.GetCouponLists(sid);
                couponList = couponsql.Where(d => d.Himall_CouponSetting.Any(c => c.PlatForm == PlatformType.Wap)).ToList();
                var appCouponlist = new List<CouponModel>();
                foreach (CouponInfo couponinfo in couponList)
                {
                    var coupon = new CouponModel();
                    var status = 0;
                    long userid = 0;
                    if (CurrentUser != null)
                    {
                        userid = CurrentUser.Id;
                    }
                    //当前优惠券的可领状态
                    status = ShopBranchApplication.CouponIsUse(couponinfo, userid);

                    coupon.Id = couponinfo.Id;
                    coupon.CouponName = couponinfo.CouponName;
                    coupon.ShopId = couponinfo.ShopId;
                    coupon.OrderAmount = couponinfo.OrderAmount.ToString("F2");
                    coupon.Price = Math.Round(couponinfo.Price, 2);
                    coupon.StartTime = couponinfo.StartTime;
                    coupon.EndTime = couponinfo.EndTime;
                    coupon.IsUse = status;
                    coupon.UseArea = couponinfo.UseArea;
                    coupon.Remark = couponinfo.Remark;
                    appCouponlist.Add(coupon);
                }
                actives.ShopCoupons = appCouponlist.OrderBy(d => d.Price).ToList();
                //满额减活动
                var fullDiscount = FullDiscountApplication.GetOngoingActiveByShopId(sid);
                if (fullDiscount != null)
                {
                    actives.ShopActives = fullDiscount.Select(e => new ActiveInfo
                    {
                        ActiveName = e.ActiveName,
                        ShopId = e.ShopId
                    }).ToList();
                }
                //商家所有门店显示活动相同
                var shopBranchs = homepageBranchs.Where(e => e.ShopBranch.ShopId == sid);
                foreach (var shop in shopBranchs)
                {
                    shop.ShopAllActives = new ShopActiveList
                    {
                        ShopActives = actives.ShopActives,
                        ShopCoupons = actives.ShopCoupons,
                        FreeFreightAmount = shop.ShopBranch.FreeMailFee
                    };
                }
            }
            return homepageBranchs;
        }
        #endregion


        /// <summary>
        /// 获取门店标签信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetTagsInfo(long id)
        {
            var tag = ShopBranchApplication.GetShopBranchTagInfo(id);
            if (null == tag)
            {
                throw new HimallException("非法参数！");
            }
            return JsonResult<dynamic>(new
            {
                Id = tag.Id,
                Title = tag.Title,
                ShopBranchCount = tag.ShopBranchCount
            });
        }

        #region 门店

        /// <summary>
        /// 获取门店信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetStoreInfo(long id, string fromLatLng = "")
        {
            CheckOpenStore();
            var shopBranch = ShopBranchApplication.GetShopBranchById(id);
            if (shopBranch == null)
            {
                throw new HimallApiException(ApiErrorCode.Parameter_Error, "id");
            }
            var shop = ShopApplication.GetShop(shopBranch.ShopId);
            if (null != shop && shop.ShopStatus == ShopInfo.ShopAuditStatus.HasExpired)
                return Json(ErrorResult<dynamic>("此店铺已过期"));
            if (null != shop && shop.ShopStatus == ShopInfo.ShopAuditStatus.Freeze)
                return Json(ErrorResult<dynamic>("此店铺已冻结"));
            if (!string.IsNullOrWhiteSpace(fromLatLng))
            {
                shopBranch.Distance = ShopBranchApplication.GetLatLngDistances(fromLatLng, string.Format("{0},{1}", shopBranch.Latitude, shopBranch.Longitude));
            }
            shopBranch.AddressDetail = ShopBranchApplication.RenderAddress(shopBranch.AddressPath, shopBranch.AddressDetail, 2);
            shopBranch.ShopImages = HimallIO.GetRomoteImagePath(shopBranch.ShopImages);
            Mapper.CreateMap<ShopBranch, HomeGetShopBranchInfoModel>();
            var store = Mapper.Map<ShopBranch, HomeGetShopBranchInfoModel>(shopBranch);
            var homepageBranch = ProcessBranchHomePageData(new List<ShopBranch>() { shopBranch }, true).FirstOrDefault();
            //过滤不能领取的优惠券
            homepageBranch.ShopAllActives.ShopCoupons = homepageBranch.ShopAllActives.ShopCoupons.Where(e => e.IsUse == 0).ToList();
            return JsonResult<dynamic>(new
            {
                Store = store,
                homepageBranch.ShopAllActives,
                CommentScore = ShopBranchApplication.GetServiceMark(store.Id).ComprehensiveMark,   //评分
            });
        }
        /// <summary>
        /// 获取商铺分类
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public JsonResult<Result<List<ShopCategory>>> GetShopCategory(long shopId, long pid = 0, long shopBranchId = 0)
        {
            var cate = ShopCategoryApplication.GetCategoryByParentId(pid, shopId);
            if (shopBranchId > 0)
            {
                //屏蔽没有商品的分类
                List<long> noshowcid = new List<long>();
                foreach (var item in cate)
                {
                    ShopBranchProductQuery query = new ShopBranchProductQuery();
                    query.PageSize = 1;
                    query.PageNo = 1;
                    query.ShopId = shopId;
                    query.shopBranchId = shopBranchId;
                    query.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
                    query.ShopCategoryId = item.Id;
                    var _pros = ShopBranchApplication.GetShopBranchProducts(query);
                    if (_pros.Total <= 0)
                    {
                        noshowcid.Add(item.Id);
                    }
                }
                if (noshowcid.Count > 0)
                {
                    cate = cate.Where(d => !noshowcid.Contains(d.Id)).ToList();
                }
            }
            return JsonResult(cate);
        }

        #endregion

        #region 评价
        /// <summary>
        /// 评价聚合
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult<Result<ProductCommentCountAggregateModel>> GetCommentCountAggregate(long id)
        {
            var data = CommentApplication.GetProductCommentCountAggregate(shopBranchId: id);
            return JsonResult(data);
        }
        /// <summary>
        /// 获取评价
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetComments([FromUri]ProductCommentQuery query)
        {
            if (query.PageNo == 0) query.PageNo = 1;
            if (query.PageSize == 0) query.PageSize = 5;
            var data = CommentApplication.GetProductComments(query);
            AutoMapper.Mapper.CreateMap<ProductComment, HomeGetCommentListModel>();
            var datalist = Mapper.Map<List<ProductComment>, List<HomeGetCommentListModel>>(data.Models);
            var users = MemberApplication.GetMembers(datalist.Select(d => d.UserId));
            //补充数据信息
            foreach (var item in datalist)
            {
                var u = users.FirstOrDefault(d => d.Id == item.UserId);
                if (u != null)
                {
                    item.UserPhoto = Himall.Core.HimallIO.GetRomoteImagePath(u.Photo);
                }
                //规格
                var sku = SKUApplication.GetSku(item.SkuId);
                if (sku != null)
                {
                    List<string> skucs = new List<string>();
                    if (!string.IsNullOrWhiteSpace(sku.Color))
                    {
                        skucs.Add(sku.Color);
                    }
                    if (!string.IsNullOrWhiteSpace(sku.Size))
                    {
                        skucs.Add(sku.Size);
                    }
                    if (!string.IsNullOrWhiteSpace(sku.Version))
                    {
                        skucs.Add(sku.Version);
                    }
                    item.SKU = string.Join("+", skucs);
                }
                foreach (var pitem in item.Images)
                {
                    pitem.CommentImage = HimallIO.GetRomoteImagePath(pitem.CommentImage);
                }
            }
            return JsonResult<dynamic>(new { total = data.Total, rows = datalist });
        }
        #endregion
    }
}
