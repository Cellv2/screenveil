using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia;
using ScreenVeil.Core.Models;

[assembly: InternalsVisibleTo("ScreenVeil.Tests")]

namespace ScreenVeil.App;

internal static class OverlayInputRegion
{
    public static IReadOnlyList<PixelPoint[]> GetPolygons(
        OverlayShape shape, double width, double height, double rotation, Size clientSize, double scaling)
    {
        var padding = 2 / scaling;
        var radians = rotation * Math.PI / 180;
        var cosine = Math.Cos(radians);
        var sine = Math.Sin(radians);
        var polygons = new List<PixelPoint[]>();

        PixelPoint Transform(double x, double y) => new(
            (int)Math.Round((clientSize.Width / 2 + x * cosine - y * sine) * scaling),
            (int)Math.Round((clientSize.Height / 2 + x * sine + y * cosine) * scaling));

        void Rectangle(double x, double y, double rectangleWidth, double rectangleHeight)
        {
            var left = x - rectangleWidth / 2 - padding;
            var right = x + rectangleWidth / 2 + padding;
            var top = y - rectangleHeight / 2 - padding;
            var bottom = y + rectangleHeight / 2 + padding;
            polygons.Add([Transform(left, top), Transform(right, top), Transform(right, bottom), Transform(left, bottom)]);
        }

        if (shape == OverlayShape.Box)
        {
            Rectangle(0, 0, width, height);
        }
        else if (shape == OverlayShape.Oval)
        {
            const int pointCount = 128;
            var points = new PixelPoint[pointCount];
            var factor = 1 / Math.Cos(Math.PI / pointCount);
            for (var i = 0; i < pointCount; i++)
            {
                var angle = 2 * Math.PI * i / pointCount;
                points[i] = Transform(
                    (width / 2 + padding) * factor * Math.Cos(angle),
                    (height / 2 + padding) * factor * Math.Sin(angle));
            }

            polygons.Add(points);
            // These rectangles match the oval's resize and rotation controls in OverlayWindow.axaml.
            Rectangle(-width / 2 + 5, 0, 10, 28);
            Rectangle(width / 2 - 5, 0, 10, 28);
            Rectangle(0, -height / 2 + 5, 28, 10);
            Rectangle(0, height / 2 - 5, 28, 10);
            Rectangle(-width * 0.35, -height * 0.35, 14, 14);
            Rectangle(width * 0.35, -height * 0.35, 14, 14);
            Rectangle(-width * 0.35, height * 0.35, 14, 14);
            Rectangle(width * 0.35, height * 0.35, 22, 22);
            Rectangle(0, -height / 2 + 25, 22, 22);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(shape));
        }

        return polygons;
    }

    internal static void Apply(nint window, IReadOnlyList<PixelPoint[]> polygons)
    {
        nint region = 0;
        try
        {
            foreach (var polygon in polygons)
            {
                var part = CreatePolygonRgn(polygon, polygon.Length, 2);
                if (part == 0)
                {
                    throw new InvalidOperationException("Failed to create a Windows cover region.");
                }

                if (region == 0)
                {
                    region = part;
                    continue;
                }

                try
                {
                    if (CombineRgn(region, region, part, 2) == 0)
                    {
                        throw new InvalidOperationException("Failed to combine Windows cover regions.");
                    }
                }
                finally
                {
                    DeleteObject(part);
                }
            }

            if (SetWindowRgn(window, region, true) == 0)
            {
                throw new InvalidOperationException("Failed to apply the Windows cover region.");
            }

            // Windows owns the region after SetWindowRgn succeeds.
            region = 0;
        }
        finally
        {
            if (region != 0)
            {
                DeleteObject(region);
            }
        }
    }

    [DllImport("gdi32.dll")]
    private static extern nint CreatePolygonRgn([In] PixelPoint[] points, int count, int fillMode);

    [DllImport("gdi32.dll")]
    private static extern int CombineRgn(nint destination, nint first, nint second, int mode);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(nint handle);

    [DllImport("user32.dll")]
    private static extern int SetWindowRgn(nint window, nint region, [MarshalAs(UnmanagedType.Bool)] bool redraw);
}
