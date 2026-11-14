namespace ScreenVeil.Core.Models;

public static class OverlayGeometry
{
    public static (double X, double Y) GetWindowPosition(
        double x,
        double y,
        double width,
        double height,
        double renderScaling)
    {
        ValidateRenderScaling(renderScaling);
        var windowSize = GetWindowSize(width, height);
        return (
            x + (width - windowSize) * renderScaling / 2,
            y + (height - windowSize) * renderScaling / 2);
    }

    public static (double X, double Y) GetCoverPosition(
        double windowX,
        double windowY,
        double width,
        double height,
        double renderScaling)
    {
        ValidateRenderScaling(renderScaling);
        var windowSize = GetWindowSize(width, height);
        return (
            windowX + (windowSize - width) * renderScaling / 2,
            windowY + (windowSize - height) * renderScaling / 2);
    }

    public static double GetWindowSize(double width, double height)
    {
        if (!double.IsFinite(width) || width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        if (!double.IsFinite(height) || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height));
        }

        // A diagonal-sized surface contains the cover at every angle without resizing during rotation.
        return Math.Ceiling(Math.Sqrt(width * width + height * height)) + 2;
    }

    private static void ValidateRenderScaling(double renderScaling)
    {
        if (!double.IsFinite(renderScaling) || renderScaling <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(renderScaling));
        }
    }
}
