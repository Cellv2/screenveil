using ScreenVeil.Core.Models;

namespace ScreenVeil.Tests;

public sealed class OverlayGeometryTests
{
    [Theory]
    [InlineData(0, 320, 150)]
    [InlineData(90, 150, 320)]
    [InlineData(-90, 150, 320)]
    [InlineData(45, 333, 333)]
    public void GetWindowSizeContainsTheRotatedCover(double rotation, double expectedWidth, double expectedHeight)
    {
        var (width, height) = OverlayGeometry.GetWindowSize(320, 150, rotation);

        Assert.Equal(expectedWidth, width);
        Assert.Equal(expectedHeight, height);
    }

    [Fact]
    public void GetWindowSizeRejectsNonPositiveDimensions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => OverlayGeometry.GetWindowSize(0, 150, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => OverlayGeometry.GetWindowSize(320, double.NaN, 0));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(45, 1)]
    [InlineData(45, 1.5)]
    [InlineData(135, 2)]
    public void WindowAndCoverPositionsRoundTrip(double rotation, double renderScaling)
    {
        const double x = 80;
        const double y = 120;
        var windowPosition = OverlayGeometry.GetWindowPosition(x, y, 320, 150, rotation, renderScaling);
        var coverPosition = OverlayGeometry.GetCoverPosition(
            windowPosition.X,
            windowPosition.Y,
            320,
            150,
            rotation,
            renderScaling);

        Assert.Equal(x, coverPosition.X, 10);
        Assert.Equal(y, coverPosition.Y, 10);
    }
}
