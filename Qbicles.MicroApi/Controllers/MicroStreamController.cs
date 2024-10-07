using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.MicroQbicleStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using static Qbicles.BusinessRules.Enums;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroStreamController : BaseApiController
    {
        [Route("stream/filteroption")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage StreamFilterOption(int qbicleId)
        {
            HeaderInformation(Request);
            try
            {
                var options = new StreamFilterOption
                {
                    ActivityTypes = new List<BaseModel> {
                        new BaseModel { Id = (int)Enums.QbicleModule.Discussions, Name = Enums.QbicleModule.Discussions.GetDescription() },
                        new BaseModel { Id = (int)Enums.QbicleModule.Approvals, Name = Enums.QbicleModule.Approvals.GetDescription() },
                        new BaseModel { Id = (int)Enums.QbicleModule.Events, Name = Enums.QbicleModule.Events.GetDescription() },
                        new BaseModel { Id = (int)Enums.QbicleModule.Media, Name = Enums.QbicleModule.Media.GetDescription() },
                        new BaseModel { Id = (int)Enums.QbicleModule.Post, Name = Enums.QbicleModule.Post.GetDescription() },
                        new BaseModel { Id = (int)Enums.QbicleModule.Links, Name = Enums.QbicleModule.Links.GetDescription() },
                        new BaseModel { Id = (int)Enums.QbicleModule.Tasks, Name = Enums.QbicleModule.Tasks.GetDescription() }
                    },
                    Topics = _microContext.Context.Topics.Where(e => e.Qbicle.Id == qbicleId).Select(t => new BaseModel { Id = t.Id, Name = t.Name }).ToList(),
                    AppTypes = { "Bookkeeping", "CleanBooks", "Operator", "Trader", "SalesAndMarketing", "Spannered" }
                };

                return Request.CreateResponse(HttpStatusCode.OK, options, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("stream/calendarfilteroption")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage StreamCalendarFilterOption(int qbicleId)
        {
            HeaderInformation(Request);
            try
            {
                var options = new StreamCalendarFilterOption
                {
                    ActivityTypes = new List<BaseModel> {
                        new BaseModel { Id = 1, Name = Enums.QbicleModule.Discussions.GetDescription() },
                        new BaseModel { Id = 12, Name = Enums.QbicleModule.Links.GetDescription() },
                        new BaseModel { Id = 8, Name = Enums.QbicleModule.Approvals.GetDescription() },
                        new BaseModel { Id = 4, Name = Enums.QbicleModule.Events.GetDescription() },
                        new BaseModel { Id = 5, Name = Enums.QbicleModule.Media.GetDescription() },
                        new BaseModel { Id = 3, Name = Enums.QbicleModule.Alerts.GetDescription() },
                        new BaseModel { Id = 11, Name = "Approval App" },
                        new BaseModel { Id = 2, Name = Enums.QbicleModule.Tasks.GetDescription() }
                    },
                    Topics = _microContext.Context.Topics.Where(e => e.Qbicle.Id == qbicleId).Select(t => new BaseModel { Id = t.Id, Name = t.Name }).ToList(),
                    AppTypes = { "Bookkeeping", "CleanBooks", "Operator", "Trader", "Sales&Marketing", "Spannered" },
                    CalendarTypes = { "today", "week", "month" },
                    Orderby = new Dictionary<string, string>
                    {
                        { "TimelineDate desc", "Last updated (newest first)" },
                        { "TimelineDate asc", "Last updated (oldest first)" },
                        { "isComplete asc", "Status" }
                    },
                    Users = new List<MicroUserBase>()
                };
                var users = _microContext.Context.Qbicles.Find(qbicleId).Members.ToList();
                users.ForEach(m =>
                {
                    options.Users.Add(new MicroUserBase
                    {
                        Id = m.Id,
                        FullName = m.GetFullName(),
                        ProfilePic = m.ProfilePic.ToUri(FileTypeEnum.Image, "T")
                    });
                });
                return Request.CreateResponse(HttpStatusCode.OK, options, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("stream/calendar")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage MicroStreamCalendar(CalendarFilterModel filter)
        {
            HeaderInformation(Request);
            try
            {
                var microStreams = new MicroStreamRules(_microContext).CalendarStream(filter);
                return Request.CreateResponse(HttpStatusCode.OK, microStreams, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("stream/calendarcontrol")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage CalendarDataControl(int qbicleId, int? year, int? month)
        {
            HeaderInformation(Request);
            try
            {
                var microStreams = new MicroStreamRules(_microContext).CalendarDataControl(qbicleId, year, month);
                return Request.CreateResponse(HttpStatusCode.OK, microStreams, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("stream/activities")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage MicroStreamActivities(QbicleFillterModel filter)
        {
            HeaderInformation(Request);
            try
            {
                filter.UserId = _microContext.UserId;

                var microStreams = new MicroStreamRules(_microContext).QbiclesStream(filter);
                return Request.CreateResponse(HttpStatusCode.OK, microStreams, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        /// Qbicles stream
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [Route("stream/activities/list")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage MicroQbicleStreams(MicroStreamParameter filter)
        {
            HeaderInformation(Request);
            try
            {
                filter.UserId = _microContext.UserId;

                var microStreams = new MicroStreamRules(_microContext).MicroQbicleStreams(filter);
                return Request.CreateResponse(HttpStatusCode.OK, microStreams, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        /// Community stream ( B2C, Community detail, filter)
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [Route("b2c/comms/communicate/details")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage B2CCommDetail(MicroStreamParameter filter)
        {
            HeaderInformation(Request);

            if (!string.IsNullOrEmpty(filter.Key))
                filter.QbicleId = filter.Key.Decrypt2Int();
            //filter.Type = 1;

            var refModel = new MicroStreamRules(_microContext).MicroCommunityStreams(filter);
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }

        [Route("chat/visibility/alert")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage ChatVisibilityAlert(int id)
        {
            HeaderInformation(Request);

            var refModel = new MicroStreamRules(_microContext).ChatVisibilityAlert(id);
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }

        [Route("chat/visibility/alert/list")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage ChatVisibilityAlertList(AlertNotificationParameter parameter)
        {
            HeaderInformation(Request);

            var refModel = new MicroStreamRules(_microContext).ChatVisibilityAlertList(parameter);
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }
    }
}
