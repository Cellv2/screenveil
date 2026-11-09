namespace ScreenVeil.Core.Models;

public sealed record OverlayConfiguration
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = "Cover";
    public string Color { get; init; } = "#090B0A";
    public bool IsVisible { get; init; } = true;
    public double X { get; init; } = 80;
    public double Y { get; init; } = 80;
    public double Width { get; init; } = 320;
    public double Height { get; init; } = 150;
    public double Rotation { get; init; }

    public static OverlayConfiguration Create(int index) => new()
    {
        Name = $"Cover {index}",
        X = 80 + (index - 1) * 28,
        Y = 80 + (index - 1) * 28
    };
}