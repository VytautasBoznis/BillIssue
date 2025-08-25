namespace BillIssue.Web.Interfaces.Account
{
    public interface IAccountFacade
    {
        Task<string> Login(string email, string password);
        Task<string> Register(string email, string firstName, string lastName, string password);
        Task<bool> RemindPassword(string email);
    }
}
