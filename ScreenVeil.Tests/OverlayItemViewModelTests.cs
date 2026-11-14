using Avalonia;
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

    [Theory]
    [InlineData(OverlayShape.Box)]
    [InlineData(OverlayShape.Oval)]
    public void RotationKeepsWindowBoundsAndCoverPositionStable(OverlayShape shape)
    {
        var changes = new List<string?>();
        var overlay = new OverlayItemViewModel(
            new OverlayConfiguration { Shape = shape, X = 100, Y = 200 },
            _ => { },
            _ => { });
        overlay.PropertyChanged += (_, e) => changes.Add(e.PropertyName);
        var size = overlay.WindowWidth;
        var position = OverlayGeometry.GetWindowPosition(overlay.X, overlay.Y, overlay.Width, overlay.Height, 1);

        overlay.PreviewRotation(45);
        overlay.SetRotation(45);

        Assert.Equal(size, overlay.WindowWidth);
        Assert.Equal(size, overlay.WindowHeight);
        Assert.Equal(position, OverlayGeometry.GetWindowPosition(overlay.X, overlay.Y, overlay.Width, overlay.Height, 1));
        Assert.DoesNotContain(nameof(overlay.WindowWidth), changes);
        Assert.DoesNotContain(nameof(overlay.WindowHeight), changes);

        overlay.UpdatePosition(new PixelPoint(50, 75), 1.5);
        var actualPosition = OverlayGeometry.GetWindowPosition(overlay.X, overlay.Y, overlay.Width, overlay.Height, 1.5);
        Assert.Equal(50, actualPosition.X, 10);
        Assert.Equal(75, actualPosition.Y, 10);
    }
}
