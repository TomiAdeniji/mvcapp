using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.B2B
{
    [Table("b2b_logisticsagreement")]
    public class B2BLogisticsAgreement
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual LogisticsPartnership LogisticsPartnership { get; set; }

        [Required]
        public AgreementStatus Status { get; set; }

        public virtual B2BProviderPriceList PriceList { get; set; }

        public DateTime CreatedDate { get; set; } // - Draft date

        public DateTime ActivatedDate { get; set; }

        public DateTime ArchivedDate { get; set; } // - Draft date


        [Column(TypeName = "bit")]
        public bool IsProviderAgreed { get; set; }


        [Column(TypeName = "bit")]
        public bool IsConsumerAgreed { get; set; }

        public virtual List<TraderLocation> ConsumerLocations { get; set; } = new List<TraderLocation>();
    }
    public enum AgreementStatus
    {
        IsDraft = 0,
        IsActive = 1,
        IsArchived = 2
    }
}
