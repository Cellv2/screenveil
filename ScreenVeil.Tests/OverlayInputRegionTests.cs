using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Avalonia;
using ScreenVeil.App;
using ScreenVeil.Core.Models;

namespace ScreenVeil.Tests;

public sealed class OverlayInputRegionTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(1.5)]
    public void RotatedBoxRegionTracksTheCover(double scaling)
    {
        var size = new Size(356, 356);
        var unrotated = OverlayInputRegion.GetPolygons(OverlayShape.Box, 320, 150, 0, size, scaling);
        var rotated = OverlayInputRegion.GetPolygons(OverlayShape.Box, 320, 150, 90, size, scaling);

        Assert.True(Contains(unrotated, 308 * scaling, 178 * scaling));
        Assert.False(Contains(rotated, 308 * scaling, 178 * scaling));
        Assert.True(Contains(rotated, 178 * scaling, 308 * scaling));
        Assert.False(Contains(rotated, 20 * scaling, 20 * scaling));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(45, 1.5)]
    public void OvalRegionIncludesResizeHandlesButNotEmptyCorners(double rotation, double scaling)
    {
        var region = OverlayInputRegion.GetPolygons(OverlayShape.Oval, 320, 150, rotation, new Size(356, 356), scaling);
        var radians = rotation * Math.PI / 180;
        (double X, double Y) Transform(double x, double y) => (
            (178 + x * Math.Cos(radians) - y * Math.Sin(radians)) * scaling,
            (178 + x * Math.Sin(radians) + y * Math.Cos(radians)) * scaling);

        var handle = Transform(112, 52.5);
        var emptyCorner = Transform(142, 62);
        Assert.True(Contains(region, 178 * scaling, 178 * scaling));
        Assert.True(Contains(region, handle.X, handle.Y));
        Assert.False(Contains(region, emptyCorner.X, emptyCorner.Y));
    }

    [Fact]
    public void WindowsCoverUpdatesItsRegionDuringRotationAndShapeChanges()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var desktop = CreateDesktop("ScreenVeilTest" + Guid.NewGuid().ToString("N"), 0, 0, 0, 0x10000000, 0);
        Assert.NotEqual(nint.Zero, desktop);
        ExceptionDispatchInfo? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                Assert.True(SetThreadDesktop(desktop), $"SetThreadDesktop failed: {Marshal.GetLastWin32Error()}");
                AppBuilder.Configure<ScreenVeil.App.App>().UsePlatformDetect().SetupWithoutStarting();
                var region = CreateRectRgn(0, 0, 0, 0);
                Assert.NotEqual(nint.Zero, region);
                try
                {
                    var overlay = new OverlayItemViewModel(new OverlayConfiguration(), _ => { }, _ => { });
                    var window = new OverlayWindow(overlay)
                    {
                        Width = overlay.WindowWidth,
                        Height = overlay.WindowHeight,
                        Position = new PixelPoint(100, 100)
                    };
                    try
                    {
                        window.Show();
                        var handle = window.TryGetPlatformHandle()!.Handle;
                        var scaling = window.RenderScaling;
                        var initialSize = window.ClientSize;
                        var initialPosition = window.Position;
                        bool ContainsPoint(double x, double y) => PtInRegion(region,
                            (int)Math.Round(x * scaling), (int)Math.Round(y * scaling));

                        Assert.NotEqual(0, GetWindowRgn(handle, region));
                        Assert.True(ContainsPoint(178, 178));
                        Assert.False(ContainsPoint(20, 20));
                        Assert.True(ContainsPoint(308, 178));
                        overlay.PreviewRotation(90);
                        Assert.NotEqual(0, GetWindowRgn(handle, region));
                        Assert.False(ContainsPoint(308, 178));
                        Assert.True(ContainsPoint(178, 308));
                        overlay.SetRotation(90);
                        Assert.Equal(initialSize, window.ClientSize);
                        Assert.Equal(initialPosition, window.Position);

                        overlay.SelectedShape = OverlayShape.Oval;
                        overlay.PreviewRotation(0);
                        Assert.NotEqual(0, GetWindowRgn(handle, region));
                        Assert.False(ContainsPoint(320, 240));
                        Assert.True(ContainsPoint(290, 230));
                    }
                    finally
                    {
                        window.Close();
                    }
                }
                finally
                {
                    DeleteObject(region);
                }
            }
            catch (Exception exception)
            {
                failure = ExceptionDispatchInfo.Capture(exception);
            }
        })
        { IsBackground = true };
        try
        {
            thread.Start();
            Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "Windows GUI test timed out.");
            failure?.Throw();
        }
        finally
        {
            CloseDesktop(desktop);
        }
    }

    [Fact]
    public void WindowsRegionUpdatesWhenCoverRotates()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var window = CreateWindowEx(0, "STATIC", "", 0x80000000, 0, 0, 356, 356, 0, 0, 0, 0);
        Assert.NotEqual(nint.Zero, window);
        nint region = 0;
        try
        {
            region = CreateRectRgn(0, 0, 0, 0);
            Assert.NotEqual(nint.Zero, region);
            OverlayInputRegion.Apply(window, OverlayInputRegion.GetPolygons(
                OverlayShape.Box, 320, 150, 0, new Size(356, 356), 1));
            Assert.NotEqual(0, GetWindowRgn(window, region));
            Assert.True(PtInRegion(region, 308, 178));
            Assert.False(PtInRegion(region, 20, 20));

            OverlayInputRegion.Apply(window, OverlayInputRegion.GetPolygons(
                OverlayShape.Box, 320, 150, 90, new Size(356, 356), 1));
            Assert.NotEqual(0, GetWindowRgn(window, region));
            Assert.False(PtInRegion(region, 308, 178));
            Assert.True(PtInRegion(region, 178, 308));

            OverlayInputRegion.Apply(window, OverlayInputRegion.GetPolygons(
                OverlayShape.Oval, 320, 150, 0, new Size(356, 356), 1));
            Assert.NotEqual(0, GetWindowRgn(window, region));
            Assert.True(PtInRegion(region, 290, 230));
            Assert.False(PtInRegion(region, 320, 240));
        }
        finally
        {
            if (region != 0)
            {
                DeleteObject(region);
            }
            DestroyWindow(window);
        }
    }

    private static bool Contains(IReadOnlyList<PixelPoint[]> polygons, double x, double y)
    {
        foreach (var polygon in polygons)
        {
            var inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                var a = polygon[i];
                var b = polygon[j];
                if ((a.Y > y) != (b.Y > y) && x < (double)(b.X - a.X) * (y - a.Y) / (b.Y - a.Y) + a.X)
                {
                    inside = !inside;
                }
            }

            if (inside)
            {
                return true;
            }
        }

        return false;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint CreateWindowEx(uint exStyle, string className, string title, uint style,
        int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint CreateDesktop(string name, nint device, nint deviceMode, uint flags, uint access, nint attributes);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetThreadDesktop(nint desktop);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseDesktop(nint desktop);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(nint window);

    [DllImport("user32.dll")]
    private static extern int GetWindowRgn(nint window, nint region);

    [DllImport("gdi32.dll")]
    private static extern nint CreateRectRgn(int left, int top, int right, int bottom);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PtInRegion(nint region, int x, int y);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(nint handle);
}
