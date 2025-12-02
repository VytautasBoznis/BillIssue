using BillIssue.Shared.Models.Response.Notifications.Dto;

namespace BillIssue.Shared.Models.Response.Auth.Dto
{
    public class SessionDto
    {
        // legacy session id kept for backward compatibility
        public Guid AuthToken { get; set; }

        // new JWT issued on successful authentication
        public string JwtToken { get; set; }

        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<NotificationDto> Notifications { get; set; }
    }
}
