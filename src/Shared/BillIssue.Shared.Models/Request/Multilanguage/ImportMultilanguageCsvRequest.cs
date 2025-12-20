using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Multilanguage
{
    public class ImportMultilanguageCsvRequest : AuthenticatedRequest
    {
        public Stream FileStream { get; set; }
    }
}
