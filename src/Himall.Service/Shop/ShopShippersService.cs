using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.DTO;
using Himall.Entity;
using Himall.IServices;
using Himall.DTO.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Himall.Service
{
    public class ShopShippersService : ServiceBase, IShopShippersServiceService
    {
        /// <summary>
        /// 获得默认发货地址信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public ShopShippersInfo GetDefaultSendGoodsShipper(long shopId)
        {
            return Context.ShopShippersInfo.FirstOrDefault(d => d.ShopId == shopId && d.IsDefaultSendGoods == true);
        }
        /// <summary>
        /// 获得默认收货地址信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public ShopShippersInfo GetDefaultGetGoodsShipper(long shopId)
        {
            return Context.ShopShippersInfo.FirstOrDefault(d => d.ShopId == shopId && d.IsDefaultGetGoods == true);
        }
        /// <summary>
        /// 设置默认发货地址信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="id"></param>
        public void SetDefaultSendGoodsShipper(long shopId, long id)
        {
            var datalist = Context.ShopShippersInfo.Where(d => d.ShopId == shopId && d.IsDefaultSendGoods == true).ToList();
            var data = Context.ShopShippersInfo.FirstOrDefault(d => d.ShopId == shopId && d.Id == id);
            if (data == null)
            {
                throw new HimallException("错误的参数");
            }
            foreach (var item in datalist)
            {
                item.IsDefaultSendGoods = false;
            }
            data.IsDefaultSendGoods = true;
            Context.SaveChanges();
        }
        /// <summary>
        /// 设置默认收货地址信息
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="id"></param>
        public void SetDefaultGetGoodsShipper(long shopId, long id)
        {
            var datalist = Context.ShopShippersInfo.Where(d => d.ShopId == shopId && d.IsDefaultGetGoods == true).ToList();
            var data = Context.ShopShippersInfo.FirstOrDefault(d => d.ShopId == shopId && d.Id == id);
            if (data == null)
            {
                throw new HimallException("错误的参数");
            }
            foreach (var item in datalist)
            {
                item.IsDefaultGetGoods = false;
            }
            data.IsDefaultGetGoods = true;
            Context.SaveChanges();

        }

        /// <summary>
        /// 获取所有发收货地址
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public List<ShopShippersInfo> GetShopShippers(long shopId)
        {
            return Context.ShopShippersInfo.Where(d => d.ShopId == shopId).ToList();
        }

        /// <summary>
        /// 添加发收货地址
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="data"></param>
        public void Add(long shopId, ShopShippersInfo data)
        {
            if (Context.ShopShippersInfo.Count(d => d.ShopId == shopId) == 0)
            {
                data.IsDefaultGetGoods = true;
                data.IsDefaultSendGoods = true;
            }
            data.ShopId = shopId;
            Context.ShopShippersInfo.Add(data);
            Context.SaveChanges();
        }

        /// <summary>
        /// 修改发收货地址
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="data"></param>
        public void Update(long shopId, ShopShippersInfo data)
        {
            var _d = Context.ShopShippersInfo.FirstOrDefault(d => d.Id == data.Id && d.ShopId == shopId);
            if (_d == null)
            {
                throw new HimallException("错误的参数");
            }
            _d.ShipperTag = data.ShipperTag;
            _d.ShipperName = data.ShipperName;
            _d.TelPhone = data.TelPhone;
            _d.IsDefaultGetGoods = data.IsDefaultGetGoods;
            _d.IsDefaultSendGoods = data.IsDefaultSendGoods;
            _d.Latitude = data.Latitude;
            _d.Longitude = data.Longitude;
            _d.RegionId = data.RegionId;
            _d.Address = data.Address;
            _d.ShopId = shopId;
            _d.WxOpenId = data.WxOpenId;
            _d.Zipcode = data.Zipcode;
            Context.SaveChanges();
        }

        /// <summary>
        /// 删除发收货地址
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="id"></param>
        public void Delete(long shopId, long id)
        {
            var _d = Context.ShopShippersInfo.FirstOrDefault(d => d.Id == id && d.ShopId == shopId);
            if (_d == null)
            {
                throw new HimallException("错误的参数");
            }
            if (_d.IsDefaultGetGoods || _d.IsDefaultSendGoods)
            {
                throw new HimallException("不能删除默认的发货/收货信息");
            }
            Context.ShopShippersInfo.Remove(_d);
            Context.SaveChanges();
        }

        public ShopShippersInfo GetShopShipper(long shopId, long id)
        {
            return Context.ShopShippersInfo.FirstOrDefault(d => d.ShopId == shopId && d.Id == id);
        }
    }
}
