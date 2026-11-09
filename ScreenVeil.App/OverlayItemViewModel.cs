using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Media;
using ScreenVeil.Core.Models;

namespace ScreenVeil.App;

public sealed class OverlayColorOption(string name, string hex)
{
    public string Name { get; } = name;
    public string Hex { get; } = hex;
    public IBrush Brush => new SolidColorBrush(Color.Parse(Hex));

    public override string ToString() => Name;
}

public sealed class OverlayItemViewModel : INotifyPropertyChanged
{
    public static IReadOnlyList<OverlayColorOption> Colors { get; } =
    [
        new("Black", "#090B0A"),
        new("White", "#F4F4F1"),
        new("Moss", "#28684E"),
        new("Coral", "#D36F52")
    ];

    private readonly Action<OverlayItemViewModel> _onChanged;
    private readonly Action<OverlayItemViewModel> _onDelete;
    private OverlayConfiguration _configuration;
    private OverlayColorOption _selectedColor;

    public OverlayItemViewModel(
        OverlayConfiguration configuration,
        Action<OverlayItemViewModel> onChanged,
        Action<OverlayItemViewModel> onDelete)
    {
        _configuration = configuration;
        _onChanged = onChanged;
        _onDelete = onDelete;
        _selectedColor = Colors.FirstOrDefault(color => color.Hex == configuration.Color) ?? Colors[0];
        ToggleVisibilityCommand = new RelayCommand(() => IsVisible = !IsVisible);
        DeleteCommand = new RelayCommand(() => _onDelete(this));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Guid Id => _configuration.Id;
    public string Name => _configuration.Name;
    public double X => _configuration.X;
    public double Y => _configuration.Y;
    public double Width => _configuration.Width;
    public double Height => _configuration.Height;
    public double Rotation => _configuration.Rotation;
    public double WindowWidth => OverlayGeometry.GetWindowSize(Width, Height, Rotation).Width;
    public double WindowHeight => OverlayGeometry.GetWindowSize(Width, Height, Rotation).Height;
    public IBrush Background => new SolidColorBrush(Color.Parse(_configuration.Color));
    public IReadOnlyList<OverlayColorOption> Palette => Colors;
    public RelayCommand ToggleVisibilityCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public string VisibilityLabel => IsVisible ? "Hide" : "Show";
    public string StatusText => IsVisible ? "VISIBLE / TOPMOST REQUESTED" : "HIDDEN";
    public OverlayConfiguration Configuration => _configuration;

    public OverlayColorOption SelectedColor
    {
        get => _selectedColor;
        set
        {
            if (_selectedColor == value)
            {
                return;
            }

            _selectedColor = value;
            UpdateConfiguration(_configuration with { Color = value.Hex }, nameof(SelectedColor), nameof(Background));
        }
    }

    public bool IsVisible
    {
        get => _configuration.IsVisible;
        set => UpdateConfiguration(_configuration with { IsVisible = value }, nameof(IsVisible), nameof(VisibilityLabel), nameof(StatusText));
    }

    public void UpdatePosition(PixelPoint position, double renderScaling)
    {
        var coverPosition = OverlayGeometry.GetCoverPosition(
            position.X,
            position.Y,
            Width,
            Height,
            Rotation,
            renderScaling);
        var updated = _configuration with
        {
            X = coverPosition.X,
            Y = coverPosition.Y
        };

        if (_configuration != updated)
        {
            _configuration = updated;
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            _onChanged(this);
        }
    }

    public void UpdateGeometry(double x, double y, double width, double height) =>
        UpdateConfiguration(
            _configuration with { X = x, Y = y, Width = width, Height = height },
            nameof(X),
            nameof(Y),
            nameof(Width),
            nameof(Height),
            nameof(WindowWidth),
            nameof(WindowHeight));

    public void SetRotation(double rotation)
    {
        if (!double.IsFinite(rotation))
        {
            throw new ArgumentOutOfRangeException(nameof(rotation));
        }

        var normalizedRotation = (rotation % 360 + 360) % 360;
        UpdateConfiguration(
            _configuration with { Rotation = normalizedRotation },
            nameof(Rotation),
            nameof(WindowWidth),
            nameof(WindowHeight));
    }

    private void UpdateConfiguration(OverlayConfiguration updated, params string[] properties)
    {
        if (_configuration == updated)
        {
            return;
        }

        _configuration = updated;
        foreach (var property in properties)
        {
            OnPropertyChanged(property);
        }

        _onChanged(this);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}