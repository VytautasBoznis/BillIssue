namespace BillIssue.Api.Models.Exceptions
{
    public class WorkspaceException : BaseAppException
    {
        public WorkspaceException(string message, string errorCode) : base(message, errorCode: errorCode)
        {
        }
    }
}
