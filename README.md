# Segmentation Manual

Console application for managing field visibility and filtering segments in the segmentation API.

## Features

- Hide/Unhide fields via API
- Filter segments by data product ID
- JSON configuration support
- Multiple DataSources support
- Error handling and detailed reports

## Environment URLs

| Tenant | Base URL |
|--------|----------|
| 777 | https://sgmt-segmentation-du.777be-prd-ee-sg.777.be |
| CIBE | https://sgmt-segmentation-du.cibe-prd-ee-sg.circus.be |

## Available Tools

### 1. Visibility Tool (Default)

Manages field visibility (hide/unhide) based on configuration file.

### 2. Segment Filter Tool

Filters and lists all segments that use a specific data product ID in their query definition.

## Getting Started

### For Visibility Management

#### Step 1: Find Your Segment IDs

First, identify the segment sources you need to modify using these endpoints:

```bash
GET {baseUrl}/api/SegmentationAttributeSources
GET {baseUrl}/api/SegmentationActivitySources
GET {baseUrl}/api/SegmentationAudienceSources
```

Each response contains segment objects with an `id` property - this is your `dataSourceId`.

#### Step 2: List Available Fields

For each segment ID, retrieve all fields to verify their names:

```bash
GET {baseUrl}/api/SegmentationFields/{segmentId}
```

#### Step 3: Create Configuration File

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

#### Step 4: Run the Application

```bash
dotnet run -- --url {baseUrl} --config your-config.json --method visibility
```

### For Segment Filtering

Filter all segments that use a specific data product ID:

```bash
dotnet run -- --url https://sgmt-segmentation-777.cibe-prd-ee-sg.circus.be --tool segment-filter --data-product-id 45e8c6f2-6fe0-4075-ab0d-9829e6d99591
```

This will:
1. ? Retrieve all segments from `/Segments` endpoint
2. ? Fetch detailed information for each segment from `/InHouseSegments/{segmentId}`
3. ? Search recursively through the query definition for the specified data product ID
4. ? **Automatically transform field names and values:**
   - `current_stage` ? `current_stage_id` with lifecycle stage names converted to IDs
   - `previous_stage` ? `previous_stage_id` with lifecycle stage names converted to IDs
   - `last_vip_status` ? `last_vip_status_guid` with club level names converted to GUIDs
   - `max_vip_status` ? `max_vip_status_guid` with club level names converted to GUIDs
5. ? Display only segments that **require transformation** (skip segments that don't use these fields)
6. ? Generate ready-to-use PUT request bodies with transformed query definitions

**Transformations applied:**

**Lifecycle Stages:**
- "Non Depositors" ? 2
- "Churn 1" ? 5
- "Active" ? 6
- etc. (see full mapping in code)

**VIP Club Levels:**
- "Copper" ? "651c5e91-7a6d-45d4-957a-78f75389b398"
- "Diamond" ? "91276247-b9bd-43d2-8138-50c88154e6f0"
- "Gold" ? "0571c538-da08-4ada-822a-614bc86b6866"
- etc. (see full mapping in code)

**Output includes:**
- Segment name and ID
- Account classification
- List ID
- Last synchronized date
- Synced players count
- **Full PUT request body ready to copy-paste** (only for segments needing transformation)

## Saving Console Logs

To keep a record of your executions, save the console output to the `Logs` folder using this naming convention:

```bash
dotnet run -- --url {baseUrl} --config your-config.json --method visibility > Logs/{ticket-id}-{tenant}-{action}-{date}.log
```

**Examples:**
- `Logs/ip-1707-777-hide-020926.log`
- `Logs/310-cibe-030226.log`
- `Logs/segment-filter-45e8c6f2-160326.log`

**Naming pattern:** `{ticket-id}-{tenant}-{action}-{DDMMYY}.log`

## Command Line Options

- `--url <url>` - Base API URL (overrides `baseUrl` in config)
- `--config <file>` - JSON configuration file (default: `config.json`)
- `--method <tool>` - Tool to use (default: `visibility`)
- `--tool <tool>` - Alias for `--method`
- `--data-product-id <id>` - Data Product ID to filter (required for segment-filter tool)
- `--help, -h` - Show help

## Usage Examples

### Visibility Tool
```bash
dotnet run -- --url https://sgmt-segmentation-du.777be-prd-ee-sg.777.be --config your-config.json --method visibility
```

### Segment Filter Tool
```bash
# Filter segments by data product ID
dotnet run -- --url https://sgmt-segmentation-777.cibe-prd-ee-sg.circus.be --tool segment-filter --data-product-id 45e8c6f2-6fe0-4075-ab0d-9829e6d99591

# Save output to log file
dotnet run -- --url https://sgmt-segmentation-777.cibe-prd-ee-sg.circus.be --tool segment-filter --data-product-id 45e8c6f2-6fe0-4075-ab0d-9829e6d99591 > Logs/segment-filter-45e8c6f2-160326.log
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
| Get all segments | `GET /Segments` |
| Get segment detail | `GET /InHouseSegments/{segmentId}` |
