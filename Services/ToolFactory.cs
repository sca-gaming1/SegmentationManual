using SegmentationManual.Services;

namespace SegmentationManual.Services;

public static class ToolFactory
{
    public static ITool CreateTool(string toolName, ApiClient apiClient)
    {
        return toolName.ToLowerInvariant() switch
        {
            "visibility" => new VisibilityService(apiClient),
            "segment-filter" => new SegmentFilterService(apiClient),
            _ => throw new ArgumentException(
                $"Unknown tool: {toolName}. Available tools: visibility, segment-filter"
            ),
        };
    }

    public static void ListAvailableTools()
    {
        Console.WriteLine("Available tools:");
        Console.WriteLine("  visibility      - Manages field visibility (hide/unhide)");
        Console.WriteLine("  segment-filter  - Filter segments by data product ID");
    }
}
