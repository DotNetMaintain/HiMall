using Himall.CommonModel;
using Himall.Core;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Himall.Service
{
    public class BranchCartService : ServiceBase, IBranchCartService
    {
        public void AddToCart(string skuId, int count, long memberId, long shopbranchId)
        {
            if (count != 0)
            {
                CheckCartItem(skuId, count, memberId, shopbranchId);
                var cartItem = Context.ShoppingCartItemInfo.FirstOrDefault(item => item.UserId == memberId && item.SkuId == skuId && item.ShopBranchId.HasValue && item.ShopBranchId.Value == shopbranchId);
                if (cartItem != null)//首先查询，如果已经存在则直接更新，否则新建
                {
                    cartItem.Quantity += count;//否则更新数量
                }
                else if (count > 0)
                {
                    long productId = long.Parse(skuId.Split('_')[0]);//SKU第一节为商品Id
                    Context.ShoppingCartItemInfo.Add(new ShoppingCartItemInfo() { UserId = memberId, Quantity = count, SkuId = skuId, ProductId = productId, AddTime = DateTime.Now, ShopBranchId = shopbranchId });
                }
                Context.SaveChanges();
                Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memberId));
            }
        }

        public void AddToCart(IEnumerable<ShoppingCartItem> cartItems, long memberId)
        {
            foreach (var cartItem in cartItems.ToList())
            {
                if (cartItem.ShopBranchId.HasValue)
                {
                    CheckCartItem(cartItem.SkuId, cartItem.Quantity, memberId, cartItem.ShopBranchId.Value);
                    var oriCartItem = Context.ShoppingCartItemInfo.FirstOrDefault(item => item.UserId == memberId && item.SkuId == cartItem.SkuId && item.ShopBranchId.HasValue && item.ShopBranchId.Value == cartItem.ShopBranchId.Value);
                    if (oriCartItem != null)//首先查询，如果已经存在则直接更新，否则新建
                        oriCartItem.Quantity += cartItem.Quantity;
                    else
                    {
                        long productId = long.Parse(cartItem.SkuId.Split('_')[0]);//SKU第一节为商品Id
                        Context.ShoppingCartItemInfo.Add(new ShoppingCartItemInfo() { UserId = memberId, Quantity = cartItem.Quantity, SkuId = cartItem.SkuId, ProductId = productId, AddTime = DateTime.Now, ShopBranchId = cartItem.ShopBranchId.Value });
                    }
                }
            }
            Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memberId));
        }

        public void UpdateCart(string skuId, int count, long memberId, long shopbranchId)
        {
            CheckCartItem(skuId, count, memberId, shopbranchId);
            var cartItem = Context.ShoppingCartItemInfo.FirstOrDefault(item => item.UserId == memberId && item.SkuId == skuId && item.ShopBranchId.HasValue && item.ShopBranchId.Value == shopbranchId);
            if (cartItem != null)//首先查询，如果已经存在则直接更新，否则新建
            {
                if (count == 0)//数量为0时，删除对应项
                    Context.ShoppingCartItemInfo.Remove(n => n.Id == cartItem.Id);
                else
                    cartItem.Quantity = count;//否则更新数量
            }
            else if (count > 0)
            {
                long productId = long.Parse(skuId.Split('_')[0]);//SKU第一节为商品Id
                Context.ShoppingCartItemInfo.Add(new ShoppingCartItemInfo() { UserId = memberId, Quantity = count, SkuId = skuId, ProductId = productId, AddTime = DateTime.Now, ShopBranchId = shopbranchId });
            }
            Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memberId));
        }

        public void ClearCart(long memeberId, long shopbranchId)
        {
            Context.ShoppingCartItemInfo.Remove(item => item.UserId == memeberId && item.ShopBranchId.HasValue && item.ShopBranchId.Value == shopbranchId);
            Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memeberId));
        }

        public void ClearInvalidCart(long memeberId, long shopbranchId)
        {
            //清理状态不对
            var skusql = Context.ShopBranchSkusInfo.Where(d => d.Status == CommonModel.ShopBranchSkuStatus.Normal && d.ShopBranchId == shopbranchId).Select(d => d.SkuId);
            var sql = Context.ShoppingCartItemInfo.Where(item => item.UserId == memeberId && item.ShopBranchId.HasValue && item.ShopBranchId.Value == shopbranchId);
            var cleardata = sql.Where(d => !skusql.Contains(d.SkuId)).ToList();
            Context.ShoppingCartItemInfo.RemoveRange(cleardata);
            //清理库存不足
            var delsql = (from sku in Context.ShopBranchSkusInfo
                          from c in Context.ShoppingCartItemInfo
                          where c.Quantity > sku.Stock && c.UserId == memeberId && c.ShopBranchId == shopbranchId
                          && c.ShopBranchId == sku.ShopBranchId && c.SkuId == sku.SkuId
                          select c);
            var delnumdata = delsql.ToList();
            Context.ShoppingCartItemInfo.RemoveRange(delnumdata);
            Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memeberId));
        }

        public void DeleteCartItem(string skuId, long memberId, long shopbranchId)
        {
            Context.ShoppingCartItemInfo.Remove(item => item.SkuId == skuId && item.UserId == memberId && item.ShopBranchId.HasValue && item.ShopBranchId.Value == shopbranchId);
            Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memberId));
        }


        public void DeleteCartItem(IEnumerable<string> skuIds, long memberId, long shopbranchId)
        {
            Context.ShoppingCartItemInfo.Remove(item => skuIds.Contains(item.SkuId) && item.UserId == memberId && item.ShopBranchId.HasValue && item.ShopBranchId.Value == shopbranchId);
            Context.SaveChanges();
            Cache.Remove(CacheKeyCollection.CACHE_BRANCHCART(memberId));
        }

        public ShoppingCartInfo GetCart(long memeberId, long shopbranchId)
        {
            ShoppingCartInfo shoppingCartInfo = new ShoppingCartInfo() { MemberId = memeberId };
            if (Cache.Exists(CacheKeyCollection.CACHE_BRANCHCART(memeberId)))
            {
                shoppingCartInfo = Cache.Get<ShoppingCartInfo>(CacheKeyCollection.CACHE_BRANCHCART(memeberId));
            }
            else
            {
                var cartItems = Context.ShoppingCartItemInfo.Where(item => item.UserId == memeberId && item.ShopBranchId.HasValue);
                shoppingCartInfo.Items = cartItems.Select(item => new ShoppingCartItem()
                {
                    Id = item.Id,
                    SkuId = item.SkuId,
                    Quantity = item.Quantity,
                    AddTime = item.AddTime,
                    ProductId = item.ProductId,
                    ShopBranchId = item.ShopBranchId
                }).ToList();
                Cache.Insert<ShoppingCartInfo>(CacheKeyCollection.CACHE_BRANCHCART(memeberId), shoppingCartInfo, 600);
            }
            if (shopbranchId > 0)
            {
                return new ShoppingCartInfo() { MemberId = memeberId, Items = shoppingCartInfo.Items.Where(x => x.ShopBranchId == shopbranchId) };
            }
            return shoppingCartInfo;
        }

        public ShoppingCartInfo GetCartNoCache(long memeberId, long shopbranchId)
        {
            ShoppingCartInfo shoppingCartInfo = new ShoppingCartInfo() { MemberId = memeberId };
            var cartItems = Context.ShoppingCartItemInfo.Where(item => item.UserId == memeberId && item.ShopBranchId.HasValue && (item.ShopBranchId.Value == shopbranchId || shopbranchId == 0));
            shoppingCartInfo.Items = cartItems.Select(item => new ShoppingCartItem()
            {
                Id = item.Id,
                SkuId = item.SkuId,
                Quantity = item.Quantity,
                AddTime = item.AddTime,
                ProductId = item.ProductId,
                ShopBranchId = item.ShopBranchId
            });
            return shoppingCartInfo;
        }

        void CheckCartItem(string skuId, int count, long memberId, long shopbranchId)
        {
            if (string.IsNullOrWhiteSpace(skuId))
                throw new InvalidPropertyException("SKUId不能为空");
            else if (count < 0)
                throw new InvalidPropertyException("商品数量不能小于0");
            else if (shopbranchId <= 0)
                throw new InvalidPropertyException("门店ID不能为空");
            else
            {
                var member = Context.UserMemberInfo.FirstOrDefault(item => item.Id == memberId);
                if (member == null)
                    throw new InvalidPropertyException("会员Id" + memberId + "不存在");
            }
        }

        public IQueryable<ShoppingCartItem> GetCartItems(IEnumerable<long> cartItemIds, long shopbranchId)
        {
            var shoppingCartItems = Context.ShoppingCartItemInfo
                .FindBy(item => cartItemIds.Contains(item.Id))
                .Select(item => new ShoppingCartItem()
                {
                    Id = item.Id,
                    SkuId = item.SkuId,
                    Quantity = item.Quantity,
                    ProductId = item.ProductId,
                    AddTime = item.AddTime,
                    ShopBranchId = item.ShopBranchId
                });
            return shoppingCartItems;
        }


        public IQueryable<ShoppingCartItem> GetCartItems(IEnumerable<string> skuIds, long memberId, long shopbranchId)
        {
            return Context.ShoppingCartItemInfo.Where(item => item.UserId == memberId && skuIds.Contains(item.SkuId) && item.ShopBranchId.HasValue && item.ShopBranchId.Value == shopbranchId)
                                               .Select(item => new ShoppingCartItem()
                                               {
                                                   Id = item.Id,
                                                   SkuId = item.SkuId,
                                                   Quantity = item.Quantity,
                                                   ProductId = item.ProductId,
                                                   AddTime = item.AddTime,
                                                   ShopBranchId = item.ShopBranchId
                                               });
        }
        /// <summary>
        /// 获取购物车对应商品数量
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="skuId"></param>
        /// <returns></returns>
        public int GetCartProductQuantity(long memberId, long shopbranchId, long productId = 0, string skuId = "")
        {
            int cartQuantity = 0;
            if (productId > 0)
            {
                var shopInfo = Context.ShoppingCartItemInfo.Where(p => p.UserId == memberId && p.ProductId == productId && p.ShopBranchId.HasValue && p.ShopBranchId.Value == shopbranchId);
                if (shopInfo != null)
                {
                    cartQuantity += shopInfo.Sum(d => d.Quantity);
                }
            }
            else
            {
                var shopInfo = Context.ShoppingCartItemInfo.Where(p => p.UserId == memberId && p.SkuId == skuId && p.ShopBranchId.HasValue && p.ShopBranchId.Value == shopbranchId);
                if (shopInfo != null)
                {
                    cartQuantity += shopInfo.Sum(d => d.Quantity);
                }
            }
            return cartQuantity;
        }

        public IQueryable<ShoppingCartItem> GetCartQuantityByIds(long memberId, IEnumerable<long> productIds, long shopbranchId)
        {
            return Context.ShoppingCartItemInfo.Where(item => item.UserId == memberId && productIds.Contains(item.ProductId) && item.ShopBranchId.HasValue && item.ShopBranchId.Value == shopbranchId)
                .Select(item => new ShoppingCartItem()
                {
                    Id = item.Id,
                    SkuId = item.SkuId,
                    Quantity = item.Quantity,
                    ProductId = item.ProductId,
                    AddTime = item.AddTime,
                    ShopBranchId = item.ShopBranchId
                });
        }
    }
}
