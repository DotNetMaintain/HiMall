using Himall.CommonModel;
using Himall.Core;
using Himall.Model;
using System;
using System.Collections.Generic;

namespace Himall.DTO
{
    public class OrderCreateModel
    {
        public OrderCreateModel()
        {
            PlatformType = PlatformType.PC;
            this.ProductList = new List<ProductInfo>();
            this.SKUList = new List<SKUInfo>();
        }
        public PlatformType PlatformType { set; get; }

        public UserMemberInfo CurrentUser { set; get; }
        /// <summary>
        /// 收货地址坐标
        /// </summary>
        public float ReceiveLatitude { get; set; }
        /// <summary>
        /// 收货地址坐标
        /// </summary>
        public float ReceiveLongitude { get; set; }


        public long ReceiveAddressId { get; set; }

        public IEnumerable<string[]> CouponIdsStr { set; get; } 

        /// <summary>
        /// 组合购的商品编号
        /// </summary>
        public IEnumerable<long> CollPids { set; get; }

        public int Integral { set; get; }
        /// <summary>
        /// 预付款支付金额
        /// </summary>
        public decimal Capital { get; set; }

        public InvoiceType Invoice { set; get; }

        public string InvoiceTitle { set; get; }
        public string InvoiceCode { set; get; }
        /// <summary>
        /// 买家留言
        /// </summary>
        public IEnumerable<string> OrderRemarks { get; set; }
        public string InvoiceContext
        {
            set;
            get; 
        }
        public long[] CartItemIds { set; get; }
        public IEnumerable<string> SkuIds { set; get; }
        public IEnumerable<int> Counts { set; get; }

        /// <summary>
        /// 是否货到付款
        /// </summary>
        public bool IsCashOnDelivery { get; set; }

        public bool IslimitBuy { set; get; }

        public List<ProductInfo> ProductList { set; get; }
        public List<SKUInfo> SKUList { set; get; }

        /// <summary>
        /// 拼团活动
        /// </summary>
        public long ActiveId { get; set; }
        /// <summary>
        /// 拼团活动
        /// </summary>
        public long GroupId { get; set; }

		public CommonModel.OrderShop[] OrderShops { get; set; }

        public string formId { get; set; }
        public string personname { get; set; }
        public string personpn { get; set; }
        public string personid { get; set; }
        public string Insurery { get; set; }
        public string Insurero { get; set; }
        public string Supplier { get; set; }
        /// <summary>
        /// 是否为门店订单
        /// </summary>
        public bool IsShopbranchOrder { get; set; }
        /// <summary>
        /// 使用预付款金额
        /// </summary>
        //public decimal CapitalAmount { get; set; }
        /// <summary>
        /// 分销关系linkid
        /// </summary>
        public List<long> DistributionUserLinkId { get; set; }
    }

    /// <summary>
    /// 订单的额外对象，其中有创建日期、收货地址、使用的优惠券
    /// </summary>
    public class OrderCreateAdditional
    {    
        public DateTime CreateDate { set; get; }
        public ShippingAddressInfo Address { set; get; }
        public IEnumerable<CouponRecordInfo> Coupons { set; get; }

        public IEnumerable<BaseAdditionalCoupon> BaseCoupons { set; get; }
        //public decimal OrdersTotal { set; get; } 
        public decimal IntegralTotal { set; get; }
        /// <summary>
        /// 预付款金额
        /// </summary>
        public decimal CapitalTotal { get; set; }

        /// <summary>
        /// 分销是否开启了
        /// </summary>
        public bool IsEnableDistribution { set; get; }
    }

    public class BaseAdditionalCoupon
    {
        public object Coupon{get;set;}

        public int Type { get; set; }

        public long ShopId { get; set; }
    }
}
