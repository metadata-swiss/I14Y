using System;

namespace Bfs.Iop.Admin.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SwaggerVersionAttribute : Attribute
{
    public SwaggerVersionAttribute(string version)
    {
        Version = version;
    }

    public string Version { get; }
}