using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.Resources
{
    [Table("trad_ResourceAccessArea")]
    public class AccessArea
    {
        public int Id { get; set; }


        /// <summary>
        /// This is the name of the Access Area
        /// It MUST be unique with a Domain
        /// </summary>
        [Required]
        public string AreaName { get; set; }


        /// <summary>
        /// This is to indicate the area at a location in which the Access Area is located
        /// i.e. In the warehouse, in the store etc
        /// </summary>
        public AccessAreaType Type { get; set; }

        /// <summary>
        /// This is the Location of the Access Area
        /// It can be selected by the user but is not required.
        /// </summary>
        public virtual TraderLocation Location {get; set;}

        /// <summary>
        /// This is the URI for the image.
        /// The image is stored using QBicles.Doc and the resulting GUID stored in this field
        /// </summary>
        [Required]
        public string ImageUri { get; set; }


        /// <summary>
        /// The user from Qbicles who created the Access ARea
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this Access Area was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }



        /// <summary>
        /// The Domain with which this Resource Access Area is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }


        /// <summary>
        /// The description of the Access Area
        /// </summary>
        public string Description { get; set; }
    }

    public enum AccessAreaType
    {
        Warehouse = 1,
        Instore = 2
    }
}
