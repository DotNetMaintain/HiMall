using Himall.Entity;
using Himall.IServices;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using System.IO;
using EntityFramework.Extensions;
using Himall.CommonModel;
using System.Data.Common;
using MySql.Data.MySqlClient;
using NetRube.Data;
using Himall.DTO.QueryModel;

namespace Himall.Service
{
    public class CompanyService : ServiceBase, ICompanyService
    {
        public bool AddCompany(Model.CompanyInfo model)
        {
            return new BaseDao().Add(model) > 0;
        }

        public bool UpdateCompany(Model.CompanyInfo model)
        {
            return new BaseDao().Update(model);
        }

        public bool DeleteCompany(long id)
        {
            StringBuilder sqlsb = new StringBuilder();

            sqlsb.Append(" SELECT COUNT(*) FROM himall_members WHERE (Status = 0 or Status = 1) and CompanyId= " + id);

            int total = Context.Database.SqlQuery<int>(sqlsb.ToString()).FirstOrDefault();
            if (total > 0)//已有人员注册的公司不可删除
                return false;
            else
            {
                new BaseDao().Delete<CompanyInfo>(id);
                //删除部门
                sqlsb.Clear();
                sqlsb.Append(" DELETE FROM Custom_CompanyDepInfos WHERE CompanyId= " + id);
                Context.Database.ExecuteSqlCommand(sqlsb.ToString(), id);
                return true;
            }
        }

        public QueryPageModel<CompanyInfo> GetCompanyInfos(CompanyQuery companyQuery)
        {
            QueryPageModel<CompanyInfo> model = new QueryPageModel<CompanyInfo>();
            StringBuilder sqlsb = new StringBuilder();
            StringBuilder countsb = new StringBuilder();
            StringBuilder wheresb = new StringBuilder();

            countsb.Append(" SELECT COUNT(1) FROM Custom_CompanyInfos");
            sqlsb.Append(" SELECT c.*,IFNULL(d.depNum,0) DepNum,IFNULL(m.memNum,0) MemNum,IFNULL(am.memNum,0) MemNumAuditing FROM Custom_CompanyInfos c LEFT JOIN (SELECT CompanyId,COUNT(Id) depNum FROM custom_companydepinfos GROUP BY CompanyId) d ON c.Id=d.CompanyId LEFT JOIN (SELECT CompanyId,COUNT(Id) memNum from himall_members where Status = 1 GROUP BY CompanyId ) m on c.Id=m.CompanyId LEFT JOIN (SELECT CompanyId,COUNT(Id) memNum from himall_members where Status = 0 GROUP BY CompanyId ) am on c.Id=am.CompanyId");
            wheresb.Append(" WHERE 1=1 "); 

            if (!string.IsNullOrWhiteSpace(companyQuery.AddressDetail))
                wheresb.AppendFormat(" and c.AddressDetail like '%{0}%' ", companyQuery.AddressDetail);
            if (!string.IsNullOrWhiteSpace(companyQuery.Code))
                wheresb.AppendFormat(" and c.Code like '%{0}%' ", companyQuery.Code);
            if (!string.IsNullOrWhiteSpace(companyQuery.Contacts))
                wheresb.AppendFormat(" and c.Contacts like '%{0}%' ", companyQuery.Contacts);
            if (!string.IsNullOrWhiteSpace(companyQuery.Name))
                wheresb.AppendFormat(" and c.Name like '%{0}%' ", companyQuery.Name);
            if (!string.IsNullOrWhiteSpace(companyQuery.Phone))
                wheresb.AppendFormat(" and c.Phone like '%{0}%' ", companyQuery.Phone);
            if (companyQuery.RegionId.HasValue)
                wheresb.AppendFormat(" and c.CONCAT(',',Regionlink,',') LIKE '%,{0},%' ", companyQuery.RegionId.Value);

            if (companyQuery.EndDate.HasValue)
                wheresb.AppendFormat(" and c.CreateDate < '{0}'", companyQuery.EndDate.Value.ToString("yyyy-MM-dd") + " 23:59:59");
            if (companyQuery.StartDate.HasValue)
                wheresb.AppendFormat(" and c.CreateDate > '{0}'", companyQuery.StartDate.Value.ToString("yyyy-MM-dd") + " 00:00:00");

            if (!string.IsNullOrWhiteSpace(companyQuery.Sort))
                wheresb.Append(" ORDER BY " + companyQuery.Sort + (companyQuery.IsAsc ? " ASC" : " DESC"));
            else
                wheresb.Append(" ORDER BY CreateDate DESC");


            var start = (companyQuery.PageNo - 1) * companyQuery.PageSize;
            var end = companyQuery.PageNo * companyQuery.PageSize;
            countsb.Append(wheresb);
            sqlsb.Append(wheresb);

            sqlsb.Append(" limit " + start + "," + end);

            var list = Context.Database.SqlQuery<CompanyInfo>(sqlsb.ToString()).ToList();

            model.Models = list;
            var count = 0;
            count = Context.Database.SqlQuery<int>(countsb.ToString()).FirstOrDefault();
            model.Total = count;
            return model;
        }

        public List<CompanyInfo> GetCompanyList(CompanyQuery companyQuery)
        {
            QueryPageModel<CompanyInfo> model = new QueryPageModel<CompanyInfo>();
            StringBuilder sqlsb = new StringBuilder();
            StringBuilder wheresb = new StringBuilder();

            sqlsb.Append(" SELECT * FROM Custom_CompanyInfos ");
            wheresb.Append(" WHERE 1=1 ");

            if (!string.IsNullOrWhiteSpace(companyQuery.AddressDetail))
                wheresb.AppendFormat(" and AddressDetail like '%{0}%' ", companyQuery.AddressDetail);
            if (!string.IsNullOrWhiteSpace(companyQuery.Code))
                wheresb.AppendFormat(" and Code like '%{0}%' ", companyQuery.Code);
            if (!string.IsNullOrWhiteSpace(companyQuery.Contacts))
                wheresb.AppendFormat(" and Contacts like '%{0}%' ", companyQuery.Contacts);
            if (!string.IsNullOrWhiteSpace(companyQuery.Name))
                wheresb.AppendFormat(" and Name like '%{0}%' ", companyQuery.Name);
            if (!string.IsNullOrWhiteSpace(companyQuery.Phone))
                wheresb.AppendFormat(" and Phone like '%{0}%' ", companyQuery.Phone);
            if (companyQuery.RegionId.HasValue)
                wheresb.AppendFormat(" and CONCAT(',',Regionlink,',') LIKE '%,{0},%' ", companyQuery.RegionId.Value);

            if (companyQuery.EndDate.HasValue)
                wheresb.AppendFormat(" and CreateDate < '{0}'", companyQuery.EndDate.Value.ToString("yyyy-MM-dd") + " 23:59:59");
            if (companyQuery.StartDate.HasValue)
                wheresb.AppendFormat(" and CreateDate > '{0}'", companyQuery.StartDate.Value.ToString("yyyy-MM-dd") + " 00:00:00");

            if (!string.IsNullOrWhiteSpace(companyQuery.Sort))
                wheresb.Append(" ORDER BY " + companyQuery.Sort + (companyQuery.IsAsc ? " ASC" : " DESC"));
            else
                wheresb.Append(" ORDER BY CreateDate DESC");

            sqlsb.Append(wheresb);

            var list = Context.Database.SqlQuery<CompanyInfo>(sqlsb.ToString()).ToList();
            return list;
        }
        public bool hasCompanyInfo(CompanyQuery companyQuery)
        {
            QueryPageModel<CompanyInfo> model = new QueryPageModel<CompanyInfo>();
            StringBuilder sqlsb = new StringBuilder();
            StringBuilder wheresb = new StringBuilder();

            sqlsb.Append(" SELECT COUNT(*) FROM Custom_CompanyInfos ");
            wheresb.Append(" WHERE 1=0 ");
            if (!companyQuery.Id.HasValue)
            {
                if (!string.IsNullOrWhiteSpace(companyQuery.Code))
                    wheresb.AppendFormat(" or Code = '{0}' ", companyQuery.Code);
                if (!string.IsNullOrWhiteSpace(companyQuery.Contacts))
                    wheresb.AppendFormat(" or Contacts = '{0}' ", companyQuery.Contacts);
                if (!string.IsNullOrWhiteSpace(companyQuery.Name))
                    wheresb.AppendFormat(" or Name = '{0}' ", companyQuery.Name);
                if (!string.IsNullOrWhiteSpace(companyQuery.Phone))
                    wheresb.AppendFormat(" or Phone = '{0}' ", companyQuery.Phone);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(companyQuery.Code))
                    wheresb.AppendFormat(" or (Code = '{0}' and Id <> {1}) ", companyQuery.Code, companyQuery.Id.Value);
                if (!string.IsNullOrWhiteSpace(companyQuery.Contacts))
                    wheresb.AppendFormat(" or (Contacts = '{0}' and Id <> {1}) ", companyQuery.Contacts, companyQuery.Id.Value);
                if (!string.IsNullOrWhiteSpace(companyQuery.Name))
                    wheresb.AppendFormat(" or (Name = '{0}' and Id <> {1}) ", companyQuery.Name, companyQuery.Id.Value);
                if (!string.IsNullOrWhiteSpace(companyQuery.Phone))
                    wheresb.AppendFormat(" or (Phone = '{0}' and Id <> {1}) ", companyQuery.Phone, companyQuery.Id.Value);
            }
            sqlsb.Append(wheresb);

            int total = Context.Database.SqlQuery<int>(sqlsb.ToString()).FirstOrDefault();
            return total > 0;
        }
        public CompanyInfo GetCompanyInfo(long Id)
        {
            //List<SKUInfo> list = new List<SKUInfo>();
            //var sql = new Sql("SELECT * FROM Himall_SKUs WHERE ProductId=@0", productId);

            //list = DbFactory.Default.Query<SKUInfo>(sql).ToList();
            var sql = new Sql("SELECT * FROM Custom_CompanyInfos WHERE Id=@0", Id);

            var info = DbFactory.Default.FirstOrDefault<CompanyInfo>(sql);

            return info;
        }

        /// <summary>
        /// 获取所有的公司Id和公司名称
        /// </summary>
        /// <returns></returns>
        public List<CompanyInfo> GetCompanyListAll()
        {
            StringBuilder sqlsb = new StringBuilder();
            sqlsb.Append(" SELECT Id,Name FROM Custom_CompanyInfos Order by CreateDate Desc");
            return DbFactory.Default.Query<CompanyInfo>(sqlsb.ToString(), "").ToList();
        }

        /// <summary>
        /// 根据用户Id获取单个子公司信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public CompanyInfo GetCompanyInfoByUserId(long userId)
        {
            var sql = new Sql("SELECT * FROM Custom_CompanyInfos WHERE Id = (SELECT companyId from himall_members where id=@0 LIMIT 1)", userId);

            return DbFactory.Default.FirstOrDefault<CompanyInfo>(sql);
        }


    }
}

