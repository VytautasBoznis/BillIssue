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
        protected IUnitOfWorkFactory _unitOfWorkFactory;
        protected OperationFactory _operationFactory;

        private IValidator<TRequest> _validator;

        public BaseOperation(ILogger logger, IUnitOfWorkFactory unitOfWorkFactory, OperationFactory operationFactory, IValidator<TRequest> validator)
        {
            _logger = logger;
            _unitOfWorkFactory = unitOfWorkFactory;
            _operationFactory = operationFactory;
            _validator = validator;
        }

        public async Task<TResponse> Run(TRequest request, IUnitOfWork? unitOfWork = null)
        {
            ValidateRequest(request);

            _logger.LogInformation("Starting operation {OperationName}", this.GetType().Name);

            IUnitOfWork internalUnitOfWork = unitOfWork ?? await _unitOfWorkFactory.CreateAsync();
            var response = await Execute(request, internalUnitOfWork);
            
            _logger.LogInformation("Finished operation {OperationName}", this.GetType().Name);
            return response;
        }

        protected abstract Task<TResponse> Execute(TRequest request, IUnitOfWork unitOfWork);

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
