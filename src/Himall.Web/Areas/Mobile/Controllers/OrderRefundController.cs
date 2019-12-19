using Himall.IServices;
using Himall.DTO.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Core;
using Himall.Web.Areas.Mobile.Models;
using System.IO;
using Himall.CommonModel;
using Himall.Application;
using Senparc.Weixin.MP.CommonAPIs;
using System.Net;
using Himall.Web.Areas.Web.Models;

namespace Himall.Web.Areas.Mobile.Controllers
{
    public class OrderRefundController : BaseMobileTemplatesController
    {
        private IOrderService _iOrderService;
        private IRefundService _iRefundService;
        private IShopService _iShopService;
        private IVShopService _iVShopService;
        public OrderRefundController(IOrderService iOrderService, IRefundService iRefundService, IShopService iShopService, IVShopService iVShopService)
        {
            _iOrderService = iOrderService;
            _iRefundService = iRefundService;
            _iShopService = iShopService;
            _iVShopService = iVShopService;
        }
        [ValidateInput(false)]
        // GET: Web/OrderRefund
        /// <summary>
        /// 退款申请
        /// </summary>
        /// <param name="id"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public ActionResult RefundApply(long orderid, long? itemId, long? refundid)
        {
            Himall.Web.Areas.Web.Models.RefundApplyModel model = new Himall.Web.Areas.Web.Models.RefundApplyModel();
            model.RefundMode = null;
            model.OrderItemId = null;
            List<SelectListItem> typelist = new List<SelectListItem>();
            typelist.Add(new SelectListItem { Value = "0", Text = "请选择" });
            typelist.Add(new SelectListItem { Value = "1", Text = "未收到货仅退款" });
            typelist.Add(new SelectListItem { Value = "2", Text = "已收到货仅退款" });
            typelist.Add(new SelectListItem { Value = "3", Text = "退货退款" });
            model.typelist = typelist;
            var ordser = ServiceApplication.Create<IOrderService>();
            var order = ordser.GetOrder(orderid, CurrentUser.Id);
            string errormsg = "";
            string jumpurl = "/" + ViewBag.AreaName + "/Member/Center";
            bool isok = true;

            if (isok)
            {
                if (order == null)
                {
                    isok = false;
                    errormsg = "该订单已删除或不属于该用户";
                    return Redirect(jumpurl);
                    throw new Himall.Core.HimallException("该订单已删除或不属于该用户");
                }
            }

            if (isok)
            {
                if ((int)order.OrderStatus < 2)
                {
                    isok = false;
                    errormsg = "错误的售后申请,订单状态有误";
                    return Redirect(jumpurl);
                    throw new Himall.Core.HimallException("错误的售后申请,订单状态有误");
                }
            }

            //if (isok)
            //{
            //    if (itemId == null && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp)
            //    {
            //        isok = false;
            //        errormsg = "错误的订单退款申请,订单状态有误";
            //        return Redirect(jumpurl);
            //        throw new Himall.Core.HimallException("错误的订单退款申请,订单状态有误");
            //    }
            //}

            if (isok)
            {
                //售后时间限制
                if (_iOrderService.IsRefundTimeOut(orderid))
                {
                    isok = false;
                    errormsg = "订单已超过售后期";
                    return Redirect(jumpurl);
                    throw new Himall.Core.HimallException("订单已超过售后期");
                }
            }

            if (isok)
            {
                //计算可退金额 预留
                ordser.CalculateOrderItemRefund(orderid);

                OrderItemInfo item = new OrderItemInfo();
                model.MaxRefundGoodsNumber = 0;
                model.MaxRefundAmount = order.OrderEnabledRefundAmount;
                Core.Log.Debug("orderid" + orderid);
                if (itemId == null)
                {
                    model.OrderItems = order.OrderItemInfo.ToList();
                    item = order.OrderItemInfo.Where(a => a.OrderId == orderid).FirstOrDefault();
                    itemId = item.Id;
                    model.OrderItems.Add(item);
                    Core.Log.Debug("item.Quantity===" + item.Quantity + "item.ReturnQuantity" + item.ReturnQuantity);
                    model.MaxRefundGoodsNumber = item.Quantity - item.ReturnQuantity;
                    model.MaxRefundAmount = item.EnabledRefundAmount - item.RefundPrice;
                }
                else
                {
                    item = order.OrderItemInfo.Where(a => a.Id == itemId).FirstOrDefault();
                    model.OrderItems.Add(item);
                    Core.Log.Debug("item.Quantity===" + item.Quantity+ "item.ReturnQuantity" + item.ReturnQuantity);
                    model.MaxRefundGoodsNumber = item.Quantity - item.ReturnQuantity;
                    model.MaxRefundAmount = item.EnabledRefundAmount - item.RefundPrice;
                }
                if (!model.MaxRefundAmount.HasValue)
                {
                    model.MaxRefundAmount = 0;
                }
                bool isCanApply = false;
                var refundser = _iRefundService;
                OrderRefundInfo refunddata;

                if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery|| order.OrderStatus == OrderInfo.OrderOperateStatus.WaitReceiving || order.OrderStatus == OrderInfo.OrderOperateStatus.UnComment)
                {
                    isCanApply = refundser.CanApplyRefund(orderid, item.Id);
                }
                else
                {
                    isCanApply = refundser.CanApplyRefund(orderid, item.Id, false);
                }
                if (!refundid.HasValue)
                {
                    if (!isCanApply)
                    {
                        isok = false;
                        errormsg = "您已申请过售后，不可重复申请";
                        return Redirect(jumpurl);
                        throw new Himall.Core.HimallException("您已申请过售后，不可重复申请");
                    }

                    //model.ContactPerson = CurrentUser.RealName;
                    //model.ContactCellPhone = CurrentUser.CellPhone;
                    // model.ContactCellPhone = order.CellPhone;

                    model.ContactPerson = string.IsNullOrEmpty(order.ShipTo) ? CurrentUser.RealName : order.ShipTo;
                    model.ContactCellPhone = string.IsNullOrEmpty(order.CellPhone) ? CurrentUser.CellPhone : order.CellPhone;
                    
                    model.OrderItemId = itemId;
                    if (!model.OrderItemId.HasValue)
                    {
                        model.IsOrderAllRefund = true;
                        model.RefundMode = OrderRefundInfo.OrderRefundMode.OrderRefund;
                    }
                }
                else
                {
                    refunddata = refundser.GetOrderRefund(refundid.Value, CurrentUser.Id);
                    if (refunddata == null)
                    {
                        isok = false;
                        errormsg = "错误的售后数据";
                        return Redirect(jumpurl);
                        throw new Himall.Core.HimallException("错误的售后数据");
                    }
                    if (isok)
                    {
                        if (refunddata.SellerAuditStatus != OrderRefundInfo.OrderRefundAuditStatus.UnAudit)
                        {
                            isok = false;
                            errormsg = "错误的售后状态，不可激活";
                            return Redirect(jumpurl);
                            throw new Himall.Core.HimallException("错误的售后状态，不可激活");
                        }
                    }
                    if (isok)
                    {
                        model.ContactPerson = refunddata.ContactPerson;
                        model.ContactCellPhone = refunddata.ContactCellPhone;
                        model.OrderItemId = refunddata.OrderItemId;
                        model.IsOrderAllRefund = (refunddata.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund);
                        model.RefundMode = refunddata.RefundMode;
                        model.RefundReasonValue = refunddata.Reason;
                        model.RefundWayValue = refunddata.RefundPayType;
                        model.CertPic1 = refunddata.CertPic1;
                        model.CertPic2 = refunddata.CertPic2;
                        model.CertPic3 = refunddata.CertPic3;
                    }
                }
                if (!model.IsOrderAllRefund && item.EnabledRefundAmount.HasValue)
                {
                    model.RefundGoodsPrice = item.EnabledRefundAmount.Value / item.Quantity;
                }

                if (isok)
                {
                    model.OrderInfo = order;
                    model.OrderId = orderid;
                    model.RefundId = refundid;

                    var reasons = refundser.GetRefundReasons();
                    foreach (var _ir in reasons)
                    {
                        _ir.AfterSalesText = _ir.AfterSalesText.Trim();
                    }
                    List<SelectListItem> reasel = new List<SelectListItem>();
                    SelectListItem _tmpsel;
                    _tmpsel = new SelectListItem { Text = "选择售后理由", Value = "" };
                    reasel.Add(_tmpsel);
                    foreach (var _i in reasons)
                    {
                        _tmpsel = new SelectListItem { Text = _i.AfterSalesText, Value = _i.AfterSalesText };
                        if (!string.IsNullOrWhiteSpace(model.RefundReasonValue))
                        {
                            if (_i.AfterSalesText == model.RefundReasonValue)
                            {
                                _tmpsel.Selected = true;
                            }
                        }
                        reasel.Add(_tmpsel);
                    }
                    model.RefundReasons = reasel;

                    List<SelectListItem> list = new List<SelectListItem> {
                        new SelectListItem{
                            Text=OrderRefundInfo.OrderRefundPayType.BackCapital.ToDescription(),
                            Value=((int)OrderRefundInfo.OrderRefundPayType.BackCapital).ToString()
                        }
                    };
                    if (order.CanBackOut())
                    {
                        _tmpsel = new SelectListItem
                        {
                            Text = OrderRefundInfo.OrderRefundPayType.BackOut.ToDescription(),
                            Value = ((int)OrderRefundInfo.OrderRefundPayType.BackOut).ToString()
                        };
                        //if (model.RefundWayValue.HasValue)
                        //{
                        //    if (_tmpsel.Value == model.RefundWayValue.ToString())
                        //    {
                        //        _tmpsel.Selected = true;
                        //    }
                        //}
                        _tmpsel.Selected = true;  //若订单支付方式为支付宝、微信支付则退款方式默认选中“退款原路返回”
                        list.Add(_tmpsel);
                        model.BackOut = 1;
                    }
                    model.RefundWay = list;
                }
                if (order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake)
                {
                    var shopBranch = ShopBranchApplication.GetShopBranchById(order.ShopBranchId.Value);
                    model.ReturnGoodsAddress = RegionApplication.GetFullName(shopBranch.AddressId);
                    model.ReturnGoodsAddress += " " + shopBranch.AddressDetail;
                    model.ReturnGoodsAddress += " " + shopBranch.ContactPhone;
                }
            }
            ViewBag.errormsg = errormsg;
            return View(model);
        }
        /// <summary>
        /// 退款申请处理
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult RefundApply(OrderRefundInfo info)
        {
            if (info.ReasonDetail != null && info.ReasonDetail.Length > 1000)
                throw new Himall.Core.HimallException("退款说明不能超过1000字符");
            var order = _iOrderService.GetOrder(info.OrderId, CurrentUser.Id);
            Core.Log.Debug("info.OrderId===="+ info.OrderId+ "CurrentUser.Id==="+ CurrentUser.Id);
            if (order == null) throw new Himall.Core.HimallException("该订单已删除或不属于该用户");
            if ((int)order.OrderStatus < 2) throw new Himall.Core.HimallException("错误的售后申请,订单状态有误");
            //售后时间限制
            if (_iOrderService.IsRefundTimeOut(info.OrderId))
            {
                throw new Himall.Core.HimallException("订单已超过售后期");
            }

            Core.Log.Debug("info.RefundMode"+ info.RefundMode);
            //if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery || order.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp || order.OrderStatus == OrderInfo.OrderOperateStatus.WaitReceiving || order.OrderStatus == OrderInfo.OrderOperateStatus.UnComment)
            //{
            //    info.RefundMode = OrderRefundInfo.OrderRefundMode.OrderRefund;
            //    //info.ReturnQuantity = 0;
            //}
            if (info.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund && info.Id == 0&& info.RefundMode != OrderRefundInfo.OrderRefundMode.OrderItemRefund)
            {
                var _refobj = OrderApplication.GetOrderRefunds(new long[] { info.OrderItemId }).FirstOrDefault(d => d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                if (_refobj != null)
                {
                    info.Id = _refobj.Id;
                }
            }
            Core.Log.Debug("info.ReturnQuantity"+ info.ReturnQuantity);
            //if (info.RefundType == 1)
            //{
            //    info.ReturnQuantity = 0;
            //    info.IsReturn = false;
            //}
            if (info.ReturnQuantity == null || info.ReturnQuantity < 0)
                throw new Himall.Core.HimallException("错误的退货数量");
            var orderitem = order.OrderItemInfo.FirstOrDefault(a => a.Id == info.OrderItemId);
            if (orderitem == null && info.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund)
                throw new Himall.Core.HimallException("该订单条目已删除或不属于该用户");
            if (info.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund|| info.RefundMode == OrderRefundInfo.OrderRefundMode.OrderItemRefund)
            {
                //if (order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery && order.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp || order.OrderStatus == OrderInfo.OrderOperateStatus.WaitReceiving || order.OrderStatus == OrderInfo.OrderOperateStatus.UnComment)
                //    throw new Himall.Core.HimallException("错误的订单退款申请,订单状态有误");
                info.IsReturn = false;
                info.ReturnQuantity = 0;
                if (info.Amount > order.OrderEnabledRefundAmount)
                    throw new Himall.Core.HimallException("退款金额不能超过订单的实际支付金额");
            }
            else
            {
                if (info.Amount > (orderitem.EnabledRefundAmount - orderitem.RefundPrice)) throw new Himall.Core.HimallException("退款金额不能超过订单的可退金额");
                if (info.ReturnQuantity > (orderitem.Quantity - orderitem.ReturnQuantity)) throw new Himall.Core.HimallException("退货数量不可以超出可退数量");
            }
            info.IsReturn = false;
            if (info.ReturnQuantity > 0) info.IsReturn = true;
            //if (info.RefundType == 2) info.IsReturn = true;
            if (info.IsReturn == true && info.ReturnQuantity < 1) throw new Himall.Core.HimallException("错误的退货数量");
            if (info.Amount <= 0) throw new Himall.Core.HimallException("错误的退款金额");
            info.ShopId = order.ShopId;
            info.ShopName = order.ShopName;
            info.UserId = CurrentUser.Id;
            info.Applicant = CurrentUser.UserName;
            info.ApplyDate = DateTime.Now;
            info.ReasonDetail = HttpUtility.HtmlEncode(info.ReasonDetail);
            info.Reason = HTMLEncode(info.Reason.Replace("'", "‘").Replace("\"", "”"));
            if (!info.IsWxUpload)
            {
                info.CertPic1 = MoveImages(info.CertPic1, CurrentUser.Id, info.OrderItemId);
                info.CertPic2 = MoveImages(info.CertPic2, CurrentUser.Id, info.OrderItemId);
                info.CertPic3 = MoveImages(info.CertPic3, CurrentUser.Id, info.OrderItemId);
            }
            else
            {
                info.CertPic1 = DownloadWxImage(info.CertPic1, CurrentUser.Id, info.OrderItemId);
                info.CertPic2 = DownloadWxImage(info.CertPic2, CurrentUser.Id, info.OrderItemId);
                info.CertPic3 = DownloadWxImage(info.CertPic3, CurrentUser.Id, info.OrderItemId);
            }
            //info.RefundAccount = HTMLEncode(info.RefundAccount.Replace("'", "‘").Replace("\"", "”"));
            if (info.Id > 0)
            {
                _iRefundService.ActiveRefund(info);
            }
            else
            {
                _iRefundService.AddOrderRefund(info);
            }
            return SuccessResult<dynamic>(msg: "提交成功", data: info.Id);
        }

        /// <summary>
        /// 显示售后记录
        /// </summary>
        /// <param name="applyDate"></param>
        /// <param name="auditStatus"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="showtype">0 所有 1 订单退款 2 仅退款(包含订单退款) 3 退货 4 仅退款</param>
        /// <returns></returns>
        public JsonResult List(int pageNo = 1, int pageSize = 10)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;

            var queryModel = new RefundQuery()
            {
                StartDate = startDate,
                EndDate = endDate,
                UserId = CurrentUser.Id,
                PageSize = pageSize,
                PageNo = pageNo,
                ShowRefundType = 0
            };
            var refunds = _iRefundService.GetOrderRefunds(queryModel);
            var list = refunds.Models.Select(item =>
            {
                var vshop = _iVShopService.GetVShopByShopId(item.ShopId) ?? new VShopInfo() { Id = 0 };
                IEnumerable<OrderItemInfo> orderItems = null;
                bool IsSelfTake = false;
                var order = _iOrderService.GetOrder(item.OrderId, CurrentUser.Id);
                if (order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake)
                {
                    IsSelfTake = true;
                }
                if (item.RefundMode == OrderRefundInfo.OrderRefundMode.OrderRefund)
                {
                    orderItems = order.OrderItemInfo.ToList();
                }
                var status = string.Empty;
                if (IsSelfTake || (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0))//分配门店订单与自提订单一致
                {
                    status = item.RefundStatus.Replace("商家", "门店");
                }
                var shopBranch = ShopBranchApplication.GetShopBranchById(order.ShopBranchId ?? 0);
                var branchName = shopBranch == null ? "" : shopBranch.ShopBranchName;
                return new
                {
                    ShopName = item.ShopName,
                    Vshopid = vshop.Id,
                    ShopBranchId = order.ShopBranchId,
                    ShopBranchName = branchName,
                    RefundStatus = string.IsNullOrEmpty(status) ? item.RefundStatus : status,
                    Id = item.Id,
                    ProductName = item.OrderItemInfo.ProductName,
                    EnabledRefundAmount = item.EnabledRefundAmount,
                    Amount = item.Amount,
                    Img = HimallIO.GetProductSizeImage(item.OrderItemInfo.ThumbnailsUrl, 1, (int)ImageSize.Size_100),
                    ShopId = item.ShopId,
                    RefundMode = item.RefundMode,
                    OrderId = item.OrderId,
                    OrderTotal = order.OrderTotalAmount.ToString("f2"),
                    OrderItems = orderItems != null ? orderItems.Select(e => new
                    {
                        ThumbnailsUrl = HimallIO.GetProductSizeImage(e.ThumbnailsUrl, 1, (int)ImageSize.Size_100),
                        ProductName = e.ProductName
                    }) : null,
                    SellerAuditStatus = item.SellerAuditStatus
                };
            });
            return SuccessResult<dynamic>(data: list);
        }


        [HttpGet]
        public JsonResult GetShopInfo(long shopId)
        {
            var shopinfo = _iShopService.GetShop(shopId);
            var model = new { SenderAddress = shopinfo.SenderAddress, SenderPhone = shopinfo.SenderPhone, SenderName = shopinfo.SenderName };
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateRefund(long id, string expressCompanyName, string shipOrderNumber)
        {
            _iRefundService.UserConfirmRefundGood(id, CurrentUser.UserName, expressCompanyName, shipOrderNumber);
            return SuccessResult<dynamic>(msg: "提交成功");
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


        public ActionResult RefundList()
        {
            return View();
        }
        public ActionResult RefundDetail(long id)
        {
            var refundinfo = _iRefundService.GetOrderRefund(id, CurrentUser.Id);
            ViewBag.RefundPayType = refundinfo.RefundPayType.ToDescription();
            refundinfo.IsOrderRefundTimeOut = _iOrderService.IsRefundTimeOut(refundinfo.OrderId);
            var order = _iOrderService.GetOrder(refundinfo.OrderId, CurrentUser.Id);
            string status = refundinfo.RefundStatus;
            if (order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake || (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0))
            {
                status = refundinfo.RefundStatus.Replace("商家", "门店");
            }
            var shopBranch = ShopBranchApplication.GetShopBranchById(order.ShopBranchId ?? 0);
            var branchName = shopBranch == null ? "" : shopBranch.ShopBranchName;
            ViewBag.ShopBranchId = order.ShopBranchId;
            ViewBag.ShopBranchName = branchName;
            ViewBag.RefundStatus = status;
            return View(refundinfo);
        }
        public ActionResult RefundProcessDetail(long id)
        {
            var refundinfo = _iRefundService.GetOrderRefund(id, CurrentUser.Id);
            int curappnum = refundinfo.ApplyNumber.HasValue ? refundinfo.ApplyNumber.Value : 1;
            ViewBag.RefundLogs = _iRefundService.GetRefundLogs(refundinfo.Id, curappnum, false);
            return View(refundinfo);
        }
        [HttpGet]
        public JsonResult GetShopGetAddress(long shopId, long shopBranchId = 0)
        {
            if (shopBranchId <= 0)
            {
                var data = ShopShippersApplication.GetDefaultGetGoodsShipper(shopId);
                if (data == null)
                {
                    data = new DTO.ShopShipper() { };
                }
                else
                {
                    data.RegionStr = RegionApplication.GetFullName(data.RegionId);
                }
                var model = new
                {
                    Region = data.RegionStr,
                    Address = data.Address,
                    Phone = data.TelPhone,
                    ShipperName = data.ShipperName
                };
                return Json<dynamic>(true, data: model);
            }
            else
            {
                var data = ShopBranchApplication.GetShopBranchById(shopBranchId);
                string redionstr = "";
                if (data != null)
                {
                    redionstr = RegionApplication.GetFullName(data.AddressId);
                }
                var model = new
                {
                    Region = redionstr,
                    Address = data.AddressDetail,
                    Phone = data.ContactPhone,
                    ShipperName = data.ContactUser
                };
                return Json<dynamic>(true, data: model);
            }
        }
        /// <summary>
        /// 转移图片
        /// </summary>
        /// <param name="image"></param>
        /// <param name="userId"></param>
        /// <param name="itemid"></param>
        /// <returns></returns>
        private string MoveImages(string image, long userId, long itemid)
        {
            if (!string.IsNullOrWhiteSpace(image))
            {
                var ext = Path.GetFileName(image);
                string ImageDir = string.Empty;
                //转移图片
                string relativeDir = "/Storage/Plat/Refund/" + userId.ToString() + "/";
                string fileName = itemid.ToString() + "_" + DateTime.Now.ToString("yyMMddHHmmssffff") + ext;
                if (image.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
                {
                    var de = image.Substring(image.LastIndexOf("/temp/"));
                    Core.HimallIO.CopyFile(de, relativeDir + fileName, true);
                    return relativeDir + fileName;
                }  //目标地址
                else if (image.Contains("/Storage/"))
                {
                    return image.Substring(image.LastIndexOf("/Storage/"));
                }

                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
        /// <summary>
        /// 下载微信图片
        /// </summary>
        /// <param name="link">下载地址</param>
        /// <param name="filePath">保存相对路径</param>
        /// <param name="fileName">保存地址</param>
        /// <returns></returns>
        private string DownloadWxImage(string mediaId, long userId, long itemid)
        {
            var token = AccessTokenContainer.TryGetToken(CurrentSiteSetting.WeixinAppId, CurrentSiteSetting.WeixinAppSecret);
            var address = string.Format("https://file.api.weixin.qq.com/cgi-bin/media/get?access_token={0}&media_id={1}", token, mediaId);
            Random ra = new Random();
            string fileName = itemid.ToString() + "_" + DateTime.Now.ToString("yyMMddHHmmssffff") + ".jpg";
            var ImageDir = "/Storage/Plat/Refund/" + userId.ToString() + "/";
            WebClient wc = new WebClient();
            try
            {
                string fullPath = Path.Combine(ImageDir, fileName);
                var data = wc.DownloadData(address);
                MemoryStream stream = new MemoryStream(data);
                Core.HimallIO.CreateFile(fullPath, stream, FileCreateType.Create);
                return ImageDir + fileName;
            }
            catch (Exception ex)
            {
                Log.Error("下载图片发生异常" + ex.Message);
                return string.Empty;
            }
        }
    }
}