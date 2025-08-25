using BillIssue.Data.Enums;

namespace BillIssue.Domain.Response.Multilanguage.Dto
{
    public class MultilanguageItemDto
    {
        public string MultilanguageIndexName { get; set; }
        public string MultilanguageTranslation { get; set; }
        public LanguageTypeEnum LanguageType { get; set; }
    }
}
