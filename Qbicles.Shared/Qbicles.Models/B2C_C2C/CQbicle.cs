using System.Collections.Generic;

namespace Qbicles.Models.B2C_C2C
{
    /// <summary>
    /// This class is the base class for all Qbicles that are related to Custimer QBicles B2C & C2C
    /// </summary>
    public class CQbicle : Qbicle
    {
        /// <summary>
        /// This is the collection of customers (ApplicationUsers) who like (The loveheart in the UI) the QBicle
        /// </summary>
        public virtual List<ApplicationUser> LikedBy { get; set; } = new List<ApplicationUser>();


        /// <summary>
        /// This indicates the status of the communications between the parties in the Qbicle
        /// </summary>
        public CommsStatus Status { get; set; }


        /// <summary>
        /// If the status of the relationship is Blocked, who blocked it 
        /// </summary>
        public virtual ApplicationUser Blocker { get; set; }
    }

    public enum CommsStatus
    {
        Pending = 1,
        /// <summary>
        /// approve request
        /// </summary>
        Approved = 2,
        /// <summary>
        /// block request
        /// </summary>
        Blocked = 3,
        /// <summary>
        /// cancel request
        /// </summary>
        Cancel = 4
    }
}
