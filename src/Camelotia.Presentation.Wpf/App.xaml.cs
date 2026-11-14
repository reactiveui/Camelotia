using System.Windows;
using Akavache.Reactive;
using Akavache.Reactive.Sqlite3;
using Akavache.Reactive.SystemTextJson;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Infrastructure;
using Camelotia.Presentation.ViewModels;
using Camelotia.Presentation.Wpf.Services;
using Camelotia.Services;
using ReactiveUI;
using ReactiveUI.Reactive;
using ReactiveUI.Reactive.Builder;

namespace Camelotia.Presentation.Wpf;

public partial class App : Application
{
    private readonly AutoSuspendHelper _autoSuspendHelper;

    public App()
    {
        InitializeComponent();
        _autoSuspendHelper = new AutoSuspendHelper(this);
        RxAppBuilder.CreateReactiveUIBuilder().WithWpf().BuildApp();
        RxSuspension.SuspensionHost.CreateNewAppState = () => new MainState();
        RxSuspension.SuspensionHost.SetupDefaultSuspendResume(new NewtonsoftJsonSuspensionDriver("appstate.json"));
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Splat.Builder.AppBuilder.CreateSplatBuilder()
            .WithAkavacheCacheDatabase<SystemJsonSerializer>(static builder => builder.WithSqliteProvider().WithSqliteDefaults(), "Camelotia");
        var mainState = RxSuspension.SuspensionHost.GetAppState<MainState>();
        var mainViewModel = new MainViewModel(
            mainState,
            new CloudFactory(
                mainState.CloudConfiguration,
                new WindowsPresentationYandexAuthenticator(),
                Akavache.Reactive.CacheDatabase.UserAccount),
            (state, provider) => new CloudViewModel(
                state,
                owner => new CreateFolderViewModel(state.CreateFolderState, owner, provider),
                owner => new RenameFileViewModel(state.RenameFileState, owner, provider),
                (file, owner) => new FileViewModel(owner, file),
                (folder, owner) => new FolderViewModel(owner, folder),
                new AuthViewModel(
                    new DirectAuthViewModel(state.AuthState.DirectAuthState, provider),
                    new HostAuthViewModel(state.AuthState.HostAuthState, provider),
                    new OAuthViewModel(provider),
                    provider),
                new WindowsPresentationFileManager(),
                provider));

        var window = new MainView { DataContext = mainViewModel };
        window.Closed += (sender, e) => Shutdown();
        window.Show();
    }
}
