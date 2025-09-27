using System.Net;

namespace BillIssue.Api.Models.Exceptions
{
    public class AlertException : BaseAppException
    {
        public AlertException(string message, string description = "", HttpStatusCode statusCode = HttpStatusCode.InternalServerError, string errorCode = "") : base(message, description, statusCode, errorCode)
        {
        }
    }
}
