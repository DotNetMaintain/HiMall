using Himall.Application;
using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.Core.Plugins.Payment;
using Himall.DTO;
using Himall.Model;
using Himall.SmallProgAPI.Model.ParamsModel;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;

namespace Himall.SmallProgAPI
{
    public class O2OPaymentController : BaseO2OApiController
    {
        public JsonResult<Result<dynamic>> GetPaymentList(string orderId)
        {
            CheckUserLogin();
            var mobilePayments = Core.PluginsManagement.GetPlugins<IPaymentPlugin>(true).Where(item => item.Biz.SupportPlatforms.Contains(Core.PlatformType.WeiXinO2OSmallProg));
            string webRoot = Core.Helper.WebHelper.GetScheme() + "://" + Core.Helper.WebHelper.GetHost();
            string urlPre = webRoot + "/m-" + Core.PlatformType.Android + "/Payment/";

            List<long> ordids = orderId.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).Select(t => long.Parse(t)).ToList();
            //获取待支付的所有订单
            IEnumerable<DTO.FullOrder> orders = OrderApplication.GetFullOrders(ordids).ToList();
            orders = orders.Where(r => r.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay);
            var totalAmount = orders.Sum(t => t.OrderTotalAmount - t.CapitalAmount);

            //获取所有订单中的商品名称
            string productInfos = GetProductNameDescriptionFromOrders(orders);


            string notifyPre = urlPre + "Notify/", returnPre = webRoot + "/m-" + Core.PlatformType.Android + "/Member/PaymentToOrders?ids=" + orderId;
            var orderPayModel = ordids.Select(item => new OrderPay
            {
                PayId = 0,
                OrderId = item
            });
            //保存支付订单
            var payid = OrderApplication.SaveOrderPayInfo(orderPayModel, Core.PlatformType.WeiXinSmallProg);
            var ids = payid.ToString();

            var models = mobilePayments.ToArray().Select(item =>
            {
                string url = string.Empty;
                try
                {
                    url = item.Biz.GetRequestUrl(returnPre, notifyPre + item.PluginInfo.PluginId.Replace(".", "-") + "/", ids, totalAmount, productInfos, CurrentUserOpenId);
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
            if (null == model) return JsonResult<dynamic>();

            if (!string.IsNullOrEmpty(model.prepayId))
            {
                WXAppletFormDatasInfo info = new WXAppletFormDatasInfo();
                info.EventId = Convert.ToInt64(MessageTypeEnum.OrderPay);
                info.EventTime = DateTime.Now;
                info.EventValue = orderId;
                info.ExpireTime = DateTime.Now.AddDays(7);
                info.FormId = model.prepayId;
                WXMsgTemplateApplication.AddWXAppletFromData(info);
            }
            return JsonResult<dynamic>(model);
        }

        string GetProductNameDescriptionFromOrders(IEnumerable<DTO.FullOrder> orders)
        {
            List<string> productNames = new List<string>();
            foreach (var order in orders)
                productNames.AddRange(order.OrderItems.Select(t => t.ProductName));
            var productInfos = productNames.Count() > 1 ? (productNames.ElementAt(0) + " 等" + productNames.Count() + "种商品") : productNames.ElementAt(0);
            return productInfos;
        }

        /// <summary>
        /// 预付款支付
        /// </summary>
        /// <param name="pmtidpmtid"></param>
        /// <param name="ids"></param>
        /// <param name="payPwd"></param>
        /// <returns></returns>
        public JsonResult<Result<int>> GetPayByCapital(string ids, string payPwd)
        {
            CheckUserLogin();
            var curUrl = Request.RequestUri.Scheme + "://" + Request.RequestUri.Authority;
            OrderApplication.PayByCapital(CurrentUser.Id, ids, payPwd, curUrl);
            return JsonResult<int>(msg: "支付成功");
        }
        public JsonResult<Result<bool>> GetPayByCapitalIsOk(string ids)
        {
            CheckUserLogin();
            var result = OrderApplication.PayByCapitalIsOk(CurrentUser.Id, ids);
            return Json(ApiResult<bool>(result, data: result));
        }

        /// <summary>
        /// 判断是否设置支付密码
        /// </summary>
        public JsonResult<Result<bool>> GetHasSetPayPwd()
        {
            CheckUserLogin();
            bool result = false;
            result = OrderApplication.GetPayPwd(CurrentUser.Id);
            return Json(ApiResult(result, data: result));
        }
        /// <summary>
        /// 判断支付密码是否正确
        /// </summary>
        public JsonResult<Result<bool>> CheckPayPwd(PostSetPayPwdModel papa)
        {
            CheckUserLogin();
            bool result = false;
            if (papa != null && !string.IsNullOrWhiteSpace(papa.pwd))
            {
                result = MemberApplication.VerificationPayPwd(CurrentUser.Id, papa.pwd);
            }
            return Json(ApiResult(result, data: result));
        }
        /// <summary>
        /// 设置密码
        /// </summary>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public JsonResult<Result<int>> PostSetPayPwd(PostSetPayPwdModel model)
        {
            CheckUserLogin();
            if (string.IsNullOrWhiteSpace(model.pwd))
            {
                return Json(ErrorResult<int>("支付密码不能为空"));
            }
            MemberCapitalApplication.SetPayPwd(CurrentUser.Id, model.pwd);
            return JsonResult<int>(msg: "设置成功");
        }

    }
}
