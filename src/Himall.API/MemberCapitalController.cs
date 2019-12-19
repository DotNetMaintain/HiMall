using Himall.Core;
using Himall.IServices;
using Himall.DTO.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Himall.API.Model.ParamsModel;
using Himall.Application;
using Himall.DTO;
using Himall.API.Model;
using Himall.CommonModel;
using System;
using Senparc.Weixin.MP.CommonAPIs;

namespace Himall.API
{
    public class MemberCapitalController : BaseApiController
    {
        private const string PLUGIN_OAUTH_WEIXIN = "Himall.Plugin.OAuth.WeiXin";
        public object Get()
        {
            CheckUserLogin();
            var capital = MemberCapitalApplication.GetCapitalInfo(CurrentUserId);
            var sitesetting = SiteSettingApplication.GetSiteSettings();
            var redPacketAmount = 0M;
            if (capital != null)
            {
                redPacketAmount = capital.Himall_CapitalDetail.Where(e => e.SourceType == Himall.Model.CapitalDetailInfo.CapitalDetailType.RedPacket).Sum(e => e.Amount);
            }
            return new
            {
                success = true,
                Balance = capital.Balance ?? 0,
                RedPacketAmount = redPacketAmount,
                ChargeAmount = capital.ChargeAmount ?? 0,
                WithDrawMinimum = sitesetting.WithDrawMinimum,
                WithDrawMaximum = sitesetting.WithDrawMaximum
            };
        }

        public object GetList(int pageNo = 1, int pageSize = 10)
        {
            CheckUserLogin();
            var capitalService = ServiceApplication.Create<IMemberCapitalService>();

            var query = new CapitalDetailQuery
            {
                memberId = CurrentUser.Id,
                PageSize = pageSize,
                PageNo = pageNo
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
                Remark = e.SourceType.ToDescription(),
                PayWay = e.Remark
            });
            dynamic result = SuccessResult();
            result.rows = model;
            result.total = pageMode.Total;

            return result;
        }


        /// <summary>
        /// 是否可以提现
        /// </summary>
        /// <returns></returns>
        public object GetCanWithDraw()
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
            dynamic result = new Result();
            result.success = canWeiXin || canAlipay;
            result.canWeiXin = canWeiXin;
            result.canAlipay = canAlipay;
            return result;
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
        public object PostApplyWithDraw(MemberCapitalApplyWithDrawModel para)
        {
            CheckUserLogin();
            if (para == null)
            {
                para = new MemberCapitalApplyWithDrawModel();
            }
            if (string.IsNullOrEmpty(para.pwd))
            {
                throw new HimallException("请输入密码！");
            }
            var success = MemberApplication.VerificationPayPwd(CurrentUser.Id, para.pwd);
            var sitesetting = SiteSettingApplication.GetSiteSettings();
            if (para.applyType == CommonModel.UserWithdrawType.ALiPay.GetHashCode() && !sitesetting.Withdraw_AlipayEnable)
            {
                throw new HimallException("不支持支付宝提现方式！");
            }
            var _iMemberCapitalService = ServiceApplication.Create<IMemberCapitalService>();
            if (!success)
            {
                throw new HimallException("支付密码不对，请重新输入！");
            }
            var capitalInfo = _iMemberCapitalService.GetCapitalInfo(CurrentUser.Id);
            if (para.amount > capitalInfo.Balance)
            {
                throw new HimallException("提现金额不能超出可用金额！");
            }
            if (para.amount <= 0)
            {
                throw new HimallException("提现金额不能小于等于0！");
            }
            if (string.IsNullOrWhiteSpace(para.openid) && para.applyType == CommonModel.UserWithdrawType.WeiChat.GetHashCode())
            {
                var mo = MemberApplication.GetMemberOpenIdInfoByuserId(CurrentUser.Id, MemberOpenIdInfo.AppIdTypeEnum.Payment, PLUGIN_OAUTH_WEIXIN);
                if (mo != null && !string.IsNullOrWhiteSpace(mo.OpenId))
                {
                    para.openid = mo.OpenId;
                }
            }
            if (string.IsNullOrWhiteSpace(para.nickname) && para.applyType == CommonModel.UserWithdrawType.ALiPay.GetHashCode())
            {
                throw new HimallException("数据异常,真实姓名不可为空！");
            }
            if (!string.IsNullOrWhiteSpace(para.openid) && para.applyType == CommonModel.UserWithdrawType.WeiChat.GetHashCode())
            {
                //para.openid = Core.Helper.SecureHelper.AESDecrypt(para.openid, "Mobile");
                if (!(string.IsNullOrWhiteSpace(sitesetting.WeixinAppId) || string.IsNullOrWhiteSpace(sitesetting.WeixinAppSecret)))
                {
                    string token = AccessTokenContainer.TryGetToken(sitesetting.WeixinAppId, sitesetting.WeixinAppSecret);
                    var userinfo = Senparc.Weixin.MP.CommonAPIs.CommonApi.GetUserInfo(token, para.openid);
                    if (userinfo != null)
                    {
                        para.nickname = userinfo.nickname;
                    }
                }
            }
            if (string.IsNullOrWhiteSpace(para.openid))
            {
                throw new HimallException("数据异常,OpenId或收款账号不可为空！");
            }

            ApplyWithDrawInfo model = new ApplyWithDrawInfo()
            {
                ApplyAmount = para.amount,
                ApplyStatus = ApplyWithDrawInfo.ApplyWithDrawStatus.WaitConfirm,
                ApplyTime = DateTime.Now,
                MemId = CurrentUser.Id,
                OpenId = para.openid,
                NickName = para.nickname,
                ApplyType = (CommonModel.UserWithdrawType)para.applyType
            };
            _iMemberCapitalService.AddWithDrawApply(model);
            return SuccessResult();
        }
    }
}