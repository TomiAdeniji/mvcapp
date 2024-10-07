using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Loyalty
{
    [Table("loy_o2p_conversion")]
    public class OrderToPointsConversion
    {

        public OrderToPointsConversion()
        {
            this.ConversionType = OrderToPointsConversionType.NotSet;
            this.IsArchived = false;
        }

        public int Id { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public OrderToPointsConversionType ConversionType { get; set; }


        [Required]
        [Column(TypeName = "bit")]
        public bool IsArchived { get; set; }


        public virtual ApplicationUser ArchivedBy { get; set; }


        public DateTime ArchivedDate { get; set; }




    }

    public enum OrderToPointsConversionType
    {
        NotSet = 0,
        Payment = 1
    }
}
