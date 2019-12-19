using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Framework;
using Himall.Application;
using Himall.DTO;
using Himall.Core;
using Himall.CommonModel;
using Himall.Web.Models;
using Himall.IServices;
using Himall.Model;
using System.Drawing;
using System.IO;
using Himall.DTO.QueryModel;
using Himall.Web.Areas.SellerAdmin.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    [StoreAuthorization]
    public class ShopBranchController : BaseSellerController
    {
        private const string DADA_STORE_PREFIX = "st_";

        // GET: SellerAdmin/ShopBranch
        public ActionResult Add()
        {
            //门店标签
            var shopBranchTagInfos = ShopBranchApplication.GetAllShopBranchTagInfos();
            List<SelectListItem> tagList = new List<SelectListItem>();
            foreach (var item in shopBranchTagInfos)
            {
                tagList.Add(new SelectListItem
                {
                    Selected = false,
                    Value = item.Id.ToString(),
                    Text = item.Title
                });
            }
            ViewBag.ShopBranchTags = tagList;
            return View(new ShopBranch { IsStoreDelive = true, ServeRadius = 0, DeliveFee = 0, DeliveTotalFee = 0, FreeMailFee = 0 });
        }
        [HttpPost]
        public ActionResult Add(ShopBranch shopBranch)
        {
            try
            {
                if (!string.Equals(shopBranch.PasswordOne, shopBranch.PasswordTwo))
                {
                    throw new HimallException("两次密码输入不一致！");
                }
                if (string.IsNullOrWhiteSpace(shopBranch.PasswordOne) || string.IsNullOrWhiteSpace(shopBranch.PasswordTwo))
                {
                    throw new HimallException("密码不能为空！");
                }
                if (shopBranch.ShopBranchName.Length > 15)
                {
                    throw new HimallException("门店名称不能超过15个字！");
                }
                if (shopBranch.AddressDetail.Length > 50)
                {
                    throw new HimallException("详细地址不能超过50个字！");
                }
                if (shopBranch.Latitude <= 0 || shopBranch.Longitude <= 0)
                {
                    throw new HimallException("请搜索地址地图定位！");
                }
                if (!shopBranch.IsAboveSelf && !shopBranch.IsStoreDelive)
                {
                    throw new HimallException("至少需要选择一种配送方式！");
                }

                shopBranch.ShopId = CurrentSellerManager.ShopId;
                shopBranch.CreateDate = DateTime.Now;
                long shopBranchId;
                ShopBranchApplication.AddShopBranch(shopBranch, out shopBranchId);

                try
                {
                    string[] shopIdList = new string[] { shopBranchId.ToString() };
                    string[] tagIdList = string.IsNullOrEmpty(shopBranch.ShopBranchTagId) ? new string[] { } : shopBranch.ShopBranchTagId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    ShopBranchApplication.SetShopBrandTagInfos(convertLongs(shopIdList), convertLongs(tagIdList));
                }
                catch { }

                //门店标签
                var shopBranchTagInfos = ShopBranchApplication.GetAllShopBranchTagInfos();
                List<SelectListItem> tagList = new List<SelectListItem>();
                foreach (var item in shopBranchTagInfos)
                {
                    tagList.Add(new SelectListItem
                    {
                        Selected = (shopBranch.ShopBranchTagId == null ? false : shopBranch.ShopBranchTagId.Split(',').Contains(item.Id.ToString()) ? true : false),
                        Value = item.Id.ToString(),
                        Text = item.Title
                    });
                }
                ViewBag.ShopBranchTags = tagList;

                if (CityExpressConfigApplication.GetDaDaCityExpressConfig(CurrentShop.Id).IsEnable)
                {
                    var dada_shop_id = GetNewDadaStoreId(CurrentShop.Id, shopBranch.Id);
                    var _area = RegionApplication.GetRegion(shopBranch.AddressId);
                    var _city = GetCity(_area);
                    var json = ExpressDaDaHelper.shopAdd(CurrentShop.Id, shopBranch.ShopBranchName, 5, _city.ShortName, _area.Parent.Name, shopBranch.AddressDetail, shopBranch.Longitude, shopBranch.Latitude, shopBranch.ContactUser, shopBranch.ContactPhone, dada_shop_id);
                    var resultObj = JsonConvert.DeserializeObject(json) as JObject;
                    string status = resultObj["status"].ToString();
                    int code = int.Parse(resultObj["code"].ToString());
                    if (status == "fail" && code != 7718)
                    {
                        return Json(new Result() { success = true, msg = "但同步门店至达达物流失败，可能所在城市达达不支持" });
                    }
                    if (string.IsNullOrWhiteSpace(shopBranch.DaDaShopId) && (status == "success" || code == 7718))
                    {
                        shopBranch.DaDaShopId = dada_shop_id;
                        ShopBranchApplication.UpdateShopBranch(shopBranch);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new Result() { success = false, msg = ex.Message });
            }
            return Json(new Result() { success = true });
        }
        public ActionResult Edit(long id)
        {
            var shopBranch = ShopBranchApplication.GetShopBranchById(id);

            //门店标签
            var shopBranchTagInfos = ShopBranchApplication.GetAllShopBranchTagInfos();
            List<SelectListItem> tagList = new List<SelectListItem>();
            foreach (var item in shopBranchTagInfos)
            {
                tagList.Add(new SelectListItem
                {
                    Selected = (shopBranch.ShopBranchTagId == null ? false : shopBranch.ShopBranchTagId.Split(',').Contains(item.Id.ToString()) ? true : false),
                    Value = item.Id.ToString(),
                    Text = item.Title
                });
            }
            ViewBag.ShopBranchTags = tagList;
            return View(shopBranch);
        }
        [HttpPost]
        public ActionResult Edit(ShopBranch shopBranch)
        {
            try
            {
                if (!string.Equals(shopBranch.PasswordOne, shopBranch.PasswordTwo))
                {
                    throw new HimallException("两次密码输入不一致！");
                }
                if (shopBranch.ShopBranchName.Length > 15)
                {
                    throw new HimallException("门店名称不能超过15个字！");
                }
                if (shopBranch.AddressDetail.Length > 50)
                {
                    throw new HimallException("详细地址不能超过50个字！");
                }
                if (shopBranch.Latitude <= 0 || shopBranch.Longitude <= 0)
                {
                    throw new HimallException("请搜索地址地图定位！");
                }
                if (!shopBranch.IsAboveSelf && !shopBranch.IsStoreDelive)
                {
                    throw new HimallException("至少需要选择一种配送方式！");
                }
                //判断是否编辑自己的门店
                shopBranch.ShopId = CurrentSellerManager.ShopId;//当前登录商家
                //门店所属商家
                var oldBranch = ShopBranchApplication.GetShopBranchById(shopBranch.Id);
                if (oldBranch != null && oldBranch.ShopId != shopBranch.ShopId)
                    throw new HimallException("不能修改其他商家的门店！");

                try
                {
                    string[] shopIdList = new string[] { shopBranch.Id.ToString() };
                    string[] tagIdList = shopBranch.ShopBranchTagId == null ? new string[] { } : shopBranch.ShopBranchTagId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    ShopBranchApplication.SetShopBrandTagInfos(convertLongs(shopIdList), convertLongs(tagIdList));
                }
                catch { }

                ShopBranchApplication.UpdateShopBranch(shopBranch);

                if (CityExpressConfigApplication.GetDaDaCityExpressConfig(CurrentShop.Id).IsEnable)
                {
                    var _area = RegionApplication.GetRegion(shopBranch.AddressId);
                    var _city = GetCity(_area);
                    string json = "";
                    var dada_shop_id = GetNewDadaStoreId(CurrentShop.Id, shopBranch.Id);
                    if (string.IsNullOrWhiteSpace(shopBranch.DaDaShopId))
                    {
                        json = ExpressDaDaHelper.shopAdd(CurrentShop.Id, shopBranch.ShopBranchName, 5, _city.ShortName, _area.Parent.Name, shopBranch.AddressDetail, shopBranch.Longitude, shopBranch.Latitude, shopBranch.ContactUser, shopBranch.ContactPhone, dada_shop_id);
                    }
                    else
                    {
                        json = ExpressDaDaHelper.shopUpdate(CurrentShop.Id, shopBranch.DaDaShopId, shopBranch.ShopBranchName, 5, _city.ShortName, _area.Parent.Name, shopBranch.AddressDetail, shopBranch.Longitude, shopBranch.Latitude, shopBranch.ContactUser, shopBranch.ContactPhone);
                    }
                    var resultObj = JsonConvert.DeserializeObject(json) as JObject;
                    string status = resultObj["status"].ToString();
                    int code = int.Parse(resultObj["code"].ToString());
                    if (status == "fail" && code != 7718)
                    {
                        return Json(new Result() { success = true, msg = "但同步门店至达达物流失败，可能所在城市达达不支持" });
                    }
                    if (string.IsNullOrWhiteSpace(shopBranch.DaDaShopId) && (status == "success" || code == 7718))
                    {
                        shopBranch.DaDaShopId = dada_shop_id;
                        ShopBranchApplication.UpdateShopBranch(shopBranch);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new Result() { success = false, msg = ex.Message });
            }
            return Json(new Result() { success = true });
        }
        long[] convertLongs(string[] strs)
        {
            List<long> list = new List<long>();
            foreach (string str in strs)
            {
                long info = 0;
                long.TryParse(str, out info);
                list.Add(info);
            }
            return list.ToArray();
        }

        public ActionResult Management()
        {
            return View();
        }
        public JsonResult List(ShopBranchQuery query, int rows, int page)
        {
            query.PageNo = page;
            query.PageSize = rows;
            query.ShopId = (int)CurrentSellerManager.ShopId;
            if (query.AddressId.HasValue)
                query.AddressPath = RegionApplication.GetRegionPath(query.AddressId.Value);
            var shopBranchs = ShopBranchApplication.GetShopBranchs(query);
            var dataGrid = new DataGridModel<ShopBranch>()
            {
                rows = shopBranchs.Models,
                total = shopBranchs.Total
            };
            return Json(dataGrid);
        }

        public JsonResult Freeze(long shopBranchId)
        {
            ShopBranchApplication.Freeze(shopBranchId);
            return Json(new { success = true, msg = "冻结成功！" });
        }
        public JsonResult UnFreeze(long shopBranchId)
        {
            ShopBranchApplication.UnFreeze(shopBranchId);
            return Json(new { success = true, msg = "解冻成功！" });
        }

        /// <summary>
        /// 门店设置
        /// </summary>
        /// <returns></returns>
        public ActionResult Setting()
        {
            var shopInfo = ShopApplication.GetShop(CurrentSellerManager.ShopId);
            if (shopInfo != null)
            {
                ViewBag.AutoAllotOrder = shopInfo.AutoAllotOrder;
            }
            return View();
        }

        [HttpPost]
        public JsonResult Setting(bool autoAllotOrder)
        {
            try
            {
                Himall.DTO.Shop info = ShopApplication.GetShop(CurrentSellerManager.ShopId);
                info.AutoAllotOrder = autoAllotOrder;
                ShopApplication.UpdateShop(info);
                Cache.Remove(CacheKeyCollection.CACHE_SHOPDTO(CurrentSellerManager.ShopId, false));
                Cache.Remove(CacheKeyCollection.CACHE_SHOP(CurrentSellerManager.ShopId, false));

                ServiceApplication.Create<IOperationLogService>().AddSellerOperationLog(
                new LogInfo
                {
                    Date = DateTime.Now,
                    Description = string.Format("{0}:订单自动分配到门店", autoAllotOrder ? "开启" : "关闭"),
                    IPAddress = Request.UserHostAddress,
                    PageUrl = "/ShopBranch/Setting",
                    UserName = CurrentSellerManager.UserName,
                    ShopId = CurrentSellerManager.ShopId
                });
                return Json(new
                {
                    success = true
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    msg = e.Message
                });
            }
        }

        /// <summary>
        /// 门店链接二维码
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public JsonResult StoresLink(string vshopUrl)
        {
            string qrCodeImagePath = string.Empty;
            if (!string.IsNullOrWhiteSpace(vshopUrl))
            {
                Bitmap map;
                map = Core.Helper.QRCodeHelper.Create(vshopUrl);
                MemoryStream ms = new MemoryStream();
                map.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                qrCodeImagePath = "data:image/gif;base64," + Convert.ToBase64String(ms.ToArray()); // 将图片内存流转成base64,图片以DataURI形式显示  
                ms.Dispose();
            }
            return Json(new { success = true, qrCodeImagePath = qrCodeImagePath }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 达达物流配置
        /// </summary>
        /// <returns></returns>
        public ActionResult DaDaConfig()
        {
            var data = CityExpressConfigApplication.GetDaDaCityExpressConfig(CurrentShop.Id);
            var result = new ShopBranchDaDaConfigModel
            {
                IsEnable = data.IsEnable,
                app_key = data.app_key,
                app_secret = data.app_secret,
                source_id = data.source_id
            };
            return View(result);
        }
        [HttpPost]
        public JsonResult DaDaConfig(ShopBranchDaDaConfigModel model)
        {
            long shopId = CurrentShop.Id;
            Result result = new Result
            {
                success = false,
                msg = "未知错误"
            };
            if (ModelState.IsValid)
            {
                if (model.IsEnable)
                {
                    if (string.IsNullOrWhiteSpace(model.app_key) || string.IsNullOrWhiteSpace(model.app_secret) || string.IsNullOrWhiteSpace(model.source_id))
                    {
                        result.success = false;
                        result.msg = "数据错误，请填写必填信息";
                        return Json(result);
                    }
                }
                var data = CityExpressConfigApplication.GetDaDaCityExpressConfig(shopId);
                data.IsEnable = model.IsEnable;
                data.app_key = model.app_key;
                data.app_secret = model.app_secret;
                data.source_id = model.source_id;
                CityExpressConfigApplication.Update(CurrentShop.Id, data);
                result.msg = "";
                //同步开通达达门店
                var sblist = ShopBranchApplication.GetShopBranchByShopId(shopId).Where(d => string.IsNullOrWhiteSpace(d.DaDaShopId));
                foreach (var item in sblist)
                {
                    var dada_shop_id = GetNewDadaStoreId(CurrentShop.Id, item.Id);
                    var _area = RegionApplication.GetRegion(item.AddressId);
                    var _city = GetCity(_area);
                    var json = ExpressDaDaHelper.shopAdd(shopId, item.ShopBranchName, 5, _city.ShortName, _area.Parent.Name, item.AddressDetail, item.Longitude, item.Latitude, item.ContactUser, item.ContactPhone, dada_shop_id);
                    var resultObj = JsonConvert.DeserializeObject(json) as JObject;
                    string status = resultObj["status"].ToString();
                    int code = int.Parse(resultObj["code"].ToString());
                    if (status == "fail" && code != 7718)
                    {
                        result.msg = "但部份同步门店失败，可能所在城市达达不支持";
                    }
                    if (string.IsNullOrWhiteSpace(item.DaDaShopId) && (status == "success" || code == 7718))
                    {
                        item.DaDaShopId = dada_shop_id;
                        ShopBranchApplication.UpdateShopBranch(item);
                    }
                }
                result.success = true;
            }
            else
            {
                result.success = false;
                result.msg = "数据错误，请填写必填信息";
            }

            return Json(result);
        }
        /// <summary>
        /// 获取市级地区
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        private CommonModel.Region GetCity(CommonModel.Region region)
        {
            CommonModel.Region _city = region;
            if (_city.Level == CommonModel.Region.RegionLevel.City || _city.Level == CommonModel.Region.RegionLevel.Province || _city.Parent == null)
            {
                return _city;
            }
            _city = _city.Parent;
            return GetCity(_city);
        }
        /// <summary>
        /// 获取达达门店编号
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="shopbranchId"></param>
        /// <returns></returns>
        private string GetNewDadaStoreId(long shopId, long shopbranchId)
        {
            return DADA_STORE_PREFIX + shopId + "_" + shopbranchId;
        }
    }
}