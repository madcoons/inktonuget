namespace SvgToDxf;

/// <summary>
/// Options for DXF conversion matching dxf_outlines executable arguments.
/// </summary>
public sealed record DxfConversionOptions
{
    /// <summary>
    /// Use LWPOLYLINE output instead of LINE segments.
    /// Corresponds to --POLY / -P flag.
    /// </summary>
    public bool UsePolyline { get; init; } = false;

    /// <summary>
    /// Flatten Bezier curves to line segments.
    /// Corresponds to --FLATTENBEZ / -F flag.
    /// </summary>
    public bool FlattenBeziers { get; init; } = false;

    /// <summary>
    /// ROBO-Master compatible spline output.
    /// Corresponds to --ROBO / -R flag.
    /// </summary>
    public bool RoboMaster { get; init; } = false;

    /// <summary>
    /// Output units. Valid values: px, in, ft, mm, cm, m.
    /// Corresponds to --units argument.
    /// </summary>
    public string Units { get; init; } = "px";

    /// <summary>
    /// Use units from the SVG document instead of the Units property.
    /// Corresponds to --unit_from_document flag.
    /// </summary>
    public bool UnitFromDocument { get; init; } = true;

    /// <summary>
    /// Character encoding for the DXF output.
    /// Corresponds to --encoding argument.
    /// </summary>
    public string Encoding { get; init; } = "latin_1";

    /// <summary>
    /// Default options for DXF conversion.
    /// </summary>
    public static DxfConversionOptions Default => new();

    /// <summary>
    /// Builds command line arguments for the dxf_outlines executable.
    /// </summary>
    internal IEnumerable<string> ToArguments()
    {
        yield return "--POLY";
        yield return UsePolyline ? "true" : "false";

        yield return "--FLATTENBEZ";
        yield return FlattenBeziers ? "true" : "false";

        yield return "--ROBO";
        yield return RoboMaster ? "true" : "false";

        yield return "--unit_from_document";
        yield return UnitFromDocument ? "true" : "false";

        if (!UnitFromDocument)
        {
            yield return "--units";
            yield return Units;
        }

        yield return "--encoding";
        yield return Encoding;
    }
}
