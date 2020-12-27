using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Infrastructure;
using Camelotia.Presentation.ViewModels;
using Camelotia.Presentation.Wpf.Services;
using Camelotia.Services;
using ReactiveUI;
using System.Windows;

namespace Camelotia.Presentation.Wpf
{
    public partial class App : Application
    {
        private readonly AutoSuspendHelper _autoSuspendHelper;

        public App()
        {
            InitializeComponent();
            _autoSuspendHelper = new AutoSuspendHelper(this);
            RxApp.SuspensionHost.CreateNewAppState = () => new MainState();
            RxApp.SuspensionHost.SetupDefaultSuspendResume(new NewtonsoftJsonSuspensionDriver("appstate.json"));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Akavache.BlobCache.ApplicationName = "Camelotia";
            var mainViewModel = new MainViewModel(
                RxApp.SuspensionHost.GetAppState<MainState>(),
                new CloudFactory(new WindowsPresentationYandexAuthenticator(), Akavache.BlobCache.UserAccount),
                (state, provider) => new CloudViewModel(state,
                    owner => new CreateFolderViewModel(state.CreateFolderState, owner, provider),
                    owner => new RenameFileViewModel(state.RenameFileState, owner, provider),
                    (file, owner) => new FileViewModel(owner, file),
                    (folder, owner) => new FolderViewModel(owner, folder),
                    new AuthViewModel(
                        new DirectAuthViewModel(state.AuthState.DirectAuthState, provider),
                        new HostAuthViewModel(state.AuthState.HostAuthState, provider),
                        new OAuthViewModel(provider),
                        provider
                    ),
                    new WindowsPresentationFileManager(),
                    provider
                )
            );

            var window = new MainView { DataContext = mainViewModel };
            window.Closed += delegate { Shutdown(); };
            window.Show();
        }
    }
}
