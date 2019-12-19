using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Framework;
using Himall.Model;
using Himall.DTO.QueryModel;
using Himall.IServices;
using Himall.Core;
using Himall.Web.Models;
using Himall.Web.Areas.Web.Models;
using Himall.Core.Plugins.Payment;
using Himall.Application;
using Himall.CommonModel;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class ShopWithDrawController : BaseAdminController
    {


        public ActionResult Management(int status = 1)
        {
            ViewBag.Status = status;
            return View();
        }

        /// <summary>
        /// 提现信息列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult List(DateTime? startDate, DateTime? endDate, DateTime? startDates, DateTime? endDates, string shopName, int Status, int page, int rows)
        {
            WithdrawQuery query = new WithdrawQuery()
            {
                ApplyStartTime = startDate,
                ApplyEndTime = endDate,
                AuditedStartTime = startDates,
                AuditedEndTime = endDates,
                ShopName = shopName,
                Status = (WithdrawStaus?)Status,
                PageSize = rows,
                PageNo = page
            };
            var model = BillingApplication.GetShopWithDraw(query);
            return Json(new { rows = model.Models, total = model.Total });
        }

        /// <summary>
        /// 审核操作
        /// </summary>
        /// <param name="id">申请ID</param>
        /// <param name="status">状态</param>
        /// <param name="remark">备注</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ConfirmPay(long id, int status, string remark)
        {
            var result = BillingApplication.ShopApplyWithDraw(id, (Himall.CommonModel.WithdrawStaus)status, remark, Request.UserHostAddress, CurrentManager.UserName, webRoot: CurrentUrlHelper.CurrentUrlNoPort());
            if (result.Success)
            {
                return Json(new { success = true, msg = "成功！", jumpurl = result.JumpUrl, status = !string.IsNullOrWhiteSpace(result.JumpUrl) ? 2 : 0 });
            }
            else
            {
                return Json(new { success = false, msg = "操作失败，可能微信证书设置错误或者扣款账号余额不足，检查完稍后再试。" });
            }
        }
        /// <summary>
        /// 取消提现
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult CancelPay(long Id)
        {
            string Msg = "ok";
            var b = true;
            if (Id > 0)
            {
                b = BillingApplication.CancelShopApplyWithDraw(Id);
                if (!b)
                    Msg = "取消失败";
            }
            else
            {
                b = false;
                Msg = "数据错误";
            }
            return Json(new Result() { success = b, msg = Msg });
        }

        /// <summary>
        /// 商家提现设置
        /// </summary>
        /// <returns></returns>
        public ActionResult Setting()
        {
            var siteSetting = CurrentSiteSetting;
            return View(siteSetting);
        }

        /// <summary>
        /// 保存商家体现设置
        /// </summary>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <param name="alipayEnable"></param>
        /// <returns></returns>
        public JsonResult SaveWithDrawSetting(string minimum, string maximum)
        {
            int min = 0, max = 0;
            if (int.TryParse(minimum, out min) && int.TryParse(maximum, out max))
            {
                if (min > 0 && min < max && max <= 1000000)
                {
                    SiteSettingApplication.SaveSetting("ShopWithDrawMaximum", maximum);
                    SiteSettingApplication.SaveSetting("ShopWithDrawMinimum", minimum);
                    return Json(new Result() { success = true, msg = "保存成功" });
                }
                return Json(new Result() { success = false, msg = "金额范围只能是(1-1000000)" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "只能输入数字" });
            }
        }
    }
}