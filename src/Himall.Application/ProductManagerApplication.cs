using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.DTO.Product;
using Himall.Model;
using Himall.Core;
using Himall.IServices;
using Himall.Application.Mappers;
using Himall.DTO.QueryModel;
using Himall.DTO;
using Himall.CommonModel;
using System.Net;
using System.IO;

namespace Himall.Application
{
    public class ProductManagerApplication
    {
        private static IProductService _productService = ObjectContainer.Current.Resolve<Himall.IServices.IProductService>();
        private static IProductDescriptionTemplateService _productDescriptionTemplateService = ObjectContainer.Current.Resolve<IServices.IProductDescriptionTemplateService>();
        private static ISearchProductService _searchProductService = ObjectContainer.Current.Resolve<IServices.ISearchProductService>();
        private static IProductLadderPriceService _productLadderPriceService = ObjectContainer.Current.Resolve<IServices.IProductLadderPriceService>();


        #region 方法
        /// <summary>
        /// 添加商品
        /// </summary>
        /// <param name="shopId">店铺id</param>
        /// <param name="product">商品信息</param>
        /// <param name="pics">需要转移的商品图片地址</param>
        /// <param name="skus">skus，至少要有一项</param>
        /// <param name="description">描述</param>
        /// <param name="attributes">商品属性</param>
        /// <param name="goodsCategory">商家分类</param>
        /// <param name="sellerSpecifications">商家自定义规格</param>
        public static Product AddProduct(long shopId, Product product, string[] pics, SKU[] skus, ProductDescription description, ProductAttribute[] attributes, long[] goodsCategory, SellerSpecificationValue[] sellerSpecifications, ProductLadderPrice[] prices)
        {
            var productInfo = product.Map<ProductInfo>();
            var skuInofs = skus.Map<SKUInfo[]>();
            var descriptionInfo = description.Map<ProductDescriptionInfo>();
            var attributeInfos = attributes.Map<ProductAttributeInfo[]>();
            var sellerSpecificationInfos = sellerSpecifications.Map<SellerSpecificationValueInfo[]>();
            var ladderpricesInfos = prices.Select(p =>
            {
                var ladder = new ProductLadderPricesInfo();
                ladder.Id = p.Id;
                ladder.MinBath = p.MinBath;
                ladder.MaxBath = p.MaxBath;
                ladder.ProductId = p.ProductId;
                ladder.Price = p.Price;
                return ladder;
            }).ToArray();
            _productService.AddProduct(shopId, productInfo, pics, skuInofs, descriptionInfo, attributeInfos,
                goodsCategory, sellerSpecificationInfos, ladderpricesInfos);
            CreateHtml(productInfo.Id);
            //  DTO.Product.Product p = new Product();
            return AutoMapper.Mapper.Map<Product>(productInfo);
        }

        /// <summary>
        /// 更新商品
        /// </summary>
        /// <param name="product">修改后的商品</param>
        /// <param name="pics">需要转移的商品图片地址</param>
        /// <param name="skus">skus，至少要有一项</param>
        /// <param name="description">描述</param>
        /// <param name="attributes">商品属性</param>
        /// <param name="goodsCategory">商家分类</param>
        /// <param name="sellerSpecifications">商家自定义规格</param>
        public static void UpdateProduct(Product product, string[] pics, SKU[] skus, ProductDescription description, ProductAttribute[] attributes, long[] goodsCategory, SellerSpecificationValue[] sellerSpecifications, ProductLadderPrice[] prices)
        {
            var productInfo = _productService.GetProduct(product.Id);
            if (productInfo == null)
                throw new HimallException("指定id对应的数据不存在");

            var editStatus = (ProductInfo.ProductEditStatus)productInfo.EditStatus;

            if (product.ProductName != productInfo.ProductName)
                editStatus = GetEditStatus(editStatus);
            if (product.ShortDescription != productInfo.ShortDescription)
                editStatus = GetEditStatus(editStatus);

            product.AddedDate = productInfo.AddedDate;
            if (productInfo.SaleStatus != ProductInfo.ProductSaleStatus.InDraft)
            {
                product.SaleStatus = productInfo.SaleStatus;
            }
            product.AuditStatus = productInfo.AuditStatus;
            product.DisplaySequence = productInfo.DisplaySequence;
            product.ShopId = productInfo.ShopId;
            product.HasSKU = productInfo.HasSKU;
            product.ImagePath = productInfo.ImagePath;
            product.SaleCounts = productInfo.SaleCounts;

            if (pics != null)
            {
                if (pics.Any(path => string.IsNullOrWhiteSpace(path) || !path.StartsWith(productInfo.ImagePath)))//有任何修改过的图片
                {
                    editStatus = GetEditStatus(editStatus);
                }
            }

            //product.IsOpenLadder = prices != null && prices.Length > 0 && (prices[0].MinBath > 0);//是否开启阶梯价
            if (product.IsOpenLadder)
            {
                editStatus = GetEditStatus(editStatus);
            }

            product.Himall_Shops = productInfo.Himall_Shops;
            product.DynamicMap(productInfo);

            productInfo.EditStatus = (int)editStatus;

            var skuInofs = skus.Map<SKUInfo[]>();
            var descriptionInfo = description.Map<ProductDescriptionInfo>();
            var attributeInfos = attributes.Map<ProductAttributeInfo[]>();
            var sellerSpecificationInfos = sellerSpecifications.Map<SellerSpecificationValueInfo[]>();
            var ladderpricesInfos = prices.Select(p =>
            {
                var ladder = new ProductLadderPricesInfo();
                ladder.Id = p.Id;
                ladder.MinBath = p.MinBath;
                ladder.MaxBath = p.MaxBath;
                ladder.ProductId = p.ProductId;
                ladder.Price = p.Price;
                return ladder;
            }).ToArray();
            _productService.UpdateProduct(productInfo, pics, skuInofs, descriptionInfo, attributeInfos, goodsCategory,
                sellerSpecificationInfos, ladderpricesInfos);
            if (productInfo.IsOpenLadder)
            {
                //处理门店
                ShopBranchApplication.UnSaleProduct(productInfo.Id);
            }
            CreateHtml(product.Id);
        }


        /// <summary>
        /// 生成指定商品详情html
        /// </summary>
        public static void CreateHtml(long productId)
        {
            WebClient wc = new WebClient();
            var preUrl = System.Configuration.ConfigurationManager.AppSettings["CurDomainUrl"];
            string url = preUrl + "/Product/Details/" + productId;
            string wapurl = preUrl + "/m-wap/Product/Details/" + productId + "?nojumpfg = 1";
            string urlHtml = "/Storage/Products/Statics/" + productId + ".html";
            string wapHtml = "/Storage/Products/Statics/" + productId + "-wap.html";
            var data = wc.DownloadData(url);
            var wapdata = wc.DownloadData(wapurl);
            MemoryStream memoryStream = new MemoryStream(data);
            MemoryStream wapMemoryStream = new MemoryStream(wapdata);
            HimallIO.CreateFile(urlHtml, memoryStream, FileCreateType.Create);
            HimallIO.CreateFile(wapHtml, wapMemoryStream, FileCreateType.Create);
        }

        static void CreatPCHtml(long productId)
        {
            WebClient wc = new WebClient();
            var preUrl = System.Configuration.ConfigurationManager.AppSettings["CurDomainUrl"];
            string url = preUrl + "/Product/Details/" + productId;
            string urlHtml = "/Storage/Products/Statics/" + productId + ".html";
            var data = wc.DownloadData(url);
            MemoryStream memoryStream = new MemoryStream(data);
            HimallIO.CreateFile(urlHtml, memoryStream, FileCreateType.Create);
        }

        static void CreatWAPHtml(long productId)
        {
            WebClient wc = new WebClient();
            var preUrl = System.Configuration.ConfigurationManager.AppSettings["CurDomainUrl"];
            string wapurl = preUrl + "/m-wap/Product/Details/" + productId + "?nojumpfg=1";
            string wapHtml = "/Storage/Products/Statics/" + productId + "-wap.html";
            var wapdata = wc.DownloadData(wapurl);
            MemoryStream wapMemoryStream = new MemoryStream(wapdata);
            HimallIO.CreateFile(wapHtml, wapMemoryStream, FileCreateType.Create);
        }

        static void CreatBrandchWAPHtml(long productId, long branchId)
        {
            WebClient wc = new WebClient();
            var preUrl = System.Configuration.ConfigurationManager.AppSettings["CurDomainUrl"];
            string wapBranchurl = preUrl + "/m-wap/BranchProduct/Details/" + productId + "?nojumpfg=1&shopBranchId=" + branchId;
            string wapBranchHtml = "/Storage/Products/Statics/" + productId + "-" + branchId + "-wap-branch.html";
            var wapbranchdata = wc.DownloadData(wapBranchurl);
            MemoryStream wapMemoryStream = new MemoryStream(wapbranchdata);
            HimallIO.CreateFile(wapBranchHtml, wapMemoryStream, FileCreateType.Create);
        }

        /// <summary>
        /// 获取指定商品详情html
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static void GetPCHtml(long productId)
        {
            string pcUrlHtml = "/Storage/Products/Statics/" + productId + ".html";
            string fuleUrl = Core.Helper.IOHelper.GetMapPath(pcUrlHtml);
            //if (File.Exists(fuleUrl))
            //{
            //    TimeSpan ts = DateTime.Now - File.GetLastWriteTime(fuleUrl);
            //    if (ts.TotalMinutes > 20)
            //        RefreshLocalProductHtml(productId, pcUrlHtml, fuleUrl);
            //}
            //else
            RefreshLocalProductHtml(productId, pcUrlHtml, fuleUrl);
        }

        /// <summary>
        /// 获取指定商品详情html
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static void GetWAPHtml(long productId)
        {
            string wapUrlHtml = "/Storage/Products/Statics/" + productId + "-wap.html";
            string fullUrl = Core.Helper.IOHelper.GetMapPath(wapUrlHtml);
            //if (File.Exists(fuleUrl))
            //{
            //    TimeSpan ts = DateTime.Now - File.GetLastWriteTime(fuleUrl);
            //    if (ts.TotalMinutes > 20)
            //        RefreshWAPLocalProductHtml(productId, wapUrlHtml, fuleUrl);
            //}
            //else
            RefreshWAPLocalProductHtml(productId, wapUrlHtml, fullUrl);
        }
        /// <summary>
        /// 获取指定门店商品详情html
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static void GetWAPBranchHtml(long productId, long branchId)
        {
            string wapUrlHtml = "/Storage/Products/Statics/" + productId + "-" + branchId + "-wap-branch.html";
            string fullUrl = Core.Helper.IOHelper.GetMapPath(wapUrlHtml);
            //if (File.Exists(fuleUrl))
            //{
            //    TimeSpan ts = DateTime.Now - File.GetLastWriteTime(fuleUrl);
            //    if (ts.TotalMinutes > 20)
            //        RefreshWAPLocalBranchProductHtml(productId, wapUrlHtml, fuleUrl, branchId);
            //}
            //else
            RefreshWAPLocalBranchProductHtml(productId, wapUrlHtml, fullUrl, branchId);
        }

        /// <summary>
        /// 刷新本地缓存商品html文件 
        /// </summary>     
        /// <param name="targetFilename">本地待生成的html文件名</param>
        static void RefreshWAPLocalProductHtml(long productId, string htmlUrl, string targetFilename)
        {
            lock (htmlUrl)
            {
                if (!File.Exists(targetFilename) || CheckNeedRefreshFile(File.GetLastWriteTime(targetFilename), 20))
                {
                    if (!HimallIO.ExistFile(htmlUrl))
                        CreatWAPHtml(productId);
                    else
                    {
                        var metaRemoteInfo = HimallIO.GetFileMetaInfo(htmlUrl);
                        if (null == metaRemoteInfo || CheckNeedRefreshFile(metaRemoteInfo.LastModifiedTime, 60))
                        {
                            CreatWAPHtml(productId);
                        }
                    }

                    var dirFullname = Core.Helper.IOHelper.GetMapPath("/Storage/Products/Statics");
                    if (!Directory.Exists(dirFullname))
                        Directory.CreateDirectory(dirFullname);
                    byte[] test = HimallIO.GetFileContent(htmlUrl);
                    File.WriteAllBytes(targetFilename, HimallIO.GetFileContent(htmlUrl));
                }
            }
        }

        /// <summary>
        /// 检查文件信息
        /// </summary>
        /// <param name="remote"></param>
        /// <param name="local"></param>
        /// <returns></returns>
        private static bool CheckNeedRefreshFile(DateTime modified, int minutes)
        {
            return (DateTime.Now - modified).TotalMinutes > minutes;
        }

        /// <summary>
        /// 刷新本地缓存门店商品html文件 
        /// </summary>     
        /// <param name="targetFilename">本地待生成的html文件名</param>
        static void RefreshWAPLocalBranchProductHtml(long productId, string htmlUrl, string targetFilename, long branchId)
        {
            lock (htmlUrl)
            {
                if (!File.Exists(targetFilename) || CheckNeedRefreshFile(File.GetLastWriteTime(targetFilename), 20))
                {
                    if (!HimallIO.ExistFile(htmlUrl))
                        CreatBrandchWAPHtml(productId, branchId);
                    else
                    {
                        var metaRemoteInfo = HimallIO.GetFileMetaInfo(htmlUrl);
                        if (null == metaRemoteInfo || CheckNeedRefreshFile(metaRemoteInfo.LastModifiedTime, 60))
                        {
                            CreatBrandchWAPHtml(productId, branchId);
                        }
                    }
                    var dirFullname = Core.Helper.IOHelper.GetMapPath("/Storage/Products/Statics");
                    if (!Directory.Exists(dirFullname))
                        Directory.CreateDirectory(dirFullname);
                    byte[] test = HimallIO.GetFileContent(htmlUrl);
                    File.WriteAllBytes(targetFilename, HimallIO.GetFileContent(htmlUrl));
                }
            }
        }

        /// <summary>
        /// 刷新本地缓存商品html文件 
        /// </summary>
        /// <param name="htmlUrl">远程html文件地址</param>
        /// <param name="targetFilename">本地待生成的html文件名</param>
        static void RefreshLocalProductHtml(long productId, string htmlUrl, string targetFilename)
        {
            lock (htmlUrl)
            {
                if (!File.Exists(targetFilename) || CheckNeedRefreshFile(File.GetLastWriteTime(targetFilename), 20))
                {
                    if (!HimallIO.ExistFile(htmlUrl))
                        CreatPCHtml(productId);
                    else
                    {
                        var metaRemoteInfo = HimallIO.GetFileMetaInfo(htmlUrl);
                        if (null == metaRemoteInfo || CheckNeedRefreshFile(metaRemoteInfo.LastModifiedTime, 60))
                        {
                            CreatPCHtml(productId);
                        }
                    }
                    var dirFullname = Core.Helper.IOHelper.GetMapPath("/Storage/Products/Statics");
                    if (!Directory.Exists(dirFullname))
                        Directory.CreateDirectory(dirFullname);
                    byte[] test = HimallIO.GetFileContent(htmlUrl);
                    File.WriteAllBytes(targetFilename, HimallIO.GetFileContent(htmlUrl));
                }
            }
        }

        /// <summary>
        /// 刷新html文件
        /// </summary>
        /// <param name="productId"></param>
        static void RefreshProductHtmlInRemote(long productId)
        {
            //string urlHtml = "/Storage/Products/Statics/" + productId + ".html";
            //using (Cache.GetCacheLocker(urlHtml))
            //{
            //if (!HimallIO.ExistFile(urlHtml))
            //CreatPCHtml(productId);
            //}
        }

        /// <summary>
        /// 刷新移动端html文件
        /// </summary>
        /// <param name="productId"></param>
        static void RefreshWapProductHtmlInRemote(long productId)
        {
            //string urlHtml = "/Storage/Products/Statics/" + productId + "-wap.html";
            //using (Cache.GetCacheLocker(urlHtml))
            //{
            //if (!HimallIO.ExistFile(urlHtml))
            CreatWAPHtml(productId);
            //}
        }
        /// <summary>
        /// 刷新移动端门店html文件
        /// </summary>
        /// <param name="productId"></param>
        static void RefreshWapBranchProductHtmlInRemote(long productId, long branchId)
        {
            //string urlHtml = "/Storage/Products/Statics/" + productId + "-" + branchId + "-wap-branch.html";
            //using (Cache.GetCacheLocker(urlHtml))
            //{
            //if (!HimallIO.ExistFile(urlHtml))
            //CreatBrandchWAPHtml(productId, branchId);
            //}
        }


        /// <summary>
        /// 获取一个商品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Product GetProduct(long id)
        {
            var product = _productService.GetProduct(id);
            var ret = product.Map<Product>();
            ret.Himall_Shops = product.Himall_Shops;
            return ret;
        }
        /// <summary>
        /// 根据多个ID取多个商品信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<Product> GetProductsByIds(IEnumerable<long> ids)
        {
            var productsInfo = _productService.GetProductByIds(ids);
            return productsInfo.ToList().Map<List<Product>>();
        }
        /// <summary>
        /// 根据多个ID，取商品信息（所有状态）
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<Product> GetAllStatusProductByIds(IEnumerable<long> ids)
        {
            var productsInfo = _productService.GetAllStatusProductByIds(ids);
            return productsInfo.ToList().Map<List<Product>>();
        }
        public static QueryPageModel<Product> GetProducts(ProductSearch query)
        {
            var data = _productService.SearchProduct(query);

            return new QueryPageModel<Product>()
            {
                Models = data.Models.ToList().Map<List<Product>>(),
                Total = data.Total
            };
        }

        public static QueryPageModel<Product> GetProducts(ProductQuery query)
        {
            var data = _productService.GetProducts(query);

            return new QueryPageModel<Product>()
            {
                Models = data.Models.ToList().Map<List<Product>>(),
                Total = data.Total
            };
        }

        /// <summary>
        /// 根据商品id获取属性
        /// </summary>
        /// <param name="id">商品id</param>
        /// <returns></returns>
        public static List<DTO.ProductAttribute> GetProductAttribute(long id)
        {
            var entities = _productService.GetProductAttribute(id).ToList();
            return AutoMapper.Mapper.Map<List<DTO.ProductAttribute>>(entities);
        }

        /// <summary>
        /// 根据商品id获取描述
        /// </summary>
        /// <param name="id">商品id</param>
        /// <returns></returns>
        public static DTO.ProductDescription GetProductDescription(long id)
        {
            var description = _productService.GetProductDescription(id);
            return AutoMapper.Mapper.Map<DTO.ProductDescription>(description);
        }

        /// <summary>
        /// 根据商品id获取描述
        /// </summary>
        /// <param name="ids">商品ids</param>
        /// <returns></returns>
        public static List<DTO.ProductDescription> GetProductDescription(long[] ids)
        {
            var description = _productService.GetProductDescriptions(ids);
            return AutoMapper.Mapper.Map<List<DTO.ProductDescription>>(description);
        }

        public static List<ProductShopCategory> GetProductShopCategoriesByProductId(long productId)
        {
            return _productService.GetProductShopCategories(productId).ToList().Map<List<ProductShopCategory>>();
        }

        /// <summary>
        /// 根据商品id获取SKU
        /// </summary>
        /// <param name="id">商品id</param>
        /// <returns></returns>
        public static List<DTO.SKU> GetSKU(long id)
        {
            var skus = _productService.GetSKUs(id).ToList();
            return AutoMapper.Mapper.Map<List<DTO.SKU>>(skus);
        }

        /// <summary>
        /// 根据商品id获取SKU
        /// </summary>
        /// <param name="productIds">商品id</param>
        /// <returns></returns>
        public static List<DTO.SKU> GetSKU(IEnumerable<long> productIds)
        {
            var skus = _productService.GetSKUs(productIds).ToList();
            return AutoMapper.Mapper.Map<List<DTO.SKU>>(skus);
        }

        /// <summary>
        /// 根据sku id 获取sku信息
        /// </summary>
        /// <param name="skuIds"></param>
        /// <returns></returns>
        public static List<DTO.SKU> GetSKUs(IEnumerable<string> skuIds)
        {
            var list = _productService.GetSKUs(skuIds);
            return list.Map<List<DTO.SKU>>();
        }
        /// <summary>
        /// 根据商品id获取SKU
        /// </summary>
        /// <param name="id">商品id</param>
        /// <returns></returns>
        public static DTO.SKU GetSKU(string skuId)
        {
            var sku = _productService.GetSku(skuId);
            var ret = AutoMapper.Mapper.Map<DTO.SKU>(sku);
            return ret;
        }
        /// <summary>
        /// 根据商品id获取SKU
        /// </summary>
        /// <param name="skuId"></param>
        /// <returns></returns>
        public static SKUInfo GetSKUInfo(string skuId)
        {
            var sku = _productService.GetSku(skuId);
            return sku;
        }

        /// <summary>
        /// 获取商品的评论数
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static int GetProductCommentCount(long productId)
        {
            return _productService.GetProductCommentCount(productId);
        }
        /// <summary>
        /// 取店铺超出安全库存的商品数
        /// </summary>
        /// <param name="shopid"></param>
        /// <returns></returns>
		public static long GetOverSafeStockProducts(long shopid)
        {
            var products = _productService.GetProducts(new ProductQuery
            {
                ShopId = shopid,
                OverSafeStock = true
            });
            return products.Total;

        }
        /// <summary>
        /// 取超出警戒库存的商品ID
        /// </summary>
        /// <param name="pids"></param>
        /// <returns></returns>
        public static IEnumerable<long> GetOverSafeStockProductIds(IEnumerable<long> pids)
        {
            var skus = _productService.GetSKUs(pids).ToList();
            var overStockPids = skus.Where(e => e.SafeStock >= e.Stock).Select(e => e.ProductId).Distinct();
            return overStockPids;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pids"></param>
        /// <param name="stock"></param>
        public static void SetProductOverSafeStock(IEnumerable<long> pids, long stock)
        {
            _productService.SetProductOverSafeStock(pids, stock);
        }
        /// <summary>
        /// 删除门店对应的商品
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="shopId"></param>
        public static void DeleteProduct(IEnumerable<long> ids, long shopId)
        {
            _productService.DeleteProduct(ids, shopId);
        }

        /// <summary>
        /// 修改推荐商品
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="relationProductIds"></param>
        public static void UpdateRelationProduct(long productId, string relationProductIds)
        {
            _productService.UpdateRelationProduct(productId, relationProductIds);
        }

        /// <summary>
        /// 获取商品的推荐商品
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static DTO.ProductRelationProduct GetRelationProductByProductId(long productId)
        {
            return _productService.GetRelationProductByProductId(productId).Map<DTO.ProductRelationProduct>();
        }

        /// <summary>
        /// 获取商品的推荐商品
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static List<ProductRelationProduct> GetRelationProductByProductIds(IEnumerable<long> productIds)
        {
            return _productService.GetRelationProductByProductIds(productIds).Map<List<DTO.ProductRelationProduct>>();
        }

        /// <summary>
        /// 获取指定类型下面热销的前N件商品
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<Product> GetHotSaleProductByCategoryId(int categoryId, int count)
        {
            return _productService.GetHotSaleProductByCategoryId(categoryId, count).Map<List<Product>>();
        }

        /// <summary>
        /// 获取商家所有商品描述模板
        /// </summary>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static List<ProductDescriptionTemplate> GetDescriptionTemplatesByShopId(long shopId)
        {
            return _productDescriptionTemplateService.GetTemplates(shopId).ToList().Map<List<ProductDescriptionTemplate>>();
        }
        /// <summary>
        /// 批量下架商品
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="shopid"></param>
        public static void BatchSaleOff(IEnumerable<long> ids, long shopid)
        {
            _productService.SaleOff(ids, shopid);
        }
        /// <summary>
        /// 批量上架商品
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="shopid"></param>
        public static void BatchOnSale(IEnumerable<long> ids, long shopid)
        {
            _productService.OnSale(ids, shopid);
        }

        /// <summary>
        /// 设置SKU库存
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="skuIds"></param>
        /// <param name="stock"></param>
        /// <param name="opType"></param>
        public static void SetSkuStock(IEnumerable<string> skuIds, IEnumerable<int> stock, CommonModel.StockOpType opType)
        {
            switch (opType)
            {
                case StockOpType.Normal:
                    _productService.SetSkusStock(skuIds, stock);
                    break;
                case StockOpType.Add:
                    _productService.AddSkuStock(skuIds, stock);
                    break;
                case StockOpType.Reduce:
                    _productService.ReduceSkuStock(skuIds, stock);
                    break;
            }
        }
        /// <summary>
        /// 设置商品库存
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="pids"></param>
        /// <param name="stock"></param>
        /// <param name="opType"></param>
        public static void SetProductStock(IEnumerable<long> pids, int stock, CommonModel.StockOpType opType)
        {
            switch (opType)
            {
                case StockOpType.Normal:
                    _productService.SetMoreProductToOneStock(pids, stock);
                    break;
                case StockOpType.Add:
                    _productService.AddProductStock(pids, stock);
                    break;
                case StockOpType.Reduce:
                    _productService.ReduceProductStock(pids, stock);
                    break;
            }
        }

        public static bool BranchCanBuy(long userId, long productId, int count, string skuId, long shopBranchId, out int reason)
        {
            var product = _productService.GetProduct(productId);
            if (product.SaleStatus != ProductInfo.ProductSaleStatus.OnSale || product.AuditStatus != ProductInfo.ProductAuditStatus.Audited)
            {
                //商城商品下架，但是门店的商品状态销售中，允许用户购买。
                //商城商品下架后，销售状态-仓库中，审核状态-待审核
                if (product.SaleStatus != ProductInfo.ProductSaleStatus.InStock && product.AuditStatus != ProductInfo.ProductAuditStatus.WaitForAuditing)
                {
                    reason = 1;
                    return false;
                }
            }
            var sku = ProductManagerApplication.GetSKU(skuId);
            if (sku == null)
            {
                reason = 2;
                return false;
            }
            var shopBranch = ShopBranchApplication.GetShopBranchById(shopBranchId);
            if (shopBranch == null)
            {
                reason = 4;
                return false;
            }
            var shopBranchSkuList = ShopBranchApplication.GetSkusByIds(shopBranchId, new List<string> { skuId });
            if (shopBranchSkuList == null || shopBranchSkuList.Count == 0 || shopBranchSkuList[0].Status == ShopBranchSkuStatus.InStock)
            {
                reason = 2;
                return false;
            }
            var sbsku = shopBranchSkuList.FirstOrDefault();
            if (sbsku.Stock < count)
            {
                reason = 9;
                return false;
            }
            if (product.IsDeleted)
            {
                reason = 2;
                return false;
            }

            if (product.MaxBuyCount <= 0)
            {
                reason = 0;
                return true;
            }

            var buyedCounts = OrderApplication.GetProductBuyCount(userId, new long[] { productId });
            if (product.MaxBuyCount < count + (buyedCounts.ContainsKey(productId) ? buyedCounts[productId] : 0))
            {
                reason = 3;
                return false;
            }
            reason = 0;
            return true;
        }
        /// <summary>
        /// 普通商品是否可购买（过滤活动购买数量）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="productId"></param>
        /// <param name="count"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static bool CanBuy(long userId, long productId, int count, out int reason)
        {
            var product = _productService.GetProduct(productId);
            if (product.SaleStatus != ProductInfo.ProductSaleStatus.OnSale || product.AuditStatus != ProductInfo.ProductAuditStatus.Audited)
            {
                reason = 1;
                return false;
            }
            long stock = product.SKUInfo.Sum(p => p.Stock);
            if (stock == 0)
            {
                reason = 9;
                return false;
            }
            if (product.IsDeleted)
            {
                reason = 2;
                return false;
            }

            if (product.MaxBuyCount <= 0)
            {
                reason = 0;
                return true;
            }

            if (product.IsOpenLadder)
            {
                reason = 0;
                return true;
            }

            var buyedCounts = OrderApplication.GetProductBuyCount(userId, new long[] { productId });
            if (product.MaxBuyCount < count + (buyedCounts.ContainsKey(productId) ? buyedCounts[productId] : 0))
            {
                reason = 3;
                return false;
            }

            reason = 0;
            return true;
        }

        public static void BindTemplates(IEnumerable<long> productIds, long? topTemplateId, long? bottomTemplateId)
        {
            _productService.BindTemplate(topTemplateId, bottomTemplateId, productIds);

        }

        public static void AddBrowsingProduct(BrowsingHistoryInfo info)
        {
            _productService.AddBrowsingProduct(info);
        }

        /// <summary>
		/// 批量获取商品信息
		/// </summary>
		/// <param name="ids"></param>
		/// <returns></returns>
        public static IQueryable<ProductInfo> GetProductByIds(IEnumerable<long> ids)
        {
            return _productService.GetProductByIds(ids);
        }

        public static IQueryable<BrowsingHistoryInfo> GetBrowsingProducts(long userId)
        {
            return _productService.GetBrowsingProducts(userId);
        }

        #region 阶梯价--张宇枫
        /// <summary>
        /// 根据商品ID获取多个价格
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="IsSelf">是否官方直营店</param>
        /// <param name="discount">会员折扣(0.01-1)</param>
        /// <returns></returns>
        public static List<ProductLadderPrice> GetLadderPriceByProductIds(long productId, bool IsSelf = false, decimal discount = 1m)
        {
            var priceInfo = _productLadderPriceService.GetLadderPricesByProductIds(productId);

            return priceInfo.Select(p =>
            {
                var lprice = p.Price;
                if (IsSelf)
                    lprice = p.Price * discount;
                var price = new ProductLadderPrice
                {
                    Id = p.Id,
                    MinBath = p.MinBath,
                    MaxBath = p.MaxBath,
                    ProductId = p.ProductId,
                    Price = Convert.ToDecimal(lprice.ToString("F2"))
                };
                return price;
            }).ToList();
        }

        /// <summary>
        /// 获取商品销售价格
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public static decimal GetProductLadderPrice(long productId, int quantity)
        {
            var ladderPrices = _productLadderPriceService.GetLadderPricesByProductIds(productId);
            var price = 0m;
            if (ladderPrices.Any())
            {
                price =
                    ladderPrices.Find(i => (quantity <= i.MinBath) || (quantity >= i.MinBath && quantity <= i.MaxBath))
                        .Price;
            }
            return price;
        }

        /// <summary>
        /// 获取阶梯商品最小批量
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public static int GetProductLadderMinMath(long productId)
        {
            var minMath = 0;
            var ladder = GetLadderPriceByProductIds(productId);
            if (ladder.Any())
                minMath = ladder.Min(p => p.MinBath);
            return minMath;
        }

        /// <summary>
        /// 判断购物车提交时，阶梯商品是否达最小批量
        /// </summary>
        /// <param name="cartItemIds"></param>
        /// <returns></returns>
        public static bool IsExistLadderMinMath(string cartItemIds,ref string msg)
        {
            msg = "结算的商品必须满足最小批量才能购买！";
            var result = true;
            var cartItemIdsArr = cartItemIds.Split(',').Where(d => !string.IsNullOrWhiteSpace(d)).Select(t => long.Parse(t));
            var cartItems = CartApplication.GetCartItems(cartItemIdsArr);
            if (cartItems.Any())
            {
                var groupCart = cartItems.Where(item => (!item.ShopBranchId.HasValue || item.ShopBranchId == 0)).ToList().Select(c => {
                    var cItem = new ShoppingCartItem();
                    var skuInfo = _productService.GetSku(c.SkuId);
                    if (skuInfo != null)
                        cItem = c;
                    return cItem;
                }).GroupBy(i => i.ProductId);
                foreach (var cart in cartItems.ToList())
                {
                    var product = GetProduct(cart.ProductId);
                    if (product.IsOpenLadder)
                    {
                        var quantity =
                            groupCart.Where(i => i.Key == cart.ProductId)
                                .ToList()
                                .Sum(cartitem => cartitem.Sum(i => i.Quantity));
                        var minMath = GetProductLadderMinMath(cart.ProductId);
                        if (minMath > 0 && quantity < minMath)
                            result = false;
                    }else
                    {
                        var sku = _productService.GetSku(cart.SkuId);
                        if (cart.Quantity > sku.Stock)
                        {
                            msg = string.Format("商品‘{0}’库存不足,仅剩{1}件", product.ProductName, sku.Stock);
                            return false;
                            //throw new HimallException(string.Format("{0}库存不足", product.ProductName));
                        }
                    }
                }
            }
            return result;
        }

        #endregion

        /// <summary>
        /// 指定地区包邮
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="discount"></param>
        /// <param name="streetId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static bool IsFreeRegion(long productId, decimal discount, int streetId, int count, string skuId)
        {
            return _productService.IsFreeRegion(productId, discount, streetId, count, skuId);
        }
        #endregion

        #region 私有方法
        private static ProductInfo.ProductEditStatus GetEditStatus(ProductInfo.ProductEditStatus status)
        {
            if (status > ProductInfo.ProductEditStatus.EditedAndPending)
                return ProductInfo.ProductEditStatus.CompelPendingHasEdited;
            return ProductInfo.ProductEditStatus.EditedAndPending;
        }
        #endregion

    }
}
