using System.ComponentModel.DataAnnotations;

namespace Qbicles.Models.Form
{
    public class Document : Attachment
    {
        [Required]
        public string DocumentUri { get; set; }
        [Required]
        public string DocumentName { get; set; }

        public Document()
        {
            this.AttachmentType = AttachmentTypeEnum.Doc;
        }
    }
}
