using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.CommonModel
{
    /// <summary>
    /// 会员审核状态
    /// </summary>
    public enum ReviewStatus
    {
        [Description("审核中")]
        Auditing = 0,
        [Description("同意")]
        Pass = 1,
        [Description("拒绝")]
        Refuse = 2
    }
}
