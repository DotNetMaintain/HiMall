using Himall.CommonModel;

namespace Himall.DTO.QueryModel
{
    public class ProductCommentQuery : QueryBase
    {
        public long? ShopId { set; get; }
        public long? ProductId { get; set; }
        public long? ShopBranchId { get; set; }
        public ProductCommentMarkType? CommentType { set; get; }
        public bool ShowHidden { get; set; }
    }
}
