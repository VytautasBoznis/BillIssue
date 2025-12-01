namespace BillIssue.Api.Models.ConfigurationOptions
{
    public class JwtOptions
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SecretKey { get; set; }
        public int ExpiryInMinutes { get; set; }
    }
}
