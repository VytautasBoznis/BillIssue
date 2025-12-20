namespace BillIssue.Api.Models.Constants
{
    public static class MultilanguageConstants
    {
        public const string CSVSeparator = ";";
        public const string CSVFileSeparatorHeader = $"sep={CSVSeparator}";
        public const string CSVHeader = "MultilanguageIndexName;MultilanguageTranslatedName;LanguageTypeId";
        public const string CSVFilename = "AllMultilanguageItems.csv";
    }
}
