namespace BillIssue.Shared.Models.Response.Auth.Dto
{
    public class SessionDto
    {
        public Guid AuthToken { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
