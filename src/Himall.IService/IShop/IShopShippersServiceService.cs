using Himall.DTO.QueryModel;
using Himall.Model;
using System.Linq;
using System;
using System.Collections.Generic;
using Himall.CommonModel;
using Himall.DTO;

namespace Himall.IServices
{
    public interface IShopShippersServiceService : IService
    {
        /// <summary>
        /// 获得默认发货地址信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        ShopShippersInfo GetDefaultSendGoodsShipper(long shopId);
        /// <summary>
        /// 获得默认收货地址信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        ShopShippersInfo GetDefaultGetGoodsShipper(long shopId);
        /// <summary>
        /// 设置默认发货地址信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="id"></param>
        void SetDefaultSendGoodsShipper(long shopId, long id);
        /// <summary>
        /// 设置默认收货地址信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="id"></param>
        void SetDefaultGetGoodsShipper(long shopId, long id);
        /// <summary>
        /// 获取收货地址
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        ShopShippersInfo GetShopShipper(long shopId,long id);
        /// <summary>
        /// 获取所有发收货地址
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        List<ShopShippersInfo> GetShopShippers(long shopId);
        /// <summary>
        /// 添加发收货地址
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="data"></param>
        void Add(long shopId, ShopShippersInfo data);
        /// <summary>
        /// 修改发收货地址
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="data"></param>
        void Update(long shopId, ShopShippersInfo data);
        /// <summary>
        /// 删除发收货地址
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="id"></param>
        void Delete(long shopId, long id);
    }
}
