namespace BillIssue.Api.Models.ConfigurationOptions
{
    public class RedisOptions
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public bool SslEnabled { get; set; }
    }
}
