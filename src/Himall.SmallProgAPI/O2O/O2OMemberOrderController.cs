using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Helper;
using Himall.DTO;
using Himall.DTO.QueryModel;
using Himall.Model;
using Himall.SmallProgAPI.O2O.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web.Http.Results;

namespace Himall.SmallProgAPI
{
    public class O2OMemberOrderController : BaseO2OApiController
    {
        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="openId">openId</param>
        /// <param name="orderId">订单编号</param>
        /// <returns></returns>
        public JsonResult<Result<int>> GetCloseOrder(string openId, string orderId)
        {
            CheckUserLogin();
            long order_Id = long.Parse(orderId);
            var order = OrderApplication.GetOrder(order_Id, CurrentUser.Id);
            if (order != null)
            {
                //拼团处理
                if (order.OrderType == OrderInfo.OrderTypes.FightGroup)
                {
                    return Json(ErrorResult<int>("拼团订单，会员不能取消！"));
                }
                OrderApplication.MemberCloseOrder(order_Id, CurrentUser.UserName);
            }
            else
            {
                return Json(ErrorResult<int>("取消失败，该订单已删除或者不属于当前用户！"));
            }
            return JsonResult<int>(msg: "操作成功！");
        }

        /// <summary>
        /// 确认收货
        /// </summary>
        /// <param name="openId">openId</param>
        /// <param name="orderId">订单编号</param>
        /// <returns></returns>
        public JsonResult<Result<int>> GetConfirmOrder(string openId, string orderId)
        {
            CheckUserLogin();
            long order_Id = long.Parse(orderId);
            OrderApplication.MembeConfirmOrder(order_Id, CurrentUser.UserName);
            // var data = ServiceProvider.Instance<IOrderService>.Create.GetOrder(orderId);
            //确认收货写入结算表(修改LH的方法)
            // ServiceProvider.Instance<IOrderService>.Create.WritePendingSettlnment(data);
            return JsonResult<int>(msg: "操作成功！");
        }

        //public object GetExpressInfo(long orderId)
        //{
        //    CheckUserLogin();
        //    OrderInfo order = OrderApplication.GetOrder(orderId, CurrentUser.Id);
        //    var expressData = ExpressApplication.GetExpressData(order.ExpressCompanyName, order.ShipOrderNumber);

        //    if (expressData.Success)
        //        expressData.ExpressDataItems = expressData.ExpressDataItems.OrderByDescending(item => item.Time);//按时间逆序排列
        //    var json = new
        //    {
        //        success = expressData.Success,
        //        traces = expressData.ExpressDataItems.Select(item => new
        //        {
        //            acceptTime = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
        //            acceptStation = item.Content
        //        })
        //    };
        //    return Json(new { success = true, ShipTo = order.ShipTo, CellPhone = order.CellPhone, Address = order.RegionFullName + order.Address, ShipOrderNumber = order.ShipOrderNumber, ExpressCompanyName = order.ExpressCompanyName, LogisticsData = json });
        //}

        public JsonResult<Result<dynamic>> GetOrderDetail(long orderId)
        {
            CheckUserLogin();
            //var orderService = ServiceProvider.Instance<IOrderService>.Create;
            try
            {
                OrderInfo order = OrderApplication.GetOrder(orderId, CurrentUser.Id);

                //var orderRefundService = ServiceProvider.Instance<IRefundService>.Create;
                //var productService = ServiceProvider.Instance<IProductService>.Create;
                //订单信息是否正常
                if (order == null)
                {
                    return Json(ErrorResult<dynamic>("订单不存在"));
                }
                var coupon = CouponApplication.GetCouponRecordInfo(order.UserId, order.Id);

                string couponName = "";
                decimal couponAmout = 0;
                if (coupon != null)
                {
                    couponName = coupon.Himall_Coupon.CouponName;
                    couponAmout = coupon.Himall_Coupon.Price;
                }

                dynamic expressTrace = new ExpandoObject();

                //取订单物流信息
                if (!string.IsNullOrWhiteSpace(order.ShipOrderNumber))
                {
                    var expressData = ExpressApplication.GetExpressData(order.ExpressCompanyName, order.ShipOrderNumber);
                    if (expressData.Success)
                    {
                        expressData.ExpressDataItems = expressData.ExpressDataItems.OrderByDescending(item => item.Time);//按时间逆序排列
                        expressTrace.traces = expressData.ExpressDataItems.Select(item => new
                        {
                            acceptTime = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                            acceptStation = item.Content
                        });

                    }
                }
                var orderRefunds = OrderApplication.GetOrderRefunds(order.OrderItemInfo.Select(p => p.Id));
                var isCanOrderReturn = OrderApplication.CanRefund(order);
                //获取订单商品项数据
                var orderDetail = new
                {
                    ShopId = order.ShopId,
                    EnabledRefundAmount = order.OrderEnabledRefundAmount,
                    OrderItems = order.OrderItemInfo.Select(item =>
                    {
                        var productinfo = ProductManagerApplication.GetProduct(item.ProductId);
                        ProductType typeInfo = TypeApplication.GetType(productinfo.TypeId);
                        string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                        string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                        string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                        var itemStatusText = "";
                        var itemrefund = orderRefunds.Where(or => or.OrderItemId == item.Id).FirstOrDefault(d => d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                        int? itemrefstate = (itemrefund == null ? 0 : (int?)itemrefund.SellerAuditStatus);
                        itemrefstate = (itemrefstate > 4 ? (int?)itemrefund.ManagerConfirmStatus : itemrefstate);
                        if (itemrefund != null)
                        {//默认为商家处理进度
                            if (itemrefstate == 4)
                            {//商家拒绝,可以再发起申请
                                itemStatusText = "";
                            }
                            else
                            {
                                itemStatusText = "售后处理中";
                            }
                        }
                        if (itemrefstate > 4)
                        {//如果商家已经处理完，则显示平台处理进度
                            if (itemrefstate == 7)
                            {
                                itemStatusText = "退款成功";
                            }
                        }

                        return new
                        {
                            Status = itemrefstate,
                            StatusText = itemStatusText,
                            Id = item.Id,
                            SkuId = item.SkuId,
                            ProductId = item.ProductId,
                            Name = item.ProductName,
                            Amount = item.Quantity,
                            Price = item.SalePrice,
                            //ProductImage = "http://" + Url.Request.RequestUri.Host + productService.GetProduct(item.ProductId).GetImage(ProductInfo.ImageSize.Size_100),
                            Image = Core.HimallIO.GetRomoteProductSizeImage(ProductManagerApplication.GetProduct(item.ProductId).RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_100),
                            color = item.Color,
                            size = item.Size,
                            version = item.Version,
                            IsCanRefund = OrderApplication.CanRefund(order, itemrefstate, itemId: item.Id),
                            ColorAlias = colorAlias,
                            SizeAlias = sizeAlias,
                            VersionAlias = versionAlias,
                            SkuText = colorAlias + ":" + item.Color + ";" + sizeAlias + ":" + item.Size + ";" + versionAlias + ":" + item.Version,
                            EnabledRefundAmount = item.EnabledRefundAmount
                        };
                    })
                };
                //取拼团订单状态
                var fightGroupOrderInfo = FightGroupApplication.GetFightGroupOrderStatusByOrderId(order.Id);
                #region 门店信息
                var branchInfo = new ShopBranch();
                if (order.ShopBranchId.HasValue && order.ShopBranchId > 0)
                {
                    branchInfo = ShopBranchApplication.GetShopBranchById(order.ShopBranchId.Value);
                }
                else
                    branchInfo = null;
                #endregion
                var orderModel = new
                {
                    OrderId = order.Id,
                    Status = (int)order.OrderStatus,
                    StatusText = order.OrderStatus.ToDescription(),
                    EnabledRefundAmount = order.OrderEnabledRefundAmount,
                    OrderTotal = order.OrderTotalAmount,
                    CapitalAmount = order.CapitalAmount,
                    OrderAmount = order.ProductTotalAmount,
                    DeductionPoints = 0,
                    DeductionMoney = order.IntegralDiscount,
                    //CouponAmount = couponAmout.ToString("F2"),//优惠劵金额
                    CouponAmount = order.DiscountAmount,//优惠劵金额
                    CouponName = couponName,//优惠劵名称
                    RefundAmount = order.RefundTotalAmount,
                    Tax = 0,
                    AdjustedFreight = order.Freight,
                    OrderDate = order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    ItemStatus = 0,
                    ItemStatusText = "",
                    ShipTo = order.ShipTo,
                    ShipToDate = order.ShippingDate.HasValue ? order.ShippingDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                    Cellphone = order.CellPhone,
                    Address = order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake && branchInfo != null ? branchInfo.AddressFullName : (order.RegionFullName + " " + order.Address),
                    FreightFreePromotionName = string.Empty,
                    ReducedPromotionName = string.Empty,
                    ReducedPromotionAmount = order.FullDiscount,
                    SentTimesPointPromotionName = string.Empty,
                    CanBackReturn = !string.IsNullOrWhiteSpace(order.PaymentTypeGateway),
                    CanCashierReturn = false,
                    PaymentType = order.PaymentType.ToDescription(),
                    Remark = string.IsNullOrEmpty(order.OrderRemarks) ? "" : order.OrderRemarks,
                    InvoiceTitle = order.InvoiceTitle,
                    Invoice = order.InvoiceType.ToDescription(),
                    InvoiceValue = (int)order.InvoiceType,
                    InvoiceContext = order.InvoiceContext,
                    InvoiceCode = order.InvoiceCode,
                    ModeName = order.DeliveryType.ToDescription(),
                    LogisticsData = expressTrace,
                    TakeCode = order.PickupCode,
                    LineItems = orderDetail.OrderItems,
                    IsCanRefund = !(orderDetail.OrderItems.Any(e => e.IsCanRefund == true)) && isCanOrderReturn,
                    IsSelfTake = order.DeliveryType == Himall.CommonModel.Enum.DeliveryType.SelfTake ? 1 : 0,
                    BranchInfo = branchInfo,
                    DeliveryType = (int)order.DeliveryType
                };
                return JsonResult<dynamic>(orderModel);
            }
            catch (Exception ex)
            {
                return Json(ErrorResult<dynamic>(ex.Message));
            }
        }

        public JsonResult<Result<dynamic>> GetOrders(int? status, int pageIndex, int pageSize = 8)
        {
            CheckUserLogin();
            //var orderService = ServiceProvider.Instance<IOrderService>.Create;
            var allOrders = OrderApplication.GetTopOrders(int.MaxValue, CurrentUser.Id);

            //待评价
            var queryModelAll = new OrderQuery()
            {
                Status = OrderInfo.OrderOperateStatus.Finish,
                UserId = CurrentUser.Id,
                PageSize = int.MaxValue,
                PageNo = 1,
                Commented = false
            };
            var allOrderCounts = allOrders.Count();
            var waitingForComments = OrderApplication.GetOrders<Order>(queryModelAll).Total;
            var waitingForRecieve = allOrders.Count(item => item.OrderStatus == OrderInfo.OrderOperateStatus.WaitReceiving);//获取待收货订单数
            var waitingForPay = allOrders.Count(item => item.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay);//获取待支付订单数

            if (status.HasValue && status == 0)
            {
                status = null;
            }
            var queryModel = new OrderQuery()
            {
                Status = (OrderInfo.OrderOperateStatus?)status,
                UserId = CurrentUser.Id,
                PageSize = pageSize,
                PageNo = pageIndex
            };
            if (queryModel.Status.HasValue && queryModel.Status.Value == OrderInfo.OrderOperateStatus.WaitReceiving)
            {
                if (queryModel.MoreStatus == null)
                {
                    queryModel.MoreStatus = new List<OrderInfo.OrderOperateStatus>() { };
                }
                queryModel.MoreStatus.Add(OrderInfo.OrderOperateStatus.WaitSelfPickUp);
            }
            if (status.GetValueOrDefault() == (int)OrderInfo.OrderOperateStatus.Finish)
                queryModel.Commented = false;//只查询未评价的订单
            QueryPageModel<OrderInfo> orders = OrderApplication.GetOrderInfos<OrderInfo>(queryModel);
            //var productService = ServiceProvider.Instance<IProductService>.Create;
            //var vshopService = ServiceProvider.Instance<IVShopService>.Create;
            //var orderRefundService = ServiceProvider.Instance<IRefundService>.Create;
            var orderItems = OrderApplication.GetOrderItemsByOrderId(orders.Models.Select(p => p.Id));
            var orderRefunds = OrderApplication.GetOrderRefunds(orderItems.Select(p => p.Id));
            var shopBranchs = ShopBranchApplication.GetShopBranchByIds(orders.Models.Where(a => a.ShopBranchId.HasValue && a.ShopBranchId.Value > 0).Select(p => p.ShopBranchId.Value));
            var result = orders.Models.ToArray().Select(item =>
            {
                var shopBranchInfo = shopBranchs.FirstOrDefault(a => a.Id == item.ShopBranchId);//当前订单所属门店信息
                if (item.OrderStatus >= OrderInfo.OrderOperateStatus.WaitDelivery)
                {
                    OrderApplication.CalculateOrderItemRefund(item.Id);
                }
                var vshop = VshopApplication.GetVShopByShopId(item.ShopId);
                var _ordrefobj = RefundApplication.GetOrderRefundByOrderId(item.Id) ?? new OrderRefundInfo { Id = 0 };
                if (item.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery && item.OrderStatus != OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                {
                    _ordrefobj = new OrderRefundInfo { Id = 0 };
                }
                int? ordrefstate = (_ordrefobj == null ? null : (int?)_ordrefobj.SellerAuditStatus);
                ordrefstate = (ordrefstate > 4 ? (int?)_ordrefobj.ManagerConfirmStatus : ordrefstate);
                //参照PC端会员中心的状态描述信息
                string statusText = item.OrderStatus.ToDescription();
                if (item.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery || item.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                {
                    if (ordrefstate.HasValue && ordrefstate != 0 && ordrefstate != 4)
                    {
                        statusText = "退款中";
                    }
                }
                //是否可售后
                bool IsShowReturn = OrderApplication.CanRefund(item, ordrefstate);

                return new
                {
                    OrderId = item.Id,
                    StatusText = statusText,
                    Status = item.OrderStatus,
                    orderType = item.OrderType,
                    orderTypeName = item.OrderType.ToDescription(),
                    shopname = item.ShopName,
                    vshopId = vshop == null ? 0 : vshop.Id,
                    Amount = item.OrderTotalAmount.ToString("F2"),
                    Quantity = item.OrderProductQuantity,
                    commentCount = item.OrderCommentInfo.Count(),
                    pickupCode = item.PickupCode,
                    EnabledRefundAmount = item.OrderEnabledRefundAmount,
                    LineItems = item.OrderItemInfo.Select(a =>
                    {
                        var prodata = ProductManagerApplication.GetProduct(a.ProductId);
                        ProductType typeInfo = TypeApplication.GetType(prodata.TypeId);
                        string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                        string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                        string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                        var itemStatusText = "";
                        var itemrefund = orderRefunds.Where(or => or.OrderItemId == a.Id).FirstOrDefault(d => d.RefundMode != OrderRefundInfo.OrderRefundMode.OrderRefund);
                        int? itemrefstate = (itemrefund == null ? 0 : (int?)itemrefund.SellerAuditStatus);
                        itemrefstate = (itemrefstate > 4 ? (int?)itemrefund.ManagerConfirmStatus : itemrefstate);
                        if (itemrefund != null)
                        {//默认为商家处理进度
                            if (itemrefstate == 4)
                            {//商家拒绝
                                itemStatusText = "";
                            }
                            else
                            {
                                itemStatusText = "售后处理中";
                            }
                        }
                        if (itemrefstate > 4)
                        {//如果商家已经处理完，则显示平台处理进度
                            if (itemrefstate == 7)
                            {
                                itemStatusText = "退款成功";
                            }
                        }
                        return new
                        {
                            Status = itemrefstate,
                            StatusText = itemStatusText,
                            Id = a.SkuId,
                            productId = a.ProductId,
                            Name = a.ProductName,
                            Image = Core.HimallIO.GetRomoteProductSizeImage(a.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_350),
                            Amount = a.Quantity,
                            Price = a.SalePrice,
                            Unit = prodata == null ? "" : prodata.MeasureUnit,
                            SkuText = colorAlias + ":" + a.Color + " " + sizeAlias + ":" + a.Size + " " + versionAlias + ":" + a.Version,
                            color = a.Color,
                            size = a.Size,
                            version = a.Version,
                            ColorAlias = colorAlias,
                            SizeAlias = sizeAlias,
                            VersionAlias = versionAlias,
                            RefundStats = itemrefstate,
                            OrderRefundId = (itemrefund == null ? 0 : itemrefund.Id),
                            EnabledRefundAmount = a.EnabledRefundAmount,
                            IsShowRefund = IsShowReturn,
                            IsShowAfterSale = IsShowReturn
                        };
                    }),
                    RefundStats = ordrefstate,
                    OrderRefundId = _ordrefobj.Id,
                    IsShowLogistics = !string.IsNullOrWhiteSpace(item.ShipOrderNumber),
                    ShipOrderNumber = item.ShipOrderNumber,
                    IsShowCreview = (item.OrderStatus == OrderInfo.OrderOperateStatus.Finish && !HasAppendComment(item)),
                    IsShowPreview = false,
                    Invoice = item.InvoiceType.ToDescription(),
                    InvoiceValue = (int)item.InvoiceType,
                    InvoiceContext = item.InvoiceContext,
                    InvoiceTitle = item.InvoiceTitle,
                    PaymentType = item.PaymentType.ToDescription(),
                    PaymentTypeValue = (int)item.PaymentType,
                    IsShowClose = (item.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay),
                    IsShowFinishOrder = (item.OrderStatus == OrderInfo.OrderOperateStatus.WaitReceiving),
                    IsShowRefund = IsShowReturn,
                    IsShowReturn = IsShowReturn,
                    IsShowTakeCodeQRCode = (!string.IsNullOrWhiteSpace(item.PickupCode) && item.OrderStatus != OrderInfo.OrderOperateStatus.Finish && item.OrderStatus != OrderInfo.OrderOperateStatus.Close),
                    OrderDate = item.OrderDate,
                    SupplierId = 0,
                    ShipperName = string.Empty,
                    StoreName = shopBranchInfo != null ? shopBranchInfo.ShopBranchName : string.Empty,
                    IsShowCertification = false,
                    HasAppendComment = HasAppendComment(item),
                    CreviewText = item.OrderCommentInfo.Count() > 0 ? "追加评论" : "评价订单",
                    ProductCommentPoint = 0,
                    DeliveryType = (int)item.DeliveryType
                };
            });
            return JsonResult<dynamic>(new { AllOrderCounts = allOrderCounts, WaitingForComments = waitingForComments, WaitingForRecieve = waitingForRecieve, WaitingForPay = waitingForPay, Data = result });
        }
        /// <summary>
        /// 获取物流公司信息
        /// </summary>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetExpressInfo(string openId, long orderId)
        {
            CheckUserLogin();
            try
            {
                //var orderService = ServiceProvider.Instance<IOrderService>.Create;
                OrderInfo order = OrderApplication.GetOrder(orderId, CurrentUser.Id);
                //订单信息是否正常
                if (order == null)
                {
                    return Json(ErrorResult<dynamic>("订单号不存在"));
                }
                List<object> TracesList = new List<object>();
                //取订单物流信息
                if (!string.IsNullOrWhiteSpace(order.ShipOrderNumber))
                {
                    var expressData = ExpressApplication.GetExpressData(order.ExpressCompanyName, order.ShipOrderNumber);
                    if (expressData.Success)
                    {
                        expressData.ExpressDataItems = expressData.ExpressDataItems.OrderByDescending(item => item.Time);//按时间逆序排列
                        foreach (var item in expressData.ExpressDataItems)
                        {
                            var traces = new
                            {
                                acceptStation = item.Content,
                                acceptTime = item.Time.ToString("yyyy-MM-dd HH:mm:ss")
                            };
                            TracesList.Add(traces);
                        }
                    }
                }
                var data = new
                {
                    LogisticsData = new
                    {
                        success = TracesList.Count > 0,
                        traces = TracesList
                    },
                    ExpressCompanyName = order.ExpressCompanyName,
                    ShipOrderNumber = order.ShipOrderNumber,
                    ShipTo = order.ShipTo,
                    CellPhone = order.CellPhone,
                    Address = order.RegionFullName + order.Address
                };
                return JsonResult<dynamic>(data: data);
            }
            catch (Exception e)
            {
                return Json(ErrorResult<dynamic>(e.Message));
            }
        }

        public JsonResult<Result<dynamic>> GetExpressList()
        {
            var express = ExpressApplication.GetAllExpress();
            var list = express.ToList().Select(item => new
            {
                ExpressName = item.Name,
                Kuaidi100Code = item.Kuaidi100Code,
                TaobaoCode = item.TaobaoCode
            });
            return JsonResult<dynamic>(list);
        }


        private bool HasAppendComment(OrderInfo list)
        {
            var item = list.OrderItemInfo.FirstOrDefault();
            var result = CommentApplication.HasAppendComment(item.Id);
            return result;
        }

        /// 订单提货码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult<Result<OrderListModel>> GetPickupGoods(long id)
        {
            CheckUserLogin();
            var orderInfo = OrderApplication.GetOrder(id);
            if (orderInfo == null)
                return Json(ErrorResult<OrderListModel>("订单不存在！"));
            if (orderInfo.UserId != CurrentUser.Id)
                return Json(ErrorResult<OrderListModel>("只能查看自己的提货码！"));
            //var productService = ServiceProvider.Instance<IProductService>.Create;
            AutoMapper.Mapper.CreateMap<Order, Himall.DTO.OrderListModel>();
            AutoMapper.Mapper.CreateMap<DTO.OrderItem, OrderItemListModel>();
            var orderModel = AutoMapper.Mapper.Map<Order, Himall.DTO.OrderListModel>(orderInfo);
            var orderItems = OrderApplication.GetOrderItemsByOrderId(orderInfo.Id);
            var newOrderItems = new List<DTO.OrderItem>();
            foreach (var item in orderItems)
            {
                item.ThumbnailsUrl = Core.HimallIO.GetRomoteProductSizeImage(ProductManagerApplication.GetProduct(item.ProductId).RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_50);
                newOrderItems.Add(item);
            }
            orderModel.OrderItemList = AutoMapper.Mapper.Map<List<DTO.OrderItem>, List<OrderItemListModel>>(newOrderItems);
            if (orderInfo.ShopBranchId.HasValue && orderInfo.ShopBranchId.Value != 0)
            {//补充数据
                var branch = ShopBranchApplication.GetShopBranchById(orderInfo.ShopBranchId.Value);
                orderModel.ShopBranchName = branch.ShopBranchName;
                orderModel.ShopBranchAddress = branch.AddressFullName;
                orderModel.ShopBranchContactPhone = branch.ContactPhone;
            }

            return JsonResult(orderModel);
        }

        public JsonResult<Result<List<BonuModel>>> GetOrderBonus(string orderIds)
        {
            CheckUserLogin();
            List<BonuModel> bonus = new List<BonuModel>();
            string orderids = orderIds;
            string[] orderArray = orderids.Split(',');
            foreach (string item in orderArray)
            {
                long orderid = 0;
                if (long.TryParse(item, out orderid))
                {
                    var bonuInfo = ShopBonusApplication.GetGrantByUserOrder(orderid, CurrentUser.Id);
                    if (bonuInfo != null)
                    {
                        BonuModel bonuObject = new BonuModel();
                        bonuObject.ShareHref = CurrentUrlHelper.CurrentUrlNoPort() + "/m-weixin/shopbonus/index/" + bonuInfo.Id;
                        bonuObject.ShareCount = bonuInfo.Himall_ShopBonus.Count;
                        bonuObject.ShareDetail = bonuInfo.Himall_ShopBonus.ShareDetail;
                        bonuObject.ShareTitle = bonuInfo.Himall_ShopBonus.ShareTitle;
                        bonuObject.ShopName = ShopApplication.GetShop(bonuInfo.Himall_ShopBonus.ShopId).ShopName;
                        bonus.Add(bonuObject);
                    }
                }
            }
            return JsonResult(bonus);
        }

        /// <summary>
        /// 获取订单状态
        /// <para>供支付时使用</para>
        /// </summary>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public JsonResult<Result<IEnumerable<MemberOrderGetStatusModel>>> GetOrerStatus(string orderIds)
        {
            CheckUserLogin();
            List<long> ordids = orderIds.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).Select(t => long.Parse(t)).ToList();
            IEnumerable<Order> orders = OrderApplication.GetOrders(ordids).ToList();
            var data = orders.Select(d =>
            {
                long activeId = 0, groupId = 0;
                if (d.OrderType == OrderInfo.OrderTypes.FightGroup)
                {
                    var fg = FightGroupApplication.GetFightGroupOrderStatusByOrderId(d.Id);
                    if (fg != null && fg.ActiveId.HasValue && fg.GroupId.HasValue)
                    {
                        activeId = fg.ActiveId.Value;
                        groupId = fg.GroupId.Value;
                    }
                }
                return new MemberOrderGetStatusModel
                {
                    orderId = d.Id,
                    status = d.OrderStatus.GetHashCode(),
                    activeId = activeId,
                    groupId = groupId

                };
            });
            return JsonResult(data);
        }

        public JsonResult<Result<string>> GetPickupCodeQRCode(string pickupCode)
        {
            string result = "";
            if (!string.IsNullOrWhiteSpace(pickupCode))
            {
                var qrcode = QRCodeHelper.Create(pickupCode);

                Bitmap bmp = new Bitmap(qrcode);
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                qrcode.Dispose();
                result = Convert.ToBase64String(arr);
                result = "data:image/png;base64," + result;
            }
            return JsonResult(result);
        }
    }
}
