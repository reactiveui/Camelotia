using System;
using System.Diagnostics;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Avalonia.Services;
using Camelotia.Presentation.Avalonia.Views;
using Camelotia.Presentation.Infrastructure;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services;
using Live.Avalonia;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia
{
    public class App : Application, ILiveView
    {
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            Akavache.BlobCache.ApplicationName = "CamelotiaV2";
            var suspension = new AutoSuspendHelper(ApplicationLifetime);
            RxApp.SuspensionHost.CreateNewAppState = () => new MainState();
            RxApp.SuspensionHost.SetupDefaultSuspendResume(new NewtonsoftJsonSuspensionDriver("appstate.json"));
            suspension.OnFrameworkInitializationCompleted();

            if (Debugger.IsAttached || IsRelease())
            {
                var window = new Window
                {
                    Height = 590,
                    Width = 850,
                    MinHeight = 590,
                    MinWidth = 850,
                };
                window.Content = CreateView(window);
                window.Show();
            }
            else
            {
                var window = new LiveViewHost(this, Console.WriteLine)
                {
                    Height = 590,
                    Width = 850,
                    MinHeight = 590,
                    MinWidth = 850,
                };
                window.StartWatchingSourceFilesForHotReloading();
                window.Show();
            }

            RxApp.DefaultExceptionHandler = Observer.Create<Exception>(Console.WriteLine);
            base.OnFrameworkInitializationCompleted();
        }

        public object CreateView(Window window)
        {
            var view = new MainView();
            view.DataContext ??= CreateViewModel(window);
            return view;
        }

        private static MainViewModel CreateViewModel(Window window)
        {
            var main = RxApp.SuspensionHost.GetAppState<MainState>();
            return new MainViewModel(
                main,
                new CloudFactory(
                    main.CloudConfiguration,
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
        }

        private static bool IsRelease()
        {
#if RELEASE
            return true;
#else
            return false;
#endif
        }
    }
}
