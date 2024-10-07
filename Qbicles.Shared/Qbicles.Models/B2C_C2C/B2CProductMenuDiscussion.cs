using Qbicles.Models.Catalogs;

namespace Qbicles.Models.B2C_C2C
{
    public class B2CProductMenuDiscussion: QbicleDiscussion
    {


        public B2CProductMenuDiscussion()
        {
            this.DiscussionType = DiscussionTypeEnum.B2CProductMenu;
        }
        public virtual Catalog ProductMenu { get; set; }
    }
}
