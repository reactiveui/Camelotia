using System.Reactive.Concurrency;
using Camelotia.Presentation.Avalonia.Views;
using Camelotia.Presentation.Avalonia.Services;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Providers;
using ReactiveUI;
using Avalonia;

namespace Camelotia.Presentation.Avalonia
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            AppBuilder
                .Configure<App>()
                .UseReactiveUI()
                .UsePlatformDetect()
                .Start<MainView>(BuildMainViewModel);
        }

        private static IMainViewModel BuildMainViewModel()
        {
            var currentThread = CurrentThreadScheduler.Instance;
            var mainThread = RxApp.MainThreadScheduler;
                    
            return new MainViewModel(
                (provider, files, auth) => new ProviderViewModel(auth, files, currentThread, mainThread, provider),
                provider => new AuthViewModel(
                    new DirectAuthViewModel(currentThread, mainThread, provider),
                    new OAuthViewModel(currentThread, mainThread, provider),
                    currentThread,
                    provider
                ),
                new ProviderStorage(
                    new LocalFileSystemProvider(),
                    new VkontakteFileSystemProvider(),
                    new YandexFileSystemProvider(
                        new ProcessStartUriLauncher()    
                    )
                ),
                new AvaloniaFileManager(),
                currentThread,
                mainThread
            );
        }
    }
}
