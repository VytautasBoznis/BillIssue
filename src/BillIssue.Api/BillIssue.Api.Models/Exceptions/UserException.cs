namespace BillIssue.Api.Models.Exceptions
{
    public class UserException : BaseAppException
    {
        public UserException(string message, string errorCode) : base(message, errorCode: errorCode)
        {
        }
    }
}
