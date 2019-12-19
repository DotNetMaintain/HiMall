using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using Himall.Web.App_Code;
using Himall.Web.Areas.Web.Models;
using Himall.Web.Framework;
using System.Linq;
using System.Web.Mvc;
using Himall.Web.Models;
using Himall.CommonModel;
using Himall.Application;
using Himall.Core;
using System.Collections.Generic;
using Himall.DTO;

namespace Himall.Web.Areas.Web.Controllers
{
    public class CartController : BaseWebController
    {
        CartHelper cartHelper;
        ICartService _iCartService;
        IProductService _iProductService;
        IMemberService _iMemberService;
        ISiteSettingService _iSiteSettingService;
        IOrderService _iOrderService;
        IShopService _iShopService;
        private ITypeService _iTypeService;

        public CartController(ICartService iCartService,
            IProductService iProductService,
            IMemberService iMemberService,
            ISiteSettingService iSiteSettingService,
            IOrderService iOrderService,
            IShopService iShopService, ITypeService iTypeService
            )
        {
            _iCartService = iCartService;
            _iProductService = iProductService;
            _iMemberService = iMemberService;
            _iSiteSettingService = iSiteSettingService;
            _iOrderService = iOrderService;
            _iShopService = iShopService;
            _iTypeService = iTypeService;
            cartHelper = new CartHelper();
        }
        /*
         *购物车存储说明：
         *游客访问时，点击加入购物车，购物车信息保存至Cookie中，游客点击结算时，Cookie中的购物车信息转移至数据库中并清空Cookie中购物车信息。
         *登录会员点击加入购物车时，购物车信息保存至数据库中。
         *Cookie存储格式： skuId1:count1,skuId2:count2,.....
         */


        // GET: Web/Cart
        public ActionResult AddToCart(string skuId, int count)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            try
            {

                cartHelper.AddToCart(skuId, count, userId);
            }
            catch { }
            return RedirectToAction("AddedToCart", new { skuId = skuId });
        }


        public ActionResult AddedToCart(string skuId)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            try
            {

                string productId = skuId.Split('_')[0];
                ViewBag.ProductId = productId;
                var productService = _iProductService;
                var cart = cartHelper.GetCart(userId);
                ProductInfo product;
                SKUInfo sku;
                var products = cart.Items.Select(item =>
                {
                    product = productService.GetProduct(item.ProductId);
                    sku = productService.GetSku(item.SkuId);
                    return new CartItemModel()
                    {
                        skuId = item.SkuId,
                        id = product.Id,
                        imgUrl = product.ImagePath + "/1_50.png",
                        name = product.ProductName,
                        price = sku.SalePrice,
                        count = item.Quantity
                    };
                });

                ViewBag.Current = products.FirstOrDefault(item => item.skuId == skuId);
                ViewBag.Others = products.Where(item => item.skuId != skuId);
                ViewBag.Amount = products.Sum(item => item.price * item.count);
                ViewBag.TotalCount = products.Sum(item => item.count);
                ViewBag.Keyword = CurrentSiteSetting.Keyword;
            }
            catch { }
            return View("AddToCart");
        }





        [HttpPost]
        public JsonResult AddProductToCart(string skuId, int count)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            try
            {

                cartHelper.AddToCart(skuId, count, userId);
                return Json(new { success = true });
            }
            catch (HimallException ex) { return Json(new { success = false, msg = ex.Message }); }
        }

        /// <summary>
        /// 验证商品是否可加入购物车
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <returns>success  JSON数据,返回真表示可以加入购物车</returns>
        [HttpPost]
        public JsonResult verificationToCart(long id)
        {
            long buyid = 0;
            bool success = false;

            var iLimitService = ServiceApplication.Create<ILimitTimeBuyService>();
            var ltmbuy = iLimitService.GetLimitTimeMarketItemByProductId(id);
            if (ltmbuy != null)
            {
                buyid = ltmbuy.Id;
            }
            else
            {
                var sku = _iProductService.GetSKUs(id);
                if (sku.ToList().Count == 1 && sku.FirstOrDefault().Id.Contains("0_0_0"))
                {
                    success = true;
                }
            }

            return Json(new { success = success, id = buyid });
        }

        public ActionResult BatchAddToCart(string skuIds, string counts)
        {
            var skuIdsArr = skuIds.Split(',');
            var countsArr = counts.Split(',').Select(item => int.Parse(item));

            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            for (int i = 0; i < skuIdsArr.Count(); i++)
                cartHelper.AddToCart(skuIdsArr.ElementAt(i), countsArr.ElementAt(i), userId);
            return RedirectToAction("cart");
        }



        public ActionResult Cart()
        {
            //Logo
            ViewBag.Logo = _iSiteSettingService.GetSiteSettings().Logo;
            ViewBag.Step = 1;

            CartCartModel model = new CartCartModel();

            var memberInfo = base.CurrentUser;

            ViewBag.Member = memberInfo;
            long uid = 0;
            if (CurrentUser != null)
            {
                uid = CurrentUser.Id;
            }

            model.Top3RecommendProducts = _iProductService.GetPlatHotSaleProductByNearShop(10, uid, true).ToList();
            ViewBag.Keyword = CurrentSiteSetting.Keyword;
            return View(model);
        }

        [HttpPost]
        public JsonResult RemoveFromCart(string skuId)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;


            cartHelper.RemoveFromCart(skuId, userId);
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult BatchRemoveFromCart(string skuIds)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;

            var skuIdsArr = skuIds.Split(',');

            cartHelper.RemoveFromCart(skuIdsArr, userId);
            return Json(new { success = true });
        }


        [HttpPost]
        public JsonResult UpdateCartItem(string skuId, int count)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            var orderService = _iOrderService;
            SKUInfo skuinfo = orderService.GetSkuByID(skuId);
            if (skuinfo.Stock < count)
                return Json(new { success = false, msg = "库存不足" });

            var product = ProductManagerApplication.GetProduct(skuinfo.ProductId);
            if (product != null)
            {
                if (product.MaxBuyCount > 0 && count > product.MaxBuyCount && !product.IsOpenLadder)
                {
                    return Json(new { success = false, msg = string.Format("每个ID限购{0}件", product.MaxBuyCount) });
                }
            }
            cartHelper.UpdateCartItem(skuId, count, userId);

            #region 购物车修改数量阶梯价变动--张宇枫
            //获取产品详情
            var price = 0m;
            if (product.IsOpenLadder)
            {
                var shop = ShopApplication.GetShop(product.ShopId);

                var groupCartByProduct = cartHelper.GetCart(userId).Items.Where(item => (!item.ShopBranchId.HasValue || item.ShopBranchId == 0)).Select(c => {
                    var cItem = new ShoppingCartItem();
                    var skuInfo = _iProductService.GetSku(c.SkuId);
                    if (skuInfo != null)
                        cItem = c;
                    return cItem;
                }).GroupBy(i => i.ProductId).ToList();
                var quantity = groupCartByProduct.Where(i => i.Key == product.Id).ToList().Sum(cartitem => cartitem.Sum(i => i.Quantity));

                decimal discount = 1M;
                if (CurrentUser != null)
                {
                    discount = CurrentUser.MemberDiscount;
                }
                price = ProductManagerApplication.GetProductLadderPrice(product.Id, quantity);
                if (shop.IsSelf)
                    price = price * discount;
            }

            #endregion

            return Json(new { success = true, saleprice = price.ToString("F2"), productid = product.Id, isOpenLadder = product.IsOpenLadder });
        }

        [HttpPost]
        public JsonResult GetSkuByID(string skuId)
        {
            var orderService = _iOrderService;
            SKUInfo skuinfo = orderService.GetSkuByID(skuId);
            var json = new
            {
                Color = skuinfo.Color,
                Size = skuinfo.Size,
                Version = skuinfo.Version
            };
            return Json(json);
        }

        [HttpPost]
        public JsonResult GetCartProducts()
        {
            long userId = 0;
            decimal discount = 1M;
            decimal prodPrice = 0.0M;//优惠价格
            if (CurrentUser != null)
            {
                userId = CurrentUser.Id;
                discount = CurrentUser.MemberDiscount;
            }

            var cart = cartHelper.GetCart(userId);
            var productService = _iProductService;
            var shopService = _iShopService;
            var typeservice = _iTypeService;

            var groupCart = cart.Items.Where(item=>(!item.ShopBranchId.HasValue || item.ShopBranchId == 0)).Select(c=> {
                var cItem = new ShoppingCartItem();
                var skuInfo = productService.GetSku(c.SkuId);
                if (skuInfo != null)
                    cItem = c;
                return cItem;
            }).GroupBy(i => i.ProductId).ToList();
            var products = cart.Items.Where(d => !d.ShopBranchId.HasValue || d.ShopBranchId == 0).Select(item =>
            {
                var product = productService.GetProduct(item.ProductId);

                ProductTypeInfo typeInfo = typeservice.GetType(product.TypeId);
                string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

                var shop = ShopApplication.GetShop(product.ShopId);
                if (null != shop)
                {
                    SKUInfo sku = productService.GetSku(item.SkuId);

                    if (sku != null)
                    {
                        var price = sku.SalePrice;//

                        #region 阶梯价--张宇枫
                        //是否达到最小批量（1：达到，0：未达到）前台购物车判断使用
                        var isMinMath = 1;
                        //阶梯价
                        if (product.IsOpenLadder)
                        {
                            var quantity = groupCart.Where(i => i.Key == item.ProductId).ToList().Sum(cartitem => cartitem.Sum(i => i.Quantity));
                            price = ProductManagerApplication.GetProductLadderPrice(item.ProductId, quantity);

                            var minMath = ProductManagerApplication.GetProductLadderMinMath(item.ProductId);
                            if (quantity < minMath)
                                isMinMath = 0;

                        }
                        #endregion
                        #region 正在参加限时抢购商品在购物车失效 TDO:ZYF
                        var isLimit = false;
                        var limit = LimitTimeApplication.GetLimitTimeMarketItemByProductId(item.ProductId);
                        if (limit != null)
                        {
                            isLimit = limit.Status == FlashSaleInfo.FlashSaleStatus.Ongoing;
                        }
                        #endregion
                        return new
                        {
                            cartItemId = item.Id,
                            skuId = item.SkuId,
                            id = product.Id,
                            imgUrl = Himall.Core.HimallIO.GetProductSizeImage(product.RelativePath, 1, (int)ImageSize.Size_50),
                            name = product.ProductName,
                            productstatus = isLimit ? 0 : (sku.Stock <= 0 ? ProductInfo.ProductSaleStatus.InStock.GetHashCode() : product.SaleStatus.GetHashCode()),
                            status = isLimit ? 1 : ((product.AuditStatus == ProductInfo.ProductAuditStatus.Audited && product.SaleStatus == ProductInfo.ProductSaleStatus.OnSale) ? (0 >= sku.Stock ? 2 : 0) : 1),//0:正常；1：失效；2：库存不足
                            productauditstatus = product.AuditStatus,
                            price = shop.IsSelf ? price * discount : price,//sku.SalePrice,
                            Color = sku.Color,
                            Size = sku.Size,
                            Version = sku.Version,
                            count = item.Quantity,
                            shopId = shop.Id,
                            shopName = shop.ShopName,
                            productcode = !(sku.Version + sku.Color + sku.Size).Equals("") ? sku.Sku : product.ProductCode,
                            ColorAlias = colorAlias,
                            SizeAlias = sizeAlias,
                            VersionAlias = versionAlias,
                            AddTime = item.AddTime,
                            minMath = isMinMath
                        };
                    }
                }
                return null;
            }).Where(p => p != null).OrderBy(s => s.status).ThenByDescending(o => o.AddTime);

            /*
            #region 门店购物车

            var branchCartHelper = new BranchCartHelper();
            var Branchcart = branchCartHelper.GetCartNoCache(userId, 0);
            var shopBranchList = Branchcart.Items.Where(x => x.ShopBranchId.HasValue).Select(x => x.ShopBranchId.Value).ToList();
            shopBranchList = shopBranchList.GroupBy(x => x).Select(x => x.First()).ToList();
            Dictionary<long, int> buyedCounts = null;
            if (userId > 0)
            {
                cart = branchCartHelper.GetCart(userId, 0);
                buyedCounts = new Dictionary<long, int>();
                buyedCounts = OrderApplication.GetProductBuyCount(userId, cart.Items.Select(x => x.ProductId));
            }

            List<object> shopBranchCart = new List<object>();
            foreach (long shopBranchId in shopBranchList)
            {
                prodPrice = 0.0M;//优惠价格
                var shopBranchSkuList = ShopBranchApplication.GetSkusByIds(shopBranchId, Branchcart.Items.Select(x => x.SkuId));

                //limitProducts = LimitTimeApplication.GetPriceByProducrIds(Branchcart.Items.Select(e => e.ProductId).ToList());//限时购价格
                var productList = Branchcart.Items.Where(x => x.ShopBranchId == shopBranchId).Select(item =>
                {
                    if (shopBranchId == 99)
                    {
                        var tempi = 0;
                    }
                    var shopBranchInfo = ShopBranchApplication.GetShopBranchById(shopBranchId);
                    var product = _iProductService.GetProduct(item.ProductId);
                    var shop = _iShopService.GetShop(product.ShopId);
                    SKUInfo sku = null;
                    if (null != shop && shopBranchInfo != null)
                    {
                        var vshop = VshopApplication.GetVShopByShopId(shop.Id);
                        sku = _iProductService.GetSku(item.SkuId);
                        if (sku == null)
                        {
                            return null;
                        }

                        prodPrice = sku.SalePrice;

                        if (shop.IsSelf)
                        {//官方自营店才计算会员折扣
                            prodPrice = sku.SalePrice * discount;
                        }

                        var shopbranchsku = shopBranchSkuList.FirstOrDefault(x => x.SkuId == item.SkuId);
                        long stock = shopbranchsku == null ? 0 : shopbranchsku.Stock;
                        if (stock > product.MaxBuyCount && product.MaxBuyCount > 0)
                            stock = product.MaxBuyCount;
                        if (product.MaxBuyCount > 0 && buyedCounts != null && buyedCounts.ContainsKey(item.ProductId))
                        {
                            int buynum = buyedCounts[item.ProductId];
                            stock = stock - buynum;
                        }
                        return new
                        {
                            cartItemId = item.Id,
                            skuId = item.SkuId,
                            id = product.Id,
                            imgUrl = Himall.Core.HimallIO.GetProductSizeImage(product.RelativePath, 1, (int)ImageSize.Size_150),
                            name = product.ProductName,
                            price = prodPrice,
                            count = item.Quantity,
                            status = shopbranchsku == null ? 1 : (shopbranchsku.Status == ShopBranchSkuStatus.Normal) ? (item.Quantity > stock ? 2 : 0) : 1,//0:正常；1：冻结；2：库存不足
                            AddTime = item.AddTime,
                            shopBranchId = shopBranchInfo.Id,
                            shopBranchName = shopBranchInfo.ShopBranchName
                        };
                    }
                    else
                    {
                        return null;
                    }
                }).Where(d => d != null).OrderByDescending(s => s.AddTime);
                shopBranchCart.Add(productList);
            }
            #endregion
            */
            var cartModel = new
            {
                products = products,
                //amount = products.Where(item => item.productstatus == ProductInfo.ProductSaleStatus.OnSale.GetHashCode() && item.productauditstatus != ProductInfo.ProductAuditStatus.InfractionSaleOff && item.productauditstatus != ProductInfo.ProductAuditStatus.WaitForAuditing).Sum(item => item.price * item.count),
                amount = products.Where(item => item.status == 0 && item.productauditstatus != ProductInfo.ProductAuditStatus.InfractionSaleOff && item.productauditstatus != ProductInfo.ProductAuditStatus.WaitForAuditing).Sum(item => item.price * item.count),
                //totalCount = products.Where(item => item.productstatus == ProductInfo.ProductSaleStatus.OnSale.GetHashCode() && item.productauditstatus != ProductInfo.ProductAuditStatus.InfractionSaleOff && item.productauditstatus != ProductInfo.ProductAuditStatus.WaitForAuditing).Sum(item => item.count),
                totalCount = products.Where(item => item.status == 0 && item.productauditstatus != ProductInfo.ProductAuditStatus.InfractionSaleOff && item.productauditstatus != ProductInfo.ProductAuditStatus.WaitForAuditing).Sum(item => item.count),
                //shopBranchCart = shopBranchCart
            };
            return Json(cartModel);
        }
    }
}