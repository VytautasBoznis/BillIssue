using BillIssue.Shared.Models.Authentication;

namespace BillIssue.Shared.Models.Request.Base
{
    public class AuthenticatedRequest: BaseRequest
    {
        public SessionUserData? SessionUserData { get; set; }
        public bool CreatedFromController { get; set; } = true;
    }
}
