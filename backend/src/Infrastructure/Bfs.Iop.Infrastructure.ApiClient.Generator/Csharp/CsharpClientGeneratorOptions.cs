namespace Bfs.Iop.Infrastructure.ApiClient.Generator.Csharp;

public sealed class CsharpClientGeneratorOptions : ClientGeneratorOptionsBase
{
    public string[] AdditionalNamespaceUsages { get; set; } = [];

    public string? ClientNamespace { get; set; }

    public string? ConfigurationClass { get; set; }

    public bool GenerateDtoTypes { get; set; }

    /// <summary>
    ///     Schema types the generator must not re-declare, because the solution already defines them
    ///     and the client references them through <see cref="AdditionalNamespaceUsages" />.
    ///     <para>
    ///         Without this a generated client gets its own copy of every contract type, and callers
    ///         need mapping code to cross between two declarations of the same thing - which for an
    ///         enum is worse than it sounds, since the copies are renumbered by declaration order.
    ///     </para>
    /// </summary>
    public string[] ExcludedTypeNames { get; set; } = [];

    public bool GenerateClientInterfaces { get; set; }

    public bool GenerateOptionalPropertiesAsNullable { get; set; } = true;

    public bool UseHttpRequestMessageCreationMethod { get; set; } = true;

    public bool GeneratePrepareRequestAndProcessResponseAsAsyncMethods { get; set; } = true;

    public bool UseBaseUrl { get; set; }
}
