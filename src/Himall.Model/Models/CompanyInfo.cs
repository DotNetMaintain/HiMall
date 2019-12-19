using System.ComponentModel;

namespace Himall.Model
{
    public partial class CompanyInfo
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Contacts { get; set; }
        public string Phone { get; set; }
        public int RegionId { get; set; }
        public string Address { get; set; }
        public string AddressDetail { get; set; }
        public System.DateTime CreateDate { get; set; }
    }
}
