using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Api.Models.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillIssue.Api.Models.Models.Auth
{
    public class UserConfirmationModel: BaseModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        [Column("confirmation_type")]
        public ConfirmationTypeEnum ConfirmationType { get; set; }
    }
}
