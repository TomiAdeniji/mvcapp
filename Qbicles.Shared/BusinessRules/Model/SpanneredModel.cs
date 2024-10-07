using Qbicles.Models;
using System.Collections.Generic;
using static Qbicles.Models.Spannered.ConsumptionReport;

namespace Qbicles.BusinessRules.Model
{
    public class AssetTasksModel
    {
        public int Id { get; set; }
        public string ActivityKey { get; set; }
        public string Name { get; set; }
        public string Assignee { get; set; }
        public string Due { get; set; }
        public string Created { get; set; }
        public int? MeterThreshold { get; set; }
        public string Status { get; set; }
        public bool IsAllowEdit { get; set; }
    }
    public class AssetTaskCPSItem {
        public int Id { get; set; }
        public int AssetInventoryId { get; set; }
        public decimal Allocated { get; set; }
    }
    public class ConsumeReportCustome
    {
        public int WorkgroupId { get; set; }
        public int LocationId { get; set; }
        public int TaskId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public QbicleDomain Domain { get; set; }
        public ApplicationUser user { get; set; }
        public ConsumptionReportStatusEnum Status { get; set; }
        public List<ConsumeReportItemCustome> Items { get; set; }
    }
    public class ConsumeReportItemCustome
    {
        public int ItemId { get; set; }
        public int UnitId { get; set; }
        public decimal Allocated { get; set; }
        public int Used { get; set; }
        public string Note { get; set; }
    }
    public class ProcessesConst
    {
        public const string Assets = "Assets";
        public const string AssetTasks = "Asset Tasks";
        public const string Meters = "Meters";
        public const string Purchases = "Purchases";
        public const string Transfers = "Transfers";
        public const string ConsumptionReports = "Consumption Reports";
    }
}
