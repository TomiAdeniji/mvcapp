namespace Qbicles.Models.UserInformation
{
    public class EducationExperience : Experience
    {
        public EducationExperience()
        {
            this.Type = ExperienceType.EducationExperience;
        }

        public string Institution { get; set; }

        public string Course { get; set; }
    }
}