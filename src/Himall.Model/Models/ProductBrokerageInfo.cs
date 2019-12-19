using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Himall.Model
{
    public partial class ProductBrokerageInfo
    {
        [NotMapped]
        public int? MonthAgent { get; set; }
        [NotMapped]
        public int? WeekAgent { get; set; }

    }
}
