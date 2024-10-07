using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Base
{
	public class DataModelBase
	{
		public int Id { get; set; }

		[NotMapped]
		public string Key
		{
			get { return Id.Encrypt(); }
			set { Id = string.IsNullOrEmpty(value) ? 0 : int.Parse(value.Decrypt()); }
		}
	}
}