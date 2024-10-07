using Microsoft.AspNet.Identity;
using Qbicles.Models;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using Qbicles.BusinessRules.Model;
using System;
using static Qbicles.Models.EmailLog;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;

namespace Qbicles.BusinessRules.Helper
{
    public class EmailHelperRules
    {
        private ApplicationDbContext dbContext;

        public EmailHelperRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public void SendEmail(string body, string subject, string mailto, AlternateView alternateBody = null, List<Attachment> attachments = null)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SendEmail", null, null, body, subject, mailto);



                var regex = new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");

                if (!regex.IsMatch(mailto))
                {
                    LogManager.Warn(MethodBase.GetCurrentMethod(), $"Email destination '{mailto}' is incorrect format.",null);
                    return;
                }

                var setting = dbContext.EmailConfigurations.FirstOrDefault();
                if (setting == null)
                {
                    LogManager.Warn(MethodBase.GetCurrentMethod(), $"EmailConfigurations data is null.",null);
                    return;
                }

                var displayName = "Qbicles";
                // (Optional) the name of a configuration set to use for this message.
                // If you comment out this line, you also need to remove or comment out
                // the "X-SES-CONFIGURATION-SET" header below.
                //var configSet = "ConfigSet";

                // If you're using Amazon SES in a region other than US West (Oregon), 
                // replace email-smtp.us-west-2.amazonaws.com with the Amazon SES SMTP  
                // endpoint in the appropriate AWS Region.

                // The port you will connect to on the Amazon SES SMTP endpoint. We
                // are choosing port 587 because we will use STARTTLS to encrypt
                // the connection.
                //int PORT = 587;


                // Create and build a new MailMessage object
                var message = new MailMessage
                {
                    IsBodyHtml = true,
                    From = new MailAddress(setting.Email, displayName),
                    Subject = subject,
                    DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure,
                    Priority = MailPriority.High,
                    BodyEncoding = UTF8Encoding.UTF8
                };

                if (alternateBody != null)
                    message.AlternateViews.Add(alternateBody);
                else
                    message.Body = body;

                if (attachments != null) attachments.ForEach(att =>
                {
                    message.Attachments.Add(att);
                });


                message.To.Add(new MailAddress(mailto));

                // Comment or delete the next line if you are not using a configuration set
                //message.Headers.Add("X-SES-CONFIGURATION-SET", CONFIGSET);
                if (setting.Port > 0)
                    using (var client = new SmtpClient(setting.SmtpServer))
                    {
                        // Pass SMTP credentials
                        client.Credentials = new NetworkCredential(setting.UserName, setting.Password);

                        // Enable SSL encryption
                        if (setting.IsSES)
                            client.EnableSsl = true;

                        // Try to send the message. Show status in console.
                        try
                        {
                            if (ConfigManager.LoggingDebugSet)
                                LogManager.Debug(MethodBase.GetCurrentMethod(), "Attempting to send email...", null, null, body, subject, mailto);
                            client.Send(message);
                            if (ConfigManager.LoggingDebugSet)
                                LogManager.Debug(MethodBase.GetCurrentMethod(), "Email sent!", null, null, body, subject, mailto);
                        }
                        catch (Exception ex)
                        {
                            LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, "The email was not sent. Error message: " + ex.Message);
                            return;
                        }
                    }

                else
                    using (var client = new SmtpClient(setting.SmtpServer, setting.Port))
                    {
                        // Pass SMTP credentials
                        client.Credentials = new NetworkCredential(setting.UserName, setting.Password);

                        // Enable SSL encryption
                        if (setting.IsSES)
                            client.EnableSsl = true;

                        // Try to send the message. Show status in console.
                        try
                        {
                            if (ConfigManager.LoggingDebugSet)
                                LogManager.Debug(MethodBase.GetCurrentMethod(), "Attempting to send email...", null, null, body, subject, mailto);
                            client.Send(message);
                            if (ConfigManager.LoggingDebugSet)
                                LogManager.Debug(MethodBase.GetCurrentMethod(), "Email sent!", null, null, body, subject, mailto);
                        }
                        catch (Exception ex)
                        {
                            LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, "The email was not sent. Error message: " + ex.Message);
                            return;
                        }
                    }

            }
            catch (SmtpFailedRecipientsException ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, "login not user Id");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }

        }

        public EmailLog SaveEmailLogNotification(IdentityMessage message, string subject, ReasonSent reasonSentId)
        {
            try
            {
                var el = new EmailLog
                {
                    SendDate = DateTime.UtcNow,
                    SentTo = message.Destination,
                    Subject = subject,
                    EmailBody = message.Body,
                    ReasonSentId = reasonSentId
                };
                dbContext.EmailLogs.Add(el);
                dbContext.SaveChanges();
                return el;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, message, subject, reasonSentId);
                return null;
            }

        }


        public async Task<int> SendEmailCampaignAsync(List<MailMessage> lstMail)
        {
            var setting = dbContext.EmailConfigurations.FirstOrDefault();
            if (setting == null)
            {
                LogManager.Warn(MethodBase.GetCurrentMethod(), $"EmailConfigurations data is null.",null);
                return 0;
            }
            int countError = 0;


            var ss = new SemaphoreSlim(5000);
            foreach (var message in lstMail)
            {
                await ss.WaitAsync();
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    try
                    {
                        if (setting.Port > 0)
                            using (var client = new SmtpClient(setting.SmtpServer))
                            {
                                // Pass SMTP credentials
                                client.Credentials = new NetworkCredential(setting.UserName, setting.Password);

                                // Enable SSL encryption
                                if (setting.IsSES)
                                    client.EnableSsl = true;

                                if (ConfigManager.LoggingDebugSet)
                                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Attempting to send email...", null, null, message);
                                client.Send(message);
                                if (ConfigManager.LoggingDebugSet)
                                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Email sent!", null, null, message);

                            }
                        else
                            using (var client = new SmtpClient(setting.SmtpServer, setting.Port))
                            {
                                // Pass SMTP credentials
                                client.Credentials = new NetworkCredential(setting.UserName, setting.Password);

                                // Enable SSL encryption
                                if (setting.IsSES)
                                    client.EnableSsl = true;

                                if (ConfigManager.LoggingDebugSet)
                                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Attempting to send email...", null, null, message);
                                client.Send(message);
                                if (ConfigManager.LoggingDebugSet)
                                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Email sent!", null, null, message);

                            }
                    }
                    catch (Exception ex)
                    {
                        LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, "The email was not sent. Error message: " + ex.Message);
                        Interlocked.Increment(ref countError);
                    }
                    finally
                    {
                        ss.Release();
                    }
                }, null);
            }

            return countError;
        }

    }
}
