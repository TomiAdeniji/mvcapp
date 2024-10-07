using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_emailtemplates")]
    public class EmailTemplate
    {
        /// <summary>
        /// The unique ID to identify the sales and marketing EmailTemplate in the database
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// This is the Domain with which this EmailTemplate is associated
        /// </summary>
        [Required]
        public QbicleDomain Domain { get; set; }
        [Required]
        [StringLength(150)]
        public string TemplateName { get; set; }
        [Required]
        [StringLength(350)]
        public string TemplateDescription { get; set; }
        /// <summary>
        /// Heading background colour (hex code)
        /// </summary>
        [Required]
        public string HeadingBg { get; set; }
        /// <summary>
        /// Headline (text) **
        /// </summary>
        [Required]
        public string HeadlineText { get; set; }
        /// <summary>
        /// Headline colour (hex code)
        /// </summary>
        [Required]
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
        [Required]
        public string BodyBg { get; set; }
        /// <summary>
        /// Body text colour (hex code)
        /// </summary>
        [Required]
        public string BodyTextColour { get; set; }
        /// <summary>
        /// Body content (WYSIWYG output) **
        /// </summary>
        [Required]
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
        [Required]
        public bool ButtonIsHidden { get; set; }
        /// <summary>
        /// Featured image (image/path) **
        /// </summary>
        [Required]
        public string FeaturedImage { get; set; }
        /// <summary>
        /// Button text (text) **
        /// </summary>
        [Required]
        public string ButtonText { get; set; }
        /// <summary>
        /// Button text colour (hex)
        /// </summary>
        [Required]
        public string ButtonTextColour { get; set; }
        /// <summary>
        /// Button link (URL) **
        /// </summary>
        [Required]
        public string ButtonLink { get; set; }
        /// <summary>
        /// Button background colour (hex) 
        /// </summary>
        [Required]
        public string ButtonBg { get; set; }
        /// <summary>
        /// Button font (Google font from dropdown selection)
        /// </summary>
        public string ButtonFont { get; set; }
        /// <summary>
        /// Button font size (number) 
        /// </summary>
        [Required]
        public string ButtonFontSize { get; set; }
        /// <summary>
        /// Advert image isHidden boolean
        /// </summary>
        [Column(TypeName = "bit")]
        [Required]
        public bool AdvertImgiIsHidden { get; set; }
        /// <summary>
        /// Advert image (image/path) **
        /// </summary>
        [Required]
        public string AdvertImage { get; set; }
        /// <summary>
        /// Advert link (URL) **
        /// </summary>
        [Required]
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
        [Column(TypeName = "bit")]
        [Required]
        public bool IsHiddenFacebook { get; set; }
        /// <summary>
        /// Social network is Hidden Instagram
        /// </summary>
        [Column(TypeName = "bit")]
        [Required]
        public bool IsHiddenInstagram { get; set; }
        /// <summary>
        /// Social network is Hidden LinkedIn
        /// </summary>
        [Column(TypeName = "bit")]
        [Required]
        public bool IsHiddenLinkedIn { get; set; }
        /// <summary>
        /// Social network is Hidden Pinterest
        /// </summary>
        [Column(TypeName = "bit")]
        [Required]
        public bool IsHiddenPinterest { get; set; }
        /// <summary>
        /// Social network is Hidden Twitter
        /// </summary>
        [Column(TypeName = "bit")]
        [Required]
        public bool IsHiddenTwitter { get; set; }
        /// <summary>
        /// Social network is Hidden Youtube
        /// </summary>
        [Column(TypeName = "bit")]
        [Required]
        public bool IsHiddenYoutube { get; set; }
        [Required]
        public DateTime CreateDate { get; set; }
        [Required]
        public ApplicationUser CreateBy { get; set; }

    }
}
