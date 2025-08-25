using BillIssue.Shared.Models.Constants;

namespace BillIssue.Api.Models.Exceptions
{
    public class SessionExpiredException : BaseAppException
    {
        public SessionExpiredException(string message) : base(message, errorCode: ExceptionCodes.AUTH_SESSION_EXPIRED)
        {
        }
    }
}
