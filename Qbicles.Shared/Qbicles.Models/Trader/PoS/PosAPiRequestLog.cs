using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.PoS
{
    [Table("pos_apirequestlog")]
    public class PosApiRequestLog
    {
        public int Id {get; set;}

        /// <summary>
        /// The date and time at which the log was created (UtcNow)
        /// 
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }
        

        /// <summary>
        /// This is the name of the API controller that was called in the request
        /// </summary>
        [Required]
        public string ApiControllerName { get; set; }

        /// <summary>
        /// This is the header information that was supplied by the request
        /// </summary>
        [Required]
        public string RequestHeader { get; set; }

        /// <summary>
        /// The query string supplied by the request
        /// </summary>
        public string QueryString { get; set; }


        /// <summary>
        /// If a valid device ID is supplied then link the relevant device here
        /// </summary>
        public virtual PosDevice Device { get; set; }


        /// <summary>
        /// If a Token can be found in the request it is to be stored here
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Indicate whether the supplied token was valid or not
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsTokenValid { get; set; }


        /// <summary>
        /// If a valid User ID is supplied then link the relevant application user here
        /// </summary>
        public virtual ApplicationUser User { get; set; }

        public string Retrieved { get; set; }
    }
}
