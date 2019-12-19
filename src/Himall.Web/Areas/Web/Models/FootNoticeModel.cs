using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Himall.Web.Areas.Web.Models
{
    public class FootNoticeModel
    {
        public string CateogryName { get; set; }
        public List<ArticleInfo> List { get; set; }
    }
}