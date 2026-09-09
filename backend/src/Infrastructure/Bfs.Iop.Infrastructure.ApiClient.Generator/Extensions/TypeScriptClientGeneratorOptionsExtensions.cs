using Bfs.Iop.Infrastructure.ApiClient.Generator.TypeScript;
using NSwag.CodeGeneration.OperationNameGenerators;
using NSwag.CodeGeneration.TypeScript;
using System.Diagnostics.CodeAnalysis;

namespace Bfs.Iop.Infrastructure.ApiClient.Generator.Extensions;

internal static class TypeScriptClientGeneratorOptionsExtensions
{
    public static bool FindExtensionsPath(this TypeScriptClientGeneratorOptions options, [NotNullWhen(true)] out string? extensionPath)
    {
        if (options.ExtensionPath == null)
        {
            extensionPath = null;
            return false;
        }

        return ClientGeneratorOptionsBaseExtensions.FindExistingFile(options.ExtensionPath, out extensionPath);
    }

    public static bool FindModuleExtensionsPath(this TypeScriptClientGeneratorOptions options, [NotNullWhen(true)] out string? extensionPath)
    {
        if (options.ModuleExtensionPath == null)
        {
            extensionPath = null;
            return false;
        }

        return ClientGeneratorOptionsBaseExtensions.FindExistingFile(options.ModuleExtensionPath, out extensionPath);
    }

    public static TypeScriptClientGeneratorSettings Map(this TypeScriptClientGeneratorOptions options, IOperationNameGenerator operationNameGenerator, string extensionCode)
    {
        var clientBaseClass = !string.IsNullOrWhiteSpace(options.ClientBaseClass) ? options.ClientBaseClass : "Extensions.ApiClientBase";

        var settings = new TypeScriptClientGeneratorSettings
        {
            ClassName = "{controller}Client",
            ClientBaseClass = clientBaseClass,
            GenerateClientClasses = true,
            GenerateClientInterfaces = false,
            GenerateDtoTypes = true,
            InjectionTokenType = InjectionTokenType.InjectionToken,
            OperationNameGenerator = operationNameGenerator,
            PromiseType = PromiseType.Promise,
            Template = TypeScriptTemplate.Angular,
            UseSingletonProvider = options.ProvideInRoot,
            UseTransformResultMethod = true,
            UseGetBaseUrlMethod = false,
            WrapResponses = options.WrapResponses,
            UseTransformOptionsMethod = true,
            ConfigurationClass = options.ConfigurationClass,
            BaseUrlTokenName = options.BaseUrlTokenName,
            RxJsVersion = options.RxJsVersion
        };

        settings.TypeScriptGeneratorSettings.ExtensionCode = extensionCode;

        return settings;
    }
}
