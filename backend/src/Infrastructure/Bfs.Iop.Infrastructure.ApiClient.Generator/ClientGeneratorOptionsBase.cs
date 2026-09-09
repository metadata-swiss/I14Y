namespace Bfs.Iop.Infrastructure.ApiClient.Generator;

public abstract class ClientGeneratorOptionsBase
{
    public string ClassName { get; set; } = null!;

    public string ClientBaseClass { get; set; } = null!;

    public string OutputPath { get; set; } = null!;

    public bool StoreSwaggerJson { get; set; } = false;

    public string SwaggerJsonUrl { get; set; } = null!;
}
