using Qbicles.BusinessRules.Hangfire;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Manufacturing;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Movement;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.UI.WebControls;
using static Qbicles.BusinessRules.Model.TB_Column;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.BusinessRules.Trader
{
    public class TraderManufacturingRules
    {
        private ApplicationDbContext dbContext;

        public TraderManufacturingRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public async Task<ReturnJsonModel> ScheduleManufactureProduct(ManufactureProductJobParamenter manufactureProduct)
        {
            var refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "ScheduleManufactureProduct", null, null, manufactureProduct);

                var user = dbContext.QbicleUser.Find(manufactureProduct.UserId);
                var traderItem = dbContext.TraderItems.Find(manufactureProduct.TraderItemId);
                var productUnit = dbContext.ProductUnits.Find(manufactureProduct.ProductUnitId);
                var traderLocation = dbContext.TraderLocations.Find(manufactureProduct.TraderLocationId);
                var workGroup = dbContext.WorkGroups.Find(manufactureProduct.WorkGroupId);
                var manuJob = dbContext.ManuJobs.Find(manufactureProduct.ManuJobId);
                var traderSale = dbContext.TraderSales.Find(manufactureProduct.TraderSaleId);

                await ManufactureProduct(user,
                                       traderItem,
                                       productUnit,
                                       traderLocation,
                                       manufactureProduct.ManufacturingQuantity,
                                       workGroup,
                                       manuJob,
                                       traderSale,
                                       false);
                return refModel;
            }
            catch (Exception ex)
            {
                refModel.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, manufactureProduct);
            }
            return refModel;
        }

        /// <summary>
        ///If it is a Manufacturing Process, and the manufacturing takes place because of a manufacturing approval then
        /// we do have a wworkGroup and we should use it.
        ///However, the ManufactureProduct is called because the application was doing a TRansfer out (for a Sale)
        ///then the WorkGroup is the Sale WorkGroup
        ///that WorkGroup should not be attached to the Manufacturing Process
        ///There is no Approval or WorkGroup for that event
        ///If the method is called as a result of a Transfer in a Sale, the we must use the Sale information for the subsequent account transation
        ///Dimensions are supplied to be assocaited with Account Transactions
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="manufacturingProduct"></param>
        /// <param name="manufacturingUnit"></param>
        /// <param name="manufacturingLocation"></param>
        /// <param name="manufacturingQuantity"></param>
        /// <param name="workGroup"></param>
        /// <param name="manufacturingJob"></param>
        /// <param name="sale"></param>
        public async Task ManufactureProduct(ApplicationUser currentUser,
                                       TraderItem manufacturingProduct,
                                       ProductUnit manufacturingUnit,
                                       TraderLocation manufacturingLocation,
                                       decimal manufacturingQuantity,
                                       WorkGroup workGroup = null,
                                       ManuJob manufacturingJob = null,
                                       TraderSale sale = null,
                                       bool sendToQueue = true)
        {
            if (sendToQueue)
            {
                //TODO: Inventory Call hangfire
                var job = new ManufactureProductJobParamenter
                {
                    EndPointName = "scheduleonmanufactureproduct",
                    SendToQueue = false,
                    UserId = currentUser.Id,
                    TraderItemId = manufacturingProduct.Id,
                    ProductUnitId = manufacturingUnit.Id,
                    TraderLocationId = manufacturingLocation.Id,
                    ManufacturingQuantity = manufacturingQuantity,
                    WorkGroupId = workGroup?.Id,
                    ManuJobId = manufacturingJob?.Id,
                    TraderSaleId = sale?.Id
                };
                Task tskHangfire = new Task(async () =>
                {
                    await new QbiclesJob().HangFireExcecuteAsync(job);
                });
                tskHangfire.Start();

                return;
            }
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, currentUser.Id, null, currentUser, manufacturingProduct,
                        manufacturingUnit, manufacturingLocation, manufacturingQuantity, workGroup, manufacturingJob, sale);

                #region Transfer out for the ingredients

                //Create the Transfers to do the manufacturing job
                // The Transfer out is to get the ingredients OUT of the Inventory
                var tradTransferOut = new TraderTransfer
                {
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.UtcNow,
                    DestinationLocation = null,
                    OriginatingLocation = manufacturingLocation,
                    ManufacturingJob = manufacturingJob,
                    Workgroup = workGroup,
                    Address = null,
                    Contact = null,
                    Status = TransferStatus.Delivered,
                    Reason = TransferReasonEnum.ManufacturingJobAdjustment,
                    Reference = new TraderReferenceRules(dbContext).GetNewReference(manufacturingLocation.Domain.Id, TraderReferenceType.Transfer)
                };
                // Get the inventory detail
                var inventoryDetail = manufacturingProduct.InventoryDetails.FirstOrDefault(e => e.Location.Id == manufacturingLocation.Id);

                var currentRecipe = inventoryDetail?.CurrentRecipe ?? new Recipe();

                //Create inventoryRules to get ingredient costs
                decimal compoundItemUnitCost = 0;

                foreach (var ingredient in currentRecipe.Ingredients)
                {
                    // Get the InventoryDetail for the Ingredient
                    var ingredientInventoryDetail =
                        ingredient.SubItem.InventoryDetails.FirstOrDefault(l => l.Location.Id == inventoryDetail.Location.Id) ?? new InventoryDetail();

                    //Example:
                    // We want to decrement by 10 Hampers
                    // Each Hamper has 2 Bundles of candles
                    // Each bundle contains 5 candles
                    // How many candles must be removed?

                    // totalBatchQuantityRequired = 10
                    //      ingredient.Quantity = 2
                    //      ingredient.Unit.QuantityOfBaseunit = 5
                    //      total number of candles = 10 * (2 * 5)
                    var ingredientQuantity = manufacturingQuantity * (ingredient.Quantity * ingredient.Unit.QuantityOfBaseunit);

                    // The transferItem that we currently have is for the Compound item
                    // However, we are no longer working with the Compound item
                    // We must create a new transferItem for each ingredient
                    var subItem = ingredient.SubItem;
                    var ingredientTransferItem = new TraderTransferItem
                    {
                        Unit = ingredient.SubItem.Units.FirstOrDefault(s => s.IsBase),
                        QuantityAtPickup = ingredientQuantity,
                        QuantityAtDelivery = ingredientQuantity,
                        TransactionItem = null,
                        TraderItem = ingredient.SubItem,
                        AssociatedTransfer = tradTransferOut
                    };
                    tradTransferOut.TransferItems.Add(ingredientTransferItem);

                    compoundItemUnitCost += (ingredientInventoryDetail.AverageCost * (ingredient.Quantity * ingredient.Unit.QuantityOfBaseunit));
                }

                dbContext.TraderTransfers.Add(tradTransferOut);
                dbContext.Entry(tradTransferOut).State = EntityState.Added;

                #endregion Transfer out for the ingredients

                #region Transfer in for the CompoundItems

                // The Transfer in is to get the Compound Items IN to the Inventory
                var tradTransferIn = new TraderTransfer
                {
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.UtcNow,
                    DestinationLocation = manufacturingLocation,
                    OriginatingLocation = null,
                    ManufacturingJob = manufacturingJob,
                    Workgroup = workGroup,
                    Address = null,
                    Contact = null,
                    Status = TransferStatus.Delivered,
                    Reason = TransferReasonEnum.ManufacturingJobAdjustment,
                    Reference = new TraderReferenceRules(dbContext).GetNewReference(manufacturingLocation.Domain.Id, TraderReferenceType.Transfer)
                };

                // TransferInItems for the manufactured items
                var transferItemIn = new TraderTransferItem
                {
                    Unit = manufacturingUnit,
                    QuantityAtPickup = manufacturingQuantity,
                    QuantityAtDelivery = manufacturingQuantity,
                    TransactionItem = null, //Not related to Sale or Purchase
                    TraderItem = manufacturingProduct,
                    AssociatedTransfer = tradTransferIn
                };

                tradTransferIn.TransferItems.Add(transferItemIn);
                dbContext.TraderTransfers.Add(tradTransferIn);
                dbContext.Entry(tradTransferIn).State = EntityState.Added;

                #endregion Transfer in for the CompoundItems

                #region Save the Transfers

                dbContext.SaveChanges();

                #endregion Save the Transfers

                // Get the unit cost of the compound item, based on the costs of the ingredients

                /* TransferOut and TransferIn */
                var transferRule = new TraderTransfersRules(dbContext);
                //Call TransferOut for the Ingredients
                await transferRule.OutgoingInventory(tradTransferOut, currentUser, null, sendToQueue = false);
                //Call TransferIn for the manufactured items
                await transferRule.IncomingInventory(tradTransferIn, currentUser, null, compoundItemUnitCost, sendToQueue = false);

                //Call Bookkeeping Integration for Manufacturing
                var bkIntegrationRule = new BookkeepingIntegrationRules(dbContext);
                bkIntegrationRule.AddManufacturingJournalEntry(currentUser, manufacturingLocation.Domain, tradTransferIn, tradTransferOut, sale, manufacturingJob);

                // If there is a workgroup, update the last edited date of the QBicles
                if (workGroup != null)
                {
                    workGroup.Qbicle.LastUpdated = DateTime.UtcNow;
                }

                #region Logging TransferLog and TransferProcessLog

                var transferLogIn = new TransferLog
                {
                    Address = tradTransferIn.Address,
                    AssociatedTransfer = tradTransferIn,
                    Contact = tradTransferIn.Contact,
                    CreatedDate = DateTime.UtcNow,
                    Sale = tradTransferIn.Sale,
                    Status = tradTransferIn.Status,
                    UpdatedBy = currentUser,
                    AssociatedShipment = tradTransferIn.AssociatedShipment,
                    DestinationLocation = tradTransferIn.DestinationLocation,
                    OriginatingLocation = tradTransferIn.OriginatingLocation,
                    TransferItems = tradTransferIn.TransferItems,
                    Workgroup = tradTransferIn.Workgroup
                };

                var transferProcessLogIn = new TransferProcessLog
                {
                    AssociatedTransfer = tradTransferIn,
                    AssociatedTransferLog = transferLogIn,
                    TransferStatus = tradTransferIn.Status,
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.UtcNow
                };

                var transferLogOut = new TransferLog
                {
                    Address = tradTransferOut.Address,
                    AssociatedTransfer = tradTransferOut,
                    Contact = tradTransferOut.Contact,
                    CreatedDate = DateTime.UtcNow,
                    Sale = tradTransferOut.Sale,
                    Status = tradTransferOut.Status,
                    UpdatedBy = currentUser,
                    AssociatedShipment = tradTransferOut.AssociatedShipment,
                    DestinationLocation = tradTransferOut.DestinationLocation,
                    OriginatingLocation = tradTransferOut.OriginatingLocation,
                    TransferItems = tradTransferOut.TransferItems,
                    Workgroup = tradTransferOut.Workgroup
                };

                var transferProcessLogOut = new TransferProcessLog
                {
                    AssociatedTransfer = tradTransferOut,
                    AssociatedTransferLog = transferLogOut,
                    TransferStatus = tradTransferOut.Status,
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.UtcNow
                };

                dbContext.TraderTransferProcessLogs.Add(transferProcessLogIn);
                dbContext.Entry(transferProcessLogIn).State = EntityState.Added;

                dbContext.TraderTransferProcessLogs.Add(transferProcessLogOut);
                dbContext.Entry(transferProcessLogOut).State = EntityState.Added;

                #endregion Logging TransferLog and TransferProcessLog

                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, currentUser.Id, currentUser, manufacturingProduct, manufacturingUnit,
                    manufacturingLocation, manufacturingQuantity, workGroup, manufacturingJob, sale);
            }
        }

        public async Task ManufacturingApproval(ApprovalReq approval)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, approval);
                var manuJob = approval.Manufacturingjobs.FirstOrDefault();
                if (manuJob == null) return;
                switch (approval.RequestStatus)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        manuJob.Status = ManuJobStatus.Pending;
                        break;

                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        manuJob.Status = ManuJobStatus.Reviewed;
                        break;

                    case ApprovalReq.RequestStatusEnum.Approved:
                        manuJob.Status = ManuJobStatus.Approved;
                        break;

                    case ApprovalReq.RequestStatusEnum.Denied:
                        manuJob.Status = ManuJobStatus.Denied;
                        break;

                    case ApprovalReq.RequestStatusEnum.Discarded:
                        manuJob.Status = ManuJobStatus.Discarded;
                        break;
                }
                dbContext.Entry(manuJob).State = EntityState.Modified;
                dbContext.SaveChanges();

                var processLog = new ManuProcessLog
                {
                    AssociatedManuJob = manuJob,
                    ManuJobStatus = manuJob.Status,
                    CreatedBy = approval.ApprovedOrDeniedAppBy,
                    CreatedDate = DateTime.UtcNow,
                    ApprovalReqHistory = new ApprovalReqHistory
                    {
                        ApprovalReq = approval,
                        UpdatedBy = approval.ApprovedOrDeniedAppBy,
                        CreatedDate = DateTime.UtcNow,
                        RequestStatus = approval.RequestStatus
                    }
                };

                var log = new ManufacturingLog
                {
                    UpdatedBy = manuJob.CreatedBy,
                    CreatedDate = DateTime.UtcNow,
                    AssociatedManuJob = manuJob,
                    CreatedBy = approval.StartedBy,
                    Location = manuJob.Location,
                    Product = manuJob.Product,
                    Quantity = manuJob.Quantity,
                    SelectedRecipe = manuJob.SelectedRecipe,
                    Status = manuJob.Status,
                    AssociatedProcessLog = processLog
                };
                dbContext.ManufacturingLogs.Add(log);
                dbContext.Entry(log).State = EntityState.Added;

                dbContext.SaveChanges();
                //Create new Transfer and Outgoing when Approved -- Manufacturing
                if (manuJob.Status != ManuJobStatus.Approved) return;

                await ManufactureProduct(
                   approval.ApprovedOrDeniedAppBy,
                   manuJob.Product,
                   manuJob.AssemblyUnit,
                   manuJob.Location,
                   manuJob.Quantity,
                   manuJob.WorkGroup,
                   manuJob,
                   null,  //No Associated Sale
                   sendToQueue: true
               );
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, approval);
            }
        }

        public ReturnJsonModel SaveManufacturing(ManuJob manuJob, string userId, string originatingConnectionId = "")
        {
            var result = new ReturnJsonModel { actionVal = 3, msgId = "0" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, manuJob);

                if (manuJob.Reference != null)
                    manuJob.Reference = new TraderReferenceRules(dbContext).GetById(manuJob.Reference.Id);

                if (manuJob.WorkGroup != null && manuJob.WorkGroup.Id > 0)
                    manuJob.WorkGroup = dbContext.WorkGroups.Find(manuJob.WorkGroup.Id);

                if (manuJob.Location != null && manuJob.Location.Id > 0)
                    manuJob.Location = dbContext.TraderLocations.Find(manuJob.Location.Id);

                if (manuJob.Product != null && manuJob.Product.Id > 0)
                    manuJob.Product = dbContext.TraderItems.Find(manuJob.Product.Id);

                if (manuJob.AssemblyUnit != null && manuJob.AssemblyUnit.Id > 0)
                    manuJob.AssemblyUnit = dbContext.ProductUnits.Find(manuJob.AssemblyUnit.Id);

                if (manuJob.SelectedRecipe != null && manuJob.SelectedRecipe.Id > 0)
                    manuJob.SelectedRecipe = dbContext.Recipes.Find(manuJob.SelectedRecipe.Id);

                var user = dbContext.QbicleUser.Find(userId);
                if (manuJob.Id == 0)
                {
                    manuJob.CreatedDate = DateTime.UtcNow;
                    manuJob.CreatedBy = user;
                    dbContext.Entry(manuJob).State = EntityState.Added;
                    dbContext.ManuJobs.Add(manuJob);
                    dbContext.SaveChanges();
                    //When the ManuJob is saved a corresponding ManufacturingLog is also created.

                    var manuLog = new ManufacturingLog
                    {
                        AssociatedManuJob = manuJob,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        Location = manuJob.Location,
                        Product = manuJob.Product,
                        Quantity = manuJob.Quantity,
                        SelectedRecipe = manuJob.SelectedRecipe,
                        Status = manuJob.Status,
                        UpdatedBy = user
                    };

                    dbContext.Entry(manuLog).State = EntityState.Added;
                    dbContext.ManufacturingLogs.Add(manuLog);

                    dbContext.SaveChanges();

                    result.msgId = manuJob.Id.ToString();
                    result.actionVal = 1;
                }
                else
                {
                    result.actionVal = 2;
                }

                var manuJobDb = dbContext.ManuJobs.Find(manuJob.Id);
                manuJobDb.Reference = manuJob.Reference;
                dbContext.SaveChanges();

                if (manuJobDb?.ManuApprovalProcess != null)
                    return result;

                if (manuJobDb == null || manuJobDb.Status != ManuJobStatus.Pending) return result;

                //NOTE: All changes in status require that a  ManuProcessLog(ApprovalRequestHistory) be created.
                manuJobDb.WorkGroup.Qbicle.LastUpdated = DateTime.UtcNow;
                var def =
                    dbContext.ManuApprovalDefinitions.FirstOrDefault(w => w.WorkGroup.Id == manuJobDb.WorkGroup.Id);
                var approval = new ApprovalReq
                {
                    ApprovalRequestDefinition = def,
                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                    Manufacturingjobs = new List<ManuJob> { manuJobDb },
                    Name = $"Compound Item Assembly:  {manuJobDb.Product.Name}",
                    Qbicle = manuJobDb.WorkGroup.Qbicle,
                    Topic = manuJobDb.WorkGroup.Topic,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    StartedBy = manuJobDb.CreatedBy,
                    StartedDate = DateTime.UtcNow,
                    TimeLineDate = DateTime.UtcNow,
                    Notes = "",
                    IsVisibleInQbicleDashboard = true,
                    App = QbicleActivity.ActivityApp.Trader
                };
                manuJobDb.ManuApprovalProcess = approval;
                manuJobDb.ManuApprovalProcess.ApprovalRequestDefinition = def;
                approval.ActivityMembers.AddRange(manuJobDb.WorkGroup.Members);

                dbContext.Entry(manuJobDb).State = EntityState.Modified;
                dbContext.SaveChanges();

                var processLog = new ManuProcessLog
                {
                    AssociatedManuJob = manuJob,
                    ManuJobStatus = manuJob.Status,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    ApprovalReqHistory = new ApprovalReqHistory
                    {
                        ApprovalReq = approval,
                        UpdatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        RequestStatus = approval.RequestStatus
                    }
                };

                var log = new ManufacturingLog
                {
                    UpdatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    AssociatedManuJob = manuJob,
                    CreatedBy = user,
                    Location = manuJob.Location,
                    Product = manuJob.Product,
                    Quantity = manuJob.Quantity,
                    SelectedRecipe = manuJob.SelectedRecipe,
                    Status = manuJob.Status,
                    AssociatedProcessLog = processLog
                };
                dbContext.ManufacturingLogs.Add(log);
                dbContext.Entry(log).State = EntityState.Added;

                dbContext.SaveChanges();

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = approval.Id,
                    EventNotify = NotificationEventEnum.ApprovalCreation,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = userId,
                    CreatedByName = HelperClass.GetFullNameOfUser(approval.StartedBy),
                    ReminderMinutes = 0
                };
                new NotificationRules(dbContext).Notification2Activity(activityNotification);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, manuJob);
                result.msg = e.Message;
            }

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id">Transfer Id</param>
        /// <param name="timezone"></param>
        /// <returns></returns>
        public List<ApprovalStatusTimeline> ManuJobApprovalStatusTimeline(int id, string timezone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, timezone);
                var timeline = new List<ApprovalStatusTimeline>();
                var logs = dbContext.ManuProcessLogs.Where(e => e.AssociatedManuJob.Id == id).OrderByDescending(d => d.CreatedDate).ToList();
                string status = "", icon = "";

                foreach (var log in logs)
                {
                    switch (log.ManuJobStatus)
                    {
                        case ManuJobStatus.Pending:
                            status = "Pending ";
                            icon = "fa fa-info bg-aqua";
                            break;

                        case ManuJobStatus.Reviewed:
                            status = "Reviewed";
                            icon = "fa fa-truck bg-yellow";
                            break;

                        case ManuJobStatus.Approved:
                            status = "Approved";
                            icon = "fa fa-check bg-green";
                            break;

                        case ManuJobStatus.Discarded:
                            status = "Denied";
                            icon = "fa fa-warning bg-red";
                            break;

                        case ManuJobStatus.Denied:
                            status = "Denied";
                            icon = "fa fa-warning bg-red";
                            break;
                    }
                    timeline.Add
                    (
                        new ApprovalStatusTimeline
                        {
                            LogDate = log.CreatedDate,
                            Time = log.CreatedDate.ConvertTimeFromUtc(timezone).ToShortTimeString(),
                            Status = status,
                            Icon = icon,
                            UserAvatar = log.CreatedBy.ProfilePic
                        }
                    );
                }

                return timeline;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id, timezone);
                return new List<ApprovalStatusTimeline>();
            }
        }

        public ManuJob GetManuJobById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.ManuJobs.FirstOrDefault(e => e.Id == id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new ManuJob();
            }
        }

        public List<ManuJob> GetManusByLocation(int locationid)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationid);
                return dbContext.ManuJobs.Where(q => q.Location.Id == locationid).OrderByDescending(e => e.CreatedDate).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, locationid);
                return new List<ManuJob>();
            }
        }

        public List<WorkGroup> GetManuWorkGroups(int locationid)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationid);
                return dbContext.ManuJobs.Where(q => q.Location.Id == locationid).Select(wq => wq.WorkGroup).Distinct().OrderBy(n => n.Name).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, locationid);
                return new List<WorkGroup>();
            }
        }

        public ReturnJsonModel UpdateManuJobReview(ManuJob manuJob, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 3, msgId = "0" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, manuJob, userId);
                var manuJobUpdate = GetManuJobById(manuJob.Id);
                if (manuJobUpdate == null) return result;
                /*
                 NOTE: All saves of the ManuJob require that a  ManufactoringLog be created.
                 */
                manuJobUpdate.Quantity = manuJob.Quantity;
                manuJobUpdate.AssemblyUnit = new TraderConversionUnitRules(dbContext).GetById(manuJob.AssemblyUnit.Id);

                dbContext.Entry(manuJobUpdate).State = EntityState.Modified;
                var user = dbContext.QbicleUser.Find(userId);
                var manuLog = new ManufacturingLog
                {
                    AssociatedManuJob = manuJobUpdate,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    Location = manuJobUpdate.Location,
                    Product = manuJobUpdate.Product,
                    Quantity = manuJobUpdate.Quantity,
                    SelectedRecipe = manuJobUpdate.SelectedRecipe,
                    Status = manuJobUpdate.Status,
                    UpdatedBy = user
                };

                dbContext.Entry(manuLog).State = EntityState.Added;
                dbContext.ManufacturingLogs.Add(manuLog);

                dbContext.SaveChanges();
                result.actionVal = 2;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, manuJob);
                result.msg = e.Message;
            }

            return result;
        }

        public DataTablesResponse GetDataManuJoAvailable(IDataTablesRequest requestModel, string userId, int locationId, string keyword = "", int groupId = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, locationId, userId, keyword);
                int totalrecords = 0;

                #region Filters

                var traderItems = dbContext.WorkGroups.Where(q =>
                       q.Location.Id == locationId
                       && q.Processes.Any(p => p.Name.Equals(TraderProcessName.Manufacturing))
                       && q.Members.Select(u => u.Id).Contains(userId)
                       ).SelectMany(e => e.ItemCategories).SelectMany(i => i.Items).Where(c => c.IsCompoundProduct && c.Locations.Select(l => l.Id).Contains(locationId));

                if (!string.IsNullOrEmpty(keyword))
                {
                    traderItems = traderItems.Where(
                        s => s.Name.Contains(keyword) || s.Description.Contains(keyword)
                            || s.SKU.Contains(keyword) || s.Barcode.Contains(keyword));
                }
                if (groupId > 0)
                {
                    traderItems = traderItems.Where(s => s.Group.Id == groupId);
                }

                traderItems = traderItems.Distinct();

                totalrecords = traderItems.Count();

                #endregion Filters

                #region Sorting

                var orderByString = string.Empty;
                foreach (var column in requestModel.Columns.GetSortedColumns())
                {
                    switch (column.Data)
                    {
                        case "Name":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Description":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Description" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Group":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Group.Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        default:
                            orderByString = "Name asc";
                            break;
                    }
                }

                traderItems = traderItems.OrderBy(orderByString == string.Empty ? "Name asc" : orderByString);

                #endregion Sorting

                #region Paging

                var list = traderItems.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                var dataJson = list.Select(q => new
                {
                    q.Id,
                    ImageUri = q.ImageUri.ToUri(),
                    q.Name,
                    Group = q.Group.Name,
                    q.Description
                });
                return new DataTablesResponse(requestModel.Draw, dataJson, totalrecords, totalrecords);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, locationId, userId, keyword);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }

        public List<TraderManufacturingView> ManufacturingHistoryViewer(int id, ref string subTotal, ref string manuJobName, ref string currencySymbol)
        {
            var views = new List<TraderManufacturingView>();
            var manuJob = dbContext.ManuJobs.FirstOrDefault(e => e.Id == id);
            if (manuJob == null)
                return views;
            //var traderItem = manuJob.Product;
            var locationId = manuJob.WorkGroup.Location.Id;
            //var recipe = manuJob.SelectedRecipe;
            var ingredients = manuJob.SelectedRecipe?.Ingredients ?? new List<Ingredient>();
            var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(manuJob.WorkGroup.Location.Domain.Id);
            currencySymbol = currencySetting.CurrencySymbol;

            decimal subTotalNumber = 0;
            manuJobName = manuJob.Product.Name;
            ingredients.ForEach(i =>
            {
                var ingredQty = i.Quantity;
                var ingredQtyOfBaseunit = i.Unit?.QuantityOfBaseunit ?? 0;
                var ingredAvgCost = i.SubItem?.InventoryDetails.FirstOrDefault(e => e.Location.Id == locationId)?.AverageCost ?? 0;

                var quantityConsumed = manuJob.Quantity * (manuJob.AssemblyUnit?.QuantityOfBaseunit ?? 0) * ingredQty;
                var unitCost = ingredAvgCost * ingredQtyOfBaseunit;
                var totalCost = quantityConsumed * unitCost;
                subTotalNumber += totalCost;

                views.Add(new TraderManufacturingView
                {
                    Item = i.SubItem.Name,
                    UnitOfMeasurement = i.Unit.Name,
                    QuantityConsumed = quantityConsumed.ToDecimalPlace(currencySetting),
                    UnitCost = unitCost.ToDecimalPlace(currencySetting),
                    TotalCost = totalCost.ToDecimalPlace(currencySetting)
                });
            });
            subTotal = subTotalNumber.ToDecimalPlace(currencySetting);
            return views;
        }
    }
}