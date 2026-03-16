using System.Text.Json;
using SegmentationManual.Models;

namespace SegmentationManual.Services;

public class SegmentFilterService : ITool
{
    private readonly ApiClient _apiClient;

    public string Name => "segment-filter";
    public string Description => "Filter segments by data product ID";

    public SegmentFilterService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<ProcessingResult> ExecuteAsync(
        VisibilityConfiguration config,
        CancellationToken cancellationToken = default
    )
    {
        var result = new ProcessingResult();

        if (config is not SegmentFilterConfiguration filterConfig)
        {
            Console.WriteLine("Error: Configuration must be a SegmentFilterConfiguration");
            result.FailureCount = 1;
            return result;
        }

        if (string.IsNullOrWhiteSpace(filterConfig.DataProductId))
        {
            Console.WriteLine("Error: DataProductId is required in configuration");
            result.FailureCount = 1;
            return result;
        }

        Console.WriteLine($"Filtering segments by Data Product ID: {filterConfig.DataProductId}\n");

        var segments = await _apiClient.GetSegmentsAsync(cancellationToken);
        Console.WriteLine($"Processing {segments.Count} segments...\n");

        var matchingSegments = new List<(Segment segment, string queryJson, string fullBody, int? syncedCount)>();
        int processedCount = 0;
        int errorCount = 0;

        foreach (var segment in segments)
        {
            processedCount++;

            var (detail, rawJson) = await GetSegmentDetailWithRetry(
                segment.SegmentId,
                maxRetries: 2,
                cancellationToken
            );

            if (!string.IsNullOrEmpty(rawJson))
            {
                // Search for dataProductId as a string in the raw JSON
                if (
                    rawJson.Contains(
                        $"\"{filterConfig.DataProductId}\"",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    // Extract just the queryDefinition part
                    string queryDefinitionJson = ExtractQueryDefinition(rawJson);

                    // Check if transformation is needed
                    bool needsTransformation = NeedsTransformation(queryDefinitionJson);
                    
                    if (!needsTransformation)
                    {
                        // Skip this segment, no transformation needed
                        continue;
                    }

                    // Transform lifecycle stages from names to IDs and VIP status to GUIDs
                    string transformedQueryJson = TransformQueryDefinition(queryDefinitionJson);
                    
                    // Generate full update body
                    string fullBody = GenerateUpdateBody(rawJson, transformedQueryJson);

                    int? syncedCount = ExtractSyncedPlayersCount(rawJson);

                    matchingSegments.Add((segment, transformedQueryJson, fullBody, syncedCount));
                    Console.WriteLine($"✓ Match: {segment.SegmentName} ({segment.SegmentId})");

                    result.Operations.Add(
                        new OperationResult
                        {
                            DataSourceId = filterConfig.DataProductId,
                            FieldName = segment.SegmentName,
                            Operation = "Match",
                            Success = true,
                        }
                    );
                    result.SuccessCount++;
                }
            }
            else
            {
                errorCount++;
            }

            await Task.Delay(300, cancellationToken);
        }

        Console.WriteLine(
            $"\n[Final: {processedCount} segments processed, {matchingSegments.Count} matches]\n"
        );

        Console.WriteLine($"{new string('=', 70)}");
        Console.WriteLine("MATCHING SEGMENTS SUMMARY");
        Console.WriteLine(new string('=', 70));

        foreach (var (segment, queryJson, fullBody, syncedCount) in matchingSegments)
        {
            Console.WriteLine($"\nSegment: {segment.SegmentName}");
            Console.WriteLine($"  ID: {segment.SegmentId}");
            Console.WriteLine($"  Classification: {segment.AccountClassification}");
            Console.WriteLine($"  List ID: {segment.ListId}");
            Console.WriteLine($"  Last Synced: {segment.LastSynchronizedDate}");
            Console.WriteLine($"  Synced Players Count: {syncedCount?.ToString() ?? "N/A"}");
        }

        Console.WriteLine($"\n{new string('=', 70)}");
        Console.WriteLine("UPDATE BODIES (Ready to copy-paste for PUT requests)");
        Console.WriteLine($"{new string('=', 70)}\n");

        foreach (var (segment, queryJson, fullBody, syncedCount) in matchingSegments)
        {
            Console.WriteLine($"// Segment: {segment.SegmentName}");
            Console.WriteLine($"// PUT /api/InHouseSegments/{segment.SegmentId}");
            Console.WriteLine(fullBody);
            Console.WriteLine($"\n{new string('-', 70)}\n");
        }

        return result;
    }

    private async Task<(SegmentDetail? detail, string rawJson)> GetSegmentDetailWithRetry(
        string segmentId,
        int maxRetries,
        CancellationToken cancellationToken
    )
    {
        Exception? lastException = null;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                var (detail, rawJson) = await _apiClient.GetSegmentDetailAsync(
                    segmentId,
                    cancellationToken
                );

                if (!string.IsNullOrEmpty(rawJson))
                {
                    return (detail, rawJson);
                }

                // If we got an empty response but no exception, it's likely a 4xx/5xx error
                // Don't retry these, just skip to next segment
                return (null, string.Empty);
            }
            catch (TaskCanceledException ex)
            {
                lastException = ex;
                if (attempt < maxRetries)
                {
                    await Task.Delay(3000 * (attempt + 1), cancellationToken);
                }
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                if (attempt < maxRetries)
                {
                    await Task.Delay(2000 * (attempt + 1), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt < maxRetries)
                {
                    await Task.Delay(1500 * (attempt + 1), cancellationToken);
                }
            }
        }

        // After all retries failed, log once
        if (lastException is TaskCanceledException)
        {
            Console.WriteLine($"✗ Timeout: {segmentId} (after {maxRetries + 1} attempts)");
        }
        else if (lastException != null)
        {
            Console.WriteLine(
                $"✗ Error: {segmentId} - {lastException.Message} (after {maxRetries + 1} attempts)"
            );
        }

        return (null, string.Empty);
    }

    private string ExtractQueryDefinition(string rawJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("queryDefinition", out var queryDef))
            {
                return JsonSerializer.Serialize(
                    queryDef,
                    new JsonSerializerOptions { WriteIndented = true }
                );
            }
        }
        catch
        {
            // If extraction fails, return empty formatted JSON
        }

        return "{}";
    }

    private string GenerateUpdateBody(string rawJson, string transformedQueryDefinitionJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            var updateBody = new Dictionary<string, object?>();

            // Extract required fields
            if (root.TryGetProperty("segmentName", out var segmentName))
                updateBody["segmentName"] = segmentName.GetString();

            if (root.TryGetProperty("listId", out var listId))
                updateBody["listId"] = listId.ValueKind == JsonValueKind.Number ? listId.GetInt32() : 0;

            if (root.TryGetProperty("lastSynchronizedDate", out var lastSyncDate))
                updateBody["lastSynchronizedDate"] = lastSyncDate.GetString();

            if (root.TryGetProperty("meshSyncStatus", out var meshSyncStatus))
            {
                if (meshSyncStatus.ValueKind == JsonValueKind.Object)
                {
                    var meshStatus = new Dictionary<string, object?>();
                    
                    if (meshSyncStatus.TryGetProperty("status", out var status))
                        meshStatus["status"] = status.GetString();
                    
                    if (meshSyncStatus.TryGetProperty("occuredAt", out var occuredAt))
                        meshStatus["occuredAt"] = occuredAt.GetString();
                    
                    if (meshSyncStatus.TryGetProperty("lastSuccessfulSyncedAt", out var lastSuccess))
                        meshStatus["lastSuccessfulSyncedAt"] = lastSuccess.GetString();
                    
                    updateBody["meshSyncStatus"] = meshStatus;
                }
                else
                {
                    updateBody["meshSyncStatus"] = null;
                }
            }

            if (root.TryGetProperty("syncedPlayersToIterableCount", out var syncedCount))
                updateBody["syncedPlayersToIterableCount"] = syncedCount.ValueKind == JsonValueKind.Number ? syncedCount.GetInt32() : 0;

            if (root.TryGetProperty("accountClassification", out var accountClass))
                updateBody["accountClassification"] = accountClass.GetString();

            // Parse and add transformed query definition
            using var queryDoc = JsonDocument.Parse(transformedQueryDefinitionJson);
            updateBody["queryDefinition"] = JsonSerializer.Deserialize<object>(transformedQueryDefinitionJson);

            return JsonSerializer.Serialize(
                updateBody,
                new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                }
            );
        }
        catch
        {
            // If body generation fails, return minimal body
            return @"{
  ""segmentName"": ""string"",
  ""listId"": 0,
  ""lastSynchronizedDate"": null,
  ""meshSyncStatus"": null,
  ""syncedPlayersToIterableCount"": 0,
  ""accountClassification"": ""string"",
  ""queryDefinition"": {}
}";
        }
    }

    private string TransformQueryDefinition(string queryDefinitionJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(queryDefinitionJson);
            var root = doc.RootElement;

            var transformed = TransformJsonElement(root);

            return JsonSerializer.Serialize(
                transformed,
                new JsonSerializerOptions { WriteIndented = true }
            );
        }
        catch
        {
            // If transformation fails, return original JSON
            return queryDefinitionJson;
        }
    }

    private bool NeedsTransformation(string queryDefinitionJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(queryDefinitionJson);
            return ContainsFieldsToTransform(doc.RootElement);
        }
        catch
        {
            return false;
        }
    }

    private bool ContainsFieldsToTransform(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("fieldName", out var fieldNameElement) &&
                fieldNameElement.ValueKind == JsonValueKind.String)
            {
                var fieldName = fieldNameElement.GetString();
                if (fieldName == "current_stage" || 
                    fieldName == "previous_stage" ||
                    fieldName == "last_vip_status" ||
                    fieldName == "max_vip_status")
                {
                    return true;
                }
            }

            foreach (var property in element.EnumerateObject())
            {
                if (ContainsFieldsToTransform(property.Value))
                {
                    return true;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (ContainsFieldsToTransform(item))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private JsonElement TransformJsonElement(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var dict = new Dictionary<string, object?>();

            string? fieldName = null;
            if (element.TryGetProperty("fieldName", out var fieldNameElement) &&
                fieldNameElement.ValueKind == JsonValueKind.String)
            {
                fieldName = fieldNameElement.GetString();
            }

            foreach (var property in element.EnumerateObject())
            {
                if (property.Name == "fieldName")
                {
                    var currentFieldName = property.Value.GetString();
                    
                    // Transform lifecycle stage fields
                    if (currentFieldName == "current_stage" || currentFieldName == "previous_stage")
                    {
                        dict[property.Name] = currentFieldName + "_id";
                    }
                    // Transform VIP status fields
                    else if (currentFieldName == "last_vip_status" || currentFieldName == "max_vip_status")
                    {
                        dict[property.Name] = currentFieldName + "_guid";
                    }
                    else
                    {
                        dict[property.Name] = currentFieldName;
                    }
                }
                else if (property.Name == "comparator" &&
                         (fieldName == "current_stage" || 
                          fieldName == "previous_stage" ||
                          fieldName == "last_vip_status" ||
                          fieldName == "max_vip_status"))
                {
                    dict[property.Name] = TransformComparator(property.Value, fieldName);
                }
                else if (property.Value.ValueKind == JsonValueKind.Object ||
                         property.Value.ValueKind == JsonValueKind.Array)
                {
                    dict[property.Name] = TransformJsonElementToObject(property.Value);
                }
                else
                {
                    dict[property.Name] = GetPrimitiveValue(property.Value);
                }
            }

            return JsonSerializer.SerializeToElement(dict);
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            var list = new List<object?>();
            foreach (var item in element.EnumerateArray())
            {
                list.Add(TransformJsonElementToObject(item));
            }
            return JsonSerializer.SerializeToElement(list);
        }

        return element;
    }

    private object? TransformJsonElementToObject(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var dict = new Dictionary<string, object?>();

            string? fieldName = null;
            if (element.TryGetProperty("fieldName", out var fieldNameElement) &&
                fieldNameElement.ValueKind == JsonValueKind.String)
            {
                fieldName = fieldNameElement.GetString();
            }

            foreach (var property in element.EnumerateObject())
            {
                if (property.Name == "fieldName")
                {
                    var currentFieldName = property.Value.GetString();
                    
                    // Transform lifecycle stage fields
                    if (currentFieldName == "current_stage" || currentFieldName == "previous_stage")
                    {
                        dict[property.Name] = currentFieldName + "_id";
                    }
                    // Transform VIP status fields
                    else if (currentFieldName == "last_vip_status" || currentFieldName == "max_vip_status")
                    {
                        dict[property.Name] = currentFieldName + "_guid";
                    }
                    else
                    {
                        dict[property.Name] = currentFieldName;
                    }
                }
                else if (property.Name == "comparator" &&
                         (fieldName == "current_stage" || 
                          fieldName == "previous_stage" ||
                          fieldName == "last_vip_status" ||
                          fieldName == "max_vip_status"))
                {
                    dict[property.Name] = TransformComparator(property.Value, fieldName);
                }
                else
                {
                    dict[property.Name] = TransformJsonElementToObject(property.Value);
                }
            }
            return dict;
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            var list = new List<object?>();
            foreach (var item in element.EnumerateArray())
            {
                list.Add(TransformJsonElementToObject(item));
            }
            return list;
        }
        else
        {
            return GetPrimitiveValue(element);
        }
    }

    private object? TransformComparator(JsonElement comparatorElement, string? fieldName)
    {
        if (comparatorElement.ValueKind != JsonValueKind.Object)
        {
            return GetPrimitiveValue(comparatorElement);
        }

        var comparatorDict = new Dictionary<string, object?>();
        bool isLifecycleStage = fieldName == "current_stage" || fieldName == "previous_stage";
        bool isVipStatus = fieldName == "last_vip_status" || fieldName == "max_vip_status";

        foreach (var property in comparatorElement.EnumerateObject())
        {
            if (property.Name == "values" && property.Value.ValueKind == JsonValueKind.Array)
            {
                var transformedValues = new List<object?>();
                foreach (var value in property.Value.EnumerateArray())
                {
                    if (value.ValueKind == JsonValueKind.String)
                    {
                        var stringValue = value.GetString();
                        
                        if (stringValue != null)
                        {
                            // Transform lifecycle stage names to IDs
                            if (isLifecycleStage && LifecycleStageMapping.TryGetStageId(stringValue, out var stageId))
                            {
                                transformedValues.Add(stageId);
                            }
                            // Transform VIP club levels to GUIDs
                            else if (isVipStatus && VipStatusMapping.TryGetClubLevelGuid(stringValue, out var guid))
                            {
                                transformedValues.Add(guid);
                            }
                            else
                            {
                                transformedValues.Add(stringValue);
                            }
                        }
                        else
                        {
                            transformedValues.Add(stringValue);
                        }
                    }
                    else
                    {
                        transformedValues.Add(GetPrimitiveValue(value));
                    }
                }
                comparatorDict[property.Name] = transformedValues;
            }
            else
            {
                comparatorDict[property.Name] = TransformJsonElementToObject(property.Value);
            }
        }

        return comparatorDict;
    }

    private object? GetPrimitiveValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }

    private int? ExtractSyncedPlayersCount(string rawJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            if (root.TryGetProperty("syncedPlayersToIterableCount", out var countElement))
            {
                if (countElement.ValueKind == JsonValueKind.Number)
                {
                    return countElement.GetInt32();
                }
            }
        }
        catch
        {
            // If extraction fails, return null
        }

        return null;
    }
}
