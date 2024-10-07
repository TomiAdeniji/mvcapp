
using Qbicles.Models.Base;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Loyalty
{
	[Table("loy_bulkdeal_Voucher")]
	public class BulkDealVoucher : DataModelBase
	{
		[Required]
		public DateTime CreatedDate { get; set; }

		[Required]
		public virtual LoyaltyBulkDealPromotion BulkDealPromotion { get; set; }

		[Required]
		public virtual QbicleDomain OptInBusiness { get; set; }


		[Column(TypeName = "bit")]
		public bool IsOptIn { get; set; } = false;

		public DateTime OptInDate { get; set; }

		public DateTime? VoucherExpiryDate { get; set; } = null;
	}

}
