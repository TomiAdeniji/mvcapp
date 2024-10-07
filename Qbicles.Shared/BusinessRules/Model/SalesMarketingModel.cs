using Qbicles.Models;
using Qbicles.Models.SalesMkt;
using System;
using System.Collections.Generic;

namespace Qbicles.BusinessRules.Model
{
    public class Select2CustomeModel
    {
        public int id { get; set; }
        public string text { get; set; }
    }
    public class SelectCustomeModel
    {
        public string id { get; set; }
        public string text { get; set; }
    }
    public class AccountNetworkCustomeModel
    {
        public string SocialName { get; set; }
        public string SocialUsername { get; set; }
    }
    public class workgroupCustomeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DomainId { get; set; }
        public string DomainKey { get; set; }
        public int QbicleId { get; set; }
        public int TopicId { get; set; }
        public int[] Process { get; set; }
        public string[] Members { get; set; }
        public string[] ReviewersApprovers { get; set; }
        public ApplicationUser CreateBy { get; set; }
    }
    public class FacebookPageGroupModel
    {
        public long id { get; set; }
        public string name { get; set; }
        public string access_token { get; set; }
        public string picture { get; set; }
    }
    public class BrandCustomModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int FeaturedImageUri { get; set; }
        /// <summary>
        /// eg: auto genarate SM-BRAND-001
        /// </summary>
        public string FolderName { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public QbicleDomain CurrentDomain { get; set; }
    }
    public class BrandProductCustomModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public int BrandId { get; set; }        
        public ApplicationUser CurrentUser { get; set; }
    }
    public class BrandAttributeGroupCustomModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public int BrandId { get; set; }        
        public ApplicationUser CurrentUser { get; set; }
        public List<int> Attributes { get; set; }
    }
    public class BrandValuePropositionCustomModel
    {
        public int Id { get; set; }
        public int BrandId { get; set; }
        public int ProductId { get; set; }
        public string WhoWantTo { get; set; }
        public string By { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public List<int> CustomerSegment { get; set; }
    }
    public class BrandAttributeCustomModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public int BrandId { get; set; }
        public ApplicationUser CurrentUser { get; set; }
    }
    public class IdeaThemeCustomeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Explanation { get; set; }
        public int Type { get; set; }
        public List<string> Links { get; set; }
        public int ResourcesFolder { get; set; }
        public string FolderName { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public QbicleDomain CurrentDomain { get; set; }
        public bool? Active { get; set; }
    }
    public class DiscussionIdeaCustomeModel
    {
        public int id { get; set; }
        public int ideaId { get; set; }
        public string openingmessage { get; set; }
        public string expirydate { get; set; }
        public bool isexpiry { get; set; }
        public ApplicationUser currentuser { get; set; }
        public string currenttimezone { get; set; }
    }

    public class DiscussionPlaceModel
    {
        public int id { get; set; }
        public int placeId { get; set; }
        public string openingmessage { get; set; }
        public string expirydate { get; set; }
        public bool isexpiry { get; set; }
        public ApplicationUser currentuser { get; set; }
        public string currenttimezone { get; set; }
    }
    public class DiscussionGoalModel
    {
        public int id { get; set; }
        public int goalId { get; set; }
        public string openingmessage { get; set; }
        public string expirydate { get; set; }
        public bool isexpiry { get; set; }
        public ApplicationUser currentuser { get; set; }
        public string currenttimezone { get; set; }
    }

    public class DiscussionPerfomanceModel
    {
        public int id { get; set; }
        public int perfomanceId { get; set; }
        public string openingmessage { get; set; }
        public string expirydate { get; set; }
        public bool isexpiry { get; set; }
        public ApplicationUser currentuser { get; set; }
        public string currenttimezone { get; set; }
    }
    public class DiscussionComplianceTaskModel
    {
        public int id { get; set; }
        public int taskId { get; set; }
        public string openingmessage { get; set; }
        public string expirydate { get; set; }
        public bool isexpiry { get; set; }
        public ApplicationUser currentuser { get; set; }
        public string currenttimezone { get; set; }
    }
    public class CriteriaCustomeModel
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public bool IsMandatory { get; set; }
        public ApplicationUser Currentuser { get; set; }
        public QbicleDomain Domain { get; set; }
        public List<OptionValueCustomeModel> Options { get; set; }
    }
    public class OptionValueCustomeModel
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public int DisplayOrder { get; set; }
    }
    public class CriteriaReOrderModel
    {
        public int MoveUpId { get; set; }
        public int MoveUpOrder { get; set; }
        public int MoveDownId { get; set; }
        public int MoveDownOrder { get; set; }
    }
    public class SegmentCustomeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public SegmentType Type { get; set; }
        public List<ClauseCriteriaModel> Criterias { get; set; }
        public List<int> Contacts{ get; set; }
        public List<int> Areas { get; set; }
        public QbicleDomain Domain { get; set; }
        public ApplicationUser Currentuser { get; set; }
    }
    public class ClauseCriteriaModel
    {
        public int CriteriaId { get; set; }
        public List<int> CriteriaValues { get; set; }
    }
    public class ContentMoreModel
    {
        public int TotalRecords { get; set; }
        public string HtmlRender { get; set; }
    }

    public class ScheduledVisitModel
    {
        public string DateTimeOfVisit { get; set; }
        public string Agent { get; set; }
        public string Status { get; set; }
        public string Duration { get; set; }
    }

    public class VisitLogModel
    {
        public string DateTimeOfVisit { get; set; }
        public DateTime VisitDate { get; set; }
        public string Agent { get; set; }
        public VisitReason Reason { get; set; }
        public string txtReason { get; set; }
        public int Leads { get; set; }
        public string Notes { get; set; }
        public long TaskId { get; set; }
        public long placeId { get; set; }
    }

    public class PlaceActivityModel
    {
        public string Date { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Agent { get; set; }
        public string Timeframe { get; set; }
        public int Recorded { get; set; }
        public string Notes { get; set; }
        public long placeId { get; set; }
    }

    public class SMContactModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string DateOfBirth { get; set; }
        public DateTime BirthDay { get; set; }
        public List<string> AgeRanges { get; set; }
        public string Phone { get; set; }
        public int Source { get; set; }
        public string SourceName { get; set; }
        public List<long> Places { get; set; }
        public List<string> Options { get; set; }
        public string AvatarUri { get; set; }
        public string SourceDescription { get; set; }
        public string ReceiveEmail { get; set; }
    }

    public class EmailCampaignModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class PipelineTasksModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Deadline { get; set; }
        public string Status { get; set; }
    }
    public class PipelineEventsModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string StartDate { get; set; }

    }

    public class BrandCampaignModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int NumberOfQueue { get; set; }
        public int NumberOfCompletedPost { get; set; }
    }

    public class ThemeCampaignModel
    {
        public int PostId { get; set; }
        public string PostTitle { get; set; }
        public string CampaignName { get; set; }
        public string Type { get; set; }
        public DateTime DateOfIssue { get; set; }
        public string StrDateOfIssue { get; set; }
        public string Status { get; set; }
    }

    public class SocialCampaignModel
    {
        public bool IsHalted { get; set; }
        public SocialCampaign Campaign { get; set; }
    }

    public class HaltedEmailCampaignModel
    {
        public bool IsHalted { get; set; }
        public EmailCampaign Campaign { get; set; }
    }
    public class EmailTemplateModel
    {
        /// <summary>
        /// The unique ID to identify the sales and marketing EmailTemplate in the database
        /// </summary>
        public int Id { get; set; }

        public string TemplateName { get; set; }

        public string TemplateDescription { get; set; }
        /// <summary>
        /// Heading background colour (hex code)
        /// </summary>

        public string HeadingBg { get; set; }
        /// <summary>
        /// Headline (text) **
        /// </summary>

        public string HeadlineText { get; set; }
        /// <summary>
        /// Headline colour (hex code)
        /// </summary>

        public string HeadlineColour { get; set; }
        /// <summary>
        /// Headline font (Google font from dropdown selection)
        /// </summary>

        public string HeadlineFont { get; set; }
        /// <summary>
        /// Headline font size (number)
        /// </summary>

        public string HeadlineFontSize { get; set; }
        /// <summary>
        /// Body background colour (hex code)
        /// </summary>

        public string BodyBg { get; set; }
        /// <summary>
        /// Body text colour (hex code)
        /// </summary>

        public string BodyTextColour { get; set; }
        /// <summary>
        /// Body content (WYSIWYG output) **
        /// </summary>

        public string BodyContent { get; set; }
        /// <summary>
        /// Body font (Google font from dropdown selection)
        /// </summary>

        public string BodyFont { get; set; }
        /// <summary>
        /// Body font size (number)
        /// </summary>

        public string BodyFontSize { get; set; }
        /// <summary>
        /// Button isHidden boolean
        /// </summary>

        public bool ButtonIsHidden { get; set; }
        /// <summary>
        /// Featured image (image/path) **
        /// </summary>

        public string FeaturedImage { get; set; }
        /// <summary>
        /// Button text (text) **
        /// </summary>

        public string ButtonText { get; set; }
        /// <summary>
        /// Button text colour (hex)
        /// </summary>

        public string ButtonTextColour { get; set; }
        /// <summary>
        /// Button link (URL) **
        /// </summary>

        public string ButtonLink { get; set; }
        /// <summary>
        /// Button background colour (hex) 
        /// </summary>

        public string ButtonBg { get; set; }
        /// <summary>
        /// Button font (Google font from dropdown selection)
        /// </summary>
        public string ButtonFont { get; set; }
        /// <summary>
        /// Button font size (number) 
        /// </summary>

        public string ButtonFontSize { get; set; }
        /// <summary>
        /// Advert image isHidden boolean
        /// </summary>

        public bool AdvertImgiIsHidden { get; set; }
        /// <summary>
        /// Advert image (image/path) **
        /// </summary>

        public string AdvertImage { get; set; }
        /// <summary>
        /// Advert link (URL) **
        /// </summary>

        public string AdvertLink { get; set; }
        /// <summary>
        /// Social network Facebook
        /// </summary>
        public string FacebookLink { get; set; }
        /// <summary>
        /// Social network Instagram
        /// </summary>
        public string InstagramLink { get; set; }
        /// <summary>
        /// Social network Linked
        /// </summary>
        public string LinkedInLink { get; set; }
        /// <summary>
        /// Social network Pinterest
        /// </summary>
        public string PinterestLink { get; set; }
        /// <summary>
        /// Social network Twitter
        /// </summary>
        public string TwitterLink { get; set; }
        /// <summary>
        /// Social network Youtube
        /// </summary>
        public string YoutubeLink { get; set; }

        /// <summary>
        /// Social network is Hidden Facebook
        /// </summary>

        public bool IsHiddenFacebook { get; set; }
        /// <summary>
        /// Social network is Hidden Instagram
        /// </summary>

        public bool IsHiddenInstagram { get; set; }
        /// <summary>
        /// Social network is Hidden LinkedIn
        /// </summary>

        public bool IsHiddenLinkedIn { get; set; }
        /// <summary>
        /// Social network is Hidden Pinterest
        /// </summary>

        public bool IsHiddenPinterest { get; set; }
        /// <summary>
        /// Social network is Hidden Twitter
        /// </summary>

        public bool IsHiddenTwitter { get; set; }
        /// <summary>
        /// Social network is Hidden Youtube
        /// </summary>

        public bool IsHiddenYoutube { get; set; }

    }


}
