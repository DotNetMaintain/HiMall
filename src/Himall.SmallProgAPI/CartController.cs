using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.IServices;
using Himall.Model;
using Himall.SmallProgAPI.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;

namespace Himall.SmallProgAPI
{
    public class CartController : BaseApiController
    {
        /// <summary>
        /// 添加到购物车
        /// </summary>
        /// <param name="context"></param>
        public JsonResult<Result<int>> GetAddToCart(string openId, string SkuID, int Quantity, int GiftID = 0)
        {
            //验证用户
            CheckUserLogin();
            CartHelper cartHelper = new CartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            var msg = "";
            try
            {
                cartHelper.AddToCart(SkuID, Quantity, userId);
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            if (!string.IsNullOrEmpty(msg))
            {
                return Json(ErrorResult<int>(msg));
            }
            else
            {
                return Json(SuccessResult<int>());
            }
        }
        /// <summary>
        /// 更新购物车数量
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="SkuID"></param>
        /// <param name="Quantity"></param>
        /// <param name="GiftID"></param>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetUpdateToCart(string openId, string SkuID, int Quantity, int GiftID = 0)
        {
            //验证用户
            CheckUserLogin();
            var cartHelper = new CartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;

            var skus = cartHelper.GetCart(userId);
            var oldQuantity = GetCartProductQuantity(skus, skuId: SkuID);
            oldQuantity = oldQuantity + Quantity;

            long productId = 0;
            var skuItem = skus.Items.FirstOrDefault(i => i.SkuId == SkuID);
            if (null == skuItem)
            {
                var sku = ProductManagerApplication.GetSKU(SkuID);
                if (null == sku) return Json(ErrorResult<dynamic>("错误的参数:SkuId"));
                productId = sku.ProductId;
            }
            else
            {
                productId = skuItem.ProductId;
            }
            var ladderPrice = 0m;
            var product = ProductManagerApplication.GetProduct(productId);
            if (product != null)
            {
                if (product.MaxBuyCount > 0 && oldQuantity > product.MaxBuyCount && !product.IsOpenLadder)
                {
                    return Json(ErrorResult<dynamic>(string.Format("每个ID限购{0}件", product.MaxBuyCount)));
                }
            }

            cartHelper.UpdateCartItem(SkuID, oldQuantity, userId);
            //调用查询购物车数据

            #region 阶梯价--张宇枫
            var isOpenLadder = product.IsOpenLadder;
            if (isOpenLadder)
            {
                var shop = ShopApplication.GetShop(product.ShopId);
                var groupCartByProduct = skus.Items.GroupBy(i => i.ProductId).ToList();
                var quantity =
                    groupCartByProduct.Where(i => i.Key == productId)
                        .ToList()
                        .Sum(cartitem => cartitem.Sum(i => i.Quantity));
                ladderPrice = ProductManagerApplication.GetProductLadderPrice(productId, quantity);
                if (shop.IsSelf)
                {
                    ladderPrice = CurrentUser.MemberDiscount * ladderPrice;
                }
            }
            #endregion
            return JsonResult<dynamic>(new { Price = ladderPrice.ToString("F2"), ProductId = productId, IsOpenLadder = isOpenLadder ? 1 : 0 });
        }

        /// <summary>
        /// 从购物车移除
        /// </summary>
        /// <param name="context"></param>
        public JsonResult<Result<int>> GetdelCartItem(string openId, string SkuIds, int GiftID = 0)
        {
            //验证用户
            CheckUserLogin();
            CartHelper cartHelper = new CartHelper();
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            var skuIdsArr = SkuIds.ToString().Split(',');
            cartHelper.RemoveFromCart(skuIdsArr, userId);
            //调用查询购物车数据

            return JsonResult<int>();
        }

        public JsonResult<Result<IEnumerable<IGrouping<string, CartProductModel>>>> GetCartProduct(string openId = "")
        {
            CheckUserLogin();
            var cartHelper = ServiceProvider.Instance<ICartService>.Create;
            var cart = cartHelper.GetCart(CurrentUser.Id);
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var shopService = ServiceProvider.Instance<IShopService>.Create;
            var vshopService = ServiceProvider.Instance<IVShopService>.Create;
            var siteSetting = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings();
            var typeservice = ServiceProvider.Instance<ITypeService>.Create;
            List<CartProductModel> products = new List<CartProductModel>();
            //会员折扣
            decimal discount = 1.0M;//默认折扣为1（没有折扣）
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }
            decimal prodPrice = 0.0M;//优惠价格
            var limitProducts = LimitTimeApplication.GetPriceByProducrIds(cart.Items.Select(e => e.ProductId).ToList());//限时购价格
            var groupCart = cart.Items.Where(item => (!item.ShopBranchId.HasValue || item.ShopBranchId == 0)).Select(c => {
                var cItem = new ShoppingCartItem();
                var skuInfo = productService.GetSku(c.SkuId);
                if (skuInfo != null)
                    cItem = c;
                return cItem;
            }).GroupBy(i => i.ProductId).ToList();
            foreach (var item in cart.Items.ToList())
            {
                var product = productService.GetProduct(item.ProductId);
                var shop = shopService.GetShop(product.ShopId);

                if (null != shop)
                {
                    var vshop = vshopService.GetVShopByShopId(shop.Id);
                    SKUInfo sku = productService.GetSku(item.SkuId);
                    if (sku == null)
                    {
                        continue;
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
                    #region 阶梯价--张宇枫
                    //阶梯价
                    if (product.IsOpenLadder)
                    {
                        var quantity = groupCart.Where(i => i.Key == item.ProductId).ToList().Sum(cartitem => cartitem.Sum(i => i.Quantity));
                        prodPrice = ProductManagerApplication.GetProductLadderPrice(item.ProductId, quantity);
                        if (shop.IsSelf)
                            prodPrice = prodPrice * discount;
                    }
                    #endregion
                    ProductTypeInfo typeInfo = typeservice.GetType(product.TypeId);
                    string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                    string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                    string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

                    if (sku != null)
                    {
                        #region 正在参加限时抢购商品在购物车失效 TDO:ZYF
                        var isLimit = false;
                        var limit = LimitTimeApplication.GetLimitTimeMarketItemByProductId(item.ProductId);
                        if (limit != null)
                        {
                            isLimit = limit.Status == FlashSaleInfo.FlashSaleStatus.Ongoing;
                        }
                        #endregion
                        var _tmp = new CartProductModel
                        {
                            CartItemId = item.Id.ToString(),
                            SkuId = item.SkuId,
                            Id = product.Id.ToString(),
                            ImgUrl = Core.HimallIO.GetRomoteProductSizeImage(product.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_150),
                            Name = product.ProductName,
                            Price = prodPrice.ToString("F2"),
                            Count = item.Quantity.ToString(),
                            ShopId = shop.Id.ToString(),
                            Size = sku.Size,
                            Color = sku.Color,
                            Version = sku.Version,
                            VShopId = vshop == null ? "0" : vshop.Id.ToString(),
                            ShopName = shop.ShopName,
                            ShopLogo = vshop == null ? "" : Core.HimallIO.GetRomoteImagePath(vshop.Logo),
                            Url = Core.HimallIO.GetRomoteImagePath("/m-IOS/product/detail/") + item.ProductId,
                            IsValid = isLimit ? 1 : ((product.AuditStatus == ProductInfo.ProductAuditStatus.Audited && product.SaleStatus == ProductInfo.ProductSaleStatus.OnSale) ? (0 >= sku.Stock ? 2 : 0) : 1),
                            ColorAlias = colorAlias,
                            SizeAlias = sizeAlias,
                            VersionAlias = versionAlias,
                            AddTime = item.AddTime,
                            IsOpenLadder = product.IsOpenLadder,
                            MaxBuyCount = product.MaxBuyCount,
                            MinBath = ProductManagerApplication.GetProductLadderMinMath(product.Id)
                        };
                        products.Add(_tmp);
                    }
                }
            }

            //products = products.OrderBy(item => item.ShopId).ThenByDescending(o => o.AddTime).ToList();
            products = products.OrderBy(p => p.IsValid).ThenByDescending(item => item.AddTime).ToList();
            var cartShop = products.GroupBy(item => item.ShopId);
            return JsonResult(cartShop);//原api返回
        }

        /// <summary>
        /// 检查失效商品
        /// </summary>
        /// <param name="skus"></param>
        /// <param name="memeberId"></param>
        /// <returns></returns>
        public JsonResult<Result<int>> GetCanSubmitOrder(string openId, string skus)
        {
            CheckUserLogin();
            if (!string.IsNullOrEmpty(skus))
            {
                bool status = true;
                var SkuIds = skus.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).ToList();
                CartHelper cartHelper = new CartHelper();
                foreach (var item in SkuIds)
                {
                    if (!cartHelper.CheckSkuId(item))
                    {
                        status = false;
                    }
                }
                if (status)
                {
                    return JsonResult<int>();
                }
                else
                {
                    return Json(ErrorResult<int>("有失效商品"));
                }
            }
            else
            {
                return Json(ErrorResult<int>("请选择商品"));
            }
        }

        /// <summary>
        /// 判断购物车结算的阶梯商品是否达到最小批量
        /// </summary>
        /// <param name="cartItemIds"></param>
        /// <returns></returns>
        public JsonResult<Result<bool>> GetLadderMintMath(string cartItemIds)
        {
            var msg = "";
            var result = ProductManagerApplication.IsExistLadderMinMath(cartItemIds, ref msg);
            return JsonResult(result, msg: msg);
        }

        private int GetCartProductQuantity(ShoppingCartInfo cartInfo, long productId = 0, string skuId = "")
        {
            int cartQuantity = 0;
            if (cartInfo == null)
            {
                return 0;
            }
            else
            {
                if (productId > 0)
                {
                    cartQuantity += cartInfo.Items.Where(p => p.ProductId == productId).Sum(d => d.Quantity);
                }
                else
                {
                    cartQuantity += cartInfo.Items.Where(p => p.SkuId == skuId).Sum(d => d.Quantity);
                }
            }
            return cartQuantity;
        }
    }
}
