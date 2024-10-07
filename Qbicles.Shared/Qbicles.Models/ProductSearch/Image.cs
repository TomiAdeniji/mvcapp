namespace Qbicles.Models.ProductSearch
{
    public class FeaturedProductImage : FeaturedProduct
    {
        /// <summary>
        /// This is the GUID to associate with the image file uploaded to S3
        /// This is a required field for the Image 
        /// BUT, it cannot hav ethe [Required] attribute because the Featrured base class is also inhertited from by the FeaturedProduct Class
        /// </summary>

        public string FeaturedImageUri { get; set; }

        /// <summary>
        /// This is a URL that might' ba associated with the image
        /// </summary>
        public string URL { get; set; }
    }
}