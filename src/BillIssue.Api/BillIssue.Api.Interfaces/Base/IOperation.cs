using BillIssue.Shared.Models.Request.Base;
using BillIssue.Shared.Models.Response.Base;

namespace BillIssue.Api.Interfaces.Base
{
    public interface IOperation<TRequest, TResponse> where TRequest: BaseRequest where TResponse: BaseResponse
    {
        abstract TResponse Execute(TRequest request);
    }
}
