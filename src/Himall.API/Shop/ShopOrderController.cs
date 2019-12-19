using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Himall.DTO.QueryModel;
using Himall.Application;
using Himall.Model;
using Himall.IServices;
using System.Web;
using Himall.Core;
using Himall.Web.Framework;
using Himall.CommonModel;
using Himall.API.Model.ParamsModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Himall.CommonModel.Enum;

namespace Himall.API
{
    public class ShopOrderController : BaseShopApiController
    {
        /// <summary>
        /// 搜索门店订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public List<DTO.FullOrder> PostSearchShopOrder(OrderQuery query)
        {
            if (query.PageNo < 1)
                query.PageNo = 1;
            if (query.PageSize < 1)
                query.PageSize = 10;

            CheckShopManageLogin();
            query.Operator = Operator.Seller;
            query.ShopId = CurrentShop.Id;

            var data = Application.OrderApplication.GetFullOrders(query);


            return data.Models;
        }

        public object GetShopBranchs()
        {
            CheckUserLogin();
            var branchs = ShopBranchApplication.GetShopBranchByShopId(CurrentUser.ShopId);
            return new { success = true, branchs = branchs };
        }

        public object GetShopOrderCount()
        {
            CheckUserLogin();
            var waitPayCount = OrderApplication.GetWaitingForPayOrders(CurrentUser.ShopId);
            var waitReceive = OrderApplication.GetWaitingForReceive(CurrentUser.ShopId);
            var waitDelivery = OrderApplication.GetWaitingForDelivery(CurrentUser.ShopId);
            var waitSelfPickUp = OrderApplication.GetWaitingForSelfPickUp(CurrentUser.ShopId);
            return new { success = true, waitPayCount = waitPayCount, waitReceive = waitReceive, waitDelivery = waitDelivery, waitSelfPickUp = waitSelfPickUp };
        }

        public object GetOrderDetail(long id)
        {
            CheckUserLogin();
            long shopid = CurrentUser.ShopId;

            var ordser = ServiceProvider.Instance<IOrderService>.Create;

            OrderInfo order = ordser.GetOrder(id);
            if (order == null || order.ShopId != shopid)
            {
                throw new HimallApiException("错误的订单编号");
            }
            var bonusService = ServiceProvider.Instance<IShopBonusService>.Create;
            var orderRefundService = ServiceProvider.Instance<IRefundService>.Create;
            var shopService = ServiceProvider.Instance<IShopService>.Create;
            var productService = ServiceProvider.Instance<IProductService>.Create;
            var vshop = ServiceProvider.Instance<IVShopService>.Create.GetVShopByShopId(order.ShopId);
            bool isCanApply = false;
            DTO.ShopBranch ShopBranchInfo = null;
            if (order.ShopBranchId.HasValue && order.ShopBranchId.Value > 0)
            {
                ShopBranchInfo = ShopBranchApplication.GetShopBranchById(order.ShopBranchId.Value);
            }
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
                ShopBranchName = (ShopBranchInfo != null ? ShopBranchInfo.ShopBranchName : ""),
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

        /// <summary>
        /// 商家发货
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public object PostShopSendGood(OrderDeliveryModel model)
        {
            CheckUserLogin();
            long shopid = CurrentShop.Id;
            string shopkeeperName = "";
            shopkeeperName = CurrentUser.UserName;
            var returnurl = String.Format("{0}/Common/ExpressData/SaveExpressData", CurrentUrlHelper.CurrentUrlNoPort());
            if (model.deliveryType == DeliveryType.CityExpress.GetHashCode())  //同城物流
            {
                var dadaconfig = CityExpressConfigApplication.GetDaDaCityExpressConfig(shopid);
                if (!dadaconfig.IsEnable)
                {
                    throw new HimallApiException("未开启同城合作物流");
                }
                var order = OrderApplication.GetOrder(model.orderId);
                if (order == null || order.ShopId != shopid || order.ShopBranchId > 0 || order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
                {
                    throw new HimallApiException("错误的订单编号");
                }
                string json = ExpressDaDaHelper.addAfterQuery(shopid, dadaconfig.source_id, model.shipOrderNumber);
                var resultObj = JsonConvert.DeserializeObject(json) as JObject;
                string status = resultObj["status"].ToString();
                if (status != "success")
                {
                    //订单码过期，重发单
                    json = SendDaDaExpress(model.orderId, shopid, false);
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
        public object GetCancelDadaExpress(long orderId, int reasonId, string cancelReason)
        {
            CheckUserLogin();
            long shopid = CurrentShop.Id;
            var order = OrderApplication.GetOrder(orderId);
            if (order == null || order.ShopId != shopid || order.ShopBranchId > 0 || order.OrderStatus != OrderInfo.OrderOperateStatus.WaitReceiving || order.DeliveryType != DeliveryType.CityExpress)
            {
                throw new HimallApiException("错误的订单编号");
            }
            if (order.DadaStatus > DadaStatus.WaitTake.GetHashCode())
            {
                throw new HimallApiException("订单配送不可取消！");
            }
            var dadaconfig = CityExpressConfigApplication.GetDaDaCityExpressConfig(shopid);
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
            long shopid = CurrentShop.Id;
            string json = SendDaDaExpress(orderId, shopid, true);
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
            long shopid = CurrentShop.Id;
            string json = ExpressDaDaHelper.orderCancelReasons(shopid);
            var resultObj = JsonConvert.DeserializeObject(json) as JObject;
            return resultObj;
        }
        private string SendDaDaExpress(long orderId, long shopid, bool isQueryOrder)
        {
            var order = OrderApplication.GetOrder(orderId);
            if (order == null || order.ShopId != shopid || order.ShopBranchId > 0 || order.OrderStatus != OrderInfo.OrderOperateStatus.WaitDelivery)
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
            var shopsend = ShopShippersApplication.GetDefaultSendGoodsShipper(shopid);
            if (shopsend == null || shopsend.Latitude <= 0 || shopsend.Longitude <= 0)
            {
                throw new HimallApiException("店铺没有发货地址或发货地址没有坐标信息，无法发单，请前往后台进行设置");
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
            string json = ExpressDaDaHelper.addOrder(shopid, dadaconfig.source_id, order.Id.ToString()
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
