// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.
using System.Windows;
using Akavache;
using Akavache.Sqlite3;
using Akavache.SystemTextJson;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Infrastructure;
using Camelotia.Presentation.Wpf.Services;
using ReactiveUI;
using ReactiveUI.Builder;
using ReactiveUI.Interfaces;
using ReactiveUI.Primitives.Advanced;
using Splat;

namespace Camelotia.Presentation.Wpf;

/// <summary>The WPF application: restores the saved state and shows the main window.</summary>
[System.Diagnostics.DebuggerDisplay("App")]
public sealed partial class App : Application, IDisposable
{
    /// <summary>Stops responding to suspend and resume requests.</summary>
    private IDisposable? _suspendResume;

    /// <summary>Owns the services and the main view model.</summary>
    private CamelotiaHost? _host;

    /// <summary>Initializes a new instance of the <see cref="App"/> class.</summary>
    public App()
    {
        InitializeComponent();
        _ = RxAppBuilder.CreateReactiveUIBuilder()
            .WithWpf()
            .WithSuspensionHost<MainState>()
            .WithExceptionHandler(Witness.Create<Exception>(static exception => LogHost.Default.Error(exception, "An unhandled error reached ReactiveUI.")))
            .BuildApp();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _host?.Dispose();
        _suspendResume?.Dispose();
    }

    /// <inheritdoc/>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // The helper forwards the app's activation, deactivation and exit events to ReactiveUI, so the state is saved.
        // The app keeps it alive through those event handlers.
        _ = new AutoSuspendHelper(this);
        _ = Splat.Builder.AppBuilder.CreateSplatBuilder()
            .WithAkavacheCacheDatabase<SystemJsonSerializer>(static builder => builder.WithSqliteProvider().WithSqliteDefaults(), "CamelotiaV2");

        var suspensionHost = (ISuspensionHost<MainState>)RxSuspension.SuspensionHost;
        suspensionHost.CreateNewAppStateTyped = static () => new MainState();
        _suspendResume = suspensionHost.SetupDefaultSuspendResume(AppStateJsonContext.Default.MainState, new AkavacheSuspensionDriver(CacheDatabase.UserAccount));
        _host = new(suspensionHost.GetAppState(), CacheDatabase.UserAccount, new WindowsPresentationFileManager());

        var window = new MainView { DataContext = _host.MainViewModel };
        window.Closed += (_, _) => Shutdown();
        window.Show();
    }

    /// <inheritdoc/>
    protected override void OnExit(ExitEventArgs e)
    {
        Dispose();
        base.OnExit(e);
    }
}
