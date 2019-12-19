using Himall.API.Model.ParamsModel;
using Himall.Application;
using Himall.DTO.QueryModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Himall.API
{
    public class ShopBranchProductController : BaseShopBranchApiController
    {
        /// <summary>
        /// 门店查询商家商品
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public object GetProducts( 
            string keywords = "", /* 搜索关键字 */
            long cid = 0,  /* 分类ID */
            int pageNo = 1, /*页码*/
            int pageSize = 10/*每页显示数据量*/
            )
        {
            CheckUserLogin();
            ProductSearch model = new ProductSearch()
            {
                shopId = CurrentShopBranch.ShopId,
                Keyword = keywords,
                PageNumber = pageNo,
                PageSize = pageSize,
                CategoryId = cid,
                shopBranchId = CurrentShopBranch.Id,
                HasLadderProduct = false
            };
            model.AttrIds = new List<string>() { };
            var products = ProductManagerApplication.GetProducts(model);
			var skus=SKUApplication.GetByProductIds(products.Models.Where(p=>p.SKUS==null).Select(p => p.Id));
			var product = products.Models.ToList().Select(item => new
			{
				id = item.Id,
				name = item.ProductName,
				price = item.MinSalePrice,
				salesCount = item.SaleCounts,
                img = Core.HimallIO.GetRomoteProductSizeImage(item.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_100),
                stock = item.SKUS == null ? skus.Where(sku => sku.ProductId == item.Id).Sum(sku => sku.Stock) : item.SKUS.Sum(e => e.Stock),
                productCode = item.ProductCode
            });
            var result = new
            {
                success = true,
                products = product,
                total = products.Total
            };
            return result;
        }
        /// <summary>
        /// 门店添加商品
        /// </summary>
        /// <param name="pids"></param>
        /// <returns></returns>
        public object GetAddProducts(string pids)
        {
            CheckUserLogin();
            if (string.IsNullOrWhiteSpace(pids))
                return new { success = false, msg = "参数异常" };
            var ids = ConvertToIEnumerable(pids);
            ShopBranchApplication.AddProductSkus(ids, CurrentShopBranch.Id, CurrentShopBranch.ShopId);
            return new { success=true,msg="添加成功" };
        }
        private IEnumerable<long> ConvertToIEnumerable(string str, char sp = ',')
        {
            var ids = str.Split(sp).Select(e =>
            {
                long id = 0;
                if (!long.TryParse(e, out id))
                {
                    id = 0;
                }
                return id;
            });
            return ids;
        }
        /// <summary>
        /// 下架商品
        /// </summary>
        /// <param name="pids"></param>
        /// <returns></returns>
        public object GetUnSaleProduct(string pids)
        {
            CheckUserLogin();
            if (string.IsNullOrWhiteSpace(pids))
                return new { success = false, msg = "参数异常" };
            var ids = ConvertToIEnumerable(pids);
            ShopBranchApplication.UnSaleProduct(CurrentShopBranch.Id, ids);
            return new { success = true, msg = "已下架" };
        }
        /// <summary>
        /// 上架商品
        /// </summary>
        /// <param name="pids"></param>
        /// <returns></returns>
        public object GetOnSaleProduct(string pids)
        {
            CheckUserLogin();
            if (string.IsNullOrWhiteSpace(pids))
                return new { success = false, msg = "参数异常" };
            var ids = ConvertToIEnumerable(pids);
            if (!ShopBranchApplication.CanOnSaleProduct(ids))
            {
                return new { success = false, msg = "有不在销售状态的商品存在，不可执行上架操作" };
            }
            if (ShopBranchApplication.IsOpenLadderInProducts(ids))
            {
                return new { success = false, msg = "有商品为阶梯批发商品，不能上架到门店" };
            }
            ShopBranchApplication.OnSaleProduct(CurrentShopBranch.Id, ids);
            return new { success = true, msg = "已上架" };
        }
        /// <summary>
        /// 设置商品库存
        /// </summary>
        /// <param name="pids"></param>
        /// <param name="stock"></param>
        /// <param name="optype"></param>
        /// <returns></returns>
        [HttpPost]
        public object PostSetProductStock(SetProductStockModel model)
        {
            CheckUserLogin();
            if (string.IsNullOrWhiteSpace(model.pids))
                return new { success = false, msg = "参数异常" };
            var ids = ConvertToIEnumerable(model.pids);
            var type = (CommonModel.StockOpType)model.optype;
            ShopBranchApplication.SetProductStock(CurrentShopBranch.Id, ids, model.stock, type);
            return new { success = true, msg = "设置成功" }; 
        }
        /// <summary>
        /// 设置SKU库存
        /// </summary>
        /// <param name="skus"></param>
        /// <param name="stock"></param>
        /// <param name="optype"></param>
        /// <returns></returns>
        [HttpPost]
        public object PostSetSkuStock(SetSkuStockModel model)
        {
            CheckUserLogin();
            if (string.IsNullOrWhiteSpace(model.skus))
                return new { success = false, msg = "参数异常" };

            var ids = model.skus.Split(',').ToList();
            var stk = model.stock.Split(',').Select(e =>
            {
                int id = 0;
                if (!int.TryParse(e, out id))
                {
                    id = 0;
                }
                return id;
            });
            var type = (CommonModel.StockOpType)model.optype;

			ShopBranchApplication.SetSkuStock(CurrentShopBranch.Id, ids, stk, type);
            return new { success = true, msg = "设置成功" };
        }
        /// <summary>
        /// 查询门店商品
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="cid"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="branchProductStatus"></param>
        /// <returns></returns>
        public object GetShopBranchProducts(
            string keywords = "", /* 搜索关键字 */
            long cid = 0,  /* 分类ID */
            int pageNo = 1, /*页码*/
            int pageSize = 10,/*每页显示数据量*/
            int branchProductStatus=0
            )
        {
            CheckUserLogin();
            ShopBranchProductQuery query = new ShopBranchProductQuery
            {
                PageNo = pageNo,
                PageSize = pageSize,
                KeyWords = keywords,
                ShopId = CurrentShopBranch.ShopId,
                shopBranchId = CurrentShopBranch.Id,
                ShopBranchProductStatus = (CommonModel.ShopBranchSkuStatus)branchProductStatus,
                HasLadderProduct = false
            };
            if (cid > 0)
            {
                query.ShopCategoryId = cid;
            }
            //查询商品
            var pageModel = ShopBranchApplication.GetShopBranchProducts(query);

            //查询门店SKU库存
            List<string> skuids = new List<string>();
            foreach(var p in pageModel.Models)
            {
                skuids.AddRange(p.SKUInfo.Select(e => e.Id));
            }
            var shopBranchSkus = ShopBranchApplication.GetSkusByIds(CurrentShopBranch.Id,skuids);
            //
            var product = pageModel.Models.ToList().Select(item =>
            {
                return new
                {
                    id = item.Id,
                    name = item.ProductName,
                    price = item.MinSalePrice,
                    salesCount = item.SaleCounts,
                    img = Core.HimallIO.GetRomoteProductSizeImage(item.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_350),
                    stock = shopBranchSkus.Where(e => e.ProductId == item.Id).Sum(s => s.Stock),
                    productCode = item.ProductCode
                };
            });
            var result = new
            {
                success = true,
                products = product,
                skus = shopBranchSkus,
                total = pageModel.Total
            };
            return result;
        }
        /// <summary>
        /// 取商品SKU
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public object GetProductSku(long pid)
        {
            CheckUserLogin();
            var sku = ShopBranchApplication.GetSkusByProductId(CurrentUser.ShopBranchId, pid);
            return new { success = true, sku = sku };
        }
    }
}
