using Qbicles.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles
{
    [Table("qb_updateqbicleprofile")]
    public class UpdateQbicleProfile
    {
        public int UserId { get; set; }
        public QbicleProfile QbicleProfile { get; set; }
    }
}
