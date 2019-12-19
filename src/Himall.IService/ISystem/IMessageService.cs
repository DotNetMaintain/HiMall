using Himall.Model;
using Himall.DTO.QueryModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core.Plugins.Message;
using Himall.CommonModel;

namespace Himall.IServices
{
    public interface IMessageService : IService
    {
        //更新信息=用户表
        void UpdateMemberContacts(MemberContactsInfo info);
        /// <summary>
        /// 获取发送目标
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="pluginId">插件ID</param>
        /// <param name="type">用户类型</param>
        /// <returns></returns>
        string GetDestination(long userId, string pluginId, MemberContactsInfo.UserTypes type);

        /// <summary>
        /// 根据插件类型和ID和目标获取信息
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="contact"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        MemberContactsInfo GetMemberContactsInfo(string pluginId, string contact, MemberContactsInfo.UserTypes type);

        /// <summary>
        /// 根据用户ID获取目标信息
        /// </summary>
        /// <param name="UserId">用户ID</param>
        /// <returns></returns>
        List<MemberContactsInfo> GetMemberContactsInfo(long UserId);

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        string SendMessageCode(string destination, string pluginId, MessageUserInfo info);

        /// <summary>
        /// 找回密码
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        void SendMessageOnFindPassWord(long userId, MessageUserInfo info);
        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        void SendMessageOnOrderCreate(long userId, MessageOrderInfo info);

        /// <summary>
        /// 订单支付
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        void SendMessageOnOrderPay(long userId, MessageOrderInfo info);
        /// <summary>
        /// 订单待支付
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        void SendMessageOnWaitPay(long userId, MessageOrderInfo info);
        /// <summary>
        /// 店铺有新订单
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        void SendMessageOnShopHasNewOrder(long shopId, MessageOrderInfo info);
        /// <summary>
        /// 订单退款
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        void SendMessageOnOrderRefund(long userId, MessageOrderInfo info, long refundid = 0, string refundTypeName = "");
        /// <summary>
        /// 售后发货信息提醒
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        void SendMessageOnRefundDeliver(long userId, MessageOrderInfo info, long refundid = 0);

        /// <summary>
        /// 订单发货
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        void SendMessageOnOrderShipping(long userId, MessageOrderInfo info);
        /// <summary>
        /// 店铺审核
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        void SendMessageOnShopAudited(long userId, MessageShopInfo info);

        /// <summary>
        /// 发送优惠券成功
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        void SendMessageOnCouponSuccess(long userId, MessageCouponInfo info);

        ///// <summary>
        ///// 店铺成功2.4去除
        ///// </summary>
        ///// <param name="destination"></param>
        ///// <param name="info"></param>
        //void SendMessageOnShopSuccess(long userId, MessageShopInfo info);
        /// <summary>
        /// 添加群发消息记录
        /// </summary>
        /// <param name="model"></param>
        void AddSendMessageRecord(dynamic model);
        /// <summary>
        /// 查询群发消息记录
        /// </summary>
        /// <param name="querymodel"></param>
        /// <returns></returns>
        QueryPageModel<object> GetSendMessageRecords(object querymodel);

        /// <summary>
        /// 退款/退货失败通知
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        /// <param name="refundid"></param>
        void SendMessageOnOrderRefundFail(long userId, MessageOrderInfo info, long refundid = 0);
     }
}
