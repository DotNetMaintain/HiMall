using Himall.Core;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class WdgjApiController : BaseSellerController
    {
        private IShopService _iShopService;
        private long CurShopId;

        public WdgjApiController(IShopService iShopService)
        {
            _iShopService = iShopService;
            if (CurrentSellerManager != null)
            {//退出登录后，直接进入controller异常处理
                CurShopId = CurrentSellerManager.ShopId;
            }
        }

        public ActionResult Index()
        {
            var data = _iShopService.GetshopWdgjInfoById(CurShopId);
            var models = new WdgjApiModel()
            {
                Id = data != null ? data.Id : 0,
                uCode = data != null ? data.uCode : "",
                uSign = data != null ? data.uSign : ""
            };
            return View(models);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult Add(WdgjApiModel wdgj)
        {
            var service = _iShopService;
            ShopWdgjSetting shopwdgjInfo = new ShopWdgjSetting()
            {
                Id = wdgj.Id,
                ShopId = CurShopId,
                uCode = wdgj.uCode,
                uSign = wdgj.uSign
            };
            if (shopwdgjInfo.Id > 0)
                service.UpdateShopWdgj(shopwdgjInfo);
            else
                service.AddShopWdgj(shopwdgjInfo);
            return Json(new { success = true });
        }
    }
}