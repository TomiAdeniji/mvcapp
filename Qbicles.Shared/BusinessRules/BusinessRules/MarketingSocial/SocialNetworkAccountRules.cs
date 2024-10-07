using Facebook;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.SalesMkt;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Reflection;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace Qbicles.BusinessRules.BusinessRules.MarketingSocial
{
    public class SocialNetworkAccountRules
    {
        private ApplicationDbContext _db;

        public SocialNetworkAccountRules()
        {
        }

        public SocialNetworkAccountRules(ApplicationDbContext context)
        {
            _db = context;
        }

        public ApplicationDbContext DbContext
        {
            get => _db ?? new ApplicationDbContext();
            private set => _db = value;
        }

        public NetworkType getNetworkTypeByName(string name)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get network type by name", null, null, name);

                return DbContext.NetworkTypes.SingleOrDefault(x => x.Name == name);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, name);
                return null;
            }
        }

        public FaceBookAccount GetFacebookAccount(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get facebook account", null, null, id);

                return DbContext.FaceBookAccounts.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }

        public TwitterAccount GetTwitterAccount(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get twitter account", null, null, id);

                return DbContext.TwitterAccounts.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }
        public ReturnJsonModel AddTwitterAccount(TwitterAccount twitterAccount, int currentDomainId)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add twitter account", null, null, twitterAccount, currentDomainId);

                var account = DbContext.TwitterAccounts.Where(x=>x.Settings.Domain.Id == currentDomainId).FirstOrDefault(x => x.TwitterId == twitterAccount.TwitterId);
                if (account != null)
                {
                    account.AvatarUrl = twitterAccount.AvatarUrl;
                    account.UserName = twitterAccount.UserName;
                    account.DisplayName = twitterAccount.DisplayName;
                    account.Token = twitterAccount.Token;
                    account.TokenSecret = twitterAccount.TokenSecret;
                    DbContext.Entry(account).State = EntityState.Modified;
                }
                else
                {
                    DbContext.TwitterAccounts.Add(twitterAccount);
                    DbContext.Entry(twitterAccount).State = EntityState.Added;
                }

                var result = DbContext.SaveChanges();
                refModel.result = result > 0 ? true : false;
                return refModel;
            }
            catch (DbEntityValidationException ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, twitterAccount, currentDomainId);
                refModel.msg = ex.Message;
                return refModel;
            }
        }

        public ReturnJsonModel AddFacebookAccount(FacebookPageGroupModel pageGroupModel, int currentDomainId, string userId, FaceBookAccount.FacebookTypeEnum facebookType)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add facebook account", userId, null, pageGroupModel, currentDomainId,  facebookType);

                var facebookAccount = new FaceBookAccount()
                {
                    CreatedBy = DbContext.QbicleUser.Find(userId),
                    Type = getNetworkTypeByName("Facebook"),
                    Settings = new SocialWorkgroupRules(DbContext).getSettingByDomainId(currentDomainId),
                    FaceBookId = pageGroupModel.id,
                    UserName = pageGroupModel.name,
                    DisplayName = pageGroupModel.name,
                    AvatarUrl = pageGroupModel.picture,
                    FacebookType = facebookType,
                    Token = pageGroupModel.access_token
                };
                var account = DbContext.FaceBookAccounts.Where(x=>x.Settings.Domain.Id == currentDomainId).FirstOrDefault(x => x.FaceBookId == pageGroupModel.id);
                if (account != null)
                {
                    if (account != null)
                    {
                        account.AvatarUrl = facebookAccount.AvatarUrl;
                        account.UserName = facebookAccount.UserName;
                        account.DisplayName = facebookAccount.DisplayName;
                        DbContext.Entry(account).State = EntityState.Modified;
                    }
                }
                else
                {
                    DbContext.FaceBookAccounts.Add(facebookAccount);
                    DbContext.Entry(facebookAccount).State = EntityState.Added;
                }
                var result = DbContext.SaveChanges();
                refModel.result = result > 0 ? true : false;
                return refModel;
            }
            catch (DbEntityValidationException ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, pageGroupModel, currentDomainId,  facebookType);
                refModel.msg = ex.Message;
                return refModel;
            }
        }
        public ReturnJsonModel deleteSocialNetworkById(int id)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete social network by id", null, null, id);

                var sn = DbContext.SocialNetworkAccounts.Find(id);
                if (sn != null)
                {
                    DbContext.SocialNetworkAccounts.Remove(sn);
                }
                var result = DbContext.SaveChanges();
                if (result > 0)
                {
                    refModel.result = true;
                }
                else
                {
                    refModel.result = false;
                }
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return refModel;
            }
        }
        public ReturnJsonModel SetDisableSocialNetwork(int id)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Set disable social network", null, null, id);

                var account = DbContext.SocialNetworkAccounts.Find(id);
                if (account != null)
                {
                    account.IsDisabled = !account.IsDisabled;
                    DbContext.Entry(account).State = EntityState.Modified;
                }
                var result = DbContext.SaveChanges();
                refModel.result = result > 0 ? true : false;
                return refModel;
            }
            catch (DbEntityValidationException ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                refModel.msg = ex.Message;
                return refModel;
            }
        }

        public string GetTwitterUriAuthentication(string redirectURI)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get twitter uri authentication", null, null, redirectURI);

                var socialNetworkSystemSetting = DbContext.SocialNetworkSystemSettings.FirstOrDefault();
                if (socialNetworkSystemSetting != null)
                {
                    var appCreds = new ConsumerCredentials(
                    socialNetworkSystemSetting.TwitterConsumerKey,
                    socialNetworkSystemSetting.TwitterConsumerSecret);
                    var _authenticationContext = AuthFlow.InitAuthentication(appCreds, redirectURI);
                    return _authenticationContext.AuthorizationURL;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, redirectURI);
                return null;
            }
        }

        public ReturnJsonModel ValidateTwitterAuth(string oauth_verifier, string authorization_id, string userId, Settings settings, int currentDomainId)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Validate twitter auth", userId, null, oauth_verifier, authorization_id,  settings, currentDomainId);
                

                var socialNetworkSystemSetting = DbContext.SocialNetworkSystemSettings.FirstOrDefault();
                if (socialNetworkSystemSetting != null)
                {
                    var appCreds = new ConsumerCredentials(
                     socialNetworkSystemSetting.TwitterConsumerKey,
                     socialNetworkSystemSetting.TwitterConsumerSecret);
                    var userCreds = AuthFlow.CreateCredentialsFromVerifierCode(oauth_verifier, authorization_id);
                    var user = Tweetinvi.User.GetAuthenticatedUser(userCreds);
                    if (user != null)
                    {
                        var twitterAccount = new TwitterAccount()
                        {
                            CreatedBy = DbContext.QbicleUser.Find(userId),
                            Type = getNetworkTypeByName("Twitter"),
                            Settings = settings,
                            TwitterId = user.Id,
                            UserName = user.Name,
                            DisplayName = user.Name,
                            AvatarUrl = user.ProfileImageUrl,
                            Token = userCreds.AccessToken,
                            TokenSecret = userCreds.AccessTokenSecret
                        };
                        var dataResult = AddTwitterAccount(twitterAccount, currentDomainId);
                        var result = DbContext.SaveChanges();
                        refModel.result = result > 0 ? true : false;
                        return refModel;
                    }
                    else
                    {
                        return refModel;
                    }
                }
                else
                {
                    return refModel;
                }
            }
            catch (DbEntityValidationException ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, oauth_verifier, authorization_id,  settings, currentDomainId);
                refModel.msg = ex.Message;
                return refModel;
            }
        }

        public string GetFacebookUriAuthentication(string redirectURI)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get facebook uri authentication", null, null, redirectURI);

                var socialNetworkSystemSetting = DbContext.SocialNetworkSystemSettings.FirstOrDefault();
                if (socialNetworkSystemSetting != null)
                {
                    var fb = new FacebookClient();
                    var loginUrl = fb.GetLoginUrl(new
                    {
                        client_id = socialNetworkSystemSetting.FacebookClientId,
                        redirect_uri = redirectURI,
                        scope = "manage_pages,publish_pages,publish_to_groups,groups_access_member_info,instagram_basic,instagram_manage_comments,instagram_manage_insights,pages_show_list,ads_management",
                        display = "popup",
                        response_type = "code"
                    });
                    return loginUrl.AbsoluteUri;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, redirectURI);
                return null;
            }
        }

        public FacebookModelResult ValidateFacebookAuth(string code, string redirectURI)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Validate facebook auth", null, null, code, redirectURI);

                var fbResult = new FacebookModelResult();
                var socialNetworkSystemSetting = DbContext.SocialNetworkSystemSettings.FirstOrDefault();
                if (socialNetworkSystemSetting != null)
                {
                    var fb = new FacebookClient();
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("client_id", socialNetworkSystemSetting.FacebookClientId);
                    parameters.Add("redirect_uri", redirectURI);
                    parameters.Add("client_secret", socialNetworkSystemSetting.FacebookClientSecret);
                    parameters.Add("code", code);
                    parameters.Add("scope", "manage_pages,publish_pages,publish_to_groups,user_photos,groups_access_member_info,instagram_basic,instagram_manage_comments,instagram_manage_insights,pages_show_list,ads_management");
                    dynamic result = fb.Get("/oauth/access_token", parameters);
                    fb.AccessToken = result.access_token;
                    fbResult.pages = fb.Get("/me/accounts", new { fields = "id,name,picture,access_token" });
                    fbResult.groups = fb.Get("/me/groups", new { fields = "id,name,picture,administrator", limit = 100 });
                    fbResult.access_token = result.access_token;
                    return fbResult;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, redirectURI);
                return null;
            }
        }

        public List<AccountSocialNetwork> ListAccountSocialNetwork(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "List account social network", null, null, domainId);

                var listAccountSocialNetwork = new List<AccountSocialNetwork>();
                var net = DbContext.NetworkTypes.ToList();
                var sm = DbContext.SalesMarketingSettings.FirstOrDefault();

                var listAccountFacebook = DbContext.FaceBookAccounts.Where(x => x.Settings.Domain.Id == domainId).ToList();
                var listAccountTwitter = DbContext.TwitterAccounts.Where(x=>x.Settings.Domain.Id == domainId).ToList();

                if (listAccountFacebook.Count > 0)
                {
                    foreach (var item in listAccountFacebook)
                    {
                        var accountSocialNetwork = new AccountSocialNetwork();
                        accountSocialNetwork.AccountName = item.DisplayName;
                        accountSocialNetwork.NetworkId = item.Id;
                        accountSocialNetwork.SocialNetworkId = item.FaceBookId;
                        accountSocialNetwork.NetworkType = item.Type.Name;
                        accountSocialNetwork.AvatarUrl = item.AvatarUrl;
                        accountSocialNetwork.IsDisabled = item.IsDisabled;
                        listAccountSocialNetwork.Add(accountSocialNetwork);
                    }
                }
                if (listAccountTwitter.Count > 0)
                {
                    foreach (var item in listAccountTwitter)
                    {
                        var accountSocialNetwork = new AccountSocialNetwork();
                        accountSocialNetwork.AccountName = item.DisplayName;
                        accountSocialNetwork.NetworkId = item.Id;
                        accountSocialNetwork.SocialNetworkId = item.TwitterId;
                        accountSocialNetwork.NetworkType = item.Type.Name;
                        accountSocialNetwork.AvatarUrl = item.AvatarUrl;
                        accountSocialNetwork.IsDisabled = item.IsDisabled;
                        listAccountSocialNetwork.Add(accountSocialNetwork);
                    }
                }
                return listAccountSocialNetwork;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return null;
            }
        }

        public ReturnJsonModel PublishTweet(SocialCampaignPost socialCampaignPost, TwitterAccount twitterAccount)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Public tweet", null, null, socialCampaignPost, twitterAccount);

                var socialNetworkSystemSetting = DbContext.SocialNetworkSystemSettings.FirstOrDefault();
                if (socialNetworkSystemSetting != null)
                {
                    var appCreds = new TwitterCredentials(
                                 socialNetworkSystemSetting.TwitterConsumerKey,
                                 socialNetworkSystemSetting.TwitterConsumerSecret,
                                 twitterAccount.Token,
                                 twitterAccount.TokenSecret
                                );
                    var tweet = Auth.ExecuteOperationWithCredentials(appCreds, () =>
                    {
                        var image = File.ReadAllBytes(@"C:\Users\PatriotSau\Pictures\twt.jpg");
                        var media = Upload.UploadBinary(image);
                       
                        return Tweet.PublishTweet("Hello world", new PublishTweetOptionalParameters
                        {
                            Medias = { media }
                        });
                    });
                    refModel.result = true;
                    return refModel;
                }
                else
                {
                    return refModel;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, socialCampaignPost, twitterAccount);
                refModel.msg = ex.Message;
                return refModel;
            }
        }

    }


}
