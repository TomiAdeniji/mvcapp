using Qbicles.Models.Base;
using Qbicles.Models.Trader.ODS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.DDS
{
    [Table("dds_Delivery")]
    public class Delivery : DataModelBase
    {

        public virtual QueueOrder ActiveOrder { get; set; }

        public virtual DeliveryQueue DeliveryQueue { get; set; }

        public virtual DeliveryQueueArchive DeliveryQueueArchive { get; set; }

        public virtual List<QueueOrder> Orders { get; set; } = new List<QueueOrder>();

        public virtual Driver Driver { get; set; }
        //{
        //    get { return this.Driver; }
        //    set
        //    {
        //        this.Driver = value;// Process is Terminated due to StackOverFlowException
        //        if (value != null)
        //        {
        //            this.DriverArchived = value; 
        //        }
        //    }
        //}
        /// <summary>
        /// Driver deliveried, use this while filter in Delivery Management
        /// </summary>
        public virtual Driver DriverArchived { get; set; }

        public DeliveryStatus Status { get; set; } = DeliveryStatus.New;


        public decimal Total { get; set; }


        public DateTime? TimeStarted { get; set; }

        public DateTime? TimeFinished { get; set; }
        /// <summary>
        /// secons
        /// </summary>
        public int? EstimateTime { get; set; }
        /// <summary>
        /// met
        /// </summary>
        public int? EstimateDistance { get; set; }

        //[System.Web.Mvc.AllowHtml]
        public string Routes { get; set; }

        public DateTime Timestamp { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        public string DeliveryProblemNote { get; set; }

        public virtual TraderReference Reference { get; set; }

        /// <summary>
        /// A discussion where this Delivery can be discussed
        /// </summary>
        public virtual QbicleDiscussion Discussion { get; set; }
    }

    public enum DeliveryStatus
    {
        New = 1,
        /// <summary>
        /// Driver accepted the delivery
        /// </summary>
        Accepted = 2,
        /// <summary>
        /// driver started the delivery
        /// </summary>
        Started = 3,
        /// <summary>
        /// Driver completed the delivery
        /// </summary>
        Completed = 4,
        [Description("Completed With Problems")]
        CompletedWithProblems = 5,
        Deleted = 6
    }
}
