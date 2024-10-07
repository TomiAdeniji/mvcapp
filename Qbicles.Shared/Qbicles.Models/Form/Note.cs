using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Form
{
    public class Note : Attachment
    {
        [Required]
        public string Text { get; set; }

        public Note ()
        {
            this.AttachmentType = AttachmentTypeEnum.Note;
        }
    }
}
