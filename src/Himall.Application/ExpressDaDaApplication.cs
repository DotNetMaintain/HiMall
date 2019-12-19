using Himall.CommonModel;
using Himall.Core;
using Himall.IServices;
using Himall.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Himall.Application
{
    /// <summary>
    /// 达达物流
    /// </summary>
    public class ExpressDaDaApplication
    {
        #region 字段
        private static IMemberService _iMemberService = ObjectContainer.Current.Resolve<IMemberService>();
        private static IExpressDaDaService _iExpressDaDaService = ObjectContainer.Current.Resolve<IExpressDaDaService>();
        #endregion
        /// <summary>
        /// 取消发单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="cancel_reason"></param>
        public static void SetOrderCancel(long orderId, string cancel_reason)
        {
            _iExpressDaDaService.SetOrderCancel(orderId, cancel_reason);
        }
        /// <summary>
        /// 己发货
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="client_id"></param>
        public static void SetOrderWaitTake(long orderId, string client_id)
        {
            _iExpressDaDaService.SetOrderWaitTake(orderId, client_id);
        }
        /// <summary>
        /// 己完成
        /// </summary>
        /// <param name="orderId"></param>
        public static void SetOrderFinished(long orderId)
        {
            var order = OrderApplication.GetOrder(orderId);
            var member = MemberApplication.GetMember(order.UserId);
            OrderApplication.MembeConfirmOrder(orderId, member.UserName);
        }
        /// <summary>
        /// 设置订单达达状态
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="status"></param>
        /// <param name="client_id">运单号</param>
        public static void SetOrderDadaStatus(long orderId,int status,string client_id)
        {
            _iExpressDaDaService.SetOrderDadaStatus(orderId, status, client_id);
        }
    }
}
