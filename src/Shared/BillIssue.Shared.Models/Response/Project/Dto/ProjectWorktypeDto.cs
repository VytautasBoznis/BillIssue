namespace BillIssue.Shared.Models.Response.Project.Dto
{
    public class ProjectWorktypeDto
    {
        public Guid ProjectId { get; set; }
        public Guid ProjectWorktypeId { get; set; }
        public string ProjectName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsBillable { get; set; }
        public bool IsDeleted { get; set; }
    }
}
