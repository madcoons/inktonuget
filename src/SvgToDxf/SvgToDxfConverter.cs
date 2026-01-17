using CliWrap;

namespace SvgToDxf;

/// <summary>
/// Converts SVG data to DXF format using the native dxf_outlines executable.
/// </summary>
public sealed class SvgToDxfConverter
{
    private readonly string _executablePath;

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
    {
        _executablePath = RuntimeResolver.GetExecutablePath();
    }

    /// <summary>
    /// Creates a new instance of the SVG to DXF converter with a custom executable path.
    /// </summary>
    /// <param name="executablePath">The full path to the dxf_outlines executable.</param>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the executable is not found at the specified path.
    /// </exception>
    public SvgToDxfConverter(string executablePath)
    {
        if (!File.Exists(executablePath))
        {
            throw new FileNotFoundException(
                $"Executable not found at '{executablePath}'.",
                executablePath);
        }

        _executablePath = executablePath;
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

        using var outputStream = new MemoryStream();
        using var errorStream = new MemoryStream();

        var command = Cli.Wrap(_executablePath)
            .WithArguments(options.ToArguments())
            .WithStandardInputPipe(PipeSource.FromBytes(svgBytes))
            .WithStandardOutputPipe(PipeTarget.ToStream(outputStream))
            .WithStandardErrorPipe(PipeTarget.ToStream(errorStream))
            .WithValidation(CommandResultValidation.None);

        var result = await command.ExecuteAsync(cancellationToken);

        if (result.ExitCode != 0)
        {
            errorStream.Position = 0;
            using var reader = new StreamReader(errorStream);
            var errorMessage = await reader.ReadToEndAsync(cancellationToken);

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
}
