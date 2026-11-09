using Avalonia.Controls;

namespace ScreenVeil.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var overlayWindows = new OverlayWindowService();
        DataContext = new MainWindowViewModel(overlayWindows);
        Closed += (_, _) => overlayWindows.CloseAll();
    }
}