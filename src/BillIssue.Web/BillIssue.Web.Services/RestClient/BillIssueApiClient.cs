using BillIssue.Data.Enums;
using BillIssue.Shared.Models.Request.Auth;
using BillIssue.Shared.Models.Response.Auth;
using BillIssue.Shared.Models.Response.Multilanguage;
using BillIssue.Web.Business.Helpers;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace BillIssue.Web.Business.RestClient
{
    public class BillIssueApiClient(HttpClient client)
    {
        private const string ApplicationJsonMediaType = "application/json";

        //TODO: add some generic error handling

        #region Authorization
        public async Task<LoginResponse> Login(LoginRequest request)
        {
            string requestAsString =  JsonConvert.SerializeObject(request);
            HttpResponseMessage response = await client.PostAsync("/api/Auth/login", new StringContent(requestAsString, Encoding.UTF8, ApplicationJsonMediaType));

            LoginResponse loginResponse = await ApiResponseHelper.ConvertResponseMessageToContent<LoginResponse>(response);

            if (loginResponse == null)
            {
                throw new Exception("Failed to get Login response");
            }

            return loginResponse;
        }

        public async Task<LoginResponse> Register(RegisterRequest request)
        {
            string requestAsString = JsonConvert.SerializeObject(request);
            HttpResponseMessage response = await client.PostAsync("/api/Auth/register", new StringContent(requestAsString, Encoding.UTF8, ApplicationJsonMediaType));

            LoginResponse loginResponse = await ApiResponseHelper.ConvertResponseMessageToContent<LoginResponse>(response);

            if (loginResponse == null)
            {
                throw new Exception("Failed to get register response");
            }

            return loginResponse;
        }

        public async Task RemindPassword(RemindPasswordRequest request)
        {
            string requestAsString = JsonConvert.SerializeObject(request);
            HttpResponseMessage response = await client.PostAsync("/api/Auth/RemindPassword", new StringContent(requestAsString, Encoding.UTF8, ApplicationJsonMediaType));

            if (response == null)
            {
                throw new Exception("Failed to start password reminder process");
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Failed to start password reminder process");
            }
        }

        #endregion

        #region Multilanguage

        public async Task<GetMultilanguageDictionyResponse> GetMultilanguageDictionary(LanguageTypeEnum languageType)
        {
            GetMultilanguageDictionyResponse response = await client.GetFromJsonAsync<GetMultilanguageDictionyResponse>($"/api/Multilanguage/GetLanguageDictionary/{languageType}");

            if (response == null)
            {
                throw new Exception("Failed to get Multilanguage dictionary response");
            }

            return response;
        }

        public async Task<GetMultilanguageDictionyResponse> GetAllMultilanguageItems()
        {
            GetMultilanguageDictionyResponse response = await client.GetFromJsonAsync<GetMultilanguageDictionyResponse>($"/api/Multilanguage/GetAllDictionary");

            if (response == null)
            {
                throw new Exception("Failed to get Multilanguage item response");
            }

            return response;
        }

        #endregion

    }
}
