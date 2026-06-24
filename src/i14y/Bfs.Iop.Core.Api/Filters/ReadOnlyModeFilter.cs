using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bfs.Iop.Core.Api.Filters;

public class ReadOnlyModeFilter : IActionFilter
{
    private const string GetHttpMethod = "GET";

    public void OnActionExecuted(ActionExecutedContext context)
    { }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Request.Method != GetHttpMethod)
        {
            context.Result = new ObjectResult(new ProblemDetails()
            {
                Detail = "Application is in Read-only mode.",
                Title = "Forbidden",
                Status = 403,
                Type = "https://httpstatuses.com/403"
            });
        }
    }
}
