using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Avalonia.Services;
using Camelotia.Presentation.Avalonia.Views;
using Camelotia.Presentation.Infrastructure;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia
{
    public class App : Application
    {
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            // Configure ReactiveUI suspension management.
            var suspension = new AutoSuspendHelper(ApplicationLifetime);
            RxApp.SuspensionHost.CreateNewAppState = () => new MainState();
            RxApp.SuspensionHost.SetupDefaultSuspendResume(new NewtonsoftJsonSuspensionDriver("appstate.json"));
            suspension.OnFrameworkInitializationCompleted();
            base.OnFrameworkInitializationCompleted();

            // Configure app dependencies.
            var window = new MainView();
            var styles = new AvaloniaStyleManager(window);
            var mainState = RxApp.SuspensionHost.GetAppState<MainState>();

            Akavache.BlobCache.ApplicationName = "CamelotiaV2";
            window.SwitchThemeButton.Click += (sender, args) => styles.UseNextTheme();
            window.DataContext = new MainViewModel(
                mainState,
                new CloudFactory(
                    mainState.CloudConfiguration,
                    new AvaloniaYandexAuthenticator(),
                    Akavache.BlobCache.UserAccount),
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
                    new AvaloniaFileManager(window),
                    provider));

            window.Show();
        }
    }
}
