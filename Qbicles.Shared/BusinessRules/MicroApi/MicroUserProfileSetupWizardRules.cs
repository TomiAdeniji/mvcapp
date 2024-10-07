using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Micro.Extensions;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System.Linq;

namespace Qbicles.BusinessRules.MicroApi
{
    public class MicroUserProfileSetupWizardRules : MicroRulesBase
    {
        public MicroUserProfileSetupWizardRules(MicroContext microContext) : base(microContext)
        {
        }

        public object GetMicroFirstLaunchedUser()
        {
            var user = CurrentUser;
            var currentstep = MicroUserWizardStep.Basics;
            switch (user.WizardStep)
            {
                case UserWizardStep.GeneralSettingsStep:
                    currentstep = MicroUserWizardStep.Basics;
                    break;
                case UserWizardStep.AddressAndPhoneSettingStep:
                    currentstep = MicroUserWizardStep.Contact;
                    break;
                case UserWizardStep.ShowcaseSettingStep:
                    currentstep = MicroUserWizardStep.Showcase;
                    break;
                case UserWizardStep.Settings:
                case UserWizardStep.InterestSettingsStep:
                case UserWizardStep.BusinessesConnectStep:
                default:
                    currentstep = MicroUserWizardStep.Basics;
                    break;
            }
            var userWizard = new UserProfileWizard
            {
                Avatar = user.ProfilePic.ToUri(),
                BrieflyDescribeYourself = user.Profile,
                DisplayName = user.DisplayUserName,
                Id = user.Id,
                Phone = user.Tell,
                TagLine = user.TagLine,
                CurrentStep = currentstep,
                CurrentStepId = currentstep.GetId()
            };

            switch (user.WizardStep)
            {
                case UserWizardStep.AddressAndPhoneSettingStep:
                case UserWizardStep.ShowcaseSettingStep:
                    userWizard.Contacts = user.TraderAddresses.ToUserAddress();
                    userWizard.Showcases = dbContext.Showcases.Where(e => e.AssociatedUser.Id == user.Id).ToList().ToUserShowcase();
                    break;
            }

            return userWizard;
        }

        public ReturnJsonModel FinisStep(MicroUserWizardStep step)
        {
            return new UserRules(dbContext).UpdateUserWizardStep(CurrentUser.Id, (UserWizardStep)step.GetId(), UserWizardPlatformType.MicroApp);
        }

        public bool FinisUserProfileWizard()
        {
            var userId = CurrentUser.Id;
            var user = dbContext.QbicleUser.FirstOrDefault(e => e.Id == userId);

            if (user.WizardStep == UserWizardStep.GeneralSettingsStep)
                return false;

            user.IsUserProfileWizardRunMicro = true;
            dbContext.SaveChanges();
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valId">
        /// 1 - skip
        /// 2 - complete</param>
        /// <returns></returns>
        public bool FinisUserImportWizard(int valId)
        {
            var userId = CurrentUser.Id;
            var user = dbContext.QbicleUser.FirstOrDefault(e => e.Id == userId);

            user.IsImportWizardMicro = valId;
            dbContext.SaveChanges();
            return true;
        }
    }
}
