using BillIssue.Domain.Response.Multilanguage.Dto;
using BillIssue.Shared.Models.Request.Base;

namespace BillIssue.Shared.Models.Request.Multilanguage
{
    public class ModifyMultilanguageItemRequest : AuthenticatedRequest
    {
        public MultilanguageItemDto MultilanguageItem { get; set; }
        public bool StopTransactionCommit { get; set; }
    }
}
