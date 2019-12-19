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

namespace Himall.Web.Areas.Admin.Controllers
{
    [StoreAuthorization]
    public class ShopBranchController : BaseAdminController
    {
        public ActionResult Tags()
        {
            return View();
        }

        public JsonResult TagList()
        {

            var shopBranchTagInfos = ShopBranchApplication.GetAllShopBranchTagInfos();
            var dataGrid = new DataGridModel<ShopBranchTagModel>() { rows = shopBranchTagInfos, total = shopBranchTagInfos.Count() };
            return Json(dataGrid);
        }
        public JsonResult AddTag(string title)
        {
            try
            {
                ShopBranchApplication.AddShopBranchTagInfo(title);
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message });
            }
        }
        public JsonResult EditTag(long Id, string title)
        {
            try
            {
                ShopBranchApplication.UpdateShopBranchTagInfo(Id, title);
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message });
            }
        }
        public JsonResult DeleteTag(long Id)
        {
            try
            {
                ShopBranchApplication.DeleteShopBranchTagInfo(Id);
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message });
            }
        }



        public ActionResult Management(long? shopBranchTagId)
        {
            var shopBranchTagInfos = ShopBranchApplication.GetAllShopBranchTagInfos();
            List<SelectListItem> tagList = new List<SelectListItem>(){new SelectListItem
            {
                Selected = true,
                Value = 0.ToString(),
                Text = "请选择..."
            }};
            foreach (var item in shopBranchTagInfos)
            {
                tagList.Add(new SelectListItem
                {
                    Selected = false,
                    Value = item.Id.ToString(),
                    Text = item.Title
                });
            }

            if(shopBranchTagId.HasValue)
            {
                var item = tagList.FirstOrDefault(t => t.Value == shopBranchTagId.ToString());
                if (item != null)
                {
                    item.Selected = true;
                }
            }

            ViewBag.ShopBranchTags = tagList;

            IShopService _iShopService = ObjectContainer.Current.Resolve<IShopService>();
            var shops = _iShopService.GetAllShops();
            List<SelectListItem> shopList = new List<SelectListItem>{new SelectListItem
            {
                Selected = true,
                Value = 0.ToString(),
                Text = "请选择..."
            }};
            foreach (var item in shops)
            {
                shopList.Add(new SelectListItem
                {
                    Selected = false,
                    Value = item.Id.ToString(),
                    Text = item.ShopName
                });
            }

            ViewBag.Shops = shopList;

            return View();
        }


        public JsonResult List(ShopBranchQuery query, int rows, int page)
        {
            query.PageNo = page;
            query.PageSize = rows;

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
        /// 批量设置门店标签
        /// </summary>
        /// <returns></returns>
        public JsonResult SetShopBranchTags(string shopIds, string tagIds)
        {
            try
            {
                string[] shopIdList = shopIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string[] tagIdList = string.IsNullOrEmpty(tagIds) ? new string[] { } : tagIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                ShopBranchApplication.SetShopBrandTagInfos(convertLongs(shopIdList), convertLongs(tagIdList));

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
    }
}