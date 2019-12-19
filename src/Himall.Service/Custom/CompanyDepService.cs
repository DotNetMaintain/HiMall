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
using Himall.DTO;

namespace Himall.Service
{
    public class CompanyDepService : ServiceBase, ICompanyDepService
    {
        public bool AddCompanyDep(Model.CompanyDepInfo model)
        {
            return new BaseDao().Add(model) > 0;
        }

        public bool UpdateCompanyDep(Model.CompanyDepInfo model)
        {
            return new BaseDao().Update(model);
        }

        public bool DeleteCompanyDep(long id)
        {
            StringBuilder sqlsb = new StringBuilder();

            sqlsb.Append(" SELECT COUNT(*) FROM himall_members WHERE (Status = 0 or Status = 1) and DepId= " + id);

            int total = Context.Database.SqlQuery<int>(sqlsb.ToString()).FirstOrDefault();
            if (total > 0)//已有人员注册的部门不可删除
                return false;
            else
                return new BaseDao().Delete<CompanyDepInfo>(id);
        }

        public QueryPageModel<CompanyDepInfo> GetCompanyDepInfos(CompanyDepQuery CompanyDepQuery)
        {
            QueryPageModel<CompanyDepInfo> model = new QueryPageModel<CompanyDepInfo>();
            StringBuilder sqlsb = new StringBuilder();
            StringBuilder countsb = new StringBuilder();
            StringBuilder wheresb = new StringBuilder();

            countsb.Append(" SELECT COUNT(1) FROM Custom_CompanyDepInfos d LEFT JOIN Custom_CompanyInfos c on c.Id=d.CompanyId  ");
            sqlsb.Append(" SELECT d.*,c.`Name` CompanyName,c.`Code` CompanyCode, IFNULL(m.memNum,0) MemNum, IFNULL(am.memNum,0) MemNumAuditing FROM Custom_CompanyDepInfos d LEFT JOIN (SELECT DepId,COUNT(Id) memNum from himall_members WHERE Status = 1 GROUP BY DepId ) m on d.Id=m.DepId  LEFT JOIN Custom_CompanyInfos c on c.Id=d.CompanyId  LEFT JOIN (SELECT DepId,COUNT(Id) memNum from himall_members where Status = 0 GROUP BY DepId ) am on d.Id=am.DepId");
            wheresb.Append(" WHERE 1=1 ");

            if (CompanyDepQuery.CompanyId.HasValue)
                wheresb.AppendFormat(" and c.CompanyId = {0}", CompanyDepQuery.CompanyId.Value);

            if (!string.IsNullOrWhiteSpace(CompanyDepQuery.CompanyCode))
                wheresb.AppendFormat(" and c.Code like '%{0}%' ", CompanyDepQuery.CompanyCode);
            if (!string.IsNullOrWhiteSpace(CompanyDepQuery.CompanyName))
                wheresb.AppendFormat(" and c.Name like '%{0}%' ", CompanyDepQuery.CompanyName);

            if (!string.IsNullOrWhiteSpace(CompanyDepQuery.Code))
                wheresb.AppendFormat(" and d.Code like '%{0}%' ", CompanyDepQuery.Code);
            if (!string.IsNullOrWhiteSpace(CompanyDepQuery.Name))
                wheresb.AppendFormat(" and d.Name like '%{0}%' ", CompanyDepQuery.Name);
            if (CompanyDepQuery.EndDate.HasValue)
                wheresb.AppendFormat(" and d.CreateDate < '{0}'", CompanyDepQuery.EndDate.Value.ToString("yyyy-MM-dd") + " 23:59:59");
            if (CompanyDepQuery.StartDate.HasValue)
                wheresb.AppendFormat(" and d.CreateDate > '{0}'", CompanyDepQuery.StartDate.Value.ToString("yyyy-MM-dd") + " 00:00:00");

            if (!string.IsNullOrWhiteSpace(CompanyDepQuery.Sort))
                wheresb.Append(" ORDER BY " + CompanyDepQuery.Sort + (CompanyDepQuery.IsAsc ? " ASC" : " DESC"));
            else
                wheresb.Append(" ORDER BY d.CreateDate DESC");


            var start = (CompanyDepQuery.PageNo - 1) * CompanyDepQuery.PageSize;
            var end = CompanyDepQuery.PageNo * CompanyDepQuery.PageSize;
            countsb.Append(wheresb);
            sqlsb.Append(wheresb);
            sqlsb.Append(" limit " + start + "," + end);

            var list = Context.Database.SqlQuery<CompanyDepInfo>(sqlsb.ToString()).ToList();

            model.Models = list;
            var count = 0;
            count = Context.Database.SqlQuery<int>(countsb.ToString()).FirstOrDefault();
            model.Total = count;
            return model;
        }

        public List<CompanyDepInfo> GetCompanyDepList(CompanyDepQuery CompanyDepQuery)
        {
            QueryPageModel<CompanyDepInfo> model = new QueryPageModel<CompanyDepInfo>();
            StringBuilder sqlsb = new StringBuilder();
            StringBuilder wheresb = new StringBuilder();

            sqlsb.Append(" SELECT d.*,c.`Name` CompanyName,c.`Code` CompanyCode, IFNULL(m.memNum,0) MemNum FROM Custom_CompanyDepInfos d LEFT JOIN (SELECT DepId,COUNT(Id) memNum from himall_members GROUP BY DepId ) m on d.Id=m.DepId  LEFT JOIN Custom_CompanyInfos c on c.Id=d.CompanyId ");
            wheresb.Append(" WHERE 1=1 ");

            if (!string.IsNullOrWhiteSpace(CompanyDepQuery.CompanyCode))
                wheresb.AppendFormat(" and c.Code like '%{0}%' ", CompanyDepQuery.CompanyCode);
            if (!string.IsNullOrWhiteSpace(CompanyDepQuery.CompanyName))
                wheresb.AppendFormat(" and c.Name like '%{0}%' ", CompanyDepQuery.CompanyName);

            if (!string.IsNullOrWhiteSpace(CompanyDepQuery.Code))
                wheresb.AppendFormat(" and d.Code like '%{0}%' ", CompanyDepQuery.Code);
            if (!string.IsNullOrWhiteSpace(CompanyDepQuery.Name))
                wheresb.AppendFormat(" and d.Name like '%{0}%' ", CompanyDepQuery.Name);
            if (CompanyDepQuery.EndDate.HasValue)
                wheresb.AppendFormat(" and d.CreateDate < '{0}'", CompanyDepQuery.EndDate.Value.ToString("yyyy-MM-dd") + " 23:59:59");
            if (CompanyDepQuery.StartDate.HasValue)
                wheresb.AppendFormat(" and d.CreateDate > '{0}'", CompanyDepQuery.StartDate.Value.ToString("yyyy-MM-dd") + " 00:00:00");

            if (!string.IsNullOrWhiteSpace(CompanyDepQuery.Sort))
                wheresb.Append(" ORDER BY " + CompanyDepQuery.Sort + (CompanyDepQuery.IsAsc ? " ASC" : " DESC"));
            else
                wheresb.Append(" ORDER BY CreateDate DESC");
            
            sqlsb.Append(wheresb);

            var list = Context.Database.SqlQuery<CompanyDepInfo>(sqlsb.ToString()).ToList();
            
            return list;
        }
        public bool HasCompanyDepInfo(CompanyDepQuery CompanyDepQuery)
        {
            QueryPageModel<CompanyInfo> model = new QueryPageModel<CompanyInfo>();
            StringBuilder sqlsb = new StringBuilder();
            StringBuilder wheresb = new StringBuilder();
            if (!CompanyDepQuery.CompanyId.HasValue)
                return true;

            sqlsb.Append(" SELECT COUNT(*) FROM Custom_CompanyDepInfos ");
            wheresb.Append(" WHERE 1=0 ");
            if (!CompanyDepQuery.Id.HasValue)
            {
                if (!string.IsNullOrWhiteSpace(CompanyDepQuery.Code))
                    wheresb.AppendFormat(" or (Code = '{0}' and CompanyId <> {1}) ", CompanyDepQuery.Code, CompanyDepQuery.CompanyId.Value);
                if (!string.IsNullOrWhiteSpace(CompanyDepQuery.Name))
                    wheresb.AppendFormat(" or (Name = '{0}' and CompanyId <> {1}) ", CompanyDepQuery.Name, CompanyDepQuery.CompanyId.Value);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(CompanyDepQuery.Code))
                    wheresb.AppendFormat(" or (Code = '{0}' and CompanyId <> {1} and Id <> {2}) ", CompanyDepQuery.Code, CompanyDepQuery.CompanyId.Value, CompanyDepQuery.Id.Value);
                if (!string.IsNullOrWhiteSpace(CompanyDepQuery.Name))
                    wheresb.AppendFormat(" or (Name = '{0}' and CompanyId <> {1} and Id <> {2}) ", CompanyDepQuery.Name, CompanyDepQuery.CompanyId.Value, CompanyDepQuery.Id.Value);
            }
            sqlsb.Append(wheresb);

            int total = Context.Database.SqlQuery<int>(sqlsb.ToString()).FirstOrDefault();
            return total > 0;
        }
        public CompanyDepInfo GetCompanyDepInfo(long Id)
        {
            var sql = new Sql("SELECT * FROM Custom_CompanyDepInfos WHERE Id=@0", Id);

            var info = DbFactory.Default.FirstOrDefault<CompanyDepInfo>(sql);

            return info;
        }


        /// <summary>
        /// 根据公司Id获取部门名称及Id
        /// </summary>
        /// <returns></returns>
        public List<CompanyDepInfo> GetCompanyDepListByCompanyId(long companyId)
        {
            StringBuilder sqlsb = new StringBuilder();
            sqlsb.Append(" SELECT Id,Name FROM Custom_CompanyDepInfos where CompanyId = "+ companyId + " Order by CreateDate Desc");
            return DbFactory.Default.Query<CompanyDepInfo>(sqlsb.ToString(), "").ToList();
        }
    }
}

