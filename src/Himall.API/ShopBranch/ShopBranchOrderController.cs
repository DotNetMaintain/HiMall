using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Himall.DTO.QueryModel;
using Himall.Model;
using Himall.Application;
using Himall.IServices;
using Himall.Core;
using Himall.DTO;
using System.Web;
using Himall.API.Model.ParamsModel;
using Himall.Web.Framework;
using Himall.CommonModel;
using Himall.API.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Himall.CommonModel.Enum;

namespace Himall.API
{
    /// <summary>
    /// 门店订单类
    /// </summary>
    public class ShopBranchOrderController : BaseShopBranchApiController
    {
        /// <summary>
        /// 根据提货码取订单
        /// </summary>
        /// <param name="pickcode"></param>
        /// <returns></returns>
        public object GetShopBranchOrder(string pickcode)
        {
            CheckUserLogin();
            var order = Application.OrderApplication.GetOrderByPickCode(pickcode);

            if (order == null)
                return new { success = false, msg = "该提货码无效" };
            if (order.ShopBranchId.Value != CurrentShopBranch.Id)
                return new { success = false, msg = "非本门店提货码，请买家核对提货信息" };
            if (order.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.Finish && order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake)
                return new { success = false, msg = "该提货码于" + order.FinishDate.ToString() + "已核销" };

            var orderItem = Application.OrderApplication.GetOrderItemsByOrderId(order.Id);
            foreach (var item in orderItem)
            {
                item.ThumbnailsUrl = Core.HimallIO.GetRomoteProductSizeImage(item.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_100);
                ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetTypeByProductId(item.ProductId);
                item.ColorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                item.SizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                item.VersionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
            }
            //退款状态
            var refundobjs = OrderApplication.GetOrderRefunds(orderItem.Select(e => e.Id));
            //小于4表示商家未确认；与平台未审核，都算退款、退货中
            var refundProcessing = refundobjs.Where(e => (int)e.SellerAuditStatus > 4 && ((int)e.SellerAuditStatus < 4 || e.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm));
            if (refundProcessing.Count() > 0)
                order.RefundStats = 1;

            return new { success = true, order = order, orderItem = orderItem };
        }
        /// <summary>
        /// 门店核销订单
        /// </summary>
        /// <param name="pickcode"></param>
        /// <returns></returns>
        public object GetShopBranchOrderConfirm(string pickcode)
        {
            CheckUserLogin();
            var order = Application.OrderApplication.GetOrderByPickCode(pickcode);

            if (order == null)
                return new { success = false, msg = "该提货码无效" };
            if (order.ShopBranchId.Value != CurrentShopBranch.Id)
                return new { success = false, msg = "非本门店提货码，请买家核对提货信息" };
            if (order.OrderStatus == Himall.Model.OrderInfo.OrderOperateStatus.Finish && order.DeliveryType == CommonModel.Enum.DeliveryType.SelfTake)
                return new { success = false, msg = "该提货码于" + order.FinishDate.ToString() + "已核销" };
            if (order.OrderStatus != Himall.Model.OrderInfo.OrderOperateStatus.WaitSelfPickUp)
                return new { success = false, msg = "只有待自提的订单才能进行核销" };

            Application.OrderApplication.ShopBranchConfirmOrder(order.Id, CurrentShopBranch.Id, this.CurrentUser.UserName);

            return new { success = true, msg = "已核销" };
        }

        /// <summary>
        /// 搜索门店订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<DTO.FullOrder> PostSearchShopBranchOrder(OrderQuery query)
        {
            if (query.PageNo < 1)
                query.PageNo = 1;
            if (query.PageSize < 1)
                query.PageSize = 10;

            CheckUserLogin();

            query.ShopBranchId = CurrentShopBranch.Id;

            var status = new[] {
                OrderInfo.OrderOperateStatus.WaitPay,
                 OrderInfo.OrderOperateStatus.WaitDelivery,
                  OrderInfo.OrderOperateStatus.WaitReceiving,
                OrderInfo.OrderOperateStatus.WaitSelfPickUp,
                OrderInfo.OrderOperateStatus.Finish,
                OrderInfo.OrderOperateStatus.Close
            };
            if (query.Status == null || !status.Contains(query.Status.Value))//门店只能查询这几种状态的订单
                query.Status = OrderInfo.OrderOperateStatus.WaitSelfPickUp;

            var data = OrderApplication.GetFullOrders(query);

            return data.Models;
        }

        public object GetShopBranchOrderCount()
        {
            CheckUserLogin();
            long shopid = CurrentShopBranch.ShopId;
            long sbid = CurrentUser.ShopBranchId;

            var waitPayCount = OrderApplication.GetWaitingForPayOrders(shopId: shopid, shopBranchId: sbid);
            var waitSelfPickUp = OrderApplication.GetWaitingForSelfPickUp(shopid, sbid);
            var waitReceive = OrderApplication.GetWaitingForReceive(shopid, sbid);
            var waitDelivery = OrderApplication.GetWaitingForDelivery(shopid, sbid);
            return new { success = true, waitPayCount = waitPayCount, waitReceive = waitReceive, waitDelivery = waitDelivery, waitSelfPickUp = waitSelfPickUp };
        }

        /// <summary>
        /// 获取订单信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public object GetOrderInfo(long orderId)
        {
            AutoMapper.Mapper.CreateMap<DTO.Order, ShopBranchOrderGetOrderModel>();
            var data = Application.OrderApplication.GetOrder(orderId);
            ShopBranchOrderGetOrderModel result = AutoMapper.Mapper.Map<DTO.Order, ShopBranchOrderGetOrderModel>(data);
            result.CanDaDaExpress = CityExpressConfigApplication.GetStatus(data.ShopId);
            return result;
        }
        /// <summary>
        /// 获取所有快递公司名称
        /// </summary>
        /// <returns></returns>
        public string GetAllExpress()
        {
            var listData = ExpressApplication.GetAllExpress().Select(i => i.Name);
            return String.Join(",", listData);
        }
        /// <summary>
        /// 门店发货
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public object PostShopSendGood(OrderDeliveryModel model)
        {
            CheckUserLogin();
            string shopkeeperName = "";
            long shopid = CurrentShopBranch.ShopId;
            long sbid = CurrentUser.ShopBranchId;
            shopkeeperName = CurrentShopBranch.UserName;
            var returnurl = String.Format("{0}/Common/ExpressData/SaveExpressData", CurrentUrlHelper.CurrentUrlNoPort());
            if (model.deliveryType == DeliveryType.CityExpress.GetHashCode())  //同城物流
            {
                var dadaconfig = CityExpressConfigApplication.GetDaDaCityExpressConfig(shopid);
                if (!dadaconfig.IsEnable)
                {
                    throw new HimallApiException("未开启同城合作物流");
                }
                var order = OrderApplication.GetOrder(model.orderId);
                if (order == null || order.ShopId != shopid || !(order.ShopBranchId > 0) || order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
                {
                    throw new HimallApiException("错误的订单编号");
                }
                var sbdata = ShopBranchApplication.GetShopBranchById(sbid);

                string json = ExpressDaDaHelper.addAfterQuery(shopid, sbdata.DaDaShopId, model.shipOrderNumber);
                var resultObj = JsonConvert.DeserializeObject(json) as JObject;
                string status = resultObj["status"].ToString();
                if (status != "success")
                {
                    //订单码过期，重发单
                    json = SendDaDaExpress(model.orderId, shopid, sbid, false);
                    var rObj2 = JsonConvert.DeserializeObject(json) as JObject;
                    string status2 = rObj2["status"].ToString();
                    if (status2 != "success")
                    {
                        string msg = rObj2["msg"].ToString();
                        return ErrorResult(msg);
                    }
                }
            }
            OrderApplication.ShopSendGood(model.orderId, model.deliveryType, shopkeeperName, model.companyName, model.shipOrderNumber, returnurl);
            return SuccessResult("发货成功");
        }

        /// <summary>
        /// 订单是否正在申请售后
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public object GetIsOrderAfterService(long orderId)
        {
            bool isAfterService = Application.OrderApplication.IsOrderAfterService(orderId);
            if (isAfterService)
            {
                return new { success = true, isAfterService = true };
            }
            else
            {
                return new { success = true, isAfterService = false };
            }
        }

        /// <summary>
        /// 查看订单物流
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public object GetLogisticsData(string expressCompanyName, string shipOrderNumber, long orderid)
        {
            var order = OrderApplication.GetOrder(orderid);
            if (order.DeliveryType == DeliveryType.CityExpress)
            {
                float StoreLat = 0, Storelng = 0;
                if (order == null)
                {
                    throw new HimallException("错误的订单编号");
                }
                if (order.ShopBranchId > 0)
                {
                    var sbdata = ShopBranchApplication.GetShopBranchById(order.ShopBranchId.Value);
                    if (sbdata != null)
                    {
                        StoreLat = sbdata.Latitude;
                        Storelng = sbdata.Longitude;
                    }
                }
                else
                {
                    var shopshiper = ShopShippersApplication.GetDefaultSendGoodsShipper(order.ShopId);
                    if (shopshiper != null && shopshiper.Latitude.HasValue && shopshiper.Longitude.HasValue)
                    {
                        StoreLat = shopshiper.Latitude.Value;
                        Storelng = shopshiper.Longitude.Value;
                    }
                }
                return new
                {
                    success = true,
                    deliveryType = DeliveryType.CityExpress.GetHashCode(),
                    userLat = order.ReceiveLatitude,
                    userLng = order.ReceiveLongitude,
                    storeLat = StoreLat,
                    Storelng = Storelng,
                    shipOrderNumber = order.ShipOrderNumber,
                };
            }
            else
            {
                var expressData = Application.OrderApplication.GetExpressData(expressCompanyName, shipOrderNumber);
                if (expressData != null)
                {
                    if (expressData.Success)
                        expressData.ExpressDataItems = expressData.ExpressDataItems.OrderByDescending(item => item.Time);//按时间逆序排列
                    var json = new
                    {
                        success = expressData.Success,
                        msg = expressData.Message,
                        data = expressData.ExpressDataItems.Select(item => new
                        {
                            time = item.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                            content = item.Content
                        })
                    };
                    return json;
                }
                else
                {
                    var json = new
                    {
                        success = false,
                        msg = "无物流信息"
                    };
                    return json;
                }

            }
        }

        public object GetOrderDetail(long id)
        {
            CheckUserLogin();
            long shopid = CurrentShopBranch.ShopId;
            long sbid = CurrentUser.ShopBranchId;

            var ordser = ServiceProvider.Instance<IOrderService>.Create;

            OrderInfo order = ordser.GetOrder(id);
            if (order == null || order.ShopBranchId != sbid)
            {
                throw new HimallApiException("错误的订单编号");
            }
            var bonusService = ServiceProvider.Instance<IShopBonusService>.Create;
            var orderRefundService = ServiceProvider.Instance<IRefundService>.Create;
            var shopService = ServiceProvider.Instance<IShopService>.Create;
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var vshop = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(order.ShopId);
            bool isCanApply = false;
            //获取订单商品项数据
            var orderDetail = new
            {
                ShopName = shopService.GetShop(order.ShopId).ShopName,
                ShopId = order.ShopId,
                OrderItems = order.OrderItemInfo.Select(item =>
                {
                    var productinfo = productService.GetProduct(item.ProductId);
                    if (order.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery)
                    {
                        isCanApply = orderRefundService.CanApplyRefund(id, item.Id);
                    }
                    else
                    {
                        isCanApply = orderRefundService.CanApplyRefund(id, item.Id, false);
                    }

                    ProductTypeInfo typeInfo = ServiceProvider.Instance<ITypeService>.Create.GetType(productinfo.TypeId);
                    string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
                    string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
                    string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;
                    return new
                    {
                        ItemId = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Count = item.Quantity,
                        Price = item.SalePrice,
                        //ProductImage = "http://" + Url.Request.RequestUri.Host + productService.GetProduct(item.ProductId).GetImage(ProductInfo.ImageSize.Size_100),
                        ProductImage = Core.HimallIO.GetRomoteProductSizeImage(productService.GetProduct(item.ProductId).RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_100),
                        color = item.Color,
                        size = item.Size,
                        version = item.Version,
                        IsCanRefund = isCanApply,
                        ColorAlias = colorAlias,
                        SizeAlias = sizeAlias,
                        VersionAlias = versionAlias
                    };
                })
            };
            var orderModel = new
            {
                Id = order.Id,
                OrderType = order.OrderType,
                OrderTypeName = order.OrderType.ToDescription(),
                Status = order.OrderStatus.ToDescription(),
                ShipTo = order.ShipTo,
                Phone = order.CellPhone,
                Address = order.RegionFullName + " " + order.Address,
                HasExpressStatus = !string.IsNullOrWhiteSpace(order.ShipOrderNumber),
                ExpressCompanyName = order.ExpressCompanyName,
                Freight = order.Freight,
                IntegralDiscount = order.IntegralDiscount,
                RealTotalAmount = order.OrderTotalAmount - order.RefundTotalAmount,
                CapitalAmount = order.CapitalAmount,
                RefundTotalAmount = order.RefundTotalAmount,
                OrderDate = order.OrderDate.ToString("yyyy-MM-dd HH:mm:ss"),
                ShopName = order.ShopName,
                ShopBranchName = CurrentShopBranch.ShopBranchName,
                VShopId = vshop == null ? 0 : vshop.Id,
                commentCount = order.OrderCommentInfo.Count(),
                ShopId = order.ShopId,
                orderStatus = (int)order.OrderStatus,
                Invoice = order.InvoiceType.ToDescription(),
                InvoiceValue = (int)order.InvoiceType,
                InvoiceContext = order.InvoiceContext,
                InvoiceTitle = order.InvoiceTitle,
                PaymentType = order.PaymentType.ToDescription(),
                PaymentTypeValue = (int)order.PaymentType,
                FullDiscount = order.FullDiscount,
                DiscountAmount = order.DiscountAmount,
                OrderRemarks = order.OrderRemarks,
                DeliveryType = order.DeliveryType,
                InvoiceCode = order.InvoiceCode
            };
            return new { success = true, Order = orderModel, OrderItem = orderDetail.OrderItems };
        }

        public object GetCancelDadaExpress(long orderId, int reasonId, string cancelReason)
        {
            CheckUserLogin();
            long shopid = CurrentShopBranch.ShopId;
            long sbid = CurrentUser.ShopBranchId;
            var order = OrderApplication.GetOrder(orderId);
            if (order == null || order.ShopBranchId != sbid || order.OrderStatus != OrderInfo.OrderOperateStatus.WaitReceiving || order.DeliveryType != DeliveryType.CityExpress)
            {
                throw new HimallApiException("错误的订单编号");
            }
            if (order.DadaStatus > DadaStatus.WaitTake.GetHashCode())
            {
                throw new HimallApiException("订单配送不可取消！");
            }
            var sbdata = ShopBranchApplication.GetShopBranchById(sbid);
            string json = ExpressDaDaHelper.orderFormalCancel(shopid, orderId.ToString(), reasonId, cancelReason);
            var resultObj = JsonConvert.DeserializeObject(json) as JObject;
            string status = resultObj["status"].ToString();
            if (status != "success")
            {
                throw new HimallApiException(resultObj["msg"].ToString());
            }
            ExpressDaDaApplication.SetOrderCancel(orderId, "商家主动取消");
            var result = JsonConvert.DeserializeObject(resultObj["result"].ToString()) as JObject;
            return new
            {
                success = true,
                deduct_fee = result["deduct_fee"].ToString()
            };
        }

        public object GetCityExpressDaDa(long orderId)
        {
            CheckUserLogin();
            long shopid = CurrentShopBranch.ShopId;
            long sbid = CurrentUser.ShopBranchId;

            string json = SendDaDaExpress(orderId, shopid, sbid, true);
            var resultObj = JsonConvert.DeserializeObject(json) as JObject;
            string status = resultObj["status"].ToString();
            if (status != "success")
            {
                throw new HimallApiException(resultObj["msg"].ToString());
            }
            var result = JsonConvert.DeserializeObject(resultObj["result"].ToString()) as JObject;

            return new
            {
                success = true,
                distance = result["distance"].ToString(),
                fee = result["fee"].ToString(),
                deliveryNo = result["deliveryNo"].ToString(),
            };
        }
        public object GetDadaCancelReasons()
        {
            CheckUserLogin();
            long shopid = CurrentShopBranch.ShopId;
            long sbid = CurrentUser.ShopBranchId;
            string json = ExpressDaDaHelper.orderCancelReasons(shopid);
            var resultObj = JsonConvert.DeserializeObject(json) as JObject;
            return resultObj;
        }

        private string SendDaDaExpress(long orderId, long shopid, long sbid, bool isQueryOrder)
        {
            var order = OrderApplication.GetOrder(orderId);
            if (order == null || order.ShopBranchId != sbid || order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
            {
                throw new HimallApiException("错误的订单编号");
            }
            var dadaconfig = CityExpressConfigApplication.GetDaDaCityExpressConfig(shopid);
            if (!dadaconfig.IsEnable)
            {
                throw new HimallApiException("未开启同城合作物流");
            }
            if (order.ReceiveLatitude <= 0 || order.ReceiveLongitude <= 0)
            {
                throw new HimallApiException("未获取到客户收货地址坐标信息，无法使用该配送方式");
            }
            var sbdata = ShopBranchApplication.GetShopBranchById(sbid);
            if (sbdata == null || string.IsNullOrWhiteSpace(sbdata.DaDaShopId))
            {
                throw new HimallApiException("门店未在达达注册，或所在城市达达不支持配送，无法发单，请商家在后台进行设置");
            }
            string cityCode = "";
            var _adregion = RegionApplication.GetRegion(order.RegionId);
            var _city = GetCity(_adregion);
            try
            {
                string cityJson = ExpressDaDaHelper.cityCodeList(shopid);
                var cityObj = JsonConvert.DeserializeObject(cityJson) as JObject;
                JArray citylist = (JArray)cityObj["result"];
                foreach (JToken item in citylist)
                {
                    if (_city.ShortName == item["cityName"].ToString())
                    {
                        cityCode = item["cityCode"].ToString();
                        break;
                    }
                }

            }
            catch
            {
            }
            //达达不支持的城市
            if (cityCode == "")
            {
                throw new HimallApiException("配送范围超区，无法配送");
            }
            string callback = CurrentUrlHelper.CurrentUrl() + "/pay/dadaOrderNotify/";
            bool isreaddorder = (order.DadaStatus == DadaStatus.Cancel.GetHashCode());
            if (isQueryOrder)
            {
                isreaddorder = false;
            }
            string json = ExpressDaDaHelper.addOrder(shopid, sbdata.DaDaShopId, order.Id.ToString()
                , cityCode, (double)order.TotalAmount, 0, ExpressDaDaHelper.DateTimeToUnixTimestamp(DateTime.Now.AddMinutes(15))
                , order.ShipTo, order.Address, order.ReceiveLatitude, order.ReceiveLongitude
                , callback, order.CellPhone, order.CellPhone, isQueryDeliverFee: isQueryOrder
                , isReAddOrder: isreaddorder);
            return json;
        }

        /// <summary>
        /// 获取市级地区
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        private CommonModel.Region GetCity(CommonModel.Region region)
        {
            CommonModel.Region _city = region;
            if (_city.Level == CommonModel.Region.RegionLevel.City || _city.Level == CommonModel.Region.RegionLevel.Province || _city.Parent == null)
            {
                return _city;
            }
            _city = _city.Parent;
            return GetCity(_city);
        }
    }
}
