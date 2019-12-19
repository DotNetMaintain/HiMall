using Himall.Core;
using Himall.Core.Plugins;
using Himall.Core.Plugins.Payment;
using Himall.DTO;
using Himall.IServices;
using Himall.DTO.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class CapitalController : BaseAdminController
    {
        IMemberCapitalService _iMemberCapitalService;
        IMemberService _iMemberService;
        ISiteSettingService _iSiteSettingService;
        IOperationLogService _iOperationLogService;
        private const string PLUGIN_PAYMENT_ALIPAY = "Himall.Plugin.Payment.Alipay";
        public CapitalController(IMemberCapitalService iMemberCapitalService,
            IMemberService iMemberService,
            ISiteSettingService iSiteSettingService,
            IOperationLogService iOperationLogService)
        {
            _iMemberCapitalService = iMemberCapitalService;
            _iMemberService = iMemberService;
            _iSiteSettingService = iSiteSettingService;
            _iOperationLogService = iOperationLogService;
        }
        // GET: Admin/Capital
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetMemberCapitals(string user, int page, int rows)
        {
            var capitalService = _iMemberCapitalService;
            var memberService = _iMemberService;
            long? membid = null;
            if (!string.IsNullOrWhiteSpace(user))
            {
                var memberInfo = memberService.GetMemberByName(user) ?? new UserMemberInfo() { Id = 0 };
                membid = memberInfo.Id;
            }

            var query = new CapitalQuery
            {
                PageNo = page,
                PageSize = rows,
                Sort = "Balance",
                memberId = membid
            };
            var pagemodel = capitalService.GetCapitals(query);
            var model = pagemodel.Models.ToList().Select(e =>
            {
                var member = memberService.GetMember(e.MemId);
                var capitalModel = new CapitalModel()
                {
                    Balance = e.Balance.Value,
                    ChargeAmount = e.ChargeAmount.HasValue ? e.ChargeAmount.Value : 0.00M,
                    FreezeAmount = e.FreezeAmount.HasValue ? e.FreezeAmount.Value : 0.00M,
                    Id = e.Id,
                    UserId = e.MemId
                };
                if (member != null)
                {
                    capitalModel.UserCode = member.UserName;
                    capitalModel.UserName = string.IsNullOrWhiteSpace(member.RealName) ? member.UserName : member.RealName;
                }
                return capitalModel;
            });
            var models = new DataGridModel<CapitalModel>
            {
                rows = model,
                total = pagemodel.Total
            };
            return Json(models);
        }
        public ActionResult Detail(long id)
        {
            ViewBag.UserId = id;
            return View();
        }
        public ActionResult WithDraw()
        {
            return View();
        }
        /// <summary>
        /// 支付宝提现管理
        /// </summary>
        /// <returns></returns>
        public ActionResult AlipayWithDraw()
        {
            return View();
        }
        public JsonResult List(CapitalDetailInfo.CapitalDetailType capitalType, long userid, string startTime, string endTime, int page, int rows)
        {
            var capitalService = _iMemberCapitalService;

            var query = new CapitalDetailQuery
            {
                memberId = userid,
                capitalType = capitalType,
                PageSize = rows,
                PageNo = page
            };
            if (!string.IsNullOrWhiteSpace(startTime))
            {
                query.startTime = DateTime.Parse(startTime);
            }
            if (!string.IsNullOrWhiteSpace(endTime))
            {
                query.endTime = DateTime.Parse(endTime).AddDays(1).AddSeconds(-1);
            }
            var pageMode = capitalService.GetCapitalDetails(query);
            var model = pageMode.Models.ToList().Select(e => new CapitalDetailModel
            {
                Id = e.Id,
                Amount = e.Amount,
                CapitalID = e.CapitalID,
                CreateTime = e.CreateTime.Value.ToString(),
                SourceData = e.SourceData,
                SourceType = e.SourceType,
                Remark = GetCapitalRemark(e.SourceType, e.SourceData, e.Id.ToString(), e.Remark),
                PayWay = GetPayWay(e.SourceData)
            }).ToList();

            var models = new DataGridModel<CapitalDetailModel>
            {
                rows = model,
                total = pageMode.Total
            };
            return Json(models);
        }

        private string GetPayWay(string sourceData)
        {
            string result = string.Empty;

            long cid = 0;
            if (long.TryParse(sourceData, out cid))
            {
                var charge = _iMemberCapitalService.GetChargeDetail(cid);
                if(charge != null)
                {
                    result = charge.ChargeWay;
                }
            }
            return result;
        }

        public string GetCapitalRemark(CapitalDetailInfo.CapitalDetailType sourceType, string sourceData, string id, string remark)
        {
            if (sourceType == CapitalDetailInfo.CapitalDetailType.Brokerage)
            {
                return remark;
            }
            //else if(sourceType == CapitalDetailInfo.CapitalDetailType.ChargeAmount)
            //{
            //    return sourceType.ToDescription() + ",单号：" + (string.IsNullOrWhiteSpace(id) ? sourceData : id) + (string.IsNullOrWhiteSpace(remark) ? "" : "(" + remark + ")");
            //}
            else
            {
                return sourceType.ToDescription() + ",单号：" + (string.IsNullOrWhiteSpace(sourceData) ? id : sourceData) + (string.IsNullOrWhiteSpace(remark) ? "" : "(" + remark + ")");
            }
        }

        public JsonResult ApplyWithDrawListByUser(long userid, Himall.CommonModel.UserWithdrawType? applyType, int page, int rows)
        {
            var capitalService = _iMemberCapitalService;
            var query = new ApplyWithDrawQuery
            {
                memberId = userid,
                ApplyType = applyType,
                PageSize = rows,
                PageNo = page
            };
            var pageMode = capitalService.GetApplyWithDraw(query);
            var model = pageMode.Models.ToList().Select(e =>
            {
                string applyStatus = string.Empty;
                if (e.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.PayFail
                    || e.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.WaitConfirm
                    )
                {
                    applyStatus = "提现中";
                }
                else if (e.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.Refuse)
                {
                    applyStatus = "提现失败";
                }
                else if (e.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.WithDrawSuccess)
                {
                    applyStatus = "提现成功";
                }
                return new ApplyWithDrawModel
                {
                    Id = e.Id,
                    ApplyAmount = e.ApplyAmount,
                    ApplyStatus = e.ApplyStatus,
                    ApplyStatusDesc = applyStatus,
                    ApplyTime = e.ApplyTime.ToString(),
                    ApplyType = e.ApplyType
                };
            });
            var models = new DataGridModel<ApplyWithDrawModel>
            {
                rows = model,
                total = pageMode.Total
            };
            return Json(models);
        }

        public JsonResult ApplyWithDrawList(ApplyWithDrawInfo.ApplyWithDrawStatus capitalType, Himall.CommonModel.UserWithdrawType? applyType, string withdrawno, string user, int page, int rows)
        {
            var capitalService = _iMemberCapitalService;
            var memberService = _iMemberService;
            long? membid = null;
            if (!string.IsNullOrWhiteSpace(user))
            {
                var memberInfo = memberService.GetMemberByName(user) ?? new UserMemberInfo() { Id = 0 };
                membid = memberInfo.Id;
            }
            var query = new ApplyWithDrawQuery
            {
                memberId = membid,
                ApplyType = applyType,
                PageSize = rows,
                PageNo = page,
                withDrawStatus = capitalType
            };
            if (!string.IsNullOrWhiteSpace(withdrawno))
            {
                query.withDrawNo = long.Parse(withdrawno);
            }
            var pageMode = capitalService.GetApplyWithDraw(query);
            var model = pageMode.Models.ToList().Select(e =>
            {
                string applyStatus = e.ApplyStatus.ToDescription();
                var mem = memberService.GetMember(e.MemId);
                return new ApplyWithDrawModel
                {
                    Id = e.Id,
                    ApplyAmount = e.ApplyAmount,
                    ApplyStatus = e.ApplyStatus,
                    ApplyStatusDesc = applyStatus,
                    ApplyTime = e.ApplyTime.ToString(),
                    NickName = e.NickName,
                    MemberName = mem.UserName,
                    ConfirmTime = e.ConfirmTime.ToString(),
                    MemId = e.MemId,
                    OpUser = e.OpUser,
                    PayNo = e.PayNo,
                    PayTime = e.PayTime.ToString(),
                    Remark = string.IsNullOrEmpty(e.Remark) ? string.Empty : e.Remark
                };
            });
            var models = new DataGridModel<ApplyWithDrawModel>
            {
                rows = model,
                total = pageMode.Total
            };
            return Json(models);
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ConfirmApply(long id, ApplyWithDrawInfo.ApplyWithDrawStatus comfirmStatus, string remark)
        {
            var service = _iMemberCapitalService;
            var status = comfirmStatus;
            var model = service.GetApplyWithDrawInfo(id);
            if (status == ApplyWithDrawInfo.ApplyWithDrawStatus.Refuse)
            {
                service.RefuseApplyWithDraw(id, status, CurrentManager.UserName, remark);
                //操作日志
                _iOperationLogService.AddPlatformOperationLog(
          new LogInfo
          {
              Date = DateTime.Now,
              Description = string.Format("会员提现审核拒绝，会员Id={0},状态为：{1}, 说明是：{2}", model.MemId,
              status, remark),
              IPAddress = Request.UserHostAddress,
              PageUrl = "/Admin/Capital/WithDraw",
              UserName = CurrentManager.UserName,
              ShopId = 0

          });
                return Json(new Result { success = true, msg = "审核成功！" });
            }
            else
            {
                if (model.ApplyType == CommonModel.UserWithdrawType.ALiPay)
                {
                    #region 支付宝提现
                    if (model.ApplyStatus == ApplyWithDrawInfo.ApplyWithDrawStatus.PayPending)
                    {
                        return Json(new Result { success = false, msg = "等待第三方处理中，如有误操作，请先取消后再进行付款操作！" });
                    }

                    var plugins = PluginsManagement.GetPlugins<IPaymentPlugin>(true).FirstOrDefault(e => e.PluginInfo.PluginId == PLUGIN_PAYMENT_ALIPAY);
                    if (plugins != null)
                    {
                        try
                        {
                            string webRoot = CurrentUrlHelper.CurrentUrlNoPort();
                            //异步通知地址
                            string payNotify = webRoot + "/Pay/EnterpriseNotify/{0}?outid={1}";

                            EnterprisePayPara para = new EnterprisePayPara()
                            {
                                amount = model.ApplyAmount,
                                check_name = false,
                                openid = model.OpenId,
                                re_user_name = model.NickName,
                                out_trade_no = model.Id.ToString(),
                                desc = "提现",
                                notify_url = string.Format(payNotify, EncodePaymentId(plugins.PluginInfo.PluginId), model.Id.ToString())
                            };
                            PaymentInfo result = plugins.Biz.EnterprisePay(para);
                            ApplyWithDrawInfo info = new ApplyWithDrawInfo
                            {
                                PayNo = result.TradNo,
                                ApplyStatus = ApplyWithDrawInfo.ApplyWithDrawStatus.PayPending,
                                PayTime = result.TradeTime.HasValue ? result.TradeTime.Value : DateTime.Now,
                                ConfirmTime = DateTime.Now,
                                OpUser = CurrentManager.UserName,
                                ApplyAmount = model.ApplyAmount,
                                Id = model.Id,
                                Remark = remark
                            };
                            //Log.Debug("提现:" + info.PayNo);
                            service.ConfirmApplyWithDraw(info);

                            //操作日志
                            _iOperationLogService.AddPlatformOperationLog(
                              new LogInfo
                              {
                                  Date = DateTime.Now,
                                  Description = string.Format("会员提现审核成功，会员Id={0},状态为：{1}, 说明是：{2}", model.MemId,
                                  status, remark),
                                  IPAddress = Request.UserHostAddress,
                                  PageUrl = "/Admin/Capital/WithDraw",
                                  UserName = CurrentManager.UserName,
                                  ShopId = 0

                              });
                            //ResponseContentWhenFinished 会回传跳转付款的链接
                            return Json(new Result { success = true, msg = "审核操作成功", status = 2, data = result.ResponseContentWhenFinished });
                        }
                        catch (PluginException pex)
                        {
                            //插件异常，直接返回错误信息
                            Log.Error("调用企业付款接口异常：" + pex.Message);
                            //操作日志
                            _iOperationLogService.AddPlatformOperationLog(
                              new LogInfo
                              {
                                  Date = DateTime.Now,
                                  Description = string.Format("会员提现审核失败，会员Id={0},状态为：{1}, 说明是：{2}", model.MemId,
                                  status, remark),
                                  IPAddress = Request.UserHostAddress,
                                  PageUrl = "/Admin/Capital/WithDraw",
                                  UserName = CurrentManager.UserName,
                                  ShopId = 0

                              });
                            return Json(new Result { success = false, msg = pex.Message });
                        }
                        catch (Exception ex)
                        {
                            Log.Error("提现审核异常：" + ex.Message);
                            return Json(new Result { success = false, msg = "付款接口异常" });
                        }
                    }
                    else
                    {
                        return Json(new Result { success = false, msg = "未找到支付插件" });
                    }
                    #endregion
                }
                else
                {
                    #region 微信提现
                    var plugins = PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(e => e.PluginInfo.PluginId.ToLower().Contains("weixin")).FirstOrDefault();
                    if (plugins != null)
                    {
                        try
                        {
                            EnterprisePayPara para = new EnterprisePayPara()
                            {
                                amount = model.ApplyAmount,
                                check_name = false,
                                openid = model.OpenId,
                                out_trade_no = model.Id.ToString(),
                                desc = "提现"
                            };
                            PaymentInfo result = plugins.Biz.EnterprisePay(para);
                            ApplyWithDrawInfo info = new ApplyWithDrawInfo
                            {
                                PayNo = result.TradNo,
                                ApplyStatus = ApplyWithDrawInfo.ApplyWithDrawStatus.WithDrawSuccess,
                                Remark = plugins.PluginInfo.Description,
                                PayTime = result.TradeTime.HasValue ? result.TradeTime.Value : DateTime.Now,
                                ConfirmTime = DateTime.Now,
                                OpUser = CurrentManager.UserName,
                                ApplyAmount = model.ApplyAmount,
                                Id = model.Id
                            };
                            //Log.Debug("提现:" + info.PayNo);
                            service.ConfirmApplyWithDraw(info);

                            //操作日志
                            _iOperationLogService.AddPlatformOperationLog(
                              new LogInfo
                              {
                                  Date = DateTime.Now,
                                  Description = string.Format("会员提现审核成功，会员Id={0},状态为：{1}, 说明是：{2}", model.MemId,
                                  status, remark),
                                  IPAddress = Request.UserHostAddress,
                                  PageUrl = "/Admin/Capital/WithDraw",
                                  UserName = CurrentManager.UserName,
                                  ShopId = 0

                              });
                        }
                        catch (PluginException pex)
                        {//插件异常，直接返回错误信息
                            Log.Error("调用企业付款接口异常：" + pex.Message);
                            //操作日志
                            _iOperationLogService.AddPlatformOperationLog(
                              new LogInfo
                              {
                                  Date = DateTime.Now,
                                  Description = string.Format("会员提现审核失败，会员Id={0},状态为：{1}, 说明是：{2}", model.MemId,
                                  status, remark),
                                  IPAddress = Request.UserHostAddress,
                                  PageUrl = "/Admin/Capital/WithDraw",
                                  UserName = CurrentManager.UserName,
                                  ShopId = 0

                              });
                            return Json(new Result { success = false, msg = pex.Message });
                        }
                        catch (Exception ex)
                        {
                            Log.Error("提现审核异常：" + ex.Message);
                            ApplyWithDrawInfo info = new ApplyWithDrawInfo
                            {
                                ApplyStatus = ApplyWithDrawInfo.ApplyWithDrawStatus.PayFail,
                                Remark = plugins.PluginInfo.Description,
                                ConfirmTime = DateTime.Now,
                                OpUser = CurrentManager.UserName,
                                ApplyAmount = model.ApplyAmount,
                                Id = model.Id
                            };
                            service.ConfirmApplyWithDraw(info);

                            //操作日志
                            _iOperationLogService.AddPlatformOperationLog(
                              new LogInfo
                              {
                                  Date = DateTime.Now,
                                  Description = string.Format("会员提现审核失败，会员Id={0},状态为：{1}, 说明是：{2}", model.MemId,
                                  status, remark),
                                  IPAddress = Request.UserHostAddress,
                                  PageUrl = "/Admin/Capital/WithDraw",
                                  UserName = CurrentManager.UserName,
                                  ShopId = 0

                              });

                            return Json(new Result { success = false, msg = "付款接口异常" });
                        }
                    }
                    else
                    {
                        return Json(new Result { success = false, msg = "未找到支付插件" });
                    }
                    #endregion
                }
            }

            return Json(new Result { success = true, msg = "审核操作成功" });
        }
        /// <summary>
        /// 对PaymentId进行加密（因为PaymentId中包含小数点"."，因此进行编码替换）
        /// </summary>
        private string EncodePaymentId(string paymentId)
        {
            return paymentId.Replace(".", "-");
        }

        public JsonResult Pay(long id)
        {

            return Json(new Result { success = true, msg = "付款成功" });
        }

        public ActionResult Setting()
        {
            var siteSetting = CurrentSiteSetting;
            return View(siteSetting);
        }

        public JsonResult SaveWithDrawSetting(string minimum, string maximum, bool alipayEnable)
        {
            int min = 0, max = 0;
            if (int.TryParse(minimum, out min) && int.TryParse(maximum, out max))
            {
                if (min > 0 && min < max && max <= 1000000)
                {
                    _iSiteSettingService.SaveSetting("WithDrawMaximum", maximum);
                    _iSiteSettingService.SaveSetting("WithDrawMinimum", minimum);
                    _iSiteSettingService.SaveSetting("Withdraw_AlipayEnable", alipayEnable);
                    return Json(new Result() { success = true, msg = "保存成功" });
                }
                return Json(new Result() { success = false, msg = "金额范围只能是(1-1000000)" });
            }
            else
            {
                return Json(new Result() { success = false, msg = "只能输入数字" });
            }
        }
        public JsonResult CancelPay(long Id)
        {
            string Msg = "ok";
            var b = true;
            if (Id > 0)
            {
                b = _iMemberCapitalService.CancelPay(Id);
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

        public JsonResult ChageCapital(long userId, decimal amount, string remark)
        {
            var result = new Result { msg = "错误的会员编号" };
            var _user = _iMemberService.GetUserByCache(userId);
            if (_user != null)
            {
                if (string.IsNullOrWhiteSpace(remark))
                {
                    result.msg = "请填写备注信息";
                }
                else
                {
                    if (amount < 0)
                    {
                        var uc = _iMemberCapitalService.GetCapitalInfo(userId);
                        if (uc == null || uc.Balance < Math.Abs(amount))
                        {
                            throw new HimallException("用户余额不足相减");
                        }
                    }
                    if (amount < 0)
                    {
                        CapitalDetailModel capita = new CapitalDetailModel
                        {
                            UserId = userId,
                            SourceType = CapitalDetailInfo.CapitalDetailType.ChargeAmount,
                            Amount = amount,
                            CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            Remark = remark,
                            PayWay = "管理员操作"
                        };
                        _iMemberCapitalService.AddCapital(capita);
                    }
                    else
                    {
                        ChargeDetailInfo detail = new ChargeDetailInfo()
                        {
                            ChargeAmount = amount,
                            ChargeStatus = ChargeDetailInfo.ChargeDetailStatus.WaitPay,
                            CreateTime = DateTime.Now,
                            MemId = userId,
                            ChargeWay = "管理员操作"
                        };
                        long id = _iMemberCapitalService.AddChargeApply(detail);
                        _iMemberCapitalService.ChargeSuccess(id,remark+ " 管理员操作");
                    }
                    result.success = true;
                    result.msg = "操作成功";
                }
            }

            return Json(result);
        }
    }
}