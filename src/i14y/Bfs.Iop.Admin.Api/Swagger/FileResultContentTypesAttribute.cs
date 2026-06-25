using System;

namespace Bfs.Iop.Admin.Api.Swagger;

/// <summary>
/// Indicates swashbuckle should expose the result of the method as a file in open api (see https://swagger.io/docs/specification/describing-responses/)
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
internal class FileResultContentTypesAttribute : Attribute
{
    public FileResultContentTypesAttribute(params string[] contentTypes)
    {
        ContentTypes = contentTypes;
    }

    /// <summary>
    /// Possible content types of the file
    /// </summary>
    public string[] ContentTypes { get; }
}