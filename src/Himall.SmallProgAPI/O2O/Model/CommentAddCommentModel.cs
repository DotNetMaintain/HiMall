using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.SmallProgAPI.O2O.Model
{
    /// <summary>
    /// 订单评论
    /// </summary>
    public class CommentAddCommentModel
    {
        public string Jsonstr { get; set; }
    }

    /// <summary>
    /// 追加评论
    /// </summary>
    public class CommentAppendCommentModel
    {
        public string productCommentsJSON { get; set; }
    }
}
