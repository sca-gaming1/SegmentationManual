# Segmentation Manual

Console application for managing field visibility in the segmentation API.

## Features

- Hide/Unhide fields via API
- JSON configuration support
- Multiple DataSources support
- Error handling and detailed reports

## Environment URLs

| Tenant | Base URL |
|--------|----------|
| 777 | https://sgmt-segmentation-du.777be-prd-ee-sg.777.be |
| CIBE | https://sgmt-segmentation-du.cibe-prd-ee-sg.circus.be |

## Getting Started

### Step 1: Find Your Segment IDs

First, identify the segment sources you need to modify using these endpoints:

```bash
GET {baseUrl}/api/SegmentationAttributeSources
GET {baseUrl}/api/SegmentationActivitySources
GET {baseUrl}/api/SegmentationAudienceSources
```

Each response contains segment objects with an `id` property - this is your `dataSourceId`.

### Step 2: List Available Fields

For each segment ID, retrieve all fields to verify their names:

```bash
GET {baseUrl}/api/SegmentationFields/{segmentId}
```

### Step 3: Create Configuration File

Create a `config.json` with your segment IDs and fields from Excel:

```json
{
  "baseUrl": "https://sgmt-segmentation-du.777be-prd-ee-sg.777.be",
  "dataProducts": [
    {
      "dataSourceId": "segment-id-from-step-1",
      "fieldsToHide": ["field1", "field2"],
      "fieldsToUnhide": ["field3"]
    }
  ]
}
```

**Note:** You can add multiple segments in the `dataProducts` array.

### Step 4: Run the Application

```bash
dotnet run -- --url {baseUrl} --config your-config.json --method visibility
```

## Saving Console Logs

To keep a record of your executions, save the console output to the `Logs` folder using this naming convention:

```bash
dotnet run -- --url {baseUrl} --config your-config.json --method visibility > Logs/{ticket-id}-{tenant}-{action}-{date}.log
```

**Examples:**
- `Logs/ip-1707-777-hide-020926.log`
- `Logs/310-cibe-030226.log`

**Naming pattern:** `{ticket-id}-{tenant}-{action}-{DDMMYY}.log`

## Command Line Options

- `--url <url>` - Base API URL (overrides `baseUrl` in config)
- `--config <file>` - JSON configuration file (default: `config.json`)
- `--method <tool>` - Tool to use (default: `visibility`)
- `--tool <tool>` - Alias for `--method`
- `--help, -h` - Show help

## Usage Examples

```bash
dotnet run --url https://sgmt-segmentation-du.777be-prd-ee-sg.777.be  --config your-config.json --method visibility
```

## API Reference

| Action | Endpoint |
|--------|----------|
| Hide field | `PUT /api/DataSources/{dataSourceId}/Fields/{fieldName}/Visibility/Hide` |
| Unhide field | `PUT /api/DataSources/{dataSourceId}/Fields/{fieldName}/Visibility/Unhide` |
| List attribute sources | `GET /api/SegmentationAttributeSources` |
| List activity sources | `GET /api/SegmentationActivitySources` |
| List audience sources | `GET /api/SegmentationAudienceSources` |
| List segment fields | `GET /api/SegmentationFields/{segmentId}` |
