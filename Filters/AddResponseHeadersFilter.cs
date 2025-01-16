using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;

namespace MyProject.Filters;

public class AddResponseHeadersFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Add 401 Unauthorized response globally
        operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });

        // Add 400 Bad Request response globally
        operation.Responses.Add("400", new OpenApiResponse { Description = "Bad Request" });
    }
}
