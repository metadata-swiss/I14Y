using Bfs.Iop.Infrastructure.ApiClient.Generator.Extensions;
using NSwag;
using NSwag.CodeGeneration.OperationNameGenerators;
using System.Text;

namespace Bfs.Iop.Infrastructure.ApiClient.Generator.Csharp;

internal class CsharpOperationNameGenerator : IOperationNameGenerator
{
    public bool SupportsMultipleClients { get; set; }

    public string GetClientName(OpenApiDocument document, string path, string httpMethod, OpenApiOperation operation) 
        => GetClientName(path);

    public string GetOperationName(OpenApiDocument document, string path, string httpMethod, OpenApiOperation operation)
    {
        var builder = new StringBuilder();

        var formattedHttpMethod = $"{httpMethod.Substring(0, 1).ToUpper()}{httpMethod[1..]}";

        builder.Append(formattedHttpMethod);

        var strippedPath = path[4..];
        var skipCount = SupportsMultipleClients ? 1 : 0;
        // split path and omit first part (which is implied to be the controller name)
        var pathParts = strippedPath.Split('/').Skip(skipCount);

        foreach (var part in pathParts)
        {
            if (part.Contains('{'))
            {
                continue;
            }

            builder.Append(part.ToUpperCamelCase());
        }

        var index = 0;
        foreach (var param in operation.Parameters)
        {
            var prefix = index == 0 ? "By" : "And";
            builder.Append($"{prefix}{param.Name.Substring(0, 1).ToUpper()}{param.Name[1..]}");
            index++;
        }

        return builder.ToString();
    }

    private static string GetClientName(string path)
    {
        var strippedPath = path[4..];
        var pathParts = strippedPath.Split('/');
        var clientName = pathParts[0];

        return clientName;
    }
}
