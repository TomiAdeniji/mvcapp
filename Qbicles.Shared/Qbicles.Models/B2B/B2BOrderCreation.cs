namespace Qbicles.Models.B2B
{
    public class B2BOrderCreation: QbicleDiscussion
    {
        public B2BOrderCreation()
        {
            this.DiscussionType = DiscussionTypeEnum.B2BOrder;
        }

        public virtual TradeOrderB2B TradeOrder { get; set; }
    }
}
