using static Qbicles.Models.QbicleTask;

namespace Qbicles.Models.Broadcast
{
    public class BroadcastTask
    {
        public int TaskId { get; set; }
        public string TaskClass { get; set; }
        public string urlImage { get; set; }
        public string Username { get; set; }
        public string Timeline { get; set; }
        public string Description { get; set; }
        public TaskPriorityEnum Priority { get; set; }
        public TaskRepeatEnum TaskRepeat { get; set; }
        public int TaskAssignMe { get; set; }
    }
}
