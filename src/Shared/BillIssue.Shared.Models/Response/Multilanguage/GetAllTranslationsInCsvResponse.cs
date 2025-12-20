using BillIssue.Shared.Models.Response.Base;

namespace BillIssue.Shared.Models.Response.Multilanguage
{
    public class GetAllTranslationsInCsvResponse : BaseResponse
    {
        public byte[] CsvContentBytes { get; set; }
    }
}
