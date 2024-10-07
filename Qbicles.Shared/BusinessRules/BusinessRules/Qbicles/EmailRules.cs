using Microsoft.AspNet.Identity;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Azure;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Highlight;
using Qbicles.Models.Loyalty;
using Qbicles.Models.SalesMkt;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.PoS;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using static Qbicles.BusinessRules.HelperClass;
using static Qbicles.Models.EmailLog;
using static Qbicles.Models.QbicleActivity;

namespace Qbicles.BusinessRules
{
    public class EmailRules
    {
        private ApplicationDbContext _db;

        public EmailRules(ApplicationDbContext context)
        {
            _db = context;
        }

        public ApplicationDbContext DbContext
        {
            get => _db ?? new ApplicationDbContext();
            private set => _db = value;
        }

        /// <summary>
        ///     return entity the Email Configuration
        /// </summary>
        /// <returns></returns>
        public EmailConfiguration GetEmailConfigurations()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get emaill configurations", null, null);

                return DbContext.EmailConfigurations.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return null;
            }
        }

        /// <summary>
        ///     Send an email and save reason to email log
        /// </summary>
        /// <param name="message">IdentityMessage</param>
        /// <returns></returns>
        public Enums.EmailSendResult SendEmail(IdentityMessage message)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Send email", null, null, message);

                var subject = "";
                var reasonSentId = new ReasonSent();

                switch (message.Subject)
                {
                    case "UserCreation":
                        reasonSentId = ReasonSent.UserCreation;
                        subject = EnumModel.GetDescriptionFromEnumValue(ReasonSent.UserCreation);
                        break;

                    case "ForgotPassword":
                        reasonSentId = ReasonSent.ForgotPassword;
                        subject = EnumModel.GetDescriptionFromEnumValue(ReasonSent.ForgotPassword);
                        break;

                    case "QbicleCreation":
                        reasonSentId = ReasonSent.QbicleCreation;
                        subject = "Qbicles Notification";
                        break;

                    case "QbicleUpdate":
                        reasonSentId = ReasonSent.QbicleUpdate;
                        subject = "Qbicles Notification";
                        break;

                    case "DiscussionCreation":
                        reasonSentId = ReasonSent.DiscussionCreation;
                        subject = "Qbicles Notification";
                        break;

                    case "DiscussionUpdate":
                        reasonSentId = ReasonSent.DiscussionUpdate;
                        subject = "Qbicles Notification";
                        break;

                    case "TaskCreation":
                        reasonSentId = ReasonSent.TaskCreation;
                        subject = "Qbicles Notification";
                        break;

                    case "TaskCompletion":
                        reasonSentId = ReasonSent.TaskCompletion;
                        subject = "Qbicles Notification";
                        break;

                    case "AlertCreation":
                        reasonSentId = ReasonSent.AlertCreation;
                        subject = "Qbicles Notification";
                        break;

                    case "EventCreation":
                        reasonSentId = ReasonSent.EventCreation;
                        subject = "Qbicles Notification";
                        break;

                    case "EventWithdrawl":
                        reasonSentId = ReasonSent.EventWithdrawl;
                        subject = "Qbicles Notification";
                        break;
                }

                var emailHelper = new EmailHelperRules(DbContext);
                emailHelper.SendEmail(message.Body, message.Subject, message.Destination);

                if (emailHelper.SaveEmailLogNotification(message, subject, reasonSentId) != null)
                    return Enums.EmailSendResult.CompleteSend;
                return Enums.EmailSendResult.FalseSaveSendLog;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, message);
                return Enums.EmailSendResult.FalseSend;
            }
        }

        /// <summary>
        ///     send email notification to user when created/updated the Activity
        /// </summary>
        /// <returns></returns>
        public EmailLog SendEmailNotification(Notification notification, ReasonSent sentReason, string activityName, string qbicleName)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Send email notification", null, null, notification, sentReason, activityName, qbicleName);

                var subject = ConfigurationManager.AppSettings["SubjectEmailNotification"];
                var body = CreateEmailBody2Notification(notification, activityName, qbicleName);

                var emailHelper = new EmailHelperRules(DbContext);
                emailHelper.SendEmail("", subject, notification.NotifiedUser.Email, body);

                var mess = new IdentityMessage
                {
                    Destination = notification.NotifiedUser.Email,
                    Subject = subject, //enum get name from value
                    Body = body.ToString()
                };
                var emailLog = emailHelper.SaveEmailLogNotification(mess, subject, sentReason);
                return emailLog;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, notification, sentReason, activityName, qbicleName);
                return null;
            }
        }

        /// <summary>
        ///     Send email notification to Guest
        /// </summary>
        /// <param name="sendByUser"></param>
        /// <param name="sendToGuest">email guest have invite</param>
        /// <param name="callbackUrl"></param>
        /// <param name="activityType">ActivityTypeEnum </param>
        /// <param name="activityName"></param>
        /// <param name="qbicleName"></param>
        /// <returns></returns>
        public bool SendEmailInvitedGuest(string sendByUserId, string sendToGuest,
            string callbackUrl, ActivityTypeEnum activityType, string activityName, string qbicleName)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Send invited email to guest", null, null, sendByUserId, sendToGuest, callbackUrl, activityType, activityName, qbicleName);

                var sendByUser = DbContext.QbicleUser.Find(sendByUserId);

                var subject = ConfigurationManager.AppSettings["SubjectEmailInvitedGuest"];
                var body = CreateEmailBody2InvitedGuest(sendByUser, callbackUrl, activityType, activityName, qbicleName);
                var emailHelper = new EmailHelperRules(DbContext);
                emailHelper.SendEmail(body, subject, sendToGuest);

                var mess = new IdentityMessage
                {
                    Destination = sendToGuest,
                    Subject = subject, //enum get name from value
                    Body = body
                };
                var emailLog = emailHelper.SaveEmailLogNotification(mess, subject, ReasonSent.InvitedGuest);

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, sendByUserId, sendToGuest, callbackUrl, activityType, activityName, qbicleName);
                return false;
            }
        }

        public bool SendEmailInvitation(int invitedId, ApplicationUser sendByUser, string sendToGuest, string callbackUrl,
           int domainId, string domainName, string recipientName, string bodyMessage = "", string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Send invitation email", null, null, sendByUser, sendToGuest, callbackUrl, domainName, recipientName);

                var subject = ConfigurationManager.AppSettings["SubjectEmailInvitedGuest"];
                var body = CreateEmailBody2Invitation(sendByUser, callbackUrl, domainName, recipientName, bodyMessage);

                var emailHelper = new EmailHelperRules(DbContext);
                emailHelper.SendEmail("", subject, sendToGuest, body);

                var mess = new IdentityMessage
                {
                    Destination = sendToGuest,
                    Subject = subject, //enum get name from value
                    Body = body.ToString()
                };
                var emailLog = emailHelper.SaveEmailLogNotification(mess, subject, ReasonSent.InvitedGuest);

                //Send notification Bell

                var nRule = new NotificationRules(DbContext);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    DomainId = domainId,
                    EventNotify = Notification.NotificationEventEnum.QbicleInvited,
                    CreatedByName = sendToGuest,
                    ReminderMinutes = 0,
                    CreatedById = sendByUser.Id,
                    Id = invitedId
                };
                nRule.Notification2InvitedQbicle(activityNotification);
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, sendByUser, sendToGuest, callbackUrl, domainName, recipientName);
                return false;
            }
        }

        public bool SendEmailInvitation(ApplicationUser sendByUser, string sendToGuest, string callbackUrl, string domainName, string recipientName, string bodyMessage = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Send invitation email", null, null, sendByUser, sendToGuest, callbackUrl, domainName, recipientName);

                var subject = ConfigurationManager.AppSettings["SubjectEmailInvitedGuest"];
                var body = CreateEmailBody2Invitation(sendByUser, callbackUrl, domainName, recipientName, bodyMessage);

                var emailHelper = new EmailHelperRules(DbContext);
                emailHelper.SendEmail("", subject, sendToGuest, body);

                var mess = new IdentityMessage
                {
                    Destination = sendToGuest,
                    Subject = subject, //enum get name from value
                    Body = body.ToString()
                };
                var emailLog = emailHelper.SaveEmailLogNotification(mess, subject, ReasonSent.InvitedGuest);

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, sendByUser, sendToGuest, callbackUrl, domainName, recipientName);
                return false;
            }
        }

        public async Task<bool> SendEmailHLPostSharingAsync(HLSharedPost sharedPostObj)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Send Highlight post sharing email", null, null, sharedPostObj);

                var subject = "Highlight post sharing Notification";
                var body = await CreateEmailBodyOnSharingHLPostAsync(sharedPostObj);

                var emailHelper = new EmailHelperRules(DbContext);
                emailHelper.SendEmail("", subject, sharedPostObj.SharedWithEmail, body);

                var mess = new IdentityMessage
                {
                    Destination = sharedPostObj.SharedWithEmail,
                    Subject = subject, //enum get name from value
                    Body = body.ToString()
                };
                var emailLog = emailHelper.SaveEmailLogNotification(mess, subject, ReasonSent.EmailHLPostSharing);

                //Send notification Bell

                var nRule = new NotificationRules(DbContext);

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, sharedPostObj);
                return false;
            }
        }

        public async Task<bool> SendEmailPromotionAsync(LoyaltySharedPromotion sharedPromotion)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Send sharing promotion email", null, null, sharedPromotion);

                var subject = "Promotion sharing Notification";
                var body = await CreateEmailBodyOnSharingPromotionAsync(sharedPromotion);

                var emailHelper = new EmailHelperRules(DbContext);
                emailHelper.SendEmail("", subject, sharedPromotion.SharedWithEmail, body);

                var mess = new IdentityMessage
                {
                    Destination = sharedPromotion.SharedWithEmail,
                    Subject = subject, //enum get name from value
                    Body = body.ToString()
                };
                var emailLog = emailHelper.SaveEmailLogNotification(mess, subject, ReasonSent.EmailHLPostSharing);

                //Send notification Bell

                var nRule = new NotificationRules(DbContext);

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, sharedPromotion);
                return false;
            }
        }

        public bool SendEmailBody2VerifitaionCreNewAcLoginOrPwReset(string sendToEmail, string pinVerification, string callbackUrl, string template, string subject)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Send email", null, null, sendToEmail, callbackUrl);

                var body = CreateEmailBody2VerifitaionCreNewAcLoginOrPwReset(sendToEmail, pinVerification, callbackUrl, template);

                var emailHelper = new EmailHelperRules(DbContext);
                emailHelper.SendEmail("", subject, sendToEmail, body);

                var mess = new IdentityMessage
                {
                    Destination = sendToEmail,
                    Subject = subject, //enum get name from value
                    Body = body?.ToString() ?? subject
                };
                var emailLog = emailHelper.SaveEmailLogNotification(mess, subject, ReasonSent.EmailVerification);

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, sendToEmail, callbackUrl);
                return false;
            }
        }

        /// <summary>
        ///
        /// Reset password
        /// </summary>
        /// <param name="callbackUrl"></param>
        /// <param name="template">html template</param>
        /// <returns></returns>
        private AlternateView CreateEmailBody2VerifitaionCreNewAcLoginOrPwReset(string sendToEmail, string pinVerification, string callbackUrl, string template)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create email body", null, null, callbackUrl);

                string body;
                var startupPath = AppDomain.CurrentDomain.RelativeSearchPath;
                var pathTemplate = Path.Combine(startupPath, "Templates", template);
                var logoPath = Path.Combine(startupPath, "Templates", "logo_200x75.png");
                var headlinePath = Path.Combine(startupPath, "Templates", "lock.jpg");

                using (var reader = new StreamReader(pathTemplate))
                {
                    body = reader.ReadToEnd();
                }
                var logoResource = new LinkedResource(logoPath)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                logoResource.ContentType.Name = "Logo.png";
                body = body.Replace("{logo}", "cid:" + logoResource.ContentId);
                var headlineResource = new LinkedResource(headlinePath)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                headlineResource.ContentType.Name = "lock.png";
                body = body.Replace("{lock}", "cid:" + headlineResource.ContentId);

                body = body.Replace("{url}", callbackUrl);

                var user = DbContext.QbicleUser.FirstOrDefault(e => e.Email == sendToEmail);
                body = body.Replace("{userName}", user?.GetFullName());

                body = body.Replace("{pinverification}", pinVerification);

                AlternateView alternateView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
                alternateView.LinkedResources.Add(logoResource);
                alternateView.LinkedResources.Add(headlineResource);

                return alternateView;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, callbackUrl);
                return null;
            }
        }

        /// <summary>
        /// Change my email address
        /// </summary>
        /// <param name="pinCode"> PIN Code</param>
        /// <param name="callbackUrl"></param>
        /// <param name="template">html template</param>
        /// <returns></returns>
        private AlternateView CreateEmailBody2VerifitaionNewEmailAddress(string pinCode, string callbackUrl, string template)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create email body", null, null, callbackUrl);

                string body;
                var startupPath = AppDomain.CurrentDomain.RelativeSearchPath;
                var pathTemplate = Path.Combine(startupPath, "Templates", template);
                var logoPath = Path.Combine(startupPath, "Templates", "logo_200x75.png");
                var headlinePath = Path.Combine(startupPath, "Templates", "lock.jpg");

                using (var reader = new StreamReader(pathTemplate))
                {
                    body = reader.ReadToEnd();
                }
                var logoResource = new LinkedResource(logoPath)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                logoResource.ContentType.Name = "Logo.png";
                body = body.Replace("{logo}", "cid:" + logoResource.ContentId);
                var headlineResource = new LinkedResource(headlinePath)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                headlineResource.ContentType.Name = "lock.png";
                body = body.Replace("{lock}", "cid:" + headlineResource.ContentId);
                body = body.Replace("{pin}", pinCode);
                body = body.Replace("{url}", callbackUrl);

                AlternateView alternateView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
                alternateView.LinkedResources.Add(logoResource);
                alternateView.LinkedResources.Add(headlineResource);

                return alternateView;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, callbackUrl);
                return null;
            }
        }

        public bool SendEmailNewEmailAddress(TempEmailAddress tempEmail, string callbackUrl, string template)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Send pin for change new email", null, null, tempEmail, callbackUrl, template);

                var emailHelper = new EmailHelperRules(DbContext);
                string subject = "Verify your new email address";
                var body = CreateEmailBody2VerifitaionNewEmailAddress(tempEmail.PIN, callbackUrl, template);
                emailHelper.SendEmail("", subject, tempEmail.Email, body);
                var mess = new IdentityMessage
                {
                    Destination = tempEmail.Email,
                    Subject = subject, //enum get name from value
                    Body = tempEmail.Email//must call before send email
                };
                var emailLog = emailHelper.SaveEmailLogNotification(mess, subject, ReasonSent.InvitedGuest);

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, tempEmail, callbackUrl, template);
                return false;
            }
        }

        /// <summary>
        ///     Create body for email invited the guest
        /// </summary>
        /// <param name="sendByUser"></param>
        /// <param name="callbackUrl"></param>
        /// <param name="activityType"></param>
        /// <param name="activityName"></param>
        /// <param name="qbicleName"></param>
        /// <returns></returns>
        private string CreateEmailBody2InvitedGuest(ApplicationUser sendByUser, string callbackUrl,
            ActivityTypeEnum activityType, string activityName, string qbicleName)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create email body for invited guest", null, null, sendByUser, callbackUrl, activityType, activityName, qbicleName);

                var body = string.Empty;
                var startupPath = AppDomain.CurrentDomain.RelativeSearchPath;
                var pathTemplate = Path.Combine(startupPath, "Templates", "_EmailInvitedGuest.html");
                var logoPath = Path.Combine(startupPath, "Templates", "logo_200x75.png");
                var airmailPath = Path.Combine(startupPath, "Templates", "airmail.gif");
                var timeoutToken = ConfigurationManager.AppSettings["TokenLifespan"];
                var administratorEmail = ConfigurationManager.AppSettings["AdministratorEmail"];
                var module = EnumModel.GetDescriptionFromEnumValue(activityType);
                using (var reader = new StreamReader(pathTemplate))
                {
                    body = reader.ReadToEnd();
                }

                body = body.Replace("{UserInitiated}", sendByUser.Forename + " " + sendByUser.Surname);
                body = body.Replace("{EmailInitiated}", sendByUser.Email);
                body = body.Replace("{ExpiryTime}", timeoutToken);
                body = body.Replace("{url}", callbackUrl);
                body = body.Replace("{logo}", logoPath);
                body = body.Replace("{airmail}", airmailPath);
                body = body.Replace("{administratorEmail}", administratorEmail);
                body = body.Replace("{QbicleModule}", module);

                if (activityType == ActivityTypeEnum.QbicleActivity)
                {
                    body = body.Replace("{ActivityName}", qbicleName);
                    body = body.Replace("{QbicleName}", qbicleName);
                }
                else
                {
                    body = body.Replace("{ActivityName}", activityName);
                    body = body.Replace("{QbicleName}", activityName);
                }

                return body;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, sendByUser, callbackUrl, activityType, activityName, qbicleName);
                return "";
            }
        }

        /// <summary>
        ///     Create body of email for send notification
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="activityName"></param>
        /// <param name="qbicleName"></param>
        /// <returns></returns>
        private AlternateView CreateEmailBody2Notification(Notification notification, string activityName, string qbicleName)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create email body for invited guest", null, null, notification, activityName, qbicleName);

                string body;
                var startupPath = AppDomain.CurrentDomain.RelativeSearchPath;
                var pathTemplate = Path.Combine(startupPath, "Templates", "_EmailNotification.html");
                var logoPath = Path.Combine(startupPath, "Templates", "logo_200x75.png");
                var headlinePath = Path.Combine(startupPath, "Templates", "hello.jpg");

                using (var reader = new StreamReader(pathTemplate))
                {
                    body = reader.ReadToEnd();
                }
                var logoResource = new LinkedResource(logoPath)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                logoResource.ContentType.Name = "Logo.png";
                body = body.Replace("{logo}", "cid:" + logoResource.ContentId);
                var headlineResource = new LinkedResource(headlinePath)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                headlineResource.ContentType.Name = "hello.png";
                body = body.Replace("{headlineimage}", "cid:" + headlineResource.ContentId);
                body = body.Replace("{BASE_URL}", ConfigManager.QbiclesUrl);

                var emailNotificationMessage = "";
                var emailActivityName = activityName;

                switch (notification.Event)
                {
                    case Notification.NotificationEventEnum.QbicleCreation:
                        emailNotificationMessage = "The Qbicle has been created";
                        emailActivityName = "";
                        break;

                    case Notification.NotificationEventEnum.QbicleUpdate:
                        emailNotificationMessage = "The Qbicle has been updated"; // The Qbicle has been updated";
                        emailActivityName = "";
                        break;

                    case Notification.NotificationEventEnum.DiscussionCreation:
                        emailNotificationMessage = "A Discussion has been created";
                        break;

                    case Notification.NotificationEventEnum.DiscussionUpdate:
                        emailNotificationMessage = "A Discussion has been created";
                        break;

                    case Notification.NotificationEventEnum.TaskCreation:
                        emailNotificationMessage = "A Task has been created:";
                        break;

                    case Notification.NotificationEventEnum.TaskCompletion:
                        emailNotificationMessage = "The Task has been completed:";
                        break;

                    case Notification.NotificationEventEnum.AlertCreation:
                        emailNotificationMessage = "An Alert has been created:";
                        break;

                    case Notification.NotificationEventEnum.EventCreation:
                        emailNotificationMessage = "An Event has been created:";
                        break;

                    case Notification.NotificationEventEnum.EventWithdrawl:
                        emailNotificationMessage = "Someone has withdrawn from the Event:";
                        break;

                    case Notification.NotificationEventEnum.MediaCreation:
                        emailNotificationMessage = "A Media Item has been added:";
                        break;

                    case Notification.NotificationEventEnum.PostCreation:
                        emailNotificationMessage = "A Post has been added:";
                        break;

                    case Notification.NotificationEventEnum.ApprovalCreation:
                        emailNotificationMessage = "An Approval Request has been added;";
                        break;

                    case Notification.NotificationEventEnum.CreateMember:
                        emailNotificationMessage = "A member has been added";
                        break;

                    case Notification.NotificationEventEnum.InvitedMember:
                        emailNotificationMessage = "A member has been invited";
                        break;

                    case Notification.NotificationEventEnum.AlertUpdate:
                        emailNotificationMessage = "The Alert has been updated:";
                        break;

                    case Notification.NotificationEventEnum.ApprovalUpdate:
                        emailNotificationMessage = "The Approval Request has been updated:";
                        break;

                    case Notification.NotificationEventEnum.TaskUpdate:
                        emailNotificationMessage = "The Task has been updated:";
                        break;

                    case Notification.NotificationEventEnum.EventUpdate:
                        emailNotificationMessage = "The Event has been updated:";
                        break;

                    case Notification.NotificationEventEnum.TopicPost:
                        emailNotificationMessage = "A Post has been added:";
                        break;

                    case Notification.NotificationEventEnum.MediaUpdate:
                        emailNotificationMessage = "The Media Item has been updated:";
                        break;

                    case Notification.NotificationEventEnum.ListingInterested:
                        emailNotificationMessage = "The Listing Post has been flagged:";
                        break;

                    case Notification.NotificationEventEnum.ApprovalReviewed:
                    case Notification.NotificationEventEnum.ApprovalApproved:
                    case Notification.NotificationEventEnum.ApprovalDenied:
                    case Notification.NotificationEventEnum.JournalPost:
                    case Notification.NotificationEventEnum.TransactionPost:
                    case Notification.NotificationEventEnum.LinkCreation:
                    case Notification.NotificationEventEnum.RemoveUserOutOfDomain:
                    case Notification.NotificationEventEnum.ReminderCampaignPost:
                    case Notification.NotificationEventEnum.RemoveQueue:
                    case Notification.NotificationEventEnum.QbicleInvited:
                    case Notification.NotificationEventEnum.AssignTask:
                    case Notification.NotificationEventEnum.C2CConnectionIssued:
                    case Notification.NotificationEventEnum.C2CConnectionAccepted:
                    case Notification.NotificationEventEnum.B2CConnectionCreated:
                    case Notification.NotificationEventEnum.LinkUpdate:
                    case Notification.NotificationEventEnum.ActivityComment:
                    case Notification.NotificationEventEnum.RemoveUserOutOfQbicle:
                    case Notification.NotificationEventEnum.AddUserParticipants:
                    case Notification.NotificationEventEnum.RemoveUserParticipants:
                        emailNotificationMessage = notification.Event.GetDescription();
                        break;
                }

                body = body.Replace("{QbicleModule}", emailNotificationMessage);

                var username = notification.NotifiedUser.Forename + notification.NotifiedUser.Surname == ""
                    ? notification.NotifiedUser.UserName
                    : notification.NotifiedUser.Forename + " " + notification.NotifiedUser.Surname;
                body = body.Replace("{RecipientName}", username);

                body = body.Replace("{ActivityName}", emailActivityName);
                body = body.Replace("{QbicleName}", qbicleName);
                AlternateView alternateView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
                alternateView.LinkedResources.Add(logoResource);
                alternateView.LinkedResources.Add(headlineResource);

                return alternateView;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, notification, activityName, qbicleName);
                return null;
            }
        }

        public EmailLog SendEmailQbicleIssue(object activity, string attachmentPath, Stream attachmentStream, string qbicleUri, IssueType issueType, string emails = "")
        //public EmailLog SendEmailQbicleIssue(object activity, string invoiceAttachmentPath, string qbicleUri, IssueType issueType)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Send email qbicle issue", null, null, activity, qbicleUri, issueType);

                string subject = "", body = "", emailTo = "", attachmentName = "";
                switch (issueType)
                {
                    case IssueType.Invoice:
                        var invoice = (Invoice)activity;
                        subject = $"{invoice.Workgroup.Domain.Name} Invoice #{invoice.Reference.FullRef}";
                        body = CreateEmailBody2QbicleIssue(invoice, qbicleUri, issueType);
                        emailTo = invoice.Sale.Purchaser.Email;
                        attachmentName = $"invoice-{invoice.Id}/{invoice.DueDate:dd-MM-yyyy hh:mmtt}.pdf";
                        break;

                    case IssueType.SaleOrder:
                        var saleOrder = (TraderSalesOrder)activity;
                        subject = $"{saleOrder.Sale.Workgroup.Domain.Name} Sale Order #{saleOrder.Reference.FullRef}";
                        body = CreateEmailBody2QbicleIssue(saleOrder, qbicleUri, issueType);
                        emailTo = saleOrder.Sale.Purchaser.Email;
                        attachmentName = $"Sale Order-{saleOrder.Id}/{saleOrder.Sale.CreatedDate:dd-MM-yyyy hh:mmtt}.pdf";
                        break;

                    case IssueType.PurchaseOrder:
                        var purchaseOrder = (TraderPurchaseOrder)activity;
                        subject = $"{purchaseOrder.Purchase.Workgroup.Domain.Name} Purchase Order #{purchaseOrder.Reference.FullRef}";
                        body = CreateEmailBody2QbicleIssue(purchaseOrder, qbicleUri, issueType);
                        emailTo = purchaseOrder.Purchase.Vendor.Email;
                        attachmentName = $"Purchase Order-{purchaseOrder.Id}/{purchaseOrder.Purchase.CreatedDate:dd-MM-yyyy hh:mmtt}.pdf";
                        break;

                    default:
                        break;
                }

                var startupPath = AppDomain.CurrentDomain.RelativeSearchPath;
                var logoPath = Path.Combine(startupPath, "Templates", "logo_200x75.png");
                var article1 = Path.Combine(startupPath, "Templates", "article-1.jpg");
                var article2 = Path.Combine(startupPath, "Templates", "article-1.png");

                var htmlView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");

                // create image resource from image path using LinkedResource class..
                var logoResource = new LinkedResource(logoPath, "image/jpeg")
                {
                    ContentId = "logo",
                    TransferEncoding = TransferEncoding.Base64
                };
                var article1Resource = new LinkedResource(article1, "image/jpeg")
                {
                    ContentId = "article1",
                    TransferEncoding = TransferEncoding.Base64
                };
                var article2Resource = new LinkedResource(article2, "image/jpeg")
                {
                    ContentId = "article2",
                    TransferEncoding = TransferEncoding.Base64
                };
                // adding the imaged linked to htmlView...
                htmlView.LinkedResources.Add(logoResource);
                htmlView.LinkedResources.Add(article1Resource);
                htmlView.LinkedResources.Add(article2Resource);

                var contentType = new ContentType
                {
                    MediaType = MediaTypeNames.Application.Octet,
                    Name = attachmentName
                };

                var attachment = new List<Attachment>();
                if (attachmentPath != null)
                    attachment.Add(new Attachment(attachmentPath, contentType));

                if (attachmentStream != null)
                    attachment.Add(new Attachment(attachmentStream, contentType));

                var emailHelper = new EmailHelperRules(DbContext);
                emailHelper.SendEmail(body, subject, emailTo, htmlView, attachment);

                var mess = new IdentityMessage
                {
                    Destination = emailTo,
                    Subject = subject, //enum get name from value
                    Body = body.ToString()
                };

                if (!string.IsNullOrEmpty(emails))
                {
                    char[] delimiters = { ',', ';' };
                    var sendTos = emails.Split(delimiters);
                    sendTos.ForEach(email =>
                    {
                        if (email.Trim().Length == 0) return;
                        emailHelper.SendEmail(body, subject, email.Trim(), htmlView, attachment);

                        mess = new IdentityMessage
                        {
                            Destination = email,
                            Subject = subject, //enum get name from value
                            Body = body.ToString()
                        };
                        emailHelper.SaveEmailLogNotification(mess, subject, ReasonSent.InvoiceIssue);
                    });
                }

                var emailLog = emailHelper.SaveEmailLogNotification(mess, subject, ReasonSent.InvoiceIssue);
                return emailLog;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, activity, qbicleUri, issueType);
                return null;
            }
        }

        private string CreateEmailBody2QbicleIssue(object activity, string qbicleUri, IssueType issueType)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create email body for qbicle issues", null, null, activity, qbicleUri, issueType);

                string body;
                var startupPath = AppDomain.CurrentDomain.RelativeSearchPath;
                var pathTemplate = Path.Combine(startupPath, "Templates", "_QbicleEmailIssue.html");
                var administratorEmail = ConfigurationManager.AppSettings["AdministratorEmail"];
                using (var reader = new StreamReader(pathTemplate))
                {
                    body = reader.ReadToEnd();
                }

                string domainName = "", activityId = "", contactName = "", activityName = "";
                switch (issueType)
                {
                    case IssueType.Invoice:
                        var invoice = (Invoice)activity;
                        domainName = invoice.Sale.Workgroup.Domain.Name;
                        activityId = invoice.Reference.FullRef;
                        contactName = invoice.Sale.Purchaser.Name;
                        activityName = "Invoice";
                        break;

                    case IssueType.SaleOrder:
                        var order = (TraderSalesOrder)activity;
                        domainName = order.Sale.Workgroup.Domain.Name;
                        activityId = order.Reference.FullRef;
                        contactName = order.Sale.Purchaser.Name;
                        activityName = "Sale order";
                        break;

                    case IssueType.PurchaseOrder:
                        var purchase = (TraderPurchaseOrder)activity;
                        domainName = purchase.Purchase.Workgroup.Domain.Name;
                        activityId = purchase.Reference.FullRef;
                        contactName = purchase.Purchase.Vendor.Name;
                        activityName = "Purchase order";
                        break;

                    default:
                        break;
                }
                body = body.Replace("{DomainName}", domainName);
                body = body.Replace("{ActivityId}", activityId);
                body = body.Replace("{AdminEmail}", administratorEmail);
                body = body.Replace("{ContactName}", contactName);
                body = body.Replace("{TryQbicles}", qbicleUri);
                body = body.Replace("{ActivityName}", activityName);
                return body;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, activity, qbicleUri, issueType);
                return "";
            }
        }

        private AlternateView CreateEmailBody2Invitation(ApplicationUser sendByUser, string callbackUrl, string DomainName, string RecipientName, string bodyMessage = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create email body for invitation", null, null, sendByUser, callbackUrl, DomainName, RecipientName);

                var body = string.Empty;
                var startupPath = AppDomain.CurrentDomain.RelativeSearchPath;
                var pathTemplate = Path.Combine(startupPath, "Templates", string.IsNullOrEmpty(bodyMessage) ? "_EmailInvitation.html" : "_EmailInvitationCustomizeBody.html");
                var logoPath = Path.Combine(startupPath, "Templates", "logo_200x75.png");
                var airmailPath = Path.Combine(startupPath, "Templates", "hello.jpg");
                using (var reader = new StreamReader(pathTemplate))
                {
                    body = reader.ReadToEnd();
                }
                body = body.Replace("{DomainName}", DomainName);
                body = body.Replace("{ForeName}", sendByUser.Forename);
                body = body.Replace("{RecipientName}", RecipientName);
                body = body.Replace("{FullName}", sendByUser.Forename + " " + sendByUser.Surname);
                body = body.Replace("{url}", callbackUrl);
                if (!string.IsNullOrEmpty(bodyMessage))
                    body = body.Replace("{body_message}", bodyMessage);
                var logoResource = new LinkedResource(logoPath)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                logoResource.ContentType.Name = "Logo.png";
                body = body.Replace("{logo}", "cid:" + logoResource.ContentId);

                var airMailResource = new LinkedResource(airmailPath)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                airMailResource.ContentType.Name = "Airmail.png";
                body = body.Replace("{airmail}", "cid:" + airMailResource.ContentId);

                var alternateView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
                alternateView.LinkedResources.Add(logoResource);
                alternateView.LinkedResources.Add(airMailResource);

                return alternateView;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, sendByUser, callbackUrl, DomainName, RecipientName);
                return null;
            }
        }

        private async Task<AlternateView> CreateEmailBodyOnSharingHLPostAsync(HLSharedPost sharedPostObj)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create email body for Highlight post sharing", null, null, sharedPostObj);

                var body = string.Empty;
                var startupPath = AppDomain.CurrentDomain.RelativeSearchPath;
                var pathTemplate = Path.Combine(startupPath, "Templates", "_EmailHLPostSharing.html");
                var logoPath = Path.Combine(startupPath, "Templates", "logo_200x75.png");
                var airmailPath = Path.Combine(startupPath, "Templates", "hello.jpg");
                using (var reader = new StreamReader(pathTemplate))
                {
                    body = reader.ReadToEnd();
                }
                body = body.Replace("{SenderForename}", sharedPostObj.SharedBy.Forename);
                body = body.Replace("{SenderFullName}", sharedPostObj.SharedBy.UserName ?? sharedPostObj.SharedBy.GetFullName());
                body = body.Replace("{PostTitle}", sharedPostObj.SharedPost.Title + " from " + sharedPostObj.SharedPost.CreatedBy.GetFullName());
                body = body.Replace("{PostUrl}", ConfigManager.QbiclesUrl + "/HighlightPost/HighlightPostDetail?hlPostId=" + sharedPostObj.SharedPost.Id);

                //var awsS3Bucket = AzureStorageHelper.AwsS3Client();
                var s3Object = await AzureStorageHelper.ReadObjectDataAsync(sharedPostObj.SharedPost.ImgUri);

                byte[] content = null;

                var memoryStream = new MemoryStream();
                s3Object.ObjectStream.CopyTo(memoryStream);
                content = memoryStream.ToArray();
                var stream = new MemoryStream(content);

                var postImgResource = new LinkedResource(stream)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                postImgResource.ContentType.Name = "PostImg.png";
                body = body.Replace("{PostImage}", "cid:" + postImgResource.ContentId);

                var logoResource = new LinkedResource(logoPath)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                logoResource.ContentType.Name = "Logo.png";
                body = body.Replace("{logo}", "cid:" + logoResource.ContentId);

                var alternateView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
                alternateView.LinkedResources.Add(logoResource);
                alternateView.LinkedResources.Add(postImgResource);
                return alternateView;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, sharedPostObj);
                return null;
            }
        }

        private async Task<AlternateView> CreateEmailBodyOnSharingPromotionAsync(LoyaltySharedPromotion sharedPromotion)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create email body for promotion sharing", null, null, sharedPromotion);

                var body = string.Empty;
                var startupPath = AppDomain.CurrentDomain.RelativeSearchPath;
                var pathTemplate = Path.Combine(startupPath, "Templates", "_EmailPromotionSharing.html");
                var logoPath = Path.Combine(startupPath, "Templates", "logo_200x75.png");
                using (var reader = new StreamReader(pathTemplate))
                {
                    body = reader.ReadToEnd();
                }
                body = body.Replace("{SenderForeName}", sharedPromotion.SharedBy.Forename);
                body = body.Replace("{SenderFullName}", sharedPromotion.SharedBy.UserName ?? sharedPromotion.SharedBy.GetFullName());
                body = body.Replace("{PromotionTitle}", sharedPromotion.SharedPromotion.Name);
                body = body.Replace("{PromotionUrl}", ConfigManager.QbiclesUrl + "/Monibac/PromotionDetailView?promotionKey=" + sharedPromotion.SharedPromotion.Key);

                var s3Object = await AzureStorageHelper.ReadObjectDataAsync(sharedPromotion.SharedPromotion.FeaturedImageUri);

                byte[] content = null;

                var memoryStream = new MemoryStream();
                s3Object.ObjectStream.CopyTo(memoryStream);
                content = memoryStream.ToArray();
                var stream = new MemoryStream(content);

                var promotionImgResource = new LinkedResource(stream)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                promotionImgResource.ContentType.Name = "PromotionImg.png";
                body = body.Replace("{PromotionImg}", "cid:" + promotionImgResource.ContentId);

                var logoResource = new LinkedResource(logoPath)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                logoResource.ContentType.Name = "Logo.png";
                body = body.Replace("{logo}", "cid:" + logoResource.ContentId);

                var alternateView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
                alternateView.LinkedResources.Add(logoResource);
                alternateView.LinkedResources.Add(promotionImgResource);
                return alternateView;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, sharedPromotion);
                return null;
            }
        }

        public string CreateEmailBody2Campaign(CampaignEmail post, ref List<LinkedResource> resources)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create email body for campaign", null, null, post);

                var body = string.Empty;
                var startupPath = AppDomain.CurrentDomain.RelativeSearchPath;
                var pathTemplate = Path.Combine(startupPath, "Templates", "_EmailCampaignSend.html");
                using (var reader = new StreamReader(pathTemplate))
                {
                    body = reader.ReadToEnd();
                }

                #region Heading style by the Email Template

                string headstyle = "";
                headstyle += $"font-size:{(post.Template == null || string.IsNullOrEmpty(post.Template.HeadlineFontSize) ? "34" : post.Template.HeadlineFontSize)}px;";
                headstyle += $"color:{(post.Template == null || string.IsNullOrEmpty(post.Template.HeadlineColour) ? "#ffffff" : post.Template.HeadlineColour)};";
                headstyle += (post.Template == null || string.IsNullOrEmpty(post.Template.HeadlineFont) ? "" : ("font-family:" + post.Template.HeadlineFont));
                body = body.Replace("{HeadStyle}", HttpUtility.HtmlEncode(headstyle));
                body = body.Replace("{HeadBG}", (post.Template == null || string.IsNullOrEmpty(post.Template.HeadingBg) ? "#3b4a69" : post.Template.HeadingBg));

                #endregion Heading style by the Email Template

                #region Body style by the Email Template

                string bodystyle = "";
                bodystyle += $"color:{(post.Template == null || string.IsNullOrEmpty(post.Template.BodyTextColour) ? "#999999" : post.Template.BodyTextColour)} !important;";
                bodystyle += $"font-size: {(post.Template == null || string.IsNullOrEmpty(post.Template.BodyFontSize) ? "14px" : (post.Template.BodyFontSize + "px"))};line-height: 22px; margin: 0;";
                bodystyle += (post.Template == null || string.IsNullOrEmpty(post.Template.BodyFont) ? "" : ("font-family:" + post.Template.BodyFont));
                body = body.Replace("{BodyStyle}", HttpUtility.HtmlEncode(bodystyle));
                body = body.Replace("{BodyBG}", (post.Template == null || string.IsNullOrEmpty(post.Template.BodyBg) ? "#f6f6f6" : post.Template.BodyBg));

                #endregion Body style by the Email Template

                #region Button style by the Email Template

                string buttonstyle = "";
                buttonstyle += $"display:{(post.Template == null || post.Template.ButtonIsHidden ? "inline-block" : "none")};";
                buttonstyle += $"width:250px;border-radius:5px;background-color: {(post.Template == null || string.IsNullOrEmpty(post.Template.ButtonBg) ? "#33adc2" : post.Template.ButtonBg)};";
                buttonstyle += $"font-size: {(post.Template == null || string.IsNullOrEmpty(post.Template.ButtonFontSize) ? "16px" : (post.Template.ButtonFontSize + "px"))};";
                buttonstyle += $"padding:20px 25px;color:{(post.Template == null || string.IsNullOrEmpty(post.Template.ButtonTextColour) ? "#fff" : post.Template.ButtonTextColour)};text-decoration: none;margin-bottom: 30px;";
                buttonstyle += (post.Template == null || string.IsNullOrEmpty(post.Template.ButtonFont) ? "" : ("font-family:" + post.Template.ButtonFont));
                body = body.Replace("{ButtonStyle}", HttpUtility.HtmlEncode(buttonstyle));

                #endregion Button style by the Email Template

                body = body.Replace("{Headline}", post.Headline);
                body = body.Replace("{BodyContent}", HttpUtility.UrlDecode(post.BodyContent));
                body = body.Replace("{ButtonLink}", post.ButtonLink);
                body = body.Replace("{ButtonText}", post.ButtonText);
                body = body.Replace("{DisplayPromotionImg}", post.PromotionalImage.VersionedFiles.FirstOrDefault() == null ? "none" : "block");
                if (post.Template != null && post.Template.AdvertImgiIsHidden && post.AdvertisementImage != null)
                {
                    body = body.Replace("{DisplayAdImg}", post.AdvertisementImage?.VersionedFiles.FirstOrDefault() == null ? "none" : "block");
                }
                else
                {
                    body = body.Replace("{DisplayAdImg}", post.AdvertisementImage?.VersionedFiles.FirstOrDefault() == null ? "none" : "block");
                }

                body = body.Replace("{DisplayButtonLink}", ((post.Template != null && post.Template.ButtonIsHidden && !String.IsNullOrEmpty(post.ButtonText)) || (post.Template == null && !String.IsNullOrEmpty(post.ButtonText)) ? "grid" : "none"));
                var logoPath = Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath, "Templates", "logo_200x75.png");
                var logoResource = new LinkedResource(logoPath)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                logoResource.ContentType.Name = "logo.png";
                body = body.Replace("{Logo}", "cid:" + logoResource.ContentId);
                resources.Add(logoResource);
                //Social icon link
                LinkedResource fbResource = null;
                if (post.Template != null && post.Template.IsHiddenFacebook)
                {
                    var fbPath = Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath, "Templates", "social_fb.png");
                    fbResource = new LinkedResource(fbPath)
                    {
                        ContentId = Guid.NewGuid().ToString()
                    };//table-cell":"none"
                    fbResource.ContentType.Name = "fbicon.png";
                    body = body.Replace("{DisplayFB}", "table-cell");
                    body = body.Replace("{FBPath}", "cid:" + fbResource.ContentId);
                    body = body.Replace("{FBLink}", post.Template.FacebookLink);
                    resources.Add(fbResource);
                }
                else
                {
                    body = body.Replace("{FBLink}", "");
                    body = body.Replace("{DisplayFB}", "none");
                    body = body.Replace("{FBPath}", "");
                }
                LinkedResource igResource = null;
                if (post.Template != null && post.Template.IsHiddenInstagram)
                {
                    var igPath = Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath, "Templates", "social_ig.png");
                    igResource = new LinkedResource(igPath)
                    {
                        ContentId = Guid.NewGuid().ToString()
                    };
                    igResource.ContentType.Name = "igicon.png";
                    body = body.Replace("{DisplayIG}", "table-cell");
                    body = body.Replace("{IGPath}", "cid:" + igResource.ContentId);
                    body = body.Replace("{IGLink}", post.Template.InstagramLink);
                    resources.Add(igResource);
                }
                else
                {
                    body = body.Replace("{IGLink}", "");
                    body = body.Replace("{DisplayIG}", "none");
                    body = body.Replace("{IGPath}", "");
                }
                LinkedResource liResource = null;
                if (post.Template != null && post.Template.IsHiddenLinkedIn)
                {
                    var liPath = Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath, "Templates", "social_linkedin.png");
                    liResource = new LinkedResource(liPath)
                    {
                        ContentId = Guid.NewGuid().ToString()
                    };//table-cell":"none"
                    liResource.ContentType.Name = "liicon.png";
                    body = body.Replace("{DisplayLI}", "table-cell");
                    body = body.Replace("{LIPath}", "cid:" + liResource.ContentId);
                    body = body.Replace("{LILink}", post.Template.LinkedInLink);
                    resources.Add(liResource);
                }
                else
                {
                    body = body.Replace("{LILink}", "");
                    body = body.Replace("{DisplayLI}", "none");
                    body = body.Replace("{LIPath}", "");
                }
                LinkedResource piResource = null;
                if (post.Template != null && post.Template.IsHiddenPinterest)
                {
                    var piPath = Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath, "Templates", "social_pinterest.png");
                    piResource = new LinkedResource(piPath)
                    {
                        ContentId = Guid.NewGuid().ToString()
                    };
                    piResource.ContentType.Name = "piicon.png";
                    body = body.Replace("{DisplayPI}", "table-cell");
                    body = body.Replace("{PIPath}", "cid:" + piResource.ContentId);
                    body = body.Replace("{PILink}", post.Template.PinterestLink);
                    resources.Add(piResource);
                }
                else
                {
                    body = body.Replace("{PILink}", "");
                    body = body.Replace("{DisplayPI}", "none");
                    body = body.Replace("{PIPath}", "");
                }
                LinkedResource twResource = null;
                if (post.Template != null && post.Template.IsHiddenTwitter)
                {
                    var twPath = Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath, "Templates", "social_twitter.png");
                    twResource = new LinkedResource(twPath)
                    {
                        ContentId = Guid.NewGuid().ToString()
                    };//table-cell":"none"
                    twResource.ContentType.Name = "twicon.png";
                    body = body.Replace("{DisplayTW}", "table-cell");
                    body = body.Replace("{TWPath}", "cid:" + twResource.ContentId);
                    body = body.Replace("{TWLink}", post.Template.TwitterLink);
                    resources.Add(twResource);
                }
                else
                {
                    body = body.Replace("{TWLink}", "");
                    body = body.Replace("{DisplayTW}", "none");
                    body = body.Replace("{TWPath}", "");
                }
                LinkedResource ytResource = null;
                if (post.Template != null && post.Template.IsHiddenYoutube)
                {
                    var ytPath = Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath, "Templates", "social_yt.png");
                    ytResource = new LinkedResource(ytPath)
                    {
                        ContentId = Guid.NewGuid().ToString()
                    };
                    ytResource.ContentType.Name = "twicon.png";
                    body = body.Replace("{DisplayYT}", "table-cell");
                    body = body.Replace("{YTPath}", "cid:" + ytResource.ContentId);
                    body = body.Replace("{YTLink}", post.Template.YoutubeLink);
                    resources.Add(ytResource);
                }
                else
                {
                    body = body.Replace("{YTLink}", "");
                    body = body.Replace("{DisplayYT}", "none");
                    body = body.Replace("{YTPath}", "");
                }
                return body;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, post);
                return null;
            }
        }

        public bool SendEmailPosUserPin(DeviceUser posUser)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Send email pos user pin", null, null, posUser);

                var subject = $"Issued a PIN to access the {posUser.Domain.Name} Point of Sale";
                var emailHelper = new EmailHelperRules(DbContext);
                string bodylog = "";
                var body = CreateEmailBody2PosUserPin(posUser, ref bodylog);
                emailHelper.SendEmail("", subject, posUser.User.Email, body);
                var mess = new IdentityMessage
                {
                    Destination = posUser.User.Email,
                    Subject = subject, //enum get name from value
                    Body = bodylog//must call before send email
                };
                var emailLog = emailHelper.SaveEmailLogNotification(mess, subject, ReasonSent.InvitedGuest);

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, posUser);
                return false;
            }
        }

        /// <returns></returns>
        private AlternateView CreateEmailBody2PosUserPin(DeviceUser posUser, ref string bodyHtml)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create email body for pos user pin", null, null, posUser, bodyHtml);

                var body = string.Empty;
                var startupPath = AppDomain.CurrentDomain.RelativeSearchPath;
                var pathTemplate = Path.Combine(startupPath, "Templates", "_PinEmail.html");
                var logoPath = Path.Combine(startupPath, "Templates", "logo_200x75.png");
                var airmailPath = Path.Combine(startupPath, "Templates", "hello.jpg");
                using (var reader = new StreamReader(pathTemplate))
                {
                    body = reader.ReadToEnd();
                }

                #region Logo image

                var logoResource = new LinkedResource(logoPath)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                logoResource.ContentType.Name = "Logo.png";

                #endregion Logo image

                #region Hello image

                var helloResource = new LinkedResource(airmailPath)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                helloResource.ContentType.Name = "Hello.png";

                #endregion Hello image

                body = body.Replace("{DomainName}", posUser.Domain.Name);
                body = body.Replace("{logo}", "cid:" + logoResource.ContentId);
                body = body.Replace("{airmail}", "cid:" + helloResource.ContentId);
                body = body.Replace("{UserInitiated}", HelperClass.GetFullNameOfUser(posUser.User));
                body = body.Replace("{PIN}", posUser.Pin.ToString());
                bodyHtml = body;
                var alternateView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
                alternateView.LinkedResources.Add(logoResource);
                alternateView.LinkedResources.Add(helloResource);
                return alternateView;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, posUser, bodyHtml);
                return null;
            }
        }

        public bool SendEmailStoreCreditPin(StoreCreditPIN storePin)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Send email pos user pin", null, null, storePin);

                var subject = $"Generated a new Store Credit PIN for {storePin.AssociatedUser.GetFullName()}";
                var emailHelper = new EmailHelperRules(DbContext);
                string bodylog = "";
                var body = CreateEmailBody2MbPin(storePin, ref bodylog);
                emailHelper.SendEmail("", subject, storePin.AssociatedUser.Email, body);
                var mess = new IdentityMessage
                {
                    Destination = storePin.AssociatedUser.Email,
                    Subject = subject, //enum get name from value
                    Body = bodylog//must call before send email
                };
                var emailLog = emailHelper.SaveEmailLogNotification(mess, subject, ReasonSent.GenerateNewStoreCreditPIN);

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, storePin);
                return false;
            }
        }

        private AlternateView CreateEmailBody2MbPin(StoreCreditPIN storePin, ref string bodyHtml)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create email body for new store credit pin", null, null, storePin, bodyHtml);

                var body = string.Empty;
                var startupPath = AppDomain.CurrentDomain.RelativeSearchPath;
                var pathTemplate = Path.Combine(startupPath, "Templates", "_MbPinEmail.html");
                var logoPath = Path.Combine(startupPath, "Templates", "logo_200x75.png");
                var rewardPath = Path.Combine(startupPath, "Templates", "reward.jpg");
                using (var reader = new StreamReader(pathTemplate))
                {
                    body = reader.ReadToEnd();
                }

                #region Logo image

                var logoResource = new LinkedResource(logoPath)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                logoResource.ContentType.Name = "Logo.png";

                #endregion Logo image

                #region Reward image

                var rewardResource = new LinkedResource(rewardPath)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                rewardResource.ContentType.Name = "Reward.png";

                #endregion Reward image

                body = body.Replace("{FullName}", storePin.AssociatedUser.GetFullName());
                body = body.Replace("{logo}", "cid:" + logoResource.ContentId);
                body = body.Replace("{mailimg}", "cid:" + rewardResource.ContentId);
                body = body.Replace("{PIN}", storePin.PIN);
                bodyHtml = body;
                var alternateView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
                alternateView.LinkedResources.Add(logoResource);
                alternateView.LinkedResources.Add(rewardResource);
                return alternateView;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, storePin, bodyHtml);
                return null;
            }
        }

        public bool SendEmailImportInvite(ApplicationUser user, List<MicroContact> contacts, bool isConnect = false)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create email body for invitation", null, null, contacts);

                var subject = $"{user.Forename} has invited you to join them in Qbicles!";
                var emailHelper = new EmailHelperRules(DbContext);
                var callbackUrl = $"{ConfigManager.QbiclesUrl}/registration";
                if (isConnect)
                    callbackUrl += $"?code={user.Id.Encrypt()}";
                var startupPath = AppDomain.CurrentDomain.RelativeSearchPath;
                var pathTemplate = Path.Combine(startupPath, "Templates", "_EmailImportInvite.html");
                var logoPath = Path.Combine(startupPath, "Templates", "logo_200x75.png");
                var helloPath = Path.Combine(startupPath, "Templates", "hello_import.jpg");

                contacts.ForEach(contact =>
                {
                    var body = string.Empty;
                    using (var reader = new StreamReader(pathTemplate))
                    {
                        body = reader.ReadToEnd();
                    }

                    body = body.Replace("{Forename}", user.Forename);
                    body = body.Replace("{FullName}", user.GetFullName());
                    body = body.Replace("{RecipientForename}", contact.Name);
                    body = body.Replace("{url}", callbackUrl);

                    var logoResource = new LinkedResource(logoPath)
                    {
                        ContentId = Guid.NewGuid().ToString()
                    };
                    logoResource.ContentType.Name = "Logo.png";

                    var helloResource = new LinkedResource(helloPath)
                    {
                        ContentId = Guid.NewGuid().ToString()
                    };
                    helloResource.ContentType.Name = "Hello.png";

                    body = body.Replace("{logo}", "cid:" + logoResource.ContentId);
                    body = body.Replace("{hello}", "cid:" + helloResource.ContentId);

                    var alternateView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
                    alternateView.LinkedResources.Add(logoResource);
                    alternateView.LinkedResources.Add(helloResource);

                    //var body = CreateEmailBody2PosUserPin(posUser, ref bodylog);
                    emailHelper.SendEmail("", subject, contact.Email, alternateView);
                    var mess = new IdentityMessage
                    {
                        Destination = contact.Email,
                        Subject = subject, //enum get name from value
                        Body = body//must call before send email
                    };
                    var emailLog = emailHelper.SaveEmailLogNotification(mess, subject, ReasonSent.InvitedGuest);
                });

                return true;

                //return alternateView;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, contacts);
                return false;
            }
        }

        public EmailLog SendEmailEventTaskNotificationPoints(Notification notification, ReasonSent sentReason)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Send email Event & Task notification points", null, null, notification, sentReason);

                var subject = ""; var peopleAssigneeFullName = ""; var startString = "";

                switch (notification.Event)
                {
                    case Notification.NotificationEventEnum.EventNotificationPoints:
                        var dateStart = ((QbicleEvent)notification.AssociatedAcitvity).Start;

                        var reminderMinutes = (dateStart.AddHours(-24) - DateTime.UtcNow).TotalMinutes;

                        if (reminderMinutes > 0)
                            startString = "tomorrow";
                        else
                            startString = $"{dateStart.ConvertTimeFromUtc(notification.NotifiedUser.Timezone).ToString(notification.NotifiedUser.DateFormat + " " + notification.NotifiedUser.TimeFormat)}";

                        subject = $"Reminder - {notification.AssociatedAcitvity.Name} starts {startString}, and you’re marked to attend";
                        break;

                    case Notification.NotificationEventEnum.TaskNotificationPoints:
                        subject = $"Reminder - Your task, {notification.AssociatedAcitvity.Name} is due to begin soon";
                        break;

                    case Notification.NotificationEventEnum.TaskStart:
                    case Notification.NotificationEventEnum.TaskComplete:
                        var task = DbContext.QbicleTasks.FirstOrDefault(e => e.Id == notification.AssociatedAcitvity.Id);
                        peopleAssigneeFullName = DbContext.People.Where(s => s.AssociatedSet.Id == task.AssociatedSet.Id && s.Type == QbiclePeople.PeopleTypeEnum.Assignee).FirstOrDefault()?.User.GetFullName();
                        if (notification.Event == Notification.NotificationEventEnum.TaskStart)
                            subject = $"{peopleAssigneeFullName} has begun work on {task.Name}";
                        else if (notification.Event == Notification.NotificationEventEnum.TaskComplete)
                            subject = $"{peopleAssigneeFullName} has completed their task: {task.Name}";
                        break;
                }

                var body = CreateEmailBody2EventTaskNotificationPoints(notification, peopleAssigneeFullName, startString);

                var emailHelper = new EmailHelperRules(DbContext);
                emailHelper.SendEmail("", subject, notification.NotifiedUser.Email, body);

                var mess = new IdentityMessage
                {
                    Destination = notification.NotifiedUser.Email,
                    Subject = subject, //enum get name from value
                    Body = body.ToString()
                };
                var emailLog = emailHelper.SaveEmailLogNotification(mess, subject, sentReason);
                return emailLog;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, notification, sentReason);
                return null;
            }
        }

        /// <summary>
        ///     Create body of email for send notification
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="activityName"></param>
        /// <param name="qbicleName"></param>
        /// <returns></returns>
        private AlternateView CreateEmailBody2EventTaskNotificationPoints(Notification notification, string peopleAssignee, string startString)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create email body for Event & Task notification points", null, null, notification);

                string body;
                var startupPath = AppDomain.CurrentDomain.RelativeSearchPath;
                var pathTemplate = Path.Combine(startupPath, "Templates", "_EmailEventTaskNotificationPoints.html");
                var logoPath = Path.Combine(startupPath, "Templates", "logo_200x75.png");
                var headlinePath = Path.Combine(startupPath, "Templates", "hello.jpg");

                using (var reader = new StreamReader(pathTemplate))
                {
                    body = reader.ReadToEnd();
                }
                var logoResource = new LinkedResource(logoPath)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                logoResource.ContentType.Name = "Logo.png";
                body = body.Replace("{logo}", "cid:" + logoResource.ContentId);
                var headlineResource = new LinkedResource(headlinePath)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                headlineResource.ContentType.Name = "hello.png";
                body = body.Replace("{headlineimage}", "cid:" + headlineResource.ContentId);
                body = body.Replace("{BASE_URL}", ConfigManager.QbiclesUrl);

                var message1 = "";
                var message2 = "";
                var message3 = "";

                switch (notification.Event)
                {
                    case Notification.NotificationEventEnum.EventNotificationPoints:
                        message1 = $"Reminder - ";
                        message2 = $"{notification.AssociatedAcitvity.Name} ";
                        message3 = $"starts {startString}, and you’re marked to attend";
                        break;

                    case Notification.NotificationEventEnum.TaskNotificationPoints:
                        message1 = $"Reminder - Your task, ";
                        message2 = $"{notification.AssociatedAcitvity.Name} ";
                        message3 = "is due to begin soon";
                        break;

                    case Notification.NotificationEventEnum.TaskStart:
                    case Notification.NotificationEventEnum.TaskComplete:
                        message1 = $"{peopleAssignee}";
                        message3 = $"{notification.AssociatedAcitvity.Name}";
                        if (notification.Event == Notification.NotificationEventEnum.TaskStart)
                        {
                            message2 = $" has begun work on ";
                        }
                        else if (notification.Event == Notification.NotificationEventEnum.TaskComplete)
                        {
                            message2 = $" has completed their task: ";
                        }
                        break;
                }
                body = body.Replace("{message1}", message1);
                body = body.Replace("{message2}", message2);
                body = body.Replace("{message3}", message3);

                body = body.Replace("{RecipientName}", notification.NotifiedUser.GetFullName());
                body = body.Replace("{ActivityType}", notification.AssociatedAcitvity.ActivityType.GetDescription());
                AlternateView alternateView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
                alternateView.LinkedResources.Add(logoResource);
                alternateView.LinkedResources.Add(headlineResource);

                return alternateView;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, notification);
                return null;
            }
        }
    }
}