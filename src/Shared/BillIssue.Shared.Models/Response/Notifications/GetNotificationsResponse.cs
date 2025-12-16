using BillIssue.Shared.Models.Response.Base;
using BillIssue.Shared.Models.Response.Notifications.Dto;

namespace BillIssue.Shared.Models.Response.Notifications
{
    public class GetNotificationsResponse: BaseResponse
    {
        public List<NotificationDto> NotificationDtos { get; set; }
    }
}
