using Himall.Application;
using Himall.CommonModel;
using Himall.DTO.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;

namespace Himall.SmallProgAPI
{
    public class O2OCouponController : BaseO2OApiController
    {
        /// <summary>
        /// 领取优惠券
        /// </summary>
        /// <returns></returns>
        public JsonResult<Result<int>> GetUserCoupon(string openId, long couponId)
        {
            CheckUserLogin();
            bool status = true;
            string message = "";
            var couponInfo = CouponApplication.GetCouponInfo(couponId);
            if (couponInfo.EndTime < DateTime.Now)
            {//已经失效
                status = false;
                message = "优惠券已经过期";
            }
            CouponRecordQuery crQuery = new CouponRecordQuery();
            crQuery.CouponId = couponId;
            crQuery.UserId = CurrentUser.Id;
            QueryPageModel<CouponRecordInfo> pageModel = CouponApplication.GetCouponRecordList(crQuery);
            if (couponInfo.PerMax != 0 && pageModel.Total >= couponInfo.PerMax)
            {//达到个人领取最大张数
                status = false;
                message = "达到领取最大张数";
            }
            crQuery = new CouponRecordQuery()
            {
                CouponId = couponId
            };
            pageModel = CouponApplication.GetCouponRecordList(crQuery);
            if (pageModel.Total >= couponInfo.Num)
            {//达到领取最大张数
                status = false;
                message = "此优惠券已经领完了";
            }
            if (couponInfo.ReceiveType == Himall.Model.CouponInfo.CouponReceiveType.IntegralExchange)
            {
                var userInte = MemberIntegralApplication.GetMemberIntegral(CurrentUserId);
                if (userInte.AvailableIntegrals < couponInfo.NeedIntegral)
                {
                    //积分不足
                    status = false;
                    message = "积分不足 ";
                }
            }
            if (status)
            {
                CouponRecordInfo couponRecordInfo = new CouponRecordInfo()
                {
                    CouponId = couponId,
                    UserId = CurrentUser.Id,
                    UserName = CurrentUser.UserName,
                    ShopId = couponInfo.ShopId
                };
                CouponApplication.AddCouponRecord(couponRecordInfo);
                return JsonResult<int>(msg: "领取成功");//执行成功
            }
            else
            {
                return Json(ErrorResult<int>(message));
            }
        }
        private IEnumerable<ShopBonusReceiveInfo> GetBonusList()
        {
           // var service = ServiceProvider.Instance<IShopBonusService>.Create;
            return ShopBonusApplication.GetDetailByUserId(CurrentUser.Id);
        }
        /// <summary>
        /// 获取用户优惠券列表
        /// </summary>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetLoadCoupon()
        {
            CheckUserLogin();
            //var service = ServiceProvider.Instance<ICouponService>.Create;
            //var vshop = ServiceProvider.Instance<IVShopService>.Create;
            var userCouponList = CouponApplication.GetUserCouponList(CurrentUser.Id);
            var shopBonus = GetBonusList();
            if (userCouponList != null || shopBonus != null)
            {
                //优惠券
                var couponlist = new Object();
                if (userCouponList != null)
                {
                    couponlist = userCouponList.ToArray().Select(a => new
                    {
                        UserId = a.UserId,
                        ShopId = a.ShopId,
                        CouponId = a.CouponId,
                        Price = a.Price,
                        PerMax = a.PerMax,
                        OrderAmount = a.OrderAmount,
                        Num = a.Num,
                        StartTime = a.StartTime.ToString(),
                        EndTime = a.EndTime.ToString(),
                        CreateTime = a.CreateTime.ToString(),
                        CouponName = a.CouponName,
                        UseStatus = a.UseStatus,
                        UseTime = a.UseTime.HasValue ? a.UseTime.ToString() : null,
                        //VShop = GetVShop(a.ShopId),
                        ShopName = a.ShopName,
                        Remark = a.Remark,
                        UseArea = a.UseArea
                    });
                }
                else
                    couponlist = null;
                //代金红包
                var userBonus = new object();
                if (shopBonus != null)
                {
                    userBonus = shopBonus.ToArray().Select(item =>
                    {
                        var Price = item.Price.Value;
                        var showOrderAmount = item.Himall_ShopBonusGrant.Himall_ShopBonus.UsrStatePrice > 0 ? item.Himall_ShopBonusGrant.Himall_ShopBonus.UsrStatePrice : item.Price.Value;
                        if (item.Himall_ShopBonusGrant.Himall_ShopBonus.UseState != ShopBonusInfo.UseStateType.FilledSend)
                            showOrderAmount = item.Price.Value;
                        var Logo = string.Empty;
                        long VShopId = 0;
                        long ShopId = 0;
                        if (item.Himall_ShopBonusGrant.Himall_ShopBonus.Himall_Shops.Himall_VShop != null && item.Himall_ShopBonusGrant.Himall_ShopBonus.Himall_Shops.Himall_VShop.Count() > 0)
                        {
                            //Logo ="http://" + Url.Request.RequestUri.Host+ item.Himall_ShopBonusGrant.Himall_ShopBonus.Himall_Shops.Himall_VShop.FirstOrDefault().Logo;
                            Logo = Core.HimallIO.GetRomoteImagePath(item.Himall_ShopBonusGrant.Himall_ShopBonus.Himall_Shops.Himall_VShop.FirstOrDefault().Logo);
                            VShopId = item.Himall_ShopBonusGrant.Himall_ShopBonus.Himall_Shops.Himall_VShop.FirstOrDefault().Id;
                        }

                        var State = (int)item.State;
                        if (item.State != ShopBonusReceiveInfo.ReceiveState.Use && item.Himall_ShopBonusGrant.Himall_ShopBonus.DateEnd < DateTime.Now)
                            State = (int)ShopBonusReceiveInfo.ReceiveState.Expired;
                        var BonusDateEnd = item.Himall_ShopBonusGrant.Himall_ShopBonus.BonusDateEnd.ToString("yyyy-MM-dd");
                        ShopId = item.Himall_ShopBonusGrant.Himall_ShopBonus.ShopId;

                        return new
                        {
                            Price = Price,
                            ShowOrderAmount = showOrderAmount,
                            Logo = Logo,
                            VShopId = VShopId,
                            State = State,
                            BonusDateEnd = BonusDateEnd,
                            ShopName = item.BaseShopName,
                            ShopId = ShopId,
                        };
                    });
                }
                else
                    shopBonus = null;
                //优惠券
                int NoUseCouponCount = 0;
                int UseCouponCount = 0;
                if (userCouponList != null)
                {
                    NoUseCouponCount = userCouponList.Count(item => (item.EndTime > DateTime.Now && item.UseStatus == CouponRecordInfo.CounponStatuses.Unuse));
                    UseCouponCount = userCouponList.Count() - NoUseCouponCount;
                }
                //红包
                int NoUseBonusCount = 0;
                int UseBonusCount = 0;
                if (shopBonus != null)
                {
                    NoUseBonusCount = shopBonus.Count(r => r.State == ShopBonusReceiveInfo.ReceiveState.NotUse && r.Himall_ShopBonusGrant.Himall_ShopBonus.DateEnd > DateTime.Now);
                    UseBonusCount = shopBonus.Count() - NoUseBonusCount;
                }

                int UseCount = UseCouponCount + UseBonusCount;
                int NotUseCount = NoUseCouponCount + NoUseBonusCount;

                var result = new
                {
                    success = true,
                    NoUseCount = NotUseCount,
                    UserCount = UseCount,
                    Coupon = couponlist,
                    Bonus = userBonus
                };
                return JsonResult<dynamic>(result);
            }
            else
            {
                throw new Himall.Core.HimallException("没有领取记录!");
            }
        }

        /// <summary>
        /// 获取系统可领取优惠券列表
        /// </summary>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetLoadSiteCoupon(string openId = "", int pageSize = 10, int pageIndex = 1, int obtainWay = 1)
        {
            CheckUserLogin();
            CouponRecordQuery query = new CouponRecordQuery();
            query.UserId = CurrentUser.Id;
            query.PageNo = pageIndex;
            query.PageSize = pageSize;
            if (obtainWay == 1) //（1=未使用 2=已使用 3=已过期）
            {
                query.Status = 0;
            }
            else if (obtainWay == 2)
            {
                query.Status = 1;
            }
            else
            {
                query.Status = obtainWay;
            }
            var record = CouponApplication.GetCouponRecordList(query);
            var list = record.Models.Select(
               item => new
               {
                   CouponId = item.Himall_Coupon.Id,
                   CouponName = item.Himall_Coupon.CouponName,
                   Price = Math.Round(item.Himall_Coupon.Price, 2),
                   SendCount = item.Himall_Coupon.Num,
                   UserLimitCount = item.Himall_Coupon.PerMax,
                   OrderUseLimit = Math.Round(item.Himall_Coupon.OrderAmount, 2),
                   StartTime = item.Himall_Coupon.StartTime.ToString("yyyy.MM.dd"),
                   ClosingTime = item.Himall_Coupon.EndTime.ToString("yyyy.MM.dd"),
                   ObtainWay = item.Himall_Coupon.ReceiveType,
                   NeedPoint = item.Himall_Coupon.NeedIntegral,
                   UseArea = item.Himall_Coupon.UseArea,
                   Remark = item.Himall_Coupon.Remark
               });
            return JsonResult<dynamic>(new
            {
                total = record.Total,
                Data = list
            });
        }
        /// <summary>
        /// 获取优惠券信息
        /// </summary>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetCouponDetail(string openId, int couponId = 0)
        {
            //登录
            CheckUserLogin();
            if (couponId <= 0)
            {
                return Json(ErrorResult<dynamic>("参数错误"));
            }
            CouponInfo coupon = CouponApplication.GetCouponInfo(couponId);
            if (coupon == null)
            {
                return Json(ErrorResult<dynamic>("错误的优惠券编号"));
            }
            else
            {
                return JsonResult<dynamic>(new
                {
                    CouponId = coupon.Id,
                    CouponName = coupon.CouponName,
                    Price = coupon.Price,
                    SendCount = coupon.Num,
                    UserLimitCount = coupon.PerMax,
                    OrderUseLimit = Math.Round(coupon.OrderAmount, 2),
                    StartTime = coupon.StartTime.ToString("yyyy.MM.dd"),
                    ClosingTime = coupon.EndTime.ToString("yyyy.MM.dd"),
                    CanUseProducts = "",
                    ObtainWay = coupon.ReceiveType,
                    NeedPoint = coupon.NeedIntegral,
                    UseWithGroup = false,
                    UseWithPanicBuying = false,
                    UseWithFireGroup = false,
                    UseArea = coupon.UseArea,
                    Remark = coupon.Remark
                });
            }
        }
    }
}
