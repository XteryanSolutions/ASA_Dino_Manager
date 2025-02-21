﻿using Application = Microsoft.Maui.Controls.Application;

namespace Ark_Dino_Manager;

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
        window.Width = Shared.startupX;  // Desired width
        window.Height = Shared.startupY; // Desired height

        return window;
    }
}