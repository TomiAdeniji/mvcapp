namespace Qbicles.Models.UserInformation
{
    public class WorkExperience : Experience
    {
        public WorkExperience()
        {
            this.Type = ExperienceType.WorkExperience;
        }
        public string Company { get; set; }

        public string Role { get; set; }
    }
}