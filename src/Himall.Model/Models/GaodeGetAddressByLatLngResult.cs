using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Model
{
    public class GaodeGetAddressByLatLngResult
    {
        public int status { get; set; }
        public string info { get; set; }
        public Regeocode regeocode { get; set; }
    }
    public class Regeocode
    {
        public string formatted_address { get; set; }
        public AddressComponent addressComponent { get; set; }
    }

    public class AddressComponent
    {
        public string province { get; set; }

        public object city { get; set; }
        public string strcity
        {
            get
            {
                string result = string.Join(";", city);
                result=result.Replace("[", "");
                result=result.Replace("]", "");
                return result;
            }
        }

        public string district { get; set; }

        public string township { get; set; }

        public Building building { get; set; }

        public Neighborhood neighborhood { get; set; }
    }
    public class Building
    {
        public object name { get; set; }
        public string strname
        {
            get
            {
                string result = string.Join(";", name);
                result=result.Replace("[", "");
                result=result.Replace("]", "");
                return result;
            }
        }
    }
    public class Neighborhood
    {
        public string[] name { get; set; }
        public string strname { get { return string.Join(";", name); } }
    }
}
