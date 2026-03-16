using System.Text.Json;
using SegmentationManual.Models;
using SegmentationManual.Services;

namespace SegmentationManual;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Segmentation Manual - Management Tools\n");

        string? baseUrl = null;
        string? configFile = null;
        string? toolName = null;
        string? dataProductId = null;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--url" && i + 1 < args.Length)
            {
                baseUrl = args[i + 1];
            }
            else if (args[i] == "--config" && i + 1 < args.Length)
            {
                configFile = args[i + 1];
            }
            else if (args[i] == "--method" && i + 1 < args.Length)
            {
                toolName = args[i + 1];
            }
            else if (args[i] == "--tool" && i + 1 < args.Length)
            {
                toolName = args[i + 1];
            }
            else if (args[i] == "--data-product-id" && i + 1 < args.Length)
            {
                dataProductId = args[i + 1];
            }
            else if (args[i] == "--help" || args[i] == "-h")
            {
                ShowHelp();
                return;
            }
        }

        toolName ??= "visibility";
        configFile ??= "config.json";

        VisibilityConfiguration config;
        string finalBaseUrl;

        if (toolName == "segment-filter")
        {
            if (string.IsNullOrWhiteSpace(dataProductId))
            {
                Console.WriteLine("Error: --data-product-id is required for segment-filter tool");
                Console.WriteLine("Example: --tool segment-filter --data-product-id 45e8c6f2-6fe0-4075-ab0d-9829e6d99591");
                return;
            }

            finalBaseUrl = baseUrl ?? "";
            if (string.IsNullOrWhiteSpace(finalBaseUrl) && File.Exists(configFile))
            {
                try
                {
                    var jsonContent = await File.ReadAllTextAsync(configFile);
                    var tempConfig = JsonSerializer.Deserialize<VisibilityConfiguration>(
                        jsonContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    finalBaseUrl = tempConfig?.BaseUrl ?? "";
                }
                catch { }
            }

            if (string.IsNullOrWhiteSpace(finalBaseUrl))
            {
                Console.WriteLine("Base URL missing. Use --url <url> or specify it in config.json");
                return;
            }

            config = new SegmentFilterConfiguration
            {
                BaseUrl = finalBaseUrl,
                DataProductId = dataProductId
            };
        }
        else
        {
            if (!File.Exists(configFile))
            {
                Console.WriteLine($"Configuration file '{configFile}' not found.");
                Console.WriteLine("Create a 'config.json' file or use --config <file>");
                return;
            }

            VisibilityConfiguration? loadedConfig;
            try
            {
                var jsonContent = await File.ReadAllTextAsync(configFile);
                loadedConfig = JsonSerializer.Deserialize<VisibilityConfiguration>(
                    jsonContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading configuration file: {ex.Message}");
                return;
            }

            if (loadedConfig == null)
            {
                Console.WriteLine("Invalid or empty configuration.");
                return;
            }

            finalBaseUrl = baseUrl ?? loadedConfig.BaseUrl ?? "";

            if (string.IsNullOrWhiteSpace(finalBaseUrl))
            {
                Console.WriteLine("Base URL missing. Use --url <url> or specify it in config.json");
                return;
            }

            loadedConfig.BaseUrl = finalBaseUrl;
            config = loadedConfig;
        }

        Console.WriteLine($"Base URL: {config.BaseUrl}\n");

        using var apiClient = new ApiClient(config.BaseUrl!);
        ITool tool;

        try
        {
            tool = ToolFactory.CreateTool(toolName, apiClient);
            Console.WriteLine($"Selected tool: {tool.Name} - {tool.Description}\n");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            ToolFactory.ListAvailableTools();
            return;
        }

        try
        {
            var result = await tool.ExecuteAsync(config);

            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("SUMMARY");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"Success: {result.SuccessCount}");
            Console.WriteLine($"Failures: {result.FailureCount}");
            Console.WriteLine($"Total: {result.TotalOperations} operations");
            Console.WriteLine(new string('=', 50));

            var failures = result.Operations.Where(o => !o.Success).ToList();
            if (failures.Any())
            {
                Console.WriteLine("\nFailed operations:");
                foreach (var failure in failures)
                {
                    Console.WriteLine(
                        $"  - {failure.Operation} {failure.FieldName} in {failure.DataSourceId}"
                    );
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError during processing: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Details: {ex.InnerException.Message}");
            }
            Environment.Exit(1);
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  SegmentationManual.exe [options]");
        Console.WriteLine("");
        Console.WriteLine("Options:");
        Console.WriteLine(
            "  --url <url>                Base URL of the API (ex: https://sgmt-segmentation-777.cibe-prd-ee-sg.circus.be)"
        );
        Console.WriteLine("  --config <file>            JSON configuration file (default: config.json)");
        Console.WriteLine("  --method <tool>            Method/tool to use (default: visibility)");
        Console.WriteLine("  --tool <tool>              Alias for --method");
        Console.WriteLine("  --data-product-id <id>     Data Product ID to filter (required for segment-filter tool)");
        Console.WriteLine("  --help, -h                 Show this help");
        Console.WriteLine("");
        ToolFactory.ListAvailableTools();
        Console.WriteLine("");
        Console.WriteLine("Examples:");
        Console.WriteLine(
            "  dotnet run -- --url https://sgmt-segmentation-777.cibe-prd-ee-sg.circus.be --config ip-707-config.json --method visibility"
        );
        Console.WriteLine("  dotnet run -- --config ip-707-config.json --method visibility");
        Console.WriteLine("");
        Console.WriteLine("  # Filter segments by data product ID:");
        Console.WriteLine(
            "  dotnet run -- --url https://sgmt-segmentation-777.cibe-prd-ee-sg.circus.be --tool segment-filter --data-product-id 45e8c6f2-6fe0-4075-ab0d-9829e6d99591"
        );
    }
}
