using BillIssue.Api.Models.Enums.Auth;

namespace BillIssue.Shared.Models.Authentication
{
    public class SessionUserData
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
