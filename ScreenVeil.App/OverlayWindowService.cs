using Avalonia;
using Avalonia.Controls;
using ScreenVeil.Core.Models;

namespace ScreenVeil.App;

public sealed class OverlayWindowService
{
    private readonly Dictionary<Guid, OverlayWindow> _windows = [];

    public void Create(OverlayItemViewModel overlay)
    {
        var window = new OverlayWindow(overlay);
        _windows.Add(overlay.Id, window);
        window.PositionChanged += (_, _) =>
        {
            if (window.ExpectedProgrammaticPosition is { } expectedPosition)
            {
                window.ExpectedProgrammaticPosition = null;
                if (window.Position == expectedPosition)
                {
                    return;
                }
            }

            overlay.UpdatePosition(window.Position, window.RenderScaling);
        };
        Update(overlay);
    }

    public void Update(OverlayItemViewModel overlay)
    {
        if (!_windows.TryGetValue(overlay.Id, out var window))
        {
            return;
        }

        window.Title = overlay.Name;
        window.Width = overlay.WindowWidth;
        window.Height = overlay.WindowHeight;

        var scaling = window.RenderScaling;
        var windowPosition = OverlayGeometry.GetWindowPosition(
            overlay.X,
            overlay.Y,
            overlay.Width,
            overlay.Height,
            scaling);
        var position = new PixelPoint(
            (int)Math.Round(windowPosition.X),
            (int)Math.Round(windowPosition.Y));
        SetWindowPosition(window, position);

        if (window.IsVisible != overlay.IsVisible)
        {
            if (overlay.IsVisible)
            {
                window.Show();
            }
            else
            {
                window.Hide();
            }
        }
    }

    public void Remove(Guid id)
    {
        if (_windows.Remove(id, out var window))
        {
            window.Close();
        }
    }

    public void CloseAll()
    {
        foreach (var window in _windows.Values)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private static void SetWindowPosition(OverlayWindow window, PixelPoint position)
    {
        if (window.Position == position)
        {
            return;
        }

        window.ExpectedProgrammaticPosition = position;
        window.Position = position;
    }
}