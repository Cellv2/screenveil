using ScreenVeil.Core.Models;

namespace ScreenVeil.Core.Services;

public sealed class OverlayWorkspace
{
    private readonly List<OverlayConfiguration> _overlays = [];

    public IReadOnlyList<OverlayConfiguration> Overlays => _overlays;

    public bool AreAllVisible => _overlays.Count > 0 && _overlays.All(overlay => overlay.IsVisible);

    public OverlayConfiguration Add()
    {
        var index = 1;
        while (_overlays.Any(overlay => string.Equals(overlay.Name, $"Cover {index}", StringComparison.OrdinalIgnoreCase)))
        {
            index++;
        }

        var overlay = OverlayConfiguration.Create(index);
        _overlays.Add(overlay);
        return overlay;
    }

    public bool Update(OverlayConfiguration overlay)
    {
        var index = _overlays.FindIndex(item => item.Id == overlay.Id);
        if (index < 0)
        {
            return false;
        }

        _overlays[index] = overlay;
        return true;
    }

    public bool Remove(Guid id)
    {
        var index = _overlays.FindIndex(overlay => overlay.Id == id);
        if (index < 0)
        {
            return false;
        }

        _overlays.RemoveAt(index);
        return true;
    }

    public bool SetVisibility(Guid id, bool isVisible)
    {
        var index = _overlays.FindIndex(overlay => overlay.Id == id);
        if (index < 0)
        {
            return false;
        }

        _overlays[index] = _overlays[index] with { IsVisible = isVisible };
        return true;
    }

    public void ToggleAllVisibility()
    {
        var shouldShow = !_overlays.Any(overlay => overlay.IsVisible);
        for (var index = 0; index < _overlays.Count; index++)
        {
            _overlays[index] = _overlays[index] with { IsVisible = shouldShow };
        }
    }
}