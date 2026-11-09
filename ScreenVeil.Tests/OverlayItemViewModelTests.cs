using ScreenVeil.App;
using ScreenVeil.Core.Models;

namespace ScreenVeil.Tests;

public sealed class OverlayItemViewModelTests
{
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
