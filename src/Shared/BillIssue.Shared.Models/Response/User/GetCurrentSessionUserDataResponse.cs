using BillIssue.Shared.Models.Response.Base;
using BillIssue.Shared.Models.Response.User.Dto;

namespace BillIssue.Shared.Models.Response.User
{
    public class GetCurrentSessionUserDataResponse: BaseResponse
    {
        public SessionUserDto SessionUserDto { get; set; }
    }
}
