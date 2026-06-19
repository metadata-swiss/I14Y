namespace Bfs.Iop.Infrastructure.ApiClient.Generator.Csharp;

public sealed class CsharpClientGeneratorOptions : ClientGeneratorOptionsBase
{
    public string[] AdditionalNamespaceUsages { get; set; } = [];

    public string? ClientNamespace { get; set; }

    public string? ConfigurationClass { get; set; }

    public bool GenerateDtoTypes { get; set; }

    public bool GenerateClientInterfaces { get; set; }

    public bool GenerateOptionalPropertiesAsNullable { get; set; } = true;

    public bool UseHttpRequestMessageCreationMethod { get; set; } = true;

    public bool GeneratePrepareRequestAndProcessResponseAsAsyncMethods { get; set; } = true;

    public bool UseBaseUrl { get; set; }
}
