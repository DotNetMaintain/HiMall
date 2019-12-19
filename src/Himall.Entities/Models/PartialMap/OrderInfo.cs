using PetaPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;

namespace Himall.Entities
{
    public partial class OrderInfo
    {
        public OrderInfo()
        {
            OrderItems = new List<OrderItemInfo>();
            
        }

        [ResultColumn]
        public long PayId { get; set; }

        [ResultColumn]
        public OrderPayInfo OrderPay { get; set; }

        [ResultColumn]
        public List<OrderItemInfo> OrderItems { get; set; }

        /// <summary>
        /// 订单可退金额
        /// </summary>
        [ResultColumn]
        public decimal OrderEnabledRefundAmount
        {
            get
            {
                decimal result = 0;
                switch (this.OrderStatus)
                {
                    case OrderOperateStatus.Finish:
                    case OrderOperateStatus.WaitReceiving:
                        result = this.ProductTotalAmount - this.FullDiscount - this.DiscountAmount - this.RefundTotalAmount;   //商品总价 - 优惠券 - 已退金额
                        break;
                    case OrderOperateStatus.WaitDelivery:
                        result = this.ProductTotalAmount + this.Freight - this.DiscountAmount - this.FullDiscount;            //待发货退还运费
                        break;
                    case OrderOperateStatus.WaitSelfPickUp:
                        result = this.ProductTotalAmount - this.DiscountAmount - this.FullDiscount - this.RefundTotalAmount;
                        break;
                }
                return result;
            }
        }
        /// <summary>
        /// 是否可以原路退回
        /// </summary>
        /// <returns></returns>
        public bool CanBackOut()
        {
            bool result = false;
            if (!string.IsNullOrWhiteSpace(this.PaymentTypeGateway))
            {
                if (CapitalAmount <= 0 && (this.PaymentTypeGateway.ToLower().Contains("weixin") || this.PaymentTypeGateway.ToLower().Contains("alipay")))
                {
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// 商品实付（商品应付-优惠券的价格-满额减的价格）
        /// </summary>
        public decimal ProductTotal { get { return this.ProductTotalAmount - this.DiscountAmount - this.FullDiscount; } }


        /// <summary>
        /// 订单实付金额
        /// 公式： 商品应付+运费+税 - 优惠券金额 - 积分抵扣金额-满额减金额
        /// </summary>
        public decimal OrderTotalAmount { get { return this.ProductTotalAmount + this.Freight + this.Tax - this.IntegralDiscount - this.DiscountAmount - this.FullDiscount; } }

        /// <summary>
        /// 订单金额 （商品应付+运费+税 -优惠券金额-满额减金额） 不包含积分抵扣部分
        /// </summary>

        public decimal OrderAmount { get { return this.ProductTotalAmount + this.Freight + this.Tax - this.DiscountAmount - this.FullDiscount; } }

        public enum OrderTypes
        {
            [Description("正常购")]
            Normal = 0,
            /// </summary>
            [Description("组合购")]
            Collocation = 1,
            [Description("限时购")]
            LimitBuy = 2,
            [Description("拼团")]
            FightGroup = 3,
        }
        public enum PaymentTypes
        {
            /// <summary>
            /// 未付款时的默认状态
            /// </summary>

            [Description("")]
            None = 0,

            [Description("线上支付")]
            Online = 1,

            /// <summary>
            /// 平台确认收款之类的
            /// </summary>
            [Description("线下支付")]
            Offline = 2,

            /// <summary>
            /// 货到付款
            /// </summary>
            [Description("货到付款")]
            CashOnDelivery = 3,
        }
        /// <summary>
        /// 订单状态
        /// </summary>
        public enum OrderOperateStatus
        {
            /// <summary>
            /// 待付款 1
            /// </summary>
            [Description("待付款")]
            WaitPay = 1,

            /// <summary>
            /// 待发货 2
            /// </summary>
            [Description("待发货")]
            WaitDelivery = 2,

            /// <summary>
            /// 待收货 3
            /// </summary>
            [Description("待收货")]
            WaitReceiving = 3,

            /// <summary>
            /// 已关闭 4
            /// </summary>
            [Description("已关闭")]
            Close = 4,

            /// <summary>
            /// 已完成 5
            /// </summary>
            [Description("已完成")]
            Finish = 5,

            /// <summary>
            /// 待自提 6
            /// </summary>
            [Description("待自提")]
            WaitSelfPickUp = 6,

            /// <summary>
            /// 未评价 7
            /// </summary>
            [Description("未评价")]
            UnComment = 7
        }

        /// <summary>
        /// 配送方式
        /// </summary>
        public enum DeliveryTypes
        {
            /// <summary>
            /// 快递配送
            /// </summary>
            [Description("快递配送")]
            Express = 0,

            /// <summary>
            /// 到店自提
            /// </summary>
            [Description("到店自提")]
            SelfTake = 1,

            /// <summary>
            /// 店员配送
            /// </summary>
            [Description("店员配送")]
            ShopStore = 2
        }
    }
}
