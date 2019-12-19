using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.DTO;
using Himall.DTO.QueryModel;
using Himall.Model;
using Himall.SmallProgAPI.Model.ParamsModel;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;

namespace Himall.SmallProgAPI
{
    public class O2OOrderController : BaseO2OApiController
    {
        /// <summary>
        /// 获取购物车提交页面的数据
        /// </summary>
        /// <param name="cartItemIds">购物车物品id集合</param>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetSubmitByCartModel(string cartItemIds = "", long shippingAddressId = 0, string couponIds = "")
        {
            CheckUserLogin();
            var coupons = OrderHelper.ConvertUsedCoupon(couponIds);
            var result = OrderApplication.GetMobileSubmiteByCart(CurrentUserId, cartItemIds, shippingAddressId, coupons);
            //门店订单
            Himall.DTO.ShopBranch storeInfo = null;
            if (result.shopBranchInfo == null)
                throw new HimallException("获取门店信息失败，不可提交非门店商品");
            else
            {
                storeInfo = Application.ShopBranchApplication.GetShopBranchById(result.shopBranchInfo.Id);
            }

            //解决循环引用的序列化的问题
            dynamic address = new System.Dynamic.ExpandoObject();
            if (result.Address != null)
            {
                var addDetail = result.Address.AddressDetail ?? "";
                var add = new
                {
                    Id = result.Address.Id,
                    ShipTo = result.Address.ShipTo,
                    Phone = result.Address.Phone,
                    Address = result.Address.RegionFullName + " " + result.Address.Address + " " + addDetail,
                    RegionId = result.Address.RegionId
                };
                address = add;
            }
            else
                address = null;

            var siteconfig = SiteSettingApplication.GetSiteSettings();
            bool canIntegralPerMoney = true, canCapital = true;
            CanDeductible(out canIntegralPerMoney, out canCapital);

            return JsonResult<dynamic>(new
            {
                Address = address,
                IsCashOnDelivery = false,   //门店订单不支持货到付款
                InvoiceContext = result.InvoiceContext,
                InvoiceTitle = OrderApplication.GetInvoiceTitles(CurrentUserId),
                products = result.products,

                TotalAmount = result.totalAmount,
                Freight = result.Freight,
                orderAmount = result.orderAmount,
                shopBranchInfo = storeInfo,
                IsOpenStore = SiteSettingApplication.GetSiteSettings() != null && SiteSettingApplication.GetSiteSettings().IsOpenStore,
                capitalAmount = result.capitalAmount,
                ProvideInvoice = OrderApplication.HasProvideInvoiceShop(result.products.Select(s => s.shopId).Distinct()),

                integralPerMoney = result.integralPerMoney,
                userIntegralMaxDeductible = result.userIntegralMaxDeductible,
                integralPerMoneyRate = result.integralPerMoneyRate,
                userIntegralMaxRate = SiteSettingApplication.GetSiteSettings().IntegralDeductibleRate,
                userIntegrals = result.userIntegrals,
                TotalMemberIntegral = result.memberIntegralInfo.AvailableIntegrals,
                canIntegralPerMoney = canIntegralPerMoney,
                canCapital = canCapital
            });
        }

        private void CanDeductible(out bool canIntegralPerMoney, out bool canCapital)
        {
            //授权模块控制积分抵扣、余额抵扣功能是否开放
            canIntegralPerMoney = true;
            canCapital = true;
            var siteInfo = SiteSettingApplication.GetSiteSettings();
            if (siteInfo != null)
            {
                if (!(siteInfo.IsOpenPC || siteInfo.IsOpenH5 || siteInfo.IsOpenApp || siteInfo.IsOpenMallSmallProg || siteInfo.IsOpenMultiStoreSmallProg))
                {
                    canIntegralPerMoney = false;
                }
                if (!(siteInfo.IsOpenPC || siteInfo.IsOpenH5 || siteInfo.IsOpenApp || siteInfo.IsOpenMultiStoreSmallProg))
                {
                    canCapital = false;
                }
            }
        }

        /// <summary>
        /// 删除发票抬头
        /// </summary>
        /// <param name="id">抬头ID</param>
        /// <returns>是否完成</returns>
        public JsonResult<Result<int>> PostDeleteInvoiceTitle(PostDeleteInvoiceTitlePModel para)
        {
            CheckUserLogin();
            OrderApplication.DeleteInvoiceTitle(para.id, CurrentUserId);
            return JsonResult<int>();
        }
        /// <summary>
        /// 设置发票抬头
        /// </summary>
        /// <param name="name">抬头名称</param>
        /// <returns>返回抬头ID</returns>
        [HttpPost]
        public JsonResult<Result<long>> PostSaveInvoiceTitle(PostSaveInvoiceTitlePModel para)
        {
            CheckUserLogin();
            return JsonResult(OrderApplication.SaveInvoiceTitle(CurrentUserId, para.name, para.code));
        }

        /// <summary>
        /// 购物车方式提交的订单
        /// </summary>
        /// <param name="value">数据</param>
        public JsonResult<Result<dynamic>> PostSubmitOrderByCart(PostOrderSubmitOrderByCartModel value)
        {
            CheckUserLogin();
            if (value.CapitalAmount > 0 && !string.IsNullOrEmpty(value.PayPwd))
            {
                var flag = MemberApplication.VerificationPayPwd(((UserMemberInfo)CurrentUser).Id, value.PayPwd);
                if (!flag)
                {
                    return Json(ErrorResult<dynamic>("预付款支付密码错误"));
                }
            }
            string cartItemIds = value.cartItemIds;
            long recieveAddressId = value.recieveAddressId;
            string couponIds = value.couponIds;
            int integral = value.integral;

            bool isCashOnDelivery = value.isCashOnDelivery;
            int invoiceType = value.invoiceType;
            string invoiceTitle = value.invoiceTitle;
            string invoiceContext = value.invoiceContext;
            //end
            string orderRemarks = "";//value.orderRemarks;//订单备注
            OrderCreateModel model = new OrderCreateModel();
            IEnumerable<long> orderIds;
            model.PlatformType = PlatformType.WeiXinO2OSmallProg;
            model.formId = value.formId;
            model.CurrentUser = CurrentUser;
            model.ReceiveAddressId = recieveAddressId;
            model.Integral = integral;
            model.Capital = value.CapitalAmount;

            model.IsCashOnDelivery = isCashOnDelivery;
            model.Invoice = (InvoiceType)invoiceType;
            model.InvoiceContext = invoiceContext;
            model.InvoiceTitle = invoiceTitle;
            model.InvoiceCode = value.invoiceCode;
            //end
            CommonModel.OrderShop[] OrderShops = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderShop[]>(value.jsonOrderShops);
            model.OrderShops = OrderShops;//用户APP选择门店自提时用到，2.5版本未支持门店自提
            model.OrderRemarks = OrderShops.Select(p => p.Remark).ToArray();
            model.IsShopbranchOrder = true;
            try
            {
                var cartItemIdsArr = cartItemIds.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).Select(item => long.Parse(item)).ToArray();
                //根据购物车项补充sku数据
                var cartItems = CartApplication.GetCartItems(cartItemIdsArr);
                model.SkuIds = cartItems.Select(e => e.SkuId).ToList();
                model.Counts = cartItems.Select(e => e.Quantity).ToList();

                model.CartItemIds = cartItemIdsArr;
                model.CouponIdsStr = OrderHelper.ConvertUsedCoupon(couponIds);
                var orders = OrderApplication.CreateOrder(model);
                orderIds = orders.Select(item => item.Id).ToArray();
                decimal orderTotals = orders.Where(d => d.PaymentType != OrderInfo.PaymentTypes.CashOnDelivery).Sum(item => item.OrderTotalAmount);
                return JsonResult<dynamic>(new
                {
                    OrderIds = orderIds,
                    RealTotalIsZero = (orderTotals - model.Capital) == 0
                });
            }
            catch (HimallException he)
            {
                return Json(ErrorResult<dynamic>(he.Message));
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return Json(ErrorResult<dynamic>("提交订单异常"));
            }
        }

        /// <summary>
        /// 是否可用积分购买
        /// </summary>
        /// <param name="orderIds">订单id</param>
        public JsonResult<Result<int>> GetPayOrderByIntegral(string orderIds)
        {
            CheckUserLogin();
            OrderApplication.ConfirmOrder(CurrentUser.Id, orderIds);
            return JsonResult<int>();
        }
        public JsonResult<Result<List<OrderDetailView>>> GetOrderShareProduct(string orderids)
        {
            CheckUserLogin();
            if (string.IsNullOrWhiteSpace(orderids))
            {
                throw new HimallException("订单号不能为空！");
            }
            long orderId = 0;
            var ids = orderids.Split(',').Select(e =>
            {
                if (long.TryParse(e, out orderId))
                {
                    return orderId;
                }
                else
                {
                    return 0;
                }
            }
            );
            var orders = OrderApplication.GetOrderDetailViews(ids);
            return JsonResult(orders);
        }
        public JsonResult<Result<int>> PostOrderShareAddIntegral(PostOrderShareAddIntegralModel OrderIds)
        {
            CheckUserLogin();
            var orderids = OrderIds.orderids;
            if (string.IsNullOrWhiteSpace(orderids))
            {
                throw new HimallException("订单号不能为空！");
            }
            long orderId = 0;
            var ids = orderids.Split(',').Select(e =>
            {
                if (long.TryParse(e, out orderId))
                    return orderId;
                else
                    throw new HimallException("订单分享增加积分时，订单号异常！");
            }
            );
            if (MemberIntegralApplication.OrderIsShared(ids))
            {
                throw new HimallException("订单已经分享过！");
            }
            MemberIntegralRecord record = new MemberIntegralRecord();
            record.MemberId = CurrentUser.Id;
            record.UserName = CurrentUser.UserName;
            record.RecordDate = DateTime.Now;
            record.TypeId = MemberIntegral.IntegralType.Share;
            record.ReMark = string.Format("订单号:{0}", orderids);
            List<MemberIntegralRecordAction> recordAction = new List<MemberIntegralRecordAction>();

            foreach (var id in ids)
            {
                recordAction.Add(new MemberIntegralRecordAction
                {
                    VirtualItemId = id,
                    VirtualItemTypeId = MemberIntegral.VirtualItemType.ShareOrder
                });
            }
            record.Himall_MemberIntegralRecordAction = recordAction;
            MemberIntegralApplication.AddMemberIntegralByEnum(record, MemberIntegral.IntegralType.Share);
            return JsonResult<int>(msg: "晒单添加积分成功！");
        }

        /// <summary>
        /// 获取自提门店点
        /// </summary>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetShopBranchs(long shopId, bool getParent, string skuIds, string counts, int page, int rows, long shippingAddressId, long regionId)
        {
            string[] _skuIds = skuIds.Split(',');
            int[] _counts = counts.Split(',').Select(p => Himall.Core.Helper.TypeHelper.ObjectToInt(p)).ToArray();

            var shippingAddressInfo = ShippingAddressApplication.GetUserShippingAddress(shippingAddressId);
            int streetId = 0, districtId = 0;//收货地址的街道、区域

            var query = new ShopBranchQuery()
            {
                ShopId = shopId,
                PageNo = page,
                PageSize = rows,
                Status = ShopBranchStatus.Normal,
                ShopBranchProductStatus = ShopBranchSkuStatus.Normal
            };
            if (shippingAddressInfo != null)
            {
                query.FromLatLng = string.Format("{0},{1}", shippingAddressInfo.Latitude, shippingAddressInfo.Longitude);//需要收货地址的经纬度
                streetId = shippingAddressInfo.RegionId;
                var parentAreaInfo = RegionApplication.GetRegion(shippingAddressInfo.RegionId, Region.RegionLevel.Town);//判断当前区域是否为第四级
                if (parentAreaInfo != null && parentAreaInfo.ParentId > 0) districtId = parentAreaInfo.ParentId;
                else { districtId = streetId; streetId = 0; }
            }
            bool hasLatLng = false;
            if (!string.IsNullOrWhiteSpace(query.FromLatLng)) hasLatLng = query.FromLatLng.Split(',').Length == 2;

            var region = RegionApplication.GetRegion(regionId, getParent ? CommonModel.Region.RegionLevel.City : CommonModel.Region.RegionLevel.County);//同城内门店
            if (region != null) query.AddressPath = region.GetIdPath();

            #region 3.0版本排序规则
            var skuInfos = ProductManagerApplication.GetSKUs(_skuIds);
            query.ProductIds = skuInfos.Select(p => p.ProductId).ToArray();
            var data = ShopBranchApplication.GetShopBranchsAll(query);
            var shopBranchSkus = ShopBranchApplication.GetSkus(shopId, data.Models.Select(p => p.Id));//获取该商家下具有订单内所有商品的门店状态正常数据,不考虑库存
            data.Models.ForEach(p =>
            {
                p.Enabled = skuInfos.All(skuInfo => shopBranchSkus.Any(sbSku => sbSku.ShopBranchId == p.Id && sbSku.Stock >= _counts[skuInfos.IndexOf(skuInfo)] && sbSku.SkuId == skuInfo.Id));
            });

            List<Himall.DTO.ShopBranch> newList = new List<Himall.DTO.ShopBranch>();
            List<long> fillterIds = new List<long>();
            var currentList = data.Models.Where(p => hasLatLng && p.Enabled && (p.Latitude > 0 && p.Longitude > 0)).OrderBy(p => p.Distance).ToList();
            if (currentList != null && currentList.Count() > 0)
            {
                fillterIds.AddRange(currentList.Select(p => p.Id));
                newList.AddRange(currentList);
            }
            var currentList2 = data.Models.Where(p => !fillterIds.Contains(p.Id) && p.Enabled && p.AddressPath.Contains(CommonConst.ADDRESS_PATH_SPLIT + streetId + CommonConst.ADDRESS_PATH_SPLIT)).ToList();
            if (currentList2 != null && currentList2.Count() > 0)
            {
                fillterIds.AddRange(currentList2.Select(p => p.Id));
                newList.AddRange(currentList2);
            }
            var currentList3 = data.Models.Where(p => !fillterIds.Contains(p.Id) && p.Enabled && p.AddressPath.Contains(CommonConst.ADDRESS_PATH_SPLIT + districtId + CommonConst.ADDRESS_PATH_SPLIT)).ToList();
            if (currentList3 != null && currentList3.Count() > 0)
            {
                fillterIds.AddRange(currentList3.Select(p => p.Id));
                newList.AddRange(currentList3);
            }
            var currentList4 = data.Models.Where(p => !fillterIds.Contains(p.Id) && p.Enabled).ToList();//非同街、非同区，但一定会同市
            if (currentList4 != null && currentList4.Count() > 0)
            {
                fillterIds.AddRange(currentList4.Select(p => p.Id));
                newList.AddRange(currentList4);
            }
            var currentList5 = data.Models.Where(p => !fillterIds.Contains(p.Id)).ToList();//库存不足的排最后
            if (currentList5 != null && currentList5.Count() > 0)
            {
                newList.AddRange(currentList5);
            }
            if (newList.Count() != data.Models.Count())//如果新组合的数据与原数据数量不一致
            {
                return Json(ErrorResult<dynamic>("数据异常"));
            }
            var storeList = newList.Select(sb =>
            {
                return new
                {
                    ContactUser = sb.ContactUser,
                    ContactPhone = sb.ContactPhone,
                    AddressDetail = sb.AddressDetail,
                    ShopBranchName = sb.ShopBranchName,
                    Id = sb.Id,
                    Enabled = sb.Enabled
                };
            });

            #endregion

            return JsonResult<dynamic>(storeList);
        }

        /// <summary>
        /// 是否允许门店自提
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="regionId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public JsonResult<Result<bool>> GetExistShopBranch(long shopId, long regionId, string productIds)
        {
            var query = new ShopBranchQuery();
            query.Status = ShopBranchStatus.Normal;
            query.ShopId = shopId;

            var region = RegionApplication.GetRegion(regionId, CommonModel.Region.RegionLevel.City);
            query.AddressPath = region.GetIdPath();
            query.ProductIds = productIds.Split(',').Select(p => long.Parse(p)).ToArray();
            query.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
            var existShopBranch = ShopBranchApplication.Exists(query);
            return JsonResult(existShopBranch);
            //return new { success = true, ExistShopBranch = existShopBranch ? 1 : 0 };
        }

        public JsonResult<Result<dynamic>> GetOrderCommentProduct(long orderId)
        {
            CheckUserLogin();
            var order = OrderApplication.GetOrderInfo(orderId);
            if (order != null && order.OrderCommentInfo.Count == 0)
            {
                var model = CommentApplication.GetProductEvaluationByOrderId(orderId, CurrentUser.Id).Select(item => new
                {
                    OrderItemId = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    SkuContent = item.ColorAlias + ":" + item.Color + ";" + item.SizeAlias + ":" + item.Size + ";" + item.VersionAlias + ":" + item.Version,
                    Price = item.Price,
                    SkuId = item.SkuId,
                    //Image = "http://" + Url.Request.RequestUri.Host + item.ThumbnailsUrl
                    //Image = Core.HimallIO.GetRomoteImagePath(item.ThumbnailsUrl)
                    Image = Core.HimallIO.GetRomoteProductSizeImage(item.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_220) //商城App评论时获取商品图片
                });

                //var orderEvaluation = TradeCommentApplication.GetOrderCommentInfo(orderId, CurrentUser.Id);
                return JsonResult<dynamic>(model);
            }
            else
                return Json(ErrorResult<dynamic>("该订单不存在或者已评论过", new int[0]));
        }
    }
}
