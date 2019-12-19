using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Model;
using Himall.CommonModel;
using Himall.DTO.QueryModel;

namespace Himall.IServices
{
    /// <summary>
    /// 子公司服务
    /// </summary>
    public interface ICompanyService : IService
    {
        /// <summary>
        /// 添加子公司
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool AddCompany(Model.CompanyInfo model);
        /// <summary>
        /// 修改子公司信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool UpdateCompany(Model.CompanyInfo model);
        /// <summary>
        /// 删除子公司
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool DeleteCompany(long id);
        /// <summary>
        /// 获取子公司
        /// </summary>
        /// <param name="companyQuery"></param>
        /// <returns></returns>
        QueryPageModel<CompanyInfo> GetCompanyInfos(CompanyQuery companyQuery);
        /// <summary>
        /// 获取部分公司信息
        /// </summary>
        /// <param name="companyQuery"></param>
        /// <returns></returns>
        List<CompanyInfo> GetCompanyList(CompanyQuery companyQuery);
        /// <summary>
        /// 判断是否已经拥有公司信息
        /// </summary>
        /// <param name="companyQuery"></param>
        /// <returns></returns>
        bool hasCompanyInfo(CompanyQuery companyQuery);
        /// <summary>
        /// 获取单个子公司信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        CompanyInfo GetCompanyInfo(long Id);

        /// <summary>
        /// 获取所有的公司Id和公司名称
        /// </summary>
        /// <returns></returns>
        List<CompanyInfo> GetCompanyListAll();
        /// <summary>
        /// 根据用户Id获取单个子公司信息
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        CompanyInfo GetCompanyInfoByUserId(long Id);
    }
}
