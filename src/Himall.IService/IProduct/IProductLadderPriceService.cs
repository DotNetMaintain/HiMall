using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;

namespace Himall.IServices
{
    public interface IProductLadderPriceService : IService
	{
		/// <summary>
		/// 根据id获取sku信息
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
		List<ProductLadderPricesInfo> GetLadderPricesByIds(IEnumerable<long> ids);

		/// <summary>
		/// 根据商品id获取sku信息
		/// </summary>
		/// <param name="productId"></param>
		/// <returns></returns>
        List<ProductLadderPricesInfo> GetLadderPricesByProductIds(long productId);
	}
}
