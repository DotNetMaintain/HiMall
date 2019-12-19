using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.SmallProgAPI.O2O.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;

namespace Himall.SmallProgAPI
{
    public class O2OCartController : BaseO2OApiController
    {
        /// <summary>
        /// 加入购物车
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="count"></param>
        /// <param name="memberId"></param>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public JsonResult<Result<int>> GetUpdateCartItem(string skuId, int count, long shopBranchId)
        {
            CheckUserLogin();
            CheckSkuIdIsValid(skuId, shopBranchId);
            //判断库存
            var sku = ProductManagerApplication.GetSKU(skuId);
            if (sku == null)
            {
                throw new HimallException("错误的SKU");
            }
            //if (count > sku.Stock)
            //{
            //    throw new HimallException("库存不足");
            //}
            var shopBranch = ShopBranchApplication.GetShopBranchById(shopBranchId);
            if (shopBranch == null)
            {
                throw new HimallException("错误的门店id");
            }
            var shopBranchSkuList = ShopBranchApplication.GetSkusByIds(shopBranchId, new List<string> { skuId });
            if (shopBranchSkuList == null || shopBranchSkuList.Count == 0 || shopBranchSkuList[0].Status == ShopBranchSkuStatus.InStock)
            {
                throw new HimallException("门店没有该商品或已下架");
            }
            var sbsku = shopBranchSkuList.FirstOrDefault();
            if (sbsku.Stock < count)
            {
                throw new HimallException("门店库存不足");
            }
            long memberId = CurrentUser.Id;
            CartApplication.UpdateShopBranchCart(skuId, count, memberId, shopBranchId);
            return JsonResult<int>();
        }

        /// <summary>
        /// 检验SkuId
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="shopBranchId"></param>
        private void CheckSkuIdIsValid(string skuId, long shopBranchId)
        {
            long productId = 0;
            long.TryParse(skuId.Split('_')[0], out productId);
            if (productId == 0)
                throw new Himall.Core.InvalidPropertyException("SKUId无效");

            var skuItem = ProductManagerApplication.GetSKU(skuId);
            if (skuItem == null)
                throw new Himall.Core.InvalidPropertyException("SKUId无效");

        }
        /// <summary>
        /// 获取购物车中的商品
        /// </summary>
        /// <returns></returns>
        public JsonResult<Result<List<CartStoreModel>>> GetCart(long shopBranchId)
        {
            CheckUserLogin();
            long userId = CurrentUserId;
            //会员折扣
            decimal discount = CurrentUser.MemberDiscount;
            var cart = CartApplication.GetShopBranchCart(userId, shopBranchId);
            var stores = ShopBranchApplication.GetShopBranchByIds(cart.Items.Where(d => d.ShopBranchId.HasValue).Select(d => d.ShopBranchId.Value).ToList());
            decimal prodPrice = 0.0M;//优惠价格
            var rets = new List<CartStoreModel>();
            foreach (var item in stores)
            {
                var product = cart.Items.Where(d => d.ShopBranchId == item.Id).OrderBy(s => s.Status).ThenByDescending(o => o.AddTime).ToList();
                var _store = new CartStoreModel();
                _store.ShopBranchId = item.Id;
                _store.ShopId = item.ShopId;
                _store.ShopBranchName = item.ShopBranchName;
                _store.Status = item.Status.GetHashCode();
                _store.DeliveFee = item.DeliveFee;
                _store.DeliveTotalFee = item.DeliveTotalFee;
                _store.FreeMailFee = item.FreeMailFee;
                _store.Products = new List<CartStoreProduct>();
                foreach (var pitem in product)
                {
                    var pro = ProductManagerApplication.GetProduct(pitem.ProductId);
                    var shopbranchsku = ShopBranchApplication.GetSkusByIds(_store.ShopBranchId, new List<string> { pitem.SkuId }).FirstOrDefault();
                    var shop = ShopApplication.GetShop(pro.ShopId);
                    var vshop = VshopApplication.GetVShopByShopId(pro.ShopId);
                    DTO.SKU sku = ProductManagerApplication.GetSKU(pitem.SkuId);
                    string skuDetails = "";
                    if (null != shop && sku != null)
                    {
                        prodPrice = sku.SalePrice;
                        if (shop.IsSelf)
                        {
                            //官方自营店才计算会员折扣
                            prodPrice = sku.SalePrice * discount;
                        }
                        prodPrice = decimal.Round(prodPrice, 2, MidpointRounding.AwayFromZero);

                        var typeInfo = TypeApplication.GetType(pro.TypeId);
                        skuDetails = "";
                        if (!string.IsNullOrWhiteSpace(sku.Size))
                        {
                            if (!string.IsNullOrWhiteSpace(skuDetails))
                            {
                                skuDetails += "、";
                            }
                            skuDetails += sku.Size;
                        }
                        if (!string.IsNullOrWhiteSpace(sku.Color))
                        {
                            if (!string.IsNullOrWhiteSpace(skuDetails))
                            {
                                skuDetails += "、";
                            }
                            skuDetails += sku.Color;
                        }
                        if (!string.IsNullOrWhiteSpace(sku.Version))
                        {
                            if (!string.IsNullOrWhiteSpace(skuDetails))
                            {
                                skuDetails += "、";
                            }
                            skuDetails += sku.Version;
                        }
                        string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                        string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                        string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

                        var _product = new CartStoreProduct
                        {
                            ShopBranchId = shopBranchId,
                            CartItemId = pitem.Id,
                            SkuId = pitem.SkuId,
                            Id = pro.Id,
                            ProductName = pro.ProductName,
                            Price = prodPrice,
                            Count = pitem.Quantity,
                            Stock = shopbranchsku == null ? 0 : shopbranchsku.Stock,
                            //阶梯价商品在门店购物车自动下架
                            Status = pro.IsOpenLadder ? 1 : (shopbranchsku == null ? 1 : (shopbranchsku.Status == ShopBranchSkuStatus.Normal) ? (pitem.Quantity > shopbranchsku.Stock ? 2 : 0) : 1),//0:正常；1：冻结；2：库存不足
                            SkuDetails = skuDetails,
                            ColorAlias = colorAlias,
                            SizeAlias = sizeAlias,
                            VersionAlias = versionAlias,
                            Size = sku.Size,
                            Color = sku.Color,
                            Version = sku.Version,
                            AddTime = pitem.AddTime,
                            DefaultImage = HimallIO.GetRomoteProductSizeImage(pro.ImagePath, 1, 500)
                        };
                        _store.Products.Add(_product);
                    }
                }
                _store.Amount = _store.Products.Where(x => x.Status == 0).Sum(s => s.Price * s.Count);
                _store.TotalCount = _store.Products.Where(x => x.Status == 0).Sum(s => s.Count);
                if (_store.Products.Count > 0)
                {//有商品数据，才返回门店信息
                    rets.Add(_store);
                }
            }
            return JsonResult(rets);
        }

        /// <summary>
        /// 清理门店购物车所有无效商品
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public JsonResult<Result<int>> GetClearBranchCartProducts(long shopBranchId)
        {
            CheckUserLogin();
            long userId = CurrentUser.Id;
            CartApplication.ClearShopBranchCart(userId, shopBranchId);
            return JsonResult<int>();
        }

        /// <summary>
        /// 清理门店购物车所有无效商品
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public JsonResult<Result<int>> GetClearBranchCartInvalidProducts(long shopBranchId)
        {
            long userId = CurrentUser != null ? CurrentUser.Id : 0;
            CartApplication.ClearInvalidShopBranchCart(userId, shopBranchId);
            return JsonResult<int>();
        }
    }
}
