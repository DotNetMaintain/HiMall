using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.SmallProgAPI.Model;
using Newtonsoft.Json;
using System.Web.Http.Results;

namespace Himall.SmallProgAPI
{
    public class CategoryController : BaseApiController
    {
        public JsonResult<Result<dynamic>> GetAllCategories()
        {
            var json = new Result<dynamic>();
            IEnumerable<CategoryInfo> categories = ServiceProvider.Instance<ICategoryService>.Create.GetMainCategory();
            if (categories == null)
            {
                json = ErrorResult<dynamic>("没获取到相应的分类", new int[0]);
            }
            else
            {
                var model = categories.Select(c => new
                {
                    cid = c.Id,
                    name = c.Name,
                    subs = ServiceProvider.Instance<ICategoryService>.Create.GetCategoryByParentId(c.Id).Select(a => new
                    {
                        cid = a.Id,
                        name = a.Name
                    })
                }).ToList();
                json = SuccessResult<dynamic>(data: model);
            }
            return Json(json);
        }
    }
}
