using BillIssue.Api.Models.Enums.Auth;

namespace BillIssue.Shared.Models.Response.User.Dto
{
    public class SessionUserDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserRole Role { get; set; }
    }
}
