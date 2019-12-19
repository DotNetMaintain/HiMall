using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Himall.IServices;
using Himall.Model;

namespace Himall.Application
{
    public class CartApplication
    {
        private static ICartService _iCartService = ObjectContainer.Current.Resolve<ICartService>();
        private static IBranchCartService _iBranchCartService = ObjectContainer.Current.Resolve<IBranchCartService>();
        /// <summary>
        /// 添加商品至购物车
        /// </summary>
        /// <param name="skuId">商品SKU Id</param>
        /// <param name="count">商品数量</param>
        /// <param name="memberId">会员id</param>
        public static void AddToCart(string skuId, int count, long memberId)
        {
            _iCartService.AddToCart(skuId, count, memberId);
        }

        /// <summary>
        /// 添加商品至购物车
        /// </summary>
        /// <param name="cartItems">购物车商品项</param>
        /// <param name="memberId">会员Id</param>
        public static void AddToCart(IEnumerable<ShoppingCartItem> cartItems, long memberId)
        {
            _iCartService.AddToCart(cartItems, memberId);
        }

        /// <summary>
        /// 更新购物车
        /// </summary>
        /// <param name="skuId">商品SKU Id</param>
        /// <param name="count">商品数量</param>
        /// <param name="memberId">会员id</param>
        public static void UpdateCart(string skuId, int count, long memberId)
        {
            _iCartService.UpdateCart(skuId, count, memberId);
        }

        /// <summary>
        /// 清空指定会员的购物车
        /// </summary>
        /// <param name="memeberId">会员id</param>
        public static void ClearCart(long memeberId)
        {
            _iCartService.ClearCart(memeberId);
        }

        /// <summary>
        /// 删除指定会员购物车中的指定商品
        /// </summary>
        /// <param name="skuId">待删除的商品的skuid</param>
        /// <param name="memberId">会员id</param>
        public static void DeleteCartItem(string skuId, long memberId)
        {
            _iCartService.DeleteCartItem(skuId, memberId);
        }


        /// <summary>
        /// 删除指定会员购物车中的指定商品
        /// </summary>
        /// <param name="skuIds">待删除的商品的skuid</param>
        /// <param name="memberId">会员id</param>
        public static void DeleteCartItem(IEnumerable<string> skuIds, long memberId)
        {
            _iCartService.DeleteCartItem(skuIds, memberId);
        }

        /// <summary>
        /// 获取指定会员购物车信息
        /// </summary>
        /// <param name="memeberId">会员id</param>
        /// <returns></returns>
        public static ShoppingCartInfo GetCart(long memeberId)
        {
            return _iCartService.GetCart(memeberId);
        }
        /// <summary>
        /// 获取购物车购物项
        /// </summary>
        /// <param name="cartItemIds">购物车项Id</param>
        /// <returns></returns>
        public static IQueryable<ShoppingCartItem> GetCartItems(IEnumerable<long> cartItemIds)
        {
            return _iCartService.GetCartItems(cartItemIds);
        }

        /// <summary>
        /// 获取购物车购物项
        /// </summary>
        /// <param name="skuIds">SKUId</param>
        /// <returns></returns>
        public static IQueryable<ShoppingCartItem> GetCartItems(IEnumerable<string> skuIds, long memberId)
        {
            return _iCartService.GetCartItems(skuIds, memberId);
        }

        public static List<ShoppingCartItem> GetCartQuantityByIds(long memberId, IEnumerable<long> productIds)
        {
            var shopcart = _iCartService.GetCartQuantityByIds(memberId, productIds).ToList();
            return AutoMapper.Mapper.Map<List<ShoppingCartItem>>(shopcart);
        }


        #region 门店购物车
        /// <summary>
        /// 更新购物车
        /// </summary>
        /// <param name="skuId">商品SKU Id</param>
        /// <param name="count">商品数量</param>
        /// <param name="memberId">会员id</param>
        /// <param name="shopbranchId">门店编号</param>
        public static void UpdateShopBranchCart(string skuId, int count, long memberId, long shopbranchId)
        {
            _iBranchCartService.UpdateCart(skuId, count, memberId, shopbranchId);
        }
        /// <summary>
        /// 取会员门店购物车项
        /// </summary>
        /// <param name="memeberId"></param>
        /// <param name="shopbranchId">门店编号,0取所有</param>
        /// <returns></returns>
        public static ShoppingCartInfo GetShopBranchCart(long memberId, long shopbranchId = 0)
        {
            return _iBranchCartService.GetCart(memberId, shopbranchId);
        }
        /// <summary>
        /// 取会员门店购物车项(无缓存)
        /// </summary>
        /// <param name="memeberId"></param>
        /// <param name="shopbranchId">门店编号,0取所有</param>
        /// <returns></returns>
        public static ShoppingCartInfo GetShopBranchCartNoCache(long memberId, long shopbranchId = 0)
        {
            return _iBranchCartService.GetCartNoCache(memberId, shopbranchId);
        }
        /// <summary>
        /// 删除门店购物车某项
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="memberId"></param>
        /// <param name="shopbranchId"></param>
        public static void DeleteShopBranchCartItem(string skuId, long memberId, long shopbranchId)
        {
            _iBranchCartService.DeleteCartItem(skuId, memberId, shopbranchId);
        }
        /// <summary>
        /// 删除门店购物车多项
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="memberId"></param>
        /// <param name="shopbranchId"></param>
        public static void DeleteShopBranchCartItem(IEnumerable<string> skuIds, long memberId, long shopbranchId)
        {
            _iBranchCartService.DeleteCartItem(skuIds, memberId, shopbranchId);
        }
        /// <summary>
        /// 清理门店购物车
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="shopbranchId"></param>
        public static void ClearShopBranchCart(long memberId, long shopbranchId)
        {
            _iBranchCartService.ClearCart(memberId, shopbranchId);
        }
        /// <summary>
        /// 清理门店购物车
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="shopbranchId"></param>
        public static void ClearInvalidShopBranchCart(long memberId, long shopbranchId)
        {
            _iBranchCartService.ClearInvalidCart(memberId, shopbranchId);
        }
        /// <summary>
        /// 添加商品至购物车
        /// </summary>
        /// <param name="cartItems">购物车商品项</param>
        /// <param name="memberId">会员Id</param>
        public static void AddToShopBranchCart(IEnumerable<ShoppingCartItem> cartItems, long memberId)
        {
            _iBranchCartService.AddToCart(cartItems, memberId);
        }
        /// <summary>
        /// 添加商品至购物车
        /// </summary>
        /// <param name="cartItems">购物车商品项</param>
        /// <param name="memberId">会员Id</param>
        public static void AddToShopBranchCart(string skuId, int count, long memberId, long shopbranchId)
        {
            _iBranchCartService.AddToCart(skuId, count, memberId, shopbranchId);
        }
        #endregion
    }
}
