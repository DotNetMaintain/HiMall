using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class CartController : BaseMobileMemberController
    {
        IProductService _iProductService;
        IShopService _iShopService;
        IVShopService _iVShopService;
        IShopBranchService _iShopBranchService;
        public CartController(IProductService iProductService, IShopService iShopService, IVShopService iVShopService, IShopBranchService iShopBranchService)
        {
            _iProductService = iProductService;
            _iShopService = iShopService;
            _iVShopService = iVShopService;
            _iShopBranchService = iShopBranchService;
        }
        // GET: Mobile/Cart
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Cart()
        {
            return View();
        }

        [HttpPost]
        public JsonResult AddProductToCart(string skuId, int count)
        {
            CartHelper cartHelper = new CartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            cartHelper.AddToCart(skuId, count, userId);
            return Json(new { success = true });
        }

        [HttpPost]


        public JsonResult GetCartProducts()
        {
            var cartHelper = new CartHelper();
            var cart = cartHelper.GetCart(CurrentUser.Id);
            var productService = _iProductService;
            var shopService = _iShopService;
            var vshopService = _iVShopService;
            //会员折扣
            decimal discount = 1.0M;//默认折扣为1（没有折扣）
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            List<long> pids = new List<long>();
            decimal prodPrice = 0.0M;//优惠价格
            var limitProducts = LimitTimeApplication.GetPriceByProducrIds(cart.Items.Select(e => e.ProductId).ToList());//限时购价格
            var groupCart = cart.Items.Where(item => (!item.ShopBranchId.HasValue || item.ShopBranchId == 0)).Select(c => {
                var cItem = new ShoppingCartItem();
                var skuInfo = productService.GetSku(c.SkuId);
                if (skuInfo != null)
                    cItem = c;
                return cItem;
            }).GroupBy(i => i.ProductId).ToList();
            var products = cart.Items.Where(d => (!d.ShopBranchId.HasValue || d.ShopBranchId == 0)&& d.Status==0).Select(item =>
                {
                    var product = productService.GetProduct(item.ProductId);
                    var shop = shopService.GetShop(product.ShopId);
                    SKUInfo sku = null;
                    string colorAlias = "";
                    string sizeAlias = "";
                    string versionAlias = "";
                    string skuDetails = "";
                    if (null != shop)
                    {
                        var vshop = vshopService.GetVShopByShopId(shop.Id);
                        sku = productService.GetSku(item.SkuId);
                        if (sku == null)
                        {
                            return null;
                        }
                        //处理限时购、会员折扣价格
                        var prod = limitProducts.FirstOrDefault(e => e.ProductId == item.ProductId);
                        prodPrice = sku.SalePrice;
                        if (prod != null)
                        {
                            prodPrice = prod.MinPrice;
                        }
                        else
                        {
                            if (shop.IsSelf)
                            {//官方自营店才计算会员折扣
                                prodPrice = sku.SalePrice * discount;
                            }
                        }
                        //阶梯价
                        if (product.IsOpenLadder)
                        {
                            var quantity = groupCart.Where(i => i.Key == item.ProductId).ToList().Sum(cartitem => cartitem.Sum(i => i.Quantity));
                            prodPrice = ProductManagerApplication.GetProductLadderPrice(item.ProductId, quantity);
                            if (shop.IsSelf)
                                prodPrice = prodPrice * discount;
                        }
                        var typeInfo = TypeApplication.GetType(product.TypeId);
                        colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                        sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                        versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                        skuDetails = "";
                        if (!string.IsNullOrWhiteSpace(sku.Size))
                        {
                            skuDetails += sizeAlias + "：" + sku.Size + "&nbsp;&nbsp;";
                        }
                        if (!string.IsNullOrWhiteSpace(sku.Color))
                        {
                            skuDetails += colorAlias + "：" + sku.Color + "&nbsp;&nbsp;";
                        }
                        if (!string.IsNullOrWhiteSpace(sku.Version))
                        {
                            skuDetails += versionAlias + "：" + sku.Version + "&nbsp;&nbsp;";
                        }
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
                            imgUrl = Himall.Core.HimallIO.GetProductSizeImage(product.RelativePath, 1, (int)ImageSize.Size_150),
                            name = product.ProductName,
                            price = prodPrice,
                            count = item.Quantity,
                            shopId = shop.Id,
                            vshopId = vshop == null ? 0 : vshop.Id,
                            shopName = shop.ShopName,
                            shopLogo = vshop == null ? "" : vshop.Logo,
                            status = isLimit ? 1 : ((product.AuditStatus == ProductInfo.ProductAuditStatus.Audited && product.SaleStatus == ProductInfo.ProductSaleStatus.OnSale) ? (0 >= sku.Stock ? 2 : 0) : 1),//0:正常；1：失效；2：库存不足
                            Size = sku.Size,
                            Color = sku.Color,
                            Version = sku.Version,
                            ColorAlias = colorAlias,
                            SizeAlias = sizeAlias,
                            VersionAlias = versionAlias,
                            skuDetails = skuDetails,
                            AddTime = item.AddTime,
                            minMath = ProductManagerApplication.GetProductLadderMinMath(product.Id)
                        };
                    }
                    else
                    {
                        return null;
                    }
                }).Where(d => d != null).OrderBy(p => p.status).ThenByDescending(o => o.AddTime);

            #region 门店购物车

            var branchCartHelper = new BranchCartHelper();
            long userId = 0;
            if (CurrentUser != null)
            {
                userId = CurrentUser.Id;
            }
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
                var shopBranchSkuList = _iShopBranchService.GetSkusByIds(shopBranchId, Branchcart.Items.Select(x => x.SkuId));

                //limitProducts = LimitTimeApplication.GetPriceByProducrIds(Branchcart.Items.Select(e => e.ProductId).ToList());//限时购价格
                var productList = Branchcart.Items.Where(x => x.ShopBranchId == shopBranchId).Select(item =>
                {
                    if (shopBranchId == 99)
                    {
                        var tempi = 0;
                    }
                    var shopBranchInfo = _iShopBranchService.GetShopBranchById(shopBranchId);
                    var product = _iProductService.GetProduct(item.ProductId);
                    var shop = _iShopService.GetShop(product.ShopId);
                    SKUInfo sku = null;
                    if (null != shop && shopBranchInfo != null)
                    {
                        var vshop = _iVShopService.GetVShopByShopId(shop.Id);
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
                        prodPrice = Math.Round(prodPrice, 2);
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
                            //阶梯价商品在门店购物车自动下架
                            status = product.IsOpenLadder ? 1 : (shopbranchsku == null ? 1 : (shopbranchsku.Status == ShopBranchSkuStatus.Normal) ? (item.Quantity > shopbranchsku.Stock ? 2 : 0) : 1),//0:正常；1：冻结；2：库存不足
                            AddTime = item.AddTime,
                            shopBranchId = shopBranchInfo.Id,
                            shopBranchName = shopBranchInfo.ShopBranchName
                        };
                    }
                    else
                    {
                        return null;
                    }
                }).Where(d => d != null).OrderBy(p => p.status).ThenByDescending(s => s.AddTime);
                shopBranchCart.Add(productList);
            }
            #endregion

            var cartModel = new { products = products, amount = products.Sum(item => item.price * item.count), totalCount = products.Sum(item => item.count), shopBranchCart = shopBranchCart };
            return SuccessResult<dynamic>(data: cartModel);
        }

        [HttpPost]
        public JsonResult UpdateCartItem(string skuId, int count)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            var cartHelper = new CartHelper();

            SKUInfo skuinfo = OrderApplication.GetSkuByID(skuId);
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

            return SuccessResult<dynamic>(data: new { saleprice = price.ToString("F2"), productid = product.Id, isOpenLadder = product.IsOpenLadder });
        }

        public JsonResult UpdateCartItem(Dictionary<string, int> skus, long userId)
        {
            var cartHelper = new CartHelper();
            foreach (var sku in skus)
            {
                cartHelper.UpdateCartItem(sku.Key, sku.Value, userId);
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult BatchRemoveFromCart(string skuIds)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;

            var skuIdsArr = skuIds.Split(',');
            var cartHelper = new CartHelper();
            cartHelper.RemoveFromCart(skuIdsArr, userId);
            return Json(new { success = true });
        }
        [HttpPost]
        public JsonResult RemoveFromCart(string skuId)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;

            var cartHelper = new CartHelper();
            cartHelper.RemoveFromCart(skuId, userId);
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult IsLadderCount(string cartItemIds)
        {
            var msg = "";
            var result = ProductManagerApplication.IsExistLadderMinMath(cartItemIds, ref msg);
            return Json(new { success = result,msg= msg });
        }

        #region 门店相关购物车
        /// <summary>
        /// 修改购物车商品
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="count"></param>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditBranchProductToCart(string skuId, int count, long shopBranchId)
        {
            BranchCartHelper branchCartHelper = new BranchCartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            branchCartHelper.UpdateCartItem(skuId, count, userId, shopBranchId);
            return Json(new { success = true });
        }
        /// <summary>
        /// 获取底部购物车详情
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetBranchCartProducts(long shopBranchId)
        {
            var branchCartHelper = new BranchCartHelper();
            long userId = 0;
            //会员折扣
            decimal discount = 1.0M;//默认折扣为1（没有折扣）
            if (CurrentUser != null)
            {
                userId = CurrentUser.Id;
                discount = CurrentUser.MemberDiscount;
            }
            var cart = branchCartHelper.GetCart(userId, shopBranchId);
            var shopBranch = _iShopBranchService.GetShopBranchById(shopBranchId);
            Dictionary<long, int> buyedCounts = null;
            if (userId > 0)
            {
                buyedCounts = new Dictionary<long, int>();
                buyedCounts = OrderApplication.GetProductBuyCount(userId, cart.Items.Select(x => x.ProductId));
            }
            decimal prodPrice = 0.0M;//优惠价格
            var shopBranchSkuList = _iShopBranchService.GetSkusByIds(shopBranchId, cart.Items.Select(x => x.SkuId));

            var products = cart.Items.Select(item =>
            {
                var product = _iProductService.GetProduct(item.ProductId);
                var shopbranchsku = shopBranchSkuList.FirstOrDefault(x => x.SkuId == item.SkuId);
                long stock = shopbranchsku == null ? 0 : shopbranchsku.Stock;

                if (stock > product.MaxBuyCount && product.MaxBuyCount != 0)
                    stock = product.MaxBuyCount;
                if (product.MaxBuyCount > 0 && buyedCounts != null && buyedCounts.ContainsKey(item.ProductId))
                {
                    int buynum = buyedCounts[item.ProductId];
                    stock = stock - buynum;
                }

                var shop = _iShopService.GetShop(product.ShopId);
                SKUInfo sku = null;
                string skuDetails = "";
                if (null != shop)
                {
                    var vshop = _iVShopService.GetVShopByShopId(shop.Id);
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

                    var typeInfo = TypeApplication.GetType(product.TypeId);
                    skuDetails = "";
                    if (!string.IsNullOrWhiteSpace(sku.Size))
                    {
                        skuDetails += sku.Size + "&nbsp;&nbsp;";
                    }
                    if (!string.IsNullOrWhiteSpace(sku.Color))
                    {
                        skuDetails += sku.Color + "&nbsp;&nbsp;";
                    }
                    if (!string.IsNullOrWhiteSpace(sku.Version))
                    {
                        skuDetails += sku.Version + "&nbsp;&nbsp;";
                    }
                    return new
                    {
                        bId = shopBranchId,
                        cartItemId = item.Id,
                        skuId = item.SkuId,
                        id = product.Id,
                        name = product.ProductName,
                        price = prodPrice,
                        count = item.Quantity,
                        stock = shopbranchsku == null ? 0 : stock,
                        //阶梯价商品在门店购物车自动下架
                        status = product.IsOpenLadder ? 1 : (shopbranchsku == null ? 1 : (shopbranchsku.Status == ShopBranchSkuStatus.Normal) ? (item.Quantity > stock ? 2 : 0) : 1),//0:正常；1：冻结；2：库存不足
                        skuDetails = skuDetails,
                        AddTime = item.AddTime
                    };
                }
                else
                {
                    return null;
                }
            }).Where(d => d != null).OrderBy(s => s.status).ThenByDescending(o => o.AddTime);

            var cartModel = new { products = products, amount = products.Where(x => x.status == 0).Sum(item => item.price * item.count), totalCount = products.Where(x => x.status == 0).Sum(item => item.count), DeliveFee = shopBranch.DeliveFee, DeliveTotalFee = shopBranch.DeliveTotalFee, FreeMailFee = shopBranch.FreeMailFee, shopBranchStatus = (int)shopBranch.Status };
            return SuccessResult<dynamic>(data: cartModel);
        }


        /// <summary>
        /// 清理门店购物车所有商品
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public JsonResult ClearBranchCartProducts(long shopBranchId)
        {
            BranchCartHelper branchCartHelper = new BranchCartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            var cart = branchCartHelper.GetCart(userId, shopBranchId);
            foreach (var item in cart.Items)
            {
                branchCartHelper.RemoveFromCart(item.SkuId, userId, shopBranchId);
            }
            return Json(new { success = true });
        }
        /// <summary>
        /// 清理门店购物车所有无效商品
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public JsonResult ClearBranchCartInvalidProducts(long shopBranchId)
        {
            BranchCartHelper branchCartHelper = new BranchCartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            var cart = branchCartHelper.GetCart(userId, shopBranchId);
            Dictionary<long, int> buyedCounts = null;
            if (userId > 0)
            {
                buyedCounts = new Dictionary<long, int>();
                buyedCounts = OrderApplication.GetProductBuyCount(userId, cart.Items.Select(x => x.ProductId));
            }
            foreach (var item in cart.Items)
            {
                var product = _iProductService.GetProduct(item.ProductId);
                var shopbranchsku = _iShopBranchService.GetSkusByIds(shopBranchId, new List<string> { item.SkuId }).FirstOrDefault();
                long stock = shopbranchsku == null ? 0 : shopbranchsku.Stock;

                if (stock > product.MaxBuyCount && product.MaxBuyCount != 0)
                    stock = product.MaxBuyCount;
                if (product.MaxBuyCount > 0 && buyedCounts != null && buyedCounts.ContainsKey(item.ProductId))
                {
                    int buynum = buyedCounts[item.ProductId];
                    stock = stock - buynum;
                }

                if (shopbranchsku.Status != ShopBranchSkuStatus.Normal || item.Quantity > stock)
                {
                    branchCartHelper.RemoveFromCart(item.SkuId, userId, shopBranchId);
                }
            }
            return Json(new { success = true });
        }

        #endregion
    }
}