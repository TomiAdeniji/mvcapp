using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.BusinessRules
{
    public class OrderCancellationRules
    {
        ApplicationDbContext dbContext;

        public OrderCancellationRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public DataTablesResponse GetTraderPosOrderCancellations(IDataTablesRequest requestModel, string keyword, string[] cashiers, string[] managers, string datetime, int locationId, UserSetting user)
        {
            var totalRecords = 0;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPosOrderTypeDataTable", null, null, requestModel, keyword);                               
                
                var lCancellations = new List<PosOrderCancellationPrintCheckModel>();
                var orderCancellations = dbContext.PosOrderCancels.Where(e => e.Location.Id == locationId).ToList();
                //cancels = oCancellations;

                foreach (var oCancel in orderCancellations)
                {
                    var posOrder = oCancel.OrderJson.ParseAs<Order>();
                    var queueOrders = new List<IdNameModel>();
                    if (posOrder.LinkedTraderId != null)
                        queueOrders = dbContext.QueueOrders.AsNoTracking().Where(e => e.LinkedOrderId == posOrder.LinkedTraderId).OrderByDescending(o => o.CreatedDate)
                         .Select(q => new IdNameModel { Id = q.Id, Name = q.OrderRef }).ToList();

                    var pushItem = new PosOrderCancellationPrintCheckModel()
                    {
                        Key = oCancel.Key,
                        Ref = posOrder.Reference,
                        Reason = oCancel.Description,
                        CancelledBy = oCancel.CreatedBy.GetFullName(),
                        Date = oCancel.CreatedDate.ConvertTimeFromUtc(user.Timezone).ToString(user.DateFormat + " " + user.TimeFormat),
                        Cashier = oCancel.TillUser?.GetFullName(),
                        SalesChannel = oCancel.SalesChannel.GetDescription(),
                        Customer = posOrder.Customer?.Name ?? "",
                        TotalItems = posOrder.Items?.Count().ToString("N0"),
                        ItemDetail = posOrder.Reference,
                        PDSOrders = queueOrders,
                        DiscussionKey = oCancel.Discussion?.Key ?? "",
                        CashierId = oCancel.TillUser?.Id,
                        ManagerId = oCancel.CreatedBy.Id,
                        CreatedDate = oCancel.CreatedDate
                    };
                    lCancellations.Add(pushItem);
                }

                IEnumerable<PosOrderCancellationPrintCheckModel> oCancellations = lCancellations;

                // keyword
                if (!string.IsNullOrEmpty(keyword))
                {
                    oCancellations = oCancellations.Where(q => q.Reason.ToLower().Contains(keyword) || q.Ref.ToLower().Contains(keyword)|| q.Customer.ToLower().Contains(keyword));
                }
                if (cashiers.Count() > 0)
                {
                    oCancellations = oCancellations.Where(q => cashiers.Contains(q.CashierId));
                }
                if (managers.Count() > 0)
                {
                    oCancellations = oCancellations.Where(q => managers.Contains(q.ManagerId));
                }

               
                //Filter by dates
                if (!string.IsNullOrEmpty(datetime.Trim()))
                {
                    var startDate = new DateTime();
                    var endDate = new DateTime();
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);

                    if (!string.IsNullOrEmpty(datetime.Trim()))
                    {
                        datetime.ConvertDaterangeFormat(user.DateTimeFormat, "", out startDate, out endDate, HelperClass.endDateAddedType.minute);
                        startDate = startDate.ConvertTimeToUtc(tz);
                        endDate = endDate.ConvertTimeToUtc(tz);
                    }
                    oCancellations = oCancellations.Where(q => q.CreatedDate >= startDate && q.CreatedDate < endDate);
                }

                totalRecords = oCancellations.Count();
                var orderByString = string.Empty;
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "Ref":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Ref" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Date":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Reason":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Reason" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "SalesChannel":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "SalesChannel" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "CancelledBy":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CancelledBy" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc," : " desc,");
                            break;
                        case "Cashier":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Cashier" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "Date desc ";
                            break;
                    }
                }
                oCancellations = oCancellations.OrderBy(orderByString == string.Empty ? "Date desc" : orderByString);

                oCancellations = oCancellations.Skip(requestModel.Start).Take(requestModel.Length);
                
                return new DataTablesResponse(requestModel.Draw, oCancellations.ToList(), totalRecords, totalRecords);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keyword);
            }

            return new DataTablesResponse(requestModel.Draw, new List<PosOrderCancellationPrintCheckModel>(), 0, 0); ;
        }

        public Order OrderCancellationItemDetail(string cancelKey)
        {
            var id = int.Parse(cancelKey.Decrypt());
            var orderCancellation = dbContext.PosOrderCancels.FirstOrDefault(e => e.Id == id);
            return orderCancellation.OrderJson.ParseAs<Order>();
        }

        public ReturnJsonModel SaveOrderCancellationDiscussion(DiscussionQbicleModel model, string cancelKey, UserSetting userSetting, string originatingConnectionId = "")
        {
            var returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save discussion for order cancellation", userSetting.Id, null, model);

                var id = int.Parse(cancelKey.Decrypt());
                var cancel = dbContext.PosOrderCancels.FirstOrDefault(e => e.Id == id);
                var qbicle = dbContext.PosSettings.FirstOrDefault(e => e.Location.Id == cancel.Location.Id).DefaultWorkGroup.Qbicle;

                if (!string.IsNullOrEmpty(model.UploadKey))
                    new Azure.AzureStorageRules(dbContext).ProcessingMediaS3(model.UploadKey);

                string dateFormat = string.IsNullOrEmpty(userSetting.DateFormat) ? "dd/MM/yyyy" : userSetting.DateFormat;
                dateFormat += " " + (string.IsNullOrEmpty(userSetting.TimeFormat) ? "HH:mm" : userSetting.TimeFormat);

                var currentUser = dbContext.QbicleUser.FirstOrDefault(e => e.Id == userSetting.Id);

                var tz = TimeZoneInfo.FindSystemTimeZoneById(userSetting.Timezone);
                var discussion = new QbicleDiscussion
                {
                    StartedBy = currentUser,
                    StartedDate = DateTime.UtcNow,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    Qbicle = qbicle,
                    Topic = dbContext.Topics.FirstOrDefault(t => t.Id == model.Topic),
                    TimeLineDate = DateTime.UtcNow,
                    Name = model.Title,
                    Summary = model.Summary,
                    DiscussionType = QbicleDiscussion.DiscussionTypeEnum.OrderCancellation,
                    ActivityType = QbicleActivity.ActivityTypeEnum.OrderCancellation,
                    FeaturedImageUri = model.Media.UrlGuid,
                    UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewComments,
                    IsVisibleInQbicleDashboard = true
                };

                if (model.ExpireDate != null && model.IsExpiry)
                {
                    discussion.ExpiryDate = model.ExpireDate?.ConvertTimeToUtc(tz);
                }
                else if (!string.IsNullOrEmpty(model.ExpiryDate) && model.IsExpiry)
                {
                    var expiryDate = TimeZoneInfo.ConvertTimeToUtc(model.ExpiryDate.ConvertDateFormat(dateFormat), tz);
                    if (expiryDate < DateTime.UtcNow)
                    {
                        returnModel.msg = ResourcesManager._L("ERROR_MSG_311");
                        return returnModel;
                    }
                    discussion.ExpiryDate = expiryDate;
                }
                else
                {
                    discussion.ExpiryDate = null;
                }

                #region Add First Comment
                if (!string.IsNullOrEmpty(model.Summary))
                {
                    var post = new QbiclePost
                    {
                        CreatedBy = currentUser,
                        Message = model.Summary,
                        StartedDate = DateTime.UtcNow,
                        TimeLineDate = DateTime.UtcNow
                    };

                    dbContext.Posts.Add(post);
                    dbContext.Entry(post).State = EntityState.Added;
                    discussion.Posts.Add(post);
                    discussion.Qbicle.LastUpdated = DateTime.UtcNow;
                }
                #endregion

                if (model.Assignee != null)
                {
                    var assing = model.Assignee.Where(s => s != userSetting.Id);
                    var users = dbContext.QbicleUser.Where(u => assing.Contains(u.Id)).ToList();
                    discussion.ActivityMembers.AddRange(users);
                }

                dbContext.Discussions.Add(discussion);
                dbContext.Entry(discussion).State = EntityState.Added;

                cancel.Discussion = discussion;

                dbContext.SaveChanges();

                var nRule = new NotificationRules(dbContext);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = discussion.Id,
                    EventNotify = NotificationEventEnum.DiscussionCreation,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = userSetting.Id,
                    CreatedByName = userSetting.DisplayName,
                    ReminderMinutes = 0
                };
                nRule.Notification2Activity(activityNotification);
                returnModel.result = true;
                returnModel.Object = new { discussion.Id };

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userSetting.Id, model);
                returnModel.msg = ex.Message;
            }
            return returnModel;
        }
    }
}
