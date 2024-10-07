using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro.Extensions;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroEventsRules : MicroRulesBase
    {
        public MicroEventsRules(MicroContext microContext) : base(microContext)
        {
        }

        public ReturnJsonModel CreateQbicleEventActivity(MicroEventQbicleModel eventQbicle)
        {
            var rules = new EventsRules(dbContext);
            var result = rules.DuplicateEventNameCheck(eventQbicle.QbicleId, eventQbicle.Id, eventQbicle.Name);
            if (result)
                return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_321") };

            try
            {
                eventQbicle.Start = eventQbicle.Start.ConvertTimeToUtc(CurrentUser.Timezone);
            }
            catch
            {
                eventQbicle.Start = DateTime.UtcNow;
            }

            var typeRules = new FileTypeRules(dbContext);

            var media = new MediaModel { };

            if (eventQbicle.Image != null)
                media = new MediaModel
                {
                    UrlGuid = eventQbicle.Image?.FileKey,
                    Name = eventQbicle.Image?.FileName,
                    Size = HelperClass.FileSize(int.Parse(eventQbicle.Image?.FileSize == "" ? "0" : eventQbicle.Image?.FileSize)),
                    Type = typeRules.GetFileTypeByExtension(eventQbicle.Image?.FileType) ?? typeRules.GetFileTypeByExtension(Path.GetExtension(eventQbicle.Image?.FileName))
                };
            if (!string.IsNullOrEmpty(media.UrlGuid))
            {
                if (media.Type == null)
                    return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_FILETYPE_406") };
            }
            var ev = new QbicleEvent
            {
                Id = eventQbicle.Id,
                Name = eventQbicle.Name,
                Description = eventQbicle.Description,
                Start = eventQbicle.Start,
                Location = eventQbicle.Location,
                EventType = eventQbicle.EventType,
                DurationUnit = eventQbicle.DurationUnit,
                Duration = eventQbicle.Duration,
                isRecurs = eventQbicle.IsRecurs,
                ActivityType = QbicleActivity.ActivityTypeEnum.EventActivity,
                App = QbicleActivity.ActivityApp.Qbicles
            };


            return rules.SaveEvent(ev,
                eventQbicle.Start.ToString(), eventQbicle.QbicleId, eventQbicle.Invites, null, media, CurrentUser.Id, eventQbicle.TopicId, null, null, eventQbicle.OriginatingConnectionId);


        }


        public MicroEventActivity GetQbicleEvent(int id)
        {
            var qbEvent = new EventsRules(dbContext).GetEventById(id).BusinessMapping(CurrentUser.Timezone);
            var members = new QbicleRules(dbContext).GetUsersByQbicleId(qbEvent.Qbicle.Id);
            members = members.Except(qbEvent.ActivityMembers).ToList();

            var qbicleSet = qbEvent.AssociatedSet;
            var invites = qbicleSet != null ? new TasksRules(dbContext).GetPeoples(qbicleSet.Id) : new List<QbiclePeople>();

            return qbEvent.ToMicro(members, CurrentUser, invites);
        }

        public ReturnJsonModel AttendingQbicleEventActivity(int attendingId, bool isAttending)
        {
            var attend = new EventsRules(dbContext).UpdateAttend(attendingId, isAttending);
            return new ReturnJsonModel { result = attend };
        }
    }
}
