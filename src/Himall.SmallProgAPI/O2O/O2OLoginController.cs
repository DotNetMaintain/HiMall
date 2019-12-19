using Himall.Application;
using Himall.Core.Helper;
using Himall.Model;
using Himall.SmallProgAPI.Model;
using Himall.Web.Framework;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Web.Http.Results;

namespace Himall.SmallProgAPI
{
    public class O2OLoginController : BaseO2OApiController
    {
        /// <summary>
        /// 根据OpenId判断是否有账号，根据OpenId进行登录
        /// </summary>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetLoginByOpenId(string openId = "")
        {
            //string oauthType = "Himall.Plugin.OAuth.WeiXin";//默认小程序微信端登录
            string unionid = "";
            var wxuserinfo = ApiHelper.GetAppletUserInfo(Request);
            if (wxuserinfo != null)
            {
                unionid = wxuserinfo.unionId;
            }
            if (!string.IsNullOrEmpty(openId))
            {
                UserMemberInfo member = new UserMemberInfo();
                if (!string.IsNullOrWhiteSpace(unionid))
                {
                    member = Application.MemberApplication.GetMemberByUnionIdAndProvider(O2OSmallProgServiceProvider, unionid) ?? new UserMemberInfo();
                }

                if (member.Id == 0)
                    member = Application.MemberApplication.GetMemberByOpenId(O2OSmallProgServiceProvider, openId) ?? new UserMemberInfo();
                if (member.Id > 0)
                {
                    //信任登录并且已绑定             
                    string memberId = UserCookieEncryptHelper.Encrypt(member.Id, CookieKeysCollection.USERROLE_USER);
                    return GetMember(member, openId);
                }
                else
                {
                    //信任登录未绑定
                    return Json(ErrorResult<dynamic>("未绑定商城帐号"));
                }
            }
            return Json(ErrorResult<dynamic>("登录失败"));
        }
        /// <summary>
        ///账号密码登录
        /// </summary>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetLoginByUserName(string openId = "", string userName = "", string password = "", string nickName = "")
        {
            if (!string.IsNullOrEmpty(openId) && !string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                UserMemberInfo member = null;

                try
                {
                    member = MemberApplication.Login(userName, password);
                }
                catch (Exception ex)
                {
                    return Json(ErrorResult<dynamic>(ex.Message));
                }
                if (member == null)
                {
                    return Json(ErrorResult<dynamic>("用户名或密码错误"));
                }
                else
                {
                    if (member != null)
                    {
                        bool IsUpdate = true;
                        //如果不是一键登录的 则绑定openId
                        if (!string.IsNullOrEmpty(openId))
                        {
                            MemberOpenIdInfo memberOpenIdInfo = new MemberOpenIdInfo()
                            {
                                UserId = member.Id,
                                OpenId = openId,
                                ServiceProvider = O2OSmallProgServiceProvider,
                                AppIdType = Himall.Model.MemberOpenIdInfo.AppIdTypeEnum.Normal,
                                UnionId = string.Empty
                            };
                            MemberApplication.UpdateOpenIdBindMember(memberOpenIdInfo);
                            //    }
                            //}
                        }

                        string memberId = UserCookieEncryptHelper.Encrypt(member.Id, CookieKeysCollection.USERROLE_USER);
                        var prom = DistributionApplication.GetPromoterByUserId(member.Id);
                        return GetMember(member, openId);
                    }
                }
            }
            return Json(ErrorResult<dynamic>("登录失败"));
        }
        /// <summary>
        ///一键登录
        /// </summary>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetQuickLogin(string openId = "", string nickName = "", string headImage = "")
        {
            var wxuserinfo = ApiHelper.GetAppletUserInfo(Request);
            string unionid = string.Empty;
            if (wxuserinfo != null)
            {
                unionid = wxuserinfo.unionId;
            }
            string unionopenid = "";
            if (!string.IsNullOrEmpty(openId))
            {
                string username = DateTime.Now.ToString("yyMMddHHmmssffffff");
                var member = MemberApplication.QuickRegister(username, string.Empty, nickName, O2OSmallProgServiceProvider, openId, unionid, unionopenid: unionopenid, headImage: headImage);
                //string memberId = UserCookieEncryptHelper.Encrypt(member.Id, CookieKeysCollection.USERROLE_USER);
                //var prom = DistributionApplication.GetPromoterByUserId(member.Id);
                return GetMember(member, openId);

            }
            return Json(ErrorResult<dynamic>("登录失败"));
        }
        /// <summary>
        ///退出登录
        /// </summary>
        /// <returns></returns>
        public JsonResult<Result<int>> GetLogout(string openId)
        {
            if (!string.IsNullOrEmpty(openId))
            {
                var member = MemberApplication.GetMemberByOpenId(O2OSmallProgServiceProvider, openId);
                if (member == CurrentUser)
                {
                    var cacheKey = WebHelper.GetCookie(CookieKeysCollection.HIMALL_USER);
                    if (!string.IsNullOrWhiteSpace(cacheKey))
                    {
                        //_iMemberService.DeleteMemberOpenId(userid, string.Empty);
                        WebHelper.DeleteCookie(CookieKeysCollection.HIMALL_USER);
                        WebHelper.DeleteCookie(CookieKeysCollection.SELLER_MANAGER);
                    }
                    //记录主动退出符号
                    WebHelper.SetCookie(CookieKeysCollection.HIMALL_ACTIVELOGOUT, "1", DateTime.MaxValue);
                    return JsonResult<int>(msg: "退出成功");
                }
            }
            return Json(ErrorResult<int>("退出失败"));
        }

        private JsonResult<Result<dynamic>> GetMember(UserMemberInfo member, string openId)
        {
            var model = MemberApplication.GetUserCenterModel(member.Id);
            var memgradeid = MemberApplication.GetMemberGradeByUserId(member.Id);
            string gradeName = model.GradeName == null ? "" : model.GradeName;
            //获取会员等待付款订单数
            int waitPayCount = Convert.ToInt32(model.WaitPayOrders);
            //获取会员待收货数量
            int waitFinishCount = Convert.ToInt32(model.WaitReceivingOrders);
            //获取会员待发货数量
            int waitSendCount = Convert.ToInt32(model.WaitDeliveryOrders);
            //获取会员待评论数量
            int waitReviewCount = Convert.ToInt32(model.WaitEvaluationOrders);
            //获取会员售后数量
            int afterSalesCount = model.RefundCount;
            //获取会员未使用的优惠券数目
            int couponsCount = model.UserCoupon;
            return JsonResult<dynamic>(new
            {
                couponsCount = couponsCount,
                picture = Core.HimallIO.GetRomoteImagePath(member.Photo),
                points = model.Intergral,
                waitPayCount = waitPayCount,
                waitSendCount = waitSendCount,
                waitFinishCount = waitFinishCount,
                waitReviewCount = waitReviewCount,
                afterSalesCount = afterSalesCount,
                realName = string.IsNullOrEmpty(member.ShowNick) ? (string.IsNullOrEmpty(member.RealName) ? member.UserName : member.RealName) : member.ShowNick,
                gradeId = memgradeid,
                gradeName = gradeName,
                UserName = member.UserName,
                UserId = member.Id,
                OpenId = openId,
                ServicePhone = Application.SiteSettingApplication.GetSiteSettings().SitePhone
            });
        }

        public JsonResult<Result<WeiXinOpenIdModel>> GetOpenId(string appid, string secret, string js_code)
        {
            string requestUrl = "https://api.weixin.qq.com/sns/jscode2session?appid=" + appid + "&secret=" + secret + "&js_code=" + js_code + "&grant_type=authorization_code";
            string result = "";
            var response = Himall.Core.Helper.WebHelper.GetURLResponse(requestUrl);
            using (Stream receiveStream = response.GetResponseStream())
            {

                using (StreamReader readerOfStream = new StreamReader(receiveStream, System.Text.Encoding.UTF8))
                {
                    result = readerOfStream.ReadToEnd();
                }
            }
            var openModel = JsonConvert.DeserializeObject<WeiXinOpenIdModel>(result);
            return JsonResult(openModel);
        }
    }
}
