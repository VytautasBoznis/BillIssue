using BillIssue.Api.Models.Models.Base;

namespace BillIssue.Api.Models.Models.Workspace
{
    public class WorkspaceModel: BaseModel
    {
        public Guid Id { get; set; }
        public Guid CreatorUserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
