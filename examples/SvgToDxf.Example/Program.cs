using System.Text;
using SvgToDxf;

// Example 1: Basic conversion with default options
Console.WriteLine("=== SvgToDxf Example ===\n");

var converter = new SvgToDxfConverter();

// Example SVG content
var svgContent = """
    <svg xmlns="http://www.w3.org/2000/svg" width="100" height="100">
        <rect x="10" y="10" width="80" height="80" fill="none" stroke="black" stroke-width="1"/>
        <circle cx="50" cy="50" r="30" fill="none" stroke="black" stroke-width="1"/>
    </svg>
    """;

try
{
    // Convert SVG string to DXF (API uses byte arrays)
    var svgBytes = Encoding.UTF8.GetBytes(svgContent);
    var dxfBytes = await converter.ConvertAsync(svgBytes);
    var dxfContent = Encoding.UTF8.GetString(dxfBytes);
    
    Console.WriteLine("Conversion successful!");
    Console.WriteLine($"DXF output length: {dxfContent.Length} characters\n");
    
    // Save to file
    var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "output.dxf");
    await File.WriteAllTextAsync(outputPath, dxfContent);
    Console.WriteLine($"DXF saved to: {outputPath}");
}
catch (DxfConversionException ex)
{
    Console.WriteLine($"Conversion failed: {ex.Message}");
}

// Example 2: Conversion with custom options
Console.WriteLine("\n=== Conversion with custom options ===\n");

var options = new DxfConversionOptions
{
    Units = "mm",
    UsePolyline = true
};

try
{
    var svgBytes = Encoding.UTF8.GetBytes(svgContent);
    var dxfBytes = await converter.ConvertAsync(svgBytes, options);
    Console.WriteLine($"Conversion with options successful! Output: {dxfBytes.Length} bytes");
}
catch (DxfConversionException ex)
{
    Console.WriteLine($"Conversion failed: {ex.Message}");
}

// Example 3: Convert from file
Console.WriteLine("\n=== Convert from file ===\n");

// Use the sample.svg file included with the example
var sampleSvgPath = Path.Combine(AppContext.BaseDirectory, "sample.svg");

try
{
    var svgBytes = await File.ReadAllBytesAsync(sampleSvgPath);
    var dxfBytes = await converter.ConvertAsync(svgBytes);
    Console.WriteLine($"File conversion successful! Output: {dxfBytes.Length} bytes");
    
    // Save the converted file
    var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "sample-output.dxf");
    await File.WriteAllBytesAsync(outputPath, dxfBytes);
    Console.WriteLine($"DXF saved to: {outputPath}");
}
catch (DxfConversionException ex)
{
    Console.WriteLine($"Conversion failed: {ex.Message}");
}
