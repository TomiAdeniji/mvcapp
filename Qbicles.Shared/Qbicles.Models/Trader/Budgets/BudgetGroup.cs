using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Budgets
{
    /// <summary>
    /// The BudgetGroup is used as the primary reporting item for Budgets
    /// i.e. each line item in the view of the Budget projection will be a Budget Group
    /// 
    /// In a practival sense the BudgetGroup is a collectio of Product Groups (TraderGroups) 
    /// The items of which will be used for calculating the Sales and Purchases withn a Budget Period.
    /// </summary>
    [Table("trad_BudgetGroup")]
    public class BudgetGroup
    {
        public int Id { get; set; }

        /// <summary>
        /// This is the title set for the Budget Group.
        /// It must be unioque withn a Location
        /// </summary>
        [Required]
        public string Title { get; set; }


        /// <summary>
        /// This is the description of the Budget Group
        /// </summary>
        public string Description { get; set; }


        /// <summary>
        /// This is the collection of Product Groups associated with the Budget Group.
        /// A ProductGroup, can appear at most in two BudgetGroups AT A LOCATION, 
        ///     one where BudgetGroup.Type = Expenditure
        ///     one where BudgetGroup.Type = Revenue
        /// </summary>
        public virtual List<TraderGroup> ExpenditureGroups { get; set; } = new List<TraderGroup>();
        public virtual List<TraderGroup> RevenueGroups { get; set; } = new List<TraderGroup>();
        /// <summary>
        /// This is to indicate whether the Budget Group is for Revenue or Expenditure
        /// </summary>
        [Required]
        public BudgetGroupType Type { get; set; }


        /// <summary>
        /// This is the Domain with which the BudgetGroup is associated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }

        /// <summary>
        /// These are the payment terms associated with the BudgetGroup
        /// </summary>
        [Required]
        public virtual PaymentTerms PaymentTerms { get; set; }


        /// <summary>
        /// This is the Location, within the Domain, at which this BudgetGroup is located.
        /// A BudgetGroup Title must be unique with any single location
        /// </summary>
        [Required]
        public virtual TraderLocation Location { get; set; }


        /// <summary>
        /// The user from Qbicles who created the Budget Group
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this Budget Group was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the Budget Group, this is to be set each time the Budget Group is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this Budget Group was last edited.
        /// This is to be set each time the Budget Group is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }

        /// <summary>
        /// This is a list of the BudgetScenarios with which the budget group is associated.
        /// It is part of a many-to-many relationship
        /// </summary>
        public virtual List<BudgetScenario> BudgetScenarios { get; set; } = new List<BudgetScenario>();

        

    }


    public enum BudgetGroupType
    {
        Expenditure = 1,
        Revenue = 2
    }


}
