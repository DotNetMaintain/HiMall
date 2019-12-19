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
    public class ProvinceResponce
    {
        public string code { get; set; }

        public string charge { get; set; }

        public string msg { get; set; }

        public ProvinceResults result { get; set; }
    }

    public class ProvinceResults
    {
        public ProvinceAreaGet jingdong_area_province_get_responce { get; set; }
    }

    public class ProvinceAreaGet
    {
        public string code { get; set; }

        public List<ProvinceAreas> province_areas { get; set; }

        public string success { get; set; }
    }

    public class ProvinceAreas
    {
        public string is3cod { get; set; }

        public string cod { get; set; }

        public string id { get; set; }

        public string name { get; set; }
    }
}
