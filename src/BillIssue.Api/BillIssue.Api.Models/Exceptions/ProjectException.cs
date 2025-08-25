namespace BillIssue.Api.Models.Exceptions
{
    public class ProjectException : BaseAppException
    {
        public ProjectException(string message, string errorCode) : base(message, errorCode: errorCode)
        {
        }
    }
}