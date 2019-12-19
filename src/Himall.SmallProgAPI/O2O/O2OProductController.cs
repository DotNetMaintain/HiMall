using Himall.Application;
using Himall.CommonModel;
using Himall.Core;
using Himall.DTO;
using Himall.DTO.QueryModel;
using Himall.IServices;
using Himall.Model;
using Himall.SmallProgAPI.Model;
using Himall.Web.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Http.Results;

namespace Himall.SmallProgAPI
{
    public class O2OProductController : BaseO2OApiController
    {

        #region 搜索商品
        public JsonResult<Result<dynamic>> GetProductList(long shopId, long shopBranchId, string keyWords = "", long? productId = null, long? shopCategoryId = null, long? categoryId = null, int type = 0, int pageNo = 1, int pageSize = 10)
        {
            CheckOpenStore();
            if (shopId <= 0)
            {
                throw new HimallApiException(ApiErrorCode.Parameter_Error, "shopId");
            }
            if (shopBranchId <= 0)
            {
                throw new HimallApiException(ApiErrorCode.Parameter_Error, "shopBranchId");
            }
            ShopBranchProductQuery query = new ShopBranchProductQuery();
            query.PageSize = pageSize;
            query.PageNo = pageNo;
            query.KeyWords = keyWords;
            query.ShopId = shopId;
            query.shopBranchId = shopBranchId;
            query.RproductId = productId;
            query.ShopBranchProductStatus = ShopBranchSkuStatus.Normal;
            query.OrderKey = 5;
            if (shopCategoryId.HasValue && shopCategoryId > 0)
            {
                query.ShopCategoryId = shopCategoryId;
            }
            if (categoryId.HasValue && categoryId > 0)
            {
                query.CategoryId = categoryId;
            }
            var _iBranchCartService = ServiceApplication.Create<IBranchCartService>();
            var _iLimitTimeBuyService = ServiceApplication.Create<ILimitTimeBuyService>();

            var dtNow = DateTime.Now;
            var pageModel = ShopBranchApplication.GetShopBranchProductsMonth(query, dtNow.AddDays(-30).Date, dtNow);

            #region 置顶商品
            if (productId.HasValue && productId > 0 && pageNo == 1)
            {
                if (type == 1)
                    query.ShopCategoryId = null;
                query.RproductId = null;
                query.productId = productId;
                query.ShopCategoryId = null;
                query.CategoryId = null;
                var topModel = ShopBranchApplication.GetShopBranchProductsMonth(query, dtNow.AddDays(-30).Date, dtNow);
                if (topModel.Models.Count() > 0)
                {
                    pageModel.Models.Insert(0, topModel.Models.FirstOrDefault());
                }
            }
            #endregion

            //获取门店活动
            var shopBranchs = ShopBranchApplication.GetShopBranchById(shopBranchId);

            if (pageModel.Models != null && pageModel.Models.Count > 0)
            {
                #region 处理商品 官方自营店会员折扣价。
                if (CurrentUser != null)
                {
                    var shopInfo = ShopApplication.GetShop(query.ShopId.Value);
                    if (shopInfo != null && shopInfo.IsSelf)//当前商家是否是官方自营店
                    {
                        decimal discount = 1M;
                        discount = CurrentUser.MemberDiscount;
                        foreach (var item in pageModel.Models)
                        {
                            item.MinSalePrice = Math.Round(item.MinSalePrice * discount, 2);
                        }
                    }
                    ShoppingCartInfo cartInfo = _iBranchCartService.GetCart(CurrentUser.Id, shopBranchId);//获取购物车数据
                    foreach (var item in pageModel.Models)
                    {
                        item.Quantity = 0;
                        if (cartInfo != null)
                        {
                            item.Quantity = cartInfo.Items.Where(d => d.ProductId == item.Id && d.ShopBranchId == shopBranchId).Sum(d => d.Quantity);
                        }
                    }
                }
                #endregion
            }

            var product = pageModel.Models.ToList().Select(item =>
            {
                var comcount = CommentApplication.GetProductCommentCountAggregate(productId: item.Id, shopBranchId: shopBranchId);
                return new
                {
                    Id = item.Id,
                    ProductName = item.ProductName,
                    CategoryName = item.Himall_Categories.Name,
                    MeasureUnit = item.MeasureUnit,
                    MinSalePrice = item.MinSalePrice.ToString("f2"),
                    SaleCounts = item.SaleCounts,//销量统计没有考虑订单支付完成。
                    MarketPrice = item.MarketPrice,
                    HasSku = item.HasSKU,
                    Quantity = item.Quantity.HasValue ? item.Quantity.Value : 0,
                    IsTop = item.Id == productId,
                    DefaultImage = Core.HimallIO.GetRomoteProductSizeImage(item.RelativePath, 1, (int)Himall.CommonModel.ImageSize.Size_350),
                    HighCommentCount = comcount.HighComment,
                };
            }).OrderByDescending(d => d.IsTop).ToList();

            return JsonResult<dynamic>(new
            {
                Products = product,
                Total = pageModel.Total
            });
        }
        #endregion

        #region 商品SKU
        /// <summary>
        /// 根据商品Id获取商品规格
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult<Result<dynamic>> GetProductSkuInfo(long id, long shopBranchId)
        {
            var _iProductService = ServiceApplication.Create<IProductService>();
            var _iBranchCartService = ServiceApplication.Create<IBranchCartService>();
            var _iTypeService = ServiceApplication.Create<ITypeService>();
            if (id <= 0)
            {
                throw new HimallApiException(ApiErrorCode.Parameter_Error, "id");
            }
            if (shopBranchId <= 0)
            {
                throw new HimallApiException(ApiErrorCode.Parameter_Error, "shopBranchId");
            }
            var product = _iProductService.GetProduct(id);

            decimal discount = 1M;
            if (CurrentUser != null)
            {
                discount = CurrentUser.MemberDiscount;
            }

            var skuArray = new List<ProductSKUModel>();
            object defaultsku = new object();
            int activetype = 0;
            string skuId = "", skucode = "", imageUrl = "";
            long weight = 0, stock = 0;
            decimal SalePrice = 0;
            ShoppingCartInfo cartInfo = null;
            long userId = 0;
            if (CurrentUser != null)
            {
                cartInfo = _iBranchCartService.GetCart(CurrentUser.Id, shopBranchId);//获取购物车数据
                userId = CurrentUser.Id;
            }

            var shopBranchInfo = ShopBranchApplication.GetShopBranchById(shopBranchId);
            var branchskuList = ShopBranchApplication.GetSkus(shopBranchInfo.ShopId, new List<long> { shopBranchId });
            var shopInfo = ShopApplication.GetShop(product.ShopId);
            var shopcartinfo = new BranchCartHelper().GetCart(userId, shopBranchId);
            foreach (var sku in product.SKUInfo)
            {
                decimal price = 1M;
                if (shopInfo.IsSelf)
                {
                    price = sku.SalePrice * discount;
                }
                else
                {
                    price = sku.SalePrice;
                }
                if (branchskuList.Count(x => x.SkuId == sku.Id && x.Stock > 0) > 0)
                {
                    var skuCartNumber = 0;
                    if (shopcartinfo != null && shopcartinfo.Items != null && shopcartinfo.Items.Count() > 0)
                    {
                        var _tmp = shopcartinfo.Items.FirstOrDefault(x => x.SkuId == sku.Id);
                        if (_tmp != null)
                        {
                            skuCartNumber = _tmp.Quantity;
                        }
                    }
                    skuArray.Add(new ProductSKUModel
                    {
                        Price = price,
                        SkuId = sku.Id,
                        Stock = branchskuList.FirstOrDefault(x => x.SkuId == sku.Id).Stock
                        //cartCount = (shopcartinfo == null || shopcartinfo.Items.Count() == 0) ? 0 : shopcartinfo.Items.FirstOrDefault(x => x.SkuId == sku.Id) == null ? 0 : shopcartinfo.Items.FirstOrDefault(x => x.SkuId == sku.Id).Quantity
                    });
                }

                //var price = sku.SalePrice * discount;
                //ProductSKUModel skuMode = new ProductSKUModel
                //{
                //    Price = sku.SalePrice,
                //    SkuId = sku.Id,
                //    Stock = (int)sku.Stock
                //};
                //if (limitBuy != null)
                //{
                //    activetype = 1;
                //    var limitSku = ServiceProvider.Instance<ILimitTimeBuyService>.Create.Get(limitBuy.Id);
                //    var limitSkuItem = limitSku.Details.Where(r => r.SkuId.Equals(sku.Id)).FirstOrDefault();
                //    if (limitSkuItem != null)
                //        skuMode.Price = limitSkuItem.Price;
                //}
                //skuArray.Add(skuMode);
            }

            ProductTypeInfo typeInfo = _iTypeService.GetType(product.TypeId);
            string colorAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.ColorAlias)) ? SpecificationType.Color.ToDescription() : typeInfo.ColorAlias;
            string sizeAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.SizeAlias)) ? SpecificationType.Size.ToDescription() : typeInfo.SizeAlias;
            string versionAlias = (typeInfo == null || string.IsNullOrEmpty(typeInfo.VersionAlias)) ? SpecificationType.Version.ToDescription() : typeInfo.VersionAlias;

            List<object> SkuItemList = new List<object>();
            List<object> Skus = new List<object>();
            if (product.SKUInfo != null && product.SKUInfo.Count() > 0)
            {
                #region 颜色
                long colorId = 0, sizeId = 0, versionId = 0;
                List<object> colorAttributeValue = new List<object>();
                List<string> listcolor = new List<string>();
                foreach (var sku in product.SKUInfo)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 0)
                    {
                        if (long.TryParse(specs[1], out colorId)) { }//相同颜色规格累加对应值
                        if (colorId != 0)
                        {
                            if (!listcolor.Contains(sku.Color))
                            {
                                //var c = product.SKUInfo.Where(s => s.Color.Equals(sku.Color)).Sum(s => s.Stock);
                                var colorvalue = new
                                {
                                    ValueId = colorId,
                                    Value = sku.Color,
                                    UseAttributeImage = true,
                                    ImageUrl = Himall.Core.HimallIO.GetRomoteImagePath(sku.ShowPic)
                                };
                                listcolor.Add(sku.Color);
                                colorAttributeValue.Add(colorvalue);
                            }
                        }
                    }
                }
                var color = new
                {
                    AttributeId = product.TypeId,
                    AttributeName = colorAlias,
                    AttributeValue = colorAttributeValue,
                    AttributeIndex = 0,
                };
                if (colorId > 0)
                {
                    SkuItemList.Add(color);
                }
                #endregion
                #region 容量
                List<object> sizeAttributeValue = new List<object>();
                List<string> listsize = new List<string>();
                foreach (var sku in product.SKUInfo)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 1)
                    {
                        if (long.TryParse(specs[2], out sizeId)) { }
                        if (sizeId != 0)
                        {
                            if (!listsize.Contains(sku.Size))
                            {
                                //var ss = product.SKUInfo.Where(s => s.Size.Equals(sku.Size)).Sum(s1 => s1.Stock);
                                var sizeValue = new
                                {
                                    ValueId = sizeId,
                                    Value = sku.Size,
                                    UseAttributeImage = false,
                                    ImageUrl = Himall.Core.HimallIO.GetRomoteImagePath(sku.ShowPic)
                                };
                                listsize.Add(sku.Size);
                                sizeAttributeValue.Add(sizeValue);
                            }
                        }
                    }
                }
                var size = new
                {
                    AttributeId = product.TypeId,
                    AttributeName = sizeAlias,
                    AttributeValue = sizeAttributeValue,
                    AttributeIndex = 1,
                };
                if (sizeId > 0)
                {
                    SkuItemList.Add(size);
                }
                #endregion
                #region 规格
                List<object> versionAttributeValue = new List<object>();
                List<string> listversion = new List<string>();
                foreach (var sku in product.SKUInfo)
                {
                    var specs = sku.Id.Split('_');
                    if (specs.Count() > 2)
                    {
                        if (long.TryParse(specs[3], out versionId)) { }
                        if (versionId != 0)
                        {
                            if (!listversion.Contains(sku.Version))
                            {
                                //var v = product.SKUInfo.Where(s => s.Version.Equals(sku.Version));
                                var versionValue = new
                                {
                                    ValueId = versionId,
                                    Value = sku.Version,
                                    UseAttributeImage = false,
                                    ImageUrl = Himall.Core.HimallIO.GetRomoteImagePath(sku.ShowPic)
                                };
                                listversion.Add(sku.Version);
                                versionAttributeValue.Add(versionValue);
                            }
                        }
                    }
                }
                var version = new
                {
                    AttributeId = product.TypeId,
                    AttributeName = versionAlias,
                    AttributeValue = versionAttributeValue,
                    AttributeIndex = 2,
                };
                if (versionId > 0)
                {
                    SkuItemList.Add(version);
                }
                #endregion
                #region Sku值
                foreach (var sku in product.SKUInfo)
                {
                    var prosku = new
                    {
                        SkuId = sku.Id,
                        SKU = sku.Sku,
                        Weight = product.Weight,
                        //Stock = sku.Stock,
                        Stock = branchskuList.FirstOrDefault(x => x.SkuId == sku.Id).Stock,
                        WarningStock = sku.SafeStock,
                        SalePrice = sku.SalePrice.ToString("0.##"),
                        CartQuantity = cartInfo != null ? cartInfo.Items.Where(d => d.SkuId == sku.Id && d.ShopBranchId == shopBranchId).Sum(d => d.Quantity) : 0,
                        ImageUrl = Core.HimallIO.GetRomoteProductSizeImage(sku.ShowPic, 1, (int)ImageSize.Size_350)
                    };
                    Skus.Add(prosku);
                }
                defaultsku = Skus[0];
                #endregion
            }
            return JsonResult<dynamic>(new
            {
                ProductId = id,
                ProductName = product.ProductName,
                ImageUrl = Core.HimallIO.GetRomoteProductSizeImage(product.ImagePath, 1, (int)ImageSize.Size_350), //GetImageFullPath(model.SubmitOrderImg),
                Stock = skuArray.Sum(s => s.Stock),// skus.Sum(s => s.Stock),
                                                   //ActivityUrl = activetype,
                SkuItems = SkuItemList,
                Skus = Skus,
                DefaultSku = defaultsku
            });
        }

        /// <summary>
        /// 获取门店商品规格
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="bid"></param>
        /// <returns></returns>
        public JsonResult<Result<List<ProductSKUModel>>> GetSKUInfo(long pId, long bid)
        {
            var _iProductService = ServiceApplication.Create<IProductService>();
            var _iShopBranchService = ServiceApplication.Create<IShopBranchService>();
            var _iBranchCartService = ServiceApplication.Create<IBranchCartService>();
            var product = _iProductService.GetProduct(pId);
            var shopBranchInfo = _iShopBranchService.GetShopBranchById(bid);
            var branchskuList = _iShopBranchService.GetSkus(shopBranchInfo.ShopId, new List<long> { bid });

            ShoppingCartInfo memberCartInfo = null;
            decimal discount = 1M;
            if (CurrentUser != null)
            {
                //如果已登陆取购物车数据
                memberCartInfo = _iBranchCartService.GetCart(CurrentUser.Id, bid);
                discount = CurrentUser.MemberDiscount;
            }
            var shopInfo = ShopApplication.GetShop(product.ShopId);

            var skuArray = new List<ProductSKUModel>();
            foreach (var sku in product.SKUInfo.Where(s => s.Stock > 0))
            {
                decimal price = 1M;
                if (shopInfo.IsSelf)
                {
                    price = sku.SalePrice * discount;
                }
                else
                {
                    price = sku.SalePrice;
                }
                if (branchskuList.Count(x => x.SkuId == sku.Id && x.Stock > 0) > 0)
                {
                    skuArray.Add(new ProductSKUModel
                    {
                        Price = price,
                        SkuId = sku.Id,
                        Stock = branchskuList.FirstOrDefault(x => x.SkuId == sku.Id).Stock,
                        cartCount = (memberCartInfo == null || memberCartInfo.Items.Count() == 0) ? 0 : memberCartInfo.Items.FirstOrDefault(x => x.SkuId == sku.Id) == null ? 0 : memberCartInfo.Items.FirstOrDefault(x => x.SkuId == sku.Id).Quantity
                    });
                }
            }
            return JsonResult(skuArray);
        }
        #endregion


        /// <summary>
        /// 添加商品评论（评价送积分）
        /// </summary>
        public JsonResult<Result<int>> GetAddProductReview(string openId, string DataJson)
        {
            CheckUserLogin();
            if (!string.IsNullOrEmpty(DataJson))
            {
                bool result = false;
                List<OrderCommentModel> orderComment = DataJson.FromJSON<List<OrderCommentModel>>();
                if (orderComment != null)
                {
                    List<ProductComment> list = new List<ProductComment>();
                    string orderIds = "";
                    foreach (var item in orderComment)
                    {
                        OrderCommentModel ordercom = new OrderCommentModel();
                        ordercom.ReviewDate = DateTime.Now;
                        ordercom.UserId = CurrentUser.Id;
                        ordercom.UserName = CurrentUser.UserName;
                        ordercom.UserEmail = CurrentUser.Email;
                        ordercom.OrderId = item.OrderId;
                        if (!orderIds.Contains(item.OrderId))
                        {
                            AddOrderComment(ordercom);//添加订单评价（订单评价只一次）
                            orderIds += item.OrderId + ",";
                        }

                        var model = new ProductComment();

                        var OrderInfo = ServiceApplication.Create<IOrderService>().GetOrderItemsByOrderId(long.Parse(item.OrderId)).Where(d => d.ProductId == item.ProductId).FirstOrDefault();
                        if (OrderInfo != null)
                        {
                            model.ReviewDate = DateTime.Now;
                            model.ReviewContent = item.ReviewText;
                            model.UserId = CurrentUser.Id;
                            model.UserName = CurrentUser.UserName;
                            model.Email = CurrentUser.Email;
                            model.SubOrderId = OrderInfo.Id;//订单明细Id
                            model.ReviewMark = item.Score;
                            model.ProductId = item.ProductId;
                            model.Images = new List<ProductCommentImage>();
                            foreach (var img in item.ImageUrl1.Split(','))
                            {
                                var p = new ProductCommentImage();

                                p.CommentType = 0;//0代表默认的表示评论的图片
                                p.CommentImage = Core.HimallIO.GetImagePath(img);
                                if (!string.IsNullOrEmpty(p.CommentImage))
                                {
                                    model.Images.Add(p);
                                }
                            }
                            list.Add(model);
                        }
                        result = true;
                    }
                    CommentApplication.Add(list);
                }
                if (result)
                {
                    return JsonResult<int>(msg: "评价成功");
                }
                else
                {
                    return Json(ErrorResult<int>("评价失败"));
                }
            }
            return Json(ErrorResult<int>("参数错误"));
        }

        /// <summary>
        /// 增加订单评论
        /// </summary>
        /// <param name="comment"></param>
        void AddOrderComment(OrderCommentModel comment)
        {
            TradeCommentApplication.Add(new OrderComment()
            {
                OrderId = long.Parse(comment.OrderId),
                DeliveryMark = 5,//物流评价
                ServiceMark = 5,//服务评价
                PackMark = 5,//包装评价
                UserId = comment.UserId,
                CommentDate = comment.ReviewDate,
                UserName = comment.UserName
            });
        }
    }
}
