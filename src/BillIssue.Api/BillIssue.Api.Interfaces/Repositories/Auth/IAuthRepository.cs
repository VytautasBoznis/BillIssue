using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Models.Models.Auth;

namespace BillIssue.Api.Interfaces.Repositories.Auth
{
    public interface IAuthRepository
    {
        Task<SessionModel?> GetSessionModelByEmail(string email, IUnitOfWork unitOfWork);
        Task UpdatePassword(Guid userId, string newPasswordHash, string modifiedBy, IUnitOfWork unitOfWork);
        Task CreateUserAccount(Guid userId, string passwordHash, string email, string firstName, string lastName, IUnitOfWork unitOfWork);
    }
}
