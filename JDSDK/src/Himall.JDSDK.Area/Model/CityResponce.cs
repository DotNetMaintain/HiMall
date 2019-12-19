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
    public class CityResponce
    {
        public string code { get; set; }

        public string charge { get; set; }

        public string msg { get; set; }

        public CityResults result { get; set; }
    }

    public class CityResults
    {
        public CityAreaGet jingdong_areas_city_get_responce { get; set; }
    }

    public class CityAreaGet
    {
        public string code { get; set; }
        public CityArea baseAreaServiceResponse { get; set; }
    }

    public class CityArea
    {
        public List<CityData> data { get; set; }
        public int resultCode { get; set; }
    }

    public class CityData
    {
        public int areaId { get; set; }

        public string areaName { get; set; }

        public int level { get; set; }

        public int parentId { get; set; }

        public int status { get; set; }
    }


}
