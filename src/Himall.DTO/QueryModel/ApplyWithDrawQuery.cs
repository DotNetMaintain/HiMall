using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.DTO.QueryModel
{
    public class ApplyWithDrawQuery:QueryBase
    {
        public ApplyWithDrawInfo.ApplyWithDrawStatus? withDrawStatus{get;set;}

        public long? memberId{get;set;}

        public long? withDrawNo { get; set; }
        public Himall.CommonModel.UserWithdrawType? ApplyType { get; set; }
    }
}
