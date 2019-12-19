using System.Collections.Generic;
using System.Linq;
using Himall.DTO;
using Himall.Core;

namespace Himall.Application
{
    public class ShopCategoryApplication
	{
		private static IServices.IShopCategoryService _shopCategoryService = ObjectContainer.Current.Resolve<IServices.IShopCategoryService>();

		public static List<ShopCategory> GetShopCategory(long shopId)
		{
			return _shopCategoryService.GetShopCategory(shopId).ToList().Map<List<ShopCategory>>();
		}
        /// <summary>
        /// 根据父级Id获取商品分类
        /// </summary>
        /// <param name="id"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static List<ShopCategory> GetCategoryByParentId(long id,long shopId)
        {
            return _shopCategoryService.GetCategoryByParentId(id, shopId).ToList().Map<List<ShopCategory>>();
        }

        /// <summary>
        /// 根据ID获取一个商家分类信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<Himall.Model.ShopCategoryInfo> GetCategorysByProductId(long id)
        {
            return _shopCategoryService.GetCategorysByProductId(id);
        }
    }
}
