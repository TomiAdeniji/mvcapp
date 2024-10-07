using Qbicles.Models.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Bookkeeping
{
    [Table("bk_coanode")]
    public class CoANode
    {
        [Required]
        public int Id { get; set; }


        public string Number { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public QbicleDomain Domain { get; set; }
        
        public decimal? Credit { get; set; }

        public decimal? Debit { get; set; }

        public decimal? Balance { get; set; }

        public virtual CoANode Parent { get; set; }

        public virtual List<CoANode> Children { get; set; } = new List<CoANode>();

        public BKAccountTypeEnum AccountType { get; set; }

        
        public string Description { get; set; }


        public CoANode()
        {


            // Added by DJN
            // This property sets the type of the Node to be SubGroup
            // The Group & Account objects will overwrite this property
            // and the SubGroup objects will not which will leave those CoANodew, wheich should be SUbGroups, correctly identified as SubGroups
            // QBIC-775 contains the details
            this.NodeType = BKCoANodeTypeEnum.SubGroup;
        }


        [NotMapped]
        public BKCoANodeTypeEnum NodeType { get; set; }

        public enum BKAccountTypeEnum
        {
            Assets = 1,
            Liabilities = 2,
            Expenses = 3,
            Equity = 4,
            Revenue = 5
        };

        public enum BKCoANodeTypeEnum
        {
            Group = 1,
            SubGroup = 2,
            Account = 3,
            GroupList = 4
        }
    }
}