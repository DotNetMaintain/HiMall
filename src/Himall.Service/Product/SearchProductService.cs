using Himall.CommonModel;
using Himall.Core;
using Himall.DTO.QueryModel;
using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using MySql.Data.MySqlClient;
using NetRube.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Himall.Service
{
    public class SearchProductService : ServiceBase, ISearchProductService
    {

        public void AddSearchProduct(long productId)
        {
            var sql = new Sql();
            sql.Append("INSERT INTO `himall_searchproducts` (`ProductId`, `ProductName`, `ShopId`, `ShopName`, `BrandId`, `BrandName`, `FirstCateId`, `FirstCateName`, `SecondCateId`, `SecondCateName`, `ThirdCateId`, `ThirdCateName`, `AttrValues`, `Comments`, `SaleCount`, `SalePrice`, `OnSaleTime`, `ImagePath`, `CanSearch`, `BrandLogo`) ");
            sql.Append("select a.Id,a.ProductName,a.ShopId,b.ShopName,a.BrandId,c.`Name` as BrandName,SUBSTRING_INDEX(a.CategoryPath, '|', 1) AS FirstCateId, d.Name as FirstCateName,SUBSTRING_INDEX(SUBSTRING_INDEX(CategoryPath, '|', 2), '|', -1) AS SecondCateId, e.Name as SecondCateName,SUBSTRING_INDEX(CategoryPath, '|', -1) AS ThirdCateId, f.Name as ThirdCateName,g.AttrValues,0,0,a.MinSalePrice,a.AddedDate,a.ImagePath,(case when a.SaleStatus=1 and a.AuditStatus=2 and a.IsDeleted=0 then 1 else 0 end) as CanSearch,c.Logo from himall_products a ");
            sql.Append("left join himall_shops b on a.ShopId = b.Id ");
            sql.Append("left join himall_brands c on a.BrandId = c.Id ");
            sql.Append("left join himall_categories d on SUBSTRING_INDEX(a.CategoryPath, '|', 1) = d.Id ");
            sql.Append("left join himall_categories e on SUBSTRING_INDEX(SUBSTRING_INDEX(a.CategoryPath, '|', 2), '|', -1) = e.Id ");
            sql.Append("left join himall_categories f on SUBSTRING_INDEX(a.CategoryPath,'|', -1) = f.Id ");
            sql.Append("left join (select ProductId, group_concat(ValueId) as AttrValues from himall_productattributes group by productId) g on a.Id = g.ProductId ");
            sql.Append("where a.Id=@0", productId);

            DbFactory.Default.Execute(sql);//, new { ProductId = productId });
        }

        public void UpdateSearchProduct(long productId)
        {
            var sql = new Sql();
            sql.Append("update himall_searchproducts a ");
            sql.Append("left join himall_products b on a.ProductId = b.Id ");
            sql.Append("left join himall_categories c on SUBSTRING_INDEX(b.CategoryPath, '|', 1) = c.Id ");
            sql.Append("left join himall_categories d on SUBSTRING_INDEX(SUBSTRING_INDEX(b.CategoryPath, '|', 2), '|', -1) = d.Id ");
            sql.Append("left join himall_categories e on SUBSTRING_INDEX(b.CategoryPath, '|', -1) = e.Id ");
            sql.Append("left join (select ProductId, group_concat(ValueId) as AttrValues from himall_productattributes group by productId) f on a.ProductId = f.ProductId ");
            sql.Append("left join himall_shops g on b.ShopId = g.Id ");
            sql.Append("left join himall_brands h on b.BrandId = h.Id ");
            sql.Append("set a.ProductName = b.ProductName,a.ShopName = g.ShopName,a.BrandId=h.Id,a.BrandName = h.`Name`,a.BrandLogo = h.Logo,a.FirstCateId = c.Id,a.FirstCateName = c.`Name`, ");
            sql.Append("a.SecondCateId = d.Id,a.SecondCateName = d.`Name`,a.ThirdCateId = e.Id,a.ThirdCateName = e.`Name`,a.AttrValues = f.AttrValues,a.SalePrice = b.MinSalePrice,a.ImagePath = b.ImagePath, ");
            sql.Append("a.CanSearch = (case when b.SaleStatus =1 and b.AuditStatus = 2 and b.IsDeleted=0 then 1 else 0 end) ");
            sql.Append("where a.ProductId=@0 ", productId);

            DbFactory.Default.Execute(sql);//, new { ProductId = productId });
        }

        public void UpdateShop(long shopId, string shopName)
        {
            var sql = new Sql("update himall_searchProducts set ShopName=@0 where ShopId=@1", shopName, shopId);

            DbFactory.Default.Execute(sql);//, new { ShopName = shopName, ShopId = shopId });
        }

        public void UpdateSearchStatusByProduct(long productId)
        {
            var sql = new Sql("update himall_searchproducts a left join himall_products b on a.productid=b.id set a.cansearch=(case when b.SaleStatus=1 and b.AuditStatus=2 and b.IsDeleted=0 then 1 else 0 end) where a.productid=@0", productId);

            DbFactory.Default.Execute(sql);//, new { ProductId = productId });
        }

        public void UpdateSearchStatusByProducts(List<long> productIds)
        {
            var sql = new Sql("update himall_searchproducts a left join himall_products b on a.productid=b.id set a.cansearch=(case when b.SaleStatus=1 and b.AuditStatus=2 and b.IsDeleted=0 then 1 else 0 end) where a.productid in (@0)", productIds);

            DbFactory.Default.Execute(sql);//, new { ProductId = productIds.ToArray() });
        }
        public void UpdateSearchStatusByShop(long shopId)
        {
            var sql = new Sql("update himall_searchproducts a left join himall_products b on a.productid = b.id set CanSearch = 0 where b.shopid = @0 and cansearch=1; ", shopId);
            sql.Append(" update himall_searchproducts a left join himall_products b on a.productid = b.id set CanSearch = 1 where a.cansearch=0 and a.shopid = @0 and b.AuditStatus = 2 and b.SaleStatus = 1 and b.IsDeleted = 0; ", shopId);
            DbFactory.Default.Execute(sql);
            //cmd.Parameters.AddWithValue("@id", shopId);
            //cmd.ExecuteNonQuery();
        }

        public void UpdateBrand(BrandInfo brand)
        {
            var sql = new Sql("update himall_searchProducts set BrandName=@0,BrandLogo=@1 where BrandId=@2", brand.Name, brand.Logo, brand.Id);

            DbFactory.Default.Execute(sql);//, new { BrandName = brand.Name, BrandLogo = brand.Logo, BrandId = brand.Id });

        }
        public void UpdateCategory(CategoryInfo category)
        {
            var sql = "update himall_searchProducts set {0}CateName=@0 where {0}CateId=@1";
            switch (category.Depth)
            {
                case 1:
                    sql = string.Format(sql, "First");
                    break;
                case 2:
                    sql = string.Format(sql, "Second");
                    break;
                case 3:
                    sql = string.Format(sql, "Third");
                    break;
            }
            DbFactory.Default.Execute(new Sql(sql, category.Name, category.Id));//, new { CateName = category.Name, CateId = category.Id });
        }

        /// <summary>
        /// 商品搜索
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public SearchProductResult SearchProduct(SearchProductQuery query)
        {
            SearchProductResult result = new SearchProductResult();
            string countsql = "select count(1) from himall_searchproducts ";
            string sql = "select ProductId,ProductName,SalePrice,ImagePath,ShopId,ShopName,SaleCount,ThirdCateId,Comments from himall_searchproducts ";
            var where = new Sql();
            GetSearchWhere(query, where);
            var order = new Sql();
            GetSearchOrder(query, order);
            string index = GetForceIndex(query);
            string page = GetSearchPage(query);

            result.Data = DbFactory.Default.Query<ProductView>(string.Concat(sql, index, where.SQL, order.SQL, page), where.Arguments).ToList();
            result.Total = DbFactory.Default.ExecuteScalar<int>(string.Concat(countsql, where.SQL), where.Arguments);

            return result;
        }

        #region 旧方法 SearchProductFilter
        /*
        public SearchProductFilterResult SearchProductFilter(SearchProductQuery query)
        {

            try
            {
                SearchProductFilterResult result = new SearchProductFilterResult();
                DynamicParameters parms = new DynamicParameters();
                string sql = "select DISTINCT Id,FirstCateId,FirstCateName,SecondCateId,SecondCateName,ThirdCateId,ThirdCateName,BrandId,BrandName,BrandLogo,AttrValues from himall_searchproducts  ps ";
                string where = GetSearchWhere(query, parms);
                string order = GetSearchOrder(query);
                string index = GetForceIndex(query);
                string page = string.Empty;
                string AttrValueIds = string.Empty;
                bool hasAttrCache = false;
                List<dynamic> data = null;
                List<AttributeInfo> listAttr = new List<AttributeInfo>();
                List<AttributeValueInfo> listAttrVal = new List<AttributeValueInfo>();
                if (Cache.Exists(CacheKeyCollection.CACHE_ATTRIBUTE_LIST) && Cache.Exists(CacheKeyCollection.CACHE_ATTRIBUTEVALUE_LIST))
                {
                    hasAttrCache = true;
                    listAttr = Cache.Get<List<AttributeInfo>>(CacheKeyCollection.CACHE_ATTRIBUTE_LIST);
                    listAttrVal = Cache.Get<List<AttributeValueInfo>>(CacheKeyCollection.CACHE_ATTRIBUTEVALUE_LIST);
                }

                using (MySqlConnection conn = new MySqlConnection(Connection.ConnectionString))
                {
                    data = conn.Query(string.Concat(sql, index, where, order, page), parms).ToList();
                    foreach (dynamic o in data)
                        AttrValueIds += o.AttrValues + ",";

                    if (!hasAttrCache)
                    {
                        sql = "SELECT * FROM HiMall_Attributes";
                        listAttr = conn.Query<AttributeInfo>(sql).ToList();
                        sql = "SELECT * FROM himall_attributevalues";
                        listAttrVal = conn.Query<AttributeValueInfo>(sql).ToList();
                    }
                }
                if (!hasAttrCache)
                {
                    Cache.Insert<List<AttributeInfo>>(CacheKeyCollection.CACHE_ATTRIBUTE_LIST, listAttr);
                    Cache.Insert<List<AttributeValueInfo>>(CacheKeyCollection.CACHE_ATTRIBUTEVALUE_LIST, listAttrVal);
                }

                List<string> ValueIds = AttrValueIds.TrimEnd(',').Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                listAttrVal = listAttrVal.Where(r => ValueIds.Contains(r.Id.ToString())).ToList();
                listAttr = listAttr.Where(r => listAttrVal.Select(z => z.AttributeId).Contains(r.Id)).ToList();

                result.Attribute = listAttr.Select(r => new AttributeView()
                {
                    AttrId = r.Id,
                    Name = r.Name,
                    AttrValues = listAttrVal.Where(z => z.AttributeId == r.Id).Select(s =>
                                  new AttributeValue()
                                  {
                                      Id = s.Id,
                                      Name = s.Value
                                  }).ToList()
                }).ToList();

                result.Brand = data.Where((x, i) => data.FindIndex(z => z.BrandId == x.BrandId && z.BrandId > 0) == i)
                                   .Select(p => new BrandView() { Id = p.BrandId, Name = p.BrandName, Logo = p.BrandLogo }).ToList();

                result.Category = data.Where((x, i) => data.FindIndex(z => z.FirstCateId == x.FirstCateId) == i) //去重
                                      .Select(f => new CategoryView()
                                      {
                                          Id = f.FirstCateId,
                                          Name = f.FirstCateName,
                                          SubCategory = data.Where((x, i) => data.FindIndex(z => z.SecondCateId == x.SecondCateId) == i) //二级去重
                                                            .Where(r => r.FirstCateId == f.FirstCateId) //查找指定一级分类的下级
                                                            .Select(s => new CategoryView()
                                                            {
                                                                Id = s.SecondCateId,
                                                                Name = s.SecondCateName,
                                                                SubCategory = data.Where((x, i) => data.FindIndex(z => z.ThirdCateId == x.ThirdCateId) == i) //三级去重
                                                                                  .Where(r => r.SecondCateId == s.SecondCateId) //查找指定二级分类的下级
                                                                                  .Select(t => new CategoryView()
                                                                                  {
                                                                                      Id = t.ThirdCateId,
                                                                                      Name = t.ThirdCateName
                                                                                  }).ToList()
                                                            }).ToList()
                                      }).ToList();
                return result;
            }
            catch (Exception ex)
            {

                Log.Error("搜索不出来了：", ex);
                return new SearchProductFilterResult();
            }
        }
        */
        #endregion

        /// <summary>
        /// 商品属性、分类、品牌搜索
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public SearchProductFilterResult SearchProductFilter(SearchProductQuery query)
        {
            try
            {
                var result = new SearchProductFilterResult();
                var where = new Sql();
                GetSearchWhere(query, where);
                //将参数转为MySql参数
                //var @params = temp.ParameterNames.Select(name => new MySqlParameter("@" + name, temp.Get<object>(name))).ToArray();

                //取出缓存中的属性数据
                var listAttr = Cache.Get<List<AttributeInfo>>(CacheKeyCollection.CACHE_ATTRIBUTE_LIST);
                var listAttrVal = Cache.Get<List<AttributeValueInfo>>(CacheKeyCollection.CACHE_ATTRIBUTEVALUE_LIST);

                //如果没有则从数据库查询半缓存，缓存时间为默认时间
                if (listAttr == null)
                {
                    var sql = "SELECT * FROM HiMall_Attributes";
                    listAttr = Context.Database.SqlQuery<AttributeInfo>(sql).ToList();
                    Cache.Insert(CacheKeyCollection.CACHE_ATTRIBUTE_LIST, listAttr);
                }

                //如果没有则从数据库查询半缓存，缓存时间为默认时间
                if (listAttrVal == null)
                {
                    var sql = "SELECT * FROM himall_attributevalues";
                    listAttrVal = Context.Database.SqlQuery<AttributeValueInfo>(sql).ToList();
                    Cache.Insert(CacheKeyCollection.CACHE_ATTRIBUTEVALUE_LIST, listAttrVal);
                }

                //查出符合条件的属性值
                string attrSql = "select AttrValues from himall_searchproducts " + where.SQL;
                var sttrValueIds = DbFactory.Default.Query<string>(attrSql, where.Arguments).ToList();
                //按','拆分
                var valueIds = sttrValueIds.Where(i => !string.IsNullOrWhiteSpace(i)).SelectMany(item => item.Split(',')).ToList();
                //过滤符合结果的属性值
                listAttrVal = listAttrVal.Where(r => valueIds.Contains(r.Id.ToString())).ToList();
                listAttr = listAttr.Where(r => listAttrVal.Select(z => z.AttributeId).Contains(r.Id)).ToList();

                result.Attribute = listAttr.Select(r => new AttributeView()
                {
                    AttrId = r.Id,
                    Name = r.Name,
                    AttrValues = listAttrVal.Where(z => z.AttributeId == r.Id).Select(s =>
                    new AttributeValue()
                    {
                        Id = s.Id,
                        Name = s.Value
                    }).ToList()
                }).ToList();

                //查询符合条件的品牌
                var brandSql = "select DISTINCT BrandId Id,BrandName Name,BrandLogo Logo from himall_searchproducts " + where.SQL + " and BrandId is not null AND BrandId<>0";
                result.Brand = DbFactory.Default.Query<BrandView>(brandSql, where.Arguments).ToList();

                //查询符合条件的分类
                var categorySql = "select FirstCateId,MAX(FirstCateName) FirstCateName,SecondCateId,MAX(SecondCateName) SecondCateName,ThirdCateId,MAX(ThirdCateName) ThirdCateName from himall_searchproducts " + where.SQL + " group by  FirstCateId , SecondCateId , ThirdCateId ";
                var data = DbFactory.Default.Query<CategorySeachModel>(categorySql, where.Arguments).ToList();
                result.Category = data.GroupBy(item => item.FirstCateId).Select(first => new CategoryView//根据一级分类分组
                {
                    Id = first.Key,
                    Name = first.Select(item => item.FirstCateName).FirstOrDefault(),
                    SubCategory = first.GroupBy(item => item.SecondCateId).Select(second => new CategoryView//根据二级分类分组
                    {
                        Id = second.Key,
                        Name = second.Select(item => item.SecondCateName).FirstOrDefault(),
                        SubCategory = second.GroupBy(item => item.ThirdCateId).Select(three => new CategoryView//根据三级分类分组
                        {
                            Id = three.Key,
                            Name = three.Select(item => item.ThirdCateName).FirstOrDefault()
                        }).ToList()
                    }).ToList()
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {

                Log.Error("搜索不出来了：", ex);
                return new SearchProductFilterResult();
            }
        }
        #region 组装sql
        /// <summary>
        /// 获取搜索过滤sql
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        private void GetSearchWhere(SearchProductQuery query, Sql where)
        {
            where.Append("WHERE CanSearch=1 ");
            #region 过滤条件
            if (query.ShopId != 0)
            {
                where.Append("AND ShopId=@0 ", query.ShopId);
                //parms.Add("@ShopId", query.ShopId);
            }

            if (query.VShopId != 0)
            {
                where.Append(" AND ShopId IN (SELECT ShopId FROM himall_vshop where Id=@0) ", query.VShopId);
                //parms.Add("@VShopId", query.VShopId);
            }

            if ((query.VShopId != 0 || query.ShopId != 0) && query.ShopCategoryId != 0)
            {
                where.Append(" AND ProductId IN (select ProductId from himall_productshopcategories where ShopCategoryId in(select id from Himall_ShopCategories where ShopId = @0 and(id = @1 or ParentCategoryId = @1))) ", query.ShopId, query.ShopCategoryId);
                //parms.Add("@ShopCategoryId", query.ShopCategoryId);

            }

            if (query.BrandId != 0)
            {
                where.Append("AND BrandId=@0 ", query.BrandId);
                //parms.Add("@BrandId", query.BrandId);

            }

            if (query.FirstCateId != 0)
            {
                where.Append("AND FirstCateId=@0 ", query.FirstCateId);
                //parms.Add("@FirstCateId", query.FirstCateId);

            }
            else if (query.SecondCateId != 0)
            {
                where.Append("AND SecondCateId=@0 ", query.SecondCateId);
                //parms.Add("@SecondCateId", query.SecondCateId);

            }
            else if (query.ThirdCateId != 0)
            {
                where.Append("AND ThirdCateId=@0 ", query.ThirdCateId);
                //parms.Add("@ThirdCateId", query.ThirdCateId);

            }

            if (query.StartPrice >= 0)
            {
                where.Append(" AND SalePrice>=@0 ", query.StartPrice);
                //parms.Add("@StartPrice", query.StartPrice);

            }

            if (query.EndPrice > 0 && query.EndPrice >= query.StartPrice)
            {
                where.Append(" AND SalePrice <= @0 ", query.EndPrice);
                //parms.Add("@EndPrice", query.EndPrice);

            }

            if (query.AttrValIds.Count > 0)
            {
                where.Append("  AND ProductId IN (SELECT DISTINCT ProductId FROM Himall_ProductAttributes ");
                //此处属性筛选，要取交集非并集
                foreach (var item in query.AttrValIds)
                {
                    where.Append(" INNER JOIN (SELECT DISTINCT ProductId FROM Himall_ProductAttributes WHERE  (ValueId = "+item+" ) ) t"+query.AttrValIds.IndexOf(item)+" USING (ProductId) ");
                }
                where.Append(")");
                //where.Append(" AND ProductId IN (SELECT DISTINCT ProductId FROM Himall_ProductAttributes WHERE ValueId IN (@0)) ", query.AttrValIds);
                //parms.Add("@ValueIds", query.AttrValIds.ToArray());

            }

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                if (!query.IsLikeSearch)
                {
                    where.Append("AND MATCH(ProductName) AGAINST(@0 IN BOOLEAN MODE) ", string.Concat(query.Keyword.TrimEnd(' '), "*").Replace(" ", "*"));
                    //parms.Add("@ProductName", string.Concat(query.Keyword, "*").Replace(" ", "*"));

                }
                else
                {
                    where.Append("AND ProductName like @0 ", "%" + query.Keyword + "%");
                    //parms.Add("@ProductName", "%" + query.Keyword + "%");

                }
            }
            #endregion
        }
        /// <summary>
        /// 获取搜索排序sql
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private void GetSearchOrder(SearchProductQuery query, Sql order)
        {
            switch (query.OrderKey)
            {
                case 2:
                    order.Append(" ORDER BY SaleCount ");
                    break;
                case 3:
                    order.Append(" ORDER BY SalePrice ");
                    break;
                case 4:
                    order.Append(" ORDER BY Comments ");
                    break;
                case 5:
                    order.Append(" ORDER BY OnSaleTime ");
                    break;
                default:
                    order.Append(" ORDER BY Id ");
                    break;
            }
            if (!query.OrderType)
                order.Append(" DESC ");
            else
                order.Append(" ASC ");
        }

        /// <summary>
        /// 非主键排序时强制使用索引
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private string GetForceIndex(SearchProductQuery query)
        {
            if (!string.IsNullOrEmpty(query.Keyword))
                return string.Empty;

            string index = string.Empty;
            switch (query.OrderKey)
            {
                case 2:
                    index = " FORCE INDEX(IX_SaleCount) ";
                    break;
                case 3:
                    index = " FORCE INDEX(IX_SalePrice)  ";
                    break;
                case 4:
                    index = " FORCE INDEX(IX_Comments) ";
                    break;
                case 5:
                    index = " FORCE INDEX(IX_OnSaleTime) ";
                    break;
            }

            return index;
        }
        /// <summary>
        /// 获取搜索商品分页sql
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private string GetSearchPage(SearchProductQuery query)
        {
            return string.Format(" LIMIT {0},{1} ", (query.PageNumber - 1) * query.PageSize, query.PageSize);
        }
        #endregion

        #region 小程序商品查询

        /// <summary>
        /// 商品搜索
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public SearchProductResult SearchAppletProduct(SearchProductQuery query)
        {
            SearchProductResult result = new SearchProductResult();
            string countsql = "select count(1) from himall_searchproducts ps ";
            string sql = "select ps.ProductId,ps.ProductName,ps.SalePrice,ps.ImagePath,ps.ShopId,ps.ShopName,ps.SaleCount,ps.ThirdCateId,ps.Comments,pt.HasSKU,pt.MinSalePrice,(select id from Himall_SKUs where ProductId=ps.ProductId order by id desc limit 1) as SkuId,IFNULL((select Sum(Quantity) from Himall_ShoppingCarts cs where cs.ProductId=ps.ProductId),0) as cartquantity  from himall_searchproducts ps left join Himall_Products pt on ps.ProductId=pt.Id ";
            var where = new Sql();
            GetAppletSearchWhere(query, where);
            var order = new Sql();
            GetAppletSearchOrder(query, order);
            string index = GetForceIndex(query);
            string page = GetSearchPage(query);

            result.Data = DbFactory.Default.Query<ProductView>(string.Concat(sql, index, where.SQL, order.SQL, page), where.Arguments).ToList();
            result.Total = DbFactory.Default.ExecuteScalar<int>(string.Concat(countsql, where.SQL), where.Arguments);

            return result;
        }
        /// <summary>
        /// 获取搜索过滤sql
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        private void GetAppletSearchWhere(SearchProductQuery query, Sql where)
        {
            where.Append("WHERE CanSearch=1 ");
            #region 过滤条件
            if (query.ShopId != 0)
            {
                where.Append("AND ps.ShopId=@0 ", query.ShopId);
                //parms.Add("@ShopId", query.ShopId);
            }

            if (query.VShopId != 0)
            {
                where.Append(" AND ps.ShopId IN (SELECT ShopId FROM himall_vshop where Id=@0) ", query.VShopId);
                //parms.Add("@VShopId", query.VShopId);
            }

            if ((query.VShopId != 0 || query.ShopId != 0) && query.ShopCategoryId != 0)
            {
                where.Append(" AND ps.ProductId IN (select ProductId from himall_productshopcategories where ShopCategoryId in (select id from Himall_ShopCategories where ShopId = @1 and(id = @0 or ParentCategoryId = @0))) ", query.ShopCategoryId, query.ShopId);
                //parms.Add("@ShopCategoryId", query.ShopCategoryId);

            }

            if (query.BrandId != 0)
            {
                where.Append("AND ps.BrandId=@0 ", query.BrandId);
                //parms.Add("@BrandId", query.BrandId);

            }

            if (query.FirstCateId != 0)
            {
                where.Append("AND ps.FirstCateId=@0 ", query.FirstCateId);
                //parms.Add("@FirstCateId", query.FirstCateId);

            }
            else if (query.SecondCateId != 0)
            {
                where.Append("AND ps.SecondCateId=@0 ", query.SecondCateId);
                //parms.Add("@SecondCateId", query.SecondCateId);

            }
            else if (query.ThirdCateId != 0)
            {
                where.Append("AND ps.ThirdCateId=@0 ", query.ThirdCateId);
                //parms.Add("@ThirdCateId", query.ThirdCateId);

            }

            if (query.StartPrice > 0)
            {
                where.Append(" AND ps.SalePrice>=@0 ", query.StartPrice);
                //parms.Add("@StartPrice", query.StartPrice);

            }

            if (query.EndPrice > 0 && query.EndPrice >= query.StartPrice)
            {
                where.Append(" AND ps.SalePrice <= @EndPrice ");
                //parms.Add("@EndPrice", query.EndPrice);

            }

            if (query.AttrValIds.Count > 0)
            {
                where.Append(" AND ps.ProductId IN (SELECT DISTINCT ProductId FROM Himall_ProductAttributes WHERE ValueId IN (@0)) ", query.AttrValIds);
                //parms.Add("@ValueIds", query.AttrValIds.ToArray());

            }

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                if (!query.IsLikeSearch)
                {
                    where.Append("AND MATCH(ps.ProductName) AGAINST(@0 IN BOOLEAN MODE) ", string.Concat(query.Keyword, "*").Replace(" ", "*"));
                    //parms.Add("@ProductName", string.Concat(query.Keyword, "*").Replace(" ", "*"));

                }
                else
                {
                    where.Append("AND ps.ProductName like @0 ", "%" + query.Keyword + "%");
                    //parms.Add("@ProductName", "%" + query.Keyword + "%");

                }
            }
            #endregion
        }
        /// <summary>
        /// 获取搜索排序sql
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private void GetAppletSearchOrder(SearchProductQuery query, Sql order)
        {
            switch (query.OrderKey)
            {
                case 2:
                    order.Append(" ORDER BY ps.SaleCount ");
                    break;
                case 3:
                    order.Append(" ORDER BY ps.SalePrice ");
                    break;
                case 4:
                    order.Append(" ORDER BY ps.Comments ");
                    break;
                case 5:
                    order.Append(" ORDER BY ps.OnSaleTime ");
                    break;
                default:
                    order.Append(" ORDER BY ps.Id ");
                    break;
            }
            if (!query.OrderType)
                order.Append(" DESC ");
            else
                order.Append(" ASC ");
        }
        #endregion
    }
}
