using Himall.CommonModel;
using Himall.Core;
using Himall.Core.Plugins.Message;
using Himall.DTO;
using Himall.Entity;
using Himall.IServices;
using Himall.DTO.QueryModel;
using Himall.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Himall.Service
{
    public class AccountService : ServiceBase, IAccountService
    {
        public QueryPageModel<AccountInfo> GetAccounts(AccountQuery query)
        {
            IQueryable<AccountInfo> complaints = Context.AccountInfo.AsQueryable();

            #region 条件组合
            if (query.Status.HasValue)
            {
                complaints = complaints.Where(item => query.Status == item.Status);
            }
            if (query.ShopId.HasValue)
            {
                complaints = complaints.Where(item => query.ShopId == item.ShopId);
            }
            if (!string.IsNullOrEmpty(query.ShopName))
            {
                complaints = complaints.Where(item => item.ShopName.Contains(query.ShopName));
            }
            #endregion

            int total;
            complaints = complaints.GetPage(d => d.OrderByDescending(o => o.Id), out total, query.PageNo, query.PageSize);

            QueryPageModel<AccountInfo> pageModel = new QueryPageModel<AccountInfo>() { Models = complaints.ToList(), Total = total };
            return pageModel;
        }

        public AccountInfo GetAccount(long id)
        {
            return Context.AccountInfo.FirstOrDefault(p => p.Id == id);
        }

        public QueryPageModel<AccountDetailInfo> GetAccountDetails(AccountQuery query)
        {
            IQueryable<AccountDetailInfo> accountDetails = Context.AccountDetailInfo.Where(item => item.OrderType == query.EnumOrderType
                && item.AccountId == query.AccountId);
            if (query.StartDate.HasValue)
            {
                accountDetails = accountDetails.Where(item => item.Date >= query.StartDate);
            }
            if (query.EndDate.HasValue)
            {
                accountDetails = accountDetails.Where(item => item.Date < query.EndDate);
            }
            int total;
            accountDetails = accountDetails.GetPage(d => d.OrderByDescending(o => o.Id), out total, query.PageNo, query.PageSize);

            QueryPageModel<AccountDetailInfo> pageModel = new QueryPageModel<AccountDetailInfo>() { Models = accountDetails.ToList(), Total = total };
            return pageModel;
        }

        public void ConfirmAccount(long id, string managerRemark)
        {
            AccountInfo account = Context.AccountInfo.FirstOrDefault(p => p.Id == id);

            account.Status = AccountInfo.AccountStatus.Accounted;
            account.Remark = managerRemark;

            Context.SaveChanges();
            //发送通知消息
            //var shopMessage = new MessageSettlementInfo();
            //shopMessage.ShopId = account.ShopId;
            //shopMessage.ShopName = account.ShopName;
            //shopMessage.Start = account.StartDate;
            //shopMessage.End = account.EndDate;
            //shopMessage.Amount = account.AccountAmount;
            //shopMessage.SiteName = ServiceProvider.Instance<ISiteSettingService>.Create.GetSiteSettings().SiteName;
            //Task.Factory.StartNew(() => ServiceProvider.Instance<IMessageService>.Create.SendMessageOnShopSettlement(account.ShopId, shopMessage));
        }

        public QueryPageModel<AccountPurchaseAgreementInfo> GetAccountPurchaseAgreements(AccountQuery query)
        {
            IQueryable<AccountPurchaseAgreementInfo> accountPurchaseAgreements = Context.AccountPurchaseAgreementInfo.Where(item => item.AccountId == query.AccountId);
            if (query.StartDate.HasValue)
            {
                accountPurchaseAgreements = accountPurchaseAgreements.Where(item => item.Date >= query.StartDate);
            }
            if (query.EndDate.HasValue)
            {
                accountPurchaseAgreements = accountPurchaseAgreements.Where(item => item.Date < query.EndDate);
            }
            int total;
            accountPurchaseAgreements = accountPurchaseAgreements.GetPage(d => d.OrderByDescending(o => o.Id), out total, query.PageNo, query.PageSize);

            QueryPageModel<AccountPurchaseAgreementInfo> pageModel = new QueryPageModel<AccountPurchaseAgreementInfo>() { Models = accountPurchaseAgreements.ToList(), Total = total };
            return pageModel;
        }

        public QueryPageModel<AccountMetaModel> GetAccountMeta(AccountQuery query)
        {
            var result = (from a in Context.AccountInfo
                          join b in Context.AccountMetaInfo on a.Id equals b.AccountId
                          where (!query.StartDate.HasValue || a.StartDate >= query.StartDate) &&
                                          (!query.EndDate.HasValue || a.EndDate < query.EndDate) &&
                                          a.Id == query.AccountId
                          select new AccountMetaModel
                          {
                              AccountId = a.Id,
                              Id = b.Id,
                              EndDate = b.ServiceEndTime,
                              StartDate = b.ServiceStartTime,
                              MetaKey = b.MetaKey,
                              MetaValue = b.MetaValue
                          });

            int total;
            var metaInfo = result.FindBy(item => true, query.PageNo, query.PageSize, out total, item => item.Id,  false);

            QueryPageModel<AccountMetaModel> pageModel = new QueryPageModel<AccountMetaModel>() { Models = metaInfo.ToList(), Total = total };
            return pageModel;
        }

        public QueryPageModel<BrokerageModel> GetBrokerageList(AccountQuery query)
        {
            var accountDetails = Context.AccountDetailInfo.Where(a => a.AccountId == query.AccountId && a.OrderType == AccountDetailInfo.EnumOrderType.FinishedOrder);
            if (query.StartDate.HasValue)
            {
                accountDetails = accountDetails.Where(item => item.Date >= query.StartDate);
            }
            if (query.EndDate.HasValue)
            {
                accountDetails = accountDetails.Where(item => item.Date < query.EndDate);
            }
            var model = Context.BrokerageIncomeInfo.Join(accountDetails, a => a.OrderId, b => b.OrderId, (a, b) => new
            {
                a.OrderId,
                a.Id,
                a.ProductName,
                a.ProductID,
                a.TotalPrice,
                a.UserId,
                a.Brokerage,
                b.Date
            });
            var list = model.Join(Context.UserMemberInfo, a => a.UserId, b => b.Id, (a, b) => new BrokerageModel
            {
                OrderId = a.OrderId.Value,
                ProductName = a.ProductName,
                ProductId = a.ProductID,
                RealTotal = a.TotalPrice.Value,
                UserId = a.UserId,
                Brokerage = a.Brokerage,
                SettlementTime = a.Date,
                Id = a.Id,
                UserName = b.UserName
            });
            var total = 0;
            list = list.GetPage(out total, d => d.OrderBy(item => item.SettlementTime).ThenBy(item => item.Id), query.PageNo, query.PageSize);
            QueryPageModel<BrokerageModel> pageModel = new QueryPageModel<BrokerageModel>() { Models = list.ToList(), Total = total };
            return pageModel;
        }
    }
}
