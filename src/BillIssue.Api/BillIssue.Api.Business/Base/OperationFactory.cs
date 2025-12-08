namespace BillIssue.Api.Business.Base
{
    public class OperationFactory
    {
        private readonly IServiceProvider _provider;

        public OperationFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public T Get<T>(Type operationType) where T : class
        {
            if (_provider.GetService(operationType) is not T svc)
            {
                throw new InvalidOperationException($"No registered operation for {operationType.FullName}");
            }

            return svc;
        }
    }
}
