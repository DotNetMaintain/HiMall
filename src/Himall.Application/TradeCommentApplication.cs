using Himall.IServices;
using Himall.Core;
using Himall.Model;
using Himall.CommonModel;
using Himall.DTO.QueryModel;

namespace Himall.Application
{
    public class TradeCommentApplication
    {
        private static ITradeCommentService _tradeCommentService = ObjectContainer.Current.Resolve<ITradeCommentService>();

        /// <summary>
        /// 根据用户ID和订单ID获取单个订单评价信息
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public static DTO.OrderComment GetOrderComment(long orderId, long userId)
        {
            return _tradeCommentService.GetOrderCommentInfo(orderId, userId).Map<DTO.OrderComment>();
        }

        /// <summary>
        /// 查询订单评价
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static QueryPageModel<OrderCommentInfo> GetOrderComments(long shopId)
        {
            return _tradeCommentService.GetOrderComments(new OrderCommentQuery
            {
                ShopId = shopId,
                PageNo = 1,
                PageSize = 100000
            });
        }

        public static void Add(DTO.OrderComment model)
        {
            var info = model.Map<Model.OrderCommentInfo>();
            _tradeCommentService.AddOrderComment(info);
            model.Id = info.Id;
        }
        public static OrderCommentInfo GetOrderCommentInfo(long orderId, long userId)
        {
            return _tradeCommentService.GetOrderCommentInfo(orderId, userId);
        }
    }
}
