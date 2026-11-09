using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace ScreenVeil.App;

public partial class OverlayWindow : Window
{
    private const double MinimumWidth = 100;
    private const double MinimumHeight = 64;
    private RotationGesture? _rotationGesture;
    private ResizeGesture? _resizeGesture;
    private IPointer? _capturedPointer;
    private Action<OverlayItemViewModel> _onRotationStarted = static _ => { };
    private Action<OverlayItemViewModel> _onRotationEnded = static _ => { };

    internal bool IsRotationGestureInProgress { get; set; }
    internal PixelPoint? ExpectedProgrammaticPosition { get; set; }

    public OverlayWindow()
    {
        InitializeComponent();
    }

    public OverlayWindow(
        OverlayItemViewModel overlay,
        Action<OverlayItemViewModel> onRotationStarted,
        Action<OverlayItemViewModel> onRotationEnded) : this()
    {
        DataContext = overlay;
        Title = overlay.Name;
        _onRotationStarted = onRotationStarted;
        _onRotationEnded = onRotationEnded;
    }

    private void OnMovePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
            e.Handled = true;
        }
    }

    private void OnResizePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed ||
            DataContext is not OverlayItemViewModel overlay ||
            sender is not Control { Tag: string edgeName } ||
            !Enum.TryParse<WindowEdge>(edgeName, true, out var edge))
        {
            return;
        }

        var center = GetWindowCenter();
        var localPointer = ToOverlayCoordinates(GetScreenPosition(e.GetPosition(this)), center, overlay.Rotation);
        var left = -overlay.Width / 2;
        var right = overlay.Width / 2;
        var top = -overlay.Height / 2;
        var bottom = overlay.Height / 2;
        _resizeGesture = new ResizeGesture(
            edge,
            center,
            overlay.Rotation,
            overlay.Width,
            overlay.Height,
            localPointer.X - (IsWest(edge) ? left : right),
            localPointer.Y - (IsNorth(edge) ? top : bottom),
            RenderScaling);
        _rotationGesture = null;
        CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void OnRotatePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed ||
            DataContext is not OverlayItemViewModel overlay)
        {
            return;
        }

        var center = GetWindowCenter();
        var pointerPosition = GetScreenPosition(e.GetPosition(this));
        var pointerAngle = Math.Atan2(pointerPosition.Y - center.Y, pointerPosition.X - center.X);
        _rotationGesture = new RotationGesture(center, pointerAngle, overlay.Rotation);
        _resizeGesture = null;
        _onRotationStarted(overlay);
        CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_capturedPointer != e.Pointer || DataContext is not OverlayItemViewModel overlay)
        {
            return;
        }

        var pointerPosition = GetScreenPosition(e.GetPosition(this));
        if (_rotationGesture is { } rotation)
        {
            var pointerAngle = Math.Atan2(pointerPosition.Y - rotation.Center.Y, pointerPosition.X - rotation.Center.X);
            var delta = NormalizeRadians(pointerAngle - rotation.StartPointerAngle);
            overlay.PreviewRotation(rotation.StartRotation + delta * 180 / Math.PI);
            e.Handled = true;
            return;
        }

        if (_resizeGesture is { } resize)
        {
            ResizeOverlay(overlay, resize, pointerPosition);
            e.Handled = true;
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_capturedPointer != e.Pointer)
        {
            return;
        }

        if (_rotationGesture is { } rotation && DataContext is OverlayItemViewModel overlay)
        {
            var pointerPosition = GetScreenPosition(e.GetPosition(this));
            var pointerAngle = Math.Atan2(pointerPosition.Y - rotation.Center.Y, pointerPosition.X - rotation.Center.X);
            var delta = NormalizeRadians(pointerAngle - rotation.StartPointerAngle);
            overlay.SetRotation(rotation.StartRotation + delta * 180 / Math.PI);
            _onRotationEnded(overlay);
        }

        e.Pointer.Capture(null);
        _capturedPointer = null;
        _rotationGesture = null;
        _resizeGesture = null;
    }

    private void CapturePointer(IPointer pointer)
    {
        _capturedPointer = pointer;
        pointer.Capture(this);
    }

    private void ResizeOverlay(OverlayItemViewModel overlay, ResizeGesture gesture, Point pointerPosition)
    {
        var localPointer = ToOverlayCoordinates(pointerPosition, gesture.Center, gesture.Rotation);
        var left = -gesture.Width / 2;
        var right = gesture.Width / 2;
        var top = -gesture.Height / 2;
        var bottom = gesture.Height / 2;

        if (IsWest(gesture.Edge))
        {
            left = Math.Min(localPointer.X - gesture.PointerOffsetX, right - MinimumWidth);
        }
        else if (IsEast(gesture.Edge))
        {
            right = Math.Max(localPointer.X - gesture.PointerOffsetX, left + MinimumWidth);
        }

        if (IsNorth(gesture.Edge))
        {
            top = Math.Min(localPointer.Y - gesture.PointerOffsetY, bottom - MinimumHeight);
        }
        else if (IsSouth(gesture.Edge))
        {
            bottom = Math.Max(localPointer.Y - gesture.PointerOffsetY, top + MinimumHeight);
        }

        var width = right - left;
        var height = bottom - top;
        var localCenterX = (left + right) / 2;
        var localCenterY = (top + bottom) / 2;
        var radians = gesture.Rotation * Math.PI / 180;
        var cosine = Math.Cos(radians);
        var sine = Math.Sin(radians);
        var centerX = gesture.Center.X + localCenterX * cosine - localCenterY * sine;
        var centerY = gesture.Center.Y + localCenterX * sine + localCenterY * cosine;

        overlay.UpdateGeometry(
            (centerX - width / 2) * gesture.RenderScaling,
            (centerY - height / 2) * gesture.RenderScaling,
            width,
            height);
    }

    private Point GetScreenPosition(Point windowPosition) =>
        new(Position.X / RenderScaling + windowPosition.X, Position.Y / RenderScaling + windowPosition.Y);

    private Point GetWindowCenter() =>
        new(Position.X / RenderScaling + Width / 2, Position.Y / RenderScaling + Height / 2);

    private static Point ToOverlayCoordinates(Point point, Point center, double rotation)
    {
        var radians = rotation * Math.PI / 180;
        var deltaX = point.X - center.X;
        var deltaY = point.Y - center.Y;
        return new Point(
            deltaX * Math.Cos(radians) + deltaY * Math.Sin(radians),
            -deltaX * Math.Sin(radians) + deltaY * Math.Cos(radians));
    }

    private static bool IsWest(WindowEdge edge) =>
        edge is WindowEdge.West or WindowEdge.NorthWest or WindowEdge.SouthWest;

    private static bool IsEast(WindowEdge edge) =>
        edge is WindowEdge.East or WindowEdge.NorthEast or WindowEdge.SouthEast;

    private static bool IsNorth(WindowEdge edge) =>
        edge is WindowEdge.North or WindowEdge.NorthWest or WindowEdge.NorthEast;

    private static bool IsSouth(WindowEdge edge) =>
        edge is WindowEdge.South or WindowEdge.SouthWest or WindowEdge.SouthEast;

    private static double NormalizeRadians(double radians) =>
        Math.Atan2(Math.Sin(radians), Math.Cos(radians));

    private sealed record RotationGesture(Point Center, double StartPointerAngle, double StartRotation);

    private sealed record ResizeGesture(
        WindowEdge Edge,
        Point Center,
        double Rotation,
        double Width,
        double Height,
        double PointerOffsetX,
        double PointerOffsetY,
        double RenderScaling);
}
