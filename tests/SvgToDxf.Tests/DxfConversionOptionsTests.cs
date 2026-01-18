using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;

namespace SvgToDxf.Tests;

public class DxfConversionOptionsTests
{
    [Test]
    public async Task Default_ReturnsExpectedValues()
    {
        // Arrange & Act
        var options = DxfConversionOptions.Default;

        // Assert
        await Assert.That(options.UsePolyline).IsFalse();
        await Assert.That(options.FlattenBeziers).IsFalse();
        await Assert.That(options.RoboMaster).IsFalse();
        await Assert.That(options.Units).IsEqualTo("px");
        await Assert.That(options.UnitFromDocument).IsTrue();
        await Assert.That(options.Encoding).IsEqualTo("latin_1");
    }

    [Test]
    public async Task ToArguments_WithDefaults_IncludesUnitFromDocument()
    {
        // Arrange
        var options = DxfConversionOptions.Default;

        // Act
        var args = options.ToArguments().ToList();

        // Assert
        await Assert.That(args).Contains("--unit_from_document");
        await Assert.That(args).Contains("true");
        await Assert.That(args).Contains("--encoding");
        await Assert.That(args).Contains("latin_1");
    }

    [Test]
    public async Task ToArguments_WithUsePolyline_IncludesPolyFlag()
    {
        // Arrange
        var options = new DxfConversionOptions { UsePolyline = true };

        // Act
        var args = options.ToArguments().ToList();

        // Assert
        await Assert.That(args).Contains("--POLY");
        var polyIndex = args.IndexOf("--POLY");
        await Assert.That(args[polyIndex + 1]).IsEqualTo("true");
    }

    [Test]
    public async Task ToArguments_WithFlattenBeziers_IncludesFlattenFlag()
    {
        // Arrange
        var options = new DxfConversionOptions { FlattenBeziers = true };

        // Act
        var args = options.ToArguments().ToList();

        // Assert
        await Assert.That(args).Contains("--FLATTENBEZ");
        var flattenIndex = args.IndexOf("--FLATTENBEZ");
        await Assert.That(args[flattenIndex + 1]).IsEqualTo("true");
    }

    [Test]
    public async Task ToArguments_WithRoboMaster_IncludesRoboFlag()
    {
        // Arrange
        var options = new DxfConversionOptions { RoboMaster = true };

        // Act
        var args = options.ToArguments().ToList();

        // Assert
        await Assert.That(args).Contains("--ROBO");
        var roboIndex = args.IndexOf("--ROBO");
        await Assert.That(args[roboIndex + 1]).IsEqualTo("true");
    }

    [Test]
    public async Task ToArguments_WithCustomUnits_IncludesUnitsArg()
    {
        // Arrange
        var options = new DxfConversionOptions 
        { 
            UnitFromDocument = false, 
            Units = "mm" 
        };

        // Act
        var args = options.ToArguments().ToList();

        // Assert
        await Assert.That(args).Contains("--units");
        await Assert.That(args).Contains("mm");
        await Assert.That(args).Contains("--unit_from_document");
        await Assert.That(args).Contains("false");
    }
}
