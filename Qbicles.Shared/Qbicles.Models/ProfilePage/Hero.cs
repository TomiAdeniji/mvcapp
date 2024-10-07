namespace Qbicles.Models.ProfilePage
{
    public class Hero : Block
    {
        public Hero()
        {
            this.Type = BusinessPageBlockType.Hero;
        }

        public HeroBackgroundType HeroBackgroundType { get; set; }

        public string HeroBackgroundColour { get; set; }

        public string HeroGradientColour1 { get; set; }

        public string HeroGradientColour2 { get; set; }

        /// <summary>
        /// Usual S3 URI/GUID
        /// </summary>
        public string HeroBackGroundImage { get; set; }

        public string HeroHeadingText { get; set; }
        public decimal HeroHeadingFontSize { get; set; }
        public string HeroHeadingColour { get; set; }
        public string HeroHeadingAccentColour { get; set; }

        public string HeroSubHeadingText { get; set; }
        public decimal HeroSubHeadingFontSize { get; set; }
        public string HeroSubHeadingColour { get; set; }

        public string HeroFeaturedImage { get; set; }
        public string HeroLogo { get; set; }
    }

    public enum HeroBackgroundType
    {
        SingleColour = 1,
        Gradient = 2,
        Image = 3
    }
}
