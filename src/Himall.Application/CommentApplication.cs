using System.Collections.Generic;
using System.Linq;
using Himall.IServices;
using Himall.Model;
using Himall.DTO;
using Himall.Core;
using Himall.CommonModel;
using Himall.DTO.QueryModel;

namespace Himall.Application
{
    public class CommentApplication
    {
        private static ICommentService _commentService = ObjectContainer.Current.Resolve<ICommentService>();


        /// <summary>
        /// 获取用户订单中商品的评价列表
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<ProductEvaluation> GetProductEvaluationByOrderId(long orderId, long userId)
        {
            return _commentService.GetProductEvaluationByOrderId(orderId, userId);
        }

        public static List<ProductEvaluation> GetProductEvaluationByOrderIdNew(long orderId, long userId)
        {
            return _commentService.GetProductEvaluationByOrderIdNew(orderId, userId);
        }
        /// <summary>
        /// 根据评论ID取评论
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ProductCommentInfo GetComment(long id)
        {
            return _commentService.GetComment(id);
        }
        public static List<ProductCommentInfo> GetCommentsByIds(IEnumerable<long> ids)
        {
            return _commentService.GetCommentsByIds(ids).ToList();
        }
        public static void Add(DTO.ProductComment comment)
        {
            var info = comment.Map<ProductCommentInfo>();
            _commentService.AddComment(info);
            comment.Id = info.Id;
        }
        public static void AddComment(Model.ProductCommentInfo model)
        {
            _commentService.AddComment(model);
        }

        public static void Add(IEnumerable<DTO.ProductComment> comments)
        {
            var list = comments.ToList().Map<List<ProductCommentInfo>>();
            _commentService.AddComment(list);
        }

        /// <summary>
        /// 追加评论
        /// </summary>
        public static void Append(List<AppendCommentModel> models)
        {
            _commentService.AppendComment(models);
        }

        /// <summary>
        /// 获取商品评价列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ProductComment> GetProductComments(ProductCommentQuery query)
        {
            var datalist = _commentService.GetProductComments(query);
            QueryPageModel<ProductComment> result = new QueryPageModel<ProductComment>();
            result.Total = datalist.Total;
            result.Models = AutoMapper.Mapper.Map<List<ProductCommentInfo>, List<ProductComment>>(datalist.Models);
            return result;
        }
        /// <summary>
        /// 获取商品评价数量聚合
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static ProductCommentCountAggregateModel GetProductCommentCountAggregate(long? productId = null, long? shopId = null, long? shopBranchId = null)
        {
            return _commentService.GetProductCommentCountAggregate(productId, shopId, shopBranchId);
        }
        /// <summary>
        /// 获取商品评价好评数
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="shopId"></param>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public static int GetProductHighCommentCount(long? productId = null, long? shopId = null, long? shopBranchId = null)
        {
            return _commentService.GetProductHighCommentCount(productId, shopId, shopBranchId);
        }
        /// <summary>
        /// 订单列表项判断有没有追加评论
        /// </summary>
        /// <param name="subOrderId"></param>
        /// <returns></returns>
        public static bool HasAppendComment(long subOrderId)
        {
            return _commentService.HasAppendComment(subOrderId);
        }
    }
}
