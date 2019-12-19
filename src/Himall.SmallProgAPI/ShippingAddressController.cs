using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.IServices;
using Himall.Model;
using Himall.SmallProgAPI.Model;
using Himall.Web.Framework;
using System;
using System.Dynamic;
using System.Linq;
using System.Web.Http.Results;

namespace Himall.SmallProgAPI
{
    /// <summary>
    /// 收货地址
    /// </summary>
    public class ShippingAddressController : BaseApiController
    {
        #region 获取
        /// <summary>
        /// 获取收货地址列表
        /// </summary>
        /// <param name="openId"></param>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetList(string openId)
        {
            CheckUserLogin();
            var shoppingAddress = ServiceProvider.Instance<IShippingAddressService>.Create.GetUserShippingAddressByUserId(CurrentUser.Id);

            var result = shoppingAddress.ToList().Select(item => new
            {
                ShippingId = item.Id,
                ShipTo = item.ShipTo,
                CellPhone = item.Phone,
                FullRegionName = item.RegionFullName,
                Address = item.Address,
                RegionId = item.RegionId,
                FullRegionPath = item.RegionIdPath,
                IsDefault = item.IsDefault,
                LatLng = item.Latitude + "," + item.Longitude,
                FullAddress = item.RegionFullName + " " + item.Address + " " + item.AddressDetail,
                TelPhone = "",
                RegionLocation = "",
                Latitude = item.Latitude,
                Longitude = item.Longitude,
                AddressDetail = item.AddressDetail ?? string.Empty,
                NeedUpdate = item.NeedUpdate
            });
            return JsonResult<dynamic>(result);

        }
        /// <summary>
        /// 获取收货地址
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetShippingAddress(long shippingId)
        {
            CheckUserLogin();
            var shoppingAddress = ServiceProvider.Instance<IShippingAddressService>.Create.GetUserShippingAddressByUserId(CurrentUser.Id);
            var shopaddressInfo = shoppingAddress.FirstOrDefault(e => e.Id == shippingId);
            if (shopaddressInfo != null)
            {
                dynamic model = new ExpandoObject();

                model.ShippingId = shopaddressInfo.Id;
                model.ShipTo = shopaddressInfo.ShipTo;
                model.CellPhone = shopaddressInfo.Phone;
                model.FullRegionName = shopaddressInfo.RegionFullName;
                model.Address = shopaddressInfo.Address;
                model.RegionId = shopaddressInfo.RegionId;
                model.FullRegionPath = shopaddressInfo.RegionIdPath;
                model.IsDefault = shopaddressInfo.IsDefault;
                model.LatLng = shopaddressInfo.Latitude + "," + shopaddressInfo.Longitude;
                model.FullAddress = shopaddressInfo.RegionFullName + " " + shopaddressInfo.Address;
                model.TelPhone = "";
                model.RegionLocation = "";
                model.UserId = CurrentUserId;
                model.Zipcode = "";
                model.AddressDetail = shopaddressInfo.AddressDetail;
                return JsonResult<dynamic>(model);
            }
            else
            {
                return Json(ErrorResult<dynamic>("参数错误"));
            }

        }
        #endregion

        /// <summary>
        /// 新增收货地址
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public JsonResult<Result<long>> PostAddAddress(ShippingAddressOperaAddressPModel value)
        {
            CheckUserLogin();
            ShippingAddressInfo shippingAddr = new ShippingAddressInfo();
            shippingAddr.UserId = CurrentUser.Id;
            shippingAddr.RegionId = value.regionId;
            shippingAddr.Address = value.address;
            shippingAddr.Phone = value.cellphone;
            shippingAddr.ShipTo = value.shipTo;
            shippingAddr.Latitude = value.lat;
            shippingAddr.Longitude = value.lng;
            shippingAddr.AddressDetail = value.addressDetail;
            try
            {
                ServiceProvider.Instance<IShippingAddressService>.Create.AddShippingAddress(shippingAddr);
                if (value.isDefault)
                {
                    ServiceProvider.Instance<IShippingAddressService>.Create.SetDefaultShippingAddress(shippingAddr.Id, CurrentUserId);
                }
            }
            catch (Exception ex)
            {
                return Json(ErrorResult<long>(ex.Message));
            }
            return JsonResult<long>(shippingAddr.Id);
        }
        /// <summary>
        /// 新增收货地址(微信地址)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public JsonResult<Result<long>> PostAddWXAddress(ShippingAddressOperaAddressPModel value)
        {
            CheckUserLogin();
            try
            {
                if (string.IsNullOrWhiteSpace(value.address))
                {
                    throw new HimallException("请填写详细地址");
                }
                if (value.regionId <= 0 && (string.IsNullOrWhiteSpace(value.city) || string.IsNullOrWhiteSpace(value.county)))
                {
                    throw new HimallException("参数错误");
                }
                if (value.regionId <= 0)
                {
                    var _region = ServiceProvider.Instance<IRegionService>.Create.GetRegionByName(value.county, Region.RegionLevel.County);
                    if (_region != null)
                    {
                        value.regionId = _region.Id;
                    }
                }
                if (value.regionId <= 0)
                {
                    throw new HimallException("错误的地区信息");
                }
            }
            catch (Exception ex)
            {
                return Json(ErrorResult<long>(ex.Message));
            }

            #region 如果存在相同地址就返回
            var shoppingAddress = ServiceProvider.Instance<IShippingAddressService>.Create.GetUserShippingAddressByUserId(CurrentUser.Id);
            var _tmp = shoppingAddress.FirstOrDefault(d => d.RegionId == value.regionId && d.Address == value.address);
            if (_tmp != null)
            {
                return JsonResult(_tmp.Id);
            }
            #endregion

            ShippingAddressInfo shippingAddr = new ShippingAddressInfo();
            shippingAddr.UserId = CurrentUser.Id;
            shippingAddr.RegionId = value.regionId;
            shippingAddr.Address = value.address;
            shippingAddr.Phone = value.cellphone;
            shippingAddr.ShipTo = value.shipTo;
            shippingAddr.Latitude = value.lat;
            shippingAddr.Longitude = value.lng;
            shippingAddr.AddressDetail = value.addressDetail;
            try
            {
                ServiceProvider.Instance<IShippingAddressService>.Create.AddShippingAddress(shippingAddr);
                if (value.isDefault)
                {
                    ServiceProvider.Instance<IShippingAddressService>.Create.SetDefaultShippingAddress(shippingAddr.Id, CurrentUserId);
                }
            }
            catch (Exception ex)
            {
                return Json(ErrorResult<long>(ex.Message));
            }
            return JsonResult(shippingAddr.Id);
        }
        /// <summary>
        /// 修改收货地址
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public JsonResult<Result<long>> PostUpdateAddress(ShippingAddressOperaAddressPModel value)
        {
            CheckUserLogin();
            ShippingAddressInfo shippingAddr = new ShippingAddressInfo();
            shippingAddr.UserId = CurrentUser.Id;
            shippingAddr.Id = value.shippingId;
            shippingAddr.RegionId = value.regionId;
            shippingAddr.Address = value.address;
            shippingAddr.Phone = value.cellphone;
            shippingAddr.ShipTo = value.shipTo;
            shippingAddr.Latitude = value.lat;
            shippingAddr.Longitude = value.lng;
            shippingAddr.AddressDetail = value.addressDetail;
            try
            {
                ServiceProvider.Instance<IShippingAddressService>.Create.UpdateShippingAddress(shippingAddr);
                if (value.isDefault)
                {
                    ServiceProvider.Instance<IShippingAddressService>.Create.SetDefaultShippingAddress(shippingAddr.Id, CurrentUserId);
                }
            }
            catch (Exception ex)
            {
                return Json(ErrorResult<long>(ex.Message));
            }
            return JsonResult(shippingAddr.Id);
        }
        /// <summary>
        /// 设置默认地址
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public JsonResult<Result<int>> GetSetDefault(long shippingId)
        {
            CheckUserLogin();
            ServiceProvider.Instance<IShippingAddressService>.Create.SetDefaultShippingAddress(shippingId, CurrentUserId);
            return JsonResult<int>(msg: "设置成功");
        }
        /// <summary>
        /// 删除收货地址
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public JsonResult<Result<int>> GetDeleteAddress(long shippingId)
        {
            CheckUserLogin();
            ServiceProvider.Instance<IShippingAddressService>.Create.DeleteShippingAddress(shippingId, CurrentUser.Id);
            return JsonResult<int>(msg: "删除成功");
        }

        /// <summary>
        /// 根据搜索地址反向匹配出区域信息
        /// </summary>
        /// <param name="fromLatLng"></param>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetRegion(string fromLatLng = "")
        {
            CheckUserLogin();
            string address = string.Empty, province = string.Empty, city = string.Empty, district = string.Empty, street = string.Empty, fullPath = string.Empty, newStreet = string.Empty;
            ShopbranchHelper.GetAddressByLatLng(fromLatLng, ref address, ref province, ref city, ref district, ref street);
            if (district == "" && street != "")
            {
                district = street;
                street = "";
            }
            fullPath = RegionApplication.GetAddress_Components(city, district, street, out newStreet);
            if (fullPath.Split(',').Length <= 3) newStreet = string.Empty;//如果无法匹配街道，则置为空
            return JsonResult<dynamic>(new { fullPath = fullPath, showCity = string.Format("{0} {1} {2} {3}", province, city, district, newStreet), street = newStreet });
        }
    }
}
