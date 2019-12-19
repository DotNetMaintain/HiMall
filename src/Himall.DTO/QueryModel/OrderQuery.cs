using Himall.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Himall.DTO.QueryModel
{
    public partial class OrderQuery : QueryBase
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public OrderInfo.OrderOperateStatus? Status { get; set; }

        public string OrderId { get; set; }

        public long? ShopId { get; set; }

        public long? ShopBranchId { get; set; }

        public string ShopName { get; set; }

        public long? UserId { get; set; }

        public string UserName { get; set; }

        /// <summary>
        /// 用户联系方式
        /// </summary>
        public string UserContact { get; set; }

        public string PaymentTypeName { get; set; }

        public string PaymentTypeGateway { get; set; }

        public string SearchKeyWords { set; get; }

        public bool? Commented { get; set; }

        public string ShopBranchName { get; set; }

        /// <summary>
        /// 多个状态搜索
        /// <para>Status的补充条件，请与Status配合合用</para>
        /// </summary>
        public List<OrderInfo.OrderOperateStatus> MoreStatus { get; set; }

        public int? OrderType { get; set; }


        public OrderInfo.PaymentTypes PaymentType { get; set; }

        /// <summary>
        /// 是否是购买记录（只查询已付款且没关闭的订单）
        /// </summary>
        public bool IsBuyRecord { set; get; }
        /// <summary>
        /// 门店配送
        /// </summary>
        public int? AllotStore { get; set; }

        public Operator Operator { get; set; }
    }

    /// <summary>
    /// 订单操作
    /// </summary>
    public enum Operator
    {
        /// <summary>
        /// 前端 0
        /// </summary>
        [Description("前端")]
        None = 0,
        /// <summary>
        /// 平台 1
        /// </summary>
        [Description("平台")]
        Admin = 1,

        /// <summary>
        /// 商家 2
        /// </summary>
        [Description("商家")]
        Seller = 2
    }
}
