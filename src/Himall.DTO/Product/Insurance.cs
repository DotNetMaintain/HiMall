using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.DTO
{
    public class Insurance
    {
        public new long Id { get; set; }
        public string Insurance_informed_consent { get; set; }
        public string Insurance_company { get; set; }
        public string Insurance_age { get; set; }
    }
}
