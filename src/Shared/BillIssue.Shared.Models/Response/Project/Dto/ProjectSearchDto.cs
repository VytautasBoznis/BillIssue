using BillIssue.Shared.Models.Enums.Project;

namespace BillIssue.Shared.Models.Response.Project.Dto
{
    public class ProjectSearchDto
    {
        public Guid WorkspaceId { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ProjectUserRoles UserRole { get; set; }
        public bool IsDeleted { get; set; }
    }
}
