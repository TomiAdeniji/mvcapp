using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Highlight
{
    [Table("hl_userpostxref")]
    public class UserAndHighlightPostXref
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int HighlightPostId { get; set; }
        public bool IsLiked { get; set; }
        public bool IsBookmarked { get; set; }
        public bool IsFlagged { get; set; }
    }
}
