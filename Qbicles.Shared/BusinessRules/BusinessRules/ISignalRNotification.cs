using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.B2B;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Broadcast;
using Qbicles.Models.MicroQbicleStream;
using System;
using System.Configuration;
using System.Linq;
using System.Text;
using static Qbicles.BusinessRules.HelperClass;
using static Qbicles.Models.Notification;
using static Qbicles.Models.QbicleActivity;

namespace Qbicles.BusinessRules
{
    public class ISignalRNotification : ServiceToken
    {
        /// <summary>
        ///  SignalR Send Notification to user if send method as broadcast
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected void SendBroadcastNotification(Notification parameter, object tradeOrder = null)
        {
            var hubConnection = GetHubConnection().Result;
            var hubProxy = hubConnection.CreateHubProxy(ConfigManager.HubName);
            hubConnection.Start().Wait();

            var icon = parameter.CreatedBy.ProfilePic;
            if (icon.Contains("avatar.jpg"))
                icon = ConfigManager.DefaultUserUrlGuid;
            hubProxy.Invoke(ConfigManager.HubInvokeMethod, new BroadcastMessage
            {
                OriginatingConnectionId = parameter.OriginatingConnectionId,
                OriginatingCreationId = parameter.OriginatingCreationId,
                UserReceived = parameter.NotifiedUser.UserName,
                NotificationId = parameter.Id,
                CreatedId = parameter.CreatedBy.Id,
                CreatedEmail = parameter.CreatedBy.Email,
                CreatedName = parameter.CreatedBy.GetFullName(),
                BroadcastEvent = parameter.Event,
                CreatedByIcon = icon.ToDocumentUri(Enums.FileTypeEnum.Image, "T"),
                TradeOder = tradeOrder
            }).Wait();
            hubConnection.Stop();
        }


        /// <summary>
        ///     render html append to page display
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="notify2ToUser"></param>
        /// <param name="appendToPageName"></param>
        /// <param name="userAvatarPathId"></param>
        /// <returns></returns>
        public string HtmlRender(object activities, ApplicationUser notify2ToUser, ApplicationPageName appendToPageName, NotificationEventEnum notifyEvent, Notification notification, bool isCustomerView = false)
        {
            if (activities == null)
                return "";
            notify2ToUser.Timezone = string.IsNullOrEmpty(notify2ToUser.Timezone)
                ? ConfigurationManager.AppSettings["Timezone"]
                : notify2ToUser.Timezone;
            var dateFormat = string.IsNullOrEmpty(notify2ToUser.DateFormat) ? "dd/MM/yyyy" : notify2ToUser.DateFormat;
            var render = new StringBuilder();
            if (activities is QbicleActivity)
                RenderActivity((QbicleActivity)activities, notify2ToUser, notifyEvent, ref render, isCustomerView);
            else if (activities is QbiclePost)
                switch (appendToPageName)
                {
                    case ApplicationPageName.Activities:
                        RenderPost2QbicleStream((QbiclePost)activities, ref render, notify2ToUser.Timezone, notify2ToUser.Id, notifyEvent);
                        break;
                    case ApplicationPageName.Task:
                    case ApplicationPageName.Event:
                    case ApplicationPageName.Media:
                    case ApplicationPageName.Alert:
                    case ApplicationPageName.Approval:
                    case ApplicationPageName.Link:
                    case ApplicationPageName.bookkeeping:
                    case ApplicationPageName.Discussion:
                    case ApplicationPageName.DiscussionMenu:
                        RenderPost2Activity((QbiclePost)activities, ref render, notify2ToUser.Id, dateFormat, notify2ToUser.Timezone);
                        break;
                    case ApplicationPageName.DiscussionOrder:
                        RenderPost2DiscussionOrder((QbiclePost)activities, ref render, notify2ToUser.Id, dateFormat, notify2ToUser.Timezone, notification);
                        break;
                }
            else if (activities is Qbicle)
                RenderQbicle2Domain((Qbicle)activities, ref render, notify2ToUser);
            else if (activities is TradeOrder)
            {

            }
            return render.ToString();
        }

        private void RenderQbicle2Domain(Qbicle qbicle, ref StringBuilder render, ApplicationUser notify2ToUser)
        {
            render.Append($"<article class='col' id='domain-qbicle-{qbicle.Id}'>");
            render.Append($"<span class='last-updated'>Updated {qbicle.LastUpdated.ConvertTimeFromUtc(notify2ToUser.Timezone).ToString(notify2ToUser.DateFormat + " " + notify2ToUser.TimeFormat)}</span>");
            render.Append($"<div class='btn-group optsnew defaulted dropdown'>");
            render.Append($"<button class='btn btn-default dropdown-toggle' data-toggle='dropdown'>");
            render.Append($"<i class='fa fa-cog'></i></button>");
            render.Append($"<ul class='dropdown-menu dropdown-menu-right'>");

            if (qbicle.Domain.Administrators.Any(a => a.Id == notify2ToUser.Id) || qbicle.Manager.Id == notify2ToUser.Id)
            {
                render.Append($"<li>");
                render.Append($"<a href=\"javascript:void(0)\" onclick=\"ConfigQbicle('{qbicle.Key})'\" data-target=\"#edit-qbicle\" data-toggle=\"modal\">");
                render.Append($"Edit</a></li>");
            }

            render.Append($"<li><a href=\"javascript:void(0)\" onclick=\"ShowOrHideQbicle('{qbicle.Key}',{qbicle.IsHidden})\">{(qbicle.IsHidden ? "Show" : "Hide")}</a></li>");
            render.Append($"</ul>");
            render.Append($"</div>");

            render.Append($"<a href='javascript:void(0)' onclick=\"QbicleSelected('" + qbicle?.Key + "', '" + Enums.QbicleModule.Dashboard + "'); \">");
            render.Append($"<div class='avatar' style='background-image: url(\"{qbicle.LogoUri.ToUri(Enums.FileTypeEnum.Image, "T")}\");'>&nbsp;</div>");
            render.Append($"<h1 style='color: #333;'>{qbicle.Name}</h1>");
            render.Append($"</a>");
            render.Append($"<p class='qbicle-detail text-detail'>{qbicle.Description}</p>");
            render.Append($"</article>");
        }

        private void RenderPost2Activity(QbiclePost post, ref StringBuilder render, string notify2ToUserId, string dateFormat, string currentTimeZone)
        {
            var cssValid = "";
            var createByName = GetFullNameOfUser(post.CreatedBy);
            if (post.BKTransaction != null)
                cssValid = "appentTransaction";
            var myPostOrReply = "";
            if (post.CreatedBy.Id == notify2ToUserId)
                createByName = "Me";
            render.AppendFormat($"<article id=\"post-{post.Id}\" class=\"activity post reprisedcomments animated bounceIn {myPostOrReply} {cssValid}\">");
            render.AppendFormat(
                $"<div class=\"activity-avatar\" style=\"background-image: url('{post.CreatedBy.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}'); \"></div>");
            render.AppendFormat("        <div class=\"activity-detail\">");
            render.AppendFormat("            <div class=\"activity-meta\">");
            render.AppendFormat($"                <h4>{createByName}</h4>");
            //render.AppendFormat("                <small class=\"db-date\">{0:" + dateFormat + ", hh:mmtt}</small>", post.StartedDate.ConvertTimeFromUtc(currentTimeZone));
            //render.AppendFormat($"                <small class=\"db-date\">{post.StartedDate.ConvertTimeFromUtc(currentTimeZone).GetTimeRelative()}</small>");
            render.AppendFormat($"                <small class=\"db-date\">{post.StartedDate.ConvertTimeFromUtc(currentTimeZone).ToString(dateFormat + ", hh:mmtt")}</small>");
            render.AppendFormat("            </div>");
            render.AppendFormat("            <div class=\"activity-overview media-comment\">");
            render.AppendFormat("                <p>{0}</p>", post.Message.Replace(Environment.NewLine, "<br/>"));
            render.AppendFormat("        </div>");
            render.AppendFormat("    </div>");
            render.AppendFormat("    <div class=\"clearfix\"></div>");
            render.AppendFormat("</article>");
        }

        private void RenderPost2DiscussionOrder(QbiclePost post, ref StringBuilder render, string notify2ToUserId, string dateFormat, string currentTimeZone, Notification notification)
        {
            var creatorTheQbcile = notification.AssociatedQbicle.GetCreatorTheQbcile();

            var createdBy = ""; var createdByImg = ""; var domainId = 0;

            createdBy = notification.CreatedBy.GetFullName();
            createdByImg = notification.CreatedBy?.ProfilePic.ToUriString(Enums.FileTypeEnum.Image, "T");
            //if bussiness
            if (notification.IsCreatorTheCustomer == false)
            {
                if (creatorTheQbcile == QbicleType.B2CQbicle)
                {
                    domainId = (notification.AssociatedQbicle as B2CQbicle).Business.Id;
                    var b2BProfiles = new ApplicationDbContext().B2BProfiles.AsNoTracking().Where(s => s.Domain.Id == domainId).FirstOrDefault() ?? new B2BProfile();
                    createdBy = b2BProfiles.BusinessName;
                    createdByImg = b2BProfiles.LogoUri.ToUriString(Enums.FileTypeEnum.Image, "T");
                }
            }
            else
            {
                createdBy = post.CreatedBy.GetFullName();
                if (post.CreatedBy.Id == notify2ToUserId)
                    createdBy = "Me";
            }

            render.AppendFormat($"<div class='activity-overview post-mini animated fadeInUp newpost'>");
            render.AppendFormat($"<p>{post.Message.Replace(Environment.NewLine, "<br/>")}</p>");
            render.AppendFormat($"<a href='#'>");
            render.AppendFormat($"<div class='activity-avatar' style='background-image: url(\"{createdByImg}\");'></div>");
            render.AppendFormat($"<h5>{createdBy}, {post.StartedDate.ConvertTimeFromUtc(currentTimeZone).GetTimeRelative()}</h5>");
            render.AppendFormat($"</a>");
            render.AppendFormat($"</div>");
            render.AppendFormat($"");
            render.AppendFormat($"");
            render.AppendFormat($"");
            render.AppendFormat($"");
        }

        private void RenderPost2QbicleStream(QbiclePost post, ref StringBuilder render, string currentTimeZone, string notify2ToUserId, NotificationEventEnum notifyEvent)
        {
            var createByName = GetFullNameOfUser(post.CreatedBy);

            var postEvent = "Added post";
            if (notifyEvent == NotificationEventEnum.PostEdit)
                postEvent = "Updated post";
            var myPostOrReply = "";
            string posKey = post.Key;
            if (post.CreatedBy.Id == notify2ToUserId)
                myPostOrReply = "me";
            var datePost = DateTime.UtcNow.Date == post.StartedDate.Date ? "Today, " + post.StartedDate.ConvertTimeFromUtc(currentTimeZone).ToString("hh:mmtt") : post.StartedDate.ConvertTimeFromUtc(currentTimeZone).ToString("dd MMM yyyy, hh:mmtt");
            render.Append($"<article id=\"post-{post.Id}\" class=\"activity post {myPostOrReply}\">");
            render.Append(
                $"<div class=\"activity-avatar\" style=\"background-image: url('{post.CreatedBy.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}'); \"></div>");
            render.Append("           <div class=\"activity-detail\">");
            render.Append("               <div class=\"activity-meta\" style=\"position: relative;\">");
            render.Append($"                   <h4>{createByName}</h4>");
            render.Append($"<small>{datePost}</small>");
            render.Append("<br class=\"visible-xs\">");
            render.Append(
                $"<a href=\"javascript:void(0);\" onclick=\"ShowTopic(event,{post.Topic.Id});\" class=\"topic-label\"><span class=\"label label-info\">{post.Topic.Name}</span></a>");
            render.Append($"&nbsp;<span class='label label-warning post-event'>{postEvent}</span>");
            //Update dropdown toggle button attached to the upper-right corner of these loose posts
            render.Append("<div class=\"btn-group optsnew defaulted dropdown\" style=\"top: -8px; right: auto;\">");
            render.Append("<button class=\"btn btn-default dropdown-toggle\" data-toggle=\"dropdown\" style=\"background: #fff !important;\" aria-expanded=\"false\"><i class=\"fa fa-user-cog\" style=\"font-size: 14px;\"></i></button>");
            render.Append("<ul class=\"dropdown-menu dropdown-menu-right\">");
            render.Append($"<li><a href=\"#\" onclick=\"addPostToDiscuss('{posKey}')\">Discuss</a></li>");
            render.Append($"<li class=\"op-forward\"><a href=\"#\" onclick=\"showForwardPostModal('{posKey}')\">Forward</a></li>");
            if (post.CreatedBy.Id == notify2ToUserId)
            {
                render.Append($"<li><a href=\"#\" onclick=\"loadEditPostModal('post-{post.Id}','{posKey}')\">Edit</a></li>");
                render.Append($"<li><a href=\"#\" onclick=\"deletePost('post-{post.Id}','{posKey}')\">Delete</a></li>");
            }
            render.Append("</ul></div>");
            //end
            render.Append("               </div>");
            render.Append("               <div class=\"activity-overview\">");
            render.Append($"                   <p>{post.Message.Replace(Environment.NewLine, "<br/>")}</p>   ");
            render.Append("               </div>                         ");
            render.Append("           </div>                             ");
            render.Append("           <div class=\"clearfix\"></div>       ");
            render.Append("       </article>                             ");
            render.Append("       <div class=\"clearfix\"></div>           ");
        }


        private void RenderMedia(StringBuilder render, QbicleMedia media, string timezone, string myPostOrReply)
        {
            var css = "";
            var dateStr = media.TimeLineDate.Date == DateTime.UtcNow.Date
                ? "Today, " + media.TimeLineDate.ConvertTimeFromUtc(timezone).ToString("hh:mmtt")
                : media.TimeLineDate.ConvertTimeFromUtc(timezone).ToString("dd MMM yyyy, hh:mmtt");
            render.Append($"<article id = 'activity-{media.Id}'" + $" class=\"activity media {myPostOrReply} \">");
            render.Append(
                $"<div class=\"activity-avatar\" style=\"background-image: url('{media.StartedBy.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}'); \"></div>");
            render.Append("<div class=\"activity-detail\">");
            render.Append("<div class=\"activity-meta\">");
            render.Append($"<h4>{GetFullNameOfUser(media.StartedBy)}</h4>");
            render.Append($"<small>{dateStr}</small>");
            render.Append("<br class=\"visible-xs\">");
            render.Append("<a href=\"javascript:void(0);\" " +
                          $"onclick=\"ShowTopic(event,'{media.Topic.Id}');\" " +
                          $"class=\"topic-label\"><span class=\"label label-info\">{media.Topic.Name}</span></a>");
            if (media.UpdateReason == ActivityUpdateReasonEnum.NoUpdates)
                css = "update-reason-hide";
            render.Append($" <span class='label label-info {css}'>{media.UpdateReason.GetDescription()}</span> ");

            render.Append("<span class=\"label label-default pin-this\" " +
                          $"id=\"pinIcon-{media.Id}\" onclick=\"PinnedActivity('{media.Id}',false,event)\">" +
                          "<i class=\"fa fa-thumb-tack\"></i></span>");
            render.Append("</div>");
            render.Append("<div class=\"activity-overview media\">");
            render.Append("<div class=\"row\">");
            render.Append("<div class=\"col-xs-12\">");

            //media preview
            var mediaLastUpdateS = media.VersionedFiles.Where(e => !e.IsDeleted).OrderByDescending(x => x.UploadedDate)
                .FirstOrDefault();

            //render.Append($"<img id='{documentPathId}' src='' class='img-responsive'>");
            if (media.FileType.Type.Equals("Image File", StringComparison.OrdinalIgnoreCase))
            {
                render.Append($"<a href=\"javascript:void(0);\" id='media-uri' onclick=\"ShowMediaPage('{media.Key}',false);\" class=\"stream-img-preview\" style=\"background-image: url('{mediaLastUpdateS?.Uri.ToUri()}');\">");
            }
            else if (media.FileType.Type.Equals("Compressed File", StringComparison.OrdinalIgnoreCase))
            {
                render.Append($"<a href=\"javascript:void(0);\" onclick=\"ShowMediaPage('{media.Key}',false);\">");
                render.Append($"<img id='media-uri' src='{media.FileType.IconPath}' />");
            }
            else if (media.FileType.Type.Equals("Video File", StringComparison.OrdinalIgnoreCase))
            {
                render.Append($"<a href=\"javascript:void(0);\" onclick=\"ShowMediaPage('{media.Key}',false);\">");
                render.Append(
                    "<video width='640' height='320' controls='' id='embed' style='display: inline-block;' class='fancybox-video'>");
                render.Append(
                    $"<source src='{string.Format(ConfigManager.ApiGetVideoUri, mediaLastUpdateS?.Uri, "mp4")}' type='video/mp4'>");
                render.Append(
                    $"<source src='{string.Format(ConfigManager.ApiGetVideoUri, mediaLastUpdateS?.Uri, "webm")}' type='video/webm'>");
                render.Append(
                    $"<source src='{string.Format(ConfigManager.ApiGetVideoUri, mediaLastUpdateS?.Uri, "ogv")}' type='video/ogv'>");
                render.Append("</video>");
            }
            else
            {
                render.Append($"<a href=\"javascript:void(0);\" onclick=\"ShowMediaPage('{media.Key}',false);\">");
                render.Append($"<img id='media-uri' src='{media.FileType.IconPath}'/>");
            }

            render.Append("</a>");
            render.Append("</div>");
            render.Append("<div class=\"col-xs-12 description\">");
            render.Append($"<a href=\"javascript:void(0);\" onclick=\"ShowMediaPage('{media.Key}',false);\">");
            render.Append($"<h5>{media.Name}</h5>");
            render.Append("<p>");
            render.Append($"{(media.Description != null ? media.Description.Replace(Environment.NewLine, "<br/>") : "")}");
            render.Append("</p>");
            render.Append(
                $"<small>{Utility.GetFileTypeDescription(media.FileType.Extension)} | {media.VersionedFiles.Where(e => !e.IsDeleted).OrderByDescending(x => x.UploadedDate).FirstOrDefault()?.UploadedDate.ConvertTimeFromUtc(timezone):dd MMM yyyy, hh:mmtt}</small>");
            render.Append("</a>");
            render.Append("</div>");
            render.Append("</div>");
            render.Append("</div>");
            render.Append("</div>");
            render.Append("<div class=\"clearfix\"></div>");
            render.Append("</article>");
            render.Append("<div class=\"clearfix\"></div>");
        }

        private void RenderActivity(QbicleActivity activity, ApplicationUser notify2ToUser, NotificationEventEnum notifyEvent, ref StringBuilder render, bool isCustomerView = false)
        {
            render = ActivityPostHtmlTemplateRules.GetActivityHtmlForSignalR(activity, notifyEvent, notify2ToUser.Id, notify2ToUser.Timezone, notify2ToUser.DateFormat, isCustomerView);
        }
    }
}