using Himall.Core;
using Himall.IServices;
using Himall.Model;
using Himall.Web.Areas.SellerAdmin.Models;
using Himall.Web.Framework;
using Himall.Web.Models;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web.Mvc;

namespace Himall.Web.Areas.SellerAdmin.Controllers
{
    public class BrandController : BaseSellerController
    {

        private IBrandService _iBrandService;
        public BrandController(IBrandService iBrandService)
        {
            _iBrandService = iBrandService;
        }

        public ActionResult Management()
        {
            return View();
        }

        public ActionResult Add()
        {
            return View();
        }
        public ActionResult Edit(long id)
        {
            var data = _iBrandService.GetBrandApply(id);
            if (data.ShopId != CurrentShop.Id)
            {
                throw new HimallException("错误的参数");
            }
            return View(data);
        }
        public ActionResult Show(long id)
        {
            var model = _iBrandService.GetBrandApply(id);
            return View(model);
        }


        [Description("分页获取品牌列表JSON数据")]
        [UnAuthorize]
        [HttpPost]
        // GET: Admin/Brand
        public JsonResult List(int page, int rows)
        {
            var shopId = CurrentSellerManager.ShopId;
            var result = _iBrandService.GetShopBrandApplys(shopId, null, page, rows, "");
            IEnumerable<BrandApplyModel> brands = result.Models.ToArray().Select(item => new BrandApplyModel()
            {
                Id = item.Id,
                BrandId = item.BrandId == null ? 0 : (long)item.BrandId,
                ShopId = item.ShopId,
                BrandName = item.BrandName,
                BrandLogo = Core.HimallIO.GetImagePath(item.Logo),
                BrandDesc = item.Description == null ? "" : item.Description,
                BrandAuthPic = item.AuthCertificate,
                Remark = item.Remark,
                BrandMode = item.ApplyMode,
                PlatRemark = item.PlatRemark,
                AuditStatus = item.AuditStatus,
            });
            DataGridModel<BrandApplyModel> model = new DataGridModel<BrandApplyModel>() { rows = brands, total = result.Total };
            return Json(model);
        }
        [HttpPost]
        [UnAuthorize]
        [ShopOperationLog(Message = "重新编辑申请品牌")]
        public JsonResult EditApply(BrandApplyModel brand)
        {
            var shopId = CurrentSellerManager.ShopId;
            ShopBrandApplysInfo model = _iBrandService.GetBrandApply(brand.Id);
            if (model == null)
            {
                throw new Himall.Core.HimallException("该品牌审核不通过，请重新编辑");
            }
            if (model.ApplyMode == 1)
            {
                var m = _iBrandService.GetBrand(brand.BrandId);
                if (m == null)
                {
                    throw new Himall.Core.HimallException("品牌不存在");
                }
                model.BrandName = m.Name;
                model.Logo = m.Logo;
                model.Description = m.Description;
                model.BrandId = brand.BrandId;
            }
            else
            {
                if (brand.BrandDesc.Length > 200)
                {
                    return Json(new Result() { success = false, msg = "简介过长！" });
                }
                model.BrandId = null;
                model.BrandName = brand.BrandName.Trim();
                model.Logo = brand.BrandLogo;
                model.Description = brand.BrandDesc;
            }

            model.Remark = brand.Remark;
            model.AuthCertificate = brand.BrandAuthPic;
            model.ShopId = shopId;
            model.ApplyTime = DateTime.Now;            

            var oldapply = _iBrandService.GetExistApply(shopId, model.BrandName);
            if (oldapply == null)
            {
                model.AuditStatus = 0;
                _iBrandService.UpdateApplyBrand(model);
            }
            else
            {
                BrandInfo orderband = null;
                if (oldapply.BrandId != null && oldapply.BrandId.HasValue)
                    orderband = _iBrandService.GetBrand(oldapply.BrandId.Value);
                if (orderband != null && orderband.IsDeleted)
                {
                    model.Id = oldapply.Id;
                }
                else
                {
                    if (oldapply.AuditStatus == (int)ShopBrandApplysInfo.BrandAuditStatus.Audited || oldapply.AuditStatus == (int)ShopBrandApplysInfo.BrandAuditStatus.UnAudit)
                    {
                        throw new Himall.Core.HimallException("该品牌申请已存在，请选择申请其他品牌");
                    }
                    model.AuditStatus = 0;
                    _iBrandService.UpdateApplyBrand(model);
                }
            }
            return Json(new Result() { success = true, msg = "重新编辑品牌申请成功！" });
        }


        [HttpPost]
        [UnAuthorize]
        [ShopOperationLog(Message = "申请品牌")]
        public JsonResult Apply(BrandApplyModel brand)
        {
            var shopId = CurrentSellerManager.ShopId;
            ShopBrandApplysInfo model = new ShopBrandApplysInfo();

            model.BrandId = brand.BrandId;
            model.ApplyMode = brand.BrandMode == 1 ? (int)Himall.Model.ShopBrandApplysInfo.BrandApplyMode.Exist : (int)Himall.Model.ShopBrandApplysInfo.BrandApplyMode.New;
            if (brand.BrandMode == 1)
            {
                var m = _iBrandService.GetBrand(brand.BrandId);
                if (m == null)
                {
                    throw new Himall.Core.HimallException("品牌不存在，请刷新页面重新申请");
                }
                model.BrandName = m.Name;
                model.Logo = m.Logo;
                model.Description = m.Description;
            }
            else
            {
                if (brand.BrandDesc.Length > 200)
                {
                    return Json(new Result() { success = false, msg = "简介过长！" });
                }
                model.BrandId = null;
                model.BrandName = brand.BrandName.Trim();
                model.Logo = brand.BrandLogo;
                model.Description = brand.BrandDesc;
            }

            model.Remark = brand.Remark;
            model.AuthCertificate = brand.BrandAuthPic;
            model.ShopId = shopId;
            model.ApplyTime = DateTime.Now;

            var oldapply = _iBrandService.GetExistApply(shopId, model.BrandName);
            //bool flag = _iBrandService.IsExistApply(shopId, model.BrandName);
            if (oldapply == null)
            {
                _iBrandService.ApplyBrand(model);
                return Json(new Result() { success = true, msg = "申请成功！" });
            }
            else
            {
                if (oldapply.Himall_Brands != null && oldapply.Himall_Brands.IsDeleted)
                {
                    model.Id = oldapply.Id;
                }
                else
                {
                    if (oldapply.AuditStatus == (int)ShopBrandApplysInfo.BrandAuditStatus.UnAudit)
                    {
                        return Json(new Result() { success = false, msg = "该品牌正在审核中！" });
                    }
                    else if (oldapply.AuditStatus == (int)ShopBrandApplysInfo.BrandAuditStatus.Audited)
                    {
                        return Json(new Result() { success = false, msg = "该品牌已存在，请选择申请已有品牌！" });
                    }
                }
                model.AuditStatus = 0;
                model.Himall_Brands = oldapply.Himall_Brands;
                model.Himall_Brands.IsDeleted = false;
                _iBrandService.UpdateApplyBrand(model);
                return Json(new Result() { success = true, msg = "申请成功！" });
            }
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult UpdateSellerBrand(BrandModel brand)
        {
            BrandInfo model = new BrandInfo()
            {
                Id = brand.ID,
                Name = brand.BrandName,
                Description = brand.BrandDesc,
                Logo = brand.BrandLogo,
                //AuditStatus = BrandInfo.BrandAuditStatus.UnAudit
            };
            //_iBrandService.UpdateSellerBrand(model);
            return Json(new Result() { success = true, msg = "更新成功" });
        }

        [UnAuthorize]
        [HttpPost]
        public JsonResult IsExist(string name)
        {
            bool flag = _iBrandService.IsExistBrand(name);
            if (flag == false)
            {
                return Json(new Result() { success = false, msg = "该品牌不存在！" });
            }
            else
                return Json(new Result() { success = true, msg = "该品牌已存在！" });
        }

        [UnAuthorize]
        public JsonResult GetBrandsAjax(long? id,string action)
        {
            var brands = _iBrandService.GetBrands("", CurrentSellerManager.ShopId, action);
            var data = new List<Himall.Web.Areas.Admin.Models.Product.BrandViewModel>();
            foreach (var brand in brands)
            {
                data.Add(new Himall.Web.Areas.Admin.Models.Product.BrandViewModel
                {
                    id = brand.Id,
                    isChecked = null == id ? false : id.Equals(brand.Id),
                    value = brand.Name
                });
            }
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
    }
}