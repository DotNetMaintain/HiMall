using Himall.Application;
using Himall.DTO;
using Himall.Model;
using Himall.SmallProgAPI.O2O.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web.Http.Results;

namespace Himall.SmallProgAPI
{
    public class O2OCommentController : BaseO2OApiController
    {
        /// <summary>
        /// 根据订单ID获取评价
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetComment(long orderId)
        {
            CheckUserLogin();
            var order = OrderApplication.GetOrderInfo(orderId);
            if (order != null && order.OrderCommentInfo.Count == 0)
            {
                var model = CommentApplication.GetProductEvaluationByOrderId(orderId, CurrentUser.Id).Select(item => new
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    //Image = "http://" + Url.Request.RequestUri.Host + item.ThumbnailsUrl
                    //Image = Core.HimallIO.GetRomoteImagePath(item.ThumbnailsUrl)
                    Image = Core.HimallIO.GetRomoteProductSizeImage(item.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_220) //商城App评论时获取商品图片
                });

                var orderEvaluation = TradeCommentApplication.GetOrderCommentInfo(orderId, CurrentUser.Id);
                return JsonResult<dynamic>(new { Product = model, orderItemIds = order.OrderItemInfo.Select(item => item.Id) });
            }
            else
                return Json(ErrorResult<dynamic>("该订单不存在或者已评论过"));
        }

        //发布评论
        public JsonResult<Result<int>> PostAddComment(CommentAddCommentModel value)
        {
            CheckUserLogin();
            try
            {
                string Jsonstr = value.Jsonstr;
                bool result = false;
                var orderComment = Newtonsoft.Json.JsonConvert.DeserializeObject<OrderCommentModel>(Jsonstr);
                if (orderComment != null)
                {
                    using (TransactionScope scope = new TransactionScope())
                    {
                        AddOrderComment(orderComment);//添加订单评价
                        AddProductsComment(orderComment.OrderId, orderComment.ProductComments);//添加商品评论
                        scope.Complete();
                    }
                    result = true;
                }
                return Json(ApiResult<int>(result));
            }
            catch (Exception ex)
            {
                return Json(ErrorResult<int>(ex.Message));
            }
        }

        void AddOrderComment(OrderCommentModel comment)
        {
            TradeCommentApplication.Add(new DTO.OrderComment()
            {
                OrderId = comment.OrderId,
                DeliveryMark = comment.DeliveryMark,
                ServiceMark = comment.ServiceMark,
                PackMark = comment.PackMark,
                UserId = CurrentUser.Id,
            });
        }

        void AddProductsComment(long orderId, IEnumerable<ProductCommentModel> productComments)
        {
            //var commentService = ServiceProvider.Instance<ICommentService>.Create;
            foreach (var productComment in productComments)
            {
                ProductCommentInfo model = new ProductCommentInfo();
                model.ReviewDate = DateTime.Now;
                model.ReviewContent = productComment.Content;
                model.UserId = CurrentUser.Id;
                model.UserName = CurrentUser.UserName;
                model.Email = CurrentUser.Email;
                model.SubOrderId = productComment.OrderItemId;
                model.ReviewMark = productComment.Mark;
                model.ProductId = productComment.ProductId;
                if (productComment.Images != null && productComment.Images.Length > 0)
                {
                    foreach (var img in productComment.Images)
                    {
                        var p = new ProductCommentsImagesInfo();
                        p.CommentType = 0;//0代表默认的表示评论的图片
                        p.CommentImage = MoveImages(img, CurrentUser.Id);
                        model.Himall_ProductCommentsImages.Add(p);
                    }
                }
                #region APP中 微信图片可以去除掉 
                //else if (productComment.WXmediaId != null && productComment.WXmediaId.Length > 0)
                //{
                //    foreach (var img in productComment.WXmediaId)
                //    {
                //        var p = new ProductCommentsImagesInfo();
                //        p.CommentType = 0;//0代表默认的表示评论的图片
                //        p.CommentImage = DownloadWxImage(img);
                //        if (!string.IsNullOrEmpty(p.CommentImage))
                //        {
                //            model.Himall_ProductCommentsImages.Add(p);
                //        }
                //    }
                //}
                #endregion

                CommentApplication.AddComment(model);
            }
        }

        private string MoveImages(string image, long userId)
        {
            string OriUrl = Core.Helper.IOHelper.GetMapPath(image);
            //  var ext = new System.IO.FileInfo(OriUrl).Extension;
            var oldname = new System.IO.FileInfo(OriUrl).Name;
            string ImageDir = string.Empty;

            //转移图片
            ImageDir = Core.Helper.IOHelper.GetMapPath("/Storage/Plat/Comment");
            string relativeDir = "/Storage/Plat/Comment/";
            string fileName = userId + oldname;
            if (!System.IO.Directory.Exists(ImageDir))
                System.IO.Directory.CreateDirectory(ImageDir);//创建图片目录

            if (image.Replace("\\", "/").Contains("/temp/"))//只有在临时目录中的图片才需要复制
            {
                Core.Helper.IOHelper.CopyFile(OriUrl, ImageDir, false, fileName);
                return relativeDir + fileName;
            }  //目标地址
            else
            {
                return image;
            }
        }
        /// <summary>
        /// 获取追加评论
        /// </summary>
        /// <param name="orderid"></param>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetAppendComment(long orderId)
        {
            CheckUserLogin();
            var model = CommentApplication.GetProductEvaluationByOrderIdNew(orderId, CurrentUser.Id);

            if (model.Count() > 0 && model.FirstOrDefault().AppendTime.HasValue)
                return Json(ErrorResult<dynamic>("追加评论时，获取数据异常", new int[0]));
            else
            {
                var listResult = model.Select(item => new
                {
                    Id = item.Id,
                    CommentId = item.CommentId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    //ThumbnailsUrl = item.ThumbnailsUrl,
                    ThumbnailsUrl = Core.HimallIO.GetRomoteProductSizeImage(item.ThumbnailsUrl, 1, (int)Himall.CommonModel.ImageSize.Size_220), //商城App追加评论时获取商品图片
                    BuyTime = item.BuyTime,
                    EvaluationStatus = item.EvaluationStatus,
                    EvaluationContent = item.EvaluationContent,
                    AppendContent = item.AppendContent,
                    AppendTime = item.AppendTime,
                    EvaluationTime = item.EvaluationTime,
                    ReplyTime = item.ReplyTime,
                    ReplyContent = item.ReplyContent,
                    ReplyAppendTime = item.ReplyAppendTime,
                    ReplyAppendContent = item.ReplyAppendContent,
                    EvaluationRank = item.EvaluationRank,
                    OrderId = item.OrderId,
                    CommentImages = item.CommentImages.Select(r => new
                    {
                        CommentImage = r.CommentImage,
                        CommentId = r.CommentId,
                        CommentType = r.CommentType
                    }).ToList(),
                    Color = item.Color,
                    Size = item.Size,
                    Version = item.Version
                }).ToList();
                return JsonResult<dynamic>(listResult);
            }
        }
        /// <summary>
        /// 追加评价
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public JsonResult<Result<int>> PostAppendComment(CommentAppendCommentModel value)
        {
            CheckUserLogin();
            string productCommentsJSON = value.productCommentsJSON;
            //var commentService = ServiceProvider.Instance<ICommentService>.Create;
            var productComments = JsonConvert.DeserializeObject<List<AppendCommentModel>>(productCommentsJSON);

            foreach (var m in productComments)
            {
                m.UserId = CurrentUser.Id;
            }
            CommentApplication.Append(productComments);
            return JsonResult<int>();
        }
    }
}
