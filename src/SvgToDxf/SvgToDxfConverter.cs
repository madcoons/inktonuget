using CliWrap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SvgToDxf;

/// <summary>
/// Converts SVG data to DXF format using the native dxf_outlines executable.
/// </summary>
public sealed class SvgToDxfConverter
{
    private static volatile bool s_isWarmedUp;

    private readonly string _executablePath;
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new instance of the SVG to DXF converter.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown when the current platform is not supported.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the native executable is not found.
    /// </exception>
    public SvgToDxfConverter()
        : this(RuntimeResolver.GetExecutablePath(), NullLogger.Instance)
    {
    }

    /// <summary>
    /// Creates a new instance of the SVG to DXF converter with a logger.
    /// </summary>
    /// <param name="logger">The logger to use for logging conversion output.</param>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown when the current platform is not supported.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the native executable is not found.
    /// </exception>
    public SvgToDxfConverter(ILogger logger)
        : this(RuntimeResolver.GetExecutablePath(), logger)
    {
    }

    /// <summary>
    /// Creates a new instance of the SVG to DXF converter with a custom executable path.
    /// </summary>
    /// <param name="executablePath">The full path to the dxf_outlines executable.</param>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the executable is not found at the specified path.
    /// </exception>
    public SvgToDxfConverter(string executablePath)
        : this(executablePath, NullLogger.Instance)
    {
    }

    /// <summary>
    /// Creates a new instance of the SVG to DXF converter with a custom executable path and logger.
    /// </summary>
    /// <param name="executablePath">The full path to the dxf_outlines executable.</param>
    /// <param name="logger">The logger to use for logging conversion output.</param>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the executable is not found at the specified path.
    /// </exception>
    public SvgToDxfConverter(string executablePath, ILogger logger)
    {
        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException(
                $"Executable not found at '{executablePath}'.",
                executablePath);
        }

        _executablePath = executablePath;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Converts SVG bytes to DXF bytes using default options.
    /// </summary>
    /// <param name="svgBytes">The SVG content as a byte array.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The DXF content as a byte array.</returns>
    /// <exception cref="DxfConversionException">
    /// Thrown when the conversion fails.
    /// </exception>
    public Task<byte[]> ConvertAsync(byte[] svgBytes, CancellationToken cancellationToken = default)
    {
        return ConvertAsync(svgBytes, DxfConversionOptions.Default, cancellationToken);
    }

    /// <summary>
    /// Converts SVG bytes to DXF bytes using specified options.
    /// </summary>
    /// <param name="svgBytes">The SVG content as a byte array.</param>
    /// <param name="options">The conversion options.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The DXF content as a byte array.</returns>
    /// <exception cref="DxfConversionException">
    /// Thrown when the conversion fails.
    /// </exception>
    public async Task<byte[]> ConvertAsync(
        byte[] svgBytes,
        DxfConversionOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(svgBytes);
        ArgumentNullException.ThrowIfNull(options);

        // Ensure warmup is complete before first conversion (Nuitka onefile extraction)
        await EnsureWarmedUpAsync(cancellationToken);

        using var outputStream = new MemoryStream();
        var stderrLines = new List<string>();

        _logger.LogDebug("Starting DXF conversion with executable: {ExecutablePath}", _executablePath);
        _logger.LogDebug("Conversion arguments: {Arguments}", string.Join(" ", options.ToArguments()));

        var command = Cli.Wrap(_executablePath)
            .WithArguments(options.ToArguments())
            .WithStandardInputPipe(PipeSource.FromBytes(svgBytes))
            .WithStandardOutputPipe(PipeTarget.ToStream(outputStream))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(line =>
            {
                stderrLines.Add(line);
                _logger.LogWarning("[dxf_outlines] {Line}", line);
            }))
            .WithValidation(CommandResultValidation.None);

        var result = await command.ExecuteAsync(cancellationToken);

        _logger.LogDebug("DXF conversion completed with exit code: {ExitCode}", result.ExitCode);

        if (result.ExitCode != 0)
        {
            var errorMessage = string.Join(Environment.NewLine, stderrLines);
            _logger.LogError("DXF conversion failed with exit code {ExitCode}: {ErrorMessage}", result.ExitCode, errorMessage);

            throw new DxfConversionException(
                $"DXF conversion failed with exit code {result.ExitCode}.",
                result.ExitCode,
                errorMessage);
        }

        return outputStream.ToArray();
    }

    /// <summary>
    /// Converts SVG stream to DXF bytes using default options.
    /// </summary>
    /// <param name="svgStream">The SVG content as a stream.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The DXF content as a byte array.</returns>
    /// <exception cref="DxfConversionException">
    /// Thrown when the conversion fails.
    /// </exception>
    public async Task<byte[]> ConvertAsync(Stream svgStream, CancellationToken cancellationToken = default)
    {
        return await ConvertAsync(svgStream, DxfConversionOptions.Default, cancellationToken);
    }

    /// <summary>
    /// Converts SVG stream to DXF bytes using specified options.
    /// </summary>
    /// <param name="svgStream">The SVG content as a stream.</param>
    /// <param name="options">The conversion options.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The DXF content as a byte array.</returns>
    /// <exception cref="DxfConversionException">
    /// Thrown when the conversion fails.
    /// </exception>
    public async Task<byte[]> ConvertAsync(
        Stream svgStream,
        DxfConversionOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(svgStream);

        using var memoryStream = new MemoryStream();
        await svgStream.CopyToAsync(memoryStream, cancellationToken);
        return await ConvertAsync(memoryStream.ToArray(), options, cancellationToken);
    }

    /// <summary>
    /// Ensures the native executable is warmed up (extracted from Nuitka onefile if needed).
    /// Uses a file lock to prevent race conditions when multiple processes start simultaneously.
    /// </summary>
    private async Task EnsureWarmedUpAsync(CancellationToken cancellationToken)
    {
        if (s_isWarmedUp)
            return;

        _logger.LogDebug("Warming up native executable (first run extraction)...");

        // Use a file lock for cross-process synchronization
        var lockPath = Path.Combine(Path.GetTempPath(), "dxf_outlines.lock");
        
        // Retry loop to acquire file lock
        FileStream? lockFile = null;
        while (lockFile == null)
        {
            try
            {
                lockFile = new FileStream(
                    lockPath,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None);
            }
            catch (IOException)
            {
                // Lock held by another process, wait and retry
                await Task.Delay(100, cancellationToken);
            }
        }

        try
        {
            if (s_isWarmedUp)
                return;

            // Run --help to trigger Nuitka onefile extraction without actual conversion
            var result = await Cli.Wrap(_executablePath)
                .WithArguments(["--help"])
                .WithValidation(CommandResultValidation.None)
                .ExecuteAsync(cancellationToken);

            _logger.LogDebug("Warmup completed with exit code: {ExitCode}", result.ExitCode);

            s_isWarmedUp = true;
        }
        finally
        {
            lockFile.Dispose();
        }
    }
}
