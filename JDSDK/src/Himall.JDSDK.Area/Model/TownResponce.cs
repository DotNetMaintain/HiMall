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
    public class TownResponce
    {
        public string code { get; set; }

        public string charge { get; set; }

        public string msg { get; set; }

        public TownResults result { get; set; }
    }

    public class TownResults
    {
        public TownAreaGet jingdong_areas_town_get_responce { get; set; }
    }

    public class TownAreaGet
    {
        public string code { get; set; }
        public TownArea baseAreaServiceResponse { get; set; }
    }

    public class TownArea
    {
        public List<TownData> data { get; set; }
        public int resultCode { get; set; }
    }

    public class TownData
    {
        public int areaId { get; set; }

        public string areaName { get; set; }

        public int level { get; set; }

        public int parentId { get; set; }

        public int status { get; set; }
    }


}
