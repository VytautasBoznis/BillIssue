namespace BillIssue.Api.Models.Constants
{
    public static class AppSettingKeys
    {
        public const string DatabaseConnectionKey = "Database:ConnectionString";

        public const string RedisHostKey = "Redis:Host";
        public const string RedisPortKey = "Redis:Port";
        public const string RedisSslEnabledKey = "Redis:SslEnabled";

        public const string SendgridApiKeyKey = "Sendgrid:ApiKey";
        public const string SendgridFromEmailKey = "Sendgrid:FromEmail";
        public const string SendgridFromEmailDisplayNameKey = "Sendgrid:FromEmailDisplayName";
        public const string SendgridPasswordReminderLinkTemplateKey = "Sendgrid:PasswordReminderLinkTemplate";
    }
}
