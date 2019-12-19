﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Himall.Entity
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using Himall.Model;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class Entities : DbContext
    {
        public Entities()
            : base("name=Entities")
        {
            ShoppingCartItemInfo = Set<ShoppingCartItemInfo>();
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<CategoryInfo> CategoryInfo { get; set; }
        public virtual DbSet<AttributeInfo> AttributeInfo { get; set; }
        public virtual DbSet<AttributeValueInfo> AttributeValueInfo { get; set; }
        public virtual DbSet<BrandInfo> BrandInfo { get; set; }
        public virtual DbSet<ProductConsultationInfo> ProductConsultationInfo { get; set; }
        public virtual DbSet<ProductInfo> ProductInfo { get; set; }
        public virtual DbSet<SellerSpecificationValueInfo> SellerSpecificationValueInfo { get; set; }
        public virtual DbSet<SpecificationValueInfo> SpecificationValueInfo { get; set; }
        public virtual DbSet<TypeBrandInfo> TypeBrandInfo { get; set; }
        public virtual DbSet<ProductTypeInfo> ProductTypeInfo { get; set; }
        public virtual DbSet<ProductAttributeInfo> ProductAttributeInfo { get; set; }
        public virtual DbSet<ProductDescriptionInfo> ProductDescriptionInfo { get; set; }
        public virtual DbSet<ProductDescriptionTemplateInfo> ProductDescriptionTemplateInfo { get; set; }
        public virtual DbSet<SKUInfo> SKUInfo { get; set; }
        public virtual DbSet<AccountInfo> AccountInfo { get; set; }
        public virtual DbSet<ArticleCategoryInfo> ArticleCategoryInfo { get; set; }
        public virtual DbSet<ArticleInfo> ArticleInfo { get; set; }
        public virtual DbSet<BannerInfo> BannerInfo { get; set; }
        public virtual DbSet<CustomerServiceInfo> CustomerServiceInfo { get; set; }
        public virtual DbSet<FavoriteInfo> FavoriteInfo { get; set; }
        public virtual DbSet<FloorBrandInfo> FloorBrandInfo { get; set; }
        public virtual DbSet<FloorCategoryInfo> FloorCategoryInfo { get; set; }
        public virtual DbSet<FloorProductInfo> FloorProductInfo { get; set; }
        public virtual DbSet<FloorTopicInfo> FloorTopicInfo { get; set; }
        public virtual DbSet<HandSlideAdInfo> HandSlideAdInfo { get; set; }
        public virtual DbSet<HomeCategoryInfo> HomeCategoryInfo { get; set; }
        public virtual DbSet<HomeFloorInfo> HomeFloorInfo { get; set; }
        public virtual DbSet<ImageAdInfo> ImageAdInfo { get; set; }
        public virtual DbSet<LogInfo> LogInfo { get; set; }
        public virtual DbSet<ManagerInfo> ManagerInfo { get; set; }
        public virtual DbSet<MemberOpenIdInfo> MemberOpenIdInfo { get; set; }
        public virtual DbSet<UserMemberInfo> UserMemberInfo { get; set; }
        public virtual DbSet<ModuleProductInfo> ModuleProductInfo { get; set; }
        public virtual DbSet<OrderComplaintInfo> OrderComplaintInfo { get; set; }
        public virtual DbSet<OrderItemInfo> OrderItemInfo { get; set; }
        public virtual DbSet<OrderOperationLogInfo> OrderOperationLogInfo { get; set; }
        public virtual DbSet<OrderRefundInfo> OrderRefundInfo { get; set; }
        public virtual DbSet<ProductShopCategoryInfo> ProductShopCategoryInfo { get; set; }
        public virtual DbSet<RolePrivilegeInfo> RolePrivilegeInfo { get; set; }
        public virtual DbSet<RoleInfo> RoleInfo { get; set; }
        public virtual DbSet<ShippingAddressInfo> ShippingAddressInfo { get; set; }
        public virtual DbSet<ShopCategoryInfo> ShopCategoryInfo { get; set; }
        public virtual DbSet<ShopGradeInfo> ShopGradeInfo { get; set; }
        internal virtual DbSet<ShoppingCartItemInfo> ShoppingCartItemInfo { get; set; }
        public virtual DbSet<SiteSettingsInfo> SiteSettingsInfo { get; set; }
        public virtual DbSet<SlideAdInfo> SlideAdInfo { get; set; }
        public virtual DbSet<TopicModuleInfo> TopicModuleInfo { get; set; }
        public virtual DbSet<TopicInfo> TopicInfo { get; set; }
        public virtual DbSet<ProductCommentInfo> ProductCommentInfo { get; set; }
        public virtual DbSet<OrderCommentInfo> OrderCommentInfo { get; set; }
        public virtual DbSet<BusinessCategoryInfo> BusinessCategoryInfo { get; set; }
        public virtual DbSet<AccountDetailInfo> AccountDetailInfo { get; set; }
        public virtual DbSet<HomeCategoryRowInfo> HomeCategoryRowInfo { get; set; }
        public virtual DbSet<ShopHomeModuleProductInfo> ShopHomeModuleProductInfo { get; set; }
        public virtual DbSet<ShopHomeModuleInfo> ShopHomeModuleInfo { get; set; }
        public virtual DbSet<ProductVistiInfo> ProductVistiInfo { get; set; }
        public virtual DbSet<ShopVistiInfo> ShopVistiInfo { get; set; }
        public virtual DbSet<FavoriteShopInfo> FavoriteShopInfo { get; set; }
        public virtual DbSet<BrowsingHistoryInfo> BrowsingHistoryInfo { get; set; }
        public virtual DbSet<MenuInfo> MenuInfo { get; set; }
        public virtual DbSet<MessageLog> MessageLog { get; set; }
        public virtual DbSet<MemberContactsInfo> MemberContactsInfo { get; set; }
        public virtual DbSet<FreightAreaContentInfo> FreightAreaContentInfo { get; set; }
        public virtual DbSet<FreightTemplateInfo> FreightTemplateInfo { get; set; }
        public virtual DbSet<AgreementInfo> AgreementInfo { get; set; }
        public virtual DbSet<CashDepositInfo> CashDepositInfo { get; set; }
        public virtual DbSet<CashDepositDetailInfo> CashDepositDetailInfo { get; set; }
        public virtual DbSet<ShopBrandApplysInfo> ShopBrandApplysInfo { get; set; }
        public virtual DbSet<ShopBrandsInfo> ShopBrandsInfo { get; set; }
        public virtual DbSet<StatisticOrderCommentsInfo> StatisticOrderCommentsInfo { get; set; }
        public virtual DbSet<AccountPurchaseAgreementInfo> AccountPurchaseAgreementInfo { get; set; }
        public virtual DbSet<SensitiveWordsInfo> SensitiveWordsInfo { get; set; }
        public virtual DbSet<CouponInfo> CouponInfo { get; set; }
        public virtual DbSet<CouponRecordInfo> CouponRecordInfo { get; set; }
        public virtual DbSet<CouponSettingInfo> CouponSettingInfo { get; set; }
        public virtual DbSet<MemberGrade> MemberGrade { get; set; }
        public virtual DbSet<MemberIntegral> MemberIntegral { get; set; }
        public virtual DbSet<MemberIntegralExchangeRules> MemberIntegralExchangeRules { get; set; }
        public virtual DbSet<MemberIntegralRecord> MemberIntegralRecord { get; set; }
        public virtual DbSet<MemberIntegralRecordAction> MemberIntegralRecordAction { get; set; }
        public virtual DbSet<MemberIntegralRule> MemberIntegralRule { get; set; }
        public virtual DbSet<VShopInfo> VShopInfo { get; set; }
        public virtual DbSet<VShopExtendInfo> VShopExtendInfo { get; set; }
        public virtual DbSet<WXShopInfo> WXShopInfo { get; set; }
        public virtual DbSet<ActiveMarketServiceInfo> ActiveMarketServiceInfo { get; set; }
        public virtual DbSet<AccountMetaInfo> AccountMetaInfo { get; set; }
        public virtual DbSet<LimitTimeMarketInfo> LimitTimeMarketInfo { get; set; }
        public virtual DbSet<MarketServiceRecordInfo> MarketServiceRecordInfo { get; set; }
        public virtual DbSet<MarketSettingInfo> MarketSettingInfo { get; set; }
        public virtual DbSet<MarketSettingMetaInfo> MarketSettingMetaInfo { get; set; }
        public virtual DbSet<MobileHomeProductsInfo> MobileHomeProductsInfo { get; set; }
        public virtual DbSet<MobileHomeTopicsInfo> MobileHomeTopicsInfo { get; set; }
        public virtual DbSet<FloorTablDetailsInfo> FloorTablDetailsInfo { get; set; }
        public virtual DbSet<FloorTablsInfo> FloorTablsInfo { get; set; }
        public virtual DbSet<InvoiceContextInfo> InvoiceContextInfo { get; set; }
        public virtual DbSet<InvoiceTitleInfo> InvoiceTitleInfo { get; set; }
        public virtual DbSet<OrderPayInfo> OrderPayInfo { get; set; }
        public virtual DbSet<GiftOrderItemInfo> GiftOrderItemInfo { get; set; }
        public virtual DbSet<GiftInfo> GiftInfo { get; set; }
        public virtual DbSet<GiftOrderInfo> GiftOrderInfo { get; set; }
        public virtual DbSet<BonusInfo> BonusInfo { get; set; }
        public virtual DbSet<BonusReceiveInfo> BonusReceiveInfo { get; set; }
        public virtual DbSet<ApplyWithDrawInfo> ApplyWithDrawInfo { get; set; }
        public virtual DbSet<CapitalInfo> CapitalInfo { get; set; }
        public virtual DbSet<CapitalDetailInfo> CapitalDetailInfo { get; set; }
        public virtual DbSet<CategoryCashDepositInfo> CategoryCashDepositInfo { get; set; }
        public virtual DbSet<InviteRecordInfo> InviteRecordInfo { get; set; }
        public virtual DbSet<InviteRuleInfo> InviteRuleInfo { get; set; }
        public virtual DbSet<OpenIdsInfo> OpenIdsInfo { get; set; }
        public virtual DbSet<ChargeDetailInfo> ChargeDetailInfo { get; set; }
        public virtual DbSet<WeiXinBasicInfo> WeiXinBasicInfo { get; set; }
        public virtual DbSet<WXCardCodeLogInfo> WXCardCodeLogInfo { get; set; }
        public virtual DbSet<WXCardLogInfo> WXCardLogInfo { get; set; }
        public virtual DbSet<CollocationInfo> CollocationInfo { get; set; }
        public virtual DbSet<CollocationPoruductInfo> CollocationPoruductInfo { get; set; }
        public virtual DbSet<CollocationSkuInfo> CollocationSkuInfo { get; set; }
        public virtual DbSet<WXAccTokenInfo> WXAccTokenInfo { get; set; }
        public virtual DbSet<ShopBonusInfo> ShopBonusInfo { get; set; }
        public virtual DbSet<ShopBonusGrantInfo> ShopBonusGrantInfo { get; set; }
        public virtual DbSet<ShopBonusReceiveInfo> ShopBonusReceiveInfo { get; set; }
        public virtual DbSet<PaymentConfigInfo> PaymentConfigInfo { get; set; }
        public virtual DbSet<MemberSignInInfo> MemberSignInInfo { get; set; }
        public virtual DbSet<SiteSignInConfigInfo> SiteSignInConfigInfo { get; set; }
        public virtual DbSet<AgentProductsInfo> AgentProductsInfo { get; set; }
        public virtual DbSet<BrokerageIncomeInfo> BrokerageIncomeInfo { get; set; }
        public virtual DbSet<BrokerageRefundInfo> BrokerageRefundInfo { get; set; }
        public virtual DbSet<DistributorSettingInfo> DistributorSettingInfo { get; set; }
        public virtual DbSet<ProductBrokerageInfo> ProductBrokerageInfo { get; set; }
        public virtual DbSet<PromoterInfo> PromoterInfo { get; set; }
        public virtual DbSet<RecruitPlanInfo> RecruitPlanInfo { get; set; }
        public virtual DbSet<RecruitSettingInfo> RecruitSettingInfo { get; set; }
        public virtual DbSet<ShopDistributorSettingInfo> ShopDistributorSettingInfo { get; set; }
        public virtual DbSet<LabelInfo> LabelInfo { get; set; }
        public virtual DbSet<MemberLabelInfo> MemberLabelInfo { get; set; }
        public virtual DbSet<SendMessageRecordInfo> SendMessageRecordInfo { get; set; }
        public virtual DbSet<WeiXinMsgTemplateInfo> WeiXinMsgTemplateInfo { get; set; }
        public virtual DbSet<FlashSaleConfigInfo> FlashSaleConfigInfo { get; set; }
        public virtual DbSet<FlashSaleInfo> FlashSaleInfo { get; set; }
        public virtual DbSet<FlashSaleDetailInfo> FlashSaleDetailInfo { get; set; }
        public virtual DbSet<PhotoSpaceInfo> PhotoSpaceInfo { get; set; }
        public virtual DbSet<PhotoSpaceCategoryInfo> PhotoSpaceCategoryInfo { get; set; }
        public virtual DbSet<TemplateVisualizationSettingsInfo> TemplateVisualizationSettingsInfo { get; set; }
        public virtual DbSet<DistributionShareSetting> DistributionShareSetting { get; set; }
        public virtual DbSet<DistributionUserLinkInfo> DistributionUserLinkInfo { get; set; }
        public virtual DbSet<FlashSaleRemindInfo> FlashSaleRemindInfo { get; set; }
        public virtual DbSet<ShopHomeModulesTopImgInfo> ShopHomeModulesTopImgInfo { get; set; }
        public virtual DbSet<ShopFooterInfo> ShopFooterInfo { get; set; }
        public virtual DbSet<ReceivingAddressInfo> ReceivingAddressInfo { get; set; }
        public virtual DbSet<OrderExpressInfo> OrderExpressInfo { get; set; }
        public virtual DbSet<OrderRefundlogsInfo> OrderRefundlogsInfo { get; set; }
        public virtual DbSet<ProductCommentsImagesInfo> ProductCommentsImagesInfo { get; set; }
        public virtual DbSet<RefundReasonInfo> RefundReasonInfo { get; set; }
        public virtual DbSet<BusinessCategoriesApplyInfo> BusinessCategoriesApplyInfo { get; set; }
        public virtual DbSet<BusinessCategoriesApplyDetailInfo> BusinessCategoriesApplyDetailInfo { get; set; }
        public virtual DbSet<ShopRenewRecord> ShopRenewRecord { get; set; }
        public virtual DbSet<ShopOpenApiSettingInfo> ShopOpenApiSettingInfo { get; set; }
        public virtual DbSet<ThemeInfo> ThemeInfo { get; set; }
        public virtual DbSet<CouponSendByRegisterInfo> CouponSendByRegisterInfo { get; set; }
        public virtual DbSet<CouponSendByRegisterDetailedInfo> CouponSendByRegisterDetailedInfo { get; set; }
        public virtual DbSet<SendmessagerecordCouponInfo> SendmessagerecordCouponInfo { get; set; }
        public virtual DbSet<FightGroupActiveInfo> FightGroupActiveInfo { get; set; }
        public virtual DbSet<FightGroupActiveItemInfo> FightGroupActiveItemInfo { get; set; }
        public virtual DbSet<FightGroupOrderInfo> FightGroupOrderInfo { get; set; }
        public virtual DbSet<FightGroupsInfo> FightGroupsInfo { get; set; }
        public virtual DbSet<WeiActivityAwardInfo> WeiActivityAwardInfo { get; set; }
        public virtual DbSet<WeiActivityInfo> WeiActivityInfo { get; set; }
        public virtual DbSet<WeiActivityWinInfo> WeiActivityWinInfo { get; set; }
        public virtual DbSet<ShopInfo> ShopInfo { get; set; }
        public virtual DbSet<SettledInfo> SettledInfo { get; set; }
        public virtual DbSet<DistributionProductsInfo> DistributionProductsInfo { get; set; }
        public virtual DbSet<PendingSettlementOrdersInfo> PendingSettlementOrdersInfo { get; set; }
        public virtual DbSet<PlatAccountInfo> PlatAccountInfo { get; set; }
        public virtual DbSet<PlatAccountItemInfo> PlatAccountItemInfo { get; set; }
        public virtual DbSet<ShopAccountInfo> ShopAccountInfo { get; set; }
        public virtual DbSet<ShopAccountItemInfo> ShopAccountItemInfo { get; set; }
        public virtual DbSet<ShopWithDrawInfo> ShopWithDrawInfo { get; set; }
        public virtual DbSet<ChargeDetailShopInfo> ChargeDetailShopInfo { get; set; }
        public virtual DbSet<AutoReplyInfo> AutoReplyInfo { get; set; }
        public virtual DbSet<AppBaseSafeSettingInfo> AppBaseSafeSettingInfo { get; set; }
        public virtual DbSet<ShopBranchInfo> ShopBranchInfo { get; set; }
        public virtual DbSet<ShopBranchManagersInfo> ShopBranchManagersInfo { get; set; }
        public virtual DbSet<FreightAreaDetailInfo> FreightAreaDetailInfo { get; set; }
        public virtual DbSet<ShopBranchSkusInfo> ShopBranchSkusInfo { get; set; }
        public virtual DbSet<ProductRelationProductInfo> ProductRelationProductInfo { get; set; }
        public virtual DbSet<PlatVisitsInfo> PlatVisitsInfo { get; set; }
        public virtual DbSet<MemberActivityDegreeInfo> MemberActivityDegreeInfo { get; set; }
        public virtual DbSet<MemberBuyCategoryInfo> MemberBuyCategoryInfo { get; set; }
        public virtual DbSet<MemberConsumeStatisticsInfo> MemberConsumeStatisticsInfo { get; set; }
        public virtual DbSet<MemberGroupInfo> MemberGroupInfo { get; set; }
        public virtual DbSet<IntegralMallAdInfo> IntegralMallAdInfo { get; set; }
        public virtual DbSet<ActiveInfo> ActiveInfo { get; set; }
        public virtual DbSet<ActiveProductInfo> ActiveProductInfo { get; set; }
        public virtual DbSet<FullDiscountRulesInfo> FullDiscountRulesInfo { get; set; }
        public virtual DbSet<SendmessagerecordCouponSNInfo> SendmessagerecordCouponSNInfo { get; set; }
        public virtual DbSet<ProductWordsInfo> ProductWordsInfo { get; set; }
        public virtual DbSet<SegmentWordsInfo> SegmentWordsInfo { get; set; }
        public virtual DbSet<AppMessagesInfo> AppMessagesInfo { get; set; }
        public virtual DbSet<SearchProductsInfo> SearchProductsInfo { get; set; }
        public virtual DbSet<WXSmallChoiceProductsInfo> WXSmallChoiceProductsInfo { get; set; }
        public virtual DbSet<WXAppletFormDatasInfo> WXAppletFormDatasInfo { get; set; }
        public virtual DbSet<ShopBranchInTagInfo> ShopBranchInTagInfo { get; set; }
        public virtual DbSet<ShopBranchTagInfo> ShopBranchTagInfo { get; set; }
        public virtual DbSet<MobileFootMenuInfo> MobileFootMenuInfo { get; set; }
        public virtual DbSet<ShopWdgjSetting> ShopWdgjSetting { get; set; }
        public virtual DbSet<ProductLadderPricesInfo> ProductLadderPricesInfo { get; set; }
        public virtual DbSet<OrderInfo> OrderInfo { get; set; }
        public virtual DbSet<CouponProductInfo> CouponProductInfo { get; set; }
        public virtual DbSet<ShippingFreeGroupInfo> ShippingFreeGroupInfo { get; set; }
        public virtual DbSet<ShippingFreeRegionInfo> ShippingFreeRegionInfo { get; set; }
        public virtual DbSet<ExpressElementInfo> ExpressElementInfo { get; set; }
        public virtual DbSet<ExpressInfo> ExpressInfo { get; set; }
        public virtual DbSet<ShopShippersInfo> ShopShippersInfo { get; set; }
        public virtual DbSet<CityExpressConfigInfo> CityExpressConfigInfo { get; set; }
    
        public virtual int Job_Account(Nullable<System.DateTime> startDate, Nullable<System.DateTime> endDate)
        {
            var startDateParameter = startDate.HasValue ?
                new ObjectParameter("StartDate", startDate) :
                new ObjectParameter("StartDate", typeof(System.DateTime));
    
            var endDateParameter = endDate.HasValue ?
                new ObjectParameter("EndDate", endDate) :
                new ObjectParameter("EndDate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("Job_Account", startDateParameter, endDateParameter);
        }
    }
}
