using BillIssue.Shared.Models.Constants;
using System.Net;

namespace BillIssue.Api.Models.Exceptions
{
    public class UnauthorizedException : BaseAppException
    {
        public UnauthorizedException(string message, string description = "", HttpStatusCode statusCode = HttpStatusCode.Unauthorized, string errorCode = ExceptionCodes.AUTH_UNOTHORIZED_EXCEPTION) : base(message, description, statusCode, errorCode)
        {
        }
    }
}
