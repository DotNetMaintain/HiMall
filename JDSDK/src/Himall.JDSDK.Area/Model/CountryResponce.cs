using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO.Swagger.Model
{
    /// <summary>
    /// 京东省份数据响应实体类
    /// </summary>
    public class CountryResponce
    {
        public string code { get; set; }

        public string charge { get; set; }

        public string msg { get; set; }

        public CountryResults result { get; set; }
    }

    public class CountryResults
    {
        public CountryAreaGet jingdong_areas_county_get_responce { get; set; }
    }

    public class CountryAreaGet
    {
        public string code { get; set; }
        public CountryArea baseAreaServiceResponse { get; set; }
    }

    public class CountryArea
    {
        public List<CountryData> data { get; set; }
        public int resultCode { get; set; }
    }

    public class CountryData
    {
        public int areaId { get; set; }

        public string areaName { get; set; }

        public int level { get; set; }

        public int parentId { get; set; }

        public int status { get; set; }
    }


}
