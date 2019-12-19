using Himall.CommonModel;
using Himall.CommonModel.Delegates;
using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.Core.Plugins.Payment;
using Himall.DTO;
using Himall.Entity;
using Himall.IServices;
using Himall.DTO.QueryModel;
using Himall.Model;
using Himall.ServiceProvider;
using MySql.Data.MySqlClient;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Himall.Service
{
    public class ExpressDaDa : ServiceBase, IExpressDaDaService
    {

        /// <summary>
        /// 取消发单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="cancel_reason"></param>
        public void SetOrderCancel(long orderId, string cancel_reason)
        {
            var order = Context.OrderInfo.FirstOrDefault(d => d.Id == orderId);
            if (order != null)
            {
                order.DadaStatus = DadaStatus.Cancel.GetHashCode();
                order.OrderStatus = OrderInfo.OrderOperateStatus.WaitDelivery;
            }
            Context.SaveChanges();
        }
        /// <summary>
        /// 己发货
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="client_id"></param>
        public void SetOrderWaitTake(long orderId, string client_id)
        {
            var order = Context.OrderInfo.FirstOrDefault(p => p.Id == orderId);
            order.DeliveryType = CommonModel.Enum.DeliveryType.CityExpress;
            order.ExpressCompanyName = "同城合作物流";
            order.ShipOrderNumber = client_id;
            order.ShippingDate = DateTime.Now;
            order.LastModifyTime = DateTime.Now;
            order.DadaStatus = DadaStatus.WaitTake.GetHashCode();

            Context.SaveChanges();
        }
        /// <summary>
        /// 设置订单达达状态
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="status"></param>
        /// <param name="client_id">运单号</param>
        public void SetOrderDadaStatus(long orderId, int status, string client_id)
        {
            var order = Context.OrderInfo.FirstOrDefault(p => p.Id == orderId);
            order.DadaStatus = status;
            order.ShipOrderNumber = client_id;
            if (status == (int)DadaStatus.Cancel)
            {
                order.DeliveryType = CommonModel.Enum.DeliveryType.Express;
                order.ShipOrderNumber = "";
                order.ExpressCompanyName = "";
            }
            Context.SaveChanges();

        }
    }
}