using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO.QueryModel
{
    public class PromoterQuery : QueryBase
    {
        public string UserName { set; get; }

        public PromoterInfo.PromoterStatus? Status { get; set; }

    }
}
