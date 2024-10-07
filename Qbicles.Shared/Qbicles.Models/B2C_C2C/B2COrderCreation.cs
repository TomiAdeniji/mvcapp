namespace Qbicles.Models.B2C_C2C
{
    public class B2COrderCreation : QbicleDiscussion
    {
        public B2COrderCreation()
        {
            this.DiscussionType = DiscussionTypeEnum.B2COrder;
            this.Interaction = OrderDiscussionInteraction.Noninteraction;
        }

        public virtual TradeOrder TradeOrder { get; set; }

        public OrderDiscussionInteraction Interaction { get; set; }

    }
    public enum OrderDiscussionInteraction
    {
        Interaction = 0,
        Noninteraction = 1
    }
}
