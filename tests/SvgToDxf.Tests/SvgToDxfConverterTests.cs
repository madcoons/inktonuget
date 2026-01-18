using Microsoft.Extensions.Logging;
using TUnit.Core;
using TUnit.Core.Logging;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;

namespace SvgToDxf.Tests;

public class SvgToDxfConverterTests
{
    private static Microsoft.Extensions.Logging.ILogger CreateLogger() => new TUnitLoggerAdapter(TestContext.Current!.GetDefaultLogger());

    [Test]
    public async Task ConvertAsync_WithValidSvg_ReturnsDxfBytes()
    {
        // Arrange
        var svgContent = """
            <?xml version="1.0" encoding="UTF-8"?>
            <svg xmlns="http://www.w3.org/2000/svg" width="100" height="100">
                <rect x="10" y="10" width="80" height="80" fill="none" stroke="black"/>
            </svg>
            """;
        var svgBytes = System.Text.Encoding.UTF8.GetBytes(svgContent);
        var converter = new SvgToDxfConverter(CreateLogger());

        // Act
        var dxfBytes = await converter.ConvertAsync(svgBytes);

        // Assert
        await Assert.That(dxfBytes).IsNotNull();
        await Assert.That(dxfBytes.Length).IsGreaterThan(0);
        
        var dxfContent = System.Text.Encoding.UTF8.GetString(dxfBytes);
        await Assert.That(dxfContent).Contains("SECTION");
        await Assert.That(dxfContent).Contains("ENTITIES");
    }

    [Test]
    public async Task ConvertAsync_WithOptions_AppliesOptions()
    {
        // Arrange
        var svgContent = """
            <?xml version="1.0" encoding="UTF-8"?>
            <svg xmlns="http://www.w3.org/2000/svg" width="100" height="100">
                <circle cx="50" cy="50" r="40" fill="none" stroke="black"/>
            </svg>
            """;
        var svgBytes = System.Text.Encoding.UTF8.GetBytes(svgContent);
        var converter = new SvgToDxfConverter(CreateLogger());
        var options = new DxfConversionOptions
        {
            UsePolyline = true,
            Units = "mm"
        };

        // Act
        var dxfBytes = await converter.ConvertAsync(svgBytes, options);

        // Assert
        await Assert.That(dxfBytes).IsNotNull();
        await Assert.That(dxfBytes.Length).IsGreaterThan(0);
    }

    [Test]
    public async Task ConvertAsync_WithStream_ReturnsDxfBytes()
    {
        // Arrange
        var svgContent = """
            <?xml version="1.0" encoding="UTF-8"?>
            <svg xmlns="http://www.w3.org/2000/svg" width="100" height="100">
                <line x1="0" y1="0" x2="100" y2="100" stroke="black"/>
            </svg>
            """;
        using var svgStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(svgContent));
        var converter = new SvgToDxfConverter(CreateLogger());

        // Act
        var dxfBytes = await converter.ConvertAsync(svgStream);

        // Assert
        await Assert.That(dxfBytes).IsNotNull();
        await Assert.That(dxfBytes.Length).IsGreaterThan(0);
    }

    [Test]
    public async Task ConvertAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        var svgContent = """
            <?xml version="1.0" encoding="UTF-8"?>
            <svg xmlns="http://www.w3.org/2000/svg" width="100" height="100">
                <rect x="10" y="10" width="80" height="80" fill="none" stroke="black"/>
            </svg>
            """;
        var svgBytes = System.Text.Encoding.UTF8.GetBytes(svgContent);
        var converter = new SvgToDxfConverter(CreateLogger());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.That(async () => await converter.ConvertAsync(svgBytes, cts.Token))
            .Throws<OperationCanceledException>();
    }

    [Test]
    public async Task ConvertAsync_WithNullBytes_ThrowsArgumentNullException()
    {
        // Arrange
        var converter = new SvgToDxfConverter(CreateLogger());

        // Act & Assert
        await Assert.That(async () => await converter.ConvertAsync((byte[])null!))
            .Throws<ArgumentNullException>();
    }
}

/// <summary>
/// Adapter to bridge TUnit's DefaultLogger to Microsoft.Extensions.Logging.ILogger.
/// </summary>
file class TUnitLoggerAdapter(DefaultLogger tunitLogger) : Microsoft.Extensions.Logging.ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

    public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        tunitLogger.LogInformation(message);
        if (exception != null)
        {
            tunitLogger.LogInformation(exception.ToString());
        }
    }
}
