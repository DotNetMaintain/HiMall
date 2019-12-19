using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.DTO;
using Himall.IServices;
using Himall.Core;
using Himall.Application.Mappers;
using Himall.Model;
using Himall.Core.Helper;
using Himall.CommonModel;
using Himall.DTO.Product;
using Himall.DTO.QueryModel;
using System.Web;

namespace Himall.Application
{
    /// <summary>
    /// 门店应用服务
    /// </summary>
    public class ShopBranchApplication
    {
        private static IShopBranchService _shopBranchService = ObjectContainer.Current.Resolve<IShopBranchService>();
        private static IAppMessageService _appMessageService = ObjectContainer.Current.Resolve<IAppMessageService>();
        private static ICouponService _iCouponService = ObjectContainer.Current.Resolve<ICouponService>();

        #region 密码加密处理
        /// <summary>
        /// 二次加盐后的密码
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string GetPasswrodEncode(string password, string salt)
        {
            string encryptedPassword = SecureHelper.MD5(password);//一次MD5加密
            string encryptedWithSaltPassword = SecureHelper.MD5(encryptedPassword + salt);//一次结果加盐后二次加密
            return encryptedWithSaltPassword;
        }
        /// <summary>
        /// 取密码盐
        /// </summary>
        /// <returns></returns>
        public static string GetSalt()
        {
            return Guid.NewGuid().ToString("N").Substring(12);
        }
        #endregion 密码加密处理

        #region 查询相关
        /// <summary>
        /// 获取门店
        /// </summary>
        /// <returns></returns>
        public static ShopBranch GetShopBranchById(long id)
        {
            var branchInfo = _shopBranchService.GetShopBranchById(id);
            if (branchInfo == null)
                return null;
            var branchManagers = _shopBranchService.GetShopBranchManagers(id);
            var shopBranch = AutoMapper.Mapper.Map<ShopBranchInfo, ShopBranch>(branchInfo);
            //补充地址中文名称
            //shopBranch.AddressFullName = RegionApplication.GetFullName(shopBranch.AddressId, CommonConst.ADDRESS_PATH_SPLIT);
            shopBranch.AddressFullName = RenderAddress(shopBranch.AddressPath, shopBranch.AddressDetail, 0);
            if (branchManagers != null && branchManagers.Count() > 0)
            {//补充管理员名称
                shopBranch.UserName = branchManagers.FirstOrDefault().UserName;
            }
            //补充标签
            if (branchInfo.Himall_ShopBranchInTags.Count > 0)
            {
                shopBranch.ShopBranchTagId = string.Join(",", branchInfo.Himall_ShopBranchInTags.Select(x => x.ShopBranchTagId));
            }
            return shopBranch;
        }


        /// <summary>
        /// 根据 IDs批量获取门店信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<ShopBranch> GetShopBranchByIds(IEnumerable<long> ids)
        {
            var branchInfos = _shopBranchService.GetShopBranchByIds(ids);
            var shopBranchs = AutoMapper.Mapper.Map<List<ShopBranch>>(branchInfos);
            return shopBranchs;
        }

        /// <summary>
        /// 根据门店联系方式获取门店信息
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public static ShopBranch GetShopBranchByContact(string contact)
        {
            var branchInfo = _shopBranchService.GetShopBranchByContact(contact);
            return branchInfo.Map<DTO.ShopBranch>();
        }

        /// <summary>
        /// 分页查询门店
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ShopBranch> GetShopBranchs(ShopBranchQuery query)
        {
            IShopService _iShopService = ObjectContainer.Current.Resolve<IShopService>();
            var shopBranchInfos = _shopBranchService.GetShopBranchs(query);
            QueryPageModel<ShopBranch> shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    AddressDetail = e.AddressDetail,
                    AddressFullName = RegionApplication.GetFullName(e.AddressId, CommonConst.ADDRESS_PATH_SPLIT) + CommonConst.ADDRESS_PATH_SPLIT + e.AddressDetail,
                    AddressId = e.AddressId,
                    ContactPhone = e.ContactPhone,
                    ContactUser = e.ContactUser,
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                    ShopId = e.ShopId,
                    Status = e.Status,
                    ShopBranchInTagNames = string.Join(",", e.Himall_ShopBranchInTags.Select(x => x.Himall_ShopBranchTags.Title)),
                    ShopName = _iShopService.GetShop(e.ShopId).ShopName
                }).ToList(),
                Total = shopBranchInfos.Total
            };
            return shopBranchs;
        }
        public static List<ShopBranch> GetShopBranchByShopId(long shopId)
        {
            var shopBranch = _shopBranchService.GetShopBranchByShopId(shopId).ToList();
            return shopBranch.Map<List<ShopBranch>>();
        }
        /// <summary>
        /// 根据分店id获取分店信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<ShopBranch> GetShopBranchs(IEnumerable<long> ids)
        {
            var shopBranchs = _shopBranchService.GetShopBranchs(ids).Map<List<ShopBranch>>();
            //补充地址详细信息,地址库采用了缓存，循环取
            foreach (var b in shopBranchs)
            {
                b.AddressFullName = RegionApplication.GetFullName(b.AddressId);
                b.RegionIdPath = RegionApplication.GetRegionPath(b.AddressId);
            }
            return shopBranchs;
        }
        /// <summary>
        /// 获取分店经营的商品SKU
        /// </summary>
        /// <param name="shopId"></param>
        /// <param name="shopBranchIds"></param>
        /// <returns></returns>
        public static List<ShopBranchSkusInfo> GetSkus(long shopId, IEnumerable<long> shopBranchIds, IEnumerable<string> skuids = null)
        {
            var list = _shopBranchService.GetSkus(shopId, shopBranchIds, skuids: skuids);
            return list.Map<List<ShopBranchSkusInfo>>();
        }
        /// <summary>
        /// 根据SKU AUTOID取门店SKU
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="skuids"></param>
        /// <returns></returns>
        public static List<ShopBranchSkusInfo> GetSkusByIds(long shopBranchId, IEnumerable<string> skuids)
        {
            var list = _shopBranchService.GetSkusByIds(shopBranchId, skuids);
            return list;
        }
        /// <summary>
        /// 根据商品ID取门店sku信息
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public static List<DTO.SKU> GetSkusByProductId(long shopBranchId, long pid)
        {
            var sku = ProductManagerApplication.GetSKU(pid);
            var shopBranchSkus = _shopBranchService.GetSkusByIds(shopBranchId, sku.Select(e => e.Id));
            foreach (var item in sku)
            {
                var branchSku = shopBranchSkus.FirstOrDefault(e => e.SkuId == item.Id);
                if (branchSku != null)
                    item.Stock = branchSku.Stock;
            }
            return sku;
        }
        /// <summary>
        /// 根据ID取门店管理员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ShopBranchManager GetShopBranchManager(long id)
        {
            var managerInfo = _shopBranchService.GetShopBranchManagersById(id);
            AutoMapper.Mapper.CreateMap<ShopBranchManagersInfo, ShopBranchManager>();
            var manager = AutoMapper.Mapper.Map<ShopBranchManagersInfo, ShopBranchManager>(managerInfo);
            //管理员类型为门店管理员
            manager.UserType = ManagerType.ShopBranchManager;
            return manager;
        }

        /// <summary>
        /// 根据门店id获取门店管理员
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <returns></returns>
        public static List<DTO.ShopBranchManager> GetShopBranchManagerByShopBranchId(long shopBranchId)
        {
            return _shopBranchService.GetShopBranchManagers(shopBranchId).Map<List<DTO.ShopBranchManager>>();
        }

        /// <summary>
        /// 门店商品查询
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ProductInfo> GetShopBranchProducts(ShopBranchProductQuery query)
        {
            var pageModel = _shopBranchService.SearchProduct(query);
            //补充门店销售数量
            foreach (var p in pageModel.Models)
            {
                p.SaleCounts = OrderApplication.GetSaleCount(shopBranchId: query.shopBranchId, productId: p.Id);

            }
            return pageModel;
        }

        /// <summary>
        /// 根据日期获取门店商品销量
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ProductInfo> GetShopBranchProductsMonth(ShopBranchProductQuery query, DateTime startDate, DateTime endDate)
        {
            var pageModel = _shopBranchService.SearchProduct(query);
            //TODO:补充门店销售数量
            //因为销量统计不统计当天的，然后在查询数据方法中给EndDate加了一天，因此EndDate减掉一天。
            var orders = OrderApplication.GetOrdersNoPage(new OrderQuery { ShopBranchId = query.shopBranchId, StartDate = startDate, EndDate = endDate.AddDays(-1) });
            var orderItems = OrderApplication.GetOrderItemsByOrderId(orders.Where(d => d.OrderStatus != OrderInfo.OrderOperateStatus.Close).Select(e => e.Id));
            var pids = pageModel.Models.Select(e => e.Id);
            var productSaleCounts = orderItems.Where(e => pids.Contains(e.ProductId)).GroupBy(o => o.ProductId).Select(e => new { productId = e.Key, saleCount = e.Sum(p => p.Quantity - p.ReturnQuantity) });
            foreach (var p in pageModel.Models)
            {
                var productCount = productSaleCounts.FirstOrDefault(e => e.productId == p.Id);
                if (productCount != null)
                    p.SaleCounts = productCount.saleCount;
                else
                    p.SaleCounts = 0;//门店商品无销量则为0，不应用默认的商家商品销量

            }
            return pageModel;
        }

        /// <summary>
        /// 根据日期获取门店产品销售数量
        /// </summary>
        /// <param name="shopBranchId">门店标识</param>
        /// <param name="productId">产品标示</param>
        /// <param name="startDate">启始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns></returns>
        public static long GetProductSaleCount(long shopBranchId, long productId, DateTime startDate, DateTime endDate)
        {
            //因为销量统计不统计当天的，然后在查询数据方法中给EndDate加了一天，因此EndDate减掉一天。
            var orders = OrderApplication.GetOrdersNoPage(new OrderQuery { ShopBranchId = shopBranchId, StartDate = startDate, EndDate = endDate.AddDays(-1) });
            var orderItems = OrderApplication.GetOrderItemsByOrderId(orders.Where(d => d.OrderStatus != OrderInfo.OrderOperateStatus.Close).Select(e => e.Id)).Where(e => e.ProductId == productId);

            if (orderItems.Any())
            {
                return orderItems.Sum(x => x.Quantity - x.ReturnQuantity);
            }
            else
            {
                return 0;//门店商品无销量则为0，不应用默认的商家商品销量
            }
        }

        /// <summary>
        /// 获取周边门店销量
        /// </summary>
        /// <param name="shopBranchId">门店标识</param>
        /// <param name="startDate">启始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns></returns>
        public static long GetShopBranchSaleCount(long shopBranchId, DateTime startDate, DateTime endDate)
        {
            //因为销量统计不统计当天的，然后在查询数据方法中给EndDate加了一天，因此EndDate减掉一天。
            var orders = OrderApplication.GetOrdersNoPage(new OrderQuery { ShopBranchId = shopBranchId, StartDate = startDate, EndDate = endDate.AddDays(-1) });
            var orderItems = OrderApplication.GetOrderItemsByOrderId(orders.Where(d => d.OrderStatus != OrderInfo.OrderOperateStatus.Close).Select(e => e.Id));
            return orderItems.Sum(p => p.Quantity - p.ReturnQuantity);
        }

        public static bool CheckProductIsExist(long shopBranchId, long productId)
        {
            return _shopBranchService.CheckProductIsExist(shopBranchId, productId);
        }

        /// <summary>
        /// 根据查询条件判断是否有门店
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static bool Exists(ShopBranchQuery query)
        {
            return _shopBranchService.Exists(query);
        }

        #endregion

        #region 门店管理
        /// <summary>
        /// 新增门店
        /// </summary>
        public static void AddShopBranch(ShopBranch shopBranch, out long shopBranchId)
        {
            if (isRepeatBranchName(shopBranch.ShopId, shopBranch.Id, shopBranch.ShopBranchName))
            {
                throw new HimallException("门店名称不能重复！");
            }
            var branchManangerInfo = _shopBranchService.GetShopBranchManagersByName(shopBranch.UserName);
            if (branchManangerInfo != null)
            {
                throw new HimallException("门店管理员名称不能重复！");
            }
            if (ManagerApplication.CheckUserNameExist(shopBranch.UserName))
            {
                throw new HimallException("门店管理员名称不能与商家重复！");
            }
            AutoMapper.Mapper.CreateMap<ShopBranch, ShopBranchInfo>();
            var shopBranchInfo = AutoMapper.Mapper.Map<ShopBranch, ShopBranchInfo>(shopBranch);
            shopBranchInfo.AddressPath = RegionApplication.GetRegionPath(shopBranchInfo.AddressId);
            //默认在结尾增加分隔符
            shopBranchInfo.AddressPath = shopBranchInfo.AddressPath + CommonConst.ADDRESS_PATH_SPLIT;
            _shopBranchService.AddShopBranch(shopBranchInfo);
            shopBranchId = shopBranchInfo.Id;
            var salt = GetSalt();
            var shopBranchManagerInfo = new ShopBranchManagersInfo
            {
                CreateDate = DateTime.Now,
                UserName = shopBranch.UserName,
                ShopBranchId = shopBranchInfo.Id,
                PasswordSalt = salt,
                Password = GetPasswrodEncode(shopBranch.PasswordOne, salt)
            };
            _shopBranchService.AddShopBranchManagers(shopBranchManagerInfo);
            shopBranch.Id = shopBranchInfo.Id;
        }
        /// <summary>
        /// 更新门店信息、管理员密码
        /// </summary>
        /// <param name="shopBranch"></param>
        public static void UpdateShopBranch(ShopBranch shopBranch)
        {
            if (isRepeatBranchName(shopBranch.ShopId, shopBranch.Id, shopBranch.ShopBranchName))
            {
                throw new HimallException("门店名称不能重复！");
            }
            AutoMapper.Mapper.CreateMap<ShopBranch, ShopBranchInfo>();
            var shopBranchInfo = AutoMapper.Mapper.Map<ShopBranch, ShopBranchInfo>(shopBranch);

            shopBranchInfo.AddressPath = RegionApplication.GetRegionPath(shopBranchInfo.AddressId);
            //默认在结尾增加分隔符
            shopBranchInfo.AddressPath = shopBranchInfo.AddressPath + CommonConst.ADDRESS_PATH_SPLIT;
            _shopBranchService.UpdateShopBranch(shopBranchInfo);
            if (!string.IsNullOrWhiteSpace(shopBranch.PasswordOne) && !string.IsNullOrWhiteSpace(shopBranch.PasswordTwo))
            {//编辑时可以不输入密码
                var salt = GetSalt();//取salt
                var encodePwd = GetPasswrodEncode(shopBranch.PasswordOne, salt);

                _shopBranchService.UpdateShopBranchManagerPwd(shopBranch.Id, shopBranch.UserName, encodePwd, salt);
            }
        }

        /// <summary>
        /// 更新指定门店管理员的密码
        /// </summary>
        /// <param name="managerId"></param>
        /// <param name="password"></param>
        public static void UpdateShopBranchManagerPwd(long managerId, string password)
        {
            _shopBranchService.UpdateShopBranchManagerPwd(managerId, password);
        }

        /// <summary>
        /// 删除门店
        /// </summary>
        /// <param name="branchId"></param>
        public static void DeleteShopBranch(long branchId)
        {
            //TODO:门店删除逻辑

            _shopBranchService.DeleteShopBranch(branchId);
        }
        /// <summary>
        /// 冻结门店
        /// </summary>
        /// <param name="shopBranchId"></param>
        public static void Freeze(long shopBranchId)
        {
            _shopBranchService.FreezeShopBranch(shopBranchId);
        }
        /// <summary>
        /// 解冻门店
        /// </summary>
        /// <param name="shopBranchId"></param>
        public static void UnFreeze(long shopBranchId)
        {
            _shopBranchService.UnFreezeShopBranch(shopBranchId);
        }
        #endregion 门店管理

        #region 门店登录
        /// <summary>
        /// 门店登录验证
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static ShopBranchManager ShopBranchLogin(string userName, string password)
        {
            var managerInfo = _shopBranchService.GetShopBranchManagersByName(userName);
            if (managerInfo == null)
                return null;

            password = GetPasswrodEncode(password, managerInfo.PasswordSalt);
            if (!string.Equals(password, managerInfo.Password))
                return null;

            AutoMapper.Mapper.CreateMap<ShopBranchManagersInfo, ShopBranchManager>();
            var manager = AutoMapper.Mapper.Map<ShopBranchManagersInfo, ShopBranchManager>(managerInfo);
            return manager;
        }
        #endregion 门店登录

        #region 门店商品管理
        /// <summary>
        /// 添加SKU，并过滤已添加的
        /// </summary>
        /// <param name="pids"></param>
        /// <param name="shopBranchId"></param>
        /// <param name="shopId"></param>
        public static void AddProductSkus(IEnumerable<long> pids, long shopBranchId, long shopId)
        {
            var productsInfo = ProductManagerApplication.GetProductsByIds(pids).Where(e => e.ShopId == shopId);
            if (productsInfo == null)
                throw new HimallException("未找到商品数据");
            if (productsInfo.Any(d => d.IsOpenLadder))
                throw new HimallException("不可添加阶梯架商品");
            //查询已添加的SKU，用于添加时过滤
            var oldskus = _shopBranchService.GetSkus(shopId, new List<long> { shopBranchId }, null).Select(e => e.SkuId);
            var allSkus = SKUApplication.GetByProductIds(productsInfo.Select(p => p.Id));
            var shopBranchSkus = new List<ShopBranchSkusInfo> { };

            var skus = allSkus.Where(s => !oldskus.Any(sku => sku == s.Id)).Select(e => new ShopBranchSkusInfo
            {
                ProductId = e.ProductId,
                SkuId = e.Id,
                ShopId = shopId,
                ShopBranchId = shopBranchId,
                Stock = 0,
                CreateDate = DateTime.Now
            });
            shopBranchSkus.AddRange(skus);

            _shopBranchService.AddSkus(shopBranchSkus);
        }
        /// <summary>
        /// 修正商品sku
        /// <para>0库存添加新的sku</para>
        /// </summary>
        /// <param name="productId"></param>
        public static void CorrectBranchProductSkus(long productId, long shopId)
        {
            var productsInfo = ProductManagerApplication.GetProduct(productId);
            if (productsInfo == null || productsInfo.ShopId != shopId)
            {
                throw new HimallException("未找到商品数据");
            }
            var shopbrids = _shopBranchService.GetAgentShopBranchIds(productId);
            List<long> pids = new List<long>();
            pids.Add(productId);

            foreach (var shopBranchId in shopbrids)
            {
                //查询已添加的SKU，用于添加时过滤
                var oldskus = _shopBranchService.GetSkus(shopId, new List<long> { shopBranchId }, null).Select(e => e.SkuId);
                var allSkus = SKUApplication.GetByProductIds(pids);
                var shopBranchSkus = new List<ShopBranchSkusInfo> { };

                var skus = allSkus.Where(s => !oldskus.Any(sku => sku == s.Id)).Select(e => new ShopBranchSkusInfo
                {
                    ProductId = e.ProductId,
                    SkuId = e.Id,
                    ShopId = shopId,
                    ShopBranchId = shopBranchId,
                    Stock = 0,
                    CreateDate = DateTime.Now
                });
                shopBranchSkus.AddRange(skus);

                _shopBranchService.AddSkus(shopBranchSkus);
            }
        }
        /// <summary>
        /// 设置门店SKU库存
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="skuIds"></param>
        /// <param name="stock"></param>
        /// <param name="opType"></param>
        public static void SetSkuStock(long shopBranchId, IEnumerable<string> skuIds, IEnumerable<int> stock, CommonModel.StockOpType opType)
        {
            switch (opType)
            {
                case StockOpType.Normal:
                    _shopBranchService.SetStock(shopBranchId, skuIds, stock);
                    break;
                case StockOpType.Add:
                    _shopBranchService.AddStock(shopBranchId, skuIds, stock);
                    break;
                case StockOpType.Reduce:
                    _shopBranchService.ReduceStock(shopBranchId, skuIds, stock);
                    break;
            }
        }
        /// <summary>
        /// 修改门店商品库存
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="pids"></param>
        /// <param name="stock"></param>
        /// <param name="opType"></param>
        public static void SetProductStock(long shopBranchId, IEnumerable<long> pids, int stock, CommonModel.StockOpType opType)
        {
            switch (opType)
            {
                case StockOpType.Normal:
                    _shopBranchService.SetProductStock(shopBranchId, pids, stock);
                    break;
                case StockOpType.Add:
                    _shopBranchService.AddProductStock(shopBranchId, pids, stock);
                    break;
                case StockOpType.Reduce:
                    _shopBranchService.ReduceProductStock(shopBranchId, pids, stock);
                    break;
            }
        }

        /// <summary>
        /// 下架商品
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="productIds"></param>
        public static void UnSaleProduct(long shopBranchId, IEnumerable<long> productIds)
        {
            _shopBranchService.SetBranchProductStatus(shopBranchId, productIds, ShopBranchSkuStatus.InStock);
        }
        /// <summary>
        /// 下架所有门店的商品
        /// <para></para>
        /// </summary>
        /// <param name="productId"></param>
        public static void UnSaleProduct(long productId)
        {
            _shopBranchService.SetBranchProductStatus(productId, ShopBranchSkuStatus.InStock);
        }
        /// <summary>
        /// 检测商品是否可以上架
        /// </summary>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public static bool CanOnSaleProduct(IEnumerable<long> productIds)
        {
            bool result = false;
            var products = ProductManagerApplication.GetProductsByIds(productIds);
            if (products.Count() == productIds.Count())
            {
                result = true;
            }
            return result;
        }

        public static bool IsOpenLadderInProducts(IEnumerable<long> productIds)
        {
            bool result = false;
            var products = ProductManagerApplication.GetProductsByIds(productIds);
            if (products.Where(p => p.IsOpenLadder).Count() > 0)
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 上架商品
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="productIds"></param>
        public static void OnSaleProduct(long shopBranchId, IEnumerable<long> productIds)
        {
            _shopBranchService.SetBranchProductStatus(shopBranchId, productIds, ShopBranchSkuStatus.Normal);
        }
        #endregion 门店商品管理

        #region 私有方法
        private static bool isRepeatBranchName(long shopId, long shopBranchId, string branchName)
        {
            var exists = _shopBranchService.Exists(shopId, shopBranchId, branchName);
            return exists;
        }
        #endregion
        /// <summary>
        /// 取门店商品数量
        /// </summary>
        /// <param name="shopBranchId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static IEnumerable<ShopBranchSkusInfo> GetShopBranchProductCount(long shopBranchId, DateTime? startDate, DateTime? endDate)
        {
            var skus = _shopBranchService.SearchShopBranchSkus(shopBranchId, startDate, endDate);
            return skus;
        }

        #region 周边门店
        /// <summary>
        /// 获取周边门店-分页
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static QueryPageModel<ShopBranch> GetNearShopBranchs(ShopBranchQuery query)
        {
            var shopBranchInfos = _shopBranchService.GetNearShopBranchs(query);
            QueryPageModel<ShopBranch> shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    AddressDetail = RenderAddress(e.AddressPath, e.AddressDetail, 1),
                    ContactPhone = e.ContactPhone,
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                    Status = e.Status,
                    DistanceUnit = e.Distance >= 1 ? e.Distance + "KM" : e.Distance * 1000 + "M",
                    Distance = e.Distance,
                    ServeRadius = e.ServeRadius.HasValue ? e.ServeRadius.Value : 0,
                    Latitude = e.Latitude.HasValue ? e.Latitude.Value : 0,
                    Longitude = e.Longitude.HasValue ? e.Longitude.Value : 0,
                    DeliveFee = e.DeliveFee,
                    DeliveTotalFee = e.DeliveTotalFee,
                    IsAboveSelf = e.IsAboveSelf,
                    IsStoreDelive = e.IsStoreDelive,
                    ShopImages = HimallIO.GetRomoteImagePath(e.ShopImages),
                    ShopId = e.ShopId,
                    FreeMailFee = e.FreeMailFee,
                    IsRecommend = (e.IsAboveSelf && e.IsRecommend) || (e.IsStoreDelive && e.Distance <= e.ServeRadius ? e.IsRecommend : false),
                    RecommendSequence = e.RecommendSequence == 0 ? long.MaxValue : e.RecommendSequence//非推荐门店取最大值，便于显示排序
                }).OrderBy(e => e.RecommendSequence).ToList(),
                Total = shopBranchInfos.Total
            };
            return shopBranchs;
        }
        /// <summary>
        /// 搜索周边门店-分页
        /// </summary>
        /// <param name="search"></param>
        /// <returns>关键字包括对门店名称和门店商品的过滤</returns>
        public static QueryPageModel<ShopBranch> SearchNearShopBranchs(ShopBranchQuery search)
        {
            var shopBranchInfos = _shopBranchService.SearchNearShopBranchs(search);
            QueryPageModel<ShopBranch> shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    AddressDetail = RenderAddress(e.AddressPath, e.AddressDetail, 1),
                    ContactPhone = e.ContactPhone,
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                    Status = e.Status,
                    DistanceUnit = e.Distance >= 1 ? e.Distance + "KM" : e.Distance * 1000 + "M",
                    Distance = e.Distance,
                    ServeRadius = e.ServeRadius.HasValue ? e.ServeRadius.Value : 0,
                    Latitude = e.Latitude.HasValue ? e.Latitude.Value : 0,
                    Longitude = e.Longitude.HasValue ? e.Longitude.Value : 0,
                    DeliveFee = e.DeliveFee,
                    DeliveTotalFee = e.DeliveTotalFee,
                    IsAboveSelf = e.IsAboveSelf,
                    IsStoreDelive = e.IsStoreDelive,
                    ShopImages = HimallIO.GetRomoteImagePath(e.ShopImages),
                    ShopId = e.ShopId,
                    FreeMailFee = e.FreeMailFee,
                    IsRecommend = (e.IsAboveSelf && e.IsRecommend) || (e.IsStoreDelive && e.Distance <= e.ServeRadius ? e.IsRecommend : false),
                    RecommendSequence = e.RecommendSequence == 0 ? long.MaxValue : e.RecommendSequence//非推荐门店取最大值，便于显示排序
                }).ToList(),
                Total = shopBranchInfos.Total
            };
            //修正距离，数据库计算出来的结果有点偏差
            foreach (var item in shopBranchs.Models)
            {
                if (item.Latitude > 0 || item.Longitude > 0)
                {
                    item.Distance = _shopBranchService.GetLatLngDistancesFromAPI(search.FromLatLng, item.Latitude + "," + item.Longitude);
                    item.DistanceUnit = item.Distance >= 1 ? item.Distance + "KM" : item.Distance * 1000 + "M";
                }
            }
            return shopBranchs;
        }
        /// <summary>
        /// 搜索周边门店-分页
        /// </summary>
        /// <param name="search"></param>
        /// <returns>标签的过滤</returns>
        public static QueryPageModel<ShopBranch> TagsSearchNearShopBranchs(ShopBranchQuery search)
        {
            var shopBranchInfos = _shopBranchService.TagsSearchNearShopBranchs(search);
            QueryPageModel<ShopBranch> shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    AddressDetail = RenderAddress(e.AddressPath, e.AddressDetail, 1),
                    ContactPhone = e.ContactPhone,
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                    Status = e.Status,
                    DistanceUnit = e.Distance >= 1 ? e.Distance + "KM" : e.Distance * 1000 + "M",
                    Distance = e.Distance,
                    ServeRadius = e.ServeRadius.HasValue ? e.ServeRadius.Value : 0,
                    Latitude = e.Latitude.HasValue ? e.Latitude.Value : 0,
                    Longitude = e.Longitude.HasValue ? e.Longitude.Value : 0,
                    DeliveFee = e.DeliveFee,
                    DeliveTotalFee = e.DeliveTotalFee,
                    IsAboveSelf = e.IsAboveSelf,
                    IsStoreDelive = e.IsStoreDelive,
                    ShopImages = HimallIO.GetRomoteImagePath(e.ShopImages),
                    ShopId = e.ShopId,
                    FreeMailFee = e.FreeMailFee,
                    IsRecommend = e.IsRecommend,
                    RecommendSequence = e.RecommendSequence == 0 ? long.MaxValue : e.RecommendSequence//非推荐门店取最大值，便于显示排序
                }).ToList(),
                Total = shopBranchInfos.Total
            };
            return shopBranchs;
        }
        /// <summary>
        /// 根据商品搜索周边门店-分页
        /// </summary>
        /// <param name="search"></param>
        /// <returns>标签的过滤</returns>
        public static QueryPageModel<ShopBranch> StoreByProductNearShopBranchs(ShopBranchQuery search)
        {
            var shopBranchInfos = _shopBranchService.StoreByProductNearShopBranchs(search);
            QueryPageModel<ShopBranch> shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    AddressDetail = RenderAddress(e.AddressPath, e.AddressDetail, 1),
                    ContactPhone = e.ContactPhone,
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                    Status = e.Status,
                    DistanceUnit = e.Distance >= 1 ? e.Distance + "KM" : e.Distance * 1000 + "M",
                    Distance = e.Distance,
                    ServeRadius = e.ServeRadius.HasValue ? e.ServeRadius.Value : 0,
                    Latitude = e.Latitude.HasValue ? e.Latitude.Value : 0,
                    Longitude = e.Longitude.HasValue ? e.Longitude.Value : 0,
                    DeliveFee = e.DeliveFee,
                    DeliveTotalFee = e.DeliveTotalFee,
                    IsAboveSelf = e.IsAboveSelf,
                    IsStoreDelive = e.IsStoreDelive,
                    ShopImages = HimallIO.GetRomoteImagePath(e.ShopImages),
                    ShopId = e.ShopId,
                    FreeMailFee = e.FreeMailFee,
                    IsRecommend = e.IsRecommend,
                    RecommendSequence = e.RecommendSequence == 0 ? long.MaxValue : e.RecommendSequence//非推荐门店取最大值，便于显示排序
                }).ToList(),
                Total = shopBranchInfos.Total
            };
            return shopBranchs;
        }

        /// <summary>
        /// 组合新地址
        /// </summary>
        /// <param name="addressPath"></param>
        /// <param name="address"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static string RenderAddress(string addressPath, string address, int level)
        {
            if (!string.IsNullOrWhiteSpace(addressPath))
            {
                string fullName = RegionApplication.GetRegionName(addressPath);
                string[] arr = fullName.Split(',');//省，市，区，街道
                if (arr.Length > 0)
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(arr[i]))
                        {
                            address = address.Replace(arr[i], "");//去掉原详细地址中的省市区街道。(为兼容旧门店数据)
                        }
                    }
                }

                if (level <= arr.Length)
                {
                    for (int i = 0; i < level; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(arr[i]))
                        {
                            fullName = fullName.Replace(arr[i], "");
                        }
                    }
                    address = fullName + address;
                }
            }
            if (!string.IsNullOrWhiteSpace(address))
            {
                address = address.Replace(",", "");
            }
            return address;
        }
        public static QueryPageModel<ShopBranch> GetShopBranchsAll(ShopBranchQuery query)
        {
            var shopBranchInfos = _shopBranchService.GetShopBranchsAll(query);
            QueryPageModel<ShopBranch> shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    AddressDetail = e.AddressDetail,
                    ContactPhone = e.ContactPhone,
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                    Status = e.Status,
                    DistanceUnit = e.Distance >= 1 ? e.Distance + "KM" : e.Distance * 1000 + "M",
                    Distance = e.Distance,
                    ServeRadius = TypeHelper.ObjectToInt(e.ServeRadius),
                    Latitude = e.Latitude.HasValue ? e.Latitude.Value : 0,
                    Longitude = e.Longitude.HasValue ? e.Longitude.Value : 0,
                    AddressPath = e.AddressPath,
                    ContactUser = e.ContactUser
                }).ToList()
            };
            return shopBranchs;
        }

        /// <summary>
        /// 获取门店-不分页
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public static List<ShopBranch> GetShopBranchsNoPager(ShopBranchQuery query)
        {
            var shopBranchInfos = _shopBranchService.GetShopBranchsNoPager(query);
            return shopBranchInfos.Select(n => new ShopBranch
            {
                Id = n.Id,
                ShopBranchName = n.ShopBranchName,
                AddressDetail = GetShopBranchsFullAddress(n.AddressPath) + n.AddressDetail,
                RecommendSequence = n.RecommendSequence
            }).OrderBy(n => n.RecommendSequence).ToList();
        }

        public static string GetShopBranchsFullAddress(string addressPath)
        {
            var str = string.Empty;
            if (!string.IsNullOrEmpty(addressPath))
            {
                str = RegionApplication.GetRegionName(addressPath);
            }
            return str.Replace(",", "");
        }

        /// <summary>
        /// 推荐门店
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static bool RecommendShopBranch(long[] ids)
        {
            var flag = _shopBranchService.RecommendShopBranch(ids);
            return flag;
        }

        /// <summary>
        /// 推荐门店排序
        /// </summary>
        /// <param name="oriShopBranchId">门店ID</param>
        /// <param name="newShopBranchId">门店ID</param>
        /// <returns></returns>
        public static bool RecommendChangeSequence(long oriShopBranchId, long newShopBranchId)
        {
            var flag = _shopBranchService.RecommendChangeSequence(oriShopBranchId, newShopBranchId);
            return flag;
        }

        /// <summary>
        /// 取消推荐门店
        /// </summary>
        /// <param name="shopBranchId">门店ID</param>
        /// <returns></returns>
        public static bool ResetShopBranchRecommend(long shopBranchId)
        {
            var flag = _shopBranchService.ResetShopBranchRecommend(shopBranchId);
            return flag;
        }

        #endregion
        #region 商家手动分配门店
        /// <summary>
        /// 获取商家下该区域范围内的可选门店
        /// </summary>
        /// <param name="areaId"></param>
        /// <param name="shopId"></param>
        /// <returns></returns>
        public static QueryPageModel<ShopBranch> GetArealShopBranchsAll(int areaId, int shopId, string latAndLng)
        {
            float latitude = 0; float longitude = 0;
            var arrLatAndLng = HttpUtility.UrlDecode(latAndLng).Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (arrLatAndLng.Length == 2)
            {
                float.TryParse(arrLatAndLng[0], out latitude);
                float.TryParse(arrLatAndLng[1], out longitude);
            }
            var shopBranchInfos = _shopBranchService.GetArealShopBranchsAll(areaId, shopId, latitude, longitude);
            QueryPageModel<ShopBranch> shopBranchs = new QueryPageModel<ShopBranch>
            {
                Models = shopBranchInfos.Models.Select(e => new ShopBranch
                {
                    Id = e.Id,
                    ShopBranchName = e.ShopBranchName,
                }).ToList()
            };
            return shopBranchs;
        }
        #endregion

        #region 门店标签相关
        /// <summary>
        /// 查询所有标签
        /// </summary>
        /// <returns></returns>
        public static List<ShopBranchTagModel> GetAllShopBranchTagInfos()
        {
            var shopBranchTagInfos = _shopBranchService.GetAllShopBranchTagInfo();
            List<ShopBranchTagModel> shopBranchTagModels = shopBranchTagInfos.Select(e => new ShopBranchTagModel
            {
                Id = e.Id,
                Title = e.Title,
                ShopBranchCount = e.Himall_ShopBranchInTags.Count()
            }).ToList();

            return shopBranchTagModels;
        }
        /// <summary>
        /// 根据ID查询标签
        /// </summary>
        /// <param name="Id">标签标识</param>
        /// <returns></returns>
        public static ShopBranchTagModel GetShopBranchTagInfo(long Id)
        {
            var shopBranchTagInfos = _shopBranchService.GetAllShopBranchTagInfo();
            var tag = shopBranchTagInfos.FirstOrDefault(n => n.Id == Id);
            if (null != tag) return new ShopBranchTagModel { Id = tag.Id, Title = tag.Title, ShopBranchCount = tag.Himall_ShopBranchInTags.Count };
            return null;
        }
        /// <summary>
        /// 增加标签
        /// </summary>
        /// <param name="shopBranchTagInfo"></param>
        public static void AddShopBranchTagInfo(string title)
        {
            if (string.IsNullOrEmpty(title)) throw new Exception("标签名称不可为空");
            ShopBranchTagInfo shopBranchTagInfo = new ShopBranchTagInfo();
            shopBranchTagInfo.Title = title;
            _shopBranchService.AddShopBranchTagInfo(shopBranchTagInfo);
        }
        /// <summary>
        /// 修改标签名称
        /// </summary>
        /// <param name="shopBranchTagInfo"></param>
        /// <returns></returns>
        public static bool UpdateShopBranchTagInfo(long Id, string title)
        {
            if (Id <= 0) throw new Exception("修改目标标签不可为空");
            if (string.IsNullOrEmpty(title)) throw new Exception("标签名称不可为空");

            ShopBranchTagInfo shopBranchTagInfo = new ShopBranchTagInfo();
            shopBranchTagInfo.Id = Id;
            shopBranchTagInfo.Title = title;
            return _shopBranchService.UpdateShopBranchTagInfo(shopBranchTagInfo);
        }
        /// <summary>
        /// 删除标签
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static bool DeleteShopBranchTagInfo(long Id)
        {
            return _shopBranchService.DeleteShopBranchTagInfo(Id);
        }
        /// <summary>
        /// 批量设置门店标签
        /// </summary>
        /// <param name="shopBranchIds"></param>
        /// <param name="shopBranchTagIds"></param>
        public static void SetShopBrandTagInfos(long[] shopBranchIds, long[] shopBranchTagIds)
        {
            //清空原有标签
            _shopBranchService.DeleteShopBranchInTagInfo(shopBranchIds);
            //添加新标签
            _shopBranchService.AddShopBranchInTagInfo(shopBranchIds, shopBranchTagIds);
        }

        #endregion
        /// <summary>
        /// 获取两点间距离
        /// </summary>
        /// <param name="fromLatLng"></param>
        /// <param name="latlng"></param>
        /// <returns></returns>
        public static double GetLatLngDistances(string fromLatLng, string latlng)
        {
            return _shopBranchService.GetLatLngDistancesFromAPI(fromLatLng, latlng);
        }

        /// <summary>
        /// 判断当前用户是否领取优惠卷
        /// </summary>
        /// <param name="couponinfo"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static int CouponIsUse(CouponInfo couponinfo, long userId)
        {
            var status = 0;
            CouponRecordQuery crQuery = new CouponRecordQuery();
            if (userId > 0)
            {//检验当前会员是否可领
                crQuery.CouponId = couponinfo.Id;
                crQuery.UserId = userId;
                QueryPageModel<CouponRecordInfo> pageModel = _iCouponService.GetCouponRecordList(crQuery);
                if (couponinfo.PerMax != 0 && pageModel.Total >= couponinfo.PerMax)
                {
                    //达到个人领取最大张数
                    status = 1;
                }
            }
            if (status == 0)
            {//检验优惠券本身是否可领
                crQuery = new CouponRecordQuery()
                {
                    CouponId = couponinfo.Id
                };
                QueryPageModel<CouponRecordInfo> pageModel = _iCouponService.GetCouponRecordList(crQuery);
                if (pageModel.Total >= couponinfo.Num)
                {
                    //达到领取最大张数
                    status = 2;
                }
            }
            return status;
        }

        public static ShopStoreServiceMark GetServiceMark(long Id)
        {
            return _shopBranchService.GetServiceMark(Id);
        }
        /// <summary>
        /// 取用户在门店的购物车数量
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="branchIds"></param>
        /// <returns></returns>
        public static Dictionary<long, int> GetShopBranchCartItemCount(long userId, IEnumerable<long> branchIds)
        {
            Dictionary<long, int> result = new Dictionary<long, int>();
            if (userId == 0)
            {
                return result;
            }
            ShoppingCartInfo memberCartInfo = new ShoppingCartInfo();
            List<DTO.Product.Product> onSaleProducts = new List<DTO.Product.Product>();
            if (userId > 0)
            {//如果已登陆取购物车数据
                memberCartInfo = CartApplication.GetShopBranchCart(userId);
                if (memberCartInfo != null)
                {
                    onSaleProducts = ProductManagerApplication.GetAllStatusProductByIds(memberCartInfo.Items.Select(e => e.ProductId)).ToList();
                }
            }

            Dictionary<long, int> buyedCounts = null;
            if (userId > 0)
            {
                buyedCounts = new Dictionary<long, int>();
                buyedCounts = OrderApplication.GetProductBuyCount(userId, memberCartInfo.Items.Select(x => x.ProductId));
            }
            var shopBranchSkuList = _shopBranchService.GetSkusByBranchIds(branchIds, skuids: memberCartInfo.Items.Select(s => s.SkuId));

            foreach (var id in branchIds)
            {
                //var cartQuantity = memberCartInfo.Items.Where(c => c.ShopBranchId.HasValue && c.ShopBranchId.Value == query.shopBranchId).Sum(c => c.Quantity);
                //过滤购物车 无效商品
                var cartQuantity = memberCartInfo.Items.Where(c => c.ShopBranchId.HasValue && c.ShopBranchId.Value == id).Select(item =>
                {
                    var product = onSaleProducts.FirstOrDefault(p => p.Id == item.ProductId);
                    var shopbranchsku = shopBranchSkuList.FirstOrDefault(x => x.ShopBranchId == id && x.SkuId == item.SkuId);
                    long stock = shopbranchsku == null ? 0 : shopbranchsku.Stock;

                    if (stock > product.MaxBuyCount && product.MaxBuyCount != 0)
                        stock = product.MaxBuyCount;
                    if (product.MaxBuyCount > 0 && buyedCounts != null && buyedCounts.ContainsKey(item.ProductId))
                    {
                        int buynum = buyedCounts[item.ProductId];
                        stock = stock - buynum;
                    }
                    var status = product.IsOpenLadder ? 1 : (shopbranchsku == null ? 1 : (shopbranchsku.Status == ShopBranchSkuStatus.Normal) ? (item.Quantity > stock ? 2 : 0) : 1);//0:正常；1：冻结；2：库存不足
                    if (status == 0)
                    {
                        return item.Quantity;
                    }
                    else
                    {
                        return 0;
                    }
                }).Sum(count => count);
                if (cartQuantity > 0)
                    result.Add(id, cartQuantity);
            }
            return result;

        }
    }
}
