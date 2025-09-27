namespace BillIssue.Shared.Models.Response.Workspace.Dto
{
    public class WorkspaceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public List<WorkspaceUserDto> WorkspaceUsers { get; set; }
    }
}
