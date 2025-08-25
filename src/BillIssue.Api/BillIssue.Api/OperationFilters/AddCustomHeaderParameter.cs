using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BillIssue.Api.OperationFilters
{
    public class AddCustomHeaderParameter : IOperationFilter
    {
        public void Apply(
            OpenApiOperation operation,
            OperationFilterContext context)
        {
            if (operation.Parameters is null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "AuthToken",
                In = ParameterLocation.Header,
                Description = "Auth token, needed for authorized actions",
            });
        }
    }
}
