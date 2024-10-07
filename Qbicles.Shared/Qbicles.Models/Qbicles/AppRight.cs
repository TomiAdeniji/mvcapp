using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_appright")]
    public class AppRight
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual List<QbicleApplication> QbicleApplications { get; set; } = new List<QbicleApplication>();
    }

    public enum RightEnum
    {
        [Description("View Content")]
        ViewContent = 1,
        [Description("Edit Content")]
        EditContent = 2,
        [Description("Add Task to Qbicle")]
        AddTaskToQbicle = 3,
        [Description("Add Document to Qbicle")]
        AddDocumentToQbicle = 4
    };
}