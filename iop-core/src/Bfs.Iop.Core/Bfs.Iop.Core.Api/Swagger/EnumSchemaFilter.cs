using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;

namespace Bfs.Iop.Core.Api.Swagger;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            var enumType = context.Type;
            schema.Enum.Clear();
            Enum.GetNames(enumType).ToList().ForEach(name => schema.Enum.Add(new OpenApiString(name)));
        }
    }
}