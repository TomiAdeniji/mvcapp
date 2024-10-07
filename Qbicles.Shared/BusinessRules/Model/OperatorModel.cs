using Qbicles.Models;
using Qbicles.Models.Form;
using Qbicles.Models.Operator;
using Qbicles.Models.Operator.Compliance;
using Qbicles.Models.Operator.Goals;
using System;
using System.Collections.Generic;
using static Qbicles.Models.QbicleTask;

namespace Qbicles.BusinessRules.Model
{
    public class OperatorTagModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public string Creator { get; set; }
        public string CreatorId { get; set; }
        public string Created { get; set; }
        public int Instances { get; set; }
    }

    public class OperatorLocationModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Creator { get; set; }
        public string CreatorId { get; set; }
        public string Created { get; set; }
    }

    public class OperatorWorkgroupModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Creator { get; set; }
        public string Qbicle { get; set; }
        public int Members { get; set; }
        public string CreatorId { get; set; }
        public string Created { get; set; }
        public int Instances { get; set; }
    }

    public class OperatorWorkgroupCustom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public QbicleDomain Domain { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public int LocationId { get; set; }
        public int QbicleId { get; set; }
        public int TopicId { get; set; }
        public WorkGroupTypeEnum Type { get; set; }
        public string TeamMembers { get; set; }
        public string TaskMembers { get; set; }
    }

    public class TeamMember
    {
        public string Id { get; set; }
        public string Role { get; set; }
    }

    public class TaskMember
    {
        public string Id { get; set; }
        public bool IsTaskCreator { get; set; }
    }

    public class OperatorPersonModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MemberId { get; set; }
        public string Role { get; set; }
        public string Location { get; set; }
        public string Workgroup { get; set; }
        public string Avatar { get; set; }
    }
    public class OperatorGoalModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public List<int> Tags { get; set; }
        public string FeaturedImageURI { get; set; }
        public MediaModel MediaResponse { get; set; }
        public string sLeadingIndicators { get; set; }
        public string sGoalMeasures { get; set; }
        public List<OperatorMeasureModel> LeadingIndicators { get; set; }
        public List<OperatorMeasureModel> GoalMeasures { get; set; }
        public QbicleDomain Domain { get; set; }
        public ApplicationUser CurrentUser { get; set; }
    }
    public class OperatorMeasureModel
    {
        public int Id { get; set; }
        public int MeasureId { get; set; }
        public int Weight { get; set; }
        public string MeasureName { get; set; }
        public string MeasureDesc { get; set; }
        public decimal? Score { get; set; }
        public int SubmitCount { get; set; }
        public GoalMeasureTypeEnum Type { get; set; }
    }
    public class OperatorMonitorData
    {
        public int MeasureId { get; set; }
        public int Weight { get; set; }
        public decimal Score { get; set; }
        public DateTime Day { get; set; }
    }
    public class OperatorMonitorDataChart
    {
        public int MeasureId { get; set; }
        public int Weight { get; set; }
        public decimal Score { get; set; }
        public string Day { get; set; }
    }
    public class OperatorGoalChart
    {
        public int goalId { get; set; }
        public int timeframe { get; set; }
        public string customDate { get; set; }
        public string currentTimeZone { get; set; }
        public string dateFormat { get; set; }
        public bool isDay { get; set; }
    }
    public class GoalChart
    {
        public decimal TotalProgressGoal { get; set; }
        public List<OperatorMeasureModel> ChartProcessMeasures { get; set; }
        public IEnumerable<dynamic> CharMonitorMeasures { get; set; }
    }
    public class OperatorClockIn
    {
        public int Id { get; set; }
        public virtual List<string> Peoples { get; set; }
        public int Workgroup { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Notes { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public string Dateformat { get; set; }
        public string Timezone { get; set; }
        
    }
    public class OperatorScheduleModel
    {
        public int Workgroup { get; set; }
        public List<string> Employees { get; set; }
        public List<string> Days { get; set; }
        public string ShiftStart { get; set; }
        public string ShiftEnd { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public string Dateformat { get; set; }
        public string Timezone { get; set; }
        public QbicleDomain Domain { get; set; }
    }
    public class OperatorScheduleUpdateModel
    {
        public int Id { get; set; }
        public string ShiftStart { get; set; }
        public string ShiftEnd { get; set; }
        public string Dateformat { get; set; }
        public string Timezone { get; set; }
    }
    public class OperatorTeamMembersModel
    {
        public string UserId { get; set; }
        public int TeamPersonId { get; set; }
        public string Fullname { get; set; }
        public string AvatarUrl { get; set; }
    }
    public class OperatorScheduleDailyModel
    {
        public int Id { get; set; }
        public string PersonId { get; set; }
        public string PersonName { get; set; }
        public string PersonUrl { get; set; }
        public string PersonJobtile { get; set; }
        public string Shift { get; set; }
        public string Location { get; set; }
    }
    public class OperatorSearchScheduleModel
    {
        public int DomainId { get; set; }
        public string Day { get; set; }
        public string Week { get; set; }
        public string Month { get; set; }
        public List<string> Peoples { get; set; }
        public List<int> Roles { get; set; }
        public List<int> Locations { get; set; }
        public string Dateformat { get; set; }
        public string Timezone { get; set; }
    }
    public class OperatorEmployeeModel
    {
        public string Id { get; set; }
        public string Location { get; set; }
    }
    public class OperatorScheduleWeekModel
    {
        public string PersonId { get; set; }
        public string PersonName { get; set; }
        public string PersonUrl { get; set; }
        public string PersonJobtile { get; set; }
        public string LabelColumn { get; set; }
        public double Value { get; set; }
    }
    public class OperatorWorkgroupPreviewModel
    {
        public string localtion { get; set; }
        public string qbicle { get; set; }
        /// <summary>
        /// Count All members in workgroup
        /// </summary>
        public int countmember { get; set; }
        /// <summary>
        /// Only takes members have role is Member
        /// </summary>
        public List<OperatorTeamMembersModel> members { get; set; }
    }

    public class PerformanceTrackingModel
    {
        public int Id { get; set; }
        public int WorkgroupId { get; set; }
        public int TeamPersonId { get; set; }
        public string Description { get; set; }
        public string TrackingMeasures { get; set; }
    }

    public class TrackingMesureModel
    {
        public int MeasureId { get; set; }
        public int Weight { get; set; }
    }

    public class WorkgroupInforModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Process { get; set; }
        public string Qbicle { get; set; }
        public int Members { get; set; }
        public List<PersonModal> Persons { get; set; } = new List<PersonModal>();
    }

    public class PersonModal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ProfilePic { get; set; }
    }

    public class OperatorPerformanceChart
    {
        public int performanceId { get; set; }
        public int timeframe { get; set; }
        public string customDate { get; set; }
        public string currentTimeZone { get; set; }
        public string dateFormat { get; set; }
        public bool isDay { get; set; }
    }

    public class PerformanceChart
    {
        public decimal TotalProgressPerformance { get; set; }
        public List<OperatorMeasureModel> ChartProcessMeasures { get; set; }
        public IEnumerable<dynamic> CharMonitorMeasures { get; set; }
    }
    public class OperatorFormModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int EstimatedTime { get; set; }
        public bool IsDraft { get; set; }
        public int DomainId { get; set; }
        public List<int> Tags { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public List<OperatorFormElementModel> FormElements { get; set; }

    }
    public class OperatorFormElementModel
    {
        public int Id { get; set; }
        public int DisplayOrder { get; set; }
        public FormElementType Type { get; set; }
        public string Label { get; set; }
        public bool AllowPhotos { get; set; }
        public bool AllowDocs { get; set; }
        public bool AllowNotes { get; set; }
        public bool AllowScore { get; set; }
        public int AssociatedMeasureId { get; set; }
    }
    public class OperatorSearchFormModel
    {
        public string keyword { get; set; }
        public int tag { get; set; }
        public string timezone { get; set; }
        public string dateformat { get; set; }
        public int domainId { get; set; }
    }
    public class OperatorTaskModel
    {
        public int Id { get; set; }
        public int WorkgroupId { get; set; }
        public string TaskName { get; set; }
        public string Assignee { get; set; }
        public string TaskDescription { get; set; }
        public string ExpectedEnd { get; set; }
        public List<int> Forms { get; set; }
        public TaskType TaskType { get; set; }
        public int? RecurrenceType { get; set; }
        public bool isRecurs { get; set; }
        public TaskDurationUnitEnum DurationUnit { get; set; }
        public int Duration { get; set; }
        public string RecurStart { get; set; }
        public string RecurEnd { get; set; }
        public string DayOrMonth { get; set; }
        public int? Pattern { get; set; }
        public int? CustomDate { get; set; }
        public short? Monthdates { get; set; }
        public List<string> ListDates { get; set; }
        public string CurrentDateTimeFormat { get; set; }
        public string Timezone { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public MediaModel MediaResponse { get; set; }
        public QbicleDomain CurrentDomain { get; set; }
    }
    public class OperatorSearchTaskModel
    {
        public string keyword { get; set; }
        public string assignee { get; set; }
        public int form { get; set; }
        public string timezone { get; set; }
        public string dateformat { get; set; }
        public int domainId { get; set; }
        public int complianceTaskId { get; set; }
        public int taskId { get; set; }
    }
    public class OperatorElementDataModel
    {
        public int Id { get; set; }
        public int FormElementId { get; set; }
        public string Value { get; set; }
        public decimal Score { get; set; }
        public string ImageKey { get; set; }
        public string ImageName { get; set; }
        public string ImageSize { get; set; }

        public MediaModel ImageFileResponse { get; set; }
        public string DocKey { get; set; }
        public string DocName { get; set; }
        public string DocSize { get; set; }
        public MediaModel DocFileResponse { get; set; }
        public string Note { get; set; }
    }
    public class OperatorFormSubmissionsModel
    {
        public int FormInstanceId { get; set; }
        public int TaskInstanceId { get; set; }
        public int FormDefinitionId { get; set; }
        public List<OperatorElementDataModel> Elements{ get; set; }
        public ApplicationUser CurrentUser { get; set; }
    }


}
