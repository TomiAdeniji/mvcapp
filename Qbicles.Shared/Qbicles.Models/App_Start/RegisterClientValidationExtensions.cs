using DataAnnotationsExtensions.ClientValidation;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Qbicles.Models.App_Start.RegisterClientValidationExtensions), "Start")]
 
namespace Qbicles.Models.App_Start {
    public static class RegisterClientValidationExtensions {
        public static void Start() {
            DataAnnotationsModelValidatorProviderExtensions.RegisterValidationExtensions();            
        }
    }
}