using SegmentationManual.Models;

namespace SegmentationManual.Services;

public class VisibilityService : ITool
{
    private readonly ApiClient _apiClient;

    public string Name => "visibility";
    public string Description => "Manages field visibility (hide/unhide)";

    public VisibilityService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<ProcessingResult> ExecuteAsync(
        VisibilityConfiguration config,
        CancellationToken cancellationToken = default
    )
    {
        var result = new ProcessingResult();

        Console.WriteLine($"\nProcessing {config.DataProducts.Count} DataProduct(s)...\n");

        foreach (var dataProduct in config.DataProducts)
        {
            Console.WriteLine($"\nDataSource: {dataProduct.DataSourceId}");

            foreach (var fieldName in dataProduct.FieldsToHide)
            {
                Console.WriteLine($"  Hide: {fieldName}");
                var success = await _apiClient.HideFieldAsync(
                    dataProduct.DataSourceId,
                    fieldName,
                    HttpMethod.Put,
                    cancellationToken
                );

                if (success)
                    result.SuccessCount++;
                else
                    result.FailureCount++;

                result.Operations.Add(
                    new OperationResult
                    {
                        DataSourceId = dataProduct.DataSourceId,
                        FieldName = fieldName,
                        Operation = "Hide",
                        Success = success,
                    }
                );

                await Task.Delay(500, cancellationToken);
            }

            foreach (var fieldName in dataProduct.FieldsToUnhide)
            {
                Console.WriteLine($"  Unhide: {fieldName}");
                var success = await _apiClient.UnhideFieldAsync(
                    dataProduct.DataSourceId,
                    fieldName,
                    HttpMethod.Put,
                    cancellationToken
                );

                if (success)
                    result.SuccessCount++;
                else
                    result.FailureCount++;

                result.Operations.Add(
                    new OperationResult
                    {
                        DataSourceId = dataProduct.DataSourceId,
                        FieldName = fieldName,
                        Operation = "Unhide",
                        Success = success,
                    }
                );

                await Task.Delay(500, cancellationToken);
            }
        }

        return result;
    }
}

public class OperationResult
{
    public string DataSourceId { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public bool Success { get; set; }
}

public class ProcessingResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<OperationResult> Operations { get; set; } = new();

    public int TotalOperations => SuccessCount + FailureCount;
}
