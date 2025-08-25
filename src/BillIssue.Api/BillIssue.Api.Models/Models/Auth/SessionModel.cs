using BillIssue.Api.Models.Enums.Auth;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillIssue.Api.Models.Models.Auth
{
    public class SessionModel
    {
        public Guid Id { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }

        [Column("is_banned")]
        public bool IsBanned { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
