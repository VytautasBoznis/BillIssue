using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Models.Models.Auth;

namespace BillIssue.Api.Interfaces.Repositories.Confirmations
{
    public interface IConfirmationRepository
    {
        Task<UserConfirmationModel?> GetConfirmationById(Guid confirmationId, IUnitOfWork unitOfWork);
        Task CreatePasswordReminderConfirmationAsync(Guid confirmationId, Guid userId, IUnitOfWork unitOfWork);
        Task DeleteConfirmationAsync(Guid confirmationId, IUnitOfWork unitOfWork);
    }
}
