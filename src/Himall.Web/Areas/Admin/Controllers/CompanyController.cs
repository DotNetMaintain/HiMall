using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Web.Models;
using Himall.IServices;
using Himall.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Himall.Web.Areas.SellerAdmin.Models;
using System.Threading.Tasks;
using Himall.DTO.QueryModel;
using Himall.DTO;
using Himall.Application;

namespace Himall.Web.Areas.Admin.Controllers
{
    public class CompanyController : BaseAdminController
    {
        private ICompanyService _iCompanyService;
        private ICompanyDepService _iCompanyDepService;
        public CompanyController(ICompanyService iCompanyService, ICompanyDepService iCompanyDepService)
        {
            _iCompanyService = iCompanyService;
            _iCompanyDepService = iCompanyDepService;
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Add()
        {
            return View();
        }

        public ActionResult Edit(long id)
        {
            var model = _iCompanyService.GetCompanyInfo(id);
            return View(model);
        }

        public ActionResult Dep(long? id)
        {
            CompanyInfo companyInfo = new CompanyInfo();
            if (id.HasValue)
            {
                companyInfo = _iCompanyService.GetCompanyInfo(id.Value);
            }
            return View(companyInfo);
        }

        /// <summary>
        /// 获取公司信息
        /// </summary>
        public JsonResult GetCompanyInfos(string name, string code, string contacts, string phone, int? addressId, string startDate, string endDate, int page, int rows)
        {
            var query = new CompanyQuery
            {
                PageNo = page,
                PageSize = rows,
                Name = name,
                Code = code,
                Contacts = contacts,
                Phone = phone,
                RegionId = addressId,
            };

            DateTime dt;
            if (!string.IsNullOrEmpty(startDate))
                if (DateTime.TryParse(startDate, out dt))
                {
                    query.StartDate = dt;
                }
            if (!string.IsNullOrEmpty(endDate))
                if (DateTime.TryParse(endDate, out dt))
                {
                    query.EndDate = dt;
                }

            var pagemodel = _iCompanyService.GetCompanyInfos(query);

            var model = pagemodel.Models.ToList().Select(e =>
            {
                var companyModel = new CompanyModel()
                {
                    Id = e.Id,
                    Name = e.Name,
                    Code = e.Code,
                    Contacts = e.Contacts + "(" + e.Phone + ")",
                    Address = e.Address + e.AddressDetail,
                    CreateDate = e.CreateDate.ToString("yyyy/MM/dd HH:mm"),
                    DepNum = e.DepNum,
                    MemberNum = e.MemNum,
                    MemberNumAuditing = e.MemNumAuditing
                };
                return companyModel;
            });

            var models = new DataGridModel<CompanyModel>
            {
                rows = model,
                total = pagemodel.Total
            };
            return Json(models);
        }
        /// <summary>
        /// 添加公司
        /// </summary>
        [HttpPost]
        public JsonResult Add(CompanyInfo model)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                return Json(new Result() { success = false, msg = "公司名称不可为空！" });
            }
            if (string.IsNullOrEmpty(model.Code))
            {
                return Json(new Result() { success = false, msg = "公司编码不可为空！" });
            }
            if (string.IsNullOrEmpty(model.Contacts))
            {
                return Json(new Result() { success = false, msg = "公司联系人不可为空！" });
            }
            if (string.IsNullOrEmpty(model.Phone))
            {
                return Json(new Result() { success = false, msg = "公司联系电话不可为空！" });
            }
            if (model.RegionId <= 0)
            {
                return Json(new Result() { success = false, msg = "公司联系地址不可为空！" });
            }
            if (string.IsNullOrEmpty(model.AddressDetail))
            {
                return Json(new Result() { success = false, msg = "公司联系电话不可为空！" });
            }
            if (_iCompanyService.hasCompanyInfo(new CompanyQuery { Name = model.Name }))
            {
                return Json(new Result() { success = false, msg = "公司名称不可重复！" });
            }
            if (_iCompanyService.hasCompanyInfo(new CompanyQuery { Code = model.Code }))
            {
                return Json(new Result() { success = false, msg = "公司编码不可重复！" });
            }
            model.Regionlink = RegionApplication.GetRegionPath(model.RegionId);
            model.Address = RegionApplication.GetFullName(model.RegionId, " ");
            model.CreateDate = DateTime.Now;
            if (_iCompanyService.AddCompany(model))
            {
                return Json(new Result() { success = true, msg = "添加成功！" });
            }
            else
                return Json(new Result() { success = true, msg = "添加失败！" });
        }
        /// <summary>
        /// 编辑公司信息
        /// </summary>
        [HttpPost]
        public JsonResult Edit(CompanyInfo model)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                return Json(new Result() { success = false, msg = "公司名称不可为空！" });
            }
            if (string.IsNullOrEmpty(model.Code))
            {
                return Json(new Result() { success = false, msg = "公司编码不可为空！" });
            }
            if (string.IsNullOrEmpty(model.Contacts))
            {
                return Json(new Result() { success = false, msg = "公司联系人不可为空！" });
            }
            if (string.IsNullOrEmpty(model.Phone))
            {
                return Json(new Result() { success = false, msg = "公司联系电话不可为空！" });
            }
            if (model.RegionId <= 0)
            {
                return Json(new Result() { success = false, msg = "公司联系地址不可为空！" });
            }
            if (string.IsNullOrEmpty(model.AddressDetail))
            {
                return Json(new Result() { success = false, msg = "公司联系电话不可为空！" });
            }
            if (_iCompanyService.hasCompanyInfo(new CompanyQuery { Name = model.Name, Id = model.Id }))
            {
                return Json(new Result() { success = false, msg = "公司名称不可重复！" });
            }
            if (_iCompanyService.hasCompanyInfo(new CompanyQuery { Code = model.Code, Id = model.Id }))
            {
                return Json(new Result() { success = false, msg = "公司编码不可重复！" });
            }
            model.Regionlink = RegionApplication.GetRegionPath(model.RegionId);
            model.Address = RegionApplication.GetFullName(model.RegionId, " ");
            model.CreateDate = DateTime.Now;
            if (_iCompanyService.UpdateCompany(model))
            {
                return Json(new Result() { success = true, msg = "修改成功！" });
            }
            else
                return Json(new Result() { success = true, msg = "修改失败！" });
        }


        [HttpPost]
        [Description("删除公司")]
        public JsonResult Delete(int id)
        {
            if (_iCompanyService.DeleteCompany(id))
                return Json(new Result() { success = true, msg = "删除成功！" });
            else
                return Json(new Result() { success = false, msg = "删除失败，已有人员注册审核通过或审核中的公司不可删除！" });
        }


        /// <summary>
        /// 获取部门信息
        /// </summary>
        public JsonResult GetCompanyDepInfos(string companyName, string companyCode, string name, string code, string startDate, string endDate, int page, int rows, long? companyId)
        {
            var query = new CompanyDepQuery
            {
                CompanyId = companyId,
                PageNo = page,
                PageSize = rows,
                CompanyName = companyName,
                CompanyCode = companyCode,
                Name = name,
                Code = code,
            };

            DateTime dt;
            if (!string.IsNullOrEmpty(startDate))
                if (DateTime.TryParse(startDate, out dt))
                {
                    query.StartDate = dt;
                }
            if (!string.IsNullOrEmpty(endDate))
                if (DateTime.TryParse(endDate, out dt))
                {
                    query.EndDate = dt;
                }

            var pagemodel = _iCompanyDepService.GetCompanyDepInfos(query);

            var model = pagemodel.Models.ToList().Select(e =>
            {
                var companyModel = new CompanyDepModel()
                {
                    Id = e.Id,
                    Name = e.Name,
                    Code = e.Code,
                    CompanyId = e.CompanyId,
                    CompanyName = e.CompanyName,
                    CompanyCode = e.CompanyCode,
                    CreateDate = e.CreateDate.ToString("yyyy/MM/dd HH:mm"),
                    MemberNum = e.MemNum,
                    MemberNumAuditing = e.MemNumAuditing
                };
                return companyModel;
            });

            var models = new DataGridModel<CompanyDepModel>
            {
                rows = model,
                total = pagemodel.Total
            };
            return Json(models);
        }


        /// <summary>
        /// 获取公司列表
        /// </summary>
        public JsonResult GetCompanySelector()
        {
            var pagemodel = _iCompanyService.GetCompanyListAll();
            var model = pagemodel.ToList().Select(e =>
            {
                return new
                {
                    e.Id,
                    e.Name
                };
            });
            return Json(model);
        }
        /// <summary>
        /// 获取公司下部门列表
        /// </summary>
        public JsonResult GetCompanyDepSelector(long companyId)
        {
            var pagemodel = _iCompanyDepService.GetCompanyDepListByCompanyId(companyId);
            var model = pagemodel.ToList().Select(e =>
            {
                return new
                {
                    e.Id,
                    e.Name
                };
            });
            return Json(model);
        }
        /// <summary>
        /// 获取部门信息
        /// </summary>
        public JsonResult GetDepInfo(long Id)
        {
            var pagemodel = _iCompanyDepService.GetCompanyDepInfo(Id);

            var data = new
            {
                pagemodel.Name,
                pagemodel.Code
            };
            return Json(data);
        }
        /// <summary>
        /// 添加部门信息
        /// </summary>
        [HttpPost]
        public JsonResult AddDep(string DepName, string DepCode, long CompanyId)
        {
            if (string.IsNullOrEmpty(DepName))
            {
                return Json(new Result() { success = false, msg = "部门名称不可为空！" });
            }

            if (string.IsNullOrEmpty(DepCode))
            {
                return Json(new Result() { success = false, msg = "部门编码不可为空！" });
            }
            if (CompanyId <= 0)
            {
                return Json(new Result() { success = false, msg = "所属公司不可为空！" });
            }
            var companyInfo = _iCompanyService.GetCompanyInfo(CompanyId);
            if (companyInfo == null)
            {
                return Json(new Result() { success = false, msg = "所属公司可能被删除，请刷新后重试！" });
            }
            if (_iCompanyDepService.HasCompanyDepInfo(new CompanyDepQuery { CompanyId = CompanyId, Name = DepName }))
            {
                return Json(new Result() { success = false, msg = "部门名称不可重复！" });
            }
            if (_iCompanyDepService.HasCompanyDepInfo(new CompanyDepQuery { CompanyId = CompanyId, Code = DepCode }))
            {
                return Json(new Result() { success = false, msg = "部门编码不可重复！" });
            }
            CompanyDepInfo dep = new CompanyDepInfo();
            dep.Code = DepCode;
            dep.Name = DepName;
            dep.CompanyId = CompanyId;
            dep.CreateDate = DateTime.Now;

            if (_iCompanyDepService.AddCompanyDep(dep))
            {
                return Json(new Result() { success = true, msg = "保存成功！" });
            }
            else
                return Json(new Result() { success = true, msg = "保存失败！" });
        }
        /// <summary>
        /// 编辑部门信息
        /// </summary>
        [HttpPost]
        public JsonResult EditDep(long Id, string DepName, string DepCode)
        {
            if (string.IsNullOrEmpty(DepName))
            {
                return Json(new Result() { success = false, msg = "部门名称不可为空！" });
            }

            if (string.IsNullOrEmpty(DepCode))
            {
                return Json(new Result() { success = false, msg = "部门编码不可为空！" });
            }
            if (Id <= 0)
            {
                return Json(new Result() { success = false, msg = "请选择编辑的部门不可为空！" });
            }
            var depInfo = _iCompanyDepService.GetCompanyDepInfo(Id);
            if (depInfo == null)
            {
                return Json(new Result() { success = false, msg = "部门可能被删除，请刷新后重试！" });
            }
            if (_iCompanyDepService.HasCompanyDepInfo(new CompanyDepQuery { CompanyId = depInfo.CompanyId, Id = Id, Name = DepName }))
            {
                return Json(new Result() { success = false, msg = "部门名称不可重复！" });
            }
            if (_iCompanyDepService.HasCompanyDepInfo(new CompanyDepQuery { CompanyId = depInfo.CompanyId, Id = Id, Code = DepCode }))
            {
                return Json(new Result() { success = false, msg = "部门编码不可重复！" });
            }

            depInfo.Code = DepCode;
            depInfo.Name = DepName;

            if (_iCompanyDepService.UpdateCompanyDep(depInfo))
            {
                return Json(new Result() { success = true, msg = "修改成功！" });
            }
            else
                return Json(new Result() { success = true, msg = "修改失败！" });
        }


        [HttpPost]
        [Description("删除部门")]
        public JsonResult DeleteDep(int id)
        {
            if (_iCompanyDepService.DeleteCompanyDep(id))
                return Json(new Result() { success = true, msg = "删除成功！" });
            else
                return Json(new Result() { success = false, msg = "删除失败，已有人员注册审核通过或审核中的部门不可删除！" });
        }
    }
}












