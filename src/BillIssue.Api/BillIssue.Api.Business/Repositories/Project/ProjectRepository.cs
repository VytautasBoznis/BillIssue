using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Repositories.Project;
using BillIssue.Api.Models.Models.Projects;
using Dapper;

namespace BillIssue.Api.Business.Repositories.Project
{
    public class ProjectRepository : IProjectRepository
    {
        public async Task<List<ProjectAndWorktypeModelFlattened>> GetAllProjectSelectionsForWorkspacesFlattened(List<Guid> WorkspaceIds, IUnitOfWork unitOfWork)
        {
            var dictionary = new Dictionary<string, object> { { "@cwi", WorkspaceIds } };

            IEnumerable<ProjectAndWorktypeModelFlattened> projectAndWorktypeModelsFlattened = await unitOfWork.Connection.QueryAsync<ProjectAndWorktypeModelFlattened>(@"
                SELECT
                    pp.workspace_id as WorkspaceId,
	                pp.id as ProjectId,
	                pp.name as ProjectName,
	                pw.id as WorktypeId,
	                pw.name as WorktypeName
                FROM project_projects pp
	                JOIN project_worktypes pw
		                ON pp.id = pw.project_id
                WHERE pp.workspace_id = ANY (@cwi)", dictionary);

            return projectAndWorktypeModelsFlattened.ToList();
        }
    }
}
