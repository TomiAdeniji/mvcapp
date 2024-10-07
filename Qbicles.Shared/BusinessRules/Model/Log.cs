using Qbicles.Models;
using System;

namespace Qbicles.BusinessRules.Model
{
    public class TempLogModel
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string SessionId { get; set; }
        public int DomainId { get; set; }
        public int QbicleId { get; set; }
        public string UserId { get; set; }
        public string IPAddress { get; set; }
        public AppType AppType { get; set; }
        public QbicleLogType Action { get; set; }
    }
    public class LogModel
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string StrCreatedDate { get; set; }
        public string SessionID { get; set; }
        public string Domain { get; set; }
        public string Qbicle { get; set; }
        public string User { get; set; }
        public string IPAddress { get; set; }
        public string App { get; set; }
        public string Action { get; set; }
    }
}