using ScreenVeil.Core.Models;

namespace ScreenVeil.Tests;

public sealed class OverlayGeometryTests
{
    [Theory]
    [InlineData(320, 150)]
    [InlineData(100, 400)]
    [InlineData(100, 100)]
    public void WindowContainsCoverAtEveryRotation(double width, double height)
    {
        var windowSize = OverlayGeometry.GetWindowSize(width, height);

        for (var rotation = 0; rotation < 360; rotation++)
        {
            var radians = rotation * Math.PI / 180;
            var rotatedWidth = width * Math.Abs(Math.Cos(radians)) + height * Math.Abs(Math.Sin(radians));
            var rotatedHeight = width * Math.Abs(Math.Sin(radians)) + height * Math.Abs(Math.Cos(radians));
            Assert.True(windowSize >= rotatedWidth);
            Assert.True(windowSize >= rotatedHeight);
        }
    }

    [Fact]
    public void GetWindowSizeRejectsNonPositiveDimensions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => OverlayGeometry.GetWindowSize(0, 150));
        Assert.Throws<ArgumentOutOfRangeException>(() => OverlayGeometry.GetWindowSize(320, double.NaN));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1.5)]
    [InlineData(2)]
    public void WindowAndCoverPositionsRoundTrip(double renderScaling)
    {
        const double x = 80;
        const double y = 120;
        var windowPosition = OverlayGeometry.GetWindowPosition(x, y, 320, 150, renderScaling);
        var coverPosition = OverlayGeometry.GetCoverPosition(
            windowPosition.X,
            windowPosition.Y,
            320,
            150,
            renderScaling);

        Assert.Equal(x, coverPosition.X, 10);
        Assert.Equal(y, coverPosition.Y, 10);
    }
}
