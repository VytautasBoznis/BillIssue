using BillIssue.Web.Interfaces.Account;
using Microsoft.JSInterop;

namespace BillIssue.Web.Services
{
    public class AccountService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IAccountFacade _accountFacade;

        private const string AuthTokenSessionKey = "AuthToken";

        public AccountService(IJSRuntime jsRuntime, IAccountFacade accountFacade)
        {
            _jsRuntime = jsRuntime;
            _accountFacade = accountFacade;
        }

        public async Task<bool> Login(string email, string password)
        {
            string authToken = "";

            try
            {
                authToken = await _accountFacade.Login(email, password);
                await SaveAuthToken(authToken);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> Register(string email, string firstName, string lastName, string password)
        {
            string authToken = "";

            try
            {
                authToken = await _accountFacade.Register(email, firstName, lastName, password);
                await SaveAuthToken(authToken);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> RemindPassword(string email)
        {
            try
            {
                return await _accountFacade.RemindPassword(email);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> IsLoggedIn()
        {
            return await GetAuthToken() != null;
        }

        public async Task Logout()
        {
            await RemoveAuthToken();
        }

        private async Task<string> GetAuthToken()
        {
            string authToken = await _jsRuntime.InvokeAsync<string>("interop.getLocalStorageItem", AuthTokenSessionKey);

            if (authToken == "null")
            {
                return null;
            }

            return authToken;
        }

        private async Task SaveAuthToken(string authToken)
        {
            await _jsRuntime.InvokeAsync<string>("interop.setLocalStorageItem", [AuthTokenSessionKey, authToken]);
        }

        private async Task RemoveAuthToken()
        {
            await _jsRuntime.InvokeAsync<string>("interop.removeLocalStorageItem", [AuthTokenSessionKey]);
        }
    }
}
