using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Broadcast
{
    [Table("qb_connectedsignalrs")]
    public class UserConnectedSignalR
    {
        [Key]
        public string UserName { get; set; }
        public ICollection<Connection> Connections { get; set; }
    }
    [Table("qb_connections")]
    public class Connection
    {
        [Key]
        public string ConnectionID { get; set; }
        public string UserAgent { get; set; }
    }
}
