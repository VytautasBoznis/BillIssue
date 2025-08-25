namespace BillIssue.Api.Models.Exceptions
{
    public class RegistrationException : BaseAppException
    {
        public RegistrationException(string description, string erroCode) : base("Failed to register", description, errorCode: erroCode)
        {
        }
    }
}
