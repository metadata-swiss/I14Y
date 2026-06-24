namespace Bfs.Iop.Core.Api.Swagger;

public class SwaggerInformation
{
    public readonly string AssemblyVersion;
    public readonly string Description;
    public readonly string ReleaseVersion;
    public readonly string Title;
    public readonly string Version = "v1";

    public SwaggerInformation(string environment, string releaseVersion)
    {
        var assemblyName = typeof(Startup).Assembly.GetName();

        AssemblyVersion = assemblyName.Version?.ToString() ?? "1";
        ReleaseVersion = releaseVersion;

        Description = $"Deployment info: {ReleaseVersion}, Assembly: {AssemblyVersion}";
        Title = $"IOP Core ({environment})";
    }
}