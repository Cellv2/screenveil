using Avalonia;
using Avalonia.Controls;
using ScreenVeil.Core.Models;

namespace ScreenVeil.App;

public sealed class OverlayWindowService
{
    private readonly Dictionary<Guid, OverlayWindow> _windows = [];

    public void Create(OverlayItemViewModel overlay)
    {
        var window = new OverlayWindow(overlay, BeginRotation, EndRotation);
        _windows.Add(overlay.Id, window);
        window.PositionChanged += (_, _) =>
        {
            if (window.IsRotationGestureInProgress)
            {
                return;
            }

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
            overlay.Rotation,
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

    private void BeginRotation(OverlayItemViewModel overlay)
    {
        if (!_windows.TryGetValue(overlay.Id, out var window))
        {
            return;
        }

        window.IsRotationGestureInProgress = true;
        var scaling = window.RenderScaling;
        var centerX = window.Position.X / scaling + window.Width / 2;
        var centerY = window.Position.Y / scaling + window.Height / 2;
        var size = Math.Ceiling(Math.Sqrt(overlay.Width * overlay.Width + overlay.Height * overlay.Height)) + 2;
        window.Width = size;
        window.Height = size;
        SetWindowPosition(
            window,
            new PixelPoint(
                (int)Math.Round((centerX - size / 2) * scaling),
                (int)Math.Round((centerY - size / 2) * scaling)));
    }

    private void EndRotation(OverlayItemViewModel overlay)
    {
        if (!_windows.TryGetValue(overlay.Id, out var window))
        {
            return;
        }

        Update(overlay);
        window.IsRotationGestureInProgress = false;
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