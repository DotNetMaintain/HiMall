using Himall.CommonModel;
using Himall.Core.Plugins.Message;
using Himall.Entity;
using Himall.IServices;
using Himall.DTO.QueryModel;
using Himall.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Himall.Service
{
    public class WXSmallProgramService : ServiceBase, IWXSmallProgramService
    {
        /// <summary>
        /// 添加商品
        /// </summary>
        /// <param name="mWXSmallChoiceProductsInfo"></param>
        public void AddWXSmallProducts(WXSmallChoiceProductsInfo mWXSmallChoiceProductsInfo)
        {
            Context.WXSmallChoiceProductsInfo.Add(mWXSmallChoiceProductsInfo);
            Context.SaveChanges();
        }

        /// <summary>
        /// 获取所有的商品
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public QueryPageModel<ProductInfo> GetWXSmallProducts(int page, int rows, ProductQuery productQuery)
        {
            QueryPageModel<ProductInfo> model = new QueryPageModel<ProductInfo>();
            StringBuilder sqlsb = new StringBuilder();
            StringBuilder countsb = new StringBuilder();
            StringBuilder wheresb = new StringBuilder();
            countsb.Append(" select count(1) from Himall_Products pt left join Himall_WXSmallChoiceProducts ps on pt.Id=ps.ProductId left join Himall_Shops s on pt.ShopId=s.Id ");
            sqlsb.Append(" select *,s.ShopName from Himall_Products pt left join Himall_WXSmallChoiceProducts ps on pt.Id=ps.ProductId ");
            sqlsb.Append(" left join Himall_Shops s on pt.ShopId=s.Id ");
            wheresb.Append(" where pt.IsDeleted=FALSE and ps.ProductId>0 ");
            if (!string.IsNullOrWhiteSpace(productQuery.KeyWords))
                wheresb.AppendFormat(" and pt.ProductName like '%{0}%' ", productQuery.KeyWords);
            if (!string.IsNullOrWhiteSpace(productQuery.ShopName))
                wheresb.AppendFormat(" and s.ShopName like '%{0}%' ", productQuery.ShopName);
            wheresb.Append(" order by ps.ProductId ");
            var start = (page - 1) * rows;
            var end = page * rows;
            countsb.Append(wheresb);
            sqlsb.Append(wheresb);
            sqlsb.Append(" limit " + start + "," + rows);
            var list = Context.Database.SqlQuery<ProductInfo>(sqlsb.ToString()).ToList();
            var shops = Context.ShopInfo;
            var products = list.ToArray().Select(item =>
            {
                var shop = shops.FirstOrDefault(s => s.Id == item.ShopId);
                if (shop != null)
                    item.ShopName = shop.ShopName;
                return item;
            });
            model.Models = products.ToList();
            var count = 0;
            count = Context.Database.SqlQuery<int>(countsb.ToString()).FirstOrDefault();
            model.Total = count;
            return model;
        }

        /// <summary>
        /// 获取商品
        /// </summary>
        /// <returns></returns>
        public List<WXSmallChoiceProductsInfo> GetWXSmallProducts()
        {
            return Context.WXSmallChoiceProductsInfo.ToList();
        }
        /// <summary>
        /// 批量获取商品信息
        /// </summary>
        /// <param name="productIds"></param>
        /// <returns></returns>
        public List<WXSmallChoiceProductsInfo> GetWXSmallProductInfo(IEnumerable<long> productIds)
        {
            return Context.WXSmallChoiceProductsInfo.Where(d => productIds.Contains(d.ProductId)).ToList();
        }

        /// <summary>
        /// 移除商品
        /// </summary>
        /// <param name="Ids"></param>
        public void RemoveWXSmallProducts(IEnumerable<long> Ids)
        {
            Context.WXSmallChoiceProductsInfo.Remove(item => Ids.Contains(item.ProductId));
            Context.SaveChanges();
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="Id"></param>
        public void DeleteWXSmallProductById(long Id)
        {
            Context.WXSmallChoiceProductsInfo.Remove(item => item.ProductId == Id);
            Context.SaveChanges();
        }
        public IQueryable<ProductInfo> GetWXSmallHomeProducts()
        {
            var products = (from mp in Context.ProductInfo
                            join p in Context.WXSmallChoiceProductsInfo on mp.Id equals p.ProductId
                            where p.ProductId > 0
                            select mp);
            return products;
        }
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="ids"></param>
        public void DeleteWXSmallProductByIds(long[] ids)
        {
            var model = Context.WXSmallChoiceProductsInfo.FindBy(item => ids.Contains(item.ProductId));
            Context.WXSmallChoiceProductsInfo.RemoveRange(model);
            Context.SaveChanges();
        }
    }
}
