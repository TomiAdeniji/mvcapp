using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.Resources
{
    [Table("trad_ResourceDocument")]
    public class ResourceDocument
    {
        [Required]
        public int Id { get; set; }


        /// <summary>
        /// The name of the file
        /// It MUST be unique with a Domain
        /// </summary>
        [Required]
        public string Name { get; set; }



        /// <summary>
        /// The description of the file
        /// </summary>
        public string Description { get; set; }


        /// <summary>
        /// This is the URI for the image or document URI
        /// The file is stored using QBicles.Doc and the resulting GUID stored in this field
        /// </summary>
        [Required]
        public string FileUri { get; set; }


        /// <summary>
        /// The user from Qbicles who created the file
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this file was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }



        /// <summary>
        /// The Domain with which this Resource File is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }


        /// <summary>
        /// This is the type of the file and is set when the file is uploaded
        /// </summary>
        [Required]
        public virtual QbicleFileType Type { get; set; }



        /// <summary>
        /// This is a list of the products associated with Additional Info
        /// </summary>
        public virtual List<TraderItem> TraderItems { get; set; } = new List<TraderItem>();



        /// <summary>
        /// This is the category of the document
        /// </summary>
        [Required]
        public virtual ResourceCategory Category { get; set; }

    }
}
