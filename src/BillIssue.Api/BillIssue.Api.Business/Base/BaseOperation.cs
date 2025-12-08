using BillIssue.Api.Interfaces.Base;
using BillIssue.Shared.Models.Request.Base;
using BillIssue.Shared.Models.Response.Base;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace BillIssue.Api.Business.Base
{
    public abstract class BaseOperation<TRequest, TResponse> where TRequest : BaseRequest where TResponse: BaseResponse
    {
        protected ILogger _logger;
        private IValidator<TRequest> _validator;

        public BaseOperation(ILogger logger, IValidator<TRequest> validator)
        {
            _logger = logger;
            _validator = validator;
        }

        public TResponse Run(TRequest request)
        {
            ValidateRequest(request);

            _logger.LogInformation("Starting operation {OperationName}", this.GetType().Name);
            var response = Execute(request);
            _logger.LogInformation("Finished operation {OperationName}", this.GetType().Name);
            return response;
        }

        protected abstract TResponse Execute(TRequest request);

        protected bool ValidateRequest(TRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");
            }

            ValidationResult result = _validator.Validate(request);

            // TODO: change to new exception type, add all bad request details
            return result.IsValid ? true : throw new Exception("Invalid request");
        }
    }
}
