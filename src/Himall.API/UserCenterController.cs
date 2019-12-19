using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Helper;
using Himall.IServices;
using Himall.DTO.QueryModel;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Himall.API.Model.ParamsModel;
using Himall.Application;
using Himall.API.Model;

namespace Himall.API
{
    public class UserCenterController : BaseApiController
    {
        private static string _encryptKey = Guid.NewGuid().ToString("N");

        public object GetUser(string UserKey)
        {
            CheckUserLogin();
            dynamic data = SuccessResult();
            long id = CurrentUser.Id;
            var member = ServiceProvider.Instance<IMemberService>.Create.GetMember(id);
            var orders = ServiceProvider.Instance<IOrderService>.Create.GetTopOrders(int.MaxValue, id);
            var fightGroupOrderCount = ServiceProvider.Instance<IOrderService>.Create.GetFightGroupOrderByUser(id);

            //待评价
            var queryModel = new OrderQuery()
            {
                Status = OrderInfo.OrderOperateStatus.Finish,
                UserId = id,
                PageSize = int.MaxValue,
                PageNo = 1,
                Commented = false
            };
            data.UserName = member.UserName;
            data.UserId = member.Id.ToString();
            //d.Photo = String.IsNullOrEmpty(member.Photo)? "" : "http://" + Url.Request.RequestUri.Host + member.Photo;
            data.Photo = String.IsNullOrEmpty(member.Photo) ? "" : Core.HimallIO.GetRomoteImagePath(member.Photo);
            data.AllOrders = orders.Count().ToString();
            data.WaitingForPay = orders.Count(item => item.OrderStatus == OrderInfo.OrderOperateStatus.WaitPay).ToString();
            data.WaitingForRecieve = orders.Count(item => item.UserId == id && item.OrderStatus == OrderInfo.OrderOperateStatus.WaitReceiving || item.OrderStatus == OrderInfo.OrderOperateStatus.WaitSelfPickUp).ToString();
            data.WaitingForDelivery = orders.Count(item => item.OrderStatus == OrderInfo.OrderOperateStatus.WaitDelivery) - fightGroupOrderCount;//获取待发货订单数
            data.WaitingForComments = ServiceProvider.Instance<IOrderService>.Create.GetOrders<OrderInfo>(queryModel).Total.ToString();

            RefundQuery query = new RefundQuery()
            {
                UserId = CurrentUser.Id,
                PageNo = 1,
                PageSize = int.MaxValue
            };


            var refundPage = ServiceProvider.Instance<IRefundService>.Create.GetOrderRefunds(query);
            data.RefundOrders = refundPage.Models.Where(e => e.ManagerConfirmStatus == OrderRefundInfo.OrderRefundConfirmStatus.UnConfirm).Count();
            data.CellPhone = member.CellPhone;
            data.FavoriteShop = ServiceProvider.Instance<IVShopService>.Create.GetUserConcernVShops(CurrentUser.Id, 1, int.MaxValue).Count();
            data.FavoriteProduct = member.FavoriteInfo.Count().ToString();
            //data.Counpon = ServiceProvider.Instance<ICouponService>.Create.GetUserCouponList(member.Id).Where(item => item.UseStatus == CouponRecordInfo.CounponStatuses.Unuse && item.EndTime > DateTime.Now).Count().ToString();
            var model = ServiceProvider.Instance<IMemberService>.Create.GetUserCenterModel(member.Id);
            data.Coupon = model.UserCoupon;
            var memberIntegral = ServiceProvider.Instance<IMemberIntegralService>.Create.GetMemberIntegral(member.Id);
            data.Integral = memberIntegral == null ? "0" : memberIntegral.AvailableIntegrals.ToString();
            var capital = MemberCapitalApplication.GetCapitalInfo(CurrentUserId);
            data.Balance = capital != null ? capital.Balance : 0m;

            //用户参与的团数量
            List<FightGroupOrderJoinStatus> seastatus = new List<FightGroupOrderJoinStatus>();
            seastatus.Add(FightGroupOrderJoinStatus.Ongoing);
            seastatus.Add(FightGroupOrderJoinStatus.JoinSuccess);
            seastatus.Add(FightGroupOrderJoinStatus.BuildFailed);
            seastatus.Add(FightGroupOrderJoinStatus.BuildSuccess);
            data.GroupTotal = ServiceProvider.Instance<IFightGroupService>.Create.GetJoinGroups(CurrentUser.Id, seastatus, 1, 10).Total;

            data.MyGroup = ServiceProvider.Instance<IFightGroupService>.Create.CountJoiningOrder(CurrentUser.Id);
            return data;
        }

        public object GetUserCollectionProduct(int pageNo, int pageSize = 16)
        {
            CheckUserLogin();
            if (CurrentUser != null)
            {
                var model = ServiceProvider.Instance<IProductService>.Create.GetUserConcernProducts(CurrentUser.Id, pageNo, pageSize);
                var result = model.Models.ToArray().Select(item => new
                {
                    Id = item.ProductId,
                    Image = Core.HimallIO.GetRomoteProductSizeImage(item.ProductInfo.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_220),
                    ProductName = item.ProductInfo.ProductName,
                    SalePrice = item.ProductInfo.MinSalePrice.ToString("F2"),
                    Evaluation = item.ProductInfo.Himall_ProductComments.Count()
                });
                return new { success = true, data = result, total = model.Total };
            }
            else
            {
                return new Result { success = false, msg = "未登录" };
            }
        }

        public object GetUserCollectionShop(int pageNo, int pageSize = 8)
        {
            CheckUserLogin();
            if (CurrentUser != null)
            {
                var model = ServiceProvider.Instance<IVShopService>.Create.GetUserConcernVShops(CurrentUser.Id, pageNo, pageSize);
                var result = model.ToArray().Select(item => new
                {
                    Id = item.Id,
                    Logo = Core.HimallIO.GetRomoteImagePath(item.Logo),
                    Name = item.Name
                });
                return new { success = true, data = result };
            }
            else
            {
                return new Result { success = false, msg = "未登录" };
            }
        }

        protected override bool CheckContact(string contact, out string errorMessage)
        {
            CheckUserLogin();

            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// 获取用户积分明细
        /// </summary>
        /// <param name="id">用户编号</param>
        /// <param name="type"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageNo"></param>
        /// <returns></returns>
        public object GetIntegralRecord(MemberIntegral.IntegralType? type = null, int page = 1, int pagesize = 10)
        {
            CheckUserLogin();
            var id = CurrentUser.Id;
            //处理当前用户与id的判断
            var _iMemberIntegralService = ServiceApplication.Create<IMemberIntegralService>();
            Himall.Model.MemberIntegral.IntegralType? integralType = type;
            var query = new IntegralRecordQuery() { IntegralType = integralType, UserId = id, PageNo = page, PageSize = pagesize };
            var result = _iMemberIntegralService.GetIntegralRecordListForWeb(query);
            var list = result.Models.ToList().Select(item => new UserCenterGetIntegralRecordModel
            {
                Id = item.Id,
                RecordDate = item.RecordDate,
                Integral = item.Integral,
                TypeId = item.TypeId,
                ShowType = (item.TypeId == MemberIntegral.IntegralType.WeiActivity) ? item.ReMark : item.TypeId.ToDescription(),
                ReMark = GetRemarkFromIntegralType(item.TypeId, item.Himall_MemberIntegralRecordAction, item.ReMark)
            });
            dynamic pageresult = SuccessResult();
            pageresult.total = result.Total;
            pageresult.data = list.ToList();
            return pageresult;
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
                    remark = "使用订单号(" + orderIds.TrimEnd(',') + ")";
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

        protected override object OnCheckCheckCodeSuccess(string contact)
        {
            CheckUserLogin();

            string pluginId = PluginsManagement.GetInstalledPluginInfos(Core.Plugins.PluginType.SMS).First().PluginId;

            var _iMemberIntegralConversionFactoryService = ServiceProvider.Instance<IMemberIntegralConversionFactoryService>.Create;
            var _iMemberIntegralService = ServiceProvider.Instance<IMemberIntegralService>.Create;
            var _iMemberInviteService = ServiceProvider.Instance<IMemberInviteService>.Create;

            var member = CurrentUser;
            if (Application.MessageApplication.GetMemberContactsInfo(pluginId, contact, MemberContactsInfo.UserTypes.General) != null)
            {
                return new { success = false, msg = contact + "已经绑定过了！" };
            }
            member.CellPhone = contact;
            MemberApplication.UpdateMember(member.Map<DTO.Members>());
            Application.MessageApplication.UpdateMemberContacts(new MemberContactsInfo() { Contact = contact, ServiceProvider = pluginId, UserId = CurrentUser.Id, UserType = MemberContactsInfo.UserTypes.General });
            Core.Cache.Remove(CacheKeyCollection.MemberPluginCheck(CurrentUser.UserName, pluginId));
            Core.Cache.Remove(CacheKeyCollection.Member(CurrentUser.Id));//移除用户缓存
            Core.Cache.Remove("Rebind" + CurrentUser.Id);

            UserMemberInfo inviteMember = null;
            if (member.InviteUserId.HasValue)
            {
                inviteMember = Application.MemberApplication.GetMember(member.InviteUserId.Value);
            }

            MemberIntegralRecord info = new MemberIntegralRecord();
            info.UserName = member.UserName;
            info.MemberId = member.Id;
            info.RecordDate = DateTime.Now;
            info.TypeId = MemberIntegral.IntegralType.Reg;
            info.ReMark = "绑定手机";
            var memberIntegral = _iMemberIntegralConversionFactoryService.Create(MemberIntegral.IntegralType.Reg);
            _iMemberIntegralService.AddMemberIntegral(info, memberIntegral);
            if (inviteMember != null)
                _iMemberInviteService.AddInviteIntegel(member, inviteMember);

            return base.OnCheckCheckCodeSuccess(contact);
        }

        protected override object ChangePasswordByOldPassword(string oldPassword, string password)
        {
            CheckUserLogin();

            var _iMemberService = ServiceProvider.Instance<IMemberService>.Create;

            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(password))
            {
                return new { success = false, msg = "密码不能为空！" };
            }
            var model = CurrentUser;
            var pwd = SecureHelper.MD5(SecureHelper.MD5(oldPassword) + model.PasswordSalt);
            bool CanChange = false;
            if (pwd == model.Password)
            {
                CanChange = true;
            }
            if (model.PasswordSalt.StartsWith("o"))
            {
                CanChange = true;
            }
            if (CanChange)
            {
                Application.MemberApplication.ChangePassword(model.Id, password);
                return SuccessResult("密码修改成功");
            }
            else
                return ErrorResult("旧密码错误");
        }

        protected override object ChangePayPwdByOldPassword(string oldPassword, string password)
        {
            CheckUserLogin();

            var _iMemberCapitalService = ServiceProvider.Instance<IMemberCapitalService>.Create;

            var hasPayPwd = MemberApplication.HasPayPwd(CurrentUser.Id);

            if (hasPayPwd && string.IsNullOrEmpty(oldPassword))
                return Json(new { success = false, msg = "请输入旧支付密码" });

            if (string.IsNullOrWhiteSpace(password))
                return Json(new { success = false, msg = "请输入新支付密码" });

            if (hasPayPwd)
            {
                var success = MemberApplication.VerificationPayPwd(CurrentUser.Id, oldPassword);
                if (!success)
                    return Json(new { success = false, msg = "旧支付密码错误" });
            }

            _iMemberCapitalService.SetPayPwd(CurrentUser.Id, password);

            return Json(new { success = true, msg = "设置成功" });
        }

        public object GetUserContact()
        {
            CheckUserLogin();
            string cellPhone = CurrentUser.CellPhone;
            string email = CurrentUser.Email;
            if (string.IsNullOrEmpty(cellPhone) && string.IsNullOrEmpty(email))
                return Json(new { success = false, msg = "没有绑定手机或邮箱" });

            string Contact = string.IsNullOrEmpty(CurrentUser.CellPhone) ? CurrentUser.Email : CurrentUser.CellPhone;

            return Json(new { success = true, msg = "", contact = Contact });
        }
    }
}
