using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Qbicles.Models
{
    public partial class ViewDatesQbicleStream
    {
        [Key, Column(Order = 0)]
        public DateTime TimeLineDate { get; set; }
        [Key, Column(Order = 1)]
        public int QbicleId { get; set; }
        [Key, Column(Order = 2)]
        public int ActivityType { get; set; }
        [Key, Column(Order = 3)]
        public int TopicId { get; set; }
        [Key, Column(Order = 4)]
        public string APP { get; set; }
    }
    public partial class ViewQbicleActivitiesStream
    {
        [Key, Column(Order = 0)]
        public int Id { get; set; }
        [Key, Column(Order = 1)]
        public DateTime TimeLineDate { get; set; }
        [Key, Column(Order = 2)]
        public int QbicleId { get; set; }
        [Key, Column(Order = 3)]
        public int ActivityType { get; set; }
        [Key, Column(Order = 4)]
        public int TopicId { get; set; }
        [Key, Column(Order = 5)]
        public string APP { get; set; }
    }
    public partial class UnusedInventoryView
    {
        [Key, Column(Order = 0)]
        public int Id { get; set; }
        [Key, Column(Order = 1)]
        public decimal CurrentInventory { get; set; }
    }
    public partial class UnusedBatchesView
    {
        [Key, Column(Order = 0)]
        public int InventoryId { get; set; }
        [Key, Column(Order = 1)]
        public int BatchId { get; set; }        
    }

    public class ViewDatesQbicleStreamConfiguration : EntityTypeConfiguration<ViewDatesQbicleStream>
    {
        public ViewDatesQbicleStreamConfiguration()
        {
            HasKey(t => t.TimeLineDate);
            HasKey(t => t.QbicleId);
            HasKey(t => t.ActivityType);
            HasKey(t => t.TopicId);
            HasKey(t => t.APP);
            ToTable("[dbo.view_lstdates_qbicle_stream]");
        }
    }

    public class ViewQbicleActivitiesStreamConfiguration : EntityTypeConfiguration<ViewQbicleActivitiesStream>
    {
        public ViewQbicleActivitiesStreamConfiguration()
        {
            HasKey(t => t.Id);
            //HasKey(t => t.TimeLineDate);
            //HasKey(t => t.QbicleId);
            //HasKey(t => t.ActivityType);
            //HasKey(t => t.TopicId);
            //HasKey(t => t.APP);
            ToTable("[dbo.view_lstactivities_qbicle_stream]");
        }
    }
    public class UnusedInventoryViewConfiguration : EntityTypeConfiguration<UnusedInventoryView>
    {
        public UnusedInventoryViewConfiguration()
        {
            HasKey(t => t.Id);
            HasKey(t => t.CurrentInventory);
            ToTable("[dbo.view_unusedinventory]");
        }
    }
    public class UnusedBatchesViewConfiguration : EntityTypeConfiguration<UnusedBatchesView>
    {
        public UnusedBatchesViewConfiguration()
        {
            HasKey(t => t.InventoryId);
            HasKey(t => t.BatchId);
            ToTable("[dbo.view_unusedbatches]");
        }
    }
}
