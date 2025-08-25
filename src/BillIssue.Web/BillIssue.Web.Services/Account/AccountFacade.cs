using BillIssue.Shared.Models.Request.Auth;
using BillIssue.Shared.Models.Response.Auth;
using BillIssue.Web.Business.RestClient;
using BillIssue.Web.Interfaces.Account;

namespace BillIssue.Web.Business.Account
{
    public class AccountFacade : IAccountFacade
    {
        private readonly BillIssueApiClient _client;
        
        public AccountFacade(BillIssueApiClient client)
        {
            _client = client;
        }
        
        public async Task<string> Login(string email, string password)
        {
            LoginRequest loginRequest = new LoginRequest
            {
                Email = email,
                Password = password
            };

            LoginResponse response = await _client.Login(loginRequest);

            return response.Session.AuthToken.ToString();
        }

        public async Task<string> Register(string email, string firstName, string lastName, string password)
        {
            RegisterRequest registerRequest = new RegisterRequest
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Password = password
            };

            LoginResponse response = await _client.Register(registerRequest);

            return response.Session.AuthToken.ToString();
        }

        public async Task<bool> RemindPassword(string email)
        {
            RemindPasswordRequest remindPasswordRequest = new RemindPasswordRequest
            {
                Email = email,
            };

            await _client.RemindPassword(remindPasswordRequest);

            return true;
        }
    }
}
