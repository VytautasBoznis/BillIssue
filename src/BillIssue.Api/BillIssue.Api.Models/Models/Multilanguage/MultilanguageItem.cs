using BillIssue.Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillIssue.Api.Models.Models.Multilanguage
{
    public class MultilanguageItem: BaseModel
    {
        public Guid MultilanguageItemId { get; set; }
        public Guid MultilanguageIndexId { get; set; }
        public LanguageTypeEnum LanguageTypeId { get; set; }
        public string Text { get; set; }
    }
}
