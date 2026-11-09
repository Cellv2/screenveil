using ScreenVeil.Core.Services;

namespace ScreenVeil.Tests;

public sealed class OverlayWorkspaceTests
{
    [Fact]
    public void AddCreatesIndependentVisibleOverlays()
    {
        var workspace = new OverlayWorkspace();

        var first = workspace.Add();
        var second = workspace.Add();

        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal("Cover 1", first.Name);
        Assert.Equal("Cover 2", second.Name);
        Assert.True(first.IsVisible);
        Assert.True(second.IsVisible);
        Assert.NotEqual(first.X, second.X);
    }

    [Fact]
    public void RemoveDeletesOnlyTheRequestedOverlay()
    {
        var workspace = new OverlayWorkspace();
        var first = workspace.Add();
        var second = workspace.Add();

        Assert.True(workspace.Remove(first.Id));

        Assert.Single(workspace.Overlays);
        Assert.Equal(second.Id, workspace.Overlays[0].Id);
        Assert.False(workspace.Remove(first.Id));
    }

    [Fact]
    public void AddReusesTheLowestAvailableCoverNumber()
    {
        var workspace = new OverlayWorkspace();
        var first = workspace.Add();

        Assert.True(workspace.Remove(first.Id));
        var replacement = workspace.Add();

        Assert.Equal("Cover 1", replacement.Name);
    }

    [Fact]
    public void ToggleAllHidesVisibleOverlaysThenShowsThem()
    {
        var workspace = new OverlayWorkspace();
        workspace.Add();
        workspace.Add();

        workspace.ToggleAllVisibility();

        Assert.All(workspace.Overlays, overlay => Assert.False(overlay.IsVisible));
        Assert.False(workspace.AreAllVisible);

        workspace.ToggleAllVisibility();

        Assert.All(workspace.Overlays, overlay => Assert.True(overlay.IsVisible));
        Assert.True(workspace.AreAllVisible);
    }

    [Fact]
    public void SetVisibilityChangesOnlyTheRequestedOverlay()
    {
        var workspace = new OverlayWorkspace();
        var first = workspace.Add();
        var second = workspace.Add();

        Assert.True(workspace.SetVisibility(first.Id, false));

        Assert.False(workspace.Overlays[0].IsVisible);
        Assert.True(workspace.Overlays[1].IsVisible);
        Assert.False(workspace.SetVisibility(Guid.NewGuid(), false));
    }
}