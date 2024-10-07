using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.B2B;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Catalogs;
using Qbicles.Models.Highlight;
using Qbicles.Models.Loyalty;
using Qbicles.Models.Manufacturing;
using Qbicles.Models.Operator.Compliance;
using Qbicles.Models.SalesMkt;
using Qbicles.Models.SystemDomain;
using Qbicles.Models.Trader.Budgets;
using Qbicles.Models.Trader.Inventory;
using Qbicles.Models.Trader.Movement;
using Qbicles.Models.Trader.Payments;
using Qbicles.Models.Trader.Returns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static Qbicles.Models.Notification;
using static Qbicles.Models.QbicleActivity;
using static Qbicles.Models.QbicleDiscussion;
using static Qbicles.Models.QbicleTask;

namespace Qbicles.BusinessRules
{
    public class ActivityPostHtmlTemplateRules
    {
        #region Generate HTML templates
        private static string PostHtmlTemplate(PostTemplateModel model)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine($"<article id=\"post-{model.POST_ID}\" class=\"activity post {model.CSS_MYPOST_REPLY}\">");
            htmlBuilder.AppendLine($"<div class=\"activity-avatar\" style=\"background-image: url('{model.AVATAR}');\"></div>");
            htmlBuilder.AppendLine("<div class=\"activity-detail\">");
            htmlBuilder.AppendLine($"<div class=\"activity-meta\" style=\"position:relative\"><h4>{model.CREATE_BY_NAME}</h4><small>{model.TIMELINE_DATE}</small><br class=\"visible-xs\">");
            if (model.TOPIC_ID > 0 && !string.IsNullOrEmpty(model.TOPIC_NAME))
                htmlBuilder.AppendLine($"<a href='javascript:void(0);' onclick='ShowTopic(event, {model.TOPIC_ID}, this);' class=\"topic-label\"><span class=\"label label-info\">{model.TOPIC_NAME}</span></a>");
            htmlBuilder.AppendLine("<div class=\"btn-group optsnew defaulted dropdown\" style=\"top: -8px; right: auto;\">");
            htmlBuilder.AppendLine("<button class=\"btn btn-default dropdown-toggle\" data-toggle=\"dropdown\" style=\"background: #fff !important;\" aria-expanded=\"false\"><i class=\"fa fa-user-cog\" style=\"font-size: 14px;\"></i></button>");
            htmlBuilder.AppendLine("<ul class=\"dropdown-menu dropdown-menu-right\">");
            htmlBuilder.AppendLine($"<li><a href=\"#\" onclick=\"addPostToDiscuss('{model.POST_KEY}')\">Discuss</a></li>");
            htmlBuilder.AppendLine($"<li class=\"op-forward\"><a href=\"#\" onclick=\"showForwardPostModal('{model.POST_KEY}')\">Forward</a></li>");
            if (model.IS_CREATEBY)
            {
                htmlBuilder.AppendLine($"<li><a href=\"#\" onclick=\"loadEditPostModal('post-{model.POST_ID}','{model.POST_KEY}')\">Edit</a></li>");
                htmlBuilder.AppendLine($"<li><a href=\"#\" onclick=\"deletePost('post-{model.POST_ID}','{model.POST_KEY}')\">Delete</a></li>");
            }
            htmlBuilder.AppendLine("</ul></div></div>");
            htmlBuilder.AppendLine($"<div class=\"activity-overview\"><p>{model.MESSAGE}</p></div></div><div class=\"clearfix\"></div>");
            htmlBuilder.AppendLine("</article><div class=\"clearfix\"></div>");
            return htmlBuilder.ToString();
        }
        private static string AlertHtmlTemplate(AlertTemplateModel model)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine($"<article id=\"activity-{model.ALERT_ID}\" class=\"activity alert_snippet {model.CSS_MYPOST_REPLY}\">");
            htmlBuilder.AppendLine($"<div class=\"activity-avatar\" style=\"background-image: url('{model.AVATAR}')\"></div>");
            htmlBuilder.AppendLine($"<div class=\"activity-detail\">");
            htmlBuilder.AppendLine($"<div class=\"activity-meta\">");
            htmlBuilder.AppendLine($"<h4>{model.CREATE_BY_NAME}</h4><small>{model.TIMELINE_DATE}</small><br class=\"visible-xs\">");
            if (model.TOPIC_ID > 0 && !string.IsNullOrEmpty(model.TOPIC_NAME))
                htmlBuilder.AppendLine($"<a href='javascript:void(0);' onclick='ShowTopic(event, {model.TOPIC_ID}, this);' class=\"topic-label\"><span class=\"label label-info\">{model.TOPIC_NAME}</span></a>");
            if (model.REASON != ActivityUpdateReasonEnum.NoUpdates)
                htmlBuilder.AppendLine($"<span class=\"label label-info\">{model.REASON.GetDescription()}</span>");
            htmlBuilder.AppendLine($"<span class=\"label label-default pin-this {(model.IS_PINNED ? "pinned" : "")}\" id=\"pinIcon-{model.ALERT_ID}\" onclick=\"PinnedActivity('{model.ALERT_ID}', false, event)\"><i class=\"fa fa-thumb-tack\"></i></span>");
            htmlBuilder.AppendLine($"</div>");//END DIV "activity-meta"
            htmlBuilder.AppendLine($"<a href=\"javascript:void(0);\" onclick=\"ShowAlertPage('{model.ALERT_KEY}', false);\">");
            htmlBuilder.AppendLine($"<div class=\"activity-overview alert-detail\"><h5><span>Alert /</span>{model.ALERT_NAME}</h5><p>{model.DESCRIPTION.Replace(Environment.NewLine, "<br />")}</p></div>");
            htmlBuilder.AppendLine($"</a>");
            htmlBuilder.AppendLine($"</div>");//END DIV "activity-detail"
            htmlBuilder.AppendLine($"<div class=\"clearfix\"></div></article><div class=\"clearfix\"></div>");
            return htmlBuilder.ToString();
        }
        private static string QbicleTaskHtmlTemplate(TaskTemplateModel model)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine($"<article id=\"activity-{model.TASK_ID}\" class=\"activity task_snippet {model.CSS_MYPOST_REPLY}\">");
            htmlBuilder.AppendLine($"<div class=\"activity-avatar\" style=\"background-image: url('{model.AVATAR}');\"></div>");
            htmlBuilder.AppendLine("<div class=\"activity-detail\">");
            htmlBuilder.AppendLine($"<div class=\"activity-meta\"><h4>{model.CREATE_BY_NAME}</h4><small>{model.TIMELINE_DATE}</small><br class=\"visible-xs\">");
            if (model.TOPIC_ID > 0 && !string.IsNullOrEmpty(model.TOPIC_NAME))
                htmlBuilder.AppendLine($"<a href='javascript:void(0);' onclick='ShowTopic(event, {model.TOPIC_ID}, this);' class=\"topic-label\"><span class=\"label label-info\">{model.TOPIC_NAME}</span></a>");
            if (model.REASON != ActivityUpdateReasonEnum.NoUpdates)
                htmlBuilder.AppendLine($"<span class=\"label label-info\">{model.REASON.GetDescription()}</span>");
            htmlBuilder.AppendLine($"<span class=\"label label-default pin-this {(model.IS_PINNED ? "pinned" : "")}\" id=\"pinIcon-{model.TASK_ID}\" onclick=\"PinnedActivity('{model.TASK_ID}', false, event)\"><i class=\"fa fa-thumb-tack\"></i></span></div>");
            htmlBuilder.AppendLine($"<a href=\"javascript: void(0);\" title=\"Click here go to Task detail\" onclick=\"ShowTaskPage('{model.TASK_KEY}', false);\">");
            htmlBuilder.AppendLine($"<div class=\"activity-overview task\"><h5><span>Task /</span> {model.TASK_NAME}</h5><p>{model.DESCRIPTION?.Replace(Environment.NewLine, "<br />")}</p></div></a>");
            htmlBuilder.AppendLine("<div class=\"activity-specifics\"><ul>");
            htmlBuilder.AppendLine($"<li><i class=\"fa fa-warning\"></i> {model.PRIORITY}</li>");
            if (model.PROGRAMMED_START.HasValue && model.PROGRAMMED_END.HasValue)
            {
                if (model.PROGRAMMED_START.Value.Date == model.PROGRAMMED_END.Value.Date)
                {
                    htmlBuilder.AppendLine($"<li><i class=\"fa fa-calendar\"></i> {model.PROGRAMMED_START.Value.ToString(model.DATE_FORMAT)}</li>");
                    htmlBuilder.AppendLine($"<li><i class=\"fa fa-clock-o\"></i> {model.PROGRAMMED_START.Value.ToString("hh:mmtt")} - {model.PROGRAMMED_END.Value.ToString("hh:mmtt")}</li>");
                }
                else
                {
                    htmlBuilder.AppendLine($"<li><i class=\"fa fa-calendar\"></i> {model.PROGRAMMED_START.Value.ToString(model.DATE_FORMAT + " hh:mmtt")} - {model.PROGRAMMED_END.Value.ToString(model.DATE_FORMAT + " hh:mmtt")}</li>");
                }
            }
            if (model.IS_RECURS)
                htmlBuilder.AppendLine("<li><i class=\"fa fa-recycle\"></i> Recurring</li>");
            htmlBuilder.AppendLine("</ul></div></div><div class=\"clearfix\"></div></article><div class=\"clearfix\"></div>");
            return htmlBuilder.ToString();
        }
        private static string CleanBookTaskHtmlTemplate(TaskTemplateModel model)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine($"<article id=\"activity-{model.TASK_ID}\" class=\"activity task_snippet {model.CSS_MYPOST_REPLY}\">");
            htmlBuilder.AppendLine($"<div class=\"activity-avatar\" style=\"background-image: url('{model.AVATAR}');\"></div>");
            htmlBuilder.AppendLine("<div class=\"activity-detail\">");
            htmlBuilder.AppendLine($"<div class=\"activity-meta\"><h4>{model.CREATE_BY_NAME}</h4><small>{model.TIMELINE_DATE}</small><br class=\"visible-xs\">");
            if (model.TOPIC_ID > 0 && !string.IsNullOrEmpty(model.TOPIC_NAME))
                htmlBuilder.AppendLine($"<a href='javascript:void(0);' onclick='ShowTopic(event, {model.TOPIC_ID}, this);' class=\"topic-label\"><span class=\"label label-info\">{model.TOPIC_NAME}</span></a>");
            if (model.REASON != ActivityUpdateReasonEnum.NoUpdates)
                htmlBuilder.AppendLine($"<span class=\"label label-info\">{model.REASON.GetDescription()}</span>");
            htmlBuilder.AppendLine($"<span class=\"label label-default pin-this {(model.IS_PINNED ? "pinned" : "")}\" id=\"pinIcon-{model.TASK_ID}\" onclick=\"PinnedActivity('{model.TASK_ID}', false, event)\"><i class=\"fa fa-thumb-tack\"></i></span></div>");
            htmlBuilder.AppendLine($"<a href=\"javascript: void(0);\" title=\"Click here go to Task detail\" onclick=\"ShowTaskPage('{model.TASK_KEY}', false);\">");
            htmlBuilder.AppendLine($"<div class=\"activity-overview generic\"><h5><span>Cleanbooks Task /</span> {model.TASK_NAME}</h5><p>{model.DESCRIPTION?.Replace(Environment.NewLine, "<br />")}</p></div></a>");
            htmlBuilder.AppendLine($"<a href=\"{model.TASK_URL}\" title=\"Click here go to {model.CB_TASK_TYPENAME}\"<div class=\"activity-specifics\"><ul>");
            htmlBuilder.AppendLine($"<li><i class=\"fa fa-briefcase\"></i> {model.CB_TASK_TYPENAME}</li>");
            htmlBuilder.AppendLine($"<li><i class=\"fa fa-warning\"></i> {model.PRIORITY}</li>");
            htmlBuilder.AppendLine("</ul></div></a></div><div class=\"clearfix\"></div></article><div class=\"clearfix\"></div>");
            return htmlBuilder.ToString();
        }
        private static string ComplianceTaskHtmlTemplate(TaskTemplateModel model)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine($"<article id=\"activity-{model.TASK_ID}\" class=\"activity task_snippet {model.CSS_MYPOST_REPLY}\">");
            htmlBuilder.AppendLine($"<div class=\"activity-avatar\" style=\"background-image: url('{model.AVATAR}');\"></div>");
            htmlBuilder.AppendLine("<div class=\"activity-detail\">");
            htmlBuilder.AppendLine($"<div class=\"activity-meta\"><h4>{model.TASK_NAME}</h4><small>{model.TIMELINE_DATE}</small><br class=\"visible-xs\">");
            if (model.TOPIC_ID > 0 && !string.IsNullOrEmpty(model.TOPIC_NAME))
                htmlBuilder.AppendLine($"<a href='javascript:void(0);' onclick='ShowTopic(event, {model.TOPIC_ID}, this);' class=\"topic-label\"><span class=\"label label-info\">{model.TOPIC_NAME}</span></a>");
            if (model.REASON != ActivityUpdateReasonEnum.NoUpdates)
                htmlBuilder.AppendLine($"<span class=\"label label-info\">{model.REASON.GetDescription()}</span>");
            htmlBuilder.AppendLine($"<span class=\"label label-default pin-this {(model.IS_PINNED ? "pinned" : "")}\" id=\"pinIcon-{model.TASK_ID}\" onclick=\"PinnedActivity('{model.TASK_ID}', false, event)\"><i class=\"fa fa-thumb-tack\"></i></span></div>");
            htmlBuilder.AppendLine($"<a href=\"{model.TASK_URL}\"><h5><span>{(model.CT_TASK_TYPE == TaskType.Repeatable ? "Repeating" : "")} Compliance Task / </span> Operator</h5><p>{model.WORKGROUP_NAME} Workgroup</p>");
            htmlBuilder.AppendLine($"<div class=\"activity-overview\"><ul>");
            htmlBuilder.AppendLine($"<li><i class=\"fa fa-wpforms\"></i> {model.CT_ORDEREDFORMS_COUNT} Forms</li>");
            htmlBuilder.AppendLine($"<li><i class=\"fa fa-clock-o\"></i> {(model.CT_ORDEREDFORMS_SUM_ESTIMATEDTIME > 60 ? $"{model.CT_ORDEREDFORMS_SUM_ESTIMATEDTIME / 60}h" : $"{model.CT_ORDEREDFORMS_SUM_ESTIMATEDTIME}m")} total</li>");
            htmlBuilder.AppendLine($"<li><i class=\"fa fa-user\"></i> {model.CREATE_BY_NAME}</li>");
            if (model.IS_RECURS)
                htmlBuilder.AppendLine($"<li><i class=\"fa fa-recycle\"></i> Recurring</li>");
            htmlBuilder.AppendLine("</ul></div></a></div><div class=\"clearfix\"></div></article><div class=\"clearfix\"></div>");
            return htmlBuilder.ToString();
        }
        private static string DiscussionOrderHtmlTemplate(DiscussionTemplateModel model)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine($"<article id=\"activity-{model.DISCUSSION_ID}\" class=\"stream-shout gift animated fadeIn\">");
            htmlBuilder.AppendLine($"<img src=\"{model.FILE_URI}\" class=\"wow\">");
            htmlBuilder.AppendLine("<div class=\"promoinfo\">");
            htmlBuilder.AppendLine($"<h5>{model.DISCUSSION_NAME}</h5>");
            htmlBuilder.AppendLine($"<p><em>{model.DESCRIPTION}</em></p><br>");
            if (model.DISCUSSION_TYPE == DiscussionTypeEnum.B2COrder)
                htmlBuilder.AppendLine($"<div style=\"margin-bottom: 30px;\"><a href=\"{model.DISCUSSION_URL}\" class=\"btn btn-info community-button w-auto\">View order</a></div>");
            else
                htmlBuilder.AppendLine($"<div style=\"margin-bottom: 30px;\"><a href=\"{model.DISCUSSION_URL}\" class=\"btn btn-info community-button w-auto\">View & manage</a></div>");
            htmlBuilder.AppendLine("</div>");
            htmlBuilder.AppendLine("</article>");
            return htmlBuilder.ToString();
        }
        private static string CatalogDiscussionHtmlTemplate(DiscussionTemplateModel model, Catalog catalog, int itemCount, string currentUserId, string discussionCreatorId, bool? isCreator = null)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            var creatorClass = currentUserId == discussionCreatorId ? "me" : "";
            if (isCreator != null && isCreator == true)
            {
                creatorClass = "me";
            }
            htmlBuilder.AppendLine($"<article class=\"activity event_snippet {creatorClass}\" style=\"margin-bottom: 25px;\">");
            htmlBuilder.AppendLine($"<div class=\"activity-avatar\" style=\"background-image: url('{model.AVATAR}');\"></div>");
            htmlBuilder.AppendLine($"<div class=\"activity-detail\">");
            htmlBuilder.AppendLine($"<article id=\"activity-{model.DISCUSSION_ID}\" class=\"promo-block col\" style=\"width: 500px; max-width: 100%;\">");
            htmlBuilder.AppendLine($"<div class=\"what\">");
            htmlBuilder.AppendLine($"<div class=\"user-options\" style=\"margin-left: 0;\">");
            htmlBuilder.AppendLine($"<div class=\"post-body\">");
            htmlBuilder.AppendLine($"<div class=\"labelling\"></div>");
            htmlBuilder.AppendLine($"<a href=\"{model.DISCUSSION_URL}\">");
            htmlBuilder.AppendLine($"<div class=\"highlight-img shorter\" style=\"background-image: url('{model.FILE_URI}&size=M');\">&nbsp;</div>");
            htmlBuilder.AppendLine($"</a>");
            htmlBuilder.AppendLine($"<div class=\"user-post\" style=\"padding-top: 40px;\">");
            htmlBuilder.AppendLine($"<a href=\"#\">");
            htmlBuilder.AppendLine($"<h1>{catalog.Name}</h1>");
            htmlBuilder.AppendLine($"<label class=\"label label-soft nohover label-lg\" style=\"font-size: 12px; top: 0;\">{itemCount} items</label>");
            htmlBuilder.AppendLine($"<br><br>");
            htmlBuilder.AppendLine($"<p>{model.DESCRIPTION}</p>");
            htmlBuilder.AppendLine($"</a>");
            htmlBuilder.AppendLine($"<br>");
            htmlBuilder.AppendLine($"<a href=\"{model.DISCUSSION_URL}\" class=\"btn btn-primary community-button w-auto\">Browse</a>");
            htmlBuilder.AppendLine($"</div>");
            htmlBuilder.AppendLine($"</div>");
            htmlBuilder.AppendLine($"</div>");
            htmlBuilder.AppendLine($"</div>");
            htmlBuilder.AppendLine($"</article>");
            htmlBuilder.AppendLine($"</div>");
            htmlBuilder.AppendLine($"<div class=\"clearfix\"></div>");
            htmlBuilder.AppendLine($"</article>");
            htmlBuilder.AppendLine($"<div class=\"clearfix\"></div>");
            return htmlBuilder.ToString();
        }
        private static string DiscussionQbicleHtmlTemplate(DiscussionTemplateModel model)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine($"<article id=\"activity-{model.DISCUSSION_ID}\" class=\"activity media {model.CSS_MYPOST_REPLY}\">");
            htmlBuilder.AppendLine($"<div class=\"activity-avatar\" style=\"background-image: url('{model.AVATAR}');\"></div>");
            htmlBuilder.AppendLine("<div class=\"activity-detail\">");
            #region DIV "activity-detail/activity-meta"
            htmlBuilder.AppendLine("<div class=\"activity-meta\">");
            htmlBuilder.AppendLine($"<h4>{model.DISCUSSION_TITLE}</h4>");
            htmlBuilder.AppendLine($"<small>{model.TIMELINE_DATE}</small>");
            htmlBuilder.AppendLine($"<br class=\"visible-xs\">");
            if (model.TOPIC_ID > 0 && !string.IsNullOrEmpty(model.TOPIC_NAME))
                htmlBuilder.AppendLine($"<a href='javascript:void(0);' onclick='ShowTopic(event, {model.TOPIC_ID}, this);' class=\"topic-label\"><span class=\"label label-info\">{model.TOPIC_NAME}</span></a>");
            htmlBuilder.AppendLine($"<span class=\"label label-default pin-this {(model.IS_PINNED ? "pinned" : "")}\" id=\"pinIcon-{model.DISCUSSION_ID}\" onclick=\"PinnedActivity('{model.DISCUSSION_ID}', false, event)\"><i class=\"fa fa-thumb-tack\"></i></span>");
            htmlBuilder.AppendLine("</div>");
            #endregion
            #region DIV "activity-detail/activity-overview"
            htmlBuilder.AppendLine($"<div class=\"activity-overview {(string.IsNullOrEmpty(model.FILE_URI) ? "task" : "media")}\">");
            #region DIV "activity-detail/activity-overview/row"
            htmlBuilder.AppendLine("<div class=\"row\">");
            if (!string.IsNullOrEmpty(model.FILE_URI))
                htmlBuilder.AppendLine($"<div class=\"col-xs-12\"><a href=\"{model.DISCUSSION_URL}\" class=\"stream-img-preview\" style=\"background-image: url('{model.FILE_URI}');\">&nbsp;</a></div>");
            #region DIV "activity-detail/activity-overview/row/col-xs-12"
            htmlBuilder.AppendLine("<div class=\"col-xs-12 description\">");
            htmlBuilder.AppendLine($"<h5><a style=\"color:#333\" href=\"{model.DISCUSSION_URL}\">{model.DISCUSSION_NAME}</a></h5>");
            htmlBuilder.AppendLine($"<a style=\"color:#333\" href=\"{model.DISCUSSION_URL}\"><p>{model.DESCRIPTION}</p>{(!string.IsNullOrEmpty(model.DISCUSSION_BREADCRUMB) ? $"<small>{model.DISCUSSION_BREADCRUMB}</small>" : "")}</a>");
            htmlBuilder.AppendLine("</div>");
            #endregion
            htmlBuilder.AppendLine("</div>");
            #endregion
            htmlBuilder.AppendLine("</div>");
            #endregion
            htmlBuilder.AppendLine("</div>");
            htmlBuilder.AppendLine("<div class=\"clearfix\"></div></article> <div class=\"clearfix\"></div>");
            return htmlBuilder.ToString();
        }
        private static string EventHtmlTemplate(EventTemplateModel model)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine($"<article id=\"activity-{model.EVENT_ID}\" class=\"activity event_snippet {model.CSS_MYPOST_REPLY}\">");
            htmlBuilder.AppendLine($"<div class=\"activity-avatar\" style=\"background-image: url('{model.AVATAR}');\"></div>");
            htmlBuilder.AppendLine("<div class=\"activity-detail\">");
            htmlBuilder.AppendLine($"<div class=\"activity-meta\"><h4>{model.CREATE_BY_NAME}</h4><small>{model.TIMELINE_DATE}</small><br class=\"visible-xs\">");
            if (model.TOPIC_ID > 0 && !string.IsNullOrEmpty(model.TOPIC_NAME))
                htmlBuilder.AppendLine($"<a href='javascript:void(0);' onclick='ShowTopic(event, {model.TOPIC_ID}, this);' class=\"topic-label\"><span class=\"label label-info\">{model.TOPIC_NAME}</span></a>");
            if (model.REASON != ActivityUpdateReasonEnum.NoUpdates)
                htmlBuilder.AppendLine($"<span class=\"label label-info\">{model.REASON.GetDescription()}</span>");
            htmlBuilder.AppendLine($"<span class=\"label label-default pin-this {(model.IS_PINNED ? "pinned" : "")}\" id=\"pinIcon-{model.EVENT_ID}\" onclick=\"PinnedActivity('{model.EVENT_ID}', false, event)\"><i class=\"fa fa-thumb-tack\"></i></span></div>");
            htmlBuilder.AppendLine($"<a href=\"javascript:void(0);\" onclick=\"ShowEventPage('{model.EVENT_KEY}', false);\"><div class=\"activity-overview event-detail\"><h5><span>Event /</span> {model.EVENT_NAME}</h5><p>{model.DESCRIPTION}</p></div>");
            htmlBuilder.AppendLine($"<div class=\"activity-specifics\"><ul>");
            if (model.START.HasValue && model.END.HasValue)
            {
                if (model.START.Value.Date == model.END.Value.Date)
                {
                    htmlBuilder.AppendLine($"<li><i class=\"fa fa-calendar\"></i> {model.START.Value.ToString(model.DATE_FORMAT)}</li>");
                    htmlBuilder.AppendLine($"<li><i class=\"fa fa-clock-o\"></i> {model.START.Value.ToString("hh:mmtt")} - {model.END.Value.ToString("hh:mmtt")}</li>");
                }
                else
                {
                    htmlBuilder.AppendLine($"<li><i class=\"fa fa-calendar\"></i> {model.START.Value.ToString(model.DATE_FORMAT + " hh:mmtt")} - {model.END.Value.ToString(model.DATE_FORMAT + " hh:mmtt")}</li>");
                }
            }
            if (!string.IsNullOrEmpty(model.LOCATION))
                htmlBuilder.AppendLine($"<li><i class=\"fa fa-map-marker\"></i> {model.LOCATION}</li>");

            htmlBuilder.AppendLine("</ul></div></a></div><div class=\"clearfix\"></div></article><div class=\"clearfix\"></div>");
            return htmlBuilder.ToString();
        }
        private static string MediaHtmlTemplate(MediaTemplateModel model)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            if (!model.IsMediaOntab)
            {
                htmlBuilder.AppendLine($"<article id=\"activity-{model.MEDIA_ID}\" class=\"activity media {model.CSS_MYPOST_REPLY}\">");
                htmlBuilder.AppendLine($"<div class=\"activity-avatar\" style=\"background-image: url('{model.AVATAR}');\"></div>");
                htmlBuilder.AppendLine("<div class=\"activity-detail\">");
                htmlBuilder.AppendLine($"<div class=\"activity-meta\"><h4>{model.CREATE_BY_NAME}</h4><small>{model.TIMELINE_DATE}</small><br class=\"visible-xs\">");
                if (model.TOPIC_ID > 0 && !string.IsNullOrEmpty(model.TOPIC_NAME))
                    htmlBuilder.AppendLine($"<a href='javascript:void(0);' onclick='ShowTopic(event, {model.TOPIC_ID}, this);' class=\"topic-label\"><span class=\"label label-info\">{model.TOPIC_NAME}</span></a>");
                if (model.REASON != ActivityUpdateReasonEnum.NoUpdates)
                    htmlBuilder.AppendLine($"<span class=\"label label-info\">{model.REASON.GetDescription()}</span>");
                htmlBuilder.AppendLine($"<span class=\"label label-default pin-this {(model.IS_PINNED ? "pinned" : "")}\" id=\"pinIcon-{model.MEDIA_ID}\" onclick=\"PinnedActivity('{model.MEDIA_ID}', false, event)\"><i class=\"fa fa-thumb-tack\"></i></span></div>");
                htmlBuilder.AppendLine($"<div class=\"activity-overview media\"><div class=\"row\">");
                htmlBuilder.AppendLine($"<div class=\"col-xs-12\">");


                if (model.IsCustomerView)
                {
                    if (model.MEDIA_TYPE.Equals("Image File", StringComparison.OrdinalIgnoreCase))
                        htmlBuilder.AppendLine($"<a href=\"{model.FILE_URI}\" class=\"stream-img-preview\" style=\"background-image: url('{model.FILE_URI}');\" target=\"_blank\";></a>");
                    else if (model.MEDIA_TYPE.Equals("Video File", StringComparison.OrdinalIgnoreCase))
                    {
                        htmlBuilder.AppendLine($"<a href=\"{model.FILE_URI_MP4}\" target=\"_blank\";>");
                        htmlBuilder.AppendLine($"<video width=\"640\" height=\"320\" controls=\"\" style=\"display:inline-block;\" class=\"fancybox-video\">");
                        htmlBuilder.AppendLine($"<source src=\"{model.FILE_URI_MP4}\" type=\"video/mp4\">");
                        htmlBuilder.AppendLine($"<source src=\"{model.FILE_URI_WEBM}\" type=\"video/webm\">");
                        htmlBuilder.AppendLine($"<source src=\"{model.FILE_URI_OGV}\" type=\"video/ogv\">");
                        htmlBuilder.AppendLine($"</video></a>");
                    }
                    else
                        htmlBuilder.AppendLine($"<a href=\"{model.FILE_URI}\" target=\"_blank\";\"><img src='{model.FILE_ICON}' /></a>");

                    htmlBuilder.AppendLine($"</div>");
                    htmlBuilder.AppendLine($"<div class=\"col-xs-12 description\">");
                    htmlBuilder.AppendLine($"<h5>{model.MEDIA_NAME}</h5><p>{model.DESCRIPTION?.Replace(Environment.NewLine, "<br />")}</p>");
                    htmlBuilder.AppendLine($"<small>{model.FILE_EXTENSION} | {model.FILE_UPLOADED_DATE}</small></div>");
                    htmlBuilder.AppendLine($"</div></div></div><div class=\"clearfix\"></div></article><div class=\"clearfix\"></div>");
                }
                else
                {
                    if (model.MEDIA_TYPE.Equals("Image File", StringComparison.OrdinalIgnoreCase))
                        htmlBuilder.AppendLine($"<a href=\"javascript:void(0);\" class=\"stream-img-preview\" style=\"background-image: url('{model.FILE_URI}');\" onclick=\"ShowMediaPage('{model.MEDIA_KEY}', false);\"></a>");
                    else if (model.MEDIA_TYPE.Equals("Video File", StringComparison.OrdinalIgnoreCase))
                    {
                        htmlBuilder.AppendLine($"<a href=\"javascript:void(0);\" onclick=\"ShowMediaPage('{model.MEDIA_KEY}', false);\">");
                        htmlBuilder.AppendLine($"<video width=\"640\" height=\"320\" controls=\"\" style=\"display:inline-block;\" class=\"fancybox-video\">");
                        htmlBuilder.AppendLine($"<source src=\"{model.FILE_URI_MP4}\" type=\"video/mp4\">");
                        htmlBuilder.AppendLine($"<source src=\"{model.FILE_URI_WEBM}\" type=\"video/webm\">");
                        htmlBuilder.AppendLine($"<source src=\"{model.FILE_URI_OGV}\" type=\"video/ogv\">");
                        htmlBuilder.AppendLine($"</video></a>");
                    }
                    else
                        htmlBuilder.AppendLine($"<a href=\"javascript:void(0);\" onclick=\"ShowMediaPage('{model.MEDIA_KEY}', false);\"><img src='{model.FILE_ICON}' /></a>");

                    htmlBuilder.AppendLine($"</div>");
                    htmlBuilder.AppendLine($"<a class=\"col-xs-12 description\" href=\"javascript:void(0);\" onclick=\"ShowMediaPage('{model.MEDIA_KEY}', false);\">");
                    htmlBuilder.AppendLine($"<h5>{model.MEDIA_NAME}</h5><p>{model.DESCRIPTION?.Replace(Environment.NewLine, "<br />")}</p>");
                    htmlBuilder.AppendLine($"<small>{model.FILE_EXTENSION} | {model.FILE_UPLOADED_DATE}</small></a>");
                    htmlBuilder.AppendLine($"</div></div></div><div class=\"clearfix\"></div></article><div class=\"clearfix\"></div>");
                }

                
            }
            else
            {
                htmlBuilder.AppendLine($"<div class='col'>");
                htmlBuilder.AppendLine($"<div class='media-folder-item activity-overview task'>");
                htmlBuilder.AppendLine($"<a href=\"javascript:void(0);\" onclick=\"ShowMediaPage('{model.MEDIA_KEY}', false);\">");
                htmlBuilder.AppendLine($"<div class='preview' ");
                htmlBuilder.AppendLine($"style=\"background-image: url('{model.FILE_URI}');\" onclick=\"ShowMediaPage('{model.MEDIA_KEY}', false);\"></div>");
                htmlBuilder.AppendLine($"</a>");
                htmlBuilder.AppendLine($"<div class='meta_desc'>");
                htmlBuilder.AppendLine($"<h5>{model.MEDIA_NAME}</h5>");
                htmlBuilder.AppendLine($"<small>{model.FILE_EXTENSION} | Updated {model.FILE_UPLOADED_DATE}</small>");
                htmlBuilder.AppendLine($"</div>");
                htmlBuilder.AppendLine($"<a href='#' data-toggle='modal' data-target='#move-media' " +
                    $"onclick=\"QbicleLoadMoveMediaFolders({model.FOLDER_ID}, '{model.MEDIA_KEY}');\" " +
                    $"class='btn btn-primary move'><i class='fa fa-exchange'></i> &nbsp; Move</a>");
                htmlBuilder.AppendLine($"</div>");
                htmlBuilder.AppendLine($"</div>");
            }
            return htmlBuilder.ToString();
        }
        private static string LinkHtmlTemplate(LinkTemplateModel model)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine($"<article id=\"activity-{model.LINK_ID}\" class=\"activity media {model.CSS_MYPOST_REPLY}\">");
            htmlBuilder.AppendLine($"<div class=\"activity-avatar\" style=\"background-image: url('{model.AVATAR}');\"></div>");
            htmlBuilder.AppendLine("<div class=\"activity-detail\">");
            htmlBuilder.AppendLine($"<div class=\"activity-meta\"><h4>{model.CREATE_BY_NAME}</h4><small>{model.TIMELINE_DATE}</small><br class=\"visible-xs\">");
            if (model.TOPIC_ID > 0 && !string.IsNullOrEmpty(model.TOPIC_NAME))
                htmlBuilder.AppendLine($"<a href='javascript:void(0);' onclick='ShowTopic(event, {model.TOPIC_ID}, this);' class=\"topic-label\"><span class=\"label label-info\">{model.TOPIC_NAME}</span></a>");
            if (model.REASON != ActivityUpdateReasonEnum.NoUpdates)
                htmlBuilder.AppendLine($"<span class=\"label label-info\">{model.REASON.GetDescription()}</span>");
            htmlBuilder.AppendLine($"<span class=\"label label-default pin-this {(model.IS_PINNED ? "pinned" : "")}\" id=\"pinIcon-{model.LINK_ID}\" onclick=\"PinnedActivity('{model.LINK_ID}', false, event)\"><i class=\"fa fa-thumb-tack\"></i></span>");
            htmlBuilder.AppendLine("</div>");//END DIV activity-meta
            htmlBuilder.AppendLine($"<div class=\"activity-overview media\"><div class=\"row\">");
            if (!string.IsNullOrEmpty(model.FILE_URI))
                htmlBuilder.AppendLine($"<div class=\"col-xs-12\"><a href=\"javascript:ShowLinkPage('{model.LINK_KEY}', false)\" class=\"stream-img-preview\" style=\"background-image: url('{model.FILE_URI}');\">&nbsp;</a></div>");
            htmlBuilder.AppendLine($"<div class=\"col-xs-12 description\">");
            htmlBuilder.AppendLine($"<h5 style=\"cursor:pointer\" onclick=\"javascript: ShowLinkPage('{model.LINK_KEY}', false)\">{model.LINK_NAME}</h5>");
            htmlBuilder.AppendLine($"<p>{model.DESCRIPTION}</p>");
            htmlBuilder.AppendLine($"<p><i class=\"fa fa-external-link-alt\" style=\"color: rgba(0, 0, 0, 0.2);\"></i>&nbsp;<a href=\"{model.LINK_URL}\" target=\"_blank\">{model.LINK_URL_HOST}</a></p>");
            htmlBuilder.AppendLine($"</div>");//END DIV col-xs-12 description
            htmlBuilder.AppendLine($"</div></div>");//END DIV activity-overview
            htmlBuilder.AppendLine($"</div>");//activity-detail
            htmlBuilder.AppendLine("<div class=\"clearfix\"></div></article><div class=\"clearfix\"></div>");
            return htmlBuilder.ToString();
        }
        private static string HighLigthPostHtmlTemplate(HLSharedPostTemplateModel model)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine($"<article id=\"activity-{model.HL_ID}\" class=\"activity event_snippet {model.CSS_MYPOST_REPLY}\" style=\"margin-bottom: 25px;\">");
            htmlBuilder.AppendLine($"<div class=\"activity-avatar\" style=\"background-image: url('{model.AVATAR}');\"></div>");
            htmlBuilder.AppendLine("<div class=\"activity-detail\">");
            htmlBuilder.AppendLine("<article class=\"promo-block\">");
            htmlBuilder.AppendLine("<div class=\"what\">");
            htmlBuilder.AppendLine("<div class=\"user-options\" style=\"margin-left: 0;\">");
            htmlBuilder.AppendLine("<div style=\"display: flex; flex-direction: row;\">");
            htmlBuilder.AppendLine("<div class=\"user-metainfo\">");
            if (model.IS_CREATEBY)
                htmlBuilder.AppendLine($"<h5 style=\"padding-bottom: 5px;\">You shared a Highlight with {model.SHARED_BY_NAME}</h5><br />");
            else
                htmlBuilder.AppendLine($"<h5 style=\"padding-bottom: 5px;\">{model.SHARED_BY_NAME} shared a Highlight with you</h5><br />");
            htmlBuilder.AppendLine($"<smaller>{model.TIMELINE_DATE}</smaller>");
            htmlBuilder.AppendLine("</div>");//END DIV "user-metainfo"
            htmlBuilder.AppendLine("</div>");//END DIV "display: flex; flex-direction: row;"
            htmlBuilder.AppendLine("<div class=\"post-body\">");
            htmlBuilder.AppendLine("<div class=\"labelling\">");
            htmlBuilder.AppendLine($"<label class=\"label label-lg label-primary\">{model.HLPOST_TYPE_DESCRIPTION}</label>");
            if (!string.IsNullOrEmpty(model.HLPOST_LOCATION))
                htmlBuilder.AppendLine($"<label class=\"label label-lg label-info\">{model.HLPOST_LOCATION}</label>");
            htmlBuilder.AppendLine($"</div>");
            htmlBuilder.AppendLine($"<a href=\"/HighlightPost/HighlightPostDetail?hlPostId={model.HLPOST_ID}\"><div class=\"highlight-img ful\" style=\"background-image: url('{model.POST_IMG_URI}');\">&nbsp;</div></a>");
            htmlBuilder.AppendLine($"<div class=\"post-options\">");
            htmlBuilder.AppendLine($"<div class=\"row\">");
            htmlBuilder.AppendLine($"<div class=\"col-xs-7\">");
            htmlBuilder.AppendLine($"<div style=\"display: flex; flex-direction: row; position: absolute; top: -20px; left: 0;\">");
            htmlBuilder.AppendLine($"<div class=\"who mb\"><div class=\"who-avatar mb\" style=\"border: 0; background-image: url('{model.DOMAIN_LOGO_URI}');\"></div></div>");
            htmlBuilder.AppendLine($"<div class=\"user-metainfo mb\" style=\"padding-left: 15px;\"><h5 style=\"font-size: 12px;\">{model.DOMAIN_NAME}</h5></div>");
            htmlBuilder.AppendLine($"</div>");
            htmlBuilder.AppendLine($"</div>");//END DIV "col-xs-7"
            htmlBuilder.AppendLine($"<div class=\"col-xs-5 text-right\"><a href=\"#lovedby\" data-toggle=\"modal\" style=\"font-weight: 500; position: relative;\">{model.HLPOST_LIKEBY_COUNT} people love this</a></div>");
            htmlBuilder.AppendLine($"</div>");//END DIV "row"
            htmlBuilder.AppendLine($"</div>");//END DIV "post-options"
            htmlBuilder.AppendLine($"<div class=\"user-post\" style=\"padding-top: 45px;\">");
            htmlBuilder.AppendLine($"<a href=\"/HighlightPost/HighlightPostDetail?hlPostId={model.HLPOST_ID}\"><h1>{model.HLPOST_NAME}</h1></a>");
            htmlBuilder.AppendLine($"<div class=\"tag-list\">");
            foreach (var tag in model.HLPOST_TAGS)
            {
                htmlBuilder.AppendLine($"<a href=\"#\" class=\"label label-soft\">{tag}</a>");
            }
            htmlBuilder.AppendLine($"</div><br /><br />");
            htmlBuilder.AppendLine($"<p>{model.HLPOST_DESCRIPTION}</p><br />");
            #region IS EVENT EventListingHighlight
            if (model.IS_EVENT_HL)
            {
                htmlBuilder.AppendLine($"<table class=\"table app_specific table-borderless highlinfo\">");
                htmlBuilder.AppendLine($"<tbody>");
                htmlBuilder.AppendLine($"<tr>");
                htmlBuilder.AppendLine($"<td><strong>When</strong></td>");
                htmlBuilder.AppendLine($"<td>{model.EVENT_HLPOST_STARTDATE} - {model.EVENT_HLPOST_ENDDATE}</td>");
                htmlBuilder.AppendLine($"</tr>");
                htmlBuilder.AppendLine($"<tr>");
                htmlBuilder.AppendLine($"<td><strong>Where</strong></td>");
                htmlBuilder.AppendLine($"<td><strong>{model.EVENT_HLPOST_LOCATION}</strong></td>");
                htmlBuilder.AppendLine($"</tr>");
                htmlBuilder.AppendLine($"</tbody>");
                htmlBuilder.AppendLine($"</table><br />");
            }
            #endregion
            htmlBuilder.AppendLine($"<a href=\"/HighlightPost/HighlightPostDetail?hlPostId={model.HLPOST_ID}\" class=\"btn btn-primary community-button w-auto\">View Highlight</a>");
            htmlBuilder.AppendLine($"</div>");
            htmlBuilder.AppendLine($"</div></div></article></div><div class=\"clearfix\"></div></article><div class=\"clearfix\"></div>");
            return htmlBuilder.ToString();
        }
        private static string SharedPromotionHtmlTemplate(LoyaltySharedPromotionTemplateModel model)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine($"<article id=\"activity-{model.PROMOTION_ID}\" class=\"activity event_snippet {model.CSS_MYPOST_REPLY}\" style=\"margin-bottom: 25px;\">");
            htmlBuilder.AppendLine($"<div class=\"activity-avatar\" style=\"background-image: url('{model.AVATAR}')\"></div>");
            htmlBuilder.AppendLine($"<div class=\"activity-detail\">");
            htmlBuilder.AppendLine($"<article class=\"promo-block col\">");
            htmlBuilder.AppendLine($"<div class=\"what\">");
            htmlBuilder.AppendLine($"<div class=\"user-options\" style=\"margin-left: 0;\">");
            htmlBuilder.AppendLine($"<div style=\"display: flex; flex-direction: row;\">");
            htmlBuilder.AppendLine($"<div class=\"user-metainfo\">");
            if (model.IS_CREATEBY)
                htmlBuilder.AppendLine($"<h5 style=\"padding-bottom: 5px;\">You shared a Promotion with {model.SHARED_BY_NAME}</h5><br>");
            else
                htmlBuilder.AppendLine($"<h5 style=\"padding-bottom: 5px;\">{model.SHARED_BY_NAME} shared a Promotion with you</h5><br>");
            htmlBuilder.AppendLine($"<smaller>{model.TIMELINE_DATE}</smaller>");
            htmlBuilder.AppendLine("</div>");//END DIV "user-metainfo"
            htmlBuilder.AppendLine("</div>");//END DIV "display: flex; flex-direction: row;"
            htmlBuilder.AppendLine($"<div class=\"post-body\">");
            htmlBuilder.AppendLine($"<div class=\"labelling\"></div>");
            htmlBuilder.AppendLine($"<a href=\"/Monibac/PromotionDetailView?promotionKey={model.PROMOTION_KEY}\"><div class=\"highlight-img\" style=\"background-image: url('{model.POST_IMG_URI}')\">&nbsp;</div></a>");
            htmlBuilder.AppendLine($"<div class=\"post-options\">");
            htmlBuilder.AppendLine($"<div class=\"row\">");
            htmlBuilder.AppendLine($"<div class=\"col-xs-7\">");
            htmlBuilder.AppendLine($"<div style=\"display: flex; flex-direction: row; position: absolute; top: -20px; left: 0;\">");
            htmlBuilder.AppendLine($"<div class=\"who mb\"><div class=\"who-avatar mb\" style=\"border: 0; background-image: url('{model.DOMAIN_LOGO_URI}');\"></div></div>");
            htmlBuilder.AppendLine($"<div class=\"user-metainfo mb\" style=\"padding-left: 15px;\"><h5 style=\"font-size: 12px;\">{model.DOMAIN_NAME}</h5></div>");
            htmlBuilder.AppendLine($"</div>");//END DIV "display: flex;"                                              
            htmlBuilder.AppendLine($"</div>");//END DIV "col-xs-7"                                              
            htmlBuilder.AppendLine($"<div class=\"col-xs-5 text-right\"><a href=\"#lovedby\" data-toggle=\"modal\" style=\"font-weight: 500; position: relative;\">{model.PROMOTION_LIKEBY_COUNT} people love this</a></div>");
            htmlBuilder.AppendLine($"</div>");//END DIV "row"     
            htmlBuilder.AppendLine($"</div>");//END DIV "post-options"     
            htmlBuilder.AppendLine($"<div class=\"user-post\">");
            htmlBuilder.AppendLine($"<div>");//START CONTENT
            htmlBuilder.AppendLine($"<h1>{model.PROMOTION_NAME}</h1>");
            if (model.IS_PROMO_OFFER_EXPIRED)
                htmlBuilder.AppendLine($"<span class=\"countdown2 label label-info label-lg\" style=\"font-size: 12px; top: 0;\">{model.PROMOTION_REMAIN}</span>");
            else
                htmlBuilder.AppendLine($"<span class=\"countdown2 label label-warning label-lg\" style=\"font-size: 12px; top: 0;\">{model.PROMOTION_REMAIN}</span>");
            htmlBuilder.AppendLine($"<br><br><br><p>{model.PROMOTION_DESCRIPTION}</p>");
            htmlBuilder.AppendLine($"</div><br><br>");//END CONTENT
            htmlBuilder.AppendLine($"<a href=\"/Monibac/PromotionDetailView?promotionKey={model.PROMOTION_KEY}\" class=\"btn btn-primary community-button w-auto\">View Promotion</a>");
            htmlBuilder.AppendLine($"</div>");//END DIV "user-post"
            htmlBuilder.AppendLine($"</div>");//END DIV "post-body"
            htmlBuilder.AppendLine($"</div>");//END DIV "user-options"
            htmlBuilder.AppendLine($"</div>");//END DIV "what"
            htmlBuilder.AppendLine($"</article>");//END DIV "promo-block"
            htmlBuilder.AppendLine($"</div><div class=\"clearfix\"></div>");//END DIV "activity-detail"
            htmlBuilder.AppendLine($"</article><div class=\"clearfix\"></div>");//END DIV "article"
            return htmlBuilder.ToString();
        }
        private static string JounralApprovalHtmlTemplate(JounralApprovalTemplateModel model)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine($"<article id=\"activity-{model.JOUNRAL_ID}\" class=\"activity alert_snippet {model.CSS_MYPOST_REPLY}\">");
            htmlBuilder.AppendLine($"<div class=\"activity-avatar\" style=\"background-image: url('{model.AVATAR}')\"></div>");
            htmlBuilder.AppendLine($"<div class=\"activity-detail\">");
            htmlBuilder.AppendLine($"<div class=\"activity-meta\">");
            htmlBuilder.AppendLine($"<h4>{model.CREATE_BY_NAME}</h4><small>{model.TIMELINE_DATE}</small><br class=\"visible-xs\">");
            if (model.TOPIC_ID > 0 && !string.IsNullOrEmpty(model.TOPIC_NAME))
                htmlBuilder.AppendLine($"<a href='javascript:void(0);' onclick='ShowTopic(event, {model.TOPIC_ID}, this);' class=\"topic-label\"><span class=\"label label-info\">{model.TOPIC_NAME}</span></a>");
            htmlBuilder.AppendLine($"<span class=\"label {model.JOUNRAL_STATUS}\">{model.JOUNRAL_STATUS}</span>");
            htmlBuilder.AppendLine($"<span class=\"label label-default pin-this {(model.IS_PINNED ? "pinned" : "")}\" id=\"pinIcon-{model.JOUNRAL_ID}\" onclick=\"PinnedActivity('{model.JOUNRAL_ID}', false, event)\"><i class=\"fa fa-thumb-tack\"></i></span>");
            htmlBuilder.AppendLine($"</div>");//END DIV "activity-meta"
            htmlBuilder.AppendLine($"<a href=\"javascript: void(0)\" onclick=\"ShowApprovalPage('{model.JOUNRAL_KEY}', false, 'journal');\">");
            htmlBuilder.AppendLine($"<div class=\"activity-overview alert-detail\"><h5><span>Approval Request /</span> Bookkeeping</h5><p>Journal Entry #{model.JOUNRAL_NUMBER}</p></div>");
            htmlBuilder.AppendLine($"<div class=\"activity-specifics\"><ul><li><i class=\"fa fa-user\"></i> {model.JOUNRAL_NAME}</li></ul></div>");
            htmlBuilder.AppendLine($"</a>");
            htmlBuilder.AppendLine($"</div>");//END DIV "activity-detail"
            htmlBuilder.AppendLine($"<div class=\"clearfix\"></div></article><div class=\"clearfix\"></div>");
            return htmlBuilder.ToString();
        }
        private static string CampaigPostApprovalHtmlTemplate(CampaigPostApprovalTemplateModel model)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine($"<article id=\"activity-{model.CAMPAIGNPOST_APP_ID}\" class=\"activity alert_snippet {model.CSS_MYPOST_REPLY}\">");
            if (model.IS_MANUAL_CAMPAIGN)
                htmlBuilder.AppendLine($"<div class=\"activity-avatar\" style=\"background-image: url('/Content/DesignStyle/img/icon_socialpost.png')\"></div>");
            else
                htmlBuilder.AppendLine($"<div class=\"activity-avatar\" style=\"background-image: url('/Content/DesignStyle/img/icon_socialpost_manual.png')\"></div>");
            htmlBuilder.AppendLine($"<div class=\"activity-detail\">");
            htmlBuilder.AppendLine($"<div class=\"activity-meta\">");
            htmlBuilder.AppendLine($"<h4>{model.APPROVAL_TITLE}</h4><small>{model.TIMELINE_DATE}</small><br class=\"visible-xs\">");
            if (model.TOPIC_ID > 0 && !string.IsNullOrEmpty(model.TOPIC_NAME))
                htmlBuilder.AppendLine($"<a href='javascript:void(0);' onclick='ShowTopic(event, {model.TOPIC_ID}, this);' class=\"topic-label\"><span class=\"label label-info\">{model.TOPIC_NAME}</span></a>");
            htmlBuilder.AppendLine($"<span class=\"label {model.REQUEST_CSS}\">{model.REQUEST_STATUS}</span>");
            htmlBuilder.AppendLine($"<span class=\"label label-default pin-this {(model.IS_PINNED ? "pinned" : "")}\" id=\"pinIcon-{model.CAMPAIGNPOST_APP_ID}\" onclick=\"PinnedActivity('{model.CAMPAIGNPOST_APP_ID}', false, event)\"><i class=\"fa fa-thumb-tack\"></i></span>");
            htmlBuilder.AppendLine($"</div>");//END DIV "activity-meta"
            if (model.IS_MEDIA)
            {
                htmlBuilder.AppendLine($"<div class=\"activity-overview media\">");
                htmlBuilder.AppendLine($"<div class=\"row\">");
                htmlBuilder.AppendLine($"<div class=\"col-xs-12\">");
                var link = model.IS_MEMBER ? $"/SalesMarketing/{(model.IS_MANUAL_CAMPAIGN ? "ManualSocialPostInApp" : "SocialPostInApp")}?id={model.CAMPAIGNPOST_APPROVAL_ID}" : "#";
                if (model.IS_VIDEO)
                {
                    htmlBuilder.AppendLine($"<a href=\"{link}\">");
                    htmlBuilder.AppendLine($"<video width=\"640\" height=\"320\" controls=\"\" style=\"display: inline-block;\" class=\"fancybox-video\">");
                    htmlBuilder.AppendLine($"<source src=\"{model.FILE_URI_MP4}\" type=\"video/mp4\">");
                    htmlBuilder.AppendLine($"<source src=\"{model.FILE_URI_WEBM}\" type=\"video/webm\">");
                    htmlBuilder.AppendLine($"<source src=\"{model.FILE_URI_OGV}\" type=\"video/ogv\">");
                    htmlBuilder.AppendLine($"</video>");
                    htmlBuilder.AppendLine($"</a>");
                }
                else
                {
                    htmlBuilder.AppendLine($"<a href=\"{link}\" class=\"stream-img-preview\" style=\"background-image: url('{model.MEDIA_URI}');\">&nbsp;</a>");
                }

                htmlBuilder.AppendLine($"<div class=\"col-xs-12 description\">");
                htmlBuilder.AppendLine($"<h5 style=\"cursor: pointer\" onclick=\"location.href = '/SalesMarketing/{(model.IS_MANUAL_CAMPAIGN ? "ManualSocialPostInApp" : "SocialPostInApp")}?id={model.CAMPAIGNPOST_APPROVAL_ID}'\">{model.CAMPAIGNPOST_ASSOCIATED_NAME} &nbsp; &gt; &nbsp; {model.CAMPAIGNPOST_TITLE}</h5><p>{model.CAMPAIGNPOST_DESCRIPTION}</P>");
                htmlBuilder.AppendLine($"</div>");//END DIV "col-xs-12 description"
                htmlBuilder.AppendLine($"</div>");//END DIV "col-xs-12"
                htmlBuilder.AppendLine($"</div>");//END DIV "row"
                htmlBuilder.AppendLine($"</div>");//END DIV "activity-overview media"
            }
            else
            {
                htmlBuilder.AppendLine($"<a href=\"/SalesMarketing/{(model.IS_MANUAL_CAMPAIGN ? "ManualSocialPostInApp" : "SocialPostInApp")}?id={model.CAMPAIGNPOST_APPROVAL_ID}\">");
                htmlBuilder.AppendLine($"<div class=\"activity-overview task\"><div class=\"row\">");
                htmlBuilder.AppendLine($"<h5 style=\"cursor: pointer\" onclick=\"location.href = '/SalesMarketing/{(model.IS_MANUAL_CAMPAIGN ? "ManualSocialPostInApp" : "SocialPostInApp")}?id={model.CAMPAIGNPOST_APPROVAL_ID}'\">{model.CAMPAIGNPOST_ASSOCIATED_NAME} &nbsp; &gt; &nbsp; {model.CAMPAIGNPOST_TITLE}</h5><p>{model.CAMPAIGNPOST_DESCRIPTION}</P>");
                htmlBuilder.AppendLine($"</div></div>");//END DIV "activity-overview task"
                htmlBuilder.AppendLine($"</a>");
            }
            htmlBuilder.AppendLine($"<div class=\"activity-specifics\"><ul><li><i class=\"fa fa-user\"></i> {model.CREATE_BY_NAME}</li></ul></div>");
            htmlBuilder.AppendLine($"</div>");//END DIV "activity-detail"
            htmlBuilder.AppendLine($"<div class=\"clearfix\"></div></article><div class=\"clearfix\"></div>");
            return htmlBuilder.ToString();
        }
        private static string EmailPostApprovalHtmlTemplate(EmailPostApprovalTemplateModel model)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine($"<article id=\"activity-{model.APP_ID}\" class=\"activity media {model.CSS_MYPOST_REPLY}\">");
            htmlBuilder.AppendLine($"<div class=\"activity-avatar\" style=\"background-image: url('/Content/DesignStyle/img/icon_email.png');\"></div>");
            htmlBuilder.AppendLine($"<div class=\"activity-detail\">");
            htmlBuilder.AppendLine($"<div class=\"activity-meta\">");
            htmlBuilder.AppendLine($"<h4>Sales & Marketing Email Approval</h4><small>{model.TIMELINE_DATE}</small><br class=\"visible-xs\">");
            if (model.TOPIC_ID > 0 && !string.IsNullOrEmpty(model.TOPIC_NAME))
                htmlBuilder.AppendLine($"<a href='javascript:void(0);' onclick='ShowTopic(event, {model.TOPIC_ID}, this);' class=\"topic-label\"><span class=\"label label-info\">{model.TOPIC_NAME}</span></a>");
            htmlBuilder.AppendLine($"<span class=\"label {model.REQUEST_CSS}\">{model.REQUEST_STATUS}</span>");
            htmlBuilder.AppendLine($"<span class=\"label label-default pin-this {(model.IS_PINNED ? "pinned" : "")}\" id=\"pinIcon-{model.APP_ID}\" onclick=\"PinnedActivity('{model.APP_ID}', false, event)\"><i class=\"fa fa-thumb-tack\"></i></span>");
            htmlBuilder.AppendLine($"</div>");//END DIV "activity-meta"
            htmlBuilder.AppendLine($"<div class=\"activity-overview media\"><div class=\"row\">");
            htmlBuilder.AppendLine($"<div class=\"col-xs-12\">");
            htmlBuilder.AppendLine($"<a href=\"/SalesMarketing/EmailPostInApp?id={model.POST_APPROVAL_ID}\" class=\"stream-img-preview\" style=\"background-image: url('{model.POST_IMG_URI}');\">&nbsp;</a>");
            htmlBuilder.AppendLine($"</div>");
            htmlBuilder.AppendLine($"<div class=\"col-xs-12 description\">");
            htmlBuilder.AppendLine($"<h5 style=\"cursor:pointer\" onclick=\"location.href='/SalesMarketing/EmailPostInApp?id={model.POST_ID}'\">{model.POST_NAME} &nbsp; &gt; &nbsp; {model.POST_TITLE}</h5>");//END DIV "col-xs-12 descriptiona"
            htmlBuilder.AppendLine($"</div>");//END DIV "col-xs-12 descriptiona"
            htmlBuilder.AppendLine($"</div></div>");//END DIV "activity-overview media"
            htmlBuilder.AppendLine($"<div class=\"activity-specifics\"><ul><li><i class=\"fa fa-user\"></i> {model.CREATE_BY_NAME}</li></ul></div>");
            htmlBuilder.AppendLine($"</div>");//END DIV "activity-detail"
            htmlBuilder.AppendLine($"<div class=\"clearfix\"></div></article><div class=\"clearfix\"></div>");
            return htmlBuilder.ToString();
        }
        private static string OperatorClockHtmlTemplate(OperatorClockTemplateModel model)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine($"<article id=\"activity-{model.APP_ID}\" class=\"activity event_snippet {model.CSS_MYPOST_REPLY}\">");
            htmlBuilder.AppendLine($"<div class=\"activity-avatar\" style=\"background-image: url('{model.AVATAR}');\"></div>");
            htmlBuilder.AppendLine($"<div class=\"activity-detail\">");
            htmlBuilder.AppendLine($"<div class=\"activity-meta\"><h4>{model.CLOCK_NAME}</h4><small>{model.TIMELINE_DATE}</small><br class=\"visible-xs\">");
            if (model.TOPIC_ID > 0 && !string.IsNullOrEmpty(model.TOPIC_NAME))
                htmlBuilder.AppendLine($"<a href='javascript:void(0);' onclick='ShowTopic(event, {model.TOPIC_ID}, this);' class=\"topic-label\"><span class=\"label label-info\">{model.TOPIC_NAME}</span></a>");
            htmlBuilder.AppendLine($"<span class=\"label label-default pin-this {(model.IS_PINNED ? "pinned" : "")}\" id=\"pinIcon-{model.APP_ID}\" onclick=\"PinnedActivity('{model.APP_ID}', false, event)\"><i class=\"fa fa-thumb-tack\"></i></span>");
            htmlBuilder.AppendLine($"</div>");//END DIV "activity-meta"
            if (model.IS_CLOCK_IN)
                htmlBuilder.AppendLine($"<a href=\"/Operator/Clocked?id={model.CLOCK_APPROVAL_ID}&type=clockin\">");
            else
                htmlBuilder.AppendLine($"<a href=\"/Operator/Clocked?id={model.CLOCK_APPROVAL_ID}&type=clockout\">");
            htmlBuilder.AppendLine($"<div class=\"activity-overview task\">");
            if (model.IS_CLOCK_IN)
                htmlBuilder.AppendLine($"<h5><span>Clock in /</span> Operator</h5>");
            else
                htmlBuilder.AppendLine($"<h5><span>Clock out /</span> Operator</h5>");
            htmlBuilder.AppendLine($"<p>{model.CLOCK_IN_NOTE}</p>");
            htmlBuilder.AppendLine($"</div>");//END DIV "activity-overview task"
            htmlBuilder.AppendLine($"<div class=\"activity-specifics\"><ul>");
            htmlBuilder.AppendLine($"<li><i class=\"fa fa-map-marker\"></i> {model.WORKGROUP_LOCATION}</li>");
            htmlBuilder.AppendLine($"<li><i class=\"fa fa-calendar\"></i> {model.CLOCK_DATE}</li>");
            htmlBuilder.AppendLine($"<li><i class=\"fa fa-clock\"></i> {model.CLOCK_TIME}</li>");
            htmlBuilder.AppendLine($"</ul></div>");//END DIV "activity-specifics"
            htmlBuilder.AppendLine($"</a>");
            htmlBuilder.AppendLine($"</div>");//END DIV "activity-detail"
            htmlBuilder.AppendLine($"<div class=\"clearfix\"></div></article><div class=\"clearfix\"></div>");
            return htmlBuilder.ToString();
        }
        private static string DefaultApprovalRequestAppHtmlTemplate(ApprovalRequestAppTemplateModel model)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine($"<article id=\"activity-{model.APP_ID}\" class=\"activity event_snippet {model.CSS_MYPOST_REPLY}\">");
            htmlBuilder.AppendLine($"<div class=\"activity-avatar\" style=\"background-image: url('{model.AVATAR}');\"></div>");
            htmlBuilder.AppendLine($"<div class=\"activity-detail\">");
            htmlBuilder.AppendLine($"<div class=\"activity-meta\"><h4>{model.CREATE_BY_NAME}</h4><small>{model.TIMELINE_DATE}</small><br class=\"visible-xs\">");
            if (model.TOPIC_ID > 0 && !string.IsNullOrEmpty(model.TOPIC_NAME))
                htmlBuilder.AppendLine($"<a href='javascript:void(0);' onclick='ShowTopic(event, {model.TOPIC_ID}, this);' class=\"topic-label\"><span class=\"label label-info\">{model.TOPIC_NAME}</span></a>");
            htmlBuilder.AppendLine($"<span class=\"label {model.APP_STATUS_CSS}\">{model.APP_STATUS}</span>");
            htmlBuilder.AppendLine($"<span class=\"label label-default pin-this {(model.IS_PINNED ? "pinned" : "")}\" id=\"pinIcon-{model.APP_ID}\" onclick=\"PinnedActivity('{model.APP_ID}', false, event)\"><i class=\"fa fa-thumb-tack\"></i></span>");
            htmlBuilder.AppendLine($"</div>");//END DIV "activity-meta"
            htmlBuilder.AppendLine($"<a href=\"javascript:void(0)\" onclick=\"ShowApprovalPage('@app.Key', false, 'approval');\">");
            htmlBuilder.AppendLine($"<div class=\"activity-overview approval-detail\">");
            htmlBuilder.AppendLine($"<h5><span>Approval Request /</span> {model.APP_NAME}</h5>");
            htmlBuilder.AppendLine($"<p>{(model.APP_NOTE?.Replace(Environment.NewLine, "<br />") ?? "")}</p>");
            htmlBuilder.AppendLine($"</div>");//END DIV "activity-overview approval-detail"
            htmlBuilder.AppendLine($"</a>");
            htmlBuilder.AppendLine($"</div>");//END DIV "activity-detail"
            htmlBuilder.AppendLine($"<div class=\"clearfix\"></div></article><div class=\"clearfix\"></div>");
            return htmlBuilder.ToString();
        }
        private static string ApprovalRequestHtmlTemplate(ApprovalRequestTemplateModel model)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine($"<article id=\"activity-{model.APP_ID}\" class=\"activity approval_snippet {model.CSS_MYPOST_REPLY}\">");
            htmlBuilder.AppendLine($"<div class=\"activity-avatar\" style=\"background-image: url('{model.APP_ICON}');\"></div>");
            htmlBuilder.AppendLine($"<div class=\"activity-detail\">");
            htmlBuilder.AppendLine($"<div class=\"activity-meta\">");
            htmlBuilder.AppendLine($"<h4>{model.APPROVAL_TITLE}</h4><small>{model.TIMELINE_DATE}</small><br class=\"visible-xs\">");
            if (model.TOPIC_ID > 0 && !string.IsNullOrEmpty(model.TOPIC_NAME))
                htmlBuilder.AppendLine($"<a href='javascript:void(0);' onclick='ShowTopic(event, {model.TOPIC_ID}, this);' class=\"topic-label\"><span class=\"label label-info\">{model.TOPIC_NAME}</span></a>");
            htmlBuilder.AppendLine($"<span class=\"label {model.REQUEST_CSS}\">{model.REQUEST_STATUS}</span>");
            htmlBuilder.AppendLine($"<span class=\"label label-default pin-this {(model.IS_PINNED ? "pinned" : "")}\" id=\"pinIcon-{model.APP_ID}\" onclick=\"PinnedActivity('{model.APP_ID}', false, event)\"><i class=\"fa fa-thumb-tack\"></i></span>");
            htmlBuilder.AppendLine($"</div>");//END DIV "activity-meta"
            htmlBuilder.AppendLine($"<a href=\"{model.APP_LINK}\">");
            htmlBuilder.AppendLine($"<div class=\"activity-overview {model.CSS_OVERVIEW}\">");
            htmlBuilder.AppendLine($"<h5><span>{model.APP_TITLE}</span> / {model.APP_NAME}</h5><p>{model.APP_MESSAGE}</p>");
            htmlBuilder.AppendLine($"</div>");//END DIV "activity-overview"
            if (!string.IsNullOrEmpty(model.STOCK_AUDIT_STARTED_DATE))
            {
                htmlBuilder.AppendLine($"<div class=\"activity-specifics\">");
                htmlBuilder.AppendLine($"<ul><li><i class=\"fa fa-calendar\"></i> {model.STOCK_AUDIT_STARTED_DATE}</li></ul>");
                htmlBuilder.AppendLine($"</div>");//END DIV "activity-specifics"
            }
            htmlBuilder.AppendLine($"<div class=\"activity-specifics\"><ul>");
            htmlBuilder.AppendLine($"<li><i class=\"fa fa-user\"></i>{model.CREATE_BY_NAME}</li>");
            if (model.IS_CONSUME_ASSOCICATE_TASK)
                htmlBuilder.AppendLine($"<li><i class=\"fa fa-link\"></i> 1 Asset Tasks</li>");
            htmlBuilder.AppendLine($"</ul></div>");//END DIV "activity-specifics"
            htmlBuilder.AppendLine($"</a>");
            htmlBuilder.AppendLine($"</div>");//END DIV "activity-detail"
            htmlBuilder.AppendLine($"<div class=\"clearfix\"></div></article><div class=\"clearfix\"></div>");
            return htmlBuilder.ToString();
        }
        #endregion
        #region Map to class model for Template and return HTML String
        private static string getPostHtml(QbiclePost post, string currentUserId, string timezone)
        {
            try
            {
                if (post == null)
                    return "";
                PostTemplateModel model = new PostTemplateModel();
                model.POST_ID = post.Id.ToString();
                model.POST_KEY = post.Key;
                model.AVATAR = $"{ConfigManager.ApiGetDocumentUri}{post.CreatedBy.ProfilePic}&size=T";
                model.CSS_MYPOST_REPLY = post.CreatedBy.Id == currentUserId ? "me" : "";
                model.CREATE_BY_NAME = model.CSS_MYPOST_REPLY == "me" ? "Me" : HelperClass.GetFullNameOfUser(post.CreatedBy);
                model.TIMELINE_DATE = (post.StartedDate.Date == DateTime.UtcNow.ConvertTimeFromUtc(timezone).Date ? "Today, " + post.StartedDate.ToString("hh:mmtt") : post.StartedDate.ToString("dd MMM yyyy, hh:mmtt"));
                model.TOPIC_ID = post.Topic?.Id ?? 0;
                model.TOPIC_NAME = post.Topic?.Name ?? "";
                model.IS_CREATEBY = post.CreatedBy.Id == currentUserId ? true : false;
                model.MESSAGE = post.Message;
                return PostHtmlTemplate(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, post, currentUserId, timezone);
                return "";
            }
        }
        private static string getAlertHtml(QbicleAlert alert, List<int> pinneds, string currentUserId, string timezone)
        {
            try
            {
                if (alert == null)
                    return "";
                AlertTemplateModel model = new AlertTemplateModel();
                model.ALERT_ID = alert.Id.ToString();
                model.ALERT_KEY = alert.Key;
                model.AVATAR = $"{ConfigManager.ApiGetDocumentUri}{alert.StartedBy.ProfilePic}&size=T";
                model.CSS_MYPOST_REPLY = alert.StartedBy.Id == currentUserId ? "me" : "";
                model.CREATE_BY_NAME = model.CSS_MYPOST_REPLY == "me" ? "Me" : HelperClass.GetFullNameOfUser(alert.StartedBy);
                model.TIMELINE_DATE = (alert.TimeLineDate.Date == DateTime.UtcNow.ConvertTimeFromUtc(timezone).Date ? "Today, " + alert.TimeLineDate.ToString("hh:mmtt") : alert.TimeLineDate.ToString("dd MMM yyyy, hh:mmtt"));
                model.TOPIC_ID = alert.Topic?.Id ?? 0;
                model.TOPIC_NAME = alert.Topic?.Name ?? "";
                model.REASON = alert.UpdateReason;
                model.IS_PINNED = pinneds != null && pinneds.Any(e => e == alert.Id) ? true : false;
                model.ALERT_NAME = alert.Name;
                model.DESCRIPTION = alert.Content;
                return AlertHtmlTemplate(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, alert, pinneds, currentUserId, timezone);
                return "";
            }
        }
        private static string getQbicleTaskHtml(QbicleTask task, List<int> pinneds, string currentUserId, string timezone, string dateFormat = "dd/MM/yyyy")
        {
            try
            {
                if (task == null)
                    return "";
                TaskTemplateModel model = new TaskTemplateModel();
                model.TASK_ID = task.Id.ToString();
                model.TASK_KEY = task.Key;
                model.AVATAR = $"{ConfigManager.ApiGetDocumentUri}{(task.Asset != null ? task.Asset.FeaturedImageUri : task.StartedBy.ProfilePic)}&size=T";
                model.CSS_MYPOST_REPLY = task.StartedBy.Id == currentUserId ? "me" : "";
                model.CREATE_BY_NAME = model.CSS_MYPOST_REPLY == "me" ? "Me" : HelperClass.GetFullNameOfUser(task.StartedBy);
                model.TIMELINE_DATE = (task.TimeLineDate.Date == DateTime.UtcNow.ConvertTimeFromUtc(timezone).Date ? "Today, " + task.TimeLineDate.ToString("hh:mmtt") : task.TimeLineDate.ToString("dd MMM yyyy, hh:mmtt"));
                model.TOPIC_ID = task.Topic?.Id ?? 0;
                model.TOPIC_NAME = task.Topic?.Name ?? "";
                model.REASON = task.UpdateReason;
                model.IS_PINNED = pinneds != null && pinneds.Any(e => e == task.Id) ? true : false;
                model.TASK_NAME = task.Name;
                model.DESCRIPTION = task.Description;
                model.PRIORITY = task.Priority;
                model.PROGRAMMED_START = task.ProgrammedStart;
                model.PROGRAMMED_END = task.ProgrammedEnd;
                model.IS_RECURS = task.isRecurs;
                model.DATE_FORMAT = dateFormat;
                return QbicleTaskHtmlTemplate(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, task, pinneds, currentUserId, timezone);
                return "";
            }
        }
        private static string getCleanBookTaskHtml(QbicleTask task, List<int> pinneds, string currentUserId, string timezone, string dateFormat = "dd/MM/yyyy")
        {
            try
            {
                if (task == null || task.task == null)
                    return "";
                TaskTemplateModel model = new TaskTemplateModel();
                model.TASK_ID = task.Id.ToString();
                model.TASK_KEY = task.Key;
                model.AVATAR = $"{ConfigManager.ApiGetDocumentUri}{(task.Asset != null ? task.Asset.FeaturedImageUri : task.StartedBy.ProfilePic)}&size=T";
                model.CSS_MYPOST_REPLY = task.StartedBy.Id == currentUserId ? "me" : "";
                model.CREATE_BY_NAME = model.CSS_MYPOST_REPLY == "me" ? "Me" : HelperClass.GetFullNameOfUser(task.StartedBy);
                model.TIMELINE_DATE = (task.TimeLineDate.Date == DateTime.UtcNow.ConvertTimeFromUtc(timezone).Date ? "Today, " + task.TimeLineDate.ToString("hh:mmtt") : task.TimeLineDate.ToString("dd MMM yyyy, hh:mmtt"));
                model.TOPIC_ID = task.Topic?.Id ?? 0;
                model.TOPIC_NAME = task.Topic?.Name ?? "";
                model.REASON = task.UpdateReason;
                model.IS_PINNED = pinneds != null && pinneds.Any(e => e == task.Id) ? true : false;
                model.TASK_NAME = task.Name;
                model.DESCRIPTION = task.Description;
                model.PRIORITY = task.Priority;
                model.IS_RECURS = task.isRecurs;
                model.CB_TASK_TYPENAME = task.task.tasktype.Name;
                model.TASK_URL = task.Qbicle.Members.Any(u => u.Id == currentUserId) ? "/Apps/Tasks" : "";
                model.DATE_FORMAT = dateFormat;
                return CleanBookTaskHtmlTemplate(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, task, pinneds, currentUserId, timezone);
                return "";
            }
        }
        private static string getComplianceTaskHtml(QbicleTask task, List<int> pinneds, string currentUserId, string timezone, string dateFormat = "dd/MM/yyyy")
        {
            try
            {
                if (task == null || task.ComplianceTask == null)
                    return "";
                TaskTemplateModel model = new TaskTemplateModel();
                model.TASK_ID = task.Id.ToString();
                model.TASK_KEY = task.Key;
                model.AVATAR = $"{ConfigManager.ApiGetDocumentUri}{task.StartedBy.ProfilePic}&size=T";
                model.CSS_MYPOST_REPLY = task.StartedBy.Id == currentUserId ? "me" : "";
                model.CREATE_BY_NAME = model.CSS_MYPOST_REPLY == "me" ? "Me" : HelperClass.GetFullNameOfUser(task.StartedBy);
                model.TIMELINE_DATE = (task.TimeLineDate.Date == DateTime.UtcNow.ConvertTimeFromUtc(timezone).Date ? "Today, " + task.TimeLineDate.ToString("hh:mmtt") : task.TimeLineDate.ToString("dd MMM yyyy, hh:mmtt"));
                model.TOPIC_ID = task.Topic?.Id ?? 0;
                model.TOPIC_NAME = task.Topic?.Name ?? "";
                model.TASK_URL = task.Qbicle.Members.Any(u => u.Id == currentUserId) ? $"/Operator/ComplianceTask?id={task.ComplianceTask.Id}&tid={task.Id}" : "";
                model.REASON = task.UpdateReason;
                model.IS_PINNED = pinneds != null && pinneds.Any(e => e == task.Id) ? true : false;
                model.TASK_NAME = task.Name;
                model.WORKGROUP_NAME = task.ComplianceTask.WorkGroup.Name;
                model.CT_TASK_TYPE = task.ComplianceTask.Type;
                model.CT_ORDEREDFORMS_COUNT = task.ComplianceTask.OrderedForms.Count();
                model.CT_ORDEREDFORMS_SUM_ESTIMATEDTIME = task.ComplianceTask.OrderedForms.Sum(s => s.FormDefinition.EstimatedTime);
                model.IS_RECURS = task.isRecurs;
                model.DATE_FORMAT = dateFormat;
                return ComplianceTaskHtmlTemplate(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, task, pinneds, currentUserId, timezone);
                return "";
            }
        }
        private static string getDiscussionOrderHtml(QbicleDiscussion discussion, string currentUserId)
        {
            try
            {
                if (discussion == null)
                    return "";

                List<ApplicationUser> users = new List<ApplicationUser>();

                if (discussion.Qbicle is B2CQbicle)
                    users = ((B2CQbicle)discussion.Qbicle).Business.Users;

                else if (discussion.Qbicle is B2BQbicle)
                    users = ((B2BQbicle)discussion.Qbicle).Members;

                var isMember = users.Any(u => u.Id == currentUserId) || discussion.Qbicle.Members.Any(u => u.Id == currentUserId);
                var model = new DiscussionTemplateModel
                {
                    DISCUSSION_ID = discussion.Id.ToString(),
                    FILE_URI = HelperClass.ToDocumentUri(ConfigManager.CommunityShop).ToString(),
                    DISCUSSION_TYPE = discussion.DiscussionType,
                    DESCRIPTION = discussion.Summary,
                    DISCUSSION_NAME = discussion.Name,
                    DISCUSSION_URL = "#"
                };
                if (isMember && discussion.DiscussionType == DiscussionTypeEnum.B2COrder)
                {
                    model.DISCUSSION_URL = $"/B2C/DiscussionOrder?disKey={discussion.Key}";
                }
                else if (isMember && discussion.DiscussionType == DiscussionTypeEnum.B2BOrder)
                {
                    model.DISCUSSION_URL = $"/Commerce/DiscussionOrder?disKey={discussion.Key}";
                    model.FILE_URI = HelperClass.ToDocumentUri(ConfigManager.B2BBuySell).ToString();
                }
                else if (isMember && discussion.DiscussionType == DiscussionTypeEnum.B2CProductMenu)
                {
                    model.DISCUSSION_URL = $"/B2C/DiscussionMenu?disKey={discussion.Key}";
                }
                return DiscussionOrderHtmlTemplate(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, discussion, currentUserId);
                return "";
            }
        }
        private static string getCatalogDiscussionHtml(B2CProductMenuDiscussion discussion, string currentUserId)
        {
            try
            {
                var catalog = discussion.ProductMenu;
                if (discussion == null || catalog == null)
                    return "";
                var isMember = discussion.Qbicle.Members.Any(u => u.Id == currentUserId);
                var model = new DiscussionTemplateModel
                {
                    DISCUSSION_ID = discussion.Id.ToString(),
                    AVATAR = $"{ConfigManager.ApiGetDocumentUri}{discussion.StartedBy.ProfilePic}&size=T",
                    DISCUSSION_TYPE = discussion.DiscussionType,
                    DESCRIPTION = discussion.Summary,
                    DISCUSSION_NAME = discussion.Name,
                    DISCUSSION_URL = "#"
                };
                var catalog_img = catalog.Image;
                model.FILE_URI = HelperClass.ToDocumentUri(string.IsNullOrEmpty(catalog_img) ?
                    ConfigManager.CatalogDefaultImage : catalog_img).ToString();
                if (isMember)
                {
                    model.DISCUSSION_URL = $"/B2C/DiscussionMenu?disKey={discussion.Key}";
                }
                //Counting catalog ItemCount



                var catalogItemCount = 0;
                using (var dbContext = new ApplicationDbContext())
                {
                    var lstIds = catalog.Categories.Select(x => x.Id).ToList();
                    //TODO: QBIC-3927 Customers now can see the non-inventory items (additional service items)          
                    var query = from citem in dbContext.PosCategoryItems
                                where lstIds.Contains(citem.Category.Id)
                                && citem.PosVariants.Any()
                                select citem;

                    catalogItemCount = query.Count();
                }
                return CatalogDiscussionHtmlTemplate(model, catalog, catalogItemCount, currentUserId, discussion.StartedBy.Id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, discussion, currentUserId);
                return "";
            }
        }
        private static string getB2BCatalogDiscussionHtml(B2BCatalogDiscussion discussion, string currentUserId, int currentDomainId)
        {
            try
            {
                var catalog = discussion.AssociatedCatalog;
                if (discussion == null || catalog == null)
                    return "";
                var isMember = (discussion.Qbicle as B2BQbicle).Domains.Any(u => u.Users.Any(p => p.Id == currentUserId));
                var model = new DiscussionTemplateModel
                {
                    DISCUSSION_ID = discussion.Id.ToString(),
                    AVATAR = catalog.Image,
                    DISCUSSION_TYPE = discussion.DiscussionType,
                    DESCRIPTION = discussion.Summary,
                    DISCUSSION_NAME = discussion.Name,
                    DISCUSSION_URL = "#"
                };
                var catalog_img = catalog.Image;
                model.FILE_URI = HelperClass.ToDocumentUri(string.IsNullOrEmpty(catalog_img) ?
                    ConfigManager.CatalogDefaultImage : catalog_img).ToString();
                if (isMember)
                {
                    model.DISCUSSION_URL = $"/Commerce/CatalogDiscussion?disKey={discussion.Key}";
                }
                //Counting catalog ItemCount
                var catalogItemCount = 0;
                using (var dbContext = new ApplicationDbContext())
                {
                    var lstIds = catalog.Categories.Select(x => x.Id).ToList();
                    //TODO: QBIC-3927 Customers now can see the non-inventory items (additional service items)     
                    var query = from citem in dbContext.PosCategoryItems
                                where lstIds.Contains(citem.Category.Id)
                                && citem.PosVariants.Any()
                                select citem;

                    catalogItemCount = query.Count();
                }
                var isCreator = discussion.SharedByDomain.Id == currentDomainId;
                return CatalogDiscussionHtmlTemplate(model, catalog, catalogItemCount, currentUserId, discussion.StartedBy.Id, isCreator);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, discussion, currentUserId);
                return "";
            }
        }
        private static string getDiscussionQbicleHtml(QbicleDiscussion discussion, List<int> pinneds, string currentUserId, string timezone)
        {
            try
            {
                if (discussion == null)
                    return "";
                var isMember = discussion.Qbicle.Members.Any(u => u.Id == currentUserId);
                var model = new DiscussionTemplateModel
                {
                    DISCUSSION_ID = discussion.Id.ToString(),
                    AVATAR = $"{ConfigManager.ApiGetDocumentUri}{(discussion.StartedBy.ProfilePic)}&size=T",
                    TIMELINE_DATE = (discussion.TimeLineDate.Date == DateTime.UtcNow.ConvertTimeFromUtc(timezone).Date ? "Today, " + discussion.TimeLineDate.ToString("hh:mmtt") : discussion.TimeLineDate.ToString("dd MMM yyyy, hh:mmtt")),
                    CSS_MYPOST_REPLY = discussion.StartedBy.Id == currentUserId ? "me" : "",
                    DISCUSSION_TYPE = discussion.DiscussionType,
                    DESCRIPTION = discussion.Summary,
                    DISCUSSION_NAME = discussion.Name,
                    IS_PINNED = pinneds != null && pinneds.Any(e => e == discussion.Id) ? true : false,
                    DISCUSSION_URL = "#"
                };
                if (discussion.DiscussionType == DiscussionTypeEnum.IdeaDiscussion)
                {
                    if (isMember)
                        model.DISCUSSION_URL = $"/SalesMarketingIdea/DiscussionIdea?disId={discussion.Id}";
                    model.DISCUSSION_BREADCRUMB = "via Sales &amp; Marketing &gt; Ideas/Themes";
                    model.DISCUSSION_TITLE = "Sales &amp; Marketing Theme Discussion";
                }
                else if (discussion.DiscussionType == DiscussionTypeEnum.GoalDiscussion)
                {
                    if (isMember)
                        model.DISCUSSION_URL = $"/Operator/DiscussionGoal?disId={discussion.Id}";
                    model.DISCUSSION_BREADCRUMB = "via Operator &gt; Goal";
                    model.DISCUSSION_TITLE = "Operator Goal Discussion";
                }
                else if (discussion.DiscussionType == DiscussionTypeEnum.ComplianceTaskDiscussion)
                {
                    if (isMember)
                        model.DISCUSSION_URL = $"/Operator/DiscussionComplianceTask?disId={discussion.Id}";
                    model.DISCUSSION_BREADCRUMB = "via Operator &gt; Compliance Task";
                    model.DISCUSSION_TITLE = "Operator Compliance Task Discussion";
                }
                else if (discussion.DiscussionType == DiscussionTypeEnum.CashManagement)
                {
                    if (isMember)
                        model.DISCUSSION_URL = $"/CashManagement/DiscussionCashManagementShow?disKey={discussion.Key}";
                    model.DISCUSSION_BREADCRUMB = "";
                    model.DISCUSSION_TITLE = "Cash Management Discussion";
                }
                else
                {
                    if (isMember)
                        model.DISCUSSION_URL = $"/Qbicles/DiscussionQbicle?disKey={discussion.Key}";
                    model.DISCUSSION_BREADCRUMB = "";
                    model.DISCUSSION_NAME = $"{discussion.ActivityMembers.Count} participants";
                    model.DISCUSSION_TITLE = discussion.Name;
                }
                model.FILE_URI = !string.IsNullOrEmpty(discussion.FeaturedImageUri) ? $"{ConfigManager.ApiGetDocumentUri}{(discussion.FeaturedImageUri)}&size=M" : "";
                return DiscussionQbicleHtmlTemplate(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, discussion, currentUserId);
                return "";
            }
        }
        private static string getEventHtml(QbicleEvent even, List<int> pinneds, string currentUserId, string timezone, string dateFormat = "dd/MM/yyyy")
        {
            try
            {
                if (even == null)
                    return "";
                var timelinedate = even.TimeLineDate;

                EventTemplateModel model = new EventTemplateModel();
                model.EVENT_ID = even.Id.ToString();
                model.EVENT_KEY = even.Key;
                model.AVATAR = $"{ConfigManager.ApiGetDocumentUri}{even.StartedBy.ProfilePic}&size=T";
                model.CSS_MYPOST_REPLY = even.StartedBy.Id == currentUserId ? "me" : "";
                model.CREATE_BY_NAME = model.CSS_MYPOST_REPLY == "me" ? "Me" : HelperClass.GetFullNameOfUser(even.StartedBy);
                model.TIMELINE_DATE = (even.TimeLineDate.Date == DateTime.UtcNow.ConvertTimeFromUtc(timezone).Date ?
                    "Today, " + timelinedate.ToString("hh:mmtt") : timelinedate.ToString("dd MMM yyyy, hh:mmtt"));
                model.TOPIC_ID = even.Topic?.Id ?? 0;
                model.TOPIC_NAME = even.Topic?.Name ?? "";
                model.REASON = even.UpdateReason;
                model.IS_PINNED = pinneds != null && pinneds.Any(e => e == even.Id) ? true : false;
                model.EVENT_NAME = even.Name;
                model.DESCRIPTION = even.Description;
                model.START = even.Start;
                model.END = even.End;
                model.LOCATION = even.Location;
                model.DATE_FORMAT = dateFormat;
                return EventHtmlTemplate(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, even, pinneds, currentUserId, timezone);
                return "";
            }
        }
        private static string getMediaHtml(QbicleMedia media, List<int> pinneds, string currentUserId, string timezone, bool isMediaOntab = false, bool isCustomerView = false)
        {
            try
            {
                if (media == null)
                    return "";
                var mediaLastupdate = media.VersionedFiles.Where(e => !e.IsDeleted).OrderByDescending(x => x.UploadedDate).FirstOrDefault();
                MediaTemplateModel model = new MediaTemplateModel();
                model.MEDIA_ID = media.Id.ToString();
                model.MEDIA_KEY = media.Key;
                model.AVATAR = $"{ConfigManager.ApiGetDocumentUri}{media.StartedBy.ProfilePic}&size=T";
                model.CSS_MYPOST_REPLY = media.StartedBy.Id == currentUserId ? "me" : "";
                model.CREATE_BY_NAME = model.CSS_MYPOST_REPLY == "me" ? "Me" : HelperClass.GetFullNameOfUser(media.StartedBy);
                model.TIMELINE_DATE = (media.TimeLineDate.Date == DateTime.UtcNow.ConvertTimeFromUtc(timezone).Date ? "Today, " + media.TimeLineDate.ToString("hh:mmtt") : media.TimeLineDate.ToString("dd MMM yyyy, hh:mmtt"));
                model.TOPIC_ID = media.Topic?.Id ?? 0;
                model.TOPIC_NAME = media.Topic?.Name ?? "";
                model.REASON = media.UpdateReason;
                model.IS_PINNED = pinneds != null && pinneds.Any(e => e == media.Id) ? true : false;
                model.MEDIA_NAME = media.Name;
                model.DESCRIPTION = media.Description;
                model.MEDIA_TYPE = media.FileType?.Type ?? "";
                model.IsCustomerView = isCustomerView;
                if (mediaLastupdate != null && model.MEDIA_TYPE.Equals("Image File", StringComparison.OrdinalIgnoreCase))
                {
                    model.FILE_URI = $"{ConfigManager.ApiGetDocumentUri}{mediaLastupdate.Uri}&size=M";
                    if(isCustomerView) model.FILE_URI = $"{ConfigManager.ApiGetDocumentUri}{mediaLastupdate.Uri}";

                }
                else if (model.MEDIA_TYPE.Equals("Video File", StringComparison.OrdinalIgnoreCase))
                {
                    model.FILE_URI_MP4 = string.Format(ConfigManager.ApiGetVideoUri, mediaLastupdate.Uri, "mp4");
                    model.FILE_URI_WEBM = string.Format(ConfigManager.ApiGetVideoUri, mediaLastupdate.Uri, "webm");
                    model.FILE_URI_OGV = string.Format(ConfigManager.ApiGetVideoUri, mediaLastupdate.Uri, "ogv");
                }
                else
                {
                    model.FILE_ICON = media.FileType?.IconPath ?? "";
                    model.FILE_URI = $"{ConfigManager.ApiGetDocumentUri}{mediaLastupdate.Uri}";
                }
                model.FILE_EXTENSION = Utility.GetFileTypeDescription(media.FileType?.Extension);
                model.FILE_UPLOADED_DATE = mediaLastupdate == null ? "" : mediaLastupdate.UploadedDate.ConvertTimeFromUtc(timezone).ToString("d MMM yyyy, hh:mmtt");

                if (!media.IsVisibleInQbicleDashboard)
                    model.CSS_MYPOST_REPLY = "";
                model.FOLDER_ID = media.MediaFolder?.Id ?? 0;
                model.IsMediaOntab = isMediaOntab;
                return MediaHtmlTemplate(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, media, pinneds, currentUserId, timezone);
                return "";
            }
        }
        private static string getLinkHtml(QbicleLink link, List<int> pinneds, string currentUserId, string timezone)
        {
            try
            {
                if (link == null)
                    return "";
                var mediaLastupdate = link.FeaturedImage?.VersionedFiles.Where(e => !e.IsDeleted).OrderByDescending(x => x.UploadedDate).FirstOrDefault() ?? null;
                LinkTemplateModel model = new LinkTemplateModel();
                model.LINK_ID = link.Id.ToString();
                model.LINK_KEY = link.Key;
                model.AVATAR = $"{ConfigManager.ApiGetDocumentUri}{link.StartedBy.ProfilePic}&size=T";
                model.CSS_MYPOST_REPLY = link.StartedBy.Id == currentUserId ? "me" : "";
                model.CREATE_BY_NAME = model.CSS_MYPOST_REPLY == "me" ? "Me" : HelperClass.GetFullNameOfUser(link.StartedBy);
                model.TIMELINE_DATE = (link.TimeLineDate.Date == DateTime.UtcNow.ConvertTimeFromUtc(timezone).Date ? "Today, " + link.TimeLineDate.ToString("hh:mmtt") : link.TimeLineDate.ToString("dd MMM yyyy, hh:mmtt"));
                model.TOPIC_ID = link.Topic?.Id ?? 0;
                model.TOPIC_NAME = link.Topic?.Name ?? "";
                model.REASON = link.UpdateReason;
                model.IS_PINNED = pinneds != null && pinneds.Any(e => e == link.Id) ? true : false;
                model.FILE_URI = mediaLastupdate != null ? $"{ConfigManager.ApiGetDocumentUri + mediaLastupdate.Uri}&size=M" : "";
                model.DESCRIPTION = link.Description;
                model.LINK_URL = link.URL;
                Uri linkUri = new Uri(link.URL);
                model.LINK_URL_HOST = !string.IsNullOrEmpty(linkUri.Host) ? linkUri.Host : link.URL;
                return LinkHtmlTemplate(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, link, pinneds, currentUserId, timezone);
                return "";
            }
        }
        public static object getHighLigthPostHtml(HLSharedPost post, string currentUserId, string timezone, string dateFormat = "dd/MM/yyyy", bool getString = true)
        {
            try
            {
                var shareHighlight = post.SharedPost;
                var model = new HLSharedPostTemplateModel
                {
                    HL_ID = post.Id.ToString(),
                    HLPOST_ID = shareHighlight.Id.ToString(),
                    AVATAR = $"{ConfigManager.ApiGetDocumentUri}{post.SharedBy.ProfilePic}&size=T",
                    TIMELINE_DATE = post.ShareDate.ToString("dd MMM yyyy, hh:mmtt"),
                    POST_IMG_URI = shareHighlight.ImgUri.ToDocumentUri().ToString(),
                    DOMAIN_LOGO_URI = shareHighlight.Domain.LogoUri.ToDocumentUri().ToString(),
                    DOMAIN_NAME = shareHighlight.Domain.Name,
                    HLPOST_LIKEBY_COUNT = (shareHighlight.LikedBy?.Count ?? 0).ToString(),
                    HLPOST_NAME = shareHighlight.Title,
                    HLPOST_DESCRIPTION = shareHighlight.Content,
                    HLPOST_TAGS = shareHighlight.Tags.Select(s => s.Name).ToList(),
                    HLPOST_TYPE_DESCRIPTION = shareHighlight.Type.GetDescription()
                };

                if (string.IsNullOrEmpty(shareHighlight.ImgUri))
                    switch (shareHighlight.Type)
                    {
                        case HighlightPostType.Article:
                            model.POST_IMG_URI = ConfigManager.HighlightBannerEvent.ToUriString();
                            break;
                        case HighlightPostType.News:
                            model.POST_IMG_URI = ConfigManager.HighlightBannerNew.ToUriString();
                            break;
                        case HighlightPostType.Knowledge:
                            model.POST_IMG_URI = ConfigManager.HighlightBannerKnowledge.ToUriString();
                            break;
                        case HighlightPostType.Listings:
                            model.POST_IMG_URI = ConfigManager.HighlightBannerListing.ToUriString();
                            break;
                    }

                if (post.StartedBy.Id == currentUserId)
                {
                    model.CSS_MYPOST_REPLY = "me";
                    model.IS_CREATEBY = true;
                    model.SHARED_BY_NAME = post.SharedWith.GetFullName();
                }
                else
                {
                    model.IS_CREATEBY = false;
                    model.SHARED_BY_NAME = post.SharedBy.GetFullName();
                }
                model.CREATE_BY_NAME = model.CSS_MYPOST_REPLY == "me" ? "Me" : HelperClass.GetFullNameOfUser(post.StartedBy);


                if (shareHighlight.Type == HighlightPostType.Listings)
                {
                    var listingHighlight = shareHighlight as ListingHighlight;
                    model.HLPOST_TYPE_DESCRIPTION = listingHighlight.ListingHighlightType.GetDescription();
                    var eventCountry = "Available everywhere";
                    var eventArea = "";
                    var countryStr = listingHighlight.Country?.CommonName ?? "";
                    if (!string.IsNullOrEmpty(countryStr))
                    {
                        eventCountry = countryStr;
                        eventArea = listingHighlight.ListingLocation?.Name ?? "";
                        if (!string.IsNullOrEmpty(eventArea))
                        {
                            model.HLPOST_LOCATION = eventArea + ", " + eventCountry;
                        }
                    }

                }


                if (shareHighlight is EventListingHighlight)
                {
                    var eventHL = shareHighlight as EventListingHighlight;
                    model.IS_EVENT_HL = true;
                    model.EVENT_HLPOST_STARTDATE = eventHL.StartDate.ConvertTimeFromUtc(timezone).ToString(dateFormat);
                    model.EVENT_HLPOST_ENDDATE = eventHL.EndDate.ConvertTimeFromUtc(timezone).ToString(dateFormat);
                    model.EVENT_HLPOST_LOCATION = eventHL.EventLocation;
                }
                if (getString)
                    return HighLigthPostHtmlTemplate(model);
                else
                    return model;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, post, currentUserId, timezone, dateFormat);
                if (getString)
                    return "";
                else
                    return new HLSharedPostTemplateModel();
            }
        }
        public static object getSharedPromotionHtml(LoyaltySharedPromotion promotion, string currentUserId, string timezone, string dateFormat = "dd/MM/yyyy", bool getString = true)
        {
            try
            {
                var model = new LoyaltySharedPromotionTemplateModel
                {
                    PROMOTION_ID = promotion.Id.ToString(),
                    PROMOTION_KEY = promotion.SharedPromotion.Key,
                    AVATAR = $"{ConfigManager.ApiGetDocumentUri}{promotion.SharedBy.ProfilePic}&size=T",
                    CREATE_BY_NAME = promotion.StartedBy.GetFullName(),
                    TIMELINE_DATE = promotion.ShareDate.ToString("dd MMM yyyy, hh:mmtt"),
                    POST_IMG_URI = promotion.SharedPromotion.FeaturedImageUri.ToDocumentUri().ToString(),
                    DOMAIN_LOGO_URI = promotion.SharedPromotion.Domain.LogoUri.ToDocumentUri().ToString(),
                    DOMAIN_NAME = promotion.SharedPromotion.Domain.Name,
                    PROMOTION_LIKEBY_COUNT = (promotion.SharedPromotion.LikedBy?.Count ?? 0).ToString(),
                    PROMOTION_NAME = promotion.SharedPromotion.Name
                };

                if (promotion.StartedBy.Id == currentUserId)
                {
                    model.CSS_MYPOST_REPLY = "me";
                    model.IS_CREATEBY = true;
                    model.SHARED_BY_NAME = promotion.SharedWith.GetFullName();
                }
                else
                {
                    model.IS_CREATEBY = false;
                    model.SHARED_BY_NAME = promotion.SharedBy.GetFullName();
                }

                var remainHtmlStr = promotion.SharedPromotion.CalRemainPromotionInfo(timezone, dateFormat, DateTime.UtcNow);
                if (remainHtmlStr.Contains("Offer expired"))
                    model.IS_PROMO_OFFER_EXPIRED = true;
                else
                    model.IS_PROMO_OFFER_EXPIRED = false;
                if (getString)
                    return SharedPromotionHtmlTemplate(model);
                else
                    return model;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, promotion, currentUserId, timezone, dateFormat);
                if (getString)
                    return "";
                else
                    return new LoyaltySharedPromotionTemplateModel();
            }
        }
        private static string getJounralApprovalHtml(ApprovalReq approval, List<int> pinneds, string currentUserId, string timezone)
        {
            try
            {
                var journal = approval.JournalEntries.FirstOrDefault();
                if (approval == null || journal == null)
                    return "";
                JounralApprovalTemplateModel model = new JounralApprovalTemplateModel();
                model.JOUNRAL_ID = approval.Id.ToString();
                model.JOUNRAL_KEY = approval.Key;
                model.AVATAR = $"{ConfigManager.ApiGetDocumentUri}{approval.StartedBy.ProfilePic}&size=T";
                model.TIMELINE_DATE = (approval.TimeLineDate.Date == DateTime.UtcNow.ConvertTimeFromUtc(timezone).Date ? "Today, " + approval.TimeLineDate.ToString("hh:mmtt") : approval.TimeLineDate.ToString("dd MMM yyyy, hh:mmtt"));
                model.CSS_MYPOST_REPLY = approval.StartedBy.Id == currentUserId ? "me" : "";
                model.CREATE_BY_NAME = approval.StartedBy.GetFullName();
                model.TOPIC_ID = approval.Topic?.Id ?? 0;
                model.TOPIC_NAME = approval.Topic?.Name ?? "";
                model.IS_PINNED = pinneds != null && pinneds.Any(e => e == approval.Id) ? true : false;
                model.JOUNRAL_NUMBER = journal?.Number.ToString() ?? "0";
                model.JOUNRAL_NAME = journal?.CreatedBy.GetFullName() ?? "";
                model.JOUNRAL_STATUS = approval.RequestStatus.GetDescription();
                switch (approval.RequestStatus)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        if (approval.ReviewedBy.Count == 0)
                        {
                            model.JOUNRAL_STATUS_CSS = "label-warning";
                        }
                        else
                        {
                            model.JOUNRAL_STATUS = ApprovalReq.RequestStatusEnum.Reviewed.ToString();
                            model.JOUNRAL_STATUS_CSS = StatusLabelStyle.Reviewed;
                        }
                        break;
                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        model.JOUNRAL_STATUS_CSS = StatusLabelStyle.Reviewed;
                        break;
                    case ApprovalReq.RequestStatusEnum.Approved:
                        model.JOUNRAL_STATUS_CSS = "label-success";
                        break;
                    case ApprovalReq.RequestStatusEnum.Denied:
                        model.JOUNRAL_STATUS_CSS = "label-danger";
                        break;
                }
                return JounralApprovalHtmlTemplate(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, approval, currentUserId, timezone);
                return "";
            }

        }
        private static string getCampaigPostApprovalHtml(ApprovalReq approval, List<int> pinneds, string currentUserId, string timezone)
        {
            try
            {

                var campaignApproval = approval.CampaigPostApproval.FirstOrDefault();
                if (approval == null || campaignApproval == null)
                    return "";
                var campaignPost = campaignApproval.CampaignPost;
                var media = campaignPost.ImageOrVideo;
                CampaigPostApprovalTemplateModel model = new CampaigPostApprovalTemplateModel();
                model.CAMPAIGNPOST_APP_ID = approval.Id.ToString();
                model.AVATAR = $"{ConfigManager.ApiGetDocumentUri}{approval.StartedBy.ProfilePic}&size=T";
                model.TIMELINE_DATE = (approval.TimeLineDate.Date == DateTime.UtcNow.ConvertTimeFromUtc(timezone).Date ? "Today, " + approval.TimeLineDate.ToString("hh:mmtt") : approval.TimeLineDate.ToString("dd MMM yyyy, hh:mmtt"));
                model.CSS_MYPOST_REPLY = approval.StartedBy.Id == currentUserId ? "me" : "";
                model.CREATE_BY_NAME = campaignPost.CreatedBy.GetFullName();
                model.TOPIC_ID = approval.Topic?.Id ?? 0;
                model.TOPIC_NAME = approval.Topic?.Name ?? "";
                model.IS_PINNED = pinneds != null && pinneds.Any(e => e == approval.Id) ? true : false;
                model.REQUEST_STATUS = approval.RequestStatus.GetDescription();
                switch (approval.RequestStatus)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        if (approval.ReviewedBy.Count == 0)
                        {
                            model.REQUEST_CSS = "label-warning";
                        }
                        else
                        {
                            model.REQUEST_STATUS = ApprovalReq.RequestStatusEnum.Reviewed.ToString(); ;
                            model.REQUEST_CSS = StatusLabelStyle.Reviewed;
                        }
                        break;
                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        model.REQUEST_CSS = StatusLabelStyle.Reviewed;
                        break;
                    case ApprovalReq.RequestStatusEnum.Approved:
                        model.REQUEST_CSS = "label-success";
                        break;
                    case ApprovalReq.RequestStatusEnum.Denied:
                        model.REQUEST_CSS = "label-danger";
                        break;
                }
                model.IS_MANUAL_CAMPAIGN = approval.CampaigPostApproval.Any(c => c.CampaignPost.AssociatedCampaign.CampaignType == CampaignType.Manual);
                model.APPROVAL_TITLE = model.IS_MANUAL_CAMPAIGN ? "Manual Social Post Approval" : "Social Media Post Approval";
                model.IS_MEDIA = media != null ? true : false;
                model.IS_MEMBER = media?.Qbicle.Members.Any(u => u.Id == currentUserId) ?? false;
                model.CAMPAIGNPOST_APPROVAL_ID = campaignApproval?.Id.ToString() ?? "0";
                if (media != null && media.FileType.Type.Equals("Video File", StringComparison.OrdinalIgnoreCase))
                {
                    var mediaLastupdate = media.VersionedFiles.Where(e => !e.IsDeleted).OrderByDescending(x => x.UploadedDate).FirstOrDefault();
                    model.IS_VIDEO = true;
                    model.FILE_URI_MP4 = string.Format(ConfigManager.ApiGetVideoUri, mediaLastupdate.Uri, "mp4");
                    model.FILE_URI_WEBM = string.Format(ConfigManager.ApiGetVideoUri, mediaLastupdate.Uri, "webm");
                    model.FILE_URI_OGV = string.Format(ConfigManager.ApiGetVideoUri, mediaLastupdate.Uri, "ogv");
                }
                else if (media != null && media.FileType.Type.Equals("Image File", StringComparison.OrdinalIgnoreCase))
                {
                    var mediaLastupdate = media.VersionedFiles.Where(e => !e.IsDeleted).OrderByDescending(x => x.UploadedDate).FirstOrDefault();
                    model.IS_VIDEO = false;
                    model.MEDIA_URI = $"{ConfigManager.ApiGetDocumentUri}{mediaLastupdate.Uri}&size=M";
                }
                else
                {
                    model.IS_VIDEO = false;
                    model.MEDIA_URI = media != null ? $"{ConfigManager.ApiGetDocumentUri}{media.FileType.IconPath}" : "";
                }
                model.CAMPAIGNPOST_ASSOCIATED_NAME = campaignPost?.AssociatedCampaign.Name ?? "";
                model.CAMPAIGNPOST_TITLE = campaignPost?.Title ?? "";
                model.CAMPAIGNPOST_DESCRIPTION = campaignPost?.Content ?? "";
                return CampaigPostApprovalHtmlTemplate(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, approval, currentUserId, timezone);
                return "";
            }
        }
        private static string getEmailPostApprovalHtml(ApprovalReq approval, List<int> pinneds, string currentUserId, string timezone)
        {
            try
            {
                var emailPostApproval = approval.EmailPostApproval.FirstOrDefault();
                if (approval == null || emailPostApproval == null)
                    return "";
                var campaignEmail = emailPostApproval.CampaignEmail;
                EmailPostApprovalTemplateModel model = new EmailPostApprovalTemplateModel();
                model.APP_ID = approval.Id.ToString();
                model.POST_APPROVAL_ID = emailPostApproval.Id.ToString();
                model.AVATAR = $"{ConfigManager.ApiGetDocumentUri}{approval.StartedBy.ProfilePic}&size=T";
                model.TIMELINE_DATE = (approval.TimeLineDate.Date == DateTime.UtcNow.ConvertTimeFromUtc(timezone).Date ? "Today, " + approval.TimeLineDate.ToString("hh:mmtt") : approval.TimeLineDate.ToString("dd MMM yyyy, hh:mmtt"));
                model.CSS_MYPOST_REPLY = approval.StartedBy.Id == currentUserId ? "me" : "";
                model.CREATE_BY_NAME = campaignEmail.CreatedBy.GetFullName();
                model.TOPIC_ID = approval.Topic?.Id ?? 0;
                model.TOPIC_NAME = approval.Topic?.Name ?? "";
                model.IS_PINNED = pinneds != null && pinneds.Any(e => e == approval.Id) ? true : false;
                model.REQUEST_STATUS = emailPostApproval.ApprovalStatus.GetDescription();
                switch (emailPostApproval.ApprovalStatus)
                {
                    case SalesMktApprovalStatusEnum.InReview:
                        if (approval.ReviewedBy.Count == 0)
                        {
                            model.REQUEST_CSS = "label-warning";
                        }
                        else
                        {
                            model.REQUEST_STATUS = ApprovalReq.RequestStatusEnum.Reviewed.ToString();
                            model.REQUEST_CSS = StatusLabelStyle.Reviewed;
                        }
                        break;
                    case SalesMktApprovalStatusEnum.Approved:
                        model.REQUEST_CSS = "label-success";
                        break;
                    case SalesMktApprovalStatusEnum.Denied:
                        model.REQUEST_CSS = "label-danger";
                        break;
                    case SalesMktApprovalStatusEnum.Queued:
                        model.REQUEST_CSS = "label-success";
                        break;
                }
                model.POST_IMG_URI = $"{ConfigManager.ApiGetDocumentUri}{campaignEmail.FeaturedImageUri}&size=M";
                model.POST_NAME = campaignEmail.Campaign?.Name ?? "";
                model.POST_TITLE = campaignEmail.Title;
                return EmailPostApprovalHtmlTemplate(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, approval, currentUserId, timezone);
                return "";
            }
        }
        private static string getOperatorClockHtml(ApprovalReq approval, List<int> pinneds, string currentUserId, string timezone, bool isClockIn = true, string dateFormat = "dd/MM/yyyy")
        {
            try
            {
                var clockedInApproval = approval.OperatorClockIn.FirstOrDefault();
                if (approval == null || clockedInApproval == null)
                    return "";

                OperatorClockTemplateModel model = new OperatorClockTemplateModel();
                model.APP_ID = approval.Id.ToString();
                model.CLOCK_APPROVAL_ID = clockedInApproval.Id.ToString();
                model.AVATAR = $"{ConfigManager.ApiGetDocumentUri}{clockedInApproval.People.ProfilePic}&size=T";
                model.TIMELINE_DATE = (approval.TimeLineDate.Date == DateTime.UtcNow.ConvertTimeFromUtc(timezone).Date ? "Today, " + approval.TimeLineDate.ToString("hh:mmtt") : approval.TimeLineDate.ToString("dd MMM yyyy, hh:mmtt"));
                model.CSS_MYPOST_REPLY = approval.StartedBy.Id == currentUserId ? "me" : "";
                model.IS_PINNED = pinneds != null && pinneds.Any(e => e == approval.Id) ? true : false;
                model.CLOCK_IN_NOTE = clockedInApproval.Notes;
                model.WORKGROUP_LOCATION = clockedInApproval.WorkGroup.Location.Name;
                model.CLOCK_DATE = clockedInApproval.Date.ConvertTimeFromUtc(timezone).ToString(dateFormat);
                model.CLOCK_TIME = clockedInApproval.TimeIn.ConvertTimeFromUtc(timezone).ToString("hh:mmtt").ToLower();
                model.IS_CLOCK_IN = isClockIn;
                return OperatorClockHtmlTemplate(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, approval, currentUserId, timezone);
                return "";
            }
        }
        private static string getDefaultApprovalRequestAppHtml(ApprovalReq approval, List<int> pinneds, string currentUserId, string timezone)
        {
            try
            {
                if (approval == null)
                    return "";

                ApprovalRequestAppTemplateModel model = new ApprovalRequestAppTemplateModel();
                model.APP_ID = approval.Id.ToString();
                model.APP_KEY = approval.Key;
                model.AVATAR = $"{ConfigManager.ApiGetDocumentUri}{approval.StartedBy.ProfilePic}&size=T";
                model.TIMELINE_DATE = (approval.TimeLineDate.Date == DateTime.UtcNow.ConvertTimeFromUtc(timezone).Date ? "Today, " + approval.TimeLineDate.ToString("hh:mmtt") : approval.TimeLineDate.ToString("dd MMM yyyy, hh:mmtt"));
                model.CSS_MYPOST_REPLY = approval.StartedBy.Id == currentUserId ? "me" : "";
                model.IS_PINNED = pinneds != null && pinneds.Any(e => e == approval.Id) ? true : false;
                model.CREATE_BY_NAME = approval.StartedBy.GetFullName();
                model.TOPIC_ID = approval.Topic?.Id ?? 0;
                model.TOPIC_NAME = approval.Topic?.Name ?? "";
                model.APP_NAME = approval.Name;
                model.APP_NOTE = approval.Notes;
                model.APP_STATUS = approval.RequestStatus.GetDescription();
                switch (approval.RequestStatus)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        if (approval.ReviewedBy.Count == 0)
                        {
                            model.APP_STATUS_CSS = "label-warning";
                        }
                        else
                        {
                            model.APP_STATUS = ApprovalReq.RequestStatusEnum.Reviewed.GetDescription();
                            model.APP_STATUS_CSS = StatusLabelStyle.Reviewed;
                        }
                        break;
                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        model.APP_STATUS_CSS = StatusLabelStyle.Reviewed;
                        break;
                    case ApprovalReq.RequestStatusEnum.Approved:
                        model.APP_STATUS_CSS = "label-success";
                        break;
                    case ApprovalReq.RequestStatusEnum.Denied:
                        model.APP_STATUS_CSS = "label-danger";
                        break;
                }
                return DefaultApprovalRequestAppHtmlTemplate(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, approval, currentUserId, timezone);
                return "";
            }
        }
        private static string getApprovalRequestHtml(ApprovalReq approval, List<int> pinneds, string currentUserId, string timezone)
        {
            try
            {
                if (approval == null)
                    return "";

                var approvalTitle = "approval request";

                var model = new ApprovalRequestTemplateModel
                {
                    APP_ID = approval.Id.ToString(),
                    CREATE_BY_NAME = approval.StartedBy.GetFullName(),
                    TIMELINE_DATE = (approval.TimeLineDate.Date == DateTime.UtcNow.ConvertTimeFromUtc(timezone).Date ? "Today, " + approval.TimeLineDate.ToString("hh:mmtt") : approval.TimeLineDate.ToString("dd MMM yyyy, hh:mmtt")),
                    APP_KEY = approval.Key,
                    TOPIC_ID = approval.Topic?.Id ?? 0,
                    TOPIC_NAME = approval.Topic?.Name ?? "",
                    CSS_MYPOST_REPLY = approval.StartedBy.Id == currentUserId ? "me" : "",
                    IS_PINNED = pinneds != null && pinneds.Any(e => e == approval.Id) ? true : false,
                    REQUEST_STATUS = approval.RequestStatus.GetDescription(),
                    APP_MESSAGE = approval.Name,
                    APP_NAME = "Trader",
                    APP_TITLE = "approval request",
                    CSS_OVERVIEW = "approval-detail"
                };
                switch (approval.RequestStatus)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        if (approval.ReviewedBy.Count == 0)
                            model.REQUEST_CSS = "label-warning";
                        else
                        {
                            model.REQUEST_STATUS = ApprovalReq.RequestStatusEnum.Reviewed.GetDescription();
                            model.REQUEST_CSS = StatusLabelStyle.Reviewed;
                        }
                        if (approval.Transfer.Any())
                            model.REQUEST_STATUS = TransferStatus.PendingPickup.GetDescription();
                        break;
                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        model.REQUEST_CSS = StatusLabelStyle.Reviewed;
                        if (approval.Transfer.Any())
                            model.REQUEST_STATUS = TransferStatus.PickedUp.GetDescription();
                        break;
                    case ApprovalReq.RequestStatusEnum.Approved:
                        model.REQUEST_CSS = StatusLabelStyle.Approved;
                        if (approval.Transfer.Any())
                            model.REQUEST_STATUS = TransferStatus.Delivered.GetDescription();
                        break;
                    case ApprovalReq.RequestStatusEnum.Denied:
                        model.REQUEST_CSS = StatusLabelStyle.Denied;
                        break;
                    case ApprovalReq.RequestStatusEnum.Discarded:
                        model.REQUEST_CSS = StatusLabelStyle.Discarded;
                        break;
                }
                if (approval.Transfer != null && approval.Transfer.Any())
                {
                    var transfer = approval.Transfer.FirstOrDefault();
                    model.CREATE_BY_NAME = transfer.CreatedBy.GetFullName();
                    model.APP_ICON = "/Content/DesignStyle/img/icon_delivery.png";
                    model.APPROVAL_TITLE = $"Transfer {approvalTitle}";
                    model.APP_LINK = "/TraderTransfers/TransferReview?key=" + transfer.Key;
                    if (transfer.Sale == null && transfer.Purchase == null)
                    {
                        model.APP_TITLE = $"Transfer #{transfer.Reference?.FullRef}/ {transfer.OriginatingLocation?.Name} to {transfer.DestinationLocation?.Name}";
                    }
                    else if (transfer.Sale != null)
                    {
                        model.APP_TITLE = $"Transfer #{transfer.Reference?.FullRef}/ {transfer.OriginatingLocation?.Name} to {transfer.Sale.Purchaser.Name}";
                    }
                    else if (transfer.Purchase != null)
                    {
                        model.APP_TITLE = $"Transfer #{transfer.Reference?.FullRef}/ {transfer.Purchase.Vendor.Name} to {transfer.DestinationLocation?.Name}";
                    }
                }
                else if (approval.StockAudits != null && approval.StockAudits.Any())
                {
                    var stockAudit = approval.StockAudits.FirstOrDefault();
                    model.APP_ICON = "/Content/DesignStyle/img/icon_audit.png";
                    model.APPROVAL_TITLE = $"Shift Audit  {approvalTitle}";
                    model.APP_TITLE = "Approval Request";
                    model.APP_MESSAGE = stockAudit.Name;
                    model.APP_LINK = "/TraderStockAudits/ShiftAuditReview?id=" + stockAudit.Id;
                }
                else if (approval.Sale != null && approval.Sale.Any())
                {
                    var sale = approval.Sale.FirstOrDefault();
                    model.APP_ICON = "/Content/DesignStyle/img/icon_bookkeeping.png";
                    model.APPROVAL_TITLE = $"Sale {approvalTitle}";
                    model.APP_TITLE = $"Sale #{sale.Reference?.FullRef}";
                    model.CREATE_BY_NAME = sale.CreatedBy.GetFullName();
                    model.APP_LINK = "/TraderSales/SaleReview?key=" + sale.Key;
                }
                else if (approval.Purchase != null && approval.Purchase.Any())
                {
                    var purchase = approval.Purchase.FirstOrDefault();
                    model.APP_ICON = "/Content/DesignStyle/img/icon_bookkeeping.png";
                    model.APP_TITLE = $"Purchase #{purchase.Reference?.FullRef}";
                    model.APPROVAL_TITLE = $"Purchase {approvalTitle}";
                    model.CREATE_BY_NAME = purchase.CreatedBy.GetFullName();
                    model.APP_LINK = "/TraderPurchases/PurchaseReview?id=" + purchase.Id;
                }
                else if (approval.TraderContact != null && approval.TraderContact.Any())
                {
                    var contact = approval.TraderContact.FirstOrDefault();
                    model.APP_TITLE = contact.Name;
                    model.APP_MESSAGE = contact.ContactGroup.Name + " Group";
                    model.APP_ICON = "/Content/DesignStyle/img/icon_contact.png";
                    model.APPROVAL_TITLE = $"Contact {approvalTitle}";
                    model.APP_LINK = "/TraderContact/ContactReview?id=" + contact.Id;
                }
                else if (approval.Invoice != null && approval.Invoice.Any())
                {
                    var invoice = approval.Invoice.FirstOrDefault();
                    model.CREATE_BY_NAME = invoice.CreatedBy.GetFullName();
                    model.APP_ICON = "/Content/DesignStyle/img/icon_invoice.png";

                    if (invoice.Purchase != null)
                    {
                        model.APP_LINK = "/TraderBill/BillReview?id=" + invoice.Id;
                        model.APPROVAL_TITLE = $"Bill {approvalTitle}";
                        model.APP_TITLE = $"Bill #{invoice.Reference?.FullRef ?? invoice.Id.ToString()}";
                        model.APP_MESSAGE = $"For Purchase #{invoice.Purchase.Reference?.FullRef ?? ""}";
                    }

                    if (invoice.Sale != null)
                    {
                        model.APP_LINK = "/TraderInvoices/InvoiceReview?key=" + invoice.Key;
                        model.APPROVAL_TITLE = $"Invoice {approvalTitle}";
                        model.APP_TITLE = $"Invoice #{invoice.Reference?.FullRef ?? invoice.Id.ToString()}";
                        model.APP_MESSAGE = $"For Sale #{invoice.Sale.Reference?.FullRef ?? ""}";
                    }
                }
                else if (approval.Payments != null && approval.Payments.Any())
                {
                    var payment = approval.Payments.FirstOrDefault();
                    model.CREATE_BY_NAME = payment.CreatedBy.GetFullName();
                    model.APP_TITLE = "Payment #" + (payment?.Reference ?? "");
                    model.APP_ICON = "/Content/DesignStyle/img/icon_payments.png";
                    model.APPROVAL_TITLE = $"Payment {approvalTitle}";
                    model.APP_LINK = "/TraderPayments/PaymentReview?id=" + payment.Id;

                    if (payment?.AssociatedInvoice != null && payment?.AssociatedInvoice?.Id != null && payment?.AssociatedInvoice?.Id > 0)
                    {
                        model.APP_MESSAGE = $"For Invoice #{payment?.AssociatedInvoice?.Reference?.FullRef ?? ""}";
                    }
                    else
                    {
                        var fromStr = "";
                        var toStr = "";
                        if (payment?.OriginatingAccount?.Name != null)
                        {
                            fromStr = $"From: {payment.OriginatingAccount.Name}";
                        }
                        if (payment?.DestinationAccount?.Name != null)
                        {
                            toStr = $"To: {payment.DestinationAccount.Name}";
                        }

                        model.APP_MESSAGE = $"{fromStr} {toStr}";

                        if (fromStr == "" && toStr == "")
                        {
                            model.APP_MESSAGE = "No account details available";
                        }
                    }
                }
                else if (approval.SpotCounts != null && approval.SpotCounts.Any())
                {
                    var spotCount = approval.SpotCounts.FirstOrDefault();
                    model.APPROVAL_TITLE = $"Spot Count {approvalTitle}";
                    model.APP_TITLE = spotCount.Name;
                    model.APP_ICON = "/Content/DesignStyle/img/icon_spotcount.png";
                    model.APP_MESSAGE = model.APP_MESSAGE.Replace("Spot Count:", "");
                    model.APP_LINK = "/TraderSpotCount/SpotCountReview?id=" + spotCount.Id;
                    switch (spotCount.Status)
                    {
                        case SpotCountStatus.CountStarted:
                            model.REQUEST_STATUS = StatusLabelName.CountStarted;
                            model.REQUEST_CSS = StatusLabelStyle.Pending;
                            break;
                        case SpotCountStatus.CountCompleted:
                            model.REQUEST_STATUS = StatusLabelName.CountCompleted;
                            model.REQUEST_CSS = StatusLabelStyle.Reviewed;
                            break;
                        case SpotCountStatus.StockAdjusted:
                            model.REQUEST_STATUS = StatusLabelName.StockAdjusted;
                            model.REQUEST_CSS = StatusLabelStyle.Approved;
                            break;
                        case SpotCountStatus.Denied:
                            model.REQUEST_STATUS = StatusLabelName.Denied;
                            model.REQUEST_CSS = StatusLabelStyle.Denied;
                            break;
                        case SpotCountStatus.Discarded:
                            model.REQUEST_STATUS = StatusLabelName.Discarded;
                            model.REQUEST_CSS = StatusLabelStyle.Discarded;
                            break;
                    }
                }
                else if (approval.WasteReports != null && approval.WasteReports.Any())
                {
                    var wasteReport = approval.WasteReports.FirstOrDefault();
                    model.APPROVAL_TITLE = $"Waste {approvalTitle}";
                    model.APP_TITLE = approvalTitle;
                    model.APP_ICON = "/Content/DesignStyle/img/icon_waste.png";
                    model.APP_MESSAGE = wasteReport.Name;
                    model.APP_LINK = "/TraderWasteReport/WasteReportReview?id=" + wasteReport.Id;
                    switch (wasteReport.Status)
                    {
                        case WasteReportStatus.Started:
                            model.REQUEST_STATUS = StatusLabelName.Started;
                            model.REQUEST_CSS = StatusLabelStyle.Pending;
                            break;
                        case WasteReportStatus.Completed:
                            model.REQUEST_STATUS = StatusLabelName.Completed;
                            model.REQUEST_CSS = StatusLabelStyle.Reviewed;
                            break;
                        case WasteReportStatus.StockAdjusted:
                            model.REQUEST_STATUS = StatusLabelName.StockAdjusted;
                            model.REQUEST_CSS = StatusLabelStyle.Approved;
                            break;
                        case WasteReportStatus.Discarded:
                            model.REQUEST_STATUS = StatusLabelName.Discarded;
                            model.REQUEST_CSS = StatusLabelStyle.Discarded;
                            break;
                    }
                }
                else if (approval.Manufacturingjobs != null && approval.Manufacturingjobs.Any())
                {
                    var manufacturingJob = approval.Manufacturingjobs.FirstOrDefault();
                    model.APPROVAL_TITLE = $"Manufacturing {approvalTitle}";
                    model.CREATE_BY_NAME = manufacturingJob.CreatedBy.GetFullName();
                    model.APP_TITLE = "Compound Item Assembly";
                    model.APP_ICON = "/Content/DesignStyle/img/icon_manufacturing.png";
                    model.APP_MESSAGE = (manufacturingJob.Reference != null ? manufacturingJob.Reference.FullRef : "") + " " + (manufacturingJob.Product.Name); //$"Items {appTrader.Manufacturingjobs.FirstOrDefault().SelectedRecipe.Ingredients.Count} manufacturing";
                    model.APP_LINK = "/Manufacturing/ManuJobReview?id=" + manufacturingJob.Id;
                    switch (manufacturingJob.Status)
                    {
                        case ManuJobStatus.Pending:
                            model.REQUEST_STATUS = StatusLabelName.Pending;
                            model.REQUEST_CSS = StatusLabelStyle.Pending;
                            break;
                        case ManuJobStatus.Reviewed:
                            model.REQUEST_STATUS = StatusLabelName.Reviewed;
                            model.REQUEST_CSS = StatusLabelStyle.Reviewed;
                            break;
                        case ManuJobStatus.Approved:
                            model.REQUEST_STATUS = StatusLabelName.Approved;
                            model.REQUEST_CSS = StatusLabelStyle.Approved;
                            break;
                        case ManuJobStatus.Denied:
                            model.REQUEST_STATUS = StatusLabelName.Denied;
                            model.REQUEST_CSS = StatusLabelStyle.Denied;
                            break;
                        case ManuJobStatus.Discarded:
                            model.REQUEST_STATUS = StatusLabelName.Discarded;
                            model.REQUEST_CSS = StatusLabelStyle.Discarded;
                            break;
                    }
                }
                else if (approval.CreditNotes != null && approval.CreditNotes.Any())
                {
                    var creditNote = approval.CreditNotes.FirstOrDefault();

                    model.CREATE_BY_NAME = creditNote.CreatedBy.GetFullName();
                    model.APP_TITLE = $"{HelperClass.GetFullNameOfUser(creditNote.CreatedBy)}";
                    model.APP_ICON = "/Content/DesignStyle/img/icon_manufacturing.png";
                    if (creditNote.Reason == CreditNoteReason.DebitNote
                    || creditNote.Reason == CreditNoteReason.PriceIncrease)
                    {
                        model.APP_MESSAGE = $"Debit note #" + creditNote.Reference?.FullRef;
                        model.APPROVAL_TITLE = $"Debit note {approval.Name} {approvalTitle}";
                    }
                    else
                    {
                        model.APP_MESSAGE = $"Credit note #" + creditNote.Reference?.FullRef;
                        model.APPROVAL_TITLE = $"Credit note {approval.Name} {approvalTitle}";
                    }
                    model.APP_LINK = "/TraderContact/CreditNoteReview?id=" + creditNote.Id;

                    switch (creditNote.Status)
                    {
                        case CreditNoteStatus.PendingReview:
                            model.REQUEST_STATUS = StatusLabelName.Pending;
                            model.REQUEST_CSS = StatusLabelStyle.Pending;
                            break;
                        case CreditNoteStatus.Reviewed:
                            model.REQUEST_STATUS = StatusLabelName.Reviewed;
                            model.REQUEST_CSS = StatusLabelStyle.Reviewed;
                            break;
                        case CreditNoteStatus.Approved:
                            model.REQUEST_STATUS = StatusLabelName.Approved;
                            model.REQUEST_CSS = StatusLabelStyle.Approved;
                            break;
                        case CreditNoteStatus.Denied:
                            model.REQUEST_STATUS = StatusLabelName.Denied;
                            model.REQUEST_CSS = StatusLabelStyle.Denied;
                            break;
                        case CreditNoteStatus.Discarded:
                            model.REQUEST_STATUS = StatusLabelName.Discarded;
                            model.REQUEST_CSS = StatusLabelStyle.Discarded;
                            break;
                    }
                }
                else if (approval.BudgetScenarioItemGroups != null && approval.BudgetScenarioItemGroups.Any())
                {
                    var budgetScenarioItemGroups = approval.BudgetScenarioItemGroups.FirstOrDefault();
                    model.APPROVAL_TITLE = "Budget {approvalTitle}";
                    model.CREATE_BY_NAME = budgetScenarioItemGroups.CreatedBy.GetFullName();
                    model.APP_TITLE = $"{HelperClass.GetFullNameOfUser(budgetScenarioItemGroups.CreatedBy)}";
                    model.APP_ICON = "/Content/DesignStyle/img/icon_bookkeeping.png";
                    model.APP_MESSAGE = $"Budget Scenario Items Group: {budgetScenarioItemGroups.BudgetScenario.Title}";
                    model.APP_LINK = "/TraderBudget/ProcessApproval?id=" + budgetScenarioItemGroups.Id + "&oView=A";
                    switch (budgetScenarioItemGroups.Status)
                    {
                        case BudgetScenarioItemGroupStatus.Pending:
                            model.REQUEST_STATUS = StatusLabelName.Pending;
                            model.REQUEST_CSS = StatusLabelStyle.Pending;
                            break;
                        case BudgetScenarioItemGroupStatus.Reviewed:
                            model.REQUEST_STATUS = StatusLabelName.Reviewed;
                            model.REQUEST_CSS = StatusLabelStyle.Reviewed;
                            break;
                        case BudgetScenarioItemGroupStatus.Approved:
                            model.REQUEST_STATUS = StatusLabelName.Approved;
                            model.REQUEST_CSS = StatusLabelStyle.Approved;
                            break;
                        case BudgetScenarioItemGroupStatus.Draft:
                            model.REQUEST_STATUS = StatusLabelName.Draft;
                            model.REQUEST_CSS = StatusLabelStyle.Draft;
                            break;
                        case BudgetScenarioItemGroupStatus.Denied:
                            model.REQUEST_STATUS = StatusLabelName.Denied;
                            model.REQUEST_CSS = StatusLabelStyle.Denied;
                            break;
                        case BudgetScenarioItemGroupStatus.Discarded:
                            model.REQUEST_STATUS = StatusLabelName.Discarded;
                            model.REQUEST_CSS = StatusLabelStyle.Discarded;
                            break;
                    }
                }
                else if (approval.TraderReturns != null && approval.TraderReturns.Any())
                {
                    var traderReturn = approval.TraderReturns.FirstOrDefault();
                    model.APPROVAL_TITLE = $"Sales Return {approvalTitle}";
                    model.CREATE_BY_NAME = traderReturn.CreatedBy.GetFullName();
                    model.APP_TITLE = $"Sale return #{traderReturn.Reference?.FullRef}";
                    model.APP_ICON = "/Content/DesignStyle/img/icon_return.png";
                    model.APP_MESSAGE = $"Reference #{traderReturn.Reference?.FullRef}";
                    model.APP_LINK = "/TraderSalesReturn/SaleReturnReview?id=" + traderReturn.Id;
                    switch (traderReturn.Status)
                    {
                        case TraderReturnStatusEnum.PendingReview:
                            model.REQUEST_STATUS = StatusLabelName.Pending;
                            model.REQUEST_CSS = StatusLabelStyle.Pending;
                            break;
                        case TraderReturnStatusEnum.Reviewed:
                            model.REQUEST_STATUS = StatusLabelName.Reviewed;
                            model.REQUEST_CSS = StatusLabelStyle.Reviewed;
                            break;
                        case TraderReturnStatusEnum.Approved:
                            model.REQUEST_STATUS = StatusLabelName.Approved;
                            model.REQUEST_CSS = StatusLabelStyle.Approved;
                            break;
                        case TraderReturnStatusEnum.Draft:
                            model.REQUEST_STATUS = StatusLabelName.Draft;
                            model.REQUEST_CSS = StatusLabelStyle.Draft;
                            break;
                        case TraderReturnStatusEnum.Denied:
                            model.REQUEST_STATUS = StatusLabelName.Denied;
                            model.REQUEST_CSS = StatusLabelStyle.Denied;
                            break;
                        case TraderReturnStatusEnum.Discarded:
                            model.REQUEST_STATUS = StatusLabelName.Discarded;
                            model.REQUEST_CSS = StatusLabelStyle.Discarded;
                            break;
                    }

                }
                else if (approval.ConsumptionReports != null && approval.ConsumptionReports.Any())
                {
                    var consume = approval.ConsumptionReports.FirstOrDefault();
                    model.APPROVAL_TITLE = $"Consumption Report {approvalTitle}";
                    model.APP_NAME = "Spannered";
                    model.APP_TITLE = "Consumption Report";
                    model.APP_ICON = "/Content/DesignStyle/img/icon_spannered.png";
                    model.APP_MESSAGE = consume?.Name;
                    model.CSS_OVERVIEW = "task";
                    model.APP_LINK = "/Spanneredfree/ConsumeReportReview?id=" + consume?.Id;
                    model.IS_CONSUME_ASSOCICATE_TASK = consume.AssociatedTask != null ? true : false;
                }
                else if (approval.TillPayment != null && approval.TillPayment.Any())
                {
                    var tillPayment = approval.TillPayment.FirstOrDefault();
                    var directionName = tillPayment.Direction == Qbicles.Models.Trader.CashMgt.TillPayment.TillPaymentDirection.InToTill ? "Pay In" : "Pay Out";
                    model.APPROVAL_TITLE = $"Till {directionName} {approvalTitle}";
                    model.APP_TITLE = $"Approval Request";
                    model.APP_ICON = "/Content/DesignStyle/img/icon_cash.png";
                    if (tillPayment.Direction == Qbicles.Models.Trader.CashMgt.TillPayment.TillPaymentDirection.InToTill)
                    {
                        model.APP_MESSAGE = $"Till Payment from the Safe \"{tillPayment.AssociatedSafe.Name}\" to the Till \"{tillPayment.AssociatedTill.Name}\"";
                    }
                    else
                    {
                        model.APP_MESSAGE = $"Till Payment from the Till \"{tillPayment.AssociatedTill.Name}\" to the Safe \"{tillPayment.AssociatedSafe.Name}\"";
                    }

                    model.APP_LINK = "/CashManagement/TillPaymentReview?tillPaymentId=" + tillPayment.Id;
                }
                else
                {
                    return "";
                }
                return ApprovalRequestHtmlTemplate(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, approval, currentUserId, timezone);
                return "";
            }
        }
        public static string getQbicleStreamsHtml(QbicleStreamModel model, string currentUserId, string timezone, string dateFormat = "dd/MM/yyyy", int currentDomainId = 0)
        {
            if (model.Dates == null || !model.Dates.Any())
                return "";
            var today = DateTime.UtcNow.Date;
            StringBuilder htmlBuilder = new StringBuilder();
            foreach (var item in model.Dates)
            {
                var dateStr = item.Date.Date == today ? "Today" : item.Date.ConvertTimeFromUtc(timezone).DatetimeToOrdinal();
                var dateGroupId = dateStr == "Today" ? "dashboard-date-today" : dateStr;
                htmlBuilder.AppendLine($"<div id=\"{dateGroupId}\" class=\"{dateGroupId} day-block\">");
                htmlBuilder.AppendLine($"<div id=\"{dateGroupId}-sub\" class=\"day-date\"><span class=\"date\">{dateStr}</span></div>");
                foreach (var activity in item.Activities)//.OrderByDescending(e=>e.StartedDate))
                {
                    if (activity is QbiclePost)
                    {
                        var modelPost = (QbiclePost)activity;
                        htmlBuilder.AppendLine(getPostHtml(modelPost, currentUserId, timezone));
                    }
                    else
                    {
                        var modelActivity = (QbicleActivity)activity;
                        switch (modelActivity.ActivityType)
                        {
                            case ActivityTypeEnum.ApprovalRequest:
                                htmlBuilder.AppendLine(getApprovalRequestHtml(((ApprovalReq)modelActivity), model.Pinneds, currentUserId, timezone));
                                break;
                            case ActivityTypeEnum.ApprovalRequestApp:
                                var approvalReq = (ApprovalReq)modelActivity;
                                if (approvalReq.JournalEntries.Any())
                                    htmlBuilder.AppendLine(getJounralApprovalHtml(approvalReq, model.Pinneds, currentUserId, timezone));
                                else if (approvalReq.CampaigPostApproval.Any())
                                    htmlBuilder.AppendLine(getCampaigPostApprovalHtml(approvalReq, model.Pinneds, currentUserId, timezone));
                                else if (approvalReq.EmailPostApproval.Any())
                                    htmlBuilder.AppendLine(getEmailPostApprovalHtml(approvalReq, model.Pinneds, currentUserId, timezone));
                                else if (approvalReq.OperatorClockIn.Any())
                                    htmlBuilder.AppendLine(getOperatorClockHtml(approvalReq, model.Pinneds, currentUserId, timezone, true, dateFormat));
                                else if (approvalReq.OperatorClockOut.Any())
                                    htmlBuilder.AppendLine(getOperatorClockHtml(approvalReq, model.Pinneds, currentUserId, timezone, false, dateFormat));
                                break;
                            case ActivityTypeEnum.TaskActivity:
                                var task = (QbicleTask)modelActivity;
                                if (task.task != null)
                                    htmlBuilder.AppendLine(getCleanBookTaskHtml(task, model.Pinneds, currentUserId, timezone, dateFormat));
                                else if (task.ComplianceTask != null)
                                    htmlBuilder.AppendLine(getComplianceTaskHtml(task, model.Pinneds, currentUserId, timezone, dateFormat));
                                else
                                    htmlBuilder.AppendLine(getQbicleTaskHtml(task, model.Pinneds, currentUserId, timezone, dateFormat));
                                break;
                            case ActivityTypeEnum.AlertActivity:
                                var alert = (QbicleAlert)modelActivity;
                                htmlBuilder.AppendLine(getAlertHtml(alert, model.Pinneds, currentUserId, timezone));
                                break;
                            case ActivityTypeEnum.EventActivity:
                                var even = (QbicleEvent)modelActivity;
                                htmlBuilder.AppendLine(getEventHtml(even, model.Pinneds, currentUserId, timezone, dateFormat));
                                break;
                            case ActivityTypeEnum.MediaActivity:
                                var media = (QbicleMedia)modelActivity;
                                htmlBuilder.AppendLine(getMediaHtml(media, model.Pinneds, currentUserId, timezone));
                                break;
                            case ActivityTypeEnum.Link:
                                var link = (QbicleLink)modelActivity;
                                htmlBuilder.AppendLine(getLinkHtml(link, model.Pinneds, currentUserId, timezone));
                                break;
                            case ActivityTypeEnum.DiscussionActivity:
                            case ActivityTypeEnum.OrderCancellation:
                                var discussion = (QbicleDiscussion)modelActivity;
                                if (!model.IsFilterDiscussionOrder)
                                {
                                    if (discussion is B2CProductMenuDiscussion)
                                    {
                                        htmlBuilder.AppendLine(getCatalogDiscussionHtml((discussion as B2CProductMenuDiscussion), currentUserId));
                                    }
                                    else if (discussion.DiscussionType == DiscussionTypeEnum.B2CProductMenu || discussion.DiscussionType == DiscussionTypeEnum.B2COrder || discussion.DiscussionType == DiscussionTypeEnum.B2BOrder)
                                    {
                                        htmlBuilder.AppendLine(getDiscussionOrderHtml(discussion, currentUserId));
                                    }
                                    else if (discussion.DiscussionType == DiscussionTypeEnum.B2BCatalogDiscussion)
                                    {
                                        htmlBuilder.AppendLine(getB2BCatalogDiscussionHtml(discussion as B2BCatalogDiscussion, currentUserId, currentDomainId));
                                    }
                                    else
                                    {
                                        htmlBuilder.AppendLine(getDiscussionQbicleHtml(discussion, model.Pinneds, currentUserId, timezone));
                                    }
                                }
                                else if (discussion is B2CProductMenuDiscussion)
                                {
                                    htmlBuilder.AppendLine(getCatalogDiscussionHtml(discussion as B2CProductMenuDiscussion, currentUserId));
                                }
                                else
                                {
                                    htmlBuilder.AppendLine(getDiscussionOrderHtml(discussion, currentUserId));
                                }
                                break;
                            case ActivityTypeEnum.SharedHLPost:
                                var sharedPost = (HLSharedPost)modelActivity;
                                htmlBuilder.AppendLine(getHighLigthPostHtml(sharedPost, currentUserId, timezone, dateFormat).ToString());
                                break;
                            case ActivityTypeEnum.SharedPromotion:
                                var sharedPromotion = (LoyaltySharedPromotion)modelActivity;
                                htmlBuilder.AppendLine(getSharedPromotionHtml(sharedPromotion, currentUserId, timezone, dateFormat).ToString());
                                break;
                        }
                    }
                }
                htmlBuilder.AppendLine($"</div>");
            }
            return htmlBuilder.ToString();
        }
        public static StringBuilder GetActivityHtmlForSignalR(QbicleActivity activity, NotificationEventEnum notifyEvent, string currentUserId, string timezone, string dateFormat = "dd/MM/yyyy", bool isCustomerView = false)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            switch (activity.ActivityType)
            {
                case ActivityTypeEnum.ApprovalRequest:
                    htmlBuilder.AppendLine(getApprovalRequestHtml(((ApprovalReq)activity).BusinessMapping(timezone), null, currentUserId, timezone));
                    break;
                case ActivityTypeEnum.ApprovalRequestApp:
                    var approvalReq = ((ApprovalReq)activity).BusinessMapping(timezone);
                    if (approvalReq.JournalEntries.Any())
                        htmlBuilder.AppendLine(getJounralApprovalHtml(approvalReq, null, currentUserId, timezone));
                    else if (approvalReq.CampaigPostApproval.Any())
                        htmlBuilder.AppendLine(getCampaigPostApprovalHtml(approvalReq, null, currentUserId, timezone));
                    else if (approvalReq.EmailPostApproval.Any())
                        htmlBuilder.AppendLine(getEmailPostApprovalHtml(approvalReq, null, currentUserId, timezone));
                    else if (approvalReq.OperatorClockIn.Any())
                        htmlBuilder.AppendLine(getOperatorClockHtml(approvalReq, null, currentUserId, timezone, true, dateFormat));
                    else if (approvalReq.OperatorClockOut.Any())
                        htmlBuilder.AppendLine(getOperatorClockHtml(approvalReq, null, currentUserId, timezone, false, dateFormat));
                    break;
                case ActivityTypeEnum.TaskActivity:
                    var task = ((QbicleTask)activity).BusinessMapping(timezone);
                    if (notifyEvent == NotificationEventEnum.TaskNotificationPoints)
                    {

                    }
                    else
                    {
                        if (task.task != null)
                            htmlBuilder.AppendLine(getCleanBookTaskHtml(task, null, currentUserId, timezone, dateFormat));
                        else if (task.ComplianceTask != null)
                            htmlBuilder.AppendLine(getComplianceTaskHtml(task, null, currentUserId, timezone, dateFormat));
                        else
                            htmlBuilder.AppendLine(getQbicleTaskHtml(task, null, currentUserId, timezone, dateFormat));
                    }

                    break;
                case ActivityTypeEnum.AlertActivity:
                    var alert = ((QbicleAlert)activity).BusinessMapping(timezone);
                    htmlBuilder.AppendLine(getAlertHtml(alert, null, currentUserId, timezone));
                    break;
                case ActivityTypeEnum.EventActivity:
                    var even = ((QbicleEvent)activity).BusinessMapping(timezone);
                    if (notifyEvent == NotificationEventEnum.EventNotificationPoints)
                    {

                    }
                    else
                        htmlBuilder.AppendLine(getEventHtml(even, null, currentUserId, timezone, dateFormat));
                    break;
                case ActivityTypeEnum.MediaActivity:
                    var media = ((QbicleMedia)activity).BusinessMapping(timezone);
                    if (notifyEvent == NotificationEventEnum.MediaTabCreation)
                    {
                        htmlBuilder.AppendLine(getMediaHtml(media, null, currentUserId, timezone, true, isCustomerView));
                    }
                    else
                        htmlBuilder.AppendLine(getMediaHtml(media, null, currentUserId, timezone, false, isCustomerView));
                    break;
                case ActivityTypeEnum.Link:
                    var link = (QbicleLink)activity;
                    htmlBuilder.AppendLine(getLinkHtml(link, null, currentUserId, timezone));
                    break;
                case ActivityTypeEnum.DiscussionActivity:
                case ActivityTypeEnum.OrderCancellation:
                    var discussion = ((QbicleDiscussion)activity).BusinessMapping(timezone);
                    if (discussion.DiscussionType == DiscussionTypeEnum.B2CProductMenu || discussion.DiscussionType == DiscussionTypeEnum.B2COrder || discussion.DiscussionType == DiscussionTypeEnum.B2BOrder)
                    {
                        if (discussion is B2CProductMenuDiscussion)
                        {
                            htmlBuilder.AppendLine(getCatalogDiscussionHtml(discussion as B2CProductMenuDiscussion, currentUserId));
                        }
                        else
                        {
                            htmlBuilder.AppendLine(getDiscussionOrderHtml(discussion, currentUserId));
                        }
                    }
                    else if (discussion is B2CProductMenuDiscussion)
                    {
                        htmlBuilder.AppendLine(getCatalogDiscussionHtml(discussion as B2CProductMenuDiscussion, currentUserId));
                    }
                    else if (discussion is B2BCatalogDiscussion)
                    {
                        var b2bCatalogDiscussion = discussion as B2BCatalogDiscussion;
                        htmlBuilder.AppendLine(getB2BCatalogDiscussionHtml(b2bCatalogDiscussion, currentUserId, 0));
                    }
                    else
                    {
                        htmlBuilder.AppendLine(getDiscussionQbicleHtml(discussion, null, currentUserId, timezone));
                    }
                    break;
                case ActivityTypeEnum.SharedHLPost:
                    var sharedPost = ((HLSharedPost)activity).BusinessMapping(timezone);
                    htmlBuilder.AppendLine(getHighLigthPostHtml(sharedPost, currentUserId, timezone, dateFormat).ToString());
                    break;
                case ActivityTypeEnum.SharedPromotion:
                    var sharedPromotion = ((LoyaltySharedPromotion)activity).BusinessMapping(timezone);
                    htmlBuilder.AppendLine(getSharedPromotionHtml(sharedPromotion, currentUserId, timezone, dateFormat).ToString());
                    break;
            }
            return htmlBuilder;
        }
        #endregion
    }
    public class PostTemplateModel
    {
        public string POST_ID { get; set; }
        public string POST_KEY { get; set; }
        public string CSS_MYPOST_REPLY { get; set; }
        public string AVATAR { get; set; }
        public string CREATE_BY_NAME { get; set; }
        public string TIMELINE_DATE { get; set; }
        public int TOPIC_ID { get; set; }
        public string TOPIC_NAME { get; set; }
        public string MESSAGE { get; set; }
        public bool IS_CREATEBY { get; set; }
    }
    public class AlertTemplateModel
    {
        public string ALERT_ID { get; set; }
        public string ALERT_KEY { get; set; }
        public string CSS_MYPOST_REPLY { get; set; }
        public string AVATAR { get; set; }
        public string CREATE_BY_NAME { get; set; }
        public string TIMELINE_DATE { get; set; }
        public int TOPIC_ID { get; set; }
        public string TOPIC_NAME { get; set; }
        public string DATE_FORMAT { get; set; }
        public string FILE_URI { get; set; }
        public ActivityUpdateReasonEnum REASON { get; set; }
        public string ALERT_NAME { get; set; }
        public string DESCRIPTION { get; set; }
        public bool IS_PINNED { get; set; }
    }
    public class TaskTemplateModel
    {
        public string TASK_ID { get; set; }
        public string TASK_KEY { get; set; }
        public string CSS_MYPOST_REPLY { get; set; }
        public string AVATAR { get; set; }
        public string CREATE_BY_NAME { get; set; }
        public string TIMELINE_DATE { get; set; }
        public int TOPIC_ID { get; set; }
        public string TOPIC_NAME { get; set; }
        public ActivityUpdateReasonEnum REASON { get; set; }
        public TaskPriorityEnum PRIORITY { get; set; }
        public DateTime? PROGRAMMED_START { get; set; }
        public DateTime? PROGRAMMED_END { get; set; }
        public string DATE_FORMAT { get; set; }
        public string TASK_NAME { get; set; }
        public string DESCRIPTION { get; set; }
        public string CB_TASK_TYPENAME { get; set; }
        public int CT_ORDEREDFORMS_COUNT { get; set; }
        public int CT_ORDEREDFORMS_SUM_ESTIMATEDTIME { get; set; }
        public TaskType CT_TASK_TYPE { get; set; }
        public string TASK_URL { get; set; }
        public string WORKGROUP_NAME { get; set; }
        public bool IS_CREATEBY { get; set; }
        public bool IS_PINNED { get; set; }
        public bool IS_RECURS { get; set; }
    }
    public class EventTemplateModel
    {
        public string EVENT_ID { get; set; }
        public string EVENT_KEY { get; set; }
        public string CSS_MYPOST_REPLY { get; set; }
        public string AVATAR { get; set; }
        public string CREATE_BY_NAME { get; set; }
        public string TIMELINE_DATE { get; set; }
        public int TOPIC_ID { get; set; }
        public string TOPIC_NAME { get; set; }
        public ActivityUpdateReasonEnum REASON { get; set; }
        public DateTime? START { get; set; }
        public DateTime? END { get; set; }
        public string DATE_FORMAT { get; set; }
        public string EVENT_NAME { get; set; }
        public string DESCRIPTION { get; set; }
        public string LOCATION { get; set; }
        public bool IS_PINNED { get; set; }
    }
    public class MediaTemplateModel
    {
        public string MEDIA_ID { get; set; }
        public string MEDIA_KEY { get; set; }
        public string CSS_MYPOST_REPLY { get; set; }
        public string AVATAR { get; set; }
        public string CREATE_BY_NAME { get; set; }
        public string TIMELINE_DATE { get; set; }
        public int TOPIC_ID { get; set; }
        public string TOPIC_NAME { get; set; }
        public ActivityUpdateReasonEnum REASON { get; set; }
        public string MEDIA_TYPE { get; set; }
        public string MEDIA_NAME { get; set; }
        public string FILE_URI { get; set; }
        public string FILE_URI_MP4 { get; set; }
        public string FILE_URI_WEBM { get; set; }
        public string FILE_URI_OGV { get; set; }
        public string FILE_ICON { get; set; }
        public string FILE_EXTENSION { get; set; }
        public string FILE_UPLOADED_DATE { get; set; }
        public string DESCRIPTION { get; set; }
        public bool IS_PINNED { get; set; }
        public int FOLDER_ID { get; set; }
        public bool IsMediaOntab { get; set; }
        public bool IsCustomerView { get; set; } = false;
    }
    public class DiscussionTemplateModel
    {
        public string DISCUSSION_ID { get; set; }
        public string DISCUSSION_KEY { get; set; }
        public string DISCUSSION_NAME { get; set; }
        public string DISCUSSION_TITLE { get; set; }
        public string DISCUSSION_BREADCRUMB { get; set; }
        public string CSS_MYPOST_REPLY { get; set; }
        public string AVATAR { get; set; }
        public string CREATE_BY_NAME { get; set; }
        public string TIMELINE_DATE { get; set; }
        public int TOPIC_ID { get; set; }
        public string TOPIC_NAME { get; set; }
        public string DESCRIPTION { get; set; }
        public string DISCUSSION_URL { get; set; }
        public string FILE_URI { get; set; }
        public DiscussionTypeEnum DISCUSSION_TYPE { get; set; }
        public string DATE_FORMAT { get; set; }
        public bool IS_PINNED { get; set; }
    }
    public class LinkTemplateModel
    {
        public string LINK_ID { get; set; }
        public string LINK_KEY { get; set; }
        public string CSS_MYPOST_REPLY { get; set; }
        public string AVATAR { get; set; }
        public string CREATE_BY_NAME { get; set; }
        public string TIMELINE_DATE { get; set; }
        public int TOPIC_ID { get; set; }
        public string TOPIC_NAME { get; set; }
        public string DATE_FORMAT { get; set; }
        public string FILE_URI { get; set; }
        public string LINK_URL { get; set; }
        public string LINK_URL_HOST { get; set; }
        public ActivityUpdateReasonEnum REASON { get; set; }
        public string LINK_NAME { get; set; }
        public string DESCRIPTION { get; set; }
        public bool IS_PINNED { get; set; }
    }
    public class HLSharedPostTemplateModel
    {
        public string HL_ID { get; set; }
        public string HLPOST_ID { get; set; }
        //public string HLPOST_KEY { get; set; }
        public string CSS_MYPOST_REPLY { get; set; }
        public string AVATAR { get; set; }
        public string CREATE_BY_NAME { get; set; }
        public string SHARED_BY_NAME { get; set; }
        public string TIMELINE_DATE { get; set; }
        public string DATE_FORMAT { get; set; }
        public string POST_IMG_URI { get; set; }
        public string DOMAIN_LOGO_URI { get; set; }
        public string DOMAIN_NAME { get; set; }
        public string HLPOST_URL { get; set; }
        public HighlightPostType HLPOST_TYPE { get; set; }
        public string HLPOST_TYPE_DESCRIPTION { get; set; }
        public string HLPOST_LOCATION { get; set; }
        public string EVENT_HLPOST_LOCATION { get; set; }
        public string EVENT_HLPOST_STARTDATE { get; set; }
        public string EVENT_HLPOST_ENDDATE { get; set; }
        public string HLPOST_LIKEBY_COUNT { get; set; }
        public List<string> HLPOST_TAGS { get; set; }
        public string HLPOST_NAME { get; set; }
        public string HLPOST_DESCRIPTION { get; set; }
        public bool IS_EVENT_HL { get; set; } = false;
        public bool IS_CREATEBY { get; set; }
    }
    public class LoyaltySharedPromotionTemplateModel
    {
        public string PROMOTION_ID { get; set; }
        public string PROMOTION_KEY { get; set; }
        public string CSS_MYPOST_REPLY { get; set; }
        public string AVATAR { get; set; }
        public string CREATE_BY_NAME { get; set; }
        public string SHARED_BY_NAME { get; set; }
        public string TIMELINE_DATE { get; set; }
        public int TOPIC_ID { get; set; }
        public string TOPIC_NAME { get; set; }
        public string DATE_FORMAT { get; set; }
        public string POST_IMG_URI { get; set; }
        public string DOMAIN_LOGO_URI { get; set; }
        public string DOMAIN_NAME { get; set; }
        public string PROMOTION_URL { get; set; }
        public string PROMOTION_TYPE_DESCRIPTION { get; set; }
        public string PROMOTION_LOCATION { get; set; }
        public string PROMOTION_LIKEBY_COUNT { get; set; }
        public List<string> PROMOTION_TAGS { get; set; }
        public ActivityUpdateReasonEnum REASON { get; set; }
        public string PROMOTION_NAME { get; set; }
        public string PROMOTION_DESCRIPTION { get; set; }
        public string PROMOTION_REMAIN { get; set; }
        public bool IS_PROMO_OFFER_EXPIRED { get; set; }
        public bool IS_CREATEBY { get; set; }
    }
    public class JounralApprovalTemplateModel
    {
        public string JOUNRAL_ID { get; set; }
        public string JOUNRAL_KEY { get; set; }
        public string CSS_MYPOST_REPLY { get; set; }
        public string AVATAR { get; set; }
        public string CREATE_BY_NAME { get; set; }
        public string TIMELINE_DATE { get; set; }
        public int TOPIC_ID { get; set; }
        public string TOPIC_NAME { get; set; }
        public string JOUNRAL_NUMBER { get; set; }
        public string JOUNRAL_STATUS { get; set; }
        public string JOUNRAL_STATUS_CSS { get; set; }
        public string JOUNRAL_NAME { get; set; }
        public string JOUNRAL_DESCRIPTION { get; set; }
        public bool IS_CREATEBY { get; set; }
        public bool IS_PINNED { get; set; }
    }
    public class CampaigPostApprovalTemplateModel
    {
        public string CAMPAIGNPOST_APPROVAL_ID { get; set; }
        public string CAMPAIGNPOST_APP_ID { get; set; }
        public string CSS_MYPOST_REPLY { get; set; }
        public string AVATAR { get; set; }
        public string CREATE_BY_NAME { get; set; }
        public string TIMELINE_DATE { get; set; }
        public int TOPIC_ID { get; set; }
        public string APPROVAL_TITLE { get; set; }
        public string TOPIC_NAME { get; set; }
        public string DATE_FORMAT { get; set; }
        public string MEDIA_URI { get; set; }
        public string FILE_URI_MP4 { get; set; }
        public string FILE_URI_WEBM { get; set; }
        public string FILE_URI_OGV { get; set; }
        public string CAMPAIGNPOST_NUMBER { get; set; }
        public string REQUEST_CSS { get; set; }
        public string REQUEST_STATUS { get; set; }
        public string CAMPAIGNPOST_ASSOCIATED_NAME { get; set; }
        public string CAMPAIGNPOST_TITLE { get; set; }
        public string CAMPAIGNPOST_DESCRIPTION { get; set; }
        public bool IS_CREATEBY { get; set; }
        public bool IS_PINNED { get; set; }
        public bool IS_MANUAL_CAMPAIGN { get; set; }
        public bool IS_MEMBER { get; set; }
        public bool IS_MEDIA { get; set; }
        public bool IS_VIDEO { get; set; } = false;

    }
    public class EmailPostApprovalTemplateModel
    {
        public string POST_ID { get; set; }
        public string POST_APPROVAL_ID { get; set; }
        public string APP_ID { get; set; }
        public string CSS_MYPOST_REPLY { get; set; }
        public string AVATAR { get; set; }
        public string CREATE_BY_NAME { get; set; }
        public string TIMELINE_DATE { get; set; }
        public int TOPIC_ID { get; set; }
        public string TOPIC_NAME { get; set; }
        public string POST_IMG_URI { get; set; }
        public string POST_NAME { get; set; }
        public string POST_TITLE { get; set; }
        public string POST_DESCRIPTION { get; set; }
        public string REQUEST_CSS { get; set; }
        public string REQUEST_STATUS { get; set; }
        public bool IS_CREATEBY { get; set; }
        public bool IS_PINNED { get; set; }
    }
    public class OperatorClockTemplateModel
    {
        public string APP_ID { get; set; }
        public string CLOCK_APPROVAL_ID { get; set; }
        public string CLOCK_NAME { get; set; }
        public string CSS_MYPOST_REPLY { get; set; }
        public string AVATAR { get; set; }
        public string TIMELINE_DATE { get; set; }
        public int TOPIC_ID { get; set; }
        public string TOPIC_NAME { get; set; }
        public string WORKGROUP_LOCATION { get; set; }
        public string CLOCK_DATE { get; set; }
        public string CLOCK_TIME { get; set; }
        public string CLOCK_IN_NOTE { get; set; }
        public bool IS_CLOCK_IN { get; set; }
        public bool IS_PINNED { get; set; }
    }
    public class ApprovalRequestAppTemplateModel
    {
        public string APP_ID { get; set; }
        public string APP_KEY { get; set; }
        public string APP_NAME { get; set; }
        public string CSS_MYPOST_REPLY { get; set; }
        public string AVATAR { get; set; }
        public string CREATE_BY_NAME { get; set; }
        public string TIMELINE_DATE { get; set; }
        public int TOPIC_ID { get; set; }
        public string TOPIC_NAME { get; set; }
        public string APP_STATUS { get; set; }
        public string APP_STATUS_CSS { get; set; }
        public string APP_NOTE { get; set; }
        public bool IS_PINNED { get; set; }
    }
    public class ApprovalRequestTemplateModel
    {
        public string APP_ID { get; set; }
        public string APP_KEY { get; set; }
        public string APP_NAME { get; set; }
        public string APP_LINK { get; set; }
        public string APP_TITLE { get; set; }
        public string APP_MESSAGE { get; set; }
        public string APPROVAL_TITLE { get; set; }
        public string CSS_OVERVIEW { get; set; }
        public string CSS_MYPOST_REPLY { get; set; }
        public string AVATAR { get; set; }
        public string CREATE_BY_NAME { get; set; }
        public string STOCK_AUDIT_STARTED_DATE { get; set; }
        public bool IS_CONSUME_ASSOCICATE_TASK { get; set; }
        public string TIMELINE_DATE { get; set; }
        public int TOPIC_ID { get; set; }
        public string TOPIC_NAME { get; set; }
        public string APP_NOTE { get; set; }
        public string APP_ICON { get; set; }
        public bool IS_PINNED { get; set; }
        public string REQUEST_CSS { get; set; }
        public string REQUEST_STATUS { get; set; }
    }
}
