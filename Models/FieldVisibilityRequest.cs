namespace SegmentationManual.Models;

public class FieldVisibilityRequest
{
    public string DataSourceId { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public bool Hide { get; set; }
}

public class DataProductConfig
{
    public string DataSourceId { get; set; } = string.Empty;
    public List<string> FieldsToHide { get; set; } = new();
    public List<string> FieldsToUnhide { get; set; } = new();
}

public class VisibilityConfiguration
{
    public string? BaseUrl { get; set; }
    public List<DataProductConfig> DataProducts { get; set; } = new();
}
