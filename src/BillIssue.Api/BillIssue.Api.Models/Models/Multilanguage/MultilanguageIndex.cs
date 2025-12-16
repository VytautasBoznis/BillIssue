using BillIssue.Api.Models.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillIssue.Api.Models.Models.Multilanguage
{
    public class MultilanguageIndex: BaseModel
    {
        public Guid MultilanguageIndexId { get; set; }
        public string MultilanguageIndexName { get; set; }
        public string DescriptionMltId { get; set; }
    }
}
