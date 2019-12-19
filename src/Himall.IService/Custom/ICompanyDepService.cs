using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;
using Himall.CommonModel;
using Himall.DTO.QueryModel;
using Himall.DTO;

namespace Himall.IServices
{
    /// <summary>
    /// 部门服务
    /// </summary>
    public interface ICompanyDepService : IService
    {
        /// <summary>
        /// 添加部门
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool AddCompanyDep(Model.CompanyDepInfo model);
        /// <summary>
        /// 修改部门信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool UpdateCompanyDep(Model.CompanyDepInfo model);
        /// <summary>
        /// 删除部门
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool DeleteCompanyDep(long id);
        /// <summary>
        /// 获取部门
        /// </summary>
        /// <param name="CompanyDepQuery"></param>
        /// <returns></returns>
        QueryPageModel<CompanyDepInfo> GetCompanyDepInfos(CompanyDepQuery CompanyDepQuery);
        /// <summary>
        /// 获取部分公司信息
        /// </summary>
        /// <param name="CompanyDepQuery"></param>
        /// <returns></returns>
        List<CompanyDepInfo> GetCompanyDepList(CompanyDepQuery CompanyDepQuery);
        /// <summary>
        /// 判断是否已经拥有部门信息
        /// </summary>
        /// <param name="CompanyDepQuery"></param>
        /// <returns></returns>
        bool HasCompanyDepInfo(CompanyDepQuery CompanyDepQuery);
        /// <summary>
        /// 获取单个部门信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        CompanyDepInfo GetCompanyDepInfo(long Id);

        /// <summary>
        /// 获取部门列表(id、名称)
        /// </summary>
        /// <param name="companyId">公司ID</param>
        /// <returns></returns>
        List<CompanyDepInfo> GetCompanyDepListByCompanyId(long companyId);
    }
}
