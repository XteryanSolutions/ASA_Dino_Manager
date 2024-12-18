using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Application = Microsoft.Maui.Controls.Application;

namespace ASA_Dino_Manager;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Force the app to use Dark Mode
        UserAppTheme = AppTheme.Dark;

        MainPage = new AppShell();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);

        // Set the default size for Windows or macOS
        window.Width = 920;  // Desired width
        window.Height = 500; // Desired height

        return window;
    }
}