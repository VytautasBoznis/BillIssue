using BillIssue.Api.Models.Exceptions;
using System.Net;

namespace BillIssue.Shared.Models.Errors
{
    public class UnknowNotificationTypeException : BaseAppException
    {
        public UnknowNotificationTypeException(string message, string description = "", HttpStatusCode statusCode = HttpStatusCode.InternalServerError, string errorCode = "") : base(message, description, statusCode, errorCode)
        {
        }
    }
}
