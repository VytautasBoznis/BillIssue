namespace BillIssue.Api.Models.Exceptions
{
    public class LoginException : BaseAppException
    {
        public LoginException(string message, string errorCode) : base(message, errorCode: errorCode)
        {
        }
    }
}
