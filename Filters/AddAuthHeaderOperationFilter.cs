namespace MyProject.Filters;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class AddAuthHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Security == null)
            operation.Security = new List<OpenApiSecurityRequirement>();

        var scheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        };

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [scheme] = new List<string>()
        });

        var authHeaderParameter = operation.Parameters?.FirstOrDefault(p => p.Name == "Authorization");
        if (authHeaderParameter != null)
        {
            authHeaderParameter.Description = "JWT token (e.g., enter only 'your-token' without 'Bearer ').";
        }
    }
}
