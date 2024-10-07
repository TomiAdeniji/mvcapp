using Qbicles.Models.Form;
using Qbicles.Models.Operator.Goals;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Operator
{
    /// <summary>
    /// The tag is attached to a From Definition for the purpose of searching for form definitions within a Domain
    /// </summary>
    [Table("op_tags")]
    public class OperatorTag
    {
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// This is the name  of the Tag
        /// It must be unique within a Domain
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// This is a description of the tag
        /// </summary>
        [StringLength(300)]
        public string Summary { get; set; }

        /// <summary>
        /// This is the Domain with which the Tag is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }

        /// <summary>
        /// This is the collection of the FromDefintions with which the tag is associated
        /// It is part of a many-to-many relationship
        /// </summary>
        public virtual List<FormDefinition> FormDefinitions { get; set; }

        /// <summary>
        /// This is the collection of the Goals with which the tag is associated
        /// It is part of a many-to-many relationship
        /// </summary>
        public virtual List<Goal> Goals { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

    }
}
