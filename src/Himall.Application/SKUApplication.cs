using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Himall.IServices;

namespace Himall.Application
{
    public class SKUApplication
    {
        private static ISkuService _skuService = ObjectContainer.Current.Resolve<IServices.ISkuService>();

        /// <summary>
        /// 根据id获取sku信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<DTO.SKU> GetByIds(IEnumerable<long> ids)
        {
            return _skuService.GetByIds(ids).Map<List<DTO.SKU>>();
        }

        /// <summary>
        /// 根据商品id获取sku信息
        /// </summary>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public static List<DTO.SKU> GetByProductIds(IEnumerable<long> productIds)
        {
            var data = _skuService.GetByProductIds(productIds);
            return data.Map<List<DTO.SKU>>();
        }
        /// <summary>
        /// 获取规格
        /// </summary>
        /// <param name="skuid"></param>
        /// <returns></returns>
        public static DTO.SKU GetSku(string skuid)
        {
            var data = _skuService.GetSku(skuid);
            return data.Map<DTO.SKU>();
        }
    }
}
