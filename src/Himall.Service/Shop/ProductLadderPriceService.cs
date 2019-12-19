using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.IServices;
using Himall.Model;

namespace Himall.Service
{
    public class ProductLadderPriceService : ServiceBase, IProductLadderPriceService
	{
		/// <summary>
		/// 根据id获取sku信息
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
        public List<ProductLadderPricesInfo> GetLadderPricesByIds(IEnumerable<long> ids)
		{
            return this.Context.ProductLadderPricesInfo.Where(p => ids.Contains(p.Id)).ToList();
		}

		/// <summary>
		/// 根据商品id获取sku信息
		/// </summary>
		/// <param name="productId"></param>
		/// <returns></returns>
        public List<ProductLadderPricesInfo> GetLadderPricesByProductIds(long productId)
		{
            return this.Context.ProductLadderPricesInfo.Where(p => p.ProductId == productId).ToList();
		}
	}
}
