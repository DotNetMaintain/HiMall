using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Plugins;
using Himall.IServices;
using Himall.DTO.QueryModel;
using Himall.Model;
using Himall.Web.Areas.Admin.Models.Product;
using Himall.Web.Framework;

using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Text;
using Himall.DTO;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class OrderRefundController : BaseSellerController
    {
        private IOrderService _iOrderService;
        private IRefundService _iRefundService;

        public OrderRefundController(IOrderService iOrderService, IRefundService iRefundService)
        {
            _iOrderService = iOrderService;
            _iRefundService = iRefundService;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="showtype">0 所有 1 订单退款 2 仅退款(包含订单退款) 3 退货 4 仅退款</param>
        /// <returns></returns>
        public ActionResult Management(int showtype = 0)
        {
            ViewBag.ShowType = showtype;
            #region 是否开启门店授权
            bool isOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore;
            if (isOpenStore)
            {
                #region 商家下所有门店
                var data = ShopBranchApplication.GetShopBranchsAll(new ShopBranchQuery()
                {
                    ShopId = CurrentSellerManager.ShopId
                });
                ViewBag.StoreList = data.Models;
                #endregion
            }
            ViewBag.IsOpenStore = isOpenStore;
            #endregion
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="orderId"></param>
        /// <param name="auditStatus"></param>
        /// <param name="userName"></param>
        /// <param name="ProductName"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <param name="showtype">0 所有 1 订单退款 2 仅退款(包含订单退款) 3 退货 4 仅退款</param>
        /// <returns></returns>
		[HttpPost]
        public JsonResult List(DateTime? startDate, DateTime? endDate, long? orderId, int? auditStatus, long? shopBranchId, string userName, string ProductName, int page, int rows, int showtype = 0)
        {
            _iOrderService.AutoCloseOrder();//过期未付款，订单自动关闭
            _iOrderService.AutoConfirmOrder();//完成过期未确认收货的订单
            _iRefundService.AutoAuditRefund();//自动审核退款
            _iRefundService.AutoCloseByDeliveryExpired();//自动关闭过期未寄货退款
            _iRefundService.AutoShopConfirmArrival();//自动商家确认到货
            var queryModel = new RefundQuery()
            {
                StartDate = startDate,
                EndDate = endDate,
                OrderId = orderId,
                ProductName = ProductName,
                AuditStatus = (Model.OrderRefundInfo.OrderRefundAuditStatus?)auditStatus,
                ShopId = CurrentSellerManager.ShopId,
                UserName = userName,
                PageSize = rows,
                PageNo = page,
                ShowRefundType = showtype
            };
            if (shopBranchId.HasValue && shopBranchId.Value >= 0) queryModel.ShopBranchId = shopBranchId.Value;

            var refunds = Application.RefundApplication.GetOrderRefunds(queryModel);
            var orders = Application.OrderApplication.GetOrders(refunds.Models.Select(p => p.OrderId));
            var orderItems = Application.OrderApplication.GetOrderItemsByOrderItemId(refunds.Models.Select(p => p.OrderItemId));
            var refundModels = refunds.Models.Select(item =>
            {
                //以下查询代码要做优化
                var order = orders.FirstOrDefault(p => p.Id == item.OrderId);
                var orderItem = orderItems.FirstOrDefault(p => p.Id == item.OrderItemId);
                string spec = ((string.IsNullOrWhiteSpace(orderItem.Color) ? "" : orderItem.Color + "，")
                                + (string.IsNullOrWhiteSpace(orderItem.Size) ? "" : orderItem.Size + "，")
                                + (string.IsNullOrWhiteSpace(orderItem.Version) ? "" : orderItem.Version + "，")).TrimEnd('，');
                if (!string.IsNullOrWhiteSpace(spec))
                {
                    spec = "  【" + spec + " 】";
                }
                string shopBranchName = "总店";
                if (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0)
                {
                    var shopBranchInfo = ShopBranchApplication.GetShopBranchById(order.ShopBranchId.Value);
                    if (shopBranchInfo != null)
                    {
                        shopBranchName = shopBranchInfo.ShopBranchName;
                    }
                }
                return new OrderRefundModel()
                {
                    RefundId = item.Id,
                    OrderId = item.OrderId,
                    AuditStatus = (order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake || (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0)) ? ((CommonModel.Enum.OrderRefundShopAuditStatus)(int)item.SellerAuditStatus).ToDescription() : item.SellerAuditStatus.ToDescription(),
                    ConfirmStatus = item.ManagerConfirmStatus.ToDescription(),
                    ApplyDate = item.ApplyDate.ToShortDateString(),
                    ShopId = item.ShopId,
                    ShopName = item.ShopName.Replace("'", "‘").Replace("\"", "”"),
                    UserId = item.UserId,
                    UserName = item.Applicant,
                    ContactPerson = HTMLEncode(item.ContactPerson),
                    ContactCellPhone = HTMLEncode(item.ContactCellPhone),
                    RefundAccount = string.IsNullOrEmpty(item.RefundAccount) ? string.Empty : HTMLEncode(item.RefundAccount.Replace("'", "‘").Replace("\"", "”")),
                    Amount = item.Amount.ToString("F2"),
                    ReturnQuantity = ((item.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                                    ? 0
                                    : (item.ShowReturnQuantity == 0 ? orderItem.ReturnQuantity : item.ShowReturnQuantity)),
                    Quantity = orderItem.Quantity,
                    SalePrice = item.EnabledRefundAmount.ToString("F2"),
                    ProductName = ((item.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                                    ? "订单所有商品"
                                    : (orderItem.ProductName + spec)),
                    Reason = string.IsNullOrEmpty(item.Reason) ? string.Empty : HTMLEncode(item.Reason.Replace("'", "‘").Replace("\"", "”")),
                    ReasonDetail = string.IsNullOrEmpty(item.ReasonDetail) ? string.Empty : item.ReasonDetail.Replace("'", "‘").Replace("\"", "”"),
                    ExpressCompanyName = HTMLEncode(item.ExpressCompanyName),
                    ShipOrderNumber = item.ShipOrderNumber,
                    Payee = string.IsNullOrEmpty(item.Payee) ? string.Empty : HTMLEncode(item.Payee),
                    PayeeAccount = string.IsNullOrEmpty(item.PayeeAccount) ? string.Empty : HTMLEncode(item.PayeeAccount.Replace("'", "‘").Replace("\"", "”")),
                    RefundMode = (int)item.RefundMode,
                    SellerRemark = string.IsNullOrEmpty(item.SellerRemark) ? string.Empty : HTMLEncode(item.SellerRemark.Replace("'", "‘").Replace("\"", "”")),
                    ManagerRemark = string.IsNullOrEmpty(item.ManagerRemark) ? string.Empty : HTMLEncode(item.ManagerRemark.Replace("'", "‘").Replace("\"", "”")),
                    RefundStatus = ((item.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited)
                                    ? item.ManagerConfirmStatus.ToDescription()
                                    : ((order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake || (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0)) ? ((CommonModel.Enum.OrderRefundShopAuditStatus)item.SellerAuditStatus).ToDescription() : item.SellerAuditStatus.ToDescription())),
                    RefundPayType = item.RefundPayType == null ? "线下处理" : item.RefundPayType.ToDescription(),
                    ApplyNumber = (item.ApplyNumber.HasValue ? item.ApplyNumber.Value : 1),
                    nextSecond = GetNextSecond(item),
                    CertPic1 = Core.HimallIO.GetImagePath(item.CertPic1),
                    CertPic2 = Core.HimallIO.GetImagePath(item.CertPic2),
                    CertPic3 = Core.HimallIO.GetImagePath(item.CertPic3),
                    ShopBranchId = order.ShopBranchId.HasValue ? order.ShopBranchId.Value : 0,
                    ShopBranchName = shopBranchName
                };
            });

            var dataGrid = new DataGridModel<OrderRefundModel>()
            {
                rows = refundModels,
                total = refunds.Total
            };
            return Json(dataGrid);
        }
        //返回退款列表
        public JsonResult getRefundList(int showtype = 0)
        {
            var queryModel = new RefundQuery()
            {
                StartDate = null,
                EndDate = null,
                OrderId = null,
                ProductName = null,
                AuditStatus = null,
                ShopId = CurrentSellerManager.ShopId,
                UserName = null,
                PageSize = 15,
                PageNo = 1,
                ShowRefundType = showtype
            };
            //if (shopBranchId.HasValue && shopBranchId.Value >= 0) queryModel.ShopBranchId = shopBranchId.Value;

            var refunds = Application.RefundApplication.GetOrderRefunds(queryModel);
            var orders = Application.OrderApplication.GetOrders(refunds.Models.Select(p => p.OrderId));
            var orderItems = Application.OrderApplication.GetOrderItemsByOrderItemId(refunds.Models.Select(p => p.OrderItemId));
            var refundModels = refunds.Models.Select(item =>
            {
                //以下查询代码要做优化
                var order = orders.FirstOrDefault(p => p.Id == item.OrderId);
                var orderItem = orderItems.FirstOrDefault(p => p.Id == item.OrderItemId);
                string spec = ((string.IsNullOrWhiteSpace(orderItem.Color) ? "" : orderItem.Color + "，")
                                + (string.IsNullOrWhiteSpace(orderItem.Size) ? "" : orderItem.Size + "，")
                                + (string.IsNullOrWhiteSpace(orderItem.Version) ? "" : orderItem.Version + "，")).TrimEnd('，');
                if (!string.IsNullOrWhiteSpace(spec))
                {
                    spec = "  【" + spec + " 】";
                }
                string shopBranchName = "总店";
                if (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0)
                {
                    var shopBranchInfo = ShopBranchApplication.GetShopBranchById(order.ShopBranchId.Value);
                    if (shopBranchInfo != null)
                    {
                        shopBranchName = shopBranchInfo.ShopBranchName;
                    }
                }
                return new OrderRefundModel()
                {
                    RefundId = item.Id,
                    OrderId = item.OrderId,
                    AuditStatus = (order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake || (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0)) ? ((CommonModel.Enum.OrderRefundShopAuditStatus)(int)item.SellerAuditStatus).ToDescription() : item.SellerAuditStatus.ToDescription(),
                    ConfirmStatus = item.ManagerConfirmStatus.ToDescription(),
                    ApplyDate = item.ApplyDate.ToShortDateString(),
                    ShopId = item.ShopId,
                    ShopName = item.ShopName.Replace("'", "‘").Replace("\"", "”"),
                    UserId = item.UserId,
                    UserName = item.Applicant,
                    ContactPerson = HTMLEncode(item.ContactPerson),
                    ContactCellPhone = HTMLEncode(item.ContactCellPhone),
                    RefundAccount = string.IsNullOrEmpty(item.RefundAccount) ? string.Empty : HTMLEncode(item.RefundAccount.Replace("'", "‘").Replace("\"", "”")),
                    Amount = item.Amount.ToString("F2"),
                    ReturnQuantity = ((item.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                                    ? 0
                                    : (item.ShowReturnQuantity == 0 ? orderItem.ReturnQuantity : item.ShowReturnQuantity)),
                    Quantity = orderItem.Quantity,
                    SalePrice = item.EnabledRefundAmount.ToString("F2"),
                    ProductName = ((item.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                                    ? "订单所有商品"
                                    : (orderItem.ProductName + spec)),
                    Reason = string.IsNullOrEmpty(item.Reason) ? string.Empty : HTMLEncode(item.Reason.Replace("'", "‘").Replace("\"", "”")),
                    ReasonDetail = string.IsNullOrEmpty(item.ReasonDetail) ? string.Empty : item.ReasonDetail.Replace("'", "‘").Replace("\"", "”"),
                    ExpressCompanyName = HTMLEncode(item.ExpressCompanyName),
                    ShipOrderNumber = item.ShipOrderNumber,
                    Payee = string.IsNullOrEmpty(item.Payee) ? string.Empty : HTMLEncode(item.Payee),
                    PayeeAccount = string.IsNullOrEmpty(item.PayeeAccount) ? string.Empty : HTMLEncode(item.PayeeAccount.Replace("'", "‘").Replace("\"", "”")),
                    RefundMode = (int)item.RefundMode,
                    SellerRemark = string.IsNullOrEmpty(item.SellerRemark) ? string.Empty : HTMLEncode(item.SellerRemark.Replace("'", "‘").Replace("\"", "”")),
                    ManagerRemark = string.IsNullOrEmpty(item.ManagerRemark) ? string.Empty : HTMLEncode(item.ManagerRemark.Replace("'", "‘").Replace("\"", "”")),
                    RefundStatus = ((item.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.Audited)
                                    ? item.ManagerConfirmStatus.ToDescription()
                                    : ((order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake || (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0)) ? ((CommonModel.Enum.OrderRefundShopAuditStatus)item.SellerAuditStatus).ToDescription() : item.SellerAuditStatus.ToDescription())),
                    RefundPayType = item.RefundPayType == null ? "线下处理" : item.RefundPayType.ToDescription(),
                    ApplyNumber = (item.ApplyNumber.HasValue ? item.ApplyNumber.Value : 1),
                    nextSecond = GetNextSecond(item),
                    CertPic1 = Core.HimallIO.GetImagePath(item.CertPic1),
                    CertPic2 = Core.HimallIO.GetImagePath(item.CertPic2),
                    CertPic3 = Core.HimallIO.GetImagePath(item.CertPic3),
                    ShopBranchId = order.ShopBranchId.HasValue ? order.ShopBranchId.Value : 0,
                    ShopBranchName = shopBranchName
                };
            });

            var dataGrid = new DataGridModel<OrderRefundModel>()
            {
                rows = refundModels,
                total = refunds.Total
            };
            return Json(refundModels, JsonRequestBehavior.AllowGet);
        }
        private double GetNextSecond(DTO.OrderRefund data)
        {
            double result = -999;
            var sitesetser = ServiceApplication.Create<ISiteSettingService>();
            var siteSetting = sitesetser.GetSiteSettings();
            if (data != null)
            {
                if (data.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitAudit ||
                    data.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery ||
                    data.SellerAuditStatus == OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving
                    )
                {
                    int num = 0;
                    DateTime _time = DateTime.Now;
                    switch (data.SellerAuditStatus)
                    {
                        case OrderRefundInfo.OrderRefundAuditStatus.WaitAudit:
                            _time = data.ApplyDate;
                            num = siteSetting.AS_ShopConfirmTimeout;
                            break;
                        case OrderRefundInfo.OrderRefundAuditStatus.WaitDelivery:
                            _time = data.SellerAuditDate;
                            num = siteSetting.AS_SendGoodsCloseTimeout;
                            break;
                        case OrderRefundInfo.OrderRefundAuditStatus.WaitReceiving:
                            _time = data.BuyerDeliverDate.GetValueOrDefault();
                            num = siteSetting.AS_ShopNoReceivingTimeout;
                            break;
                    }
                    TimeSpan ts = (DateTime.Now - _time);
                    if (num > 0)
                    {
                        result = num * 24 * 60 * 60;
                        result = result - ts.TotalSeconds;
                        if (result < 0)
                        {
                            result = -1;
                        }
                    }
                }
            }
            return result;
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult DealRefund(long refundId, int auditStatus, string sellerRemark)
        {
            Result result = new Result();
            var refundauditstatus = (OrderRefundInfo.OrderRefundAuditStatus)auditStatus;
            switch (refundauditstatus)
            {
                case OrderRefundInfo.OrderRefundAuditStatus.UnAudit:
                    if (string.IsNullOrWhiteSpace(sellerRemark))
                    {
                        throw new HimallException("请填写拒绝理由");
                    }
                    break;
            }
            var refund = RefundApplication.GetOrderRefund(refundId);
            if (refund != null && refund.IsReturn == true && auditStatus == 2)
            {
                var shopget = ShopShippersApplication.GetDefaultGetGoodsShipper(refund.ShopId);
                if (shopget == null)
                {
                    throw new HimallException("需要先设置好退货地址再进行操作，立即前往设置！");
                }
            }
            _iRefundService.SellerDealRefund(refundId, refundauditstatus, sellerRemark, CurrentSellerManager.UserName);
            result.success = true;
            if (auditStatus == 2)
            {
                StringBuilder sb = new StringBuilder();
                JavaScriptSerializer json = new JavaScriptSerializer();
                Orderrefunddetail refunds = RefundApplication.GetOrderRefunds(refundId);
                json.Serialize(refunds, sb);
                string str = sb.ToString();
                string results = GetResponseResult(string.Format("http://zxk.mhybms.cn:10000/system/refund/operate_1/saveRefundBySeller?refundJson={0}", str));
                Core.Log.Debug("results"+ results);
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult Getrefunds(long refundId)
        {
            var refund = RefundApplication.GetOrderRefunds(refundId);
            return Json(refund, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [UnAuthorize]
        public JsonResult ConfirmRefundGood(long refundId)
        {
            Result result = new Result();
            try
            {
                _iRefundService.SellerConfirmRefundGood(refundId, CurrentSellerManager.UserName);
                result.success = true;
            }
            catch (Exception ex)
            {
                result.msg = ex.Message;
            }
            return Json(result);
        }

        public static string HTMLEncode(string txt)
        {
            if (string.IsNullOrEmpty(txt))
                return string.Empty;
            string Ntxt = txt;

            Ntxt = Ntxt.Replace(" ", "&nbsp;");

            Ntxt = Ntxt.Replace("<", "&lt;");

            Ntxt = Ntxt.Replace(">", "&gt;");

            Ntxt = Ntxt.Replace("\"", "&quot;");

            Ntxt = Ntxt.Replace("'", "&#39;");

            //Ntxt = Ntxt.Replace("\n", "<br>");

            return Ntxt;

        }
        string GetResponseResult(string url)
        {
            string result;
            WebRequest req = WebRequest.Create(url);
            Core.Log.Debug("url" + url);
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                using (Stream receiveStream = response.GetResponseStream())
                {

                    using (StreamReader readerOfStream = new StreamReader(receiveStream, System.Text.Encoding.UTF8))
                    {
                        result = readerOfStream.ReadToEnd();
                        Core.Log.Debug("result" + result);
                    }
                }
            }
            return result;
        }
    }
}