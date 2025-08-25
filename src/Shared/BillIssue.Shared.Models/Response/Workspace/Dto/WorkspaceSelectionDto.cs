using BillIssue.Shared.Models.Response.Project.Dto;

namespace BillIssue.Shared.Models.Response.Workspace.Dto
{
    public class WorkspaceSelectionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<ProjectSelectionDto> Projects { get; set; }
    }
}
