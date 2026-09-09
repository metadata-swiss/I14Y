using Bfs.Iop.Infrastructure.ApiClient.Generator.Csharp;
using Bfs.Iop.Infrastructure.ApiClient.NewtonsoftJson;
using NJsonSchema.CodeGeneration.CSharp;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.OperationNameGenerators;

namespace Bfs.Iop.Infrastructure.ApiClient.Generator.Extensions;

internal static class CsharpClientGeneratorOptionsExtensions
{
    public static CSharpClientGeneratorSettings Map(this CsharpClientGeneratorOptions options, IOperationNameGenerator operationNameGenerator)
    {
        var namespaces = options.AdditionalNamespaceUsages
            .Append("Bfs.Iop.Infrastructure.ApiClient")
            .Append("Microsoft.AspNetCore.Mvc");

        var clientBaseClass = GetClientBaseClass(options);
        var configurationClass = GetConfigurationClass(options);

        var settings = new CSharpClientGeneratorSettings
        {
            AdditionalNamespaceUsages = [.. namespaces.Distinct()],
            ClassName = options.ClassName,
            ClientBaseClass = clientBaseClass,
            ClientClassAccessModifier = "internal",
            ConfigurationClass = configurationClass,
            CSharpGeneratorSettings =
            {
                GenerateOptionalPropertiesAsNullable = options.GenerateOptionalPropertiesAsNullable,
                Namespace = options.ClientNamespace,
                JsonLibrary = CSharpJsonLibrary.NewtonsoftJson
            },
            UseBaseUrl = options.UseBaseUrl,
            ExposeJsonSerializerSettings = true,
            GenerateClientClasses = true,
            GenerateClientInterfaces = options.GenerateClientInterfaces,
            GenerateDtoTypes = options.GenerateDtoTypes,
            GenerateExceptionClasses = false,
            GenerateResponseClasses = false,
            InjectHttpClient = false,
            OperationNameGenerator = operationNameGenerator,
            GenerateUpdateJsonSerializerSettingsMethod = true,
            GeneratePrepareRequestAndProcessResponseAsAsyncMethods = options.GeneratePrepareRequestAndProcessResponseAsAsyncMethods,
            QueryNullValue = "null",
            WrapResponses = true,
            UseHttpClientCreationMethod = true,
            UseHttpRequestMessageCreationMethod = options.UseHttpRequestMessageCreationMethod,
        };

        return settings;
    }

    private static string? GetConfigurationClass(CsharpClientGeneratorOptions options)
    {
        return string.IsNullOrWhiteSpace(options.ConfigurationClass)
            ? typeof(INewtonsoftJsonClientSupport).FullName
            : options.ConfigurationClass;
    }

    private static string? GetClientBaseClass(CsharpClientGeneratorOptions options)
    {
        return string.IsNullOrWhiteSpace(options.ClientBaseClass)
            ? typeof(NewtonsoftJsonWebApiClientBase).FullName
            : options.ClientBaseClass;
    }
}
