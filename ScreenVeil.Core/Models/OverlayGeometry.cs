namespace ScreenVeil.Core.Models;

public static class OverlayGeometry
{
    public static (double X, double Y) GetWindowPosition(
        double x,
        double y,
        double width,
        double height,
        double rotation,
        double renderScaling)
    {
        ValidateRenderScaling(renderScaling);
        var (windowWidth, windowHeight) = GetWindowSize(width, height, rotation);
        return (
            x + (width - windowWidth) * renderScaling / 2,
            y + (height - windowHeight) * renderScaling / 2);
    }

    public static (double X, double Y) GetCoverPosition(
        double windowX,
        double windowY,
        double width,
        double height,
        double rotation,
        double renderScaling)
    {
        ValidateRenderScaling(renderScaling);
        var (windowWidth, windowHeight) = GetWindowSize(width, height, rotation);
        return (
            windowX + (windowWidth - width) * renderScaling / 2,
            windowY + (windowHeight - height) * renderScaling / 2);
    }

    public static (double Width, double Height) GetWindowSize(double width, double height, double rotation)
    {
        if (!double.IsFinite(width) || width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        if (!double.IsFinite(height) || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height));
        }

        if (!double.IsFinite(rotation))
        {
            throw new ArgumentOutOfRangeException(nameof(rotation));
        }

        var radians = rotation * Math.PI / 180;
        var absoluteCosine = SnapUnit(Math.Abs(Math.Cos(radians)));
        var absoluteSine = SnapUnit(Math.Abs(Math.Sin(radians)));

        return (
            Math.Ceiling(width * absoluteCosine + height * absoluteSine),
            Math.Ceiling(width * absoluteSine + height * absoluteCosine));
    }

    private static double SnapUnit(double value) =>
        value < 1e-12 ? 0 : Math.Abs(1 - value) < 1e-12 ? 1 : value;

    private static void ValidateRenderScaling(double renderScaling)
    {
        if (!double.IsFinite(renderScaling) || renderScaling <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(renderScaling));
        }
    }
}
