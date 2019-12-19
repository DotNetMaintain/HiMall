using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    public class Orderrefunddetail
    {
        public long RefundId { get; set; }//售后编号

        public long OrderId { get; set; }//订单编号

        public string ApplyDate { get; set; }//申请日期

        public string UserName { get; set; }//用户名称

        public string ContactPerson { get; set; }//联系人

        public string ContactCellPhone { get; set; }//联系方式

        public string Amount { get; set; }//退款金额

        public string ProductName { get; set; }//商品名称

        public string Reason { get; set; }//退款原因

        public string ReasonDetail { set; get; }//退款详情

        public string ShipOrderNumber { get; set; }//快递单号

        public long ProductId { get; set; }//商品编号

        public int SellerAuditStatus { get; set; }//商家审核状态

        public int? RefundMode { get; set; }//退款方式（为空，基本原路返回）
        
        public int? RefundPayStatus { get; set; }//退款支付状态
    }
}
