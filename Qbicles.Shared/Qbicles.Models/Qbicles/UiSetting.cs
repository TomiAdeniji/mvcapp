using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    /// <summary>
    ///  Model for a UiSetting
    /// </summary>
    [Table("qb_uisettings")]
    public class UiSetting
    {
        public int Id { get; set; }

        [Required]
        public string CurrentPage { get; set; }
        [Required]
        public ApplicationUser CurrentUser { get; set; }
        [Required]
        public string Key { get; set; }
        public string Value { get; set; }

    }
}