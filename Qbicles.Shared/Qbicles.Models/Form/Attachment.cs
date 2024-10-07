using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Form
{
    /// <summary>
    /// THis is an item that can be attached for a FromElementData by a user
    /// </summary>
    [Table("form_attachment")]
    public abstract class Attachment
    {

        [Required]
        public int Id { get; set; }

        /// <summary>
        /// This is the FormElementData with which the attachment is associated
        /// </summary>
        [Required]
        public virtual FormElementData ParentElementData{ get; set; }

        /// <summary>
        /// This indicates which type of attachment it is.
        /// </summary>
        [Required]
        public virtual AttachmentTypeEnum AttachmentType { get; set; }

        public enum AttachmentTypeEnum
        {
            Note = 1,
            Image = 2,
            Doc = 3
        }

    }
}
