namespace SvgToDxf;

/// <summary>
/// Exception thrown when DXF conversion fails.
/// </summary>
public sealed class DxfConversionException : Exception
{
    /// <summary>
    /// Gets the exit code from the dxf_outlines executable.
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// Gets the standard error output from the dxf_outlines executable.
    /// </summary>
    public string StandardError { get; }

    /// <summary>
    /// Creates a new instance of the DXF conversion exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="exitCode">The exit code from the executable.</param>
    /// <param name="standardError">The standard error output.</param>
    public DxfConversionException(string message, int exitCode, string standardError)
        : base(message)
    {
        ExitCode = exitCode;
        StandardError = standardError;
    }

    /// <summary>
    /// Creates a new instance of the DXF conversion exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="exitCode">The exit code from the executable.</param>
    /// <param name="standardError">The standard error output.</param>
    /// <param name="innerException">The inner exception.</param>
    public DxfConversionException(string message, int exitCode, string standardError, Exception innerException)
        : base(message, innerException)
    {
        ExitCode = exitCode;
        StandardError = standardError;
    }
}
