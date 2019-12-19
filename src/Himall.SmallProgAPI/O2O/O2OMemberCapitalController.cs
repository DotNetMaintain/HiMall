﻿using Himall.Application;
using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.Core.Plugins.Payment;
using Himall.DTO;
using Himall.DTO.QueryModel;
using Himall.Model;
using Himall.SmallProgAPI.O2O.Model;
using Himall.Web.Framework;
using Senparc.Weixin.MP.CommonAPIs;
using System;
using System.Linq;
using System.Web.Http.Results;

namespace Himall.SmallProgAPI
{
    public class O2OMemberCapitalController : BaseO2OApiController
    {

        private const string PLUGIN_OAUTH_WEIXIN = "Himall.Plugin.OAuth.WeiXin";
        public JsonResult<Result<dynamic>> GetCapial()
        {
            CheckUserLogin();
            var capital = MemberCapitalApplication.GetCapitalInfo(CurrentUser.Id);
            var sitesetting = SiteSettingApplication.GetSiteSettings();
            var redPacketAmount = 0M;
            if (capital != null)
            {
                redPacketAmount = capital.Himall_CapitalDetail.Where(e => e.SourceType == Himall.Model.CapitalDetailInfo.CapitalDetailType.RedPacket).Sum(e => e.Amount);
            }
            return JsonResult<dynamic>(new
            {
                success = true,
                Balance = capital == null ? 0 : (capital.Balance ?? 0),
                RedPacketAmount = redPacketAmount,
                ChargeAmount = capital == null ? 0 : (capital.ChargeAmount ?? 0),
                WithDrawMinimum = sitesetting.WithDrawMinimum,
                WithDrawMaximum = sitesetting.WithDrawMaximum
            });
        }

        public JsonResult<Result<dynamic>> GetList(int pageNo = 1, int pageSize = 10)
        {
            CheckUserLogin();
            var query = new CapitalDetailQuery
            {
                memberId = CurrentUser.Id,
                PageSize = pageSize,
                PageNo = pageNo
            };
            var pageMode = MemberCapitalApplication.GetCapitalDetails(query);
            var model = pageMode.Models.ToList().Select(e => new CapitalDetailModel
            {
                Id = e.Id,
                Amount = e.Amount,
                CapitalID = e.CapitalID,
                CreateTime = e.CreateTime.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                SourceData = e.SourceData,
                SourceType = e.SourceType,
                Remark = e.SourceType.ToDescription() + ",单号：" + (string.IsNullOrWhiteSpace(e.SourceData) ? e.Id.ToString() : e.SourceData),
                PayWay = e.Remark
            });

            return JsonResult<dynamic>(new { rows = model, total = pageMode.Total });
        }


        /// <summary>
        /// 是否可以提现
        /// </summary>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetCanWithDraw()
        {
            CheckUserLogin();
            bool canWeiXin = false;
            bool canAlipay = false;
            var sitesetting = SiteSettingApplication.GetSiteSettings();
            //判断是否有微信openid
            var mo = MemberApplication.GetMemberOpenIdInfoByuserId(CurrentUser.Id, MemberOpenIdInfo.AppIdTypeEnum.Payment, PLUGIN_OAUTH_WEIXIN);
            if (mo != null && !string.IsNullOrWhiteSpace(mo.OpenId)) { canWeiXin = true; }
            //判断是否开启支付宝
            if (sitesetting.Withdraw_AlipayEnable)
            {
                canAlipay = true;
            }
            return Json(ApiResult<dynamic>(canWeiXin || canAlipay, data: new { canWeiXin = canWeiXin, canAlipay = canAlipay }));
        }
        /// <summary>
        /// 申请提现
        /// </summary>
        /// <param name="openid"></param>
        /// <param name="nickname"></param>
        /// <param name="amount"></param>
        /// <param name="pwd"></param>
        /// <param name="applyType"></param>
        /// <returns></returns>
        public JsonResult<Result<bool>> PostApplyWithDraw(MemberCapitalApplyWithDrawModel para)
        {
            CheckUserLogin();
            if (para == null)
            {
                para = new MemberCapitalApplyWithDrawModel();
            }
            var success = MemberApplication.VerificationPayPwd(CurrentUser.Id, para.pwd);
            var sitesetting = SiteSettingApplication.GetSiteSettings();
            if (para.applyType == CommonModel.UserWithdrawType.ALiPay.GetHashCode() && !sitesetting.Withdraw_AlipayEnable)
            {
                return Json(ErrorResult<bool>("不支持支付宝提现方式"));
            }
            //var _iMemberCapitalService = ServiceApplication.Create<IMemberCapitalService>();
            if (!success)
            {
                return Json(ErrorResult<bool>("支付密码不对，请重新输入"));
            }
            var capitalInfo = MemberCapitalApplication.GetCapitalInfo(CurrentUser.Id);
            if (para.amount > capitalInfo.Balance)
            {
                return Json(ErrorResult<bool>("提现金额不能超出可用金额！"));
            }
            if (para.amount <= 0)
            {
                return Json(ErrorResult<bool>("提现金额不能小于等于0！"));
            }
            if (string.IsNullOrWhiteSpace(para.openId) && para.applyType == CommonModel.UserWithdrawType.WeiChat.GetHashCode())
            {
                var mo = MemberApplication.GetMemberOpenIdInfoByuserId(CurrentUser.Id, MemberOpenIdInfo.AppIdTypeEnum.Payment, PLUGIN_OAUTH_WEIXIN);
                if (mo != null && !string.IsNullOrWhiteSpace(mo.OpenId))
                {
                    para.openId = mo.OpenId;
                }
            }
            if (string.IsNullOrWhiteSpace(para.nickname) && para.applyType == CommonModel.UserWithdrawType.ALiPay.GetHashCode())
            {
                return Json(ErrorResult<bool>("数据异常,真实姓名不可为空！"));
            }
            if (!string.IsNullOrWhiteSpace(para.openId) && para.applyType == CommonModel.UserWithdrawType.WeiChat.GetHashCode())
            {
                //para.openid = Core.Helper.SecureHelper.AESDecrypt(para.openid, "Mobile");
                if (!string.IsNullOrWhiteSpace(sitesetting.WeixinO2OAppletId) && !string.IsNullOrWhiteSpace(sitesetting.WeixinO2OAppletSecret))
                {
                    string token = AccessTokenContainer.TryGetToken(sitesetting.WeixinO2OAppletId, sitesetting.WeixinO2OAppletSecret);
                    var userinfo = Senparc.Weixin.MP.CommonAPIs.CommonApi.GetUserInfo(token, para.openId);
                    if (userinfo != null)
                    {
                        para.nickname = userinfo.nickname;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(para.openId))
            {
                return Json(ErrorResult<bool>("数据异常,OpenId或收款账号不可为空！"));
            }

            ApplyWithDrawInfo model = new ApplyWithDrawInfo()
            {
                ApplyAmount = para.amount,
                ApplyStatus = ApplyWithDrawInfo.ApplyWithDrawStatus.WaitConfirm,
                ApplyTime = DateTime.Now,
                MemId = CurrentUser.Id,
                OpenId = para.openId,
                NickName = para.nickname,
                ApplyType = (CommonModel.UserWithdrawType)para.applyType
            };
            MemberCapitalApplication.AddWithDrawApply(model);
            return JsonResult(true);
        }

        /// <summary>
        /// 预账户充值接口
        /// </summary>
        /// <param name="pluginId">支付插件Id</param>
        /// <param name="amount">充值金额</param>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> PostCharge(PaymentChargeModel para)
        {
            para.amount = Math.Round(para.amount, 2);
            if (para.amount <= 0)
                return Json(ErrorResult<dynamic>("请输入正确的金额"));
            if (string.IsNullOrWhiteSpace(para.openId))
            {
                return Json(ErrorResult<dynamic>("缺少OpenId"));
            }
            try
            {
                //获取支付插件
                var mobilePayments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.Biz.SupportPlatforms.Contains(Core.PlatformType.WeiXinO2OSmallProg));
                if (mobilePayments.Any())
                {
                    var plugin = mobilePayments.Where(x => x.PluginInfo.PluginId.Contains(para.typeId)).FirstOrDefault();
                    //添加充值明细
                    var chargeDetail = new DTO.ChargeDetail();
                    chargeDetail.ChargeAmount = para.amount;
                    chargeDetail.ChargeStatus = ChargeDetailInfo.ChargeDetailStatus.WaitPay;
                    chargeDetail.ChargeWay = plugin.PluginInfo.DisplayName;
                    chargeDetail.CreateTime = DateTime.Now;
                    chargeDetail.MemId = CurrentUser.Id;
                    var id = MemberCapitalApplication.AddChargeApply(chargeDetail);

                    string webRoot = CurrentUrlHelper.CurrentUrlNoPort();
                    string urlPre = webRoot + "/m-" + Core.PlatformType.Android + "/Payment/";
                    string notifyPre = urlPre + "CapitalChargeNotify/";
                    string returnPre = "";

                    var models = mobilePayments.ToArray().Select(item =>
                    {
                        string url = string.Empty;
                        try
                        {
                            url = item.Biz.GetRequestUrl(returnPre, notifyPre + item.PluginInfo.PluginId.Replace(".", "-") + "/", id.ToString(), para.amount, "会员充值", openId: para.openId);
                        }
                        catch (Exception ex)
                        {
                            Core.Log.Error("获取支付方式错误：", ex);
                        }
                    //适配小程序接口，从支付插件里解析出相应参数
                    //字符串格式：prepayId:234320480,partnerid:32423489,nonceStr=dslkfjsld
                    #region 适配小程序接口，从支付插件里解析出相应参数
                    var prepayId = string.Empty;
                        var nonceStr = string.Empty;
                        var timeStamp = string.Empty;
                        var sign = string.Empty;
                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            var paras = url.Split(',');
                            foreach (var str in paras)
                            {
                                var keyValuePair = str.Split(':');
                                if (keyValuePair.Length == 2)
                                {
                                    switch (keyValuePair[0])
                                    {
                                        case "prepayId":
                                            prepayId = keyValuePair[1];
                                            break;
                                        case "nonceStr":
                                            nonceStr = keyValuePair[1];
                                            break;
                                        case "timeStamp":
                                            timeStamp = keyValuePair[1];
                                            break;
                                        case "sign":
                                            sign = keyValuePair[1];
                                            break;
                                    }
                                }
                            }
                        }
                    #endregion
                    return new
                        {
                            prepayId = prepayId,
                            nonceStr = nonceStr,
                            timeStamp = timeStamp,
                            sign = sign
                        };
                    });
                    var model = models.FirstOrDefault();
                    if (null == model) return Json(ErrorResult<dynamic>("获取支付方式失败，请与管理员联系"));

                    return JsonResult<dynamic>(model);
                }else
                {
                    Core.Log.Error("暂未配置支付方式");
                    return Json(ErrorResult<dynamic>("暂未配置支付方式"));
                }
            }
            catch (Exception ex)
            {
                Core.Log.Error("预账户充值报错：", ex);
                return Json(ErrorResult<dynamic>("预账户充值报错"));
            }
        }
    }
}
