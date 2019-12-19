using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.DTO.QueryModel
{
    public partial class CollocationQuery : QueryBase
    {
        public string Title { get; set; }
        public long ? ShopId { get; set; }
        public int Status { get; set; }
    }
}
