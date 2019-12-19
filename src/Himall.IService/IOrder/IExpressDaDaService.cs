using Himall.Core;
using Himall.DTO.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Himall.CommonModel;
using Himall.CommonModel.Delegates;
using Himall.DTO;

namespace Himall.IServices
{
    /// <summary>
    /// 订单服务接口
    /// </summary>
    public interface IExpressDaDaService : IService
    {
        /// <summary>
        /// 取消发单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="cancel_reason"></param>
        void SetOrderCancel(long orderId, string cancel_reason);
        /// <summary>
        /// 己发货
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="client_id"></param>
        void SetOrderWaitTake(long orderId, string client_id);
        /// <summary>
        /// 设置订单达达状态
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="status"></param>
        /// <param name="client_id">运单号</param>
        void SetOrderDadaStatus(long orderId, int status,string client_id);
    }
}
