# Segmentation Manual

Console application for managing field visibility in the segmentation API.

## Features

- Hide/Unhide fields via API
- JSON configuration support
- Multiple DataSources support
- Error handling and detailed reports

## Quick Start

1. Create a `config.json` file:

```json
{
  "baseUrl": "https://sgmt-segmentation-{tenant}.cibe-prd-ee-sg.circus.be",
  "dataProducts": [
    {
      "dataSourceId": "your-datasource-id",
      "fieldsToHide": ["field1", "field2"],
      "fieldsToUnhide": ["field3"]
    }
  ]
}
```

2. Run the application:

```bash
dotnet run -- --url https://sgmt-segmentation-du.777be-prd-ee-sg.777.be --config ip-1707-config.json --method visibility
```

## Usage

```bash
# With default config.json
dotnet run

# With custom config file
dotnet run -- --config my-config.json --method visibility

# With custom URL
dotnet run -- --url https://sgmt-segmentation-777.cibe-prd-ee-sg.circus.be --config ip-1707-config.json --method visibility
```

## Options

- `--url <url>` - Base API URL
- `--config <file>` - JSON configuration file (default: config.json)
- `--method <tool>` - Tool to use (default: visibility)
- `--tool <tool>` - Alias for --method
- `--help, -h` - Show help

## Available Tools

- `visibility` - Manages field visibility (hide/unhide)

## API Endpoints

- Hide: `PUT /api/DataSources/{dataSourceId}/Fields/{fieldName}/Visibility/Hide`
- Unhide: `PUT /api/DataSources/{dataSourceId}/Fields/{fieldName}/Visibility/Unhide`
