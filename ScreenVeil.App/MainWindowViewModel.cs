using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ScreenVeil.Core.Services;

namespace ScreenVeil.App;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly OverlayWorkspace _workspace = new();
    private readonly OverlayWindowService _overlayWindows;

    public MainWindowViewModel(OverlayWindowService overlayWindows)
    {
        _overlayWindows = overlayWindows;
        AddOverlayCommand = new RelayCommand(AddOverlay);
        ToggleAllCommand = new RelayCommand(ToggleAll);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<OverlayItemViewModel> Overlays { get; } = [];
    public RelayCommand AddOverlayCommand { get; }
    public RelayCommand ToggleAllCommand { get; }
    public int OverlayCount => Overlays.Count;
    public bool IsEmpty => Overlays.Count == 0;
    public string CountText => $"{OverlayCount} {(OverlayCount == 1 ? "cover" : "covers")}";
    public string ToggleAllText => Overlays.Any(overlay => overlay.IsVisible) ? "Hide all" : "Show all";

    private void AddOverlay()
    {
        var item = new OverlayItemViewModel(_workspace.Add(), OnOverlayChanged, RemoveOverlay);
        Overlays.Add(item);
        _overlayWindows.Create(item);
        NotifyListChanged();
    }

    private void RemoveOverlay(OverlayItemViewModel item)
    {
        _workspace.Remove(item.Id);
        Overlays.Remove(item);
        _overlayWindows.Remove(item.Id);
        NotifyListChanged();
    }

    private void ToggleAll()
    {
        var shouldShow = !Overlays.Any(overlay => overlay.IsVisible);
        foreach (var overlay in Overlays)
        {
            overlay.IsVisible = shouldShow;
        }
    }

    private void OnOverlayChanged(OverlayItemViewModel item)
    {
        _workspace.Update(item.Configuration);
        _overlayWindows.Update(item);
        OnPropertyChanged(nameof(ToggleAllText));
    }

    private void NotifyListChanged()
    {
        OnPropertyChanged(nameof(OverlayCount));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(CountText));
        OnPropertyChanged(nameof(ToggleAllText));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}