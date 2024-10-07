using Qbicles.Models.SalesMkt;
using System;
using System.ComponentModel.DataAnnotations.Schema;


namespace Qbicles.Models
{
    /// <summary>
    /// Concrete class for a Qbicle event
    /// </summary>

    [Table("qb_qbicleevent")]
    public class QbicleEvent : QbicleActivity
    {
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Location { get; set; }
        public EventTypeEnum EventType { get; set; }
        public EventDurationUnitEnum DurationUnit { get; set; }
        public short Duration { get; set; }
        public virtual PipelineContact PipelineContact { get; set; }
        public enum EventTypeEnum
        {
            Holiday,
            Seminar,
            Conference,
            Team
        }
        public enum EventDurationUnitEnum
        {
            Hours = 0,
            Days = 1,
            Weeks = 2
        }
        [Column(TypeName = "bit")]
        public bool isRecurs { get; set; }
        public QbicleEvent()
        {
            this.ActivityType = ActivityTypeEnum.EventActivity;
            this.App = ActivityApp.Qbicles;
        }
    }
}
