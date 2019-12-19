using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.DTO.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace Himall.SmallProgAPI
{
    public class O2OUserCenterController : BaseO2OApiController
    {
        /// <summary>
        /// 个人中心主页
        /// </summary>
        /// <returns></returns>
        public new JsonResult<Result<dynamic>> GetUser()
        {
            CheckUserLogin();
            dynamic d = new System.Dynamic.ExpandoObject();
            long id = CurrentUser.Id;
            var member = MemberApplication.GetMember(id);
            var orders = OrderApplication.GetTopOrders(int.MaxValue, id);
            var fightGroupOrderCount = OrderApplication.GetFightGroupOrderByUser(id);

            var queryModel = new OrderQuery() //待评价
            {
                Status = OrderInfo.OrderOperateStatus.Finish,
                UserId = id,
                Commented = false
            };
            d.UserName = member.UserName;//用户名
            d.RealName = member.RealName;//真实姓名
            d.Nick = member.Nick;//昵称 
            d.UserId = member.Id.ToString();
            d.Photo = String.IsNullOrEmpty(member.Photo) ? "" : Core.HimallIO.GetRomoteImagePath(member.Photo);//头像
            d.AllOrders = orders.Count().ToString();//订单总数

            d.WaitingForPay = orders.Count(item => item.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay).ToString();//待付款订单数
            d.WaitingForRecieve = orders.Count(item => item.UserId == id && item.OrderStatus == OrderInfo.OrderOperateStatus.WaitReceiving || item.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp).ToString();//待发货订单数
            d.WaitingForDelivery = orders.Count(item => item.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery) - fightGroupOrderCount;//获取待发货订单数
            d.WaitingForComments = OrderApplication.GetWaitingForComments(queryModel).ToString();

            RefundQuery query = new RefundQuery()
            {
                UserId = CurrentUser.Id,
                PageNo = 1,
                PageSize = int.MaxValue
            };
            var refundPage = RefundApplication.GetOrderRefunds(query);
            d.RefundOrders = refundPage.Models.Where(e => e.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm).Count();
            d.CellPhone = member.CellPhone;//绑定的手机号码
            var model = MemberApplication.GetUserCenterModel(member.Id);
            d.Counpon = model.UserCoupon;
            var memberIntegral = MemberIntegralApplication.GetMemberIntegral(member.Id);
            d.Integral = memberIntegral == null ? "0" : memberIntegral.AvailableIntegrals.ToString();//我的积分
            var capital = MemberCapitalApplication.GetCapitalInfo(CurrentUserId);
            d.Balance = capital != null ? capital.Balance : 0m;//我的资产

            //用户参与的团数量
            //List<FightGroupOrderJoinStatus> seastatus = new List<FightGroupOrderJoinStatus>();
            ////seastatus.Add(FightGroupOrderJoinStatus.Ongoing);
            //seastatus.Add(FightGroupOrderJoinStatus.JoinSuccess);
            //seastatus.Add(FightGroupOrderJoinStatus.BuildFailed);
            //seastatus.Add(FightGroupOrderJoinStatus.BuildSuccess);
            //d.GroupTotal = FightGroupApplication.GetJoinGroups(CurrentUser.Id, seastatus, 1, 10).Total;
            //d.MyGroup = FightGroupApplication.CountJoiningOrder(CurrentUser.Id);//我的拼团

            return JsonResult<dynamic>(d);
        }

        public JsonResult<Result<dynamic>> GetIntegralRecordList(string openId, int pageNo = 1, int pageSize = 10)
        {
            CheckUserLogin();
            IntegralRecordQuery query = new IntegralRecordQuery();
            query.UserId = CurrentUserId;
            query.PageNo = pageNo;
            query.PageSize = pageSize;
            var list = MemberIntegralApplication.GetIntegralRecordList(query);
            if (list.Models != null)
            {
                var recordlist = list.Models.Select(a => new
                {
                    Id = a.Id,
                    MemberId = a.MemberId,
                    UserName = a.UserName,
                    TypeName = (a.TypeId == MemberIntegral.IntegralType.WeiActivity) ? a.ReMark : a.TypeId.ToDescription(),
                    Integral = a.Integral,
                    RecordDate = ((DateTime)a.RecordDate).ToString("yyyy-MM-dd HH:mm:ss"),
                    ReMark = GetRemarkFromIntegralType(a.TypeId, a.Himall_MemberIntegralRecordAction, a.ReMark)
                });
                return JsonResult<dynamic>(recordlist);
            }

            return JsonResult<dynamic>(new int[0]);
        }

        private string GetRemarkFromIntegralType(Himall.Model.MemberIntegral.IntegralType type, ICollection<MemberIntegralRecordAction> recordAction, string remark = "")
        {
            if (recordAction == null || recordAction.Count == 0)
                return remark;
            switch (type)
            {
                //case MemberIntegral.IntegralType.InvitationMemberRegiste:
                //    remark = "邀请用户(用户ID：" + recordAction.FirstOrDefault().VirtualItemId+")";
                //    break;
                case MemberIntegral.IntegralType.Consumption:
                    var orderIds = "";
                    foreach (var item in recordAction)
                    {
                        orderIds += item.VirtualItemId + ",";
                    }
                    remark = "订单号：" + orderIds.TrimEnd(',');
                    break;
                //case MemberIntegral.IntegralType.Comment:
                //    remark = "商品评价（商品ID：" + recordAction.FirstOrDefault().VirtualItemId + ")";
                //    break;
                //case MemberIntegral.IntegralType.ProportionRebate:
                //    remark = "使用订单号(" +recordAction.FirstOrDefault().VirtualItemId + ")";
                //    break;
                default:
                    return remark;
            }
            return remark;
        }
    }
}
