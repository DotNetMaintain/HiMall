using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Helper;
using Himall.DTO;
using Himall.IServices;
using Himall.Model;
using Himall.Model.Models;
using Himall.Web.Areas.Web;
using Himall.Web.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Transactions;
using System.Web.Mvc;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class LoginController : BaseMobileTemplatesController
    {
        IMemberService _iMemberService;
        IMessageService _iMessageService;
        IManagerService _iManagerService;
        private IBonusService _iBonusService;

        public LoginController(IMemberService iMemberService, IMessageService iMessageService, IManagerService iManagerService)
        {

            _iMemberService = iMemberService;
            _iMessageService = iMessageService;
            _iManagerService = iManagerService;
        }
        // GET: Mobile/Login
        public ActionResult Entrance(string returnUrl, string openId, string serviceProvider, string nickName, string headimgurl, string realName, string app, string unionid = null)
        {
            return View(CurrentSiteSetting);
        }
        public ActionResult ForgotPassword()
        {
            return View();
        }
        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SendCode(string pluginId, string destination)
        {
            //bool bo = MemberApplication.SendCode(pluginId, destination, destination, "长生树");
            //return Json(new Result() { success = bo });


            var timeout = CacheKeyCollection.MemberPluginCheckTime(destination, pluginId);
            if (Core.Cache.Exists(timeout))
            {
                return Json(new { success = false, msg = "60秒内只允许请求一次，请稍后重试!" });
            }
            var checkCode = new Random().Next(10000, 99999);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            if (pluginId.ToLower().Contains("email"))
            {
                cacheTimeout = DateTime.Now.AddHours(24);
            }
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(destination, pluginId), checkCode.ToString(), cacheTimeout);
            var user = new Himall.Core.Plugins.Message.MessageUserInfo() { UserName = destination, SiteName = CurrentSiteSetting.SiteName, CheckCode = checkCode.ToString() };
            _iMessageService.SendMessageCode(destination, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheckTime(destination, pluginId), "0", DateTime.Now.AddSeconds(55));

            return Json(new { success = true });
        }
        [HttpPost]
        public JsonResult Skip(string MemberPhone, string checkcode, string serviceProvider, string openId, string nickName, string realName, string headimgurl, string app, MemberOpenIdInfo.AppIdTypeEnum appidtype = MemberOpenIdInfo.AppIdTypeEnum.Normal, string unionid = null, string sex = null, string city = null, string province = null)
        {
            bool result = CheckCode(checkcode, MemberPhone);
            Core.Log.Debug("result" + result);
            if (result == false)
                return Json<dynamic>(success: false, msg: "验证码错误");
            else
            {
                int num = 0;
                string username = DateTime.Now.ToString("yyMMddHHmmssffffff");   //TODO:DZY[150916]未使用，在方法内会重新生成
                nickName = System.Web.HttpUtility.UrlDecode(nickName);
                realName = System.Web.HttpUtility.UrlDecode(realName);
                headimgurl = System.Web.HttpUtility.UrlDecode(headimgurl);
                province = System.Web.HttpUtility.UrlDecode(province);
                city = System.Web.HttpUtility.UrlDecode(city);
                Core.Log.Debug("city结果" + city);
                //UserMemberInfo memberInfos = _iMemberService.GetMemberByUnionId(openId);
                UserMemberInfo memberInfo = _iMemberService.GetMemberByphone(MemberPhone);
                if (memberInfo == null)
                {
                    Core.Log.Debug("openId结果" + openId);
                    Core.Log.Debug("unionid结果" + unionid);
                    memberInfo = _iMemberService.QuickRegisters(MemberPhone, username, realName, nickName, serviceProvider, openId, unionid, sex, headimgurl, appidtype, null, city, province);
                    Core.Log.Debug("memberInfo结果" + memberInfo.Id);
                    /////登录报错
                    ////TODO:ZJT  在用户注册的时候，检查此用户是否存在OpenId是否存在红包，存在则添加到用户预存款里
                    //_iBonusService.DepositToRegister(memberInfo.Id);
                    //Core.Log.Debug("memberInfo结果2" + memberInfo);
                    //////用户注册的时候，检查是否开启注册领取优惠券活动，存在自动添加到用户预存款里
                    //num = CouponApplication.RegisterSendCoupon(memberInfo.Id, memberInfo.UserName);
                    ////Core.Log.Debug("num结果" + num);
                }
                if (memberInfo != null)
                {
                    base.SetUserLoginCookie(memberInfo.Id);
                    Core.Log.Debug("memberInfo结果" + memberInfo.Id);
                    Application.MemberApplication.UpdateLastLoginDate(memberInfo.Id);
                }
                //else if (memberInfos != null)
                //{
                //    base.SetUserLoginCookie(memberInfos.Id);
                //    Application.MemberApplication.UpdateLastLoginDate(memberInfos.Id);
                //}

                WebHelper.SetCookie(CookieKeysCollection.HIMALL_ACTIVELOGOUT, "0", DateTime.MaxValue);
                Core.Log.Debug("num结果" + num);
                if (app == "appTel")
                {
                    Core.Log.Debug(MemberPhone);
                    return Json<dynamic>(success: true, data: new { num = num, app = app });
                }
                else
                {
                    Core.Log.Debug(MemberPhone);
                    return Json<dynamic>(success: true, data: new { num = num, app = "" });
                }
            }
        }


        [HttpPost]
        public JsonResult BindUser(string MemberPhone, string checkcode, string headimgurl, string serviceProvider, string openId, Model.MemberOpenIdInfo.AppIdTypeEnum appidtype = MemberOpenIdInfo.AppIdTypeEnum.Normal, string unionid = null, string sex = null, string city = null, string province = null, string country = null, string nickName = null)
        {

            CheckCode(checkcode, MemberPhone);
            var service = _iMemberService;
            var member = service.Login(MemberPhone, checkcode);
            if (member == null)
                throw new Himall.Core.HimallException("用户名和密码不匹配");

            //Log.Debug("BindUser unionid=" + (string.IsNullOrWhiteSpace(unionid) ? "null" : unionid));
            headimgurl = System.Web.HttpUtility.UrlDecode(headimgurl);
            nickName = System.Web.HttpUtility.UrlDecode(nickName);
            city = System.Web.HttpUtility.UrlDecode(city);
            province = System.Web.HttpUtility.UrlDecode(province);
            OAuthUserModel model = new OAuthUserModel()
            {
                AppIdType = appidtype,
                UserId = member.Id,
                LoginProvider = serviceProvider,
                OpenId = openId,
                Headimgurl = headimgurl,
                UnionId = unionid,
                Sex = sex,
                NickName = nickName,
                City = city,
                Province = province
            };
            service.BindMember(model);

            base.SetUserLoginCookie(member.Id);
            WebHelper.SetCookie(CookieKeysCollection.HIMALL_ACTIVELOGOUT, "0", DateTime.MaxValue);
            //SellerLoginIn(username, password);
            BizAfterLogin.Run(member.Id);//执行登录后初始化相关操作 

            return Json(new { success = true });
        }



        [HttpPost]
        public JsonResult Index(string MemberPhone, string checkcode)
        {
            try
            {
                //检查输入合法性
                CheckCode(checkcode, MemberPhone);
                var member = _iMemberService.Login(MemberPhone, "123456");
                if (member == null)
                {
                    member = _iMemberService.Register(MemberPhone, "123456", MemberPhone, "", 0);
                }
                if (member == null)
                {
                    throw new LoginException("用户名和密码不匹配", LoginException.ErrorTypes.PasswordError);
                }

                if (PlatformType == Core.PlatformType.WeiXin)
                    base.SetUserLoginCookie(member.Id);
                else
                    base.SetUserLoginCookie(member.Id, DateTime.MaxValue);

                WebHelper.SetCookie(CookieKeysCollection.HIMALL_ACTIVELOGOUT, "0", DateTime.MaxValue);
                SellerLoginIn(MemberPhone, checkcode);

                BizAfterLogin.Run(member.Id);//执行登录后初始化相关操作 
                return Json(new { success = true, memberId = member.Id });
            }
            catch (LoginException ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
            catch (HimallException ex)
            {
                return Json(new { success = false, msg = ex.Message });
            }
            catch (Exception ex)
            {
                Core.Log.Error("用户" + MemberPhone + "登录时发生异常", ex);
                return Json(new { success = false, msg = "未知错误" });
            }
        }

        void CheckInput(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new LoginException("请填写用户名", LoginException.ErrorTypes.UsernameError);

            if (string.IsNullOrWhiteSpace(password))
                throw new LoginException("请填写密码", LoginException.ErrorTypes.PasswordError);

        }
        [HttpGet]
        public JsonResult CheckLogin()
        {
            var userId = base.UserId;
            if (userId != 0)
            {
                //_iMemberService.DeleteMemberOpenId(userid, string.Empty);
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="checkCode"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckUserName(string contact, string checkCode)
        {
            var service = _iMemberService;
            string systemCheckCode = Session["checkCode"] as string;
            if (systemCheckCode.ToLower() != checkCode.ToLower())
                throw new Core.HimallException("验证码错误");
            var userMenberInfo = service.GetMemberByContactInfo(contact);
            if (userMenberInfo == null)
            {
                throw new Core.HimallException("该手机号或邮箱未绑定账号");
            }

            #region 发送验证码
            var pluginId = "";
            if (Core.Helper.ValidateHelper.IsMobile(contact))
            {
                pluginId = "Himall.Plugin.Message.SMS";
            }
            if (!string.IsNullOrEmpty(contact) && Core.Helper.ValidateHelper.IsEmail(contact))
            {
                pluginId = "Himall.Plugin.Message.Email";
            }

            var timeout = CacheKeyCollection.MemberPluginCheckTime(contact, pluginId);
            if (Core.Cache.Exists(timeout))
            {
                return Json(new { success = false, msg = "60秒内只允许请求一次，请稍后重试!", url = "FillCode", contact = contact });
            }
            var Code = new Random().Next(10000, 99999);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            if (pluginId.ToLower().Contains("email"))
            {
                cacheTimeout = DateTime.Now.AddHours(24);
            }
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(contact, pluginId), Code.ToString(), cacheTimeout);
            var user = new Himall.Core.Plugins.Message.MessageUserInfo() { UserName = contact, SiteName = CurrentSiteSetting.SiteName, CheckCode = Code.ToString() };
            _iMessageService.SendMessageCode(contact, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheckTime(contact, pluginId), "0", DateTime.Now.AddSeconds(55));

            #endregion

            return Json(new { success = true, data = new { contact = contact, url = "FillCode" } });
        }


        //public ActionResult FillCode(string contact)
        //{
        //    ViewBag.Contact = contact;

        //    return View();
        //}

        /// <summary>
        /// 重新获取验证码
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public JsonResult SendCode(string contact)
        {
            var pluginId = "";
            if (Core.Helper.ValidateHelper.IsMobile(contact))
            {
                pluginId = "Himall.Plugin.Message.SMS";
            }
            if (!string.IsNullOrEmpty(contact) && Core.Helper.ValidateHelper.IsEmail(contact))
            {
                pluginId = "Himall.Plugin.Message.Email";
            }

            var timeout = CacheKeyCollection.MemberPluginCheckTime(contact, pluginId);
            if (Core.Cache.Exists(timeout))
            {
                return Json(new { success = false, msg = "60秒内只允许请求一次，请稍后重试!" });
            }
            var checkCode = new Random().Next(10000, 99999);
            var cacheTimeout = DateTime.Now.AddMinutes(15);
            if (pluginId.ToLower().Contains("email"))
            {
                cacheTimeout = DateTime.Now.AddHours(24);
            }
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheck(contact, pluginId), checkCode.ToString(), cacheTimeout);
            var user = new Himall.Core.Plugins.Message.MessageUserInfo() { UserName = contact, SiteName = CurrentSiteSetting.SiteName, CheckCode = checkCode.ToString() };
            _iMessageService.SendMessageCode(contact, pluginId, user);
            Core.Cache.Insert(CacheKeyCollection.MemberPluginCheckTime(contact, pluginId), "0", DateTime.Now.AddSeconds(55));

            return Json(new { success = true });
        }

        /// <summary>
        /// 验证验证码
        /// </summary>
        /// <param name="code"></param>
        /// <param name="contact"></param>
        /// <returns></returns>
        public bool CheckCode(string code, string contact)
        {
            var pluginId = "";
            if (Core.Helper.ValidateHelper.IsMobile(contact))
            {
                pluginId = "Himall.Plugin.Message.SMS";
            }
            if (!string.IsNullOrEmpty(contact) && Core.Helper.ValidateHelper.IsEmail(contact))
            {
                pluginId = "Himall.Plugin.Message.Email";
            }
            ViewBag.Contact = contact;
            var cache = CacheKeyCollection.MemberPluginCheck(contact, pluginId);
            var cacheCode = Core.Cache.Get<string>(cache);
            if (cacheCode != null && cacheCode == code)
            {
                var FdCache = CacheKeyCollection.MemberFindPwd(contact);
                Core.Cache.Insert(FdCache, contact, DateTime.Now.AddMinutes(10));
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 修改密码页
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public ActionResult ResetPwd(string contact)
        {
            //判断是否通过验证
            var FdCache = CacheKeyCollection.MemberFindPwd(contact);
            if (!Core.Cache.Exists(FdCache))
            {
                Response.Redirect("ForgotPassword");
            }
            ViewBag.Contact = contact;
            return View();
        }
        public ActionResult GoResetResult()
        {
            return View();
        }
        public JsonResult ModPwd(string contact, string password, string repeatPassword)
        {
            var userMenberInfo = _iMemberService.GetMemberByContactInfo(contact);
            if (userMenberInfo == null)
            {
                throw new Core.HimallException("该手机号或邮箱未绑定账号");
            }
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(repeatPassword))
            {
                return Json(new Result() { success = false, msg = "密码不能为空！" });
            }
            if (!password.Trim().Equals(repeatPassword.Trim()))
            {
                return Json(new Result() { success = false, msg = "两次密码不一致！" });
            }
            _iMemberService.ChangePassword(userMenberInfo.Id, password);

            return Json(new { success = true, msg = "密码修改成功！", data = new { url = "GoResetResult" } });
        }
        private ManagerInfo SellerLoginIn(string MemberPhone, string checkcode, bool keep = false)
        {
            var seller = _iManagerService.Login(MemberPhone, checkcode);
            if (seller == null)
            {
                return null;
            }

            if (keep)
            {
                base.SetSellerAdminLoginCookie(seller.Id, DateTime.Now.AddYears(1));
            }
            else
            {
                base.SetSellerAdminLoginCookie(seller.Id, DateTime.MaxValue);
            }
            return seller;
        }

        public JsonResult getByOpenId(string openid)
        {
            MyMemberOpenId member = _iManagerService.GetByOpenId(openid);
            return Json(member.UserId, JsonRequestBehavior.AllowGet);

        }
        public JsonResult getMemberByOpenID(string openid)
        {
            UserMember userMemberInfo = _iManagerService.GetMemberByOpenID(openid);
            return Json(userMemberInfo, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getMemberByUserid(long userId)
        {
            UserMember userMemberInfo = _iManagerService.GetMemberByUserid(userId);
            var state = "";
            if (userMemberInfo != null)
            {
                state = userMemberInfo.Nick;
            }
            return Json(state, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getOrderByUserIdAndOrderStatus(long userId)
        {
            MyOrder order = _iManagerService.GetOrderByUserIdAndOrderStatus(userId);
            var state=0;
            if (order!=null) {
                state = Convert.ToInt32(order.OrderStatus);
            }
            return Json(state, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getOpenIdByUserId(long userId)
        {
            MyMemberOpenId member = _iManagerService.getOpenIdByUserId(userId);
            return Json(member.OpenId, JsonRequestBehavior.AllowGet);
        }
    }
}