using System.Text.Json.Serialization;

namespace SegmentationManual.Models;

public class Segment
{
    [JsonPropertyName("segmentId")]
    public string SegmentId { get; set; } = string.Empty;

    [JsonPropertyName("segmentName")]
    public string SegmentName { get; set; } = string.Empty;

    [JsonPropertyName("accountClassification")]
    public string AccountClassification { get; set; } = string.Empty;

    [JsonPropertyName("listId")]
    public int? ListId { get; set; }

    [JsonPropertyName("lastSynchronizedDate")]
    public DateTime? LastSynchronizedDate { get; set; }

    [JsonPropertyName("editedDate")]
    public DateTime? EditedDate { get; set; }
}

public class SegmentDetail
{
    [JsonPropertyName("segmentId")]
    public string SegmentId { get; set; } = string.Empty;

    [JsonPropertyName("accountClassification")]
    public string AccountClassification { get; set; } = string.Empty;

    [JsonPropertyName("queryDefinition")]
    public QueryDefinition? QueryDefinition { get; set; }

    [JsonPropertyName("isImported")]
    public bool IsImported { get; set; }

    [JsonPropertyName("editedDate")]
    public DateTime? EditedDate { get; set; }

    [JsonPropertyName("segmentName")]
    public string SegmentName { get; set; } = string.Empty;

    [JsonPropertyName("listId")]
    public int? ListId { get; set; }

    [JsonPropertyName("lastSynchronizedDate")]
    public DateTime? LastSynchronizedDate { get; set; }

    [JsonPropertyName("meshSyncStatus")]
    public string? MeshSyncStatus { get; set; }

    [JsonPropertyName("syncedPlayersToIterableCount")]
    public int? SyncedPlayersToIterableCount { get; set; }
}

public class QueryDefinition
{
    [JsonPropertyName("$type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    [JsonPropertyName("rules")]
    public List<QueryRule>? Rules { get; set; }

    [JsonPropertyName("dataProductId")]
    public string? DataProductId { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("fieldName")]
    public string? FieldName { get; set; }

    [JsonPropertyName("comparator")]
    public object? Comparator { get; set; }
}

public class QueryRule
{
    [JsonPropertyName("$type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    [JsonPropertyName("rules")]
    public List<QueryRule>? Rules { get; set; }

    [JsonPropertyName("dataProductId")]
    public string? DataProductId { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("fieldName")]
    public string? FieldName { get; set; }

    [JsonPropertyName("comparator")]
    public object? Comparator { get; set; }
}

public class SegmentFilterConfiguration : VisibilityConfiguration
{
    public string DataProductId { get; set; } = string.Empty;
}
