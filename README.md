# SvgToDxf

A .NET library for converting SVG files to DXF format using native Inkscape `dxf_outlines` extension.

[![NuGet](https://img.shields.io/nuget/v/SvgToDxf.svg)](https://www.nuget.org/packages/SvgToDxf/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- Convert SVG to DXF format
- Cross-platform: Linux (glibc/musl), macOS (Intel/Apple Silicon), Windows (x64/ARM64)
- Self-contained native binaries - no Python or Inkscape installation required
- Supports .NET 8.0, 9.0, and 10.0
- Optional logging support via `Microsoft.Extensions.Logging`

## Installation

```bash
dotnet add package SvgToDxf
```

## Usage

### Basic Usage

```csharp
using SvgToDxf;

// Create converter
var converter = new SvgToDxfConverter();

// Convert SVG bytes to DXF bytes
byte[] svgBytes = File.ReadAllBytes("input.svg");
byte[] dxfBytes = await converter.ConvertAsync(svgBytes);
File.WriteAllBytes("output.dxf", dxfBytes);
```

### With Options

```csharp
var options = new DxfConversionOptions
{
    UsePolyline = true,      // Use LWPOLYLINE instead of LINE
    UseLegacyPolyline = false,
    UseRobo = false,
    Units = "mm"             // Output units: mm, cm, in, pt, px
};

byte[] dxfBytes = await converter.ConvertAsync(svgBytes, options);
```

### With Stream Input

```csharp
using var svgStream = File.OpenRead("input.svg");
byte[] dxfBytes = await converter.ConvertAsync(svgStream);
```

### With Logging

```csharp
using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<SvgToDxfConverter>();

var converter = new SvgToDxfConverter(logger);
byte[] dxfBytes = await converter.ConvertAsync(svgBytes);
```

### With Cancellation

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
byte[] dxfBytes = await converter.ConvertAsync(svgBytes, cts.Token);
```

## Supported Platforms

| Platform | Architecture | RID |
|----------|-------------|-----|
| Linux (glibc) | x64 | linux-x64 |
| Linux (glibc) | arm64 | linux-arm64 |
| Linux (musl/Alpine) | x64 | linux-musl-x64 |
| Linux (musl/Alpine) | arm64 | linux-musl-arm64 |
| macOS | Intel | osx-x64 |
| macOS | Apple Silicon | osx-arm64 |
| Windows | x64 | win-x64 |
| Windows | ARM64 | win-arm64 |

## Error Handling

```csharp
try
{
    byte[] dxfBytes = await converter.ConvertAsync(svgBytes);
}
catch (DxfConversionException ex)
{
    Console.WriteLine($"Conversion failed: {ex.Message}");
    Console.WriteLine($"Exit code: {ex.ExitCode}");
    Console.WriteLine($"Error output: {ex.ErrorOutput}");
}
```

## License

MIT License - see [LICENSE](LICENSE) for details.

## Credits

This library uses the `dxf_outlines` extension from [Inkscape Extensions](https://gitlab.com/inkscape/extensions), compiled to native binaries using [Nuitka](https://nuitka.net/).