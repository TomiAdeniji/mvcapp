using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_ContactRef")]
    public class TraderContactRef
    {
        public int Id { get; set; }
        public virtual QbicleDomain Domain { get; set; }
        public int ReferenceNumber { get; set; }
        public string Reference { get; set; }
    }
}
