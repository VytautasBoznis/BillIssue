namespace BillIssue.Api.Interfaces.Base
{
    public interface IUnitOfWorkFactory
    {
        Task<IUnitOfWork> CreateAsync();
    }
}
