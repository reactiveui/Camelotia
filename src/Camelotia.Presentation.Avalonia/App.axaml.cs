// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using Akavache;
using Akavache.Sqlite3;
using Akavache.SystemTextJson;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Avalonia.Services;
using Camelotia.Presentation.Avalonia.Views;
using Camelotia.Presentation.Infrastructure;
using ReactiveUI;
using ReactiveUI.Avalonia;
using ReactiveUI.Interfaces;

namespace Camelotia.Presentation.Avalonia;

/// <summary>The Avalonia application: restores the saved state and shows the main window.</summary>
[System.Diagnostics.DebuggerDisplay("App")]
public sealed class App : Application, IDisposable
{
    /// <summary>The window size the layout is designed for.</summary>
    private const int MinimumWidth = 850;

    /// <summary>The window height the layout is designed for.</summary>
    private const int MinimumHeight = 590;

    /// <summary>Saves the state when the app is suspended or closes.</summary>
    private AutoSuspendHelper? _suspension;

    /// <summary>Stops responding to suspend and resume requests.</summary>
    private IDisposable? _suspendResume;

    /// <summary>Owns the services and the main view model.</summary>
    private CamelotiaHost? _host;

    /// <inheritdoc/>
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    /// <inheritdoc/>
    public override void OnFrameworkInitializationCompleted()
    {
        _ = Splat.Builder.AppBuilder.CreateSplatBuilder()
            .WithAkavacheCacheDatabase<SystemJsonSerializer>(static builder => builder.WithSqliteProvider().WithSqliteDefaults(), "CamelotiaV2");

        var suspensionHost = (ISuspensionHost<MainState>)RxSuspension.SuspensionHost;
        suspensionHost.CreateNewAppStateTyped = static () => new MainState();
        _suspension = new(ApplicationLifetime ?? throw new InvalidOperationException("The app has no lifetime."));
        _suspendResume = suspensionHost.SetupDefaultSuspendResume(AppStateJsonContext.Default.MainState, new AkavacheSuspensionDriver(CacheDatabase.UserAccount));
        _suspension.OnFrameworkInitializationCompleted();

        var window = new Window { Height = MinimumHeight, Width = MinimumWidth, MinHeight = MinimumHeight, MinWidth = MinimumWidth };
        _host = new(suspensionHost.GetAppState(), CacheDatabase.UserAccount, new AvaloniaFileManager(window));

        var styles = new AvaloniaStyleManager(this);
        var view = new MainView { DataContext = _host.MainViewModel };
        view.SwitchThemeButton.Click += (_, _) => styles.UseNextTheme();
        window.Content = view;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = window;
            desktop.Exit += (_, _) => Dispose();
        }

        window.Show();
        base.OnFrameworkInitializationCompleted();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _host?.Dispose();
        _suspendResume?.Dispose();
        _suspension?.Dispose();
    }
}
