using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_qbiclelog")]
    public class QbicleLog
    {
        public QbicleLog(QbicleLogType logType, string userId)
        {
            LogType = logType;
            UserId = userId;
            CreatedDate = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public QbicleLogType LogType { get; set; }

        public string SessionId { get; set; }

        public string IPAddress { get; set; }

        /// <summary>
        /// This is the date and time on which this log was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }
        
    }
    public enum QbicleLogType
    {
        Login = 1,
        Logout = 2,
        PasswordReset = 3,
        DomainAccess = 4,
        QbicleAccess = 5,
        MyDeskAccess = 6,
        AppAccess = 7
    };
}