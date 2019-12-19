using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Framework;
using Himall.IServices;
using Himall.DTO.QueryModel;
using Himall.Core;
using Himall.Model;
using Senparc.Weixin.MP.CommonAPIs;
using Himall.Core.Plugins.Payment;
using Himall.Application;
using Himall.DTO;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class CapitalController : BaseMobileMemberController
    {
        IMemberCapitalService _iMemberCapitalService;
        ISiteSettingService _iSiteSettingService;
        private const string PLUGIN_OAUTH_WEIXIN = "Himall.Plugin.OAuth.WeiXin";
        public CapitalController(IMemberCapitalService iMemberCapitalService, ISiteSettingService iSiteSettingService)
        {
            _iMemberCapitalService = iMemberCapitalService;
            _iSiteSettingService = iSiteSettingService;
        }
        // GET: Mobile/Capital
        public ActionResult Index()
        {
            //判断是否需要跳转到支付地址
            if (this.Request.Url.AbsolutePath.EndsWith("/Capital/Index", StringComparison.OrdinalIgnoreCase) || this.Request.Url.AbsolutePath.EndsWith("/Capital", StringComparison.OrdinalIgnoreCase))
                return Redirect(Url.RouteUrl("PayRoute") + "?area=mobile&platform=" + this.PlatformType.ToString() + "&controller=Capital&action=Index");

            var capitalService = _iMemberCapitalService;
            var model = capitalService.GetCapitalInfo(CurrentUser.Id);
            var redPacketAmount = 0M;
            if (model != null)
            {
                redPacketAmount = model.Himall_CapitalDetail.Where(e => e.SourceType == Model.CapitalDetailInfo.CapitalDetailType.RedPacket).Sum(e => e.Amount);
                ViewBag.CapitalDetails = model.Himall_CapitalDetail.OrderByDescending(e => e.CreateTime).Take(15);
            }

            ViewBag.RedPacketAmount = redPacketAmount;
            ViewBag.IsSetPwd = string.IsNullOrWhiteSpace(CurrentUser.PayPwd) ? false : true;
            var siteSetting = _iSiteSettingService.GetSiteSettings();
            ViewBag.WithDrawMinimum = siteSetting.WithDrawMinimum;
            ViewBag.WithDrawMaximum = siteSetting.WithDrawMaximum;
            return View(model);
        }
        public JsonResult List(int page, int rows)
        {
            var capitalService = _iMemberCapitalService;

            var query = new CapitalDetailQuery
            {
                memberId = CurrentUser.Id,
                PageSize = rows,
                PageNo = page
            };
            var pageMode = capitalService.GetCapitalDetails(query);
            var model = pageMode.Models.ToList().Select(e => new CapitalDetailModel
            {
                Id = e.Id,
                Amount = e.Amount,
                CapitalID = e.CapitalID,
                CreateTime = e.CreateTime.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                SourceData = e.SourceData,
                SourceType = e.SourceType,
                Remark = e.Remark,
                PayWay = e.Remark,
                SourceTypeName = e.SourceType.ToDescription()
            });

            return Json(new { model = model, total = pageMode.Total });
        }
        public JsonResult SetPayPwd(string pwd)
        {
            _iMemberCapitalService.SetPayPwd(CurrentUser.Id, pwd);
            return Json(new { success = true, msg = "设置成功" });
        }
        /// <summary>
        /// 是否可以提现
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CanWithDraw()
        {
            bool canWeiXin = false;
            bool canAlipay = false;
            if (PlatformType == PlatformType.WeiXin)
            {
                canWeiXin = true;
            }
            else
            {
                //判断是否有微信openid
                //var mo = MemberApplication.GetMemberOpenIdInfoByuserId(CurrentUser.Id, MemberOpenIdInfo.AppIdTypeEnum.Payment, PLUGIN_OAUTH_WEIXIN);
                //if (mo != null && !string.IsNullOrWhiteSpace(mo.OpenId)) { canWeiXin = true;}
            }
            //判断是否开启支付宝
            if (CurrentSiteSetting.Withdraw_AlipayEnable)
            {
                canAlipay = true;
            }
            return Json(new { success = canWeiXin || canAlipay, canWeiXin = canWeiXin, canAlipay = canAlipay });
        }
        public JsonResult ApplyWithDrawSubmit(string openid, string nickname, decimal amount, string pwd, int applyType = 1)
        {
            var success = MemberApplication.VerificationPayPwd(CurrentUser.Id, pwd);
            if (!success)
            {
                throw new HimallException("支付密码不对，请重新输入！");
            }
            if (applyType == CommonModel.UserWithdrawType.ALiPay.GetHashCode() && !CurrentSiteSetting.Withdraw_AlipayEnable)
            {
                throw new HimallException("不支持支付宝提现方式！");
            }
            var capitalInfo = _iMemberCapitalService.GetCapitalInfo(CurrentUser.Id);
            if (amount > capitalInfo.Balance)
            {
                throw new HimallException("提现金额不能超出可用金额！");
            }
            if (amount <= 0)
            {
                throw new HimallException("提现金额不能小于等于0！");
            }
            if (string.IsNullOrWhiteSpace(openid) && applyType == CommonModel.UserWithdrawType.WeiChat.GetHashCode())
            {
                openid = Core.Helper.WebHelper.GetCookie(CookieKeysCollection.HIMALL_USER_OpenID);
            }
            if (string.IsNullOrWhiteSpace(nickname) && applyType == CommonModel.UserWithdrawType.ALiPay.GetHashCode())
            {
                throw new HimallException("数据异常,真实姓名不可为空！");
            }
            if (!string.IsNullOrWhiteSpace(openid) && applyType == CommonModel.UserWithdrawType.WeiChat.GetHashCode())
            {
                openid = Core.Helper.SecureHelper.AESDecrypt(openid, "Mobile");
                var siteSetting = _iSiteSettingService.GetSiteSettings();
                if (!(string.IsNullOrWhiteSpace(siteSetting.WeixinAppId) || string.IsNullOrWhiteSpace(siteSetting.WeixinAppSecret)))
                {
                    string token = AccessTokenContainer.TryGetToken(siteSetting.WeixinAppId, siteSetting.WeixinAppSecret);
                    var userinfo = Senparc.Weixin.MP.CommonAPIs.CommonApi.GetUserInfo(token, openid);
                    if (userinfo != null)
                    {
                        nickname = userinfo.nickname;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(openid))
            {
                throw new HimallException("数据异常,OpenId或收款账号不可为空！");
            }

            ApplyWithDrawInfo model = new ApplyWithDrawInfo()
            {
                ApplyAmount = amount,
                ApplyStatus = ApplyWithDrawInfo.ApplyWithDrawStatus.WaitConfirm,
                ApplyTime = DateTime.Now,
                MemId = CurrentUser.Id,
                OpenId = openid,
                NickName = nickname,
                ApplyType = (CommonModel.UserWithdrawType)applyType
            };
            _iMemberCapitalService.AddWithDrawApply(model);
            return Json(new { success = true });
        }

        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Charge(string pluginId, decimal amount)
        {
            amount = Math.Round(amount, 2);
            if (amount <= 0)
                return Json(new { success = false, msg = "请输入正确的金额" });

            var plugin = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(pluginId);

            var chargeDetail = new DTO.ChargeDetail();
            chargeDetail.ChargeAmount = amount;
            chargeDetail.ChargeStatus = ChargeDetailInfo.ChargeDetailStatus.WaitPay;
            chargeDetail.ChargeWay = plugin.PluginInfo.DisplayName;
            chargeDetail.CreateTime = DateTime.Now;
            chargeDetail.MemId = CurrentUser.Id;
            var id = MemberCapitalApplication.AddChargeApply(chargeDetail);
            Log.Debug("id是"+ id);
            string openId = Core.Helper.WebHelper.GetCookie(CookieKeysCollection.HIMALL_USER_OpenID);
            Log.Debug("openId是" + openId);
            if (!string.IsNullOrWhiteSpace(openId))
            {
                openId = Core.Helper.SecureHelper.AESDecrypt(openId, "Mobile");
            }
            else
            {
                var openUserInfo = Application.MemberApplication.GetMemberOpenIdInfoByuserId(CurrentUser.Id, MemberOpenIdInfo.AppIdTypeEnum.Payment);
                if (openUserInfo != null)
                    openId = openUserInfo.OpenId;
            }

            string webRoot = Request.Url.Scheme + "://" + Request.Url.Authority;
            string notifyUrl = webRoot + "/m-" + PlatformType + "/Payment/CapitalChargeNotify/" + plugin.PluginInfo.PluginId.Replace(".", "-") + "/";
            string returnUrl = webRoot + "/m-" + PlatformType + "/Capital/Index";
            var requestUrl = plugin.Biz.GetRequestUrl(returnUrl, notifyUrl, id.ToString(), amount, "会员充值", openId);
            return Json(new
            {
                href = requestUrl,
                success = true
            });
        }

        public ActionResult ChargeSuccess(string id)
        {
            Log.Info("pluginId:" + id);
            var plugin = Core.PluginsManagement.GetPlugin<IPaymentPlugin>(id.Replace("-", "."));
            var payInfo = plugin.Biz.ProcessNotify(this.HttpContext.Request);
            if (payInfo != null)
            {

                var chargeApplyId = payInfo.OrderIds.FirstOrDefault();
                Log.Info("chargeApplyId:" + chargeApplyId);
                MemberCapitalApplication.ChargeSuccess(chargeApplyId);
                var response = plugin.Biz.ConfirmPayResult();
                return Content(response);
            }
            Log.Info("payInfo:为空");
            return Content(string.Empty);
        }
    }
}