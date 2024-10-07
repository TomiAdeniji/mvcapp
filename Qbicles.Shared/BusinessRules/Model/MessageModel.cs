namespace Qbicles.BusinessRules.Model
{
    public class ErrorMessageModel
    {
        public ErrorMessageModel(string errorCode, object[] obj)
        {
            this.ErrorCode = errorCode;
            this.Params = obj;
        }
        public string ErrorCode { get; set; }
        public object[] Params { get; set; }
    }
}