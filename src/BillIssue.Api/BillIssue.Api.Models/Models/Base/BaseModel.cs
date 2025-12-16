using System.ComponentModel.DataAnnotations.Schema;

namespace BillIssue.Api.Models.Models.Base
{
    public class BaseModel
    {
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
    }
}
