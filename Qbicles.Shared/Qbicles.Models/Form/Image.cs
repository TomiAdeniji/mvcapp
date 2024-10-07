using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Form
{
    public class Image : Attachment
    {
        [Required]
        public string ImageUri { get; set; }
        [Required]
        public string ImageName { get; set; }

        public Image()
        {
            this.AttachmentType = AttachmentTypeEnum.Image;
        }
    }
}
