using Bfs.Iop.Infrastructure.ApiClient.Generator.Csharp;
using Bfs.Iop.Infrastructure.ApiClient.Generator.Extensions;
using Bfs.Iop.Infrastructure.ApiClient.Generator.TypeScript;
using Microsoft.Extensions.Hosting;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.TypeScript;

namespace Bfs.Iop.Infrastructure.ApiClient.Generator;

public static class StaticClientGenerator<TStartup> where TStartup : class
{
    /// <summary>
    /// Generate a C# API Client.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="hostBuilderFactory"></param>
    /// <returns></returns>
    public static async Task GenerateCSharpClient(CsharpClientGeneratorOptions options, Func<IHostBuilder> hostBuilderFactory)
    {
        using var webApplicationFactory = new GeneratorWebApplicationFactory<TStartup>(hostBuilderFactory);
        Console.WriteLine("Web application factory created.");

        var swaggerFile = await DownloadSwaggerJson(webApplicationFactory, options);
        Console.WriteLine("Swaggerfile fetched.");

        await GenerateCSharpClient(swaggerFile, options);
        Console.WriteLine("CSharp client generated.");
    }

    /// <summary>
    /// Generate a TypeScript API Client.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="hostBuilderFactory"></param>
    /// <returns></returns>
    public static async Task GenerateTypeScriptClient(TypeScriptClientGeneratorOptions options, Func<IHostBuilder> hostBuilderFactory)
    {
        using var webApplicationFactory = new GeneratorWebApplicationFactory<TStartup>(hostBuilderFactory);
        Console.WriteLine("Web application factory created.");

        var swaggerFile = await DownloadSwaggerJson(webApplicationFactory, options);
        Console.WriteLine("Swaggerfile fetched.");

        await GenerateTypeScriptClient(swaggerFile, options);
        Console.WriteLine("TypeScript client generated.");
    }

    private static async Task<string> DownloadSwaggerJson(
        GeneratorWebApplicationFactory<TStartup> webApplicationFactory,
        ClientGeneratorOptionsBase options)
    {
        var filePath = Path.Combine(options.GetVerifiedOutputPath(), Path.GetFileName($"{options.ClassName}.swagger.json"));
        Console.WriteLine($"Downloading api description from '{options.SwaggerJsonUrl}' and saving it to '{filePath}'.");

        var client = webApplicationFactory.CreateClient();
        var fileContent = await client.GetStringAsync(options.SwaggerJsonUrl);
        fileContent = fileContent.WithCrlfLineEndings();

        if (options.StoreSwaggerJson)
        {
            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await using var streamWriter = new StreamWriter(fileStream);

            await streamWriter.WriteAsync(fileContent);
        }

        return fileContent;
    }

    private static async Task GenerateCSharpClient(string swaggerJson, CsharpClientGeneratorOptions options)
    {
        var operationNameGenerator = new CsharpOperationNameGenerator();
        var clientGeneratorSettings = options.Map(operationNameGenerator);

        var document = await OpenApiDocument.FromJsonAsync(swaggerJson);
        Console.WriteLine("Swagger JSON parsed.");

        var generator = new CSharpClientGenerator(document, clientGeneratorSettings);
        Console.WriteLine($"Generate C# client '{options.ClassName}'.");

        var filepath = Path.Combine(options.GetVerifiedOutputPath(), Path.GetFileName($"{options.ClassName}.g.cs"));
        await using var fileStream = new FileStream(filepath, FileMode.Create, FileAccess.Write);
        await using var streamWriter = new StreamWriter(fileStream);

        var partialClassPath = Path.Combine(
            options.GetVerifiedOutputPath(),
            Path.GetFileName($"{options.ClassName}.partial.g.cs"));
        await using var partialFileStream = new FileStream(partialClassPath, FileMode.Create, FileAccess.Write);
        await using var partialStreamWriter = new StreamWriter(partialFileStream);

        var newtonsoftPartialFileContent = $@"
namespace {options.ClientNamespace};
internal partial class {options.ClassName}
{{
    partial void Initialize()
    {{
        _instanceSettings = new Newtonsoft.Json.JsonSerializerSettings();
        base.UpdateInstanceJsonSerializerSettings(_instanceSettings);
    }}
}}
";

        await partialStreamWriter.WriteAsync(newtonsoftPartialFileContent);

        await streamWriter.WriteAsync(generator.GenerateFile().WithCrlfLineEndings());
        Console.WriteLine($"Generated: {filepath}");
    }

    private static async Task GenerateTypeScriptClient(string swaggerJson, TypeScriptClientGeneratorOptions options)
    {
        Console.WriteLine($"Reading extensionCode file from '{options.ExtensionPath}'.");

        var extensionsFileNameBase = $"{options.ClassName}.extensions.g";
        var extensionsFileName = $"{extensionsFileNameBase}.ts";

        var mainFileExtensionsImportCode = $"import * as Extensions from './{extensionsFileNameBase}'; {Environment.NewLine}{await GetExternalExtensionCode(options)}";
        var operationNameGenerator = new TypeScriptOperationNameGenerator();
        var settings = options.Map(operationNameGenerator, mainFileExtensionsImportCode);

        var document = await OpenApiDocument.FromJsonAsync(swaggerJson);
        Console.WriteLine("Swagger JSON parsed.");

        var generator = new TypeScriptClientGenerator(document, settings);
        Console.WriteLine($"Generate TypeScript extensions '{extensionsFileName}'.");

        var safeExtensionsFileName = Path.GetFileName(extensionsFileName);
        var extensionsFilepath = Path.Combine(options.GetVerifiedOutputPath(), safeExtensionsFileName);
        await using var extensionsFileStream = new FileStream(extensionsFilepath, FileMode.Create, FileAccess.Write);
        await using var extensionsStreamWriter = new StreamWriter(extensionsFileStream);

        var extensionCode = await GetInternalExtensionCode();
        await extensionsStreamWriter.WriteAsync(extensionCode);
        Console.WriteLine($"Generated: {extensionsFilepath}");

        Console.WriteLine($"Generate TypeScript client '{options.ClassName}'.");
        var safeClassName = Path.GetFileName(options.ClassName);
        var filepath = Path.Combine(options.GetVerifiedOutputPath(), $"{safeClassName}.g.ts");
        await using var fileStream = new FileStream(filepath, FileMode.Create, FileAccess.Write);
        await using var streamWriter = new StreamWriter(fileStream);

        var generatedClient = generator.GenerateFile();
        var generatedVersionComment = GetGeneratedVersionComment();

        await streamWriter.WriteAsync(string.IsNullOrWhiteSpace(generatedVersionComment)
            ? generatedClient
            : $"{generatedVersionComment}{Environment.NewLine}{generatedClient}");
        Console.WriteLine($"Generated: {filepath}");
        await GenerateClientModules(options);
    }

    private static string GetGeneratedVersionComment()
    {
        try
        {
            // Get assembly version from TStartup type using reflection
            var startupAssembly = typeof(TStartup).Assembly;
            var assemblyVersion = startupAssembly.GetName().Version?.ToString();
            
            if (!string.IsNullOrWhiteSpace(assemblyVersion))
            {
                Console.WriteLine($"Using assembly version from reflection: {assemblyVersion}");
                return $"// Generated for Assembly version {assemblyVersion}";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to get assembly version: {ex.Message}");
        }

        return string.Empty;
    }

    private static async Task<string> GetExternalExtensionCode(TypeScriptClientGeneratorOptions options)
    {
        string extensionCode = string.Empty;
        if (options.FindExtensionsPath(out var extensionPath))
        {
            extensionCode = await File.ReadAllTextAsync(extensionPath);
        }

        return extensionCode;
    }

    private static async Task<string> GetInternalExtensionCode()
    {
        const string defaultExtensionsFileName = "_typescript_client_default_extensions.ts";

        var assembly = typeof(TypeScriptClientGeneratorOptions).Assembly;
        var defaultExtensionsPath = assembly.GetManifestResourceNames().FirstOrDefault(i => i.EndsWith(defaultExtensionsFileName)) ?? throw new NullReferenceException($"Could not find default extension file '{defaultExtensionsFileName}' for Typescript web api client.");
        await using var defaultExtensionsStream = assembly.GetManifestResourceStream(defaultExtensionsPath) ?? throw new NullReferenceException($"Could not read default extension file '{defaultExtensionsPath}' for Typescript web api client.");
        using var streamReader = new StreamReader(defaultExtensionsStream);

        return await streamReader.ReadToEndAsync();
    }

    private static async Task GenerateClientModules(TypeScriptClientGeneratorOptions options)
    {
        if (options.GenerateClientModule && !options.ProvideInRoot)
        {
            var moduleExtensionCode = await GetExternalModuleExtensionCode(options);

            ClientModuleGenerator clientModuleGenerator = new(
                Path.Combine(options.GetVerifiedOutputPath(), Path.GetFileName(string.IsNullOrWhiteSpace(options.ModuleClassName) 
                    ? $"{options.ClassName}.module.g.ts" 
                    : $"{options.ModuleClassName}.g.ts")),
                moduleExtensionCode, 
                $"{options.ClassName}.g",
                string.IsNullOrWhiteSpace(options.SupportModuleName) 
                    ? $"{options.ConfigurationClass}Module" 
                    : options.SupportModuleName,
                ClientNameCollector.Instance.Names,
                options.ClientModuleClassPrefix ?? string.Empty);

            await clientModuleGenerator.Generate();
        }
    }
    private static async Task<string> GetExternalModuleExtensionCode(TypeScriptClientGeneratorOptions options)
    {
        string extensionCode = string.Empty;

        if (options.FindModuleExtensionsPath(out var extensionPath))
        {
            extensionCode = await File.ReadAllTextAsync(extensionPath);
        }

        return extensionCode;
    }
}
