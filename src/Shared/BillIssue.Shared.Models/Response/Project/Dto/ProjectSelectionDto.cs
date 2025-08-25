namespace BillIssue.Shared.Models.Response.Project.Dto
{
    public class ProjectSelectionDto
    {
        public Guid WorkspaceId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<ProjectWorktypeSelectionDto> Worktypes { get; set; }
    }
}
