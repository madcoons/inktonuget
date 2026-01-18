using System.Runtime.InteropServices;

namespace SvgToDxf;

/// <summary>
/// Resolves the correct native executable based on the current runtime environment.
/// </summary>
internal static class RuntimeResolver
{
    private const string ExecutableBaseName = "dxf_outlines";

    /// <summary>
    /// Gets the executable name for the current runtime platform.
    /// </summary>
    /// <returns>The platform-specific executable name.</returns>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown when the current platform is not supported.
    /// </exception>
    public static string GetExecutableName()
    {
        var rid = GetCurrentRid();
        return $"{ExecutableBaseName}-{rid}";
    }

    /// <summary>
    /// Gets the full path to the executable in the application's base directory.
    /// </summary>
    /// <returns>The full path to the platform-specific executable.</returns>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown when the current platform is not supported.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the executable is not found at the expected path.
    /// </exception>
    public static string GetExecutablePath()
    {
        var executableName = GetExecutableName();
        var path = Path.Combine(AppContext.BaseDirectory, executableName);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"Native executable '{executableName}' not found at '{path}'. " +
                $"Ensure the SvgToDxf package includes the native binary for RID '{GetCurrentRid()}'.",
                path);
        }

        return path;
    }

    /// <summary>
    /// Gets the Runtime Identifier (RID) for the current platform.
    /// </summary>
    private static string GetCurrentRid()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var arch = RuntimeInformation.OSArchitecture switch
            {
                Architecture.X64 => "x64",
                Architecture.Arm64 => "arm64",
                _ => throw new PlatformNotSupportedException(
                    $"Unsupported architecture: {RuntimeInformation.OSArchitecture}. " +
                    "SvgToDxf supports x64 and arm64 architectures only.")
            };

            var libc = IsMusl() ? "musl-" : "";
            return $"linux-{libc}{arch}";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var arch = RuntimeInformation.OSArchitecture switch
            {
                Architecture.X64 => "x64",
                Architecture.Arm64 => "arm64",
                _ => throw new PlatformNotSupportedException(
                    $"Unsupported architecture: {RuntimeInformation.OSArchitecture}. " +
                    "SvgToDxf supports x64 and arm64 architectures only.")
            };
            return $"osx-{arch}";
        }
        else
        {
            throw new PlatformNotSupportedException(
                $"SvgToDxf is only supported on Linux and macOS. Current OS: {RuntimeInformation.OSDescription}");
        }
    }

    /// <summary>
    /// Detects if the current Linux system uses musl libc (e.g., Alpine Linux).
    /// </summary>
    private static bool IsMusl()
    {
        try
        {
            // Check for musl dynamic linker in /lib directory
            // Pattern: /lib/ld-musl-*.so*
            if (Directory.Exists("/lib"))
            {
                var muslFiles = Directory.GetFiles("/lib", "ld-musl-*.so*");
                if (muslFiles.Length > 0)
                {
                    return true;
                }
            }

            // Also check /lib64 for some distributions
            if (Directory.Exists("/lib64"))
            {
                var muslFiles = Directory.GetFiles("/lib64", "ld-musl-*.so*");
                if (muslFiles.Length > 0)
                {
                    return true;
                }
            }

            return false;
        }
        catch
        {
            // If we can't check, assume glibc
            return false;
        }
    }
}
