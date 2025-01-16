using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MyProject.Filters;

public abstract class GlobalProducesResponseTypeFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();

        context.HttpContext.Response.StatusCode = resultContext.Result switch
        {
            BadRequestResult => StatusCodes.Status400BadRequest,
            UnauthorizedResult => StatusCodes.Status401Unauthorized,
            _ => context.HttpContext.Response.StatusCode
        };
    }
}
