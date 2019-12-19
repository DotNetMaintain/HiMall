namespace Himall.DTO
{
    public class ProformanceModel
    {
        public string UserName { set; get; }

        /// <summary>
        /// 未结佣金(含退款)
        /// </summary>
        public decimal UnPaid { set; get; }

        /// <summary>
        /// 已结佣金(含退款)
        /// </summary>
        public decimal Paid { set; get; }
        /// <summary>
        /// 己结退款佣金
        /// </summary>
        public decimal PaidRefund { get; set; }
        /// <summary>
        /// 未结退款佣金
        /// </summary>
        public decimal UnPaidRefund { get; set; }

        /// <summary>
        /// 成交总金额
        /// </summary>
        public decimal TotalTurnover { set; get; }
        /// <summary>
        /// 退款总金额(含退款)
        /// </summary>
        public decimal TotalTurnoverRefund { set; get; }

        /// <summary>
        /// 成交总数
        /// </summary>
        public decimal TotalNumber { set; get; }

        /// <summary>
        /// ID用户ID
        /// </summary>
        public long Id { set; get; }
    }
}
