using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Bfs.Iop.Admin.Api.Swagger;

internal class FileResultContentTypeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var requestAttribute = context.MethodInfo.GetCustomAttributes(typeof(FileResultContentTypesAttribute), false)
            .Cast<FileResultContentTypesAttribute>()
            .FirstOrDefault();

        if (requestAttribute == null) return;

        if (operation.Responses.ContainsKey("200"))
        {
            operation.Responses.Remove("200");
        }

        operation.Responses.Add("200", new OpenApiResponse
        {
            Description = "Success",
            Content = requestAttribute.ContentTypes.ToDictionary(
                x => x,
                x => new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    }
                })
        });
    }
}