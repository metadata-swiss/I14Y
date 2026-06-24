namespace Bfs.Iop.Infrastructure.ApiClient.Generator.TypeScript;

public sealed class TypeScriptClientGeneratorOptions : ClientGeneratorOptionsBase
{
    public string BaseUrlTokenName { get; set; } = "API_BASE_URL";

    /// <summary>
    /// Optional prefix prepended to module class name.
    /// </summary>
    public string? ClientModuleClassPrefix { get; set; }

    public string? ConfigurationClass { get; set; }

    /// <summary>
    /// The content of this file will be prepended to the generated client file.
    /// </summary>
    public string? ExtensionPath { get; set; }

    /// <summary>
    /// Enables client modules generation.
    /// </summary>
    /// <remarks>
    /// This flag enables generic module generation for each client generated. This value is overwritten by
    /// <see cref="ProvideInRoot"/> which makes modules concept obsolete since the clients are instantiated in root.
    /// </remarks>
    public bool GenerateClientModule { get; set; }

    /// <summary>
    /// Gets or sets the class/file name of the clients module file.
    /// </summary>
    /// <remarks>
    /// Optional value to specify class/file name of the clients module file. If not defined default value
    /// {<see cref="ClientGeneratorOptionsBase.ClassName"/>.module} is used.
    /// </remarks>
    public string? ModuleClassName { get; set; }

    /// <summary>
    /// The content of this file will be prepended to the generated client modules file.
    /// </summary>
    public string? ModuleExtensionPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use the Angular 6 Singleton Provider
    /// </summary>
    public bool ProvideInRoot { get; set; } = true;

    /// <summary>
    /// Gets or sets the name of the clients support module file.
    /// </summary>
    /// <remarks>
    /// Optional value to specify module class containing common clients support. If not defined default value
    /// {<see cref="ConfigurationClass"/>Module} is used.
    /// </remarks>
    public string? SupportModuleName { get; set; }

    public bool WrapResponses { get; set; }

    /// <summary>
    /// Gets the RxJs version (Angular template only, default: 6.0).
    /// </summary>
    public decimal RxJsVersion { get; set; } = 6.0m;
}
