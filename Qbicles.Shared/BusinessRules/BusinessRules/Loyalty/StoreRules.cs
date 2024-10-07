using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Loyalty;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.B2B;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.BusinessRules.Loyalty
{
    public class StoreRules
    {

        private readonly ApplicationDbContext _dbContext;

        public StoreRules(ApplicationDbContext context)
        {
            _dbContext = context;
        }



        /// <summary>
        /// This method returns the TraderContact with the relevant parts of the Domain and B2BProfile in 
        /// either an extended TraderContactModel or a new model
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual List<TraderContactModel> GetStoreContacts(ApplicationUser user)
        {

            // Find all the Domains who have the user as a TraderContact
            // using TraderContact.QbicleUser
            // For each of the Domains find the associated B2BProfile

            return new List<TraderContactModel>();

        }

        public List<MonibacBusinessModel> GetMonibacConnectedBusinesses(string userId, ref int total, string keysearch, int totalGet)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, userId, keysearch, totalGet);

                var query = (from tradercontact in _dbContext.TraderContacts
                             join businessprofile in _dbContext.B2BProfiles on tradercontact.ContactGroup.Domain.Id equals businessprofile.Domain.Id
                             //join conversionrule in _dbContext.PaymentConversions on tradercontact.ContactGroup.Domain.Id equals conversionrule.Domain.Id
                             join pointaccount in _dbContext.StorePointAccounts on tradercontact.Id equals pointaccount.Contact.Id into lst1

                             from lst1Item in lst1.DefaultIfEmpty()
                             join transaction in _dbContext.StorePointTransactions on lst1Item.Id equals transaction.Account.Id into lst2

                             from lst2Item in lst2.DefaultIfEmpty()
                             where tradercontact.QbicleUser.Id == userId// && tradercontact.Status == TraderContactStatusEnum.ContactApproved
                                && businessprofile.Domain.Status != QbicleDomain.DomainStatusEnum.Closed
                             orderby lst2Item.CreatedDate descending
                             select new MonibacBusinessModel()
                             {
                                 BusinessLogoUri = businessprofile.LogoUri,
                                 BusinessName = businessprofile.BusinessName,
                                 BusinessProfileId = businessprofile.Id,
                                 DomainId = businessprofile.Domain.Id,
                                 Contact = tradercontact
                             }).Distinct();


                #region Filtering
                if (!string.IsNullOrEmpty(keysearch.Trim()))
                {
                    keysearch = keysearch.ToLower();
                    query = query.Where(p => p.BusinessName.ToLower().Contains(keysearch));
                }
                #endregion

                total = query.Count();

                #region Paging
                query = query.Take(totalGet);
                #endregion

                var lstMonibacBusinessModels = query.ToList();
                return lstMonibacBusinessModels;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, keysearch, totalGet);
                return new List<MonibacBusinessModel>();
            }
        }

        public MonibacBusinessModel GetMonibacBusinessInfor(int domainId, string userId, CurrencySetting currencySettings, TraderContact contact)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, userId, contact);

                var storePointAccount = _dbContext.StorePointAccounts.FirstOrDefault(p => p.Contact.Id == contact.Id);
                //var user = contact.QbicleUser;
                // var domain = contact.ContactGroup.Domain;
                var business = contact.ContactGroup.Domain.Id.BusinesProfile();
                //var conversionRules = _dbContext.PaymentConversions.FirstOrDefault(p => p.Domain.Id == domain.Id && p.IsArchived == false);
                var storeCreditAccount = new StoreCreditRules(_dbContext).GetOrCreateStoreCreditAccount(contact, userId);

                var inforModel = new MonibacBusinessModel()
                {
                    AccountBalance = new TraderContactRules(_dbContext).GetBalanceContact(contact.Id),
                    BusinessLogoUri = business.LogoUri.ToDocumentUri().ToString(),
                    BusinessName = business.BusinessName,
                    BusinessProfileId = business.Id,
                    ContactKey = contact.Key,
                    Points = storePointAccount?.CurrentBalance ?? 0,
                    StoreCreditBalance = storeCreditAccount?.CurrentBalance ?? 0,
                    DomainId = contact.ContactGroup.Domain.Id,
                    Contact = contact,
                    QbicleId = _dbContext.B2CQbicles.FirstOrDefault(e => e.Business.Id == domainId)?.Id ?? 0
                };
                inforModel.StoreCreditBalanceString = inforModel.StoreCreditBalance.ToCurrencySymbol(currencySettings);
                inforModel.AccountBalanceString = inforModel.AccountBalance.ToCurrencySymbol(currencySettings);
                inforModel.ValidVouchersCount = new PromotionRules(_dbContext).CountVouchersValidByUserAndShop(userId, domainId);
                return inforModel;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, userId, contact);
                return null;
            }
        }



        public List<MonibacBusinessModel> MicroGetMonibacConnectedBusinesses(string userId, ref int total, string keysearch, int pageIndex)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, userId, keysearch, pageIndex);

                var query = (from tradercontact in _dbContext.TraderContacts
                             join businessprofile in _dbContext.B2BProfiles on tradercontact.ContactGroup.Domain.Id equals businessprofile.Domain.Id
                             //join conversionrule in _dbContext.PaymentConversions on tradercontact.ContactGroup.Domain.Id equals conversionrule.Domain.Id
                             join pointaccount in _dbContext.StorePointAccounts on tradercontact.Id equals pointaccount.Contact.Id into lst1

                             from lst1Item in lst1.DefaultIfEmpty()
                             join transaction in _dbContext.StorePointTransactions on lst1Item.Id equals transaction.Account.Id into lst2

                             from lst2Item in lst2.DefaultIfEmpty()
                             where tradercontact.QbicleUser.Id == userId && tradercontact.Status == TraderContactStatusEnum.ContactApproved
                             && businessprofile.Domain.Status == QbicleDomain.DomainStatusEnum.Open
                             orderby lst2Item.CreatedDate descending
                             select new MonibacBusinessModel()
                             {
                                 BusinessLogoUri = businessprofile.LogoUri,
                                 BusinessName = businessprofile.BusinessName,
                                 BusinessProfileId = businessprofile.Id,
                                 DomainId = businessprofile.Domain.Id,
                                 Contact = tradercontact
                             }).Distinct();


                #region Filtering
                if (!string.IsNullOrEmpty(keysearch))
                {
                    keysearch = keysearch.ToLower();
                    query = query.Where(p => p.BusinessName.ToLower().Contains(keysearch));
                }
                #endregion

                total = query.Count() / 15 == 0 ? 1 : query.Count() / 15;

                #region Paging

                query = query.OrderBy(e => e.BusinessName).Skip(pageIndex * 15).Take(15);
                #endregion

                var lstMonibacBusinessModels = query.ToList();
                return lstMonibacBusinessModels;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, keysearch, pageIndex);
                return new List<MonibacBusinessModel>();
            }
        }
    }
}
