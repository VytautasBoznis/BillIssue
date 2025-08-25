namespace BillIssue.Shared.Models.Response.Project.Dto
{
    public class ProjectDto
    {
        public Guid WorkspaceId { get; set; }
        public Guid ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public List<ProjectUserAssignmentDto> ProjectUserAssignments { get; set; }
        public List<ProjectWorktypeDto> ProjectWorktypes { get; set; }
    }
}
