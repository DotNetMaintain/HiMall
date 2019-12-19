using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.DTO;
using Himall.DTO.Product;
using Himall.DTO.QueryModel;
using Himall.IServices;
using Himall.Model;
using Himall.SmallProgAPI.Model;
using Himall.Web.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Http.Results;

namespace Himall.SmallProgAPI
{
    public class ProductController : BaseApiController
    {
        /// <summary>
        /// 搜索商品
        /// </summary>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetProducts(
             string keyword, /* 搜索关键字 */
            long cid = 0,  /* 分类ID */
                           //long b_id = 0, /* 品牌ID */
            string openId = "",
            //string a_id = "",  /* 属性ID, 表现形式：attrId_attrValueId */
            string sortBy = "", /* 排序项（1：默认，2：销量，3：价格，4：评论数，5：上架时间） */
            string sortOrder = "", /* 排序方式（1：升序，2：降序） */
            int pageIndex = 1, /*页码*/
            int pageSize = 10,/*每页显示数据量*/
            long vshopId = 0,
            long sid = 0/*商家ID*/
            )
        {
            //CheckUserLogin();
            //if (string.IsNullOrEmpty(keyword) && vshopId == 0 && cid <= 0)
            //    keyword = Application.SiteSettingApplication.GetSiteSettings().Keyword;
            #region 初始化查询Model
            SearchProductQuery model = new SearchProductQuery();
            model.VShopId = vshopId;
            model.ShopId = sid;
            //model.BrandId = b_id;
            if (vshopId == 0 && cid != 0)
            {
                var catelist = ServiceProvider.Instance<ICategoryService>.Create.GetCategories();
                var cate = catelist.FirstOrDefault(r => r.Id == cid);
                if (cate.Depth == 1)
                    model.FirstCateId = cid;
                else if (cate.Depth == 2)
                    model.SecondCateId = cid;
                else if (cate.Depth == 3)
                    model.ThirdCateId = cid;
            }
            else if (vshopId != 0 && cid != 0)
            {
                model.ShopCategoryId = cid;
            }

            model.Keyword = keyword;
            if (sortBy == "SalePrice")
            {
                model.OrderKey = 3;//默认
            }
            else if (sortBy == "SaleCounts")
            {
                model.OrderKey = 2;
            }
            else if (sortBy == "VistiCounts")
            {
                model.OrderKey = 4;
            }
            else
            {
                model.OrderKey = 1;
            }

            if (sortOrder == "desc")
            {
                model.OrderType = true;//降序
            }
            else
            {
                model.OrderType = false;//升序
            }


            model.PageNumber = pageIndex;
            model.PageSize = pageSize;
            #endregion
            SearchProductResult result = ServiceProvider.Instance<ISearchProductService>.Create.SearchProduct(model);
            int total = result.Total;
            //当查询的结果少于一页时用like进行补偿（与PC端同步）
            if (result.Total < pageSize)
            {
                model.IsLikeSearch = true;
                SearchProductResult result2 = ServiceProvider.Instance<ISearchProductService>.Create.SearchProduct(model);
                var idList1 = result.Data.Select(a => a.ProductId).ToList();
                var nresult = result2.Data.Where(a => !idList1.Contains(a.ProductId)).ToList();

                if (nresult.Count > 0)
                {
                    result.Total += nresult.Count;
                    result.Data.AddRange(nresult);
                }
                //补充数据后，重新排序
                Func<IEnumerable<ProductView>, IOrderedEnumerable<ProductView>> orderby = null;
                Func<IEnumerable<ProductView>, IOrderedEnumerable<ProductView>> orderByDesc = null;
                switch (model.OrderKey)
                {
                    case 2:
                        //order.Append(" ORDER BY SaleCount ");
                        orderby = e => e.OrderBy(p => p.SaleCount);
                        orderByDesc = e => e.OrderByDescending(p => p.SaleCount);
                        break;
                    case 3:
                        //order.Append(" ORDER BY SalePrice ");
                        orderby = e => e.OrderBy(p => p.SalePrice);
                        orderByDesc = e => e.OrderByDescending(p => p.SalePrice);
                        break;
                    case 4:
                        //order.Append(" ORDER BY Comments ");
                        orderby = e => e.OrderBy(p => p.Comments);
                        orderByDesc = e => e.OrderByDescending(p => p.Comments);
                        break;
                    default:
                        //order.Append(" ORDER BY Id ");
                        orderby = e => e.OrderBy(p => p.ProductId);
                        orderByDesc = e => e.OrderByDescending(p => p.ProductId);
                        break;
                }
                if (model.OrderType)
                {
                    result.Data = orderby(result.Data).ToList();
                }
                else
                {
                    result.Data = orderByDesc(result.Data).ToList();
                }
            }

            total = result.Total;
            #region 价格更新
            //会员折扣
            decimal discount = 1M;
            long SelfShopId = 0;
            long currentUserId = 0;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
                var shopInfo = ShopApplication.GetSelfShop();
                SelfShopId = shopInfo.Id;
                currentUserId = CurrentUser.Id;
            }
            //填充商品和购物车数据
            var ids = result.Data.Select(d => d.ProductId).ToArray();
            List<Product> products = ProductManagerApplication.GetProductsByIds(ids);
            List<SKU> skus = ProductManagerApplication.GetSKU(ids);
            List<ShoppingCartItem> cartitems = CartApplication.GetCartQuantityByIds(currentUserId, ids);
            List<dynamic> productList = new List<dynamic>();
            foreach (var item in result.Data)
            {
                Product proInfo = products.Where(d => d.Id == item.ProductId).FirstOrDefault();
                if (proInfo == null)
                {
                    break;
                }
                SKU skuInfo = skus.Where(d => d.ProductId == item.ProductId).FirstOrDefault();
                bool hasSku = proInfo.HasSKU;
                decimal marketPrice = proInfo.MarketPrice;
                string skuId = skuInfo.Id;
                int quantity = 0;
                quantity = cartitems.Where(d => d.ProductId == item.ProductId).Sum(d => d.Quantity);
                decimal salePrice = item.SalePrice;
                item.ImagePath = Core.HimallIO.GetRomoteProductSizeImage(Core.HimallIO.GetImagePath(item.ImagePath), 1, (int)Himall.CommonModel.ImageSize.Size_350);
                if (item.ShopId == SelfShopId)
                    salePrice = item.SalePrice * discount;

                long activeId = 0;
                int activetype = 0;
                var limitbuyser = ServiceProvider.Instance<ILimitTimeBuyService>.Create;
                var limitBuy = limitbuyser.GetLimitTimeMarketItemByProductId(item.ProductId);
                if (limitBuy != null)
                {
                    salePrice = limitBuy.MinPrice;
                    activeId = limitBuy.Id;
                    activetype = 1;
                }
                else
                {
                    #region 限时购预热
                    var FlashSale = limitbuyser.IsFlashSaleDoesNotStarted(item.ProductId);
                    var FlashSaleConfig = limitbuyser.GetConfig();

                    if (FlashSale != null)
                    {
                        TimeSpan flashSaleTime = DateTime.Parse(FlashSale.BeginDate) - DateTime.Now;  //开始时间还剩多久
                        TimeSpan preheatTime = new TimeSpan(FlashSaleConfig.Preheat, 0, 0);  //预热时间是多久
                        if (preheatTime >= flashSaleTime)  //预热大于开始
                        {
                            if (!FlashSaleConfig.IsNormalPurchase)
                            {
                                activeId = FlashSale.Id;
                                activetype = 1;
                            }
                        }
                    }
                    #endregion
                }
                //var activeInfo = ServiceProvider.Instance<IFightGroupService>.Create.GetActiveByProId(item.ProductId);
                //if (activeInfo != null)
                //{
                //    item.SalePrice = activeInfo.MiniGroupPrice;
                //    activeId = activeInfo.Id;
                //    activetype = 2;
                //}
                var pro = new
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Pic = item.ImagePath,// GetProductImageFullPath(d.Field<string>("ThumbnailUrl310")),
                    MarketPrice = marketPrice.ToString("0.##"),//市场价
                    SalePrice = salePrice.ToString("0.##"),//当前价
                    SaleCounts = item.SaleCount,
                    CartQuantity = quantity,// item.cartquantity,
                    HasSKU = hasSku,// d.Field<bool>("HasSKU"),//是否有规格
                    Stock = skuInfo.Stock,//默认规格库存
                    SkuId = skuId,// d.Field<string>("SkuId"),//规格ID
                    ActiveId = activeId,//活动Id
                    ActiveType = activetype//活动类型（1代表限购，2代表团购，3代表商品预售，4代表限购预售，5代表团购预售）
                };
                productList.Add(pro);
            }
            #endregion
            var json = JsonResult<dynamic>(data: productList);
            return json;
        }
        /// <summary>
        /// 获取商品详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetProductDetail(string openId, long ProductID)
        {
            //CheckUserLogin();
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
            long activeId = 0;
            int activetype = 0;

            product = ServiceProvider.Instance<IProductService>.Create.GetProduct(ProductID);

            var cashDepositModel = ServiceProvider.Instance<ICashDepositsService>.Create.GetCashDepositsObligation(product.Id);//提供服务（消费者保障、七天无理由、及时发货）
            model.CashDepositsServer = cashDepositModel;
            var com = product.Himall_ProductComments.Where(item => !item.IsHidden.HasValue || item.IsHidden.Value == false);
            #region 商品SKU


            var limitBuy = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetLimitTimeMarketItemByProductId(ProductID);
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
            List<object> SkuItemList = new List<object>();
            List<object> Skus = new List<object>();


            long[] Ids = { ProductID };
            long userId = 0;
            if (CurrentUser != null)
                userId = CurrentUser.Id;
            List<ShoppingCartItem> cartitems = CartApplication.GetCartQuantityByIds(userId, Ids);
            var stock = 0;
            if (product.SKUInfo != null && product.SKUInfo.Count() > 0)
            {
                #region 颜色
                long colorId = 0, sizeId = 0, versionId = 0;
                List<object> colorAttributeValue = new List<object>();
                List<string> listcolor = new List<string>();
                foreach (var sku in product.SKUInfo)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 0)
                    {
                        if (long.TryParse(specs[1], out colorId)) { }//相同颜色规格累加对应值
                        if (colorId != 0)
                        {
                            if (!listcolor.Contains(sku.Color))
                            {
                                var c = product.SKUInfo.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.Stock);
                                var colorvalue = new
                                {
                                    ValueId = colorId,
                                    UseAttributeImage = "False",
                                    Value = sku.Color,
                                    ImageUrl = Himall.Core.HimallIO.GetRomoteImagePath(sku.ShowPic)
                                };
                                listcolor.Add(sku.Color);
                                colorAttributeValue.Add(colorvalue);
                            }
                        }
                    }
                }
                var color = new
                {
                    AttributeName = colorAlias,
                    AttributeId = product.TypeId,
                    AttributeValue = colorAttributeValue,
                    AttributeIndex = 0,
                };
                if (colorId > 0)
                {
                    SkuItemList.Add(color);
                }
                #endregion

                #region 容量
                List<object> sizeAttributeValue = new List<object>();
                List<string> listsize = new List<string>();
                foreach (var sku in product.SKUInfo)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 1)
                    {
                        if (long.TryParse(specs[2], out sizeId)) { }
                        if (sizeId != 0)
                        {
                            if (!listsize.Contains(sku.Size))
                            {
                                var ss = product.SKUInfo.Where(s => s.Size.Equals(sku.Size)).Sum(s1 => s1.Stock);
                                var sizeValue = new
                                {
                                    ValueId = sizeId,
                                    UseAttributeImage = false,
                                    Value = sku.Size,
                                    ImageUrl = ""// Himall.Core.HimallIO.GetRomoteImagePath(sku.ShowPic)
                                };
                                listsize.Add(sku.Size);
                                sizeAttributeValue.Add(sizeValue);
                            }
                        }
                    }
                }
                var size = new
                {
                    AttributeName = sizeAlias,
                    AttributeId = product.TypeId,
                    AttributeValue = sizeAttributeValue,
                    AttributeIndex = 1,
                };
                if (sizeId > 0)
                {
                    SkuItemList.Add(size);
                }
                #endregion

                #region 规格
                List<object> versionAttributeValue = new List<object>();
                List<string> listversion = new List<string>();
                foreach (var sku in product.SKUInfo)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 2)
                    {
                        if (long.TryParse(specs[3], out versionId)) { }
                        if (versionId != 0)
                        {
                            if (!listversion.Contains(sku.Version))
                            {
                                var v = product.SKUInfo.Where(s => s.Version.Equals(sku.Version));
                                var versionValue = new
                                {
                                    ValueId = versionId,
                                    UseAttributeImage = false,
                                    Value = sku.Version,
                                    ImageUrl = ""// Himall.Core.HimallIO.GetRomoteImagePath(sku.ShowPic)
                                };
                                listversion.Add(sku.Version);
                                versionAttributeValue.Add(versionValue);
                            }
                        }
                    }
                }
                var version = new
                {
                    AttributeName = versionAlias,
                    AttributeId = product.TypeId,
                    AttributeValue = versionAttributeValue,
                    AttributeIndex = 2,
                };
                if (versionId > 0)
                {
                    SkuItemList.Add(version);
                }
                #endregion

                #region Sku值
                foreach (var sku in product.SKUInfo)
                {
                    int quantity = 0;
                    quantity = cartitems.Where(d => d.SkuId == sku.Id).Sum(d => d.Quantity);//购物车购买数量
                    var prosku = new
                    {
                        SkuItems = "",
                        MemberPrices = "",
                        SkuId = sku.Id,
                        ProductId = product.Id,
                        SKU = sku.Sku,
                        Weight = 0,
                        Stock = sku.Stock,
                        WarningStock = sku.SafeStock,
                        CostPrice = sku.CostPrice,
                        SalePrice = sku.SalePrice,
                        StoreStock = 0,
                        StoreSalePrice = 0,
                        OldSalePrice = 0,
                        ImageUrl = "",
                        ThumbnailUrl40 = "",
                        ThumbnailUrl410 = "",
                        MaxStock = 0,
                        FreezeStock = 0,
                        Quantity = quantity
                    };
                    Skus.Add(prosku);
                }
                #endregion
            }
            #endregion
            #region 店铺
            shop = ServiceProvider.Instance<IShopService>.Create.GetShop(product.ShopId);

            var vshopinfo = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shop.Id);
            if (vshopinfo != null)
            {
                model.VShopLog = vshopinfo.WXLogo;
                model.Shop.VShopId = vshopinfo.Id;
            }
            else
            {
                model.Shop.VShopId = -1;
                model.VShopLog = string.Empty;
            }
            var mark = ShopServiceMark.GetShopComprehensiveMark(shop.Id);
            model.Shop.PackMark = mark.PackMark;
            model.Shop.ServiceMark = mark.ServiceMark;
            model.Shop.ComprehensiveMark = mark.ComprehensiveMark;
            var comm = ServiceProvider.Instance<ICommentService>.Create.GetCommentsByProductId(ProductID);
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
            #endregion
            #region 商品
            var consultations = ServiceProvider.Instance<IConsultationService>.Create.GetConsultations(ProductID);
            double total = product.Himall_ProductComments.Count(item => !item.IsHidden.Value);
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

            decimal maxprice = shop.IsSelf ? product.SKUInfo.Max(d => d.SalePrice) * discount : product.SKUInfo.Max(d => d.SalePrice);//最高SKU价格
            decimal minprice = shop.IsSelf ? product.SKUInfo.Min(d => d.SalePrice) * discount : product.SKUInfo.Min(d => d.SalePrice);//最低价

            var productImage = new List<string>();
            for (int i = 1; i < 6; i++)
            {
                if (Core.HimallIO.ExistFile(product.RelativePath + string.Format("/{0}.png", i)))
                {
                    var path = Core.HimallIO.GetRomoteProductSizeImage(product.RelativePath, i, (int)Himall.CommonModel.ImageSize.Size_350);
                    productImage.Add(path);
                }
            }
            var ladderPrices = ProductManagerApplication.GetLadderPriceByProductIds(product.Id, shop.IsSelf, discount);
            var isValidLimitBuy = false;
            if (limitBuy != null)
            {
                maxprice = limitBuy.MinPrice;
                minprice = limitBuy.MinPrice;
                activeId = limitBuy.Id;
                activetype = 1;
                isValidLimitBuy = true;
            }
            else
            {
                #region 限时购预热
                var FlashSale = ServiceProvider.Instance<ILimitTimeBuyService>.Create.IsFlashSaleDoesNotStarted(product.Id);
                var FlashSaleConfig = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetConfig();

                if (FlashSale != null)
                {
                    TimeSpan flashSaleTime = DateTime.Parse(FlashSale.BeginDate) - DateTime.Now;  //开始时间还剩多久
                    TimeSpan preheatTime = new TimeSpan(FlashSaleConfig.Preheat, 0, 0);  //预热时间是多久
                    if (preheatTime >= flashSaleTime)  //预热大于开始
                    {
                        if (!FlashSaleConfig.IsNormalPurchase)
                        {
                            isValidLimitBuy = true;
                        }
                    }
                }
                #endregion              

            }
            if (product.IsOpenLadder)
            {
                var ladder = ladderPrices.OrderBy(l => l.MinBath).FirstOrDefault();
                if (ladder != null)
                    minprice = ladder.Price;
            }
            model.Product = new ProductInfoModel()
            {
                ProductId = product.Id,
                CommentCount = com.Count(),//product.Himall_ProductComments.Count(),
                Consultations = consultations.Count(),
                ImagePath = productImage,
                IsFavorite = isFavorite,
                MarketPrice = product.MarketPrice,
                //MinSalePrice = minSalePrice,
                NicePercent = model.Shop.ProductMark == 0 ? 100 : (int)((niceTotal / total) * 100),
                ProductName = product.ProductName,
                ProductSaleStatus = product.SaleStatus,
                AuditStatus = product.AuditStatus,
                ShortDescription = product.ShortDescription,
                ProductDescription = GetProductDescription(product.ProductDescriptionInfo),
                IsOnLimitBuy = limitBuy != null,
                IsOpenLadder = product.IsOpenLadder,
                MinMath = ProductManagerApplication.GetProductLadderMinMath(product.Id)
            };


            #endregion
            LogProduct(ProductID);
            //统计商品浏览量、店铺浏览人数
            StatisticApplication.StatisticVisitCount(product.Id, product.ShopId);

            //图片集合
            List<object> ProductImgs = new List<object>();
            for (int i = 1; i < 5; i++)
            {
                if (i == 1 || Himall.Core.HimallIO.ExistFile(product.RelativePath + string.Format("/{0}.png", i)))
                {
                    //productImage.Add(Core.HimallIO.GetRomoteImagePath(product.RelativePath + string.Format("/{0}.png", i)));
                    ProductImgs.Add(Core.HimallIO.GetRomoteProductSizeImage(product.ImagePath, i, (int)ImageSize.Size_350));
                }
            }
            //优惠劵
            var coupons = GetShopCouponList(shop.Id);


            dynamic Promotes = new System.Dynamic.ExpandoObject();
            //ProductActives actives = new ProductActives();
            var freeFreight = ServiceApplication.Create<IShopService>().GetShopFreeFreight(shop.Id);
            Promotes.freeFreight = freeFreight;
            var fullDiscount = FullDiscountApplication.GetOngoingActiveByProductId(product.Id, shop.Id);
            if (fullDiscount != null)
            {
                Promotes.FullDiscount = fullDiscount;
            }

            ////参与活动
            //var reducelist = ServiceProvider.Instance<IFullDiscountService>.Create.GetOngoingActiveByProductId(product.Id, shop.Id);//满额减
            ////满额减
            //if (reducelist != null)
            //{
            //    var FullAmountReduceList = new
            //    {
            //        StoreId = 0,
            //        PromoteType = 12,
            //        ActivityId = product.Id,
            //        ActivityName = reducelist.ActiveName,
            //        StartDate = reducelist.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
            //        GiftIds = ""
            //    };
            //    Promotes.Add(FullAmountReduceList);
            //}
            ////免运费
            //if (model.Shop.FreeFreight > 0)
            //{
            //    var FullAmountSentFreightList = new
            //    {
            //        StoreId = 0,
            //        PromoteType = 17,
            //        ActivityId = product.Id,
            //        ActivityName = "满" + model.Shop.FreeFreight + "免运费",
            //        StartDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"),
            //        GiftIds = ""
            //    };
            //    Promotes.Add(FullAmountSentFreightList);
            //}
            string skuId = product != null && product.SKUInfo != null ? product.SKUInfo.FirstOrDefault().Id : "";
            int addressId = 0;
            if (CurrentUser != null)
            {
                var addressInfo = ShippingAddressApplication.GetDefaultUserShippingAddressByUserId(CurrentUser.Id);
                if (addressInfo != null)
                {
                    addressId = addressInfo.RegionId;
                }
            }
            return JsonResult<dynamic>(new
            {
                ProductId = product.Id,
                ProductName = product.ProductName,
                ShortDescription = product.ShortDescription,
                ShowSaleCounts = product.SaleCounts,
                MetaDescription = model.Product.ProductDescription.Replace("\"/Storage/Shop", "\"" + Core.HimallIO.GetImagePath("/Storage/Shop")),//替换链接  /Storage/Shop
                MarketPrice = product.MarketPrice.ToString("0.##"),//市场价
                IsfreeShipping = "False",//是否免费送货
                MaxSalePrice = maxprice.ToString("0.##"),
                MinSalePrice = minprice.ToString("0.##"),//限时抢购或商城价格
                ThumbnailUrl60 = Core.HimallIO.GetRomoteProductSizeImage(product.ImagePath, 1, (int)ImageSize.Size_350),
                ProductImgs = ProductImgs,
                ReviewCount = total,
                Stock = product.SKUInfo.Max(d => d.Stock),
                SkuItemList = SkuItemList,
                Skus = Skus,
                Freight = GetFreightStr(product.Id, discount, skuId, addressId),//运费
                Coupons = coupons,//优惠劵
                Promotes = Promotes,//活动
                IsUnSale = product.SaleStatus == Himall.Model.ProductInfo.ProductSaleStatus.InStock ? true : false,
                ActiveId = activeId,
                ActiveType = activetype,
                IsOpenLadder = product.IsOpenLadder,//是否开启阶梯价
                LadderPrices = ladderPrices,//阶梯价
                MinBath = model.Product.MinMath,//最小批量
                Shop = model.Shop,
                VShopLog = Himall.Core.HimallIO.GetRomoteImagePath(model.VShopLog),
                MeasureUnit = product.MeasureUnit,
                MaxBuyCount = product.MaxBuyCount,//限购数
                IsOnLimitBuy = isValidLimitBuy
            });
        }
        /// <summary>
        /// 获取商品的规格信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetProductSkus(long productId, string openId = "")
        {
            CheckUserLogin();
            var product = ServiceProvider.Instance<IProductService>.Create.GetProduct(productId);
            var limitBuy = ServiceProvider.Instance<ILimitTimeBuyService>.Create.GetLimitTimeMarketItemByProductId(productId);
            var shopInfo = ShopApplication.GetShop(product.ShopId);

            decimal discount = 1M;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            ShoppingCartInfo cartInfo = CartApplication.GetCart(CurrentUser.Id);

            var skuArray = new List<ProductSKUModel>();
            object defaultsku = new object();
            int activetype = 0;
            string skuId = "", skucode = "", imageUrl = "";
            long weight = 0, stock = 0;
            decimal SalePrice = 0;

            foreach (var sku in product.SKUInfo.Where(s => s.Stock > 0))
            {
                var price = sku.SalePrice;// * discount;
                if (product.IsOpenLadder)
                {
                    var ladderPrices = ProductManagerApplication.GetLadderPriceByProductIds(product.Id);
                    var ladder = ladderPrices.OrderBy(l => l.MinBath).FirstOrDefault();
                    if (ladder != null)
                    {
                        price = SalePrice = ladder.Price;
                    }
                    //ladder = ladderPrices.OrderByDescending(l => l.MinBath).FirstOrDefault();
                    //if (ladder != null)
                    //    SalePrice = ladder.Price;

                }
                SalePrice = shopInfo.IsSelf ? SalePrice * discount : SalePrice;
                price = shopInfo.IsSelf ? price * discount : price;
                ProductSKUModel skuMode = new ProductSKUModel
                {
                    Price = price,
                    SkuId = sku.Id,
                    Stock = sku.Stock
                };
                if (limitBuy != null)
                {
                    activetype = 1;
                    var limitSku = ServiceProvider.Instance<ILimitTimeBuyService>.Create.Get(limitBuy.Id);
                    var limitSkuItem = limitSku.Details.Where(r => r.SkuId.Equals(sku.Id)).FirstOrDefault();
                    if (limitSkuItem != null)
                        skuMode.Price = limitSkuItem.Price;
                }
                skuArray.Add(skuMode);
            }

            ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

            List<object> SkuItemList = new List<object>();
            List<object> Skus = new List<object>();
            if (product.SKUInfo != null && product.SKUInfo.Count() > 0)
            {
                #region 颜色
                long colorId = 0, sizeId = 0, versionId = 0;
                List<object> colorAttributeValue = new List<object>();
                List<string> listcolor = new List<string>();
                foreach (var sku in product.SKUInfo)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 0)
                    {
                        if (long.TryParse(specs[1], out colorId)) { }//相同颜色规格累加对应值
                        if (colorId != 0)
                        {
                            if (!listcolor.Contains(sku.Color))
                            {
                                var c = product.SKUInfo.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.Stock);
                                var colorvalue = new
                                {
                                    ValueId = colorId,
                                    Value = sku.Color,
                                    UseAttributeImage = "False",
                                    ImageUrl = Himall.Core.HimallIO.GetRomoteImagePath(sku.ShowPic)
                                };
                                listcolor.Add(sku.Color);
                                colorAttributeValue.Add(colorvalue);
                            }
                        }
                    }
                }
                var color = new
                {
                    AttributeId = product.TypeId,
                    AttributeName = colorAlias,
                    AttributeValue = colorAttributeValue,
                    AttributeIndex = 0,
                };
                if (colorId > 0)
                {
                    SkuItemList.Add(color);
                }
                #endregion

                #region 容量
                List<object> sizeAttributeValue = new List<object>();
                List<string> listsize = new List<string>();
                foreach (var sku in product.SKUInfo)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 1)
                    {
                        if (long.TryParse(specs[2], out sizeId)) { }
                        if (sizeId != 0)
                        {
                            if (!listsize.Contains(sku.Size))
                            {
                                var ss = product.SKUInfo.Where(s => s.Size.Equals(sku.Size)).Sum(s1 => s1.Stock);
                                var sizeValue = new
                                {
                                    ValueId = sizeId,
                                    Value = sku.Size,
                                    UseAttributeImage = false,
                                    ImageUrl = Himall.Core.HimallIO.GetRomoteImagePath(sku.ShowPic)
                                };
                                listsize.Add(sku.Size);
                                sizeAttributeValue.Add(sizeValue);
                            }
                        }
                    }
                }
                var size = new
                {
                    AttributeId = product.TypeId,
                    AttributeName = sizeAlias,
                    AttributeValue = sizeAttributeValue,
                    AttributeIndex = 1,
                };
                if (sizeId > 0)
                {
                    SkuItemList.Add(size);
                }
                #endregion

                #region 规格
                List<object> versionAttributeValue = new List<object>();
                List<string> listversion = new List<string>();
                foreach (var sku in product.SKUInfo)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 2)
                    {
                        if (long.TryParse(specs[3], out versionId)) { }
                        if (versionId != 0)
                        {
                            if (!listversion.Contains(sku.Version))
                            {
                                var v = product.SKUInfo.Where(s => s.Version.Equals(sku.Version));
                                var versionValue = new
                                {
                                    ValueId = versionId,
                                    Value = sku.Version,
                                    UseAttributeImage = false,
                                    ImageUrl = Himall.Core.HimallIO.GetRomoteImagePath(sku.ShowPic)
                                };
                                listversion.Add(sku.Version);
                                versionAttributeValue.Add(versionValue);
                            }
                        }
                    }
                }
                var version = new
                {
                    AttributeId = product.TypeId,
                    AttributeName = versionAlias,
                    AttributeValue = versionAttributeValue,
                    AttributeIndex = 2,
                };
                if (versionId > 0)
                {
                    SkuItemList.Add(version);
                }
                #endregion

                #region Sku值
                foreach (var sku in product.SKUInfo)
                {
                    var prosku = new
                    {
                        SkuId = sku.Id,
                        SKU = sku.Sku,
                        Weight = product.Weight,
                        Stock = sku.Stock,
                        WarningStock = sku.SafeStock,
                        SalePrice = product.IsOpenLadder ? SalePrice.ToString("0.##") : sku.SalePrice.ToString("0.##"),
                        CartQuantity = cartInfo.Items.Where(d => d.SkuId == sku.Id).Sum(d => d.Quantity),
                        ImageUrl = Core.HimallIO.GetRomoteProductSizeImage(sku.ShowPic, 1, (int)ImageSize.Size_350)
                    };
                    Skus.Add(prosku);
                }
                defaultsku = Skus[0];
                #endregion
            }
            var json = JsonResult<dynamic>(new
            {
                ProductId = productId,
                ProductName = product.ProductName,
                ImageUrl = Core.HimallIO.GetRomoteProductSizeImage(product.ImagePath, 1, (int)ImageSize.Size_350), //GetImageFullPath(model.SubmitOrderImg),
                Stock = skuArray.Sum(s => s.Stock),// skus.Sum(s => s.Stock),
                ActivityUrl = activetype,
                SkuItems = SkuItemList,
                Skus = Skus,
                DefaultSku = defaultsku
            });
            return json;
        }

        /// <summary>
        /// 商品评价数接口
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetStatisticsReview(long productId)
        {
            var product = ServiceProvider.Instance<IProductService>.Create.GetProduct(productId);
            var comments = ServiceApplication.Create<ICommentService>().GetCommentsByProductId(productId);
            var json = JsonResult<dynamic>(new
            {
                productName = product.ProductName,
                reviewNum = comments.Where(c => c.IsHidden.Value == false).Count(),
                reviewNum1 = comments.Where(c => c.ReviewMark >= 4).Count(),
                reviewNum2 = comments.Where(c => c.ReviewMark == 3).Count(),
                reviewNum3 = comments.Where(c => c.ReviewMark <= 2).Count(),
                reviewNumImg = comments.Where(c => c.Himall_ProductCommentsImages.Count > 0).Count()
            });
            return json;
        }
        /// <summary>
        /// 商品评价列表
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetLoadReview(long productId, int pageIndex, int pageSize, int type)
        {
            IEnumerable<ProductCommentInfo> result;
            var comments = ServiceApplication.Create<ICommentService>().GetCommentsByProductId(productId);
            switch (type)
            {
                case 1:
                    result = comments.Where(c => c.ReviewMark >= 4).OrderByDescending(c => c.ReviewMark);
                    break;
                case 2:
                    result = comments.Where(c => c.ReviewMark == 3).OrderByDescending(c => c.ReviewMark);
                    break;
                case 3:
                    result = comments.Where(c => c.ReviewMark <= 2).OrderByDescending(c => c.ReviewMark);
                    break;
                case 4:
                    result = comments.Where(c => c.Himall_ProductCommentsImages.Count > 0);
                    break;
                default:
                    result = comments.OrderByDescending(c => c.ReviewMark);
                    break;
            }
            var temp = result.OrderByDescending(a => a.ReviewDate).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToArray();
            var data = temp.Select(c =>
            {
                string picture = Core.HimallIO.GetRomoteImagePath(c.Himall_Members.Photo);
                ProductTypeInfo typeInfo = ServiceApplication.Create<ITypeService>().GetTypeByProductId(c.ProductId);
                string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                List<string> Images = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 0).Select(a => a.CommentImage).ToList();//首评图片
                                                                                                                                         //var AppendImages = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 1).Select(a => new { CommentImage = Core.HimallIO.GetImagePath(a.CommentImage) });//追加图片
                string images1 = "", images2 = "", images3 = "", images4 = "", images5 = "";
                for (int i = 0; i < Images.Count; i++)
                {
                    if (i == 0)
                    {
                        images1 = Core.HimallIO.GetRomoteImagePath(MoveImages(Images[i]));
                    }
                    if (i == 1)
                    {
                        images2 = Core.HimallIO.GetRomoteImagePath(MoveImages(Images[i]));
                    }
                    if (i == 2)
                    {
                        images3 = Core.HimallIO.GetRomoteImagePath(MoveImages(Images[i]));
                    }
                    if (i == 3)
                    {
                        images4 = Core.HimallIO.GetRomoteImagePath(MoveImages(Images[i]));
                    }
                    if (i == 4)
                    {
                        images5 = Core.HimallIO.GetRomoteImagePath(MoveImages(Images[i]));
                    }
                }

                return new
                {
                    UserName = c.UserName,
                    Picture = picture,
                    ProductId = c.ProductId,
                    ProductName = c.ProductInfo.ProductName,
                    ThumbnailUrl100 = Core.HimallIO.GetRomoteImagePath(c.ProductInfo.ImagePath),
                    ReviewText = c.ReviewContent,
                    AppendContent = c.AppendContent,
                    SKUContent = "",
                    AppendDate = c.AppendDate.HasValue ? c.AppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                    ReplyAppendContent = c.ReplyAppendContent,
                    ReplyAppendDate = c.ReplyAppendDate.HasValue ? c.ReplyAppendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                    FinshDate = c.Himall_OrderItems.OrderInfo.FinishDate.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                    Images1 = images1,//首评图片
                    Images2 = images2,
                    Images3 = images3,
                    Images4 = images4,
                    Images5 = images5,
                    AppendImages = c.Himall_ProductCommentsImages.Where(a => a.CommentType == 1).Select(a => new { CommentImage = Core.HimallIO.GetRomoteImagePath(MoveImages(a.CommentImage)) }),//追加图片
                    ReviewDate = c.ReviewDate.ToString("yyyy-MM-dd"),
                    ReplyText = string.IsNullOrWhiteSpace(c.ReplyContent) ? null : c.ReplyContent,
                    ReplyDate = c.ReplyDate.HasValue ? c.ReplyDate.Value.ToString("yyyy-MM-dd") : " ",
                    ReviewMark = c.ReviewMark,
                    BuyDate = c.Himall_OrderItems.OrderInfo.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    Color = c.Himall_OrderItems.Color == null ? "" : c.Himall_OrderItems.Color,
                    Version = c.Himall_OrderItems.Version == null ? "" : c.Himall_OrderItems.Version,
                    Size = c.Himall_OrderItems.Size == null ? "" : c.Himall_OrderItems.Size,
                    ColorAlias = colorAlias,
                    SizeAlias = sizeAlias,
                    VersionAlias = versionAlias
                };
            }).ToArray();
            var json = JsonResult<dynamic>(new
            {
                totalCount = result.Count(),
                Data = data.AsEnumerable().Select(d => new
                {
                    UserName = d.UserName,
                    Picture = d.Picture,
                    ProductId = d.ProductId,
                    ThumbnailUrl100 = d.ThumbnailUrl100,
                    ProductName = d.ProductName,
                    SKUContent = d.SKUContent,
                    ReviewText = d.ReviewText,
                    Score = d.ReviewMark,
                    ImageUrl1 = d.Images1,
                    ImageUrl2 = d.Images2,
                    ImageUrl3 = d.Images3,
                    ImageUrl4 = d.Images4,
                    ImageUrl5 = d.Images5,
                    AppendImages = d.AppendImages,
                    ReplyText = d.ReplyText,
                    ReviewDate = d.ReviewDate,
                    ReplyDate = d.ReplyDate,
                    AppendContent = d.AppendContent,
                    AppendDate = d.AppendDate,
                    ReplyAppendContent = d.ReplyAppendContent,
                    ReplyAppendDate = d.ReplyAppendDate
                })
            });
            return json;
        }
        /// <summary>
        /// 添加商品评论（评价送积分）
        /// </summary>
        public JsonResult<Result<string>> GetAddProductReview(string openId, string DataJson)
        {
            CheckUserLogin();
            if (!string.IsNullOrEmpty(DataJson))
            {
                bool result = false;
                List<OrderCommentModel> orderComment = DataJson.FromJSON<List<OrderCommentModel>>();
                if (orderComment != null)
                {
                    List<ProductComment> list = new List<ProductComment>();
                    string orderIds = "";
                    foreach (var item in orderComment)
                    {
                        OrderCommentModel ordercom = new OrderCommentModel();
                        ordercom.ReviewDate = DateTime.Now;
                        ordercom.UserId = CurrentUser.Id;
                        ordercom.UserName = CurrentUser.UserName;
                        ordercom.UserEmail = CurrentUser.Email;
                        ordercom.OrderId = item.OrderId;
                        if (!orderIds.Contains(item.OrderId))
                        {
                            AddOrderComment(ordercom);//添加订单评价（订单评价只一次）
                            orderIds += item.OrderId + ",";
                        }

                        var model = new ProductComment();

                        var OrderInfo = ServiceApplication.Create<IOrderService>().GetOrderItemsByOrderId(long.Parse(item.OrderId)).Where(d => d.ProductId == item.ProductId).FirstOrDefault();
                        if (OrderInfo != null)
                        {
                            model.ReviewDate = DateTime.Now;
                            model.ReviewContent = item.ReviewText;
                            model.UserId = CurrentUser.Id;
                            model.UserName = CurrentUser.UserName;
                            model.Email = CurrentUser.Email;
                            model.SubOrderId = OrderInfo.Id;//订单明细Id
                            model.ReviewMark = item.Score;
                            model.ProductId = item.ProductId;
                            model.Images = new List<ProductCommentImage>();
                            foreach (var img in item.ImageUrl1.Split(','))
                            {
                                var p = new ProductCommentImage();

                                p.CommentType = 0;//0代表默认的表示评论的图片
                                p.CommentImage = Core.HimallIO.GetImagePath(img);
                                if (!string.IsNullOrEmpty(p.CommentImage))
                                {
                                    model.Images.Add(p);
                                }
                            }
                            list.Add(model);
                        }
                        result = true;
                    }
                    CommentApplication.Add(list);
                }
                if (result)
                {
                    return Json(SuccessResult("评价成功", "评价成功"));
                }
                else
                {
                    return Json(ErrorResult("评价失败", "评价失败"));
                }
            }
            return Json(ApiResult<string>(true));
        }
        /// <summary>
        /// 增加订单评论
        /// </summary>
        /// <param name="comment"></param>
        void AddOrderComment(OrderCommentModel comment)
        {
            TradeCommentApplication.Add(new OrderComment()
            {
                OrderId = long.Parse(comment.OrderId),
                DeliveryMark = 5,//物流评价
                ServiceMark = 5,//服务评价
                PackMark = 5,//包装评价
                UserId = comment.UserId,
                CommentDate = comment.ReviewDate,
                UserName = comment.UserName
            });
        }

        /// <summary>
        /// 获取商品批发价
        /// </summary>
        /// <param name="pid">商品ID</param>
        /// <param name="buyNum">数量</param>
        /// <returns></returns>
        public JsonResult<Result<string>> GetChangeNum(long pid, int buyNum)
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

            return JsonResult(_price.ToString("F2"));
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
        }

        internal IQueryable<CouponInfo> GetCouponList(long shopId)
        {
            var service = ServiceProvider.Instance<ICouponService>.Create;
            return service.GetCouponList(shopId);//商铺可用优惠券，排除过期和已领完的
        }

        private dynamic GetShopCouponList(long shopId)
        {
            var coupons = GetCouponList(shopId);
            if (coupons != null)
            {
                //VShopInfo vshop = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(shopId);
                //if (vshop == null)
                //{
                //    return null;
                //}
                var userCoupon = coupons.ToList().Where(p => p.ReceiveType == CouponInfo.CouponReceiveType.ShopIndex).Select(a => new
                {
                    CouponId = a.Id,
                    CouponName = a.CouponName,
                    Price = a.Price,
                    SendCount = a.Num,
                    UserLimitCount = a.PerMax,
                    OrderUseLimit = a.OrderAmount,
                    StartTime = a.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ClosingTime = a.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    CanUseProducts = "",
                    ObtainWay = a.ReceiveType,
                    NeedPoint = a.NeedIntegral,
                    UseWithGroup = false,
                    UseWithPanicBuying = false,
                    UseWithFireGroup = false,
                    LimitText = a.CouponName,
                    CanUseProduct = "店铺通用",
                    StartTimeText = a.StartTime.ToString("yyyy.MM.dd"),
                    ClosingTimeText = a.EndTime.ToString("yyyy.MM.dd"),
                    EndTime = a.EndTime,
                    Receive = Receive(a.ShopId, a.Id),
                    Remark = a.Remark,
                    UseArea = a.UseArea
                });
                var data = userCoupon.Where(p => p.Receive != 2 && p.Receive != 4).OrderByDescending(d => d.EndTime);//优惠券已经过期、优惠券已领完，则不显示在店铺优惠券列表中
                return data;
            }
            else
                return null;
        }

        private int Receive(long vshopId, long couponId)
        {
            var couponService = ServiceProvider.Instance<ICouponService>.Create;
            var couponInfo = couponService.GetCouponInfo(couponId);
            if (couponInfo.EndTime < DateTime.Now) return 2;//已经失效
            if (CurrentUser != null)
            {
                CouponRecordQuery crQuery = new CouponRecordQuery();
                crQuery.CouponId = couponId;
                crQuery.UserId = CurrentUser.Id;
                QueryPageModel<CouponRecordInfo> pageModel = couponService.GetCouponRecordList(crQuery);
                if (couponInfo.PerMax != 0 && pageModel.Total >= couponInfo.PerMax) return 3;//达到个人领取最大张数
                crQuery = new CouponRecordQuery()
                {
                    CouponId = couponId
                };
                pageModel = couponService.GetCouponRecordList(crQuery);
                if (pageModel.Total >= couponInfo.Num) return 4;//达到领取最大张数
                if (couponInfo.ReceiveType == Himall.Model.CouponInfo.CouponReceiveType.IntegralExchange)
                {
                    var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUserId);
                    if (userInte.AvailableIntegrals < couponInfo.NeedIntegral) return 5;//积分不足
                }
            }
            return 1;//可正常领取
        }

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

        private string MoveImages(string image)
        {
            if (string.IsNullOrWhiteSpace(image))
            {
                return "";
            }
            var oldname = Path.GetFileName(image);
            string ImageDir = string.Empty;

            //转移图片
            string relativeDir = "/Storage/Plat/Comment/";
            string fileName = oldname;
            if (image.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                var de = image.Substring(image.LastIndexOf("/temp/"));
                Core.HimallIO.CopyFile(de, relativeDir + fileName, true);
                return relativeDir + fileName;
            }  //目标地址
            else if (image.Contains("/Storage"))
            {
                return image.Substring(image.LastIndexOf("/Storage"));
            }
            return image;
        }

        private string GetFreightStr(long productId, decimal discount, string skuId, int addressId)
        {
            string freightStr = string.Empty;
            if (addressId <= 0)//如果用户的默认收货地址为空，则运费没法计算
                return freightStr;
            bool isFree = ProductManagerApplication.IsFreeRegion(productId, discount, addressId, 1, skuId);//默认取第一个规格
            if (isFree)
            {
                freightStr = "卖家承担运费";
            }
            else
            {
                decimal freight = ServiceApplication.Create<IProductService>().GetFreight(new List<long>() { productId }, new List<int>() { 1 }, addressId);
                freightStr = freight.ToString();
            }
            return freightStr;
        }

        /// <summary>
        /// 获取追加评论
        /// </summary>
        /// <param name="orderid"></param>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetAppendComment(long orderId)
        {
            CheckUserLogin();
            var model = CommentApplication.GetProductEvaluationByOrderIdNew(orderId, CurrentUser.Id);

            if (model.Count() > 0 && model.FirstOrDefault().AppendTime.HasValue)
                return Json(ErrorResult<dynamic>("追加评论时，获取数据异常", new int[0]));
            else
            {
                var listResult = model.Select(item => new
                {
                    Id = item.Id,
                    CommentId = item.CommentId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    //ThumbnailsUrl = item.ThumbnailsUrl,
                    ThumbnailsUrl = Core.HimallIO.GetRomoteProductSizeImage(item.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_220), //商城App追加评论时获取商品图片
                    BuyTime = item.BuyTime,
                    EvaluationStatus = item.EvaluationStatus,
                    EvaluationContent = item.EvaluationContent,
                    AppendContent = item.AppendContent,
                    AppendTime = item.AppendTime,
                    EvaluationTime = item.EvaluationTime,
                    ReplyTime = item.ReplyTime,
                    ReplyContent = item.ReplyContent,
                    ReplyAppendTime = item.ReplyAppendTime,
                    ReplyAppendContent = item.ReplyAppendContent,
                    EvaluationRank = item.EvaluationRank,
                    OrderId = item.OrderId,
                    CommentImages = item.CommentImages.Select(r => new
                    {
                        CommentImage = r.CommentImage,
                        CommentId = r.CommentId,
                        CommentType = r.CommentType
                    }).ToList(),
                    Color = item.Color,
                    Size = item.Size,
                    Version = item.Version
                }).ToList();
                return JsonResult<dynamic>(listResult);
            }
        }
        /// <summary>
        /// 追加评价
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public JsonResult<Result<int>> PostAppendComment(CommentAppendCommentModel value)
        {
            CheckUserLogin();
            string productCommentsJSON = value.productCommentsJSON;
            //var commentService = ServiceProvider.Instance<ICommentService>.Create;
            var productComments = JsonConvert.DeserializeObject<List<AppendCommentModel>>(productCommentsJSON);

            foreach (var m in productComments)
            {
                m.UserId = CurrentUser.Id;
            }
            CommentApplication.Append(productComments);
            return JsonResult<int>();
        }
    }
}
