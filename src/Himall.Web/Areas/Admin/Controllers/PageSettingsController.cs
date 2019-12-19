using Himall.IServices;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Himall.Model;
using Himall.Core;
using Himall.Web.Areas.Admin.Models;
using Himall.Web.Areas.Admin.Models.Product;
using System.IO;
using Himall.DTO;
using Himall.Application;
using Himall.CommonModel;
using System.Threading.Tasks;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using Himall.DTO.QueryModel;
using Himall.Web.Models;

namespace Himall.Web.Areas.Admin.Controllers
{
    [PCAuthorizationAttribute]
    public class PageSettingsController : BaseAdminController
    {
        const string _templateHtmlFileFullName = "~/Areas/Admin/Views/PageSettings/templates/@template.html";
        const string _templateDataFileFullName = "~/Areas/Admin/Views/PageSettings/templates/@template.data.json";
        const string _templatesettings = "~/Areas/Admin/Views/PageSettings/templatesettings.json";
        const string _themesettings = "~/Areas/Admin/Views/PageSettings/themesetting.json";
        const string _homeIndexFileFullName = "~/Areas/Web/Views/Home/index1.html";
        const string _templateHeadHtmlFileFullName = "~/Areas/Web/Views/Shared/head.html";
        const string _templateFootHtmlFileFullName = "~/Areas/Web/Views/Shared/foot.html";

        ISiteSettingService _iSiteSettingService;
        ISlideAdsService _iSlideAdsService;
        IBrandService _iBrandService;
        ITypeService _iTypeService;
        IHomeCategoryService _iHomeCategoryService;
        ICategoryService _iCategoryService;
        IFloorService _iFloorService;
        IArticleCategoryService _iArticleCategoryService;
        IArticleService _iArticleService;

        public PageSettingsController(
        ISiteSettingService iSiteSettingService,
        ISlideAdsService iSlideAdsService,
        IBrandService iBrandService,
        ITypeService iTypeService,
        IHomeCategoryService iHomeCategoryService,
        ICategoryService iCategoryService,
        IFloorService iFloorService,
         IArticleCategoryService iArticleCategoryService,
           IArticleService iArticleService
            )
        {
            _iSiteSettingService = iSiteSettingService;
            _iSlideAdsService = iSlideAdsService;
            _iBrandService = iBrandService;
            _iTypeService = iTypeService;
            _iHomeCategoryService = iHomeCategoryService;
            _iCategoryService = iCategoryService;
            _iFloorService = iFloorService;
            _iArticleCategoryService = iArticleCategoryService;
            _iArticleService = iArticleService;

        }
        // GET: Admin/PageSettings
        public ActionResult Index1()
        {
            var settings = _iSiteSettingService.GetSiteSettings();
            ViewBag.Logo = settings.Logo;
            ViewBag.Keyword = settings.Keyword;
            ViewBag.Hotkeywords = settings.Hotkeywords;

            var imageAds = _iSlideAdsService.GetImageAds(0).ToList();

            ViewBag.ImageAdsTop = imageAds.Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.BannerAds).ToList();
            ViewBag.HeadAds = imageAds.Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.HeadRightAds).ToList();
            ViewBag.CenterAds = imageAds.Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.Customize).ToList();
            ViewBag.ShopAds = imageAds.Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.BrandsAds).ToList();
            ViewBag.BottomPic = CurrentSiteSetting.PCBottomPic;
            ViewBag.AdvertisementUrl = CurrentSiteSetting.AdvertisementUrl;
            ViewBag.AdvertisementImagePath = CurrentSiteSetting.AdvertisementImagePath;
            ViewBag.AdvertisementState = CurrentSiteSetting.AdvertisementState;

            var imageAdsz = imageAds.Where(p => p.TypeId == Himall.CommonModel.ImageAdsType.Single).ToArray();
            return View(imageAdsz);
        }

        public ActionResult Home()
        {
            string currentTempdate = System.IO.File.ReadAllText(this.Server.MapPath(_templatesettings));//读取当前应用的模板
            TemplateSetting curTemplateObj = ParseFormJson<TemplateSetting>(currentTempdate);
            if (null == curTemplateObj)
            {
                throw new HimallException("错误的模板");
            }
            ViewBag.CurrentTemplate = curTemplateObj;
            ViewBag.CurUrl = Request.Url.Scheme + "://" + Request.Url.Authority;
            return View();
        }

        public async Task<ActionResult> UpdateCurrentTemplate(string tName, string name, int tid)
        {
            if (string.IsNullOrWhiteSpace(tName)) return Json(new { success = false, msg = "模板名称不能为空" });
            var fullName = this.Server.MapPath(_templatesettings);
            TemplateSetting info = new TemplateSetting()
            {
                CurrentTemplateName = tName,
                Name = name.Replace("_", ""),
                Id = tid
            };
            string json = ObjectToJson<TemplateSetting>(info, Encoding.UTF8);
            using (var fs = new FileStream(fullName, FileMode.Create, FileAccess.Write))
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                await fs.WriteAsync(buffer, 0, buffer.Length);
            }

            //如果当前启用的这个模板数据文件不存在，则读取默认模板的数据，生成一个该模板的数据json文件。就如同在模板编辑页中点保存一样。
            fullName = this.Server.MapPath(_templateDataFileFullName.Replace("@template", tName));
            if (!System.IO.File.Exists(fullName))
            {
                string defaultTemplateJson = System.IO.File.ReadAllText(this.Server.MapPath(_templateDataFileFullName.Replace("@template", "template_0")));
                using (var fs = new FileStream(fullName, FileMode.Create, FileAccess.Write))
                {
                    var buffer = System.Text.Encoding.UTF8.GetBytes(defaultTemplateJson);
                    await fs.WriteAsync(buffer, 0, buffer.Length);
                }
            }

            #region 将当前启用的模板应用到前台首页
            string templateJson = System.IO.File.ReadAllText(this.Server.MapPath(_templateDataFileFullName.Replace("@template", tName)));
            string otherInfo = @"\""otherInfo\"":[\s\S]*?(?<otherInfo>[\s\S]*?)\""jsonInfo\""";
            otherInfo = MatchComInfo(templateJson, otherInfo, "otherInfo").TrimStart('{').TrimEnd(',').Trim();
            otherInfo = "{" + otherInfo + "}";

            TemplateOtherInfo other = null;
            if (!string.IsNullOrWhiteSpace(otherInfo))
            {
                other = ParseFormJson<TemplateOtherInfo>(otherInfo);
            }
            string variableName = "jsonObj", script = "", resultHtml = "", footHtml = "", themeJson = "";
            if (other != null)
            {
                script = other.Script;
                resultHtml = other.ResultHtml;
                footHtml = other.FootHtml;
                themeJson = other.ThemeJson;
            }
            string html = System.IO.File.ReadAllText(this.Server.MapPath(_templateHtmlFileFullName.Replace("@template", "template_0")));//读取模板html文件内容
            html = FilterScript(html, variableName);//先移除掉之前追加的json数据
            StringBuilder str = new StringBuilder();
            str.Append("<script>");
            str.AppendFormat("var {0}={1};{2}", variableName, templateJson, script);
            str.AppendLine("</script>");
            html = html.Replace("</body>", str.ToString() + "</body>");
            if (!string.IsNullOrWhiteSpace(html))
            {
                //更新index1.html
                fullName = this.Server.MapPath(_homeIndexFileFullName);
                using (var fs = new FileStream(fullName, FileMode.Create, FileAccess.Write))
                {
                    var buffer = System.Text.Encoding.UTF8.GetBytes(html);
                    await fs.WriteAsync(buffer, 0, buffer.Length);
                }
                //更新头部html
                fullName = this.Server.MapPath(_templateHeadHtmlFileFullName);
                using (var fs = new FileStream(fullName, FileMode.Create, FileAccess.Write))
                {
                    var buffer = System.Text.Encoding.UTF8.GetBytes(resultHtml.Replace("<label>删除</label>", ""));
                    await fs.WriteAsync(buffer, 0, buffer.Length);
                }

                //更新底部html
                fullName = this.Server.MapPath(_templateFootHtmlFileFullName);
                using (var fs = new FileStream(fullName, FileMode.Create, FileAccess.Write))
                {
                    var buffer = System.Text.Encoding.UTF8.GetBytes(footHtml.Replace("<label>删除</label>", ""));
                    await fs.WriteAsync(buffer, 0, buffer.Length);
                }

                //#region 保存当前主题配色设置[前端页面使用]
                //fullName = this.Server.MapPath(_themesettings);
                //using (var fs = new FileStream(fullName, FileMode.Create, FileAccess.Write))
                //{
                //    var buffer = System.Text.Encoding.UTF8.GetBytes(themeJson.Replace("[", "").Replace("]", ""));//这里直接处理方便
                //    await fs.WriteAsync(buffer, 0, buffer.Length);
                //}
                //#endregion
            }
            #endregion


            return Json(new { success = true, msg = "模板启用成功" });
        }

        public ActionResult Index(string tName = "", string name = "", int tid = 0)
        {
            ViewBag.tName = tName;
            ViewBag.name = name;
            ViewBag.tid = tid;
            string path = this.Server.MapPath(_templateDataFileFullName.Replace("@template", tName));
            if (!System.IO.File.Exists(path))
            {
                path = this.Server.MapPath(_templateDataFileFullName.Replace("@template", "template_0"));
            }
            object data = System.IO.File.ReadAllText(path);
            return View(data);
        }

        public ActionResult Template(string tName = "")
        {
            if (!string.IsNullOrWhiteSpace(tName))
            {
                string path = this.Server.MapPath(_templateHtmlFileFullName.Replace("@template.html", "template_0.html"));//模板html公用，模板数据jsonN个

                return File(path, "text/html");
            }
            return Content("");
        }

        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> Save(bool use, string json, string tname, string name, int tid, string variableName, string script, string resultHtml, string footHtml, string themeJson)
        {
            if (string.IsNullOrWhiteSpace(tname))
                return Json(new
                {
                    Success = false
                }, true);

            var fullName = this.Server.MapPath(_templateDataFileFullName.Replace("@template", tname));
            using (var fs = new FileStream(fullName, FileMode.Create, FileAccess.Write))
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                await fs.WriteAsync(buffer, 0, buffer.Length);
            }
            string html = System.IO.File.ReadAllText(this.Server.MapPath(_templateHtmlFileFullName.Replace("@template", "template_0")));//读取模板html文件内容

            html = FilterScript(html, variableName);//先移除掉之前追加的json数据

            StringBuilder str = new StringBuilder();
            str.Append("<script>");
            str.AppendFormat("var {0}={1};{2}", variableName, json, script);
            str.AppendLine("</script>");
            html = html.Replace("</body>", str.ToString() + "</body>");

            if (use && !string.IsNullOrWhiteSpace(html))
            {
                //更新index1.html
                fullName = this.Server.MapPath(_homeIndexFileFullName);
                using (var fs = new FileStream(fullName, FileMode.Create, FileAccess.Write))
                {
                    var buffer = System.Text.Encoding.UTF8.GetBytes(html);
                    await fs.WriteAsync(buffer, 0, buffer.Length);
                }
                //更新头部html
                fullName = this.Server.MapPath(_templateHeadHtmlFileFullName);
                using (var fs = new FileStream(fullName, FileMode.Create, FileAccess.Write))
                {
                    var buffer = System.Text.Encoding.UTF8.GetBytes(resultHtml.Replace("<label>删除</label>", ""));
                    await fs.WriteAsync(buffer, 0, buffer.Length);
                }

                //更新底部html
                fullName = this.Server.MapPath(_templateFootHtmlFileFullName);
                using (var fs = new FileStream(fullName, FileMode.Create, FileAccess.Write))
                {
                    var buffer = System.Text.Encoding.UTF8.GetBytes(footHtml.Replace("<label>删除</label>", ""));
                    await fs.WriteAsync(buffer, 0, buffer.Length);
                }

                #region 保存当前模板配置
                var fullName2 = this.Server.MapPath(_templatesettings);
                TemplateSetting info = new TemplateSetting()
                {
                    CurrentTemplateName = tname,
                    Name = name.Replace("_", ""),
                    Id = tid
                };
                string json2 = ObjectToJson<TemplateSetting>(info, Encoding.UTF8);
                using (var fs = new FileStream(fullName2, FileMode.Create, FileAccess.Write))
                {
                    var buffer = System.Text.Encoding.UTF8.GetBytes(json2);
                    await fs.WriteAsync(buffer, 0, buffer.Length);
                }
                #endregion

                //#region 保存当前主题配色设置[前端页面使用]
                //fullName = this.Server.MapPath(_themesettings);
                //using (var fs = new FileStream(fullName, FileMode.Create, FileAccess.Write))
                //{
                //    var buffer = System.Text.Encoding.UTF8.GetBytes(themeJson.Replace("[", "").Replace("]", ""));//这里直接处理方便
                //    await fs.WriteAsync(buffer, 0, buffer.Length);
                //}
                //#endregion
            }

            return Json(new
            {
                Success = true
            }, true);
        }

        public JsonResult GetBrands()
        {
            var data = _iBrandService.GetBrands("").Select(item => new
            {
                item.Description,
                item.DisplaySequence,
                item.Id,
                item.Logo,
                item.Meta_Description,
                item.Meta_Keywords,
                item.Meta_Title,
                item.Name,
                item.RewriteName
            }).ToList();
            return Json(data, true);
        }

        private void SaveCurrentTemplateSetting(string tname, string name, int tid)
        {
            var fullName = this.Server.MapPath(_templatesettings);
            TemplateSetting info = new TemplateSetting()
            {
                CurrentTemplateName = tname,
                Name = name,
                Id = tid
            };
            string json = ObjectToJson<TemplateSetting>(info, Encoding.UTF8);
            using (var fs = new FileStream(fullName, FileMode.Create, FileAccess.Write))
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                fs.WriteAsync(buffer, 0, buffer.Length);
            }
        }

        [UnAuthorize]
        public JsonResult GetBrandsAjax(long id = 0)
        {
            HomeFloorInfo homeFloor = null;
            IEnumerable<HomeFloorDetail.Brand> _Floorbrands = null;
            var brands = _iBrandService.GetBrands("");
            if (id != 0)
            {
                homeFloor = _iFloorService.GetHomeFloor(id);
                _Floorbrands = homeFloor.FloorBrandInfo.Select(
                    item => new HomeFloorDetail.Brand()
                    {
                        Id = item.BrandId,
                        Name = item.BrandInfo.Name
                    }
                    );
            }

            var data = new List<BrandViewModel>();
            foreach (var brand in brands)
            {
                data.Add(new Models.Product.BrandViewModel
                {
                    id = brand.Id,
                    isChecked = null == _Floorbrands ? false : _Floorbrands.Any(b => b.Id.Equals(brand.Id)),
                    value = brand.Name
                });
            }
            return Json(new
            {
                data = data
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCategories()
        {
            var data = _iHomeCategoryService.GetHomeCategorySets();

            var parentCategoryIds = data.SelectMany(p => p.HomeCategories.Select(item => item.CategoryInfo.ParentCategoryId)).Distinct();
            var parentCategorys = CategoryApplication.GetCategories().Where(p => parentCategoryIds.Contains(p.Id));

            var categories = data.OrderBy(p => p.RowNumber).Select(p =>
            new
            {
                Top = p.HomeCategories.Where(item => item.Depth == 1).Select(item => new
                {
                    item.CategoryInfo.Icon,
                    item.CategoryInfo.Name,
                    item.CategoryId
                }),
                Group = p.HomeCategories.Where(item => item.Depth == 3).GroupBy(item => item.CategoryInfo.ParentCategoryId)
                .Select(item => new
                {
                    Parent = parentCategorys.Select(pc => new
                    {
                        pc.Id,
                        pc.Name
                    }).FirstOrDefault(pc => item.Key == pc.Id),
                    Items = item.Select(c => new
                    {
                        c.CategoryInfo.Id,
                        c.CategoryInfo.Name
                    })
                }),
                Brands = p.HomeBrand.Select(b => new
                {
                    b.Id,
                    Logo = Himall.Core.HimallIO.GetImagePath(b.Logo),
                    b.Name,
                    status = 1,
                    isrend = 0
                })
            });
            return Json(categories, true);
        }


        #region 商品分类设置

        public ActionResult HomeCategory()
        {
            var homeCategorySets = _iHomeCategoryService.GetHomeCategorySets().ToArray();
            var models = homeCategorySets.Select(item => new Models.HomeCategory()
            {
                RowNumber = item.RowNumber,
                TopCategoryNames = GetTopLevelCategoryNames(item.HomeCategories.Select(category => category.CategoryId)),
                AllCategoryIds = item.HomeCategories.Select(category => category.CategoryId)
            }).OrderBy(item => item.RowNumber);
            return View(models);
        }

        IEnumerable<string> GetTopLevelCategoryNames(IEnumerable<long> categoryIds)
        {
            var productCategoryService = _iCategoryService;
            var topLevelCateogries = productCategoryService.GetTopLevelCategories(categoryIds);
            return topLevelCateogries.Select(item => item.Name);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult SaveHomeCategory(string categoryIds, int rowNumber)
        {
            IEnumerable<long> ids;
            if (string.IsNullOrWhiteSpace(categoryIds))
                ids = new List<long>();
            else
                ids = categoryIds.Split(',').Select(item => long.Parse(item));
            _iHomeCategoryService.UpdateHomeCategorySet(rowNumber, ids);
            return Json(new
            {
                success = true
            });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult ChangeSequence(int oriRowNumber, int newRowNumber)
        {
            _iHomeCategoryService.UpdateHomeCategorySetSequence(oriRowNumber, newRowNumber);
            return Json(new
            {
                success = true
            });
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult SaveHomeTopic(int rowNumber, string url1, string imgUrl1, string url2, string imgUrl2)
        {
            var homeCategoryTopics = new HomeCategorySet.HomeCategoryTopic[] {
                 new HomeCategorySet.HomeCategoryTopic(){
                  Url = url1,
                  ImageUrl = imgUrl1
                 },
                 new HomeCategorySet.HomeCategoryTopic(){
                  ImageUrl = imgUrl2,
                   Url = url2
                 }
            };
            _iHomeCategoryService.UpdateHomeCategorySet(rowNumber, homeCategoryTopics);
            return Json(new
            {
                success = true
            });
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult GetHomeCategoryTopics(int rowNumber)
        {
            var homeCategorySet = _iHomeCategoryService.GetHomeCategorySet(rowNumber);
            int topicsCount = homeCategorySet.HomeCategoryTopics == null ? 0 : homeCategorySet.HomeCategoryTopics.Count();
            var topic1 = topicsCount > 0 ? homeCategorySet.HomeCategoryTopics.ElementAt(0) : null;
            var topic2 = topicsCount > 1 ? homeCategorySet.HomeCategoryTopics.ElementAt(1) : null;

            var model = new
            {
                imageUrl1 = topic1 == null ? "" : topic1.ImageUrl,
                url1 = topic1 == null ? "" : topic1.Url,
                imageUrl2 = topic2 == null ? "" : topic2.ImageUrl,
                url2 = topic2 == null ? "" : topic2.Url
            };

            return Json(model);
        }



        #endregion



        #region 楼层设置


        #region 基本信息

        public ActionResult HomeFloor()
        {
            var floors = _iFloorService.GetAllHomeFloors();
            var xxx = floors.ToList();
            var floorModels = floors.Select(item => new HomeFloor()
            {
                DisplaySequence = item.DisplaySequence,
                Enable = item.IsShow,
                Id = item.Id,
                Name = item.FloorName,
                StyleLevel = item.StyleLevel
            });
            return View(floorModels);
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult FloorChangeSequence(int oriRowNumber, int newRowNumber)
        {
            _iFloorService.UpdateHomeFloorSequence(oriRowNumber, newRowNumber);
            return Json(new
            {
                success = true
            });
        }


        [HttpPost]
        [UnAuthorize]
        public JsonResult FloorEnableDisplay(long id, bool enable)
        {
            _iFloorService.EnableHomeFloor(id, enable);
            return Json(new
            {
                success = true
            });
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult DeleteFloor(long id)
        {
            _iFloorService.DeleteHomeFloor(id);
            return Json(new
            {
                success = true
            });
        }



        public ActionResult AddHomeFloor(long id = 0)
        {
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
                homeFloor = new HomeFloorInfo();
            ViewBag.TopLevelCategories = _iCategoryService.GetCategoryByParentId(0);
            return View(homeFloor);
        }


        public ActionResult AddFloorChoose()
        {

            return View();
        }


        [UnAuthorize]
        [HttpPost]
        public JsonResult SaveHomeFloorBasicInfo(long id, string name, string categoryIds)
        {
            string msg = string.Empty;
            bool success = false;
            if (string.IsNullOrWhiteSpace(name))
                msg = "楼层名称不能为空";
            else if (name.Trim().Length > 4)
                msg = "楼层名称长度不能超过4个字";
            else
            {
                name = name.Trim();
                try
                {
                    IEnumerable<long> categoryIds_long = categoryIds.Split(',').Where(item => !string.IsNullOrWhiteSpace(item)).Select(item => long.Parse(item));
                    if (id > 0)
                        _iFloorService.UpdateFloorBasicInfo(id, name, categoryIds_long);
                    else
                    {
                        var basicInfo = _iFloorService.AddHomeFloorBasicInfo(name, categoryIds_long);
                        id = basicInfo.Id;
                    }
                    success = true;
                }
                catch (FormatException)
                {
                    msg = "商品分类编号有误";
                }

            }
            return Json(new
            {
                success = success,
                msg = msg,
                id = id
            });
        }

        #endregion


        #region 详细设置

        /// <summary>
        /// 楼层一
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 6; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "/",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;

                //填充文字链接
                homeFloorDetail.TextLinks = homeFloor.FloorTopicInfo.Where(item => item.TopicType == Position.Top).Select(item => new HomeFloorDetail.TextLink()
                {
                    Id = item.Id,
                    Name = item.TopicName,
                    Url = item.Url ?? "/"
                });

                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url ?? "/",
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);
            }


            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }

        /// <summary>
        /// 楼层2
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail2(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 11; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "/",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
                homeFloorDetail.StyleLevel = 1;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;
                homeFloorDetail.DefaultTabName = homeFloor.DefaultTabName;

                //填充文字链接
                homeFloorDetail.TextLinks = homeFloor.FloorTopicInfo.Where(item => item.TopicType == Position.Top).Select(item => new HomeFloorDetail.TextLink()
                {
                    Id = item.Id,
                    Name = item.TopicName,
                    Url = item.Url ?? "/"
                });

                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url ?? "/",
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);

                homeFloorDetail.Tabs = homeFloor.Himall_FloorTabls
                    .Where(item => item.FloorId == homeFloor.Id).OrderBy(p => p.Id)
                    .Select(item => new HomeFloorDetail.Tab()
                    {
                        Id = item.Id,
                        Detail = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id)
                        .Select(p => new HomeFloorDetail.ProductDetail()
                        {
                            Id = p.Id,
                            ProductId = p.ProductId
                        }),
                        Name = item.Name,
                        Count = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Count(),
                        Ids = ArrayToString(item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Select(p => p.ProductId).ToArray())
                    });
            }

            if (homeFloorDetail.Tabs == null)
            {
                homeFloorDetail.Tabs = new List<HomeFloorDetail.Tab>();
            }
            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }

        /// <summary>
        /// 楼层3
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail3(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 1; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "/",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
                homeFloorDetail.StyleLevel = 2;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;
                homeFloorDetail.DefaultTabName = homeFloor.DefaultTabName;

                //图片
                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url ?? "/",
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);


                //商品
                homeFloorDetail.ProductModules = homeFloor.FloorProductInfo
                    .Where(item => item.FloorId == homeFloor.Id).OrderBy(p => p.Id)
                    .Select(item => new HomeFloorDetail.ProductModule()
                    {
                        Id = item.Id,
                        price = item.ProductInfo.MinSalePrice,
                        ProductId = item.ProductId,
                        productImg = HimallIO.GetProductSizeImage(item.ProductInfo.ImagePath, 1, (int)ImageSize.Size_50),
                        productName = item.ProductInfo.ProductName,
                        Tab = item.Tab
                    });
            }

            if (homeFloorDetail.Tabs == null)
            {
                homeFloorDetail.Tabs = new List<HomeFloorDetail.Tab>();
            }
            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }

        /// <summary>
        /// 楼层四
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail4(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 10; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "/",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
                homeFloorDetail.StyleLevel = 3;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;

                //填充文字链接
                homeFloorDetail.TextLinks = homeFloor.FloorTopicInfo.Where(item => item.TopicType == Position.Top).Select(item => new HomeFloorDetail.TextLink()
                {
                    Id = item.Id,
                    Name = item.TopicName,
                    Url = item.Url ?? "/"
                });

                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url ?? "/",
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);
            }


            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }

        /// <summary>
        /// 默认楼层
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail5(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 12; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "/",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
                homeFloorDetail.StyleLevel = 1;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;
                homeFloorDetail.DefaultTabName = homeFloor.DefaultTabName;

                //填充文字链接
                homeFloorDetail.TextLinks = homeFloor.FloorTopicInfo.Where(item => item.TopicType == Position.Top).Select(item => new HomeFloorDetail.TextLink()
                {
                    Id = item.Id,
                    Name = item.TopicName,
                    Url = item.Url ?? "/"
                });

                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url ?? "/",
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);

                homeFloorDetail.Tabs = homeFloor.Himall_FloorTabls
                    .Where(item => item.FloorId == homeFloor.Id).OrderBy(p => p.Id)
                    .Select(item => new HomeFloorDetail.Tab()
                    {
                        Id = item.Id,
                        Detail = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id)
                        .Select(p => new HomeFloorDetail.ProductDetail()
                        {
                            Id = p.Id,
                            ProductId = p.ProductId
                        }),
                        Name = item.Name,
                        Count = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Count(),
                        Ids = ArrayToString(item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Select(p => p.ProductId).ToArray())
                    });
            }

            if (homeFloorDetail.Tabs == null)
            {
                homeFloorDetail.Tabs = new List<HomeFloorDetail.Tab>();
            }
            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }
        /// <summary>
        /// 楼层五
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail6(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 12; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "/",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
                homeFloorDetail.StyleLevel = 5;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;
                homeFloorDetail.DefaultTabName = homeFloor.DefaultTabName;

                //填充文字链接
                homeFloorDetail.TextLinks = homeFloor.FloorTopicInfo.Where(item => item.TopicType == Position.Top).Select(item => new HomeFloorDetail.TextLink()
                {
                    Id = item.Id,
                    Name = item.TopicName,
                    Url = item.Url ?? "/"
                });

                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        //Id = (long)item.TopicType,
                        Id = item.Id,
                        Name = item.Url ?? "/",
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);

                homeFloorDetail.Tabs = homeFloor.Himall_FloorTabls
                    .Where(item => item.FloorId == homeFloor.Id).OrderBy(p => p.Id)
                    .Select(item => new HomeFloorDetail.Tab()
                    {
                        Id = item.Id,
                        Detail = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id)
                        .Select(p => new HomeFloorDetail.ProductDetail()
                        {
                            Id = p.Id,
                            ProductId = p.ProductId
                        }),
                        Name = item.Name,
                        Count = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Count(),
                        Ids = ArrayToString(item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Select(p => p.ProductId).ToArray())
                    });
            }

            if (homeFloorDetail.Tabs == null)
            {
                homeFloorDetail.Tabs = new List<HomeFloorDetail.Tab>();
            }
            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }
        /// <summary>
        /// 楼层六
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail7(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 16; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "/",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
                homeFloorDetail.StyleLevel = 1;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;
                homeFloorDetail.DefaultTabName = homeFloor.DefaultTabName;

                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url ?? "/",
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);


                homeFloorDetail.Tabs = homeFloor.Himall_FloorTabls
                    .Where(item => item.FloorId == homeFloor.Id).OrderBy(p => p.Id)
                    .Select(item => new HomeFloorDetail.Tab()
                    {
                        Id = item.Id,
                        Detail = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id)
                        .Select(p => new HomeFloorDetail.ProductDetail()
                        {
                            Id = p.Id,
                            ProductId = p.ProductId
                        }),
                        Name = item.Name,
                        Count = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Count(),
                        Ids = ArrayToString(item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Select(p => p.ProductId).ToArray())
                    });
            }

            if (homeFloorDetail.Tabs == null)
            {
                homeFloorDetail.Tabs = new List<HomeFloorDetail.Tab>();
            }
            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }
        /// <summary>
        /// 楼层七
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail8(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 15; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "/",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
                homeFloorDetail.StyleLevel = 1;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;
                homeFloorDetail.DefaultTabName = homeFloor.DefaultTabName;

                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url ?? "/",
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);


                homeFloorDetail.Tabs = homeFloor.Himall_FloorTabls
                    .Where(item => item.FloorId == homeFloor.Id).OrderBy(p => p.Id)
                    .Select(item => new HomeFloorDetail.Tab()
                    {
                        Id = item.Id,
                        Detail = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id)
                        .Select(p => new HomeFloorDetail.ProductDetail()
                        {
                            Id = p.Id,
                            ProductId = p.ProductId
                        }),
                        Name = item.Name,
                        Count = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Count(),
                        Ids = ArrayToString(item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Select(p => p.ProductId).ToArray())
                    });
            }

            if (homeFloorDetail.Tabs == null)
            {
                homeFloorDetail.Tabs = new List<HomeFloorDetail.Tab>();
            }
            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail9(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 13; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "/",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
                homeFloorDetail.StyleLevel = 1;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;
                homeFloorDetail.DefaultTabName = homeFloor.DefaultTabName;

                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url ?? "/",
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);

                //填充文字链接
                homeFloorDetail.TextLinks = homeFloor.FloorTopicInfo.Where(item => item.TopicType == Position.Top).Select(item => new HomeFloorDetail.TextLink()
                {
                    Id = item.Id,
                    Name = item.TopicName,
                    Url = item.Url ?? "/"
                });
                homeFloorDetail.Tabs = homeFloor.Himall_FloorTabls
                    .Where(item => item.FloorId == homeFloor.Id).OrderBy(p => p.Id)
                    .Select(item => new HomeFloorDetail.Tab()
                    {
                        Id = item.Id,
                        Detail = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id)
                        .Select(p => new HomeFloorDetail.ProductDetail()
                        {
                            Id = p.Id,
                            ProductId = p.ProductId
                        }),
                        Name = item.Name,
                        Count = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Count(),
                        Ids = ArrayToString(item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Select(p => p.ProductId).ToArray())
                    });
            }

            if (homeFloorDetail.Tabs == null)
            {
                homeFloorDetail.Tabs = new List<HomeFloorDetail.Tab>();
            }
            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }

        /// 楼层3
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail10(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 1; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "/",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
                homeFloorDetail.CommodityStyle = 1;
                homeFloorDetail.DisplayMode = 1;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;
                homeFloorDetail.DefaultTabName = homeFloor.DefaultTabName;
                homeFloorDetail.CommodityStyle = homeFloor.CommodityStyle.HasValue ? homeFloor.CommodityStyle.Value : 1;
                homeFloorDetail.DisplayMode = homeFloor.DisplayMode.HasValue ? homeFloor.DisplayMode.Value : 1;

                //图片
                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url ?? "/",
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);


                //商品
                homeFloorDetail.ProductModules = homeFloor.FloorProductInfo
                    .Where(item => item.FloorId == homeFloor.Id).OrderBy(p => p.Id)
                    .Select(item => new HomeFloorDetail.ProductModule()
                    {
                        Id = item.Id,
                        price = item.ProductInfo.MinSalePrice,
                        ProductId = item.ProductId,
                        productImg = HimallIO.GetProductSizeImage(item.ProductInfo.ImagePath, 1, (int)ImageSize.Size_50),
                        productName = item.ProductInfo.ProductName,
                        Tab = item.Tab
                    });
            }

            if (homeFloorDetail.Tabs == null)
            {
                homeFloorDetail.Tabs = new List<HomeFloorDetail.Tab>();
            }
            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            //ViewBag.DisplayMode = 1;
            //ViewBag.CommodityStyle = 1;
            //ViewBag.DisplayMode = homeFloor == null ? 0 : homeFloor.DisplayMode;
            //ViewBag.CommodityStyle = homeFloor == null ? 0 : homeFloor.CommodityStyle;
            return View(homeFloorDetail);
        }
        /// <summary>
        /// 楼层四
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddHomeFloorDetail11(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 10; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "/",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
                homeFloorDetail.StyleLevel = 3;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;

                //填充文字链接
                homeFloorDetail.TextLinks = homeFloor.FloorTopicInfo.Where(item => item.TopicType == Position.Top).Select(item => new HomeFloorDetail.TextLink()
                {
                    Id = item.Id,
                    Name = item.TopicName,
                    Url = item.Url ?? "/"
                });

                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url ?? "/",
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);
            }


            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }

        public ActionResult AddHomeFloorDetail12(long id = 0)
        {
            var homeFloorDetail = new HomeFloorDetail()
            {
                Id = 0
            };
            var homeFloor = _iFloorService.GetHomeFloor(id);
            if (homeFloor == null)
            {
                homeFloor = new HomeFloorInfo();
                var ProductLinks = new List<HomeFloorDetail.TextLink>();
                for (int i = 0; i < 12; i++)
                {
                    ProductLinks.Add(new HomeFloorDetail.TextLink
                    {
                        Id = i,
                        Name = "/",
                        Url = ""
                    });
                }
                homeFloorDetail.ProductLinks = ProductLinks;
                homeFloorDetail.StyleLevel = 5;
            }
            else
            {
                homeFloorDetail.Id = homeFloor.Id;
                homeFloorDetail.DefaultTabName = homeFloor.DefaultTabName;

                //填充文字链接
                homeFloorDetail.TextLinks = homeFloor.FloorTopicInfo.Where(item => item.TopicType == Position.Top).Select(item => new HomeFloorDetail.TextLink()
                {
                    Id = item.Id,
                    Name = item.TopicName,
                    Url = item.Url ?? "/"
                });

                homeFloorDetail.ProductLinks = homeFloor.FloorTopicInfo
                    .Where(item => item.TopicType != Position.Top)
                    .Select(item => new HomeFloorDetail.TextLink()
                    {
                        Id = (long)item.TopicType,
                        Name = item.Url ?? "/",
                        Url = item.TopicImage,
                    }).OrderBy(i => i.Id);

                homeFloorDetail.Tabs = homeFloor.Himall_FloorTabls
                    .Where(item => item.FloorId == homeFloor.Id).OrderBy(p => p.Id)
                    .Select(item => new HomeFloorDetail.Tab()
                    {
                        Id = item.Id,
                        Detail = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id)
                        .Select(p => new HomeFloorDetail.ProductDetail()
                        {
                            Id = p.Id,
                            ProductId = p.ProductId
                        }),
                        Name = item.Name,
                        Count = item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Count(),
                        Ids = ArrayToString(item.Himall_FloorTablDetails.Where(detail => detail.TabId == item.Id).Select(p => p.ProductId).ToArray())
                    });
            }

            if (homeFloorDetail.Tabs == null)
            {
                homeFloorDetail.Tabs = new List<HomeFloorDetail.Tab>();
            }
            ViewBag.FloorName = homeFloor == null ? "" : homeFloor.FloorName;
            ViewBag.SubName = homeFloor == null ? "" : homeFloor.SubName;
            return View(homeFloorDetail);
        }

        private string ArrayToString(long[] array)
        {
            string ids = string.Empty;
            foreach (long id in array)
            {
                ids += id + ",";
            }
            return ids.Substring(0, ids.Length - 1);
        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult SaveHomeFloorDetail(string floorDetail, int tid)
        {
            var floorDetailObj = Newtonsoft.Json.JsonConvert.DeserializeObject<HomeFloorDetail>(floorDetail);

            var homeFloor = new Model.HomeFloorInfo()
            {
                Id = floorDetailObj.Id,
                FloorName = floorDetailObj.Name,
                SubName = floorDetailObj.SubName,
                DefaultTabName = floorDetailObj.DefaultTabName,
                StyleLevel = floorDetailObj.StyleLevel,
                FloorBrandInfo = floorDetailObj.Brands.Select(item => new FloorBrandInfo()
                {
                    BrandId = item.Id,
                    FloorId = floorDetailObj.Id,
                }).ToList(),
                FloorTopicInfo = floorDetailObj.TextLinks.Select(i => new FloorTopicInfo
                {
                    FloorId = floorDetailObj.Id,
                    TopicImage = "",
                    TopicName = i.Name,
                    Url = i.Url,
                    TopicType = Position.Top
                }).ToList(),

                CommodityStyle = floorDetailObj.CommodityStyle,
                DisplayMode = floorDetailObj.DisplayMode
            };
            var productLink = floorDetailObj.ProductLinks.Select(i => new FloorTopicInfo
            {
                FloorId = floorDetailObj.Id,
                TopicImage = String.IsNullOrWhiteSpace(i.Name) ? "" : i.Name,
                TopicName = "",
                Url = i.Url,
                TopicType = (Position)i.Id
            }).ToList();

            foreach (var item in productLink)
            {
                homeFloor.FloorTopicInfo.Add(item);
            }

            if (floorDetailObj.Tabs != null)
            {
                homeFloor.Himall_FloorTabls = floorDetailObj.Tabs.Select(item => new FloorTablsInfo()
                {
                    Id = item.Id,
                    Name = item.Name,
                    FloorId = floorDetailObj.Id,
                    Himall_FloorTablDetails = item.Detail.Select(d => new FloorTablDetailsInfo()
                    {
                        Id = d.Id,
                        TabId = item.Id,
                        ProductId = d.ProductId
                    }).ToList()
                }).ToList();
            }

            if (floorDetailObj.ProductModules != null)
            {
                //补充数据
                var productResult = floorDetailObj.ProductModules.ToList();
                var products = ProductManagerApplication.GetProductByIds(productResult.Select(a => a.ProductId));
                productResult.ForEach(a =>
                {
                    var proInfo = products.FirstOrDefault(p => p.Id == a.ProductId);
                    a.productImg = proInfo != null ? HimallIO.GetProductSizeImage(proInfo.ImagePath, 1, (int)ImageSize.Size_500) : "/images/default.png";
                    a.price = proInfo != null ? proInfo.MinSalePrice : 0;
                    a.marketPrice = proInfo != null ? proInfo.MarketPrice : 0;
                    a.productName = proInfo != null ? proInfo.ProductName : "";
                });
                //补充数据
                homeFloor.FloorProductInfo = floorDetailObj.ProductModules.Select(item => new FloorProductInfo()
                {
                    Id = item.Id,
                    FloorId = floorDetailObj.Id,
                    ProductId = item.ProductId,
                    Tab = item.Tab
                }).ToList();
            }

            _iFloorService.UpdateHomeFloorDetail(homeFloor);

            floorDetailObj.Id = homeFloor.Id;

            #region 返回之前处理图片路径为移动后的而非temp
            if (floorDetailObj.ProductLinks != null)
            {
                var imagePathUpdates = floorDetailObj.ProductLinks.ToList();
                foreach (var item in imagePathUpdates)
                {
                    item.Name = TransferImage(item.Name, tid, item.Id.ToString());
                }
            }
            #endregion

            return Json(new
            {
                Success = true,
                Data = floorDetailObj
            }, true);
        }

        string TransferImage(string sourceFile, int tid, string type)
        {
            if (!string.IsNullOrWhiteSpace(sourceFile) && !sourceFile.Contains("/Storage/Plat/PageSettings/Template/"))
            {
                //string newDir = "/Storage/Plat/PageSettings/HomeFloor/" + floorId + "/";
                string newDir = "/Storage/Plat/PageSettings/Template/" + tid + "/";//暂时将楼层图片和其他模板页图片放到一个目录下
                string DirUrl = Server.MapPath(newDir);
                if (!System.IO.Directory.Exists(DirUrl))
                {
                    System.IO.Directory.CreateDirectory(DirUrl);
                }

                string ext = sourceFile.Substring(sourceFile.LastIndexOf('.'));//得到扩展名
                string newName = "floor_" + type + "_" + System.DateTime.Now.ToString("yyyyMMddHHmmssffffff") + ext;//得到新的文件名


                if (!string.IsNullOrWhiteSpace(sourceFile))
                {
                    if (sourceFile.Contains("/temp/"))
                    {
                        string logoname = sourceFile.Substring(sourceFile.LastIndexOf('/') + 1);
                        string oldlogo = sourceFile.Substring(sourceFile.LastIndexOf("/temp"));
                        string newLogo = newDir + newName;
                        Core.HimallIO.CopyFile(oldlogo, newLogo, true);
                        sourceFile = newLogo;
                        return sourceFile;//返回新的文件路径
                    }
                    else if (sourceFile.Contains("/Storage/Plat/PageSettings/HomeFloor/0/"))//原来的图片也要处理掉
                    {
                        string logoname = sourceFile.Substring(sourceFile.LastIndexOf('/') + 1);
                        string oldlogo = sourceFile.Substring(sourceFile.LastIndexOf("/Storage/Plat/PageSettings/HomeFloor/0"));
                        string newLogo = newDir + newName;
                        Core.HimallIO.CopyFile(oldlogo, newLogo, true);
                        sourceFile = newLogo;
                        return sourceFile;//返回新的文件路径
                    }
                    else if (sourceFile.Contains("/Storage/"))
                    {
                        sourceFile = sourceFile.Substring(sourceFile.LastIndexOf("/Storage"));
                    }
                }
            }
            else if (sourceFile.Contains("/Storage/"))
            {
                sourceFile = sourceFile.Substring(sourceFile.LastIndexOf("/Storage"));
            }

            return sourceFile;
        }


        #endregion




        #endregion

        #region LOGO图片设置

        /// <summary>
        /// LOGO图片设置
        /// </summary>
        /// <param name="logo"></param>
        /// <returns></returns>
        [HttpPost]
        [UnAuthorize]
        public JsonResult SetLogo(string logo)
        {
            string image = logo;
            if (image.Contains("/temp/"))
            {
                string source = image.Substring(image.LastIndexOf("/temp"));
                string dest = @"/Storage/Plat/Site/";
                var ext = Path.GetExtension(source);

                image = dest + "logo" + ext;
                Core.HimallIO.CopyFile(source, image, true);
            }
            else if (image.Contains("/Storage/"))
            {
                image = image.Substring(image.LastIndexOf("/Storage/"));
            }
            _iSiteSettingService.SaveSetting("Logo", image);
            return Json(new
            {
                success = true,
                logo = HimallIO.GetImagePath(image)
            });
        }

        /// <summary>
        /// 设置广告
        /// </summary>
        /// <param name="img"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        [HttpPost]
        [UnAuthorize]
        public JsonResult SetAdvertisement(string img, string url, bool open)
        {
            img = MoveImages(img, "Advertisement", "advertisement");
            _iSiteSettingService.SaveSetting("AdvertisementImagePath", img);
            _iSiteSettingService.SaveSetting("AdvertisementUrl", url);
            _iSiteSettingService.SaveSetting("AdvertisementState", open);

            return Json(new
            {
                success = true,
                img = HimallIO.GetImagePath(img)
            });
        }

        /// <summary>
        /// 头部广告图设置
        /// </summary>
        /// <param name="logo"></param>
        /// <returns></returns>
        [HttpPost]
        [UnAuthorize]
        public JsonResult SetHeadArea(string img)
        {
            img = MoveImages(img, "HeadArea", "logo");
            _iSiteSettingService.SaveSetting("HeadArea", img);
            return Json(new
            {
                success = true,
                logo = HimallIO.GetImagePath(img)
            });
        }


        /// <summary>
        /// 转移LOGO图片
        /// </summary>
        /// <param name="id"></param>
        /// <param name="image"></param>
        /// <returns></returns>
		private string MoveImages(string image, string directoryName, string fileName)
        {
            //文件转移
            if (image.Contains("/temp/"))
            {
                string source = image.Substring(image.LastIndexOf("/temp"));
                string dest = @"/Storage/Plat/SmallProgapi/";
                var ext = Path.GetExtension(source);

                image = dest + directoryName + "/" + fileName + ext;
                Core.HimallIO.CopyFile(source, image, true);
            }
            else if (image.Contains("/Storage/"))
            {
                image = image.Substring(image.LastIndexOf("/Storage/"));
            }

            return image;
        }
        #endregion

        #region 底部服务图片

        [HttpPost]
        [UnAuthorize]
        public JsonResult SetBottomPic(string pic)
        {
            //文件转移
            if (pic.Contains("/temp/"))
            {
                string source = pic.Substring(pic.LastIndexOf("/temp"));
                string dest = @"/Storage/Plat/ImageAd/";
                var ext = Path.GetExtension(source);

                pic = dest + "PCBottomPic" + ext;
                Core.HimallIO.CopyFile(source, pic, true);
            }
            else if (pic.Contains("/Storage/"))
            {
                pic = pic.Substring(pic.LastIndexOf("/Storage/"));
            }

            _iSiteSettingService.SaveSetting("PCBottomPic", pic);
            return Json(new
            {
                success = true,
                pic = pic
            });
        }

        #endregion

        #region 关键字设置
        [HttpPost]
        [UnAuthorize]
        public JsonResult SetKeyWords(string keyword, string hotkeywords)
        {
            var sitesetting = _iSiteSettingService;
            var setting = sitesetting.GetSiteSettings();
            setting.Hotkeywords = hotkeywords;
            setting.Keyword = keyword;
            sitesetting.SetSiteSettings(setting);
            return Json(new Result
            {
                success = true
            });
        }
        #endregion

        public ViewResult Limittime()
        {
            var setting = _iSiteSettingService.GetSiteSettings();
            ViewBag.Limittime = setting.Limittime;
            return View();
        }

        /// <summary>
        /// 设置首页是否显示限时购
        /// </summary>
        /// <param name="Limittime">是否显示</param>
        /// <returns></returns>
        [HttpPost]
        [UnAuthorize]
        public JsonResult SetLimittime(bool Limittime)
        {
            var sitesetting = _iSiteSettingService;
            var setting = sitesetting.GetSiteSettings();
            setting.Limittime = Limittime;
            sitesetting.SetSiteSettings(setting);
            return Json(new Result
            {
                success = true
            });
        }

        #region 页脚设置

        [ValidateInput(false)]
        [UnAuthorize]
        [HttpPost]
        public JsonResult SetPageFoot(string content)
        {
            content = HTMLProcess(content);
            _iSiteSettingService.SaveSetting("PageFoot", content);
            return Json(new
            {
                success = true
            });
        }


        public ActionResult SetPageFoot()
        {
            var settings = _iSiteSettingService.GetSiteSettings();
            ViewBag.PageFoot = settings.PageFoot;
            return View();
        }


        /// <summary>
        /// 转移外站图片，去除script脚本
        /// </summary>
        /// <param name="content">html内容</param>
        /// <param name="id"></param>
        /// <returns></returns>
        string HTMLProcess(string content)
        {
            // string imageRealtivePath = details;
            //  content = Core.Helper.HtmlContentHelper.TransferToLocalImage(content, "/", imageRealtivePath, Core.HimallIO.GetImagePath(imageRealtivePath) + "/");
            // content = Core.Helper.HtmlContentHelper.RemoveScriptsAndStyles(content);
            string imageRealtivePath = "/Storage/Plat/PageSettings/PageFoot";
            string imageDirectory = Core.Helper.IOHelper.GetMapPath(imageRealtivePath);
            content = Core.Helper.HtmlContentHelper.TransferToLocalImage(content, "/", imageRealtivePath, Core.HimallIO.GetImagePath(imageRealtivePath) + "/");
            content = Core.Helper.HtmlContentHelper.RemoveScriptsAndStyles(content);
            return content;
        }



        #endregion


        #region  主题设置
        public ActionResult SetTheme()
        {
            return View(ThemeApplication.getTheme());
        }

        /// <summary>
        /// 修改主题设置
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <param name="typeId">0、默认；1、自定义主题</param>
        /// <param name="MainColor">主色</param>
        /// <param name="SecondaryColor">商城辅色</param>
        /// <param name="WritingColor">字体颜色</param>
        /// <param name="FrameColor">边框颜色</param>
        /// <param name="ClassifiedsColor">边框栏颜色</param>
        /// <returns></returns>
        public JsonResult updateTheme(long id, int typeId, string MainColor = "", string SecondaryColor = "", string WritingColor = "", string FrameColor = "", string ClassifiedsColor = "")
        {
            Theme mVTheme = new Theme()
            {
                ThemeId = id,
                TypeId = (Himall.CommonModel.ThemeType)typeId,
                MainColor = MainColor,
                SecondaryColor = SecondaryColor,
                WritingColor = WritingColor,
                FrameColor = FrameColor,
                ClassifiedsColor = ClassifiedsColor
            };

            ThemeApplication.SetTheme(mVTheme);

            return Json(new
            {
                status = 1
            });
        }
        #endregion

        public static T ParseFormJson<T>(string szJson)
        {
            T obj = Activator.CreateInstance<T>();
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(szJson)))
            {
                DataContractJsonSerializer dcj = new DataContractJsonSerializer(typeof(T));
                return (T)dcj.ReadObject(ms);
            }
        }

        public static string ObjectToJson<T>(Object jsonObject, Encoding encoding)
        {
            string result = String.Empty;
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                serializer.WriteObject(ms, jsonObject);
                result = encoding.GetString(ms.ToArray());
            }
            return result;
        }

        private string FilterScript(string content, string variableName)
        {
            string regexstr = string.Format(@"<script[^>]*>var {0}=([\s\S](?!<script))*?</script>", variableName);
            return Regex.Replace(content, regexstr, string.Empty, RegexOptions.IgnoreCase).Trim();

        }

        [HttpPost]
        [UnAuthorize]
        public JsonResult GetSpecial(int page, int rows, string specialName)
        {
            var topicList = ObjectContainer.Current.Resolve<ITopicService>().GetTopics(new TopicQuery
            {
                PageNo = page,
                PageSize = rows,
                PlatformType = Core.PlatformType.PC,
                Name = specialName
            });
            var data = topicList.Models.Select(p => new TopicInfo()
            {
                Name = p.Name,
                Tags = p.Tags,
                Id = p.Id
            });
            DataGridModel<TopicInfo> dataGrid = new DataGridModel<TopicInfo>() { rows = data, total = topicList.Total };
            return Json(dataGrid);
        }

        public ActionResult CategorySelect()
        {
            var ICategory = _iCategoryService;
            var categories = ICategory.GetCategories().ToList();
            var firstLevel = categories.Where(c => c.Depth == 1).OrderBy(c => c.DisplaySequence);
            List<CategoryModel> list = new List<CategoryModel>();
            foreach (var item in firstLevel)
            {
                list.Add(new CategoryModel(item));
                AddChildCategory(list, categories, item.Id);
            }
            return View(list);
        }
        void AddChildCategory(List<CategoryModel> list, List<CategoryInfo> categories, long pid)
        {
            var childCategories = categories.Where(c => c.ParentCategoryId == pid).OrderBy(c => c.DisplaySequence);
            if (childCategories.Count() == 0)
                return;
            foreach (var item in childCategories)
            {
                list.Add(new CategoryModel(item));
                AddChildCategory(list, categories, item.Id);
            }
        }
        public ActionResult Theme()
        {
            return View(ThemeApplication.getTheme());
        }

        /// <summary>
        /// 模板页加载时获取当前模板配置的主题配色
        /// </summary>
        /// <returns></returns>
        public JsonResult GetThemeSetting(int tid)
        {
            string path = this.Server.MapPath(_themesettings);
            //这里要改为读取当前模板配置的主题配色方案，而不是取公共的，当然一个模板只会对应一个配色方案
            if (System.IO.File.Exists(path))
            {
                string currentTheme = System.IO.File.ReadAllText(path);//读取当前模板应用的主题配色
                List<ThemeSetting> curThemeObjs = ParseFormJson<List<ThemeSetting>>(currentTheme);
                if (curThemeObjs != null && curThemeObjs.Count > 0)
                {
                    var info = curThemeObjs.FirstOrDefault(a => a.templateId == tid);
                    if (info != null)
                    {
                        return Json(info, true);
                    }
                }
            }
            //Theme curThemeObj = ThemeApplication.getTheme();
            //if (null != curThemeObj)
            //{
            //    return Json(curThemeObj, true);
            //}
            return Json(null, true);
        }

        private string MatchComInfo(string html, string parttern, string groupName)
        {
            string returnval = "";
            try
            {
                Regex regex = new Regex(parttern);
                MatchCollection matches = regex.Matches(html);
                if (matches.Count > 0)
                {
                    returnval = matches[0].Groups[groupName].Value;
                }
                return returnval.Replace("\r", "").Replace("\n", "").Replace("\t", "").Trim();
            }
            catch { }
            return "";
        }
    }
}
