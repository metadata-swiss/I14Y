using System.Reflection;

namespace Bfs.Iop.Partner.Api.Swagger;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class SwaggerInformation
{
    public readonly string AssemblyVersion;
    public readonly string Description;
    public readonly string ReleaseVersion;
    public readonly string Title;
    public readonly string Version = "v1";

    public SwaggerInformation(string environment, string releaseVersion)
    {
        AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1";
        ReleaseVersion = releaseVersion;

        Description = $"Deployment info: {ReleaseVersion}, Assembly: {AssemblyVersion}";
        Title = $"IOP Partner ({environment})";
    }
}