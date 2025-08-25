namespace BillIssue.Api.Models.Models.Projects
{
    public class ProjectAndWorktypeModelFlattened
    {
        public Guid WorkspaceId { get; set; }
        public Guid ProjectId { get; set; }
	    public string ProjectName { get; set; }
	    public Guid WorktypeId { get; set; }
	    public string WorktypeName { get; set; }
    }
}
