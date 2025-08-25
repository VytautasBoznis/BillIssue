using Newtonsoft.Json;

namespace BillIssue.Web.Business.Helpers
{
    public static class ApiResponseHelper
    {
        public static async Task<T> ConvertResponseMessageToContent<T>(HttpResponseMessage responseMessage)
        {
            if(responseMessage == null)
            {
                return default;
            }

            if(responseMessage.Content == null)
            {
                return default;
            }
            
            string responseText = await responseMessage.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(responseText);
        }
    }
}
