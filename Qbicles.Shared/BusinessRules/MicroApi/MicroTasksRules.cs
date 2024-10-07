using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro.Extensions;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.Models;
using System;
using System.IO;
using System.Linq;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroTasksRules : MicroRulesBase
    {
        public MicroTasksRules(MicroContext microContext) : base(microContext)
        {
        }

        public ReturnJsonModel CreateQbicleTaskActivity(MicroTaskQbicleModel taskQbicle, bool isCreatorTheCustomer)
        {
            var rules = new TasksRules(dbContext);
            var result = rules.DuplicateTaskNameCheck(taskQbicle.QbicleId, taskQbicle.Id, taskQbicle.Name);
            if (result)
                return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_111") };
            //var typeRules = new FileTypeRules(dbContext);
            //var media = new MediaModel { };

            //if (taskQbicle.Image != null)
            //    media = new MediaModel
            //    {
            //        UrlGuid = taskQbicle.Image.FileKey,
            //        Name = taskQbicle.Image.FileName,
            //        Size = HelperClass.FileSize(int.Parse(taskQbicle.Image.FileSize == "" ? "0" : taskQbicle.Image.FileSize)),
            //        Type = typeRules.GetFileTypeByExtension(taskQbicle.Image.FileType) ?? typeRules.GetFileTypeByExtension(Path.GetExtension(taskQbicle.Image.FileName))
            //    };
            //if (!string.IsNullOrEmpty(media.UrlGuid))
            //    if (media.Type == null)
            //        return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_FILETYPE_406") };
            try
            {
                taskQbicle.ProgrammedStart = taskQbicle.ProgrammedStart.ConvertTimeToUtc(CurrentUser.Timezone);
            }
            catch
            {
                taskQbicle.ProgrammedStart = DateTime.UtcNow;
            }

            var task = new QbicleTask
            {
                Id = taskQbicle.Id,
                Name = taskQbicle.Name,
                Description = taskQbicle.Description,
                ProgrammedStart = taskQbicle.ProgrammedStart,
                ActivityType = QbicleActivity.ActivityTypeEnum.TaskActivity,
                App = QbicleActivity.ActivityApp.Qbicles,
                IsVisibleInQbicleDashboard = true,
                isRecurs = false,
                isSteps = false,
                isComplete = false,
                Duration = taskQbicle.Duration,
                DurationUnit = taskQbicle.DurationUnit,
                State = QbicleActivity.ActivityStateEnum.Open,
                UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NoUpdates,
                Priority = taskQbicle.Priority,
                Repeat = QbicleTask.TaskRepeatEnum.No,
                ActualStart = null,
                IsCreatorTheCustomer = isCreatorTheCustomer
            };
            var refMode = rules.
                SaveTask(task, isCreatorTheCustomer, taskQbicle.Assignee, null, taskQbicle.Watchers, taskQbicle.QbicleId,
                CurrentUser.Id, taskQbicle.TopicId, null, null, null, null, taskQbicle.OriginatingConnectionId);

            if (refMode.result)
            {
                refMode.msg = taskQbicle.Priority.GetDescription();
                refMode.msgName = taskQbicle.ProgrammedStart?.ToString(CurrentUser.DateFormat + " " + CurrentUser.TimeFormat);
            }
            return refMode;

        }

        public ReturnJsonModel TaskTimeLogging(TimeLogging timeLogging, bool isCreatorTheCustomer)
        {
            var taskRules = new TasksRules(dbContext);
            var task = taskRules.GetTaskById(timeLogging.TaskId);
            if (!taskRules.CheckAddWorkLog(task, CurrentUser.Id))
                return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_392") };

            var timeSpent = new QbicleTimeSpent
            {
                Task = task,
                DateTime = DateTime.UtcNow,
                Days = timeLogging.Days,
                Hours = timeLogging.Hours,
                Minutes = timeLogging.Minutes
            };

            taskRules.AddWorkLog(timeSpent, isCreatorTheCustomer, timeLogging.OriginatingConnectionId);
            return new ReturnJsonModel
            {
                result = true,
                Object = new
                {
                    DateTime = timeSpent.DateTime.ConvertTimeFromUtc(CurrentUser.Timezone).ToString("dddd dd\"th\" MMMM yyyy"),
                    timeSpent.Id,
                    timeSpent.Days,
                    timeSpent.Hours,
                    timeSpent.Minutes
                }
            };
        }

        public ReturnJsonModel TimeLoggingDelete(int id)
        {
            return new TasksRules(dbContext).RemoveWorkLog(id);
        }

        public MicroTaskActivity GetQbicleTask(int id)
        {
            var qbTask = new TasksRules(dbContext).GetTaskById(id).BusinessMapping(CurrentUser.Timezone);
            var members = new QbicleRules(dbContext).GetUsersByQbicleId(qbTask.Qbicle.Id);
            members = members.Except(qbTask.ActivityMembers).ToList();

            //var taskAssignee = qbTask.AssociatedSet.Peoples.FirstOrDefault(m => m.Type == QbiclePeople.PeopleTypeEnum.Assignee);

            //var qbicleSet = qbTask.AssociatedSet;
            //var invites = qbicleSet != null ? new TasksRules(dbContext).GetPeoples(qbicleSet.Id) : new List<QbiclePeople>();




            return qbTask.ToMicro(members, CurrentUser);
        }


        public ReturnJsonModel StartProgress(int id, string originatingConnectionId)
        {
            var user = CurrentUser;
            return new TasksRules(dbContext).MicroStartProgress(id, user.Id, user.GetFullName(), originatingConnectionId);
        }

        public ReturnJsonModel MarkAsComplete(int id, string originatingConnectionId)
        {
            var user = CurrentUser;
            return new TasksRules(dbContext).MicroMarkAsComplete(id, user.Id, user.GetFullName(), originatingConnectionId);
        }

    }
}
