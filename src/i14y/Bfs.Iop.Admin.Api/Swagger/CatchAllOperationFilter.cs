using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bfs.Iop.Admin.Api.Swagger;

internal class CatchAllOperationFilter : IOperationFilter
{
    private const string _captureName = "routeParameter";

    private static readonly Regex _regex = new($"{{\\*(?<{_captureName}>\\w+)}}");

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var httpMethodAttributes = context.MethodInfo.GetCustomAttributes(typeof(HttpMethodAttribute), false).Cast<HttpMethodAttribute>();
        var httpRouteAttributes = context.MethodInfo.GetCustomAttributes(typeof(RouteAttribute), false).Cast<RouteAttribute>();

        var pathTemplate = httpMethodAttributes?.FirstOrDefault(a => a.Template?.Contains('*') ?? false)?.Template ?? httpRouteAttributes.FirstOrDefault(a => a.Template?.Contains('*') ?? false)?.Template;

        if (string.IsNullOrEmpty(pathTemplate)) return;

        var matches = _regex.Matches(pathTemplate);

        foreach (Match match in matches)
        {
            string name = match.Groups[_captureName].Value;

            var parameter = operation.Parameters.FirstOrDefault(p => p.In == ParameterLocation.Path && p.Name == name);
            if (parameter != null)
            {
                parameter.AllowEmptyValue = true;
                parameter.Required = false;
                parameter.Schema.Nullable = true;
            }
        }
    }
}