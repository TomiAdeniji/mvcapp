using Qbicles.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Qbicles.Models.SalesMkt
{
    [Table("sm_CampaignEmail")]
    public class CampaignEmail : DataModelBase
    {

        /// <summary>
        /// This is associated with CampaignEmail
        /// Use to build a email template when sending emails
        /// </summary>
        public virtual EmailTemplate Template { get; set; }

        #region Content

        /// <summary>
        /// This is the Headline of the email
        /// </summary>
        [Required]
        public string Headline { get; set; }


        /// <summary>
        /// This is the html that is the body of the email.
        /// It must be stored and retrieved as the HTML string
        /// </summary>
        [Required]
        public string BodyContent { get; set; }


        /// /// <summary>
        /// This is the image that is associated with the email .
        /// The image is the latest version in the QbicleMedia object
        /// </summary>
        [Required]
        public virtual QbicleMedia PromotionalImage { get; set; }

        /// <summary>
        /// This is the text that will appear on the button in the email.
        /// </summary>
        public string ButtonText { get; set; }

        /// <summary>
        /// This is the URL that will that will be opened by the button in the email.
        /// </summary>
        public string ButtonLink { get; set; }


        /// <summary>
        /// This is the image that is used as an advertisement the email .
        /// The image is the latest version in the QbicleMedia object
        /// </summary>
        public virtual QbicleMedia AdvertisementImage { get; set; }

        #endregion

        /// <summary>
        /// This is the image that is associated with the email.
        /// The image is uploaded and stored using the usual Qbicle.Docs procedure and the unique GUID
        /// identifier is stored in this field
        /// </summary>
        [Required]
        public string FeaturedImageUri { get; set; }

        /// <summary>
        /// The Title of the email.
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// This is the  Email Subject that will be used in sending an email from the campaign.
        /// It will be the EmailCampaign.DefaultReplyEmail unless the user specifies a different email address
        /// </summary>
        [Required]
        public string EmailSubject { get; set; }

        /// <summary>
        /// This is the Reply Email that will be used in sending an email from the campaign.
        /// It will be the EmailCampaign.DefaultReplyEmail unless the user specifies a different email address
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        public string ReplyEmail { get; set; }


        /// <summary>
        /// This is the FROM NAME that will be used in sending an email from the campaign, 
        /// It will be the EmailCampaign.DefaultFromName unless the user specifies a different email from name
        /// </summary>
        [Required]
        public string FromName { get; set; }

        /// <summary>
        /// This is the FROM EMAIL ADDRESS that will be used in sending an email from the campaign, 
        /// It will be the EmailCampaign.DefaultFromEmail unless the user specifies a different email from name
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        public string FromEmail { get; set; }



        /// <summary>
        /// This is the collection of Segments, the contacts of which will be sent the email
        /// This is a subset of the segments associated with the Email Campaign
        /// </summary>
        public virtual List<Segment> Segments { get; set; } = new List<Segment>();



        // <summary>
        /// The user from Qbicles who created the email
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this email was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// The user from Qbicles who last updated the email, this is to be set each time the email is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this email was last edited.
        /// This is to be set each time the email is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }


        /// <summary>
        /// This is the Email Campaign with which this email is associated
        /// </summary>
        public virtual EmailCampaign Campaign { get; set; }
    }
}
