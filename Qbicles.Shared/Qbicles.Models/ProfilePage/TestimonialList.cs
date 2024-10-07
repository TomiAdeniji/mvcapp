using System.Collections.Generic;

namespace Qbicles.Models.ProfilePage
{
    public class TestimonialList : Block
    {
        public TestimonialList()
        {
            this.Type = BusinessPageBlockType.Testimonial;
        }
        public virtual List<TestimonialItem> TestimonialItems { get; set; } = new List<TestimonialItem>();
    }
}
