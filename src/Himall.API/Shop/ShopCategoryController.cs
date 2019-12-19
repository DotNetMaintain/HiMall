using Himall.Core;
using Himall.Core.Helper;
using Himall.Core.Plugins.OAuth;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Himall.API.Model.ParamsModel;
using Himall.API;
using Himall.DTO.QueryModel;
using Himall.Application;
using System.IO;
using Himall.API.Model;

namespace Himall.API
{
    public class ShopCategoryController : BaseShopApiController
    {
        IShopCategoryService _ishopCategoryService;
        public ShopCategoryController()
        {
            _ishopCategoryService = ServiceProvider.Instance<IShopCategoryService>.Create;
        }

        /// <summary>
        /// 商家分类
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public object GetShopCategories()
        {
            CheckUserLogin();
            var categories = _ishopCategoryService.GetMainCategory(CurrentUser.ShopId);
            var model = categories
                .Select(item => new CategoryModel()
                {
                    Id = item.Id,
                    Name = item.Name,
                    SubCategories = GetShopSubCategories(item.Id, 1),
                    Depth = 0,
                    DisplaySequence = item.DisplaySequence
                }).OrderBy(c => c.DisplaySequence);
            return new { success = true, Category = model };
        }

        IEnumerable<CategoryModel> GetShopSubCategories(long categoryId, int depth)
        {
            var categories = _ishopCategoryService.GetCategoryByParentId(categoryId)
                .Select(item =>
                {
                    return new CategoryModel()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        SubCategories = GetShopSubCategories(item.Id, depth + 1),
                        Depth = 1,
                        DisplaySequence = item.DisplaySequence
                    };
                })
                   .OrderBy(c => c.DisplaySequence);
            return categories;
        }
    }
}
