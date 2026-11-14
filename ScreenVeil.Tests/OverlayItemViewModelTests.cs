using ScreenVeil.App;
using ScreenVeil.Core.Models;

namespace ScreenVeil.Tests;

public sealed class OverlayItemViewModelTests
{
    [Fact]
    public void ChangingShapeUpdatesConfigurationAndNotifiesWithoutChangingGeometry()
    {
        var changes = new List<string?>();
        var changeCount = 0;
        var configuration = new OverlayConfiguration { Width = 200, Height = 120, Rotation = 30 };
        var overlay = new OverlayItemViewModel(configuration, _ => changeCount++, _ => { });
        overlay.PropertyChanged += (_, e) => changes.Add(e.PropertyName);

        Assert.True(overlay.IsBox);
        Assert.False(overlay.IsOval);

        overlay.SelectedShape = OverlayShape.Oval;

        Assert.Equal(configuration with { Shape = OverlayShape.Oval }, overlay.Configuration);
        Assert.False(overlay.IsBox);
        Assert.True(overlay.IsOval);
        Assert.Contains(nameof(overlay.SelectedShape), changes);
        Assert.Contains(nameof(overlay.IsBox), changes);
        Assert.Contains(nameof(overlay.IsOval), changes);
        Assert.Equal(1, changeCount);

        overlay.SelectedShape = OverlayShape.Oval;
        Assert.Equal(1, changeCount);

        overlay.SelectedShape = OverlayShape.Box;
        Assert.True(overlay.IsBox);
        Assert.False(overlay.IsOval);
        Assert.Equal(2, changeCount);
    }

    [Fact]
    public void RotationPreviewDoesNotPersistUntilCommitted()
    {
        var changeCount = 0;
        var overlay = new OverlayItemViewModel(
            new OverlayConfiguration(),
            _ => changeCount++,
            _ => { });

        overlay.PreviewRotation(45);

        Assert.Equal(45, overlay.Rotation);
        Assert.Equal(0, overlay.Configuration.Rotation);
        Assert.Equal(0, changeCount);

        overlay.SetRotation(overlay.Rotation);

        Assert.Equal(45, overlay.Rotation);
        Assert.Equal(45, overlay.Configuration.Rotation);
        Assert.Equal(1, changeCount);
    }

    [Fact]
    public void UnchangedRotationPreviewCanBeClearedWithoutPersisting()
    {
        var changeCount = 0;
        var overlay = new OverlayItemViewModel(
            new OverlayConfiguration(),
            _ => changeCount++,
            _ => { });

        overlay.PreviewRotation(30);
        overlay.SetRotation(0);

        Assert.Equal(0, overlay.Rotation);
        Assert.Equal(0, overlay.Configuration.Rotation);
        Assert.Equal(0, changeCount);
    }
}
