namespace BillIssue.Shared.Models.Enums.Workspace
{
    public enum WorkspaceUserRole : int
    {
        Contributor = 1,
        Manager = 2, //Minimal role to add a user to company workspace (all upper ones have that permission too)
        Administrator = 3,
        Owner = 4,
    }
}
