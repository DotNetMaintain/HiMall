using Himall.Model;
using System;
using System.Collections.Generic;

namespace Himall.DTO.QueryModel
{
    public partial class DistributionUserBillQuery : QueryBase
    {
        public DistributionUserBillQuery()
        {
            UserIds = new List<long>();
        }

        public long? UserId { get; set; }
        public IEnumerable<long> UserIds { get; set; }
        public string UserName { get; set; }
        public long? ShopId { get; set; }
        public long? OrderId { get; set; }
        public OrderInfo.OrderOperateStatus? OrderState { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public IEnumerable<BrokerageIncomeInfo.BrokerageStatus> SettleState { get; set; }
    }
}
