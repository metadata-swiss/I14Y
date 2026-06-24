namespace Bfs.Iop.Infrastructure.ApiClient.Generator.TypeScript;

internal sealed class ClientModuleGenerator
{
    public ClientModuleGenerator(
        string file,
        string baseTemplate,
        string clientsFile,
        string supportModuleClass,
        IEnumerable<string> clientNames,
        string modulePrefix)
    {
        FileName = file;
        BaseTemplate = baseTemplate;
        ClientsFileName = clientsFile;
        SupportModuleClassName = supportModuleClass;
        ClientNames = clientNames;
        ModulePrefix = modulePrefix;
    }

    public string BaseTemplate { get; private set; }

    public IEnumerable<string> ClientNames { get; private set; }

    public string ClientsFileName { get; private set; }

    public string FileName { get; private set; }

    public string ModulePrefix { get; private set; }

    public string SupportModuleClassName { get; private set; }

    public async Task Generate()
    {
        using StreamWriter writer = File.CreateText(this.FileName);

        await WritePrependingTemplate(writer);
        await WriteCommonImports(writer);
        await WriteClientServiceImports(writer);
        await WriteClientModuleClasses(writer);
    }

    public async Task PerformForEachClient(Func<string, bool, Task> action)
    {
        var lastClientName = ClientNames.LastOrDefault();

        foreach (var clientName in ClientNames)
        {
            await action(clientName, lastClientName == clientName);
        }
    }

    private static async Task WriteCommonImports(StreamWriter writer)
    {
        await writer.WriteLineAsync("import { NgModule } from '@angular/core';");
        await writer.WriteLineAsync("import { CommonModule } from '@angular/common'; ");
    }

    private async Task WriteClientModuleClasses(StreamWriter writer)
    {
        await PerformForEachClient(async (clientName, last) =>
        {
            await writer.WriteLineAsync("@NgModule({");
            await writer.WriteLineAsync("  declarations: [],");
            await writer.WriteLineAsync("  imports: [");
            await writer.WriteLineAsync("    CommonModule,");
            await writer.WriteLineAsync($"    {SupportModuleClassName}");
            await writer.WriteLineAsync("  ],");
            await writer.WriteLineAsync("  providers: [");
            await writer.WriteLineAsync($"    {clientName}Client");
            await writer.WriteLineAsync("  ]");
            await writer.WriteLineAsync("})");
            await writer.WriteLineAsync($"export class {ModulePrefix}{clientName}ClientModule {{ }}");

            if (!last)
            {
                await writer.WriteLineAsync(string.Empty);
            }
        });
    }

    private async Task WriteClientServiceImports(StreamWriter writer)
    {
        await PerformForEachClient((clientName, last) => writer.WriteLineAsync($"import {{ {clientName}Client }} from './{ClientsFileName}';"));
        await writer.WriteLineAsync(string.Empty);
    }

    private async Task WritePrependingTemplate(StreamWriter writer)
    {
        if (!string.IsNullOrWhiteSpace(BaseTemplate))
        {
            await writer.WriteAsync(BaseTemplate);
            await writer.WriteLineAsync(string.Empty);
        }
    }
}
