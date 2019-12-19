using Himall.API.Model;
using Himall.API.Model.ParamsModel;
using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.DTO.QueryModel;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Himall.API
{
    public class ProductController : BaseApiController
    {
        public object GetProductDetail(long id)
        {
            ProductDetailModelForMobie model = new ProductDetailModelForMobie()
            {
                Product = new ProductInfoModel(),
                Shop = new ShopInfoModel(),
                Color = new CollectionSKU(),
                Size = new CollectionSKU(),
                Version = new CollectionSKU()
            };

            ProductInfo product = null;
            ShopInfo shop = null;

            product = ServiceProvider.Instance<IProductService>.Create.GetProduct(id);

            var cashDepositModel = ServiceProvider.Instance<ICashDepositsService>.Create.GetCashDepositsObligation(product.Id);//提供服务（消费者保障、七天无理由、及时发货）
            model.CashDepositsServer = cashDepositModel;
            #region 根据运费模板获取发货地址
            var freightTemplateService = ServiceApplication.Create<IFreightTemplateService>();
            FreightTemplateInfo template = freightTemplateService.GetFreightTemplate(product.FreightTemplateId);
            string productAddress = string.Empty;
            if (template != null && template.SourceAddress.HasValue)
            {
                var fullName = ServiceApplication.Create<IRegionService>().GetFullName(template.SourceAddress.Value);
                if (fullName != null)
                {
                    var ass = fullName.Split(' ');
                    if (ass.Length >= 2)
                    {
                        productAddress = ass[0] + " " + ass[1];
                    }
                    else
                    {
                        productAddress = ass[0];
                    }
                }
            }

            model.ProductAddress = productAddress;
            model.FreightTemplate = template;
            #endregion
            #region 店铺Logo
            long vShopId;
            shop = ServiceProvider.Instance<IShopService>.Create.GetShop(product.ShopId);
            var vshopinfo = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shop.Id);
            if (vshopinfo == null)
            {
                vShopId = -1;
                model.Shop.VShopId = vShopId;
                model.VShopLog = string.Empty;
            }
            else
            {
                vShopId = vshopinfo.Id;
                model.Shop.VShopId = vShopId;
                model.VShopLog = vshopinfo.WXLogo;
            }
            #endregion

            model.Shop.FavoriteShopCount = ServiceProvider.Instance<IShopService>.Create.GetShopFavoritesCount(product.ShopId);//关注人数

            var com = product.Himall_ProductComments.Where(item => !item.IsHidden.HasValue || item.IsHidden.Value == false);

            var limitbuyser = ServiceProvider.Instance<ILimitTimeBuyService>.Create;
            var limitBuy = limitbuyser.GetLimitTimeMarketItemByProductId(id);

            #region 商品SKU

            ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

            if (limitBuy != null)
            {
                var limitSku = ServiceProvider.Instance<ILimitTimeBuyService>.Create.Get(limitBuy.Id);
                var limitSkuItem = limitSku.Details.OrderBy(d => d.Price).FirstOrDefault();
                if (limitSkuItem != null)
                    product.MinSalePrice = limitSkuItem.Price;
            }
            if (product.SKUInfo != null && product.SKUInfo.Count() > 0)
            {
                long colorId = 0, sizeId = 0, versionId = 0;
                foreach (var sku in product.SKUInfo)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 0)
                    {
                        if (long.TryParse(specs[1], out colorId)) { }
                        if (colorId != 0)
                        {
                            if (!model.Color.Any(v => v.Value.Equals(sku.Color)))
                            {
                                var c = product.SKUInfo.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.Stock);
                                model.Color.Add(new ProductSKU
                                {
                                    //Name = "选择颜色",
                                    Name = "选择" + colorAlias,
                                    EnabledClass = c != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Color.Any(c1 => c1.SelectedClass.Equals("selected")) && c != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = colorId,
                                    Value = sku.Color,
                                    Img = Himall.Core.HimallIO.GetRomoteImagePath(sku.ShowPic)
                                });
                            }
                        }
                    }
                    if (specs.Count() > 1)
                    {
                        if (long.TryParse(specs[2], out sizeId)) { }
                        if (sizeId != 0)
                        {
                            if (!model.Size.Any(v => v.Value.Equals(sku.Size)))
                            {
                                var ss = product.SKUInfo.Where(s => s.Size.Equals(sku.Size)).Sum(s1 => s1.Stock);
                                model.Size.Add(new ProductSKU
                                {
                                    //Name = "选择尺码",
                                    Name = "选择" + sizeAlias,
                                    EnabledClass = ss != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Size.Any(s1 => s1.SelectedClass.Equals("selected")) && ss != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = sizeId,
                                    Value = sku.Size
                                });
                            }
                        }
                    }

                    if (specs.Count() > 2)
                    {
                        if (long.TryParse(specs[3], out versionId)) { }
                        if (versionId != 0)
                        {
                            if (!model.Version.Any(v => v.Value.Equals(sku.Version)))
                            {
                                var v = product.SKUInfo.Where(s => s.Version.Equals(sku.Version)).Sum(s => s.Stock);
                                model.Version.Add(new ProductSKU
                                {
                                    //Name = "选择版本",
                                    Name = "选择" + versionAlias,
                                    EnabledClass = v != 0 ? "enabled" : "disabled",
                                    //SelectedClass = !model.Version.Any(v1 => v1.SelectedClass.Equals("selected")) && v != 0 ? "selected" : "",
                                    SelectedClass = "",
                                    SkuId = versionId,
                                    Value = sku.Version
                                });
                            }
                        }
                    }

                }

            }
            #endregion

            #region 店铺
            //shop = ServiceProvider.Instance<IShopService>.Create.GetShop(product.ShopId);
            var mark = ShopServiceMark.GetShopComprehensiveMark(shop.Id);
            model.Shop.PackMark = mark.PackMark;
            model.Shop.ServiceMark = mark.ServiceMark;
            model.Shop.ComprehensiveMark = mark.ComprehensiveMark;
            var comm = ServiceProvider.Instance<ICommentService>.Create.GetCommentsByProductId(id);
            model.Shop.Name = shop.ShopName;
            model.Shop.ProductMark = (comm == null || comm.Count() == 0) ? 0 : comm.Average(p => (decimal)p.ReviewMark);
            model.Shop.Id = product.ShopId;
            model.Shop.FreeFreight = shop.FreeFreight;
            model.Shop.ProductNum = ServiceProvider.Instance<IProductService>.Create.GetShopOnsaleProducts(product.ShopId);

            var shopStatisticOrderComments = ServiceProvider.Instance<IShopService>.Create.GetShopStatisticOrderComments(product.ShopId);

            var productAndDescription = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescription).FirstOrDefault();
            var sellerServiceAttitude = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitude).FirstOrDefault();
            var sellerDeliverySpeed = shopStatisticOrderComments.Where(c => c.CommentKey ==
                StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeed).FirstOrDefault();

            var productAndDescriptionPeer = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionPeer).FirstOrDefault();
            var sellerServiceAttitudePeer = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudePeer).FirstOrDefault();
            var sellerDeliverySpeedPeer = shopStatisticOrderComments.Where(c => c.CommentKey ==
                StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedPeer).FirstOrDefault();

            var productAndDescriptionMax = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionMax).FirstOrDefault();
            var productAndDescriptionMin = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.ProductAndDescriptionMin).FirstOrDefault();

            var sellerServiceAttitudeMax = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudeMax).FirstOrDefault();
            var sellerServiceAttitudeMin = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerServiceAttitudeMin).FirstOrDefault();

            var sellerDeliverySpeedMax = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedMax).FirstOrDefault();
            var sellerDeliverySpeedMin = shopStatisticOrderComments.Where(c => c.CommentKey == StatisticOrderCommentsInfo.EnumCommentKey.SellerDeliverySpeedMin).FirstOrDefault();

            decimal defaultValue = 5;
            //宝贝与描述
            if (productAndDescription != null && productAndDescriptionPeer != null)
            {
                model.Shop.ProductAndDescription = productAndDescription.CommentValue;
            }
            else
            {
                model.Shop.ProductAndDescription = defaultValue;
            }
            //卖家服务态度
            if (sellerServiceAttitude != null && sellerServiceAttitudePeer != null)
            {
                model.Shop.SellerServiceAttitude = sellerServiceAttitude.CommentValue;
            }
            else
            {
                model.Shop.SellerServiceAttitude = defaultValue;
            }
            //卖家发货速度
            if (sellerDeliverySpeedPeer != null && sellerDeliverySpeed != null)
            {
                model.Shop.SellerDeliverySpeed = sellerDeliverySpeed.CommentValue;
            }
            else
            {
                model.Shop.SellerDeliverySpeed = defaultValue;
            }
            if (ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shop.Id) == null)
                model.Shop.VShopId = -1;
            else
                model.Shop.VShopId = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shop.Id).Id;

            //优惠券
            var couponCount = GetCouponList(shop.Id);//取设置的优惠券
            if (couponCount > 0)
            {
                model.Shop.CouponCount = couponCount;
            }

            // 客服
            var customerServices = CustomerServiceApplication.GetMobileCustomerService(shop.Id);
            var meiqia = CustomerServiceApplication.GetPreSaleByShopId(shop.Id).FirstOrDefault(p => p.Tool == CustomerServiceInfo.ServiceTool.MeiQia);
            if (meiqia != null)
                customerServices.Insert(0, meiqia);
            #endregion

            #region 商品
            var consultations = ServiceProvider.Instance<IConsultationService>.Create.GetConsultations(id);
            double total = product.Himall_ProductComments.Count();
            double niceTotal = product.Himall_ProductComments.Count(item => item.ReviewMark >= 4);
            bool isFavorite = false;
            bool IsFavoriteShop = false;
            decimal discount = 1M;
            if (CurrentUser == null)
            {
                isFavorite = false;
                IsFavoriteShop = false;
            }
            else
            {
                isFavorite = ServiceProvider.Instance<IProductService>.Create.IsFavorite(product.Id, CurrentUser.Id);
                var favoriteShopIds = ServiceProvider.Instance<IShopService>.Create.GetFavoriteShopInfos(CurrentUser.Id).Select(item => item.ShopId).ToArray();//获取已关注店铺
                IsFavoriteShop = favoriteShopIds.Contains(product.ShopId);
                discount = CurrentUser.MemberDiscount;
            }

            var productImage = new List<string>();
            var thumbImage = new List<string>();
            for (int i = 1; i < 6; i++)
            {
                if (Core.HimallIO.ExistFile(product.RelativePath + string.Format("/{0}.png", i)))
                {
                    var path = Core.HimallIO.GetRomoteProductSizeImage(product.RelativePath, i, (int)Himall.CommonModel.ImageSize.Size_500);
                    var thumb = Core.HimallIO.GetRomoteProductSizeImage(product.RelativePath, i, (int)Himall.CommonModel.ImageSize.Size_50);
                    thumbImage.Add(thumb);
                    productImage.Add(path);
                }
            }
            //File.Exists(HttpContext.Current.Server.MapPath(product.ImagePath + string.Format("/{0}.png", 1)));
            decimal minSalePrice = shop.IsSelf ? product.MinSalePrice * discount : product.MinSalePrice;
            var isValidLimitBuy = false;
            long countDownId = 0;
            if (limitBuy != null)
            {
                minSalePrice = limitBuy.MinPrice; //限时购不打折
                isValidLimitBuy = true;
                countDownId = limitBuy.Id;
            }
            else
            {
                #region 限时购预热
                var FlashSale = limitbuyser.IsFlashSaleDoesNotStarted(id);
                var FlashSaleConfig = limitbuyser.GetConfig();

                if (FlashSale != null)
                {
                    TimeSpan flashSaleTime = DateTime.Parse(FlashSale.BeginDate) - DateTime.Now;  //开始时间还剩多久
                    TimeSpan preheatTime = new TimeSpan(FlashSaleConfig.Preheat, 0, 0);  //预热时间是多久
                    if (preheatTime >= flashSaleTime)  //预热大于开始
                    {
                        if (!FlashSaleConfig.IsNormalPurchase)
                        {
                            isValidLimitBuy = true;
                            countDownId = FlashSale.Id;
                            minSalePrice = FlashSale.MinPrice;
                        }
                    }
                }
                #endregion
            }
            var ladderPrices = ProductManagerApplication.GetLadderPriceByProductIds(product.Id, shop.IsSelf, discount);
            if (product.IsOpenLadder && !isValidLimitBuy)
            {
                var ladder = ladderPrices.OrderBy(l => l.MinBath).FirstOrDefault();
                if (ladder != null)
                    minSalePrice = ladder.Price;
            }
            bool isFightGroupActive = false;
            var activeInfo = ServiceProvider.Instance<IFightGroupService>.Create.GetActiveByProId(product.Id);
            if (activeInfo != null && activeInfo.ActiveStatus > FightGroupActiveStatus.Ending)
            {
                isFightGroupActive = true;
            }

            model.Product = new ProductInfoModel()
            {
                ProductId = product.Id,
                CommentCount = com.Count(),//product.Himall_ProductComments.Count(),
                Consultations = consultations.Count(),
                ImagePath = productImage,
                ThumbnailPath = thumbImage,
                IsFavorite = isFavorite,
                MarketPrice = product.MarketPrice,
                MinSalePrice = minSalePrice,
                NicePercent = model.Shop.ProductMark == 0 ? 100 : (int)((niceTotal / total) * 100),
                ProductName = product.ProductName,
                ProductSaleStatus = product.SaleStatus,
                AuditStatus = product.AuditStatus,
                ShortDescription = product.ShortDescription,
                ProductDescription = GetProductDescription(product.ProductDescriptionInfo),
                IsOnLimitBuy = isValidLimitBuy,//limitBuy != null,
                IsOpenLadder = product.IsOpenLadder,
                MinMath = ProductManagerApplication.GetProductLadderMinMath(product.Id),
                MeasureUnit = product.MeasureUnit
            };

            #endregion

            #region 佣金
            var probroker = DistributionApplication.GetDistributionProductInfo(id, true);
            var IsDistribution = false;
            decimal Brokerage = 0;
            var _sitesetting = SiteSettingApplication.GetSiteSettings();
            bool isPromoter = false;
            if (CurrentUser != null && CurrentUser.Id > 0)
            {
                var prom = DistributionApplication.GetPromoterByUserId(CurrentUser.Id);
                if ((prom != null && prom.Status == PromoterInfo.PromoterStatus.Audited))
                {
                    isPromoter = true;
                }
            }
            if (probroker != null)
            {
                if (_sitesetting.IsAllUserShowDistributorTips || isPromoter)
                {
                    IsDistribution = true;
                    Brokerage = probroker.Commission;
                }
            }
            #endregion

            #region  代金红包

            var bonus = ServiceProvider.Instance<IShopBonusService>.Create.GetByShopId(shop.Id);
            int BonusCount = 0;
            decimal BonusGrantPrice = 0;
            decimal BonusRandomAmountStart = 0;
            decimal BonusRandomAmountEnd = 0;

            if (bonus != null)
            {
                BonusCount = bonus.Count;
                BonusGrantPrice = bonus.GrantPrice;
                BonusRandomAmountStart = bonus.RandomAmountStart;
                BonusRandomAmountEnd = bonus.RandomAmountEnd;
            }

            var fullDiscount = FullDiscountApplication.GetOngoingActiveByProductId(id, shop.Id);

            #endregion

            LogProduct(id);
            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);
            var IsPromoter = false;
            if (CurrentUser != null && CurrentUser.Id > 0)
            {
                var prom = DistributionApplication.GetPromoterByUserId(CurrentUser.Id);
                if (prom != null && prom.Status == PromoterInfo.PromoterStatus.Audited)
                {
                    IsPromoter = true;
                }
            }
            var countDownStatus = 0;
            var StartDate = string.Empty;
            var EndDate = string.Empty;
            var NowTime = string.Empty;
            if (isValidLimitBuy)
            {
                var market = ServiceProvider.Instance<ILimitTimeBuyService>.Create.Get(countDownId);
                if (market.Status == FlashSaleInfo.FlashSaleStatus.Ended)
                {
                    countDownStatus = 4;//"PullOff";  //已下架
                }
                else if (market.Status == FlashSaleInfo.FlashSaleStatus.Cancelled || market.Status == FlashSaleInfo.FlashSaleStatus.AuditFailed || market.Status == FlashSaleInfo.FlashSaleStatus.WaitForAuditing)
                {
                    countDownStatus = 4;//"NoJoin";  //未参与
                }
                else if (DateTime.Parse(market.BeginDate) > DateTime.Now)
                {
                    countDownStatus = 6; // "AboutToBegin";  //即将开始   6
                }
                else if (DateTime.Parse(market.EndDate) < DateTime.Now)
                {
                    countDownStatus = 4;// "ActivityEnd";   //已结束  4
                }
                else if (market.Status == FlashSaleInfo.FlashSaleStatus.Ended)
                {
                    countDownStatus = 6;// "SoldOut";  //已抢完
                }
                else
                {
                    countDownStatus = 2;//"Normal";  //正常  2
                }
                StartDate = DateTime.Parse(market.BeginDate).ToString("yyyy/MM/dd HH:mm:ss");
                EndDate = DateTime.Parse(market.EndDate).ToString("yyyy/MM/dd HH:mm:ss");
                NowTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            }

            var result = new
            {
                success = true,
                IsOnLimitBuy = isValidLimitBuy,
                CountDownStatus = countDownStatus,
                StartDate = StartDate,
                EndDate = EndDate,
                NowTime = NowTime,
                IsFightGroupActive = isFightGroupActive,
                ActiveId = isFightGroupActive ? activeInfo.Id : 0,
                ActiveStatus = activeInfo != null ? activeInfo.ActiveStatus.GetHashCode() : 0,
                MaxSaleCount = limitBuy == null ? product.MaxBuyCount : limitBuy.LimitCountOfThePeople,
                Title = limitBuy == null ? string.Empty : limitBuy.Title,
                Second = limitBuy == null ? 0 : (limitBuy.EndDate - DateTime.Now).TotalSeconds,
                Product = model.Product,
                CashDepositsServer = model.CashDepositsServer,//提供服务（消费者保障、七天无理由、及时发货）
                ProductAddress = model.ProductAddress,//发货地址
                Free = model.FreightTemplate.IsFree == FreightTemplateType.Free ? "免运费" : "",//是否免运费
                VShopLog = Himall.Core.HimallIO.GetRomoteImagePath(model.VShopLog),
                Shop = model.Shop,
                IsFavoriteShop = IsFavoriteShop,
                Color = model.Color,
                Size = model.Size,
                Version = model.Version,
                BonusCount = BonusCount,
                BonusGrantPrice = BonusGrantPrice,
                BonusRandomAmountStart = BonusRandomAmountStart,
                BonusRandomAmountEnd = BonusRandomAmountEnd,
                fullDiscount = fullDiscount,
                ColorAlias = colorAlias,
                SizeAlias = sizeAlias,
                VersionAlias = versionAlias,
                IsDistribution = IsDistribution,
                Brokerage = Brokerage.ToString("f2"),
                IsPromoter = IsPromoter,
                userId = CurrentUser == null ? 0 : CurrentUser.Id,
                IsOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore,
                CustomerServices = customerServices,
                LadderPrices = ladderPrices//阶梯价
            };
            return result;
        }
        /// <summary>
        /// 获取商品评论
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <param name="top">top1</param>
        /// <returns></returns>
        public object GetProductCommentShow(long id, int top = 1)
        {
            var product = ServiceProvider.Instance<IProductService>.Create.GetProduct(id);
            var model = new List<ProductDetailCommentModel>();
            if (top < 1)
            {
                top = 1;
            }

            if (product == null || product.IsDeleted)
            {
                return ErrorResult("不存在该商品！");
            }
            var com = product.Himall_ProductComments.Where(item => !item.IsHidden.HasValue || item.IsHidden.Value == false);
            var comCount = com.Count();
            if (comCount > 0)
            {
                model = com.OrderByDescending(a => a.ReviewDate).Take(top).Select(c => new ProductDetailCommentModel
                {
                    Sku = ServiceProvider.Instance<IProductService>.Create.GetSkuString(c.Himall_OrderItems.SkuId),
                    UserName = c.UserName.Substring(0, 1) + "***" + c.UserName.Substring(c.UserName.Length - 1, 1),
                    ReviewContent = c.ReviewContent,
                    AppendContent = c.AppendContent,
                    AppendDate = c.AppendDate,
                    ReplyAppendContent = c.ReplyAppendContent,
                    ReplyAppendDate = c.ReplyAppendDate,
                    FinshDate = c.Himall_OrderItems.OrderInfo.FinishDate,
                    Images = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 0).Select(a => a.CommentImage).ToList(),
                    AppendImages = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 1).Select(a => a.CommentImage).ToList(),
                    ReviewDate = c.ReviewDate,
                    ReplyContent = string.IsNullOrWhiteSpace(c.ReplyContent) ? "暂无回复" : c.ReplyContent,
                    ReplyDate = c.ReplyDate,
                    ReviewMark = c.ReviewMark,
                    BuyDate = c.Himall_OrderItems.OrderInfo.OrderDate

                }).ToList();
                foreach (var citem in model)
                {
                    if (citem.Images.Count > 0)
                    {
                        for (var _imgn = 0; _imgn < citem.Images.Count; _imgn++)
                        {
                            citem.Images[_imgn] = Himall.Core.HimallIO.GetRomoteImagePath(citem.Images[_imgn]);
                        }
                    }
                    if (citem.AppendImages.Count > 0)
                    {
                        for (var _imgn = 0; _imgn < citem.AppendImages.Count; _imgn++)
                        {
                            citem.AppendImages[_imgn] = Himall.Core.HimallIO.GetRomoteImagePath(citem.AppendImages[_imgn]);
                        }
                    }
                }
                return new { success = true, data = model };
            }
            else
            {
                return new { success = false, msg = "该宝贝暂无评论！" };
            }

        }
        internal int GetCouponList(long shopId)
        {
            var service = ServiceProvider.Instance<ICouponService>.Create;
            //var result = service.GetCouponList(shopid);
            //var couponSetList = ServiceProvider.Instance<IVShopService>.Create.GetVShopCouponSetting(shopid).Where(a => a.PlatForm == Core.PlatformType.Wap).Select(item => item.CouponID);
            //if (result.Count() > 0 && couponSetList.Count() > 0)
            //{
            //    var couponList = result.ToArray().Where(item => couponSetList.Contains(item.Id)).Select(p => new
            //    {
            //        Receive = Receive(p.ShopId, p.Id)
            //    });
            //    return couponList.Where(p => p.Receive != 2 && p.Receive != 4).Count();//排除过期和已领完的
            //}
            //return 0;
            return service.GetUserCouponCount(shopId);//商铺可用优惠券，排除过期和已领完的
        }
        internal void LogProduct(long pid)
        {
            if (CurrentUser != null)
            {
                BrowseHistrory.AddBrowsingProduct(pid, CurrentUser.Id);
            }
            else
            {
                BrowseHistrory.AddBrowsingProduct(pid);
            }

            //ServiceProvider.Instance<IProductService>.Create.LogProductVisti(pid);
        }
        //新增或取消商品收藏
        public object PostAddFavoriteProduct(ProductAddFavoriteProductModel value)
        {
            CheckUserLogin();
            long productId = value.productId;
            int status = 0;
            var productService = ServiceProvider.Instance<IProductService>.Create;
            bool isFavorite = productService.IsFavorite(productId, CurrentUser.Id);
            if (isFavorite)
            {
                productService.DeleteFavorite(productId, CurrentUser.Id);
                return SuccessResult("取消成功");
            }
            else
            {
                productService.AddFavorite(productId, CurrentUser.Id, out status);
                return SuccessResult("收藏成功");
            }
        }

        public object GetHistoryVisite()
        {
            var products = BrowseHistrory.GetBrowsingProducts(10, CurrentUserId);
            foreach (var product in products)
            {
                //product.ImagePath = "http://" + Url.Request.RequestUri.Host + product.ImagePath;
                product.ImagePath = Core.HimallIO.GetRomoteImagePath(product.ImagePath);
            }
            dynamic result = SuccessResult();
            result.Product = products;
            return result;
        }
        public object GetSKUInfo(long productId, long colloPid = 0)
        {
            var product = ServiceProvider.Instance<IProductService>.Create.GetProduct(productId);
            var limitBuy = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetLimitTimeMarketItemByProductId(productId);
            List<Himall.Model.CollocationSkuInfo> collProduct = null;
            if (colloPid != 0)
            {
                collProduct = ServiceProvider.Instance<ICollocationService>.Create.GetProductColloSKU(productId, colloPid);
            }
            decimal discount = 1M;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            var shopInfo = ShopApplication.GetShop(product.ShopId);

            var skuArray = new List<ProductSKUModel>();

            foreach (var sku in product.SKUInfo.Where(s => s.Stock > 0))
            {
                var price = sku.SalePrice;
                if (product.IsOpenLadder)
                {
                    var ladderPrices = ProductManagerApplication.GetLadderPriceByProductIds(product.Id);
                    var ladder = ladderPrices.OrderBy(l => l.MinBath).FirstOrDefault();
                    if (ladder != null)
                        price = ladder.Price;
                }

                ProductSKUModel skuMode = new ProductSKUModel
                {
                    Price = shopInfo.IsSelf ? price * discount : price,
                    SkuId = sku.Id,
                    Stock = (int)sku.Stock
                };

                if (limitBuy != null)
                {
                    var limitSku = ServiceProvider.Instance<ILimitTimeBuyService>.Create.Get(limitBuy.Id);
                    var limitSkuItem = limitSku.Details.Where(r => r.SkuId.Equals(sku.Id)).FirstOrDefault();
                    if (limitSkuItem != null)
                        skuMode.Price = limitSkuItem.Price;
                }
                skuArray.Add(skuMode);
            }
            //foreach (var item in skuArray)
            //{
            //    var str = item.SkuId.Split('_');
            //    item.SkuId = string.Format("{0};{1};{2}", str[1], str[2], str[3]);
            //}
            dynamic result = SuccessResult();
            result.SkuArray = skuArray;
            return result;
        }

        public object GetProductComment(long pId, int pageNo, int commentType = 0, int pageSize = 10)
        {
            IEnumerable<ProductCommentInfo> result;
            var comments = ServiceProvider.Instance<ICommentService>.Create.GetCommentsByProductId(pId);

            int goodComment = comments.Count(c => c.ReviewMark >= 4);
            int mediumComment = comments.Count(c => c.ReviewMark == 3);
            int bedComment = comments.Count(c => c.ReviewMark <= 2);
            int allComment = comments.Count();
            int hasAppend = comments.Where(c => c.AppendDate.HasValue).Count();
            int hasImages = comments.Where(c => c.Himall_ProductCommentsImages.Count > 0).Count();

            switch (commentType)
            {
                case 1:
                    result = comments.OrderByDescending(c => c.ReviewMark).ToArray().Where(c => c.ReviewMark >= 4);
                    break;
                case 2:
                    result = comments.OrderByDescending(c => c.ReviewMark).ToArray().Where(c => c.ReviewMark == 3);
                    break;
                case 3:
                    result = comments.OrderByDescending(c => c.ReviewMark).ToArray().Where(c => c.ReviewMark <= 2);
                    break;
                case 4:
                    result = comments.Where(c => c.Himall_ProductCommentsImages.Count > 0);
                    break;
                case 5:
                    result = comments.Where(c => c.AppendDate.HasValue);
                    break;
                default:
                    result = comments.OrderByDescending(c => c.ReviewMark).ToArray();
                    break;
            }

            var productService = ServiceProvider.Instance<IProductService>.Create;

            ProductCommentInfo[] temp = result.OrderByDescending(a => a.ReviewDate).Skip((pageNo - 1) * pageSize).Take(pageSize).ToArray();
            var data = temp.ToArray().Select(c =>
            new
            {
                Sku = productService.GetSkuString(c.Himall_OrderItems.SkuId),
                UserName = c.UserName.Substring(0, 1) + "***" + c.UserName.Substring(c.UserName.Length - 1, 1),
                ReviewContent = c.ReviewContent,
                AppendContent = c.AppendContent,
                AppendDate = c.AppendDate.HasValue ? c.AppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                ReplyAppendContent = c.ReplyAppendContent,
                ReplyAppendDate = c.ReplyAppendDate.HasValue ? c.ReplyAppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                FinshDate = c.Himall_OrderItems.OrderInfo.FinishDate.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                Images = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 0).Select(a => new { CommentImage = Core.HimallIO.GetRomoteImagePath(a.CommentImage) }).ToList(),
                AppendImages = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 1).Select(a => new { CommentImage = Core.HimallIO.GetRomoteImagePath(a.CommentImage) }).ToList(),
                ReviewDate = c.ReviewDate.ToString("yyyy-MM-dd HH:mm:ss"),
                ReplyContent = string.IsNullOrWhiteSpace(c.ReplyContent) ? "暂无回复" : c.ReplyContent,
                ReplyDate = c.ReplyDate.HasValue ? c.ReplyDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : " ",
                ReviewMark = c.ReviewMark,
                BuyDate = c.Himall_OrderItems.OrderInfo.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                AppendDays = c.AppendDate.HasValue ? (((DateTime)c.AppendDate) - c.Himall_OrderItems.OrderInfo.FinishDate.Value).Days : 0
            });
            var _result = new
            {
                success = true,
                AllCommentCount = allComment,
                GoodCount = goodComment,
                MediumCount = mediumComment,
                BadCount = bedComment,
                AppendCount = hasAppend,
                ImageCount = hasImages,
                List = data
            };
            return _result;
        }

        /// <summary>
        /// 获取该商品所在商家下距离用户最近的门店
        /// </summary>
        /// <param name="shopId">商家ID</param>
        /// <returns></returns>
        public object GetStroreInfo(long shopId, long productId, string fromLatLng = "")
        {
            if (shopId <= 0) return ErrorResult("请传入合法商家ID");
            if (!(fromLatLng.Split(',').Length == 2)) return ErrorResult("您当前定位信息异常");

            var query = new ShopBranchQuery()
            {
                ShopId = shopId,
                FromLatLng = fromLatLng,
                Status = CommonModel.ShopBranchStatus.Normal,
                ShopBranchProductStatus = ShopBranchSkuStatus.Normal,
                ProductIds = new long[] { productId }
            };
            //商家下门店总数
            var shopbranchs = ShopBranchApplication.GetShopBranchsAll(query).Models.Where(p => (p.Latitude > 0 && p.Longitude > 0)).ToList();
            int total = shopbranchs.Count;
            //商家下有该产品的且距离用户最近的门店
            var shopBranch = shopbranchs.Where(p => (p.Latitude > 0 && p.Longitude > 0)).OrderBy(p => p.Distance).Take(1).FirstOrDefault<Himall.DTO.ShopBranch>();
            dynamic result = SuccessResult();
            result.StoreInfo = shopBranch;
            result.total = total;
            return result;
        }

        /// <summary>
        /// 是否可领取优惠券
        /// </summary>
        /// <param name="vshopId"></param>
        /// <param name="couponId"></param>
        /// <returns></returns>
        //private int Receive(long vshopId, long couponId)
        //{
        //    if (CurrentUser != null && CurrentUser.Id > 0)//未登录不可领取
        //    {
        //        var couponService = ServiceProvider.Instance<ICouponService>.Create;
        //        var couponInfo = couponService.GetCouponInfo(couponId);
        //        if (couponInfo.EndTime < DateTime.Now) return 2;//已经失效

        //        CouponRecordQuery crQuery = new CouponRecordQuery();
        //        crQuery.CouponId = couponId;
        //        crQuery.UserId = CurrentUser.Id;
        //        QueryPageModel<CouponRecordInfo> pageModel = couponService.GetCouponRecordList(crQuery);
        //        if (couponInfo.PerMax != 0 && pageModel.Total >= couponInfo.PerMax) return 3;//达到个人领取最大张数

        //        crQuery = new CouponRecordQuery()
        //        {
        //            CouponId = couponId
        //        };
        //        pageModel = couponService.GetCouponRecordList(crQuery);
        //        if (pageModel.Total >= couponInfo.Num) return 4;//达到领取最大张数

        //        if (couponInfo.ReceiveType == Himall.Model.CouponInfo.CouponReceiveType.IntegralExchange)
        //        {
        //            var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUserId);
        //            if (userInte.AvailableIntegrals < couponInfo.NeedIntegral) return 5;//积分不足
        //        }

        //        return 1;//可正常领取
        //    }
        //    return 0;

        //}

        /// <summary>
        /// 将商品关联版式组合商品描述
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        private string GetProductDescription(ProductDescriptionInfo productDescription)
        {
            if (productDescription == null)
            {
                throw new Himall.Core.HimallException("错误的商品信息");
            }
            string descriptionPrefix = "", descriptiondSuffix = "";//顶部底部版式
            string description = productDescription.ShowMobileDescription.Replace("src=\"/Storage/", "src=\"" + Core.HimallIO.GetRomoteImagePath("/Storage/"));//商品描述

            var iprodestempser = ServiceApplication.Create<IProductDescriptionTemplateService>();
            if (productDescription.DescriptionPrefixId != 0)
            {
                var desc = iprodestempser.GetTemplate(productDescription.DescriptionPrefixId, productDescription.ProductInfo.ShopId);
                descriptionPrefix = desc == null ? "" : desc.MobileContent.Replace("src=\"/Storage/", "src=\"" + Core.HimallIO.GetRomoteImagePath("/Storage/"));
            }

            if (productDescription.DescriptiondSuffixId != 0)
            {
                var desc = iprodestempser.GetTemplate(productDescription.DescriptiondSuffixId, productDescription.ProductInfo.ShopId);
                descriptiondSuffix = desc == null ? "" : desc.MobileContent.Replace("src=\"/Storage/", "src=\"" + Core.HimallIO.GetRomoteImagePath("/Storage/"));
            }

            return string.Format("{0}{1}{2}", descriptionPrefix, description, descriptiondSuffix);
        }
        /// <summary>
        /// 是否可允许自提门店
        /// </summary>
        /// <param name="shopId">商家ID</param>
        /// <returns></returns>
        public object GetIsSelfDelivery(long shopId, long productId, string fromLatLng = "")
        {
            dynamic result = SuccessResult();
            if (shopId <= 0)
            {
                result = SuccessResult("请传入合法商家ID");
                result.IsSelfDelivery = false;
                return result;
            }
            if (!(fromLatLng.Split(',').Length == 2))
            {
                result = SuccessResult("请传入合法经纬度");
                result.IsSelfDelivery = false;
                return result;
            }

            var query = new ShopBranchQuery()
            {
                ShopId = shopId,
                Status = ShopBranchStatus.Normal,
                ShopBranchProductStatus = ShopBranchSkuStatus.Normal
            };
            string address = "", province = "", city = "", district = "", street = "";
            ShopbranchHelper.GetAddressByLatLng(fromLatLng, ref address, ref province, ref city, ref district, ref street);
            if (string.IsNullOrWhiteSpace(city))
            {
                result = SuccessResult("无法获取当前城市");
                result.IsSelfDelivery = false;
                return result;
            }

            Region cityInfo = RegionApplication.GetRegionByName(city, Region.RegionLevel.City);
            if (cityInfo == null)
            {
                result = SuccessResult("获取当前城市异常");
                result.IsSelfDelivery = false;
                return result;
            }
            query.CityId = cityInfo.Id;
            query.ProductIds = new long[] { productId };

            var shopBranch = ShopBranchApplication.GetShopBranchsAll(query).Models;//获取该商品所在商家下，且与用户同城内门店，且门店有该商品
            var skuInfos = ProductManagerApplication.GetSKU(productId);//获取该商品的sku
            //门店SKU内会有默认的SKU
            if (!skuInfos.Exists(p => p.Id == string.Format("{0}_0_0_0", productId))) skuInfos.Add(new DTO.SKU()
            {
                Id = string.Format("{0}_0_0_0", productId)
            });
            var shopBranchSkus = ShopBranchApplication.GetSkus(query.ShopId, shopBranch.Select(p => p.Id));//门店商品SKU

            //门店商品SKU，只要有一个sku有库存即可
            shopBranch.ForEach(p =>
                p.Enabled = skuInfos.Where(skuInfo => shopBranchSkus.Where(sbSku => sbSku.ShopBranchId == p.Id && sbSku.Stock > 0 && sbSku.SkuId == skuInfo.Id).Count() > 0).Count() > 0
            );

            result = SuccessResult("");
            result.IsSelfDelivery = shopBranch.Where(p => p.Enabled).Count() > 0 ? 1 : 0;
            return result;//至少有一个能够自提的门店，才可显示图标
        }

        /// <summary>
        /// 获取商品批发价
        /// </summary>
        /// <param name="pid">商品ID</param>
        /// <param name="buyNum">数量</param>
        /// <returns></returns>
        public object GetChangeNum(long pid, int buyNum)
        {
            var _price = 0m;
            var product = ProductManagerApplication.GetProduct(pid);
            if (product.IsOpenLadder)
            {
                _price = ProductManagerApplication.GetProductLadderPrice(pid, buyNum);
                var discount = 1m;
                if (CurrentUser != null)
                    discount = CurrentUser.MemberDiscount;

                var shop = ShopApplication.GetShop(product.ShopId);
                if (shop.IsSelf)
                    _price = _price * discount;
            }

            dynamic result = SuccessResult();
            result.price = _price;
            return result;
        }
    }
}
