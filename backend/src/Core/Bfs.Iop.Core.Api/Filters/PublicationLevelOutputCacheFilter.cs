using Bfs.Iop.DataAccess.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OutputCaching;

namespace Bfs.Iop.Core.Api.Filters;

/// <summary>
/// Prevents non-public publishable entities from ever being written to the output cache
/// </summary>
public sealed class PublicationLevelOutputCacheFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.Features.Get<IOutputCacheFeature>() is { } outputCacheFeature)
        {
            if (context.Result is ObjectResult { Value: IPublishableEntityModel model }
               && model.PublicationLevel == PublicationLevel.Public)
            {

                outputCacheFeature.Context.AllowCacheStorage = true;
            }
            else
            {
                outputCacheFeature.Context.AllowCacheStorage = false;
            }
        }
    }    
}
