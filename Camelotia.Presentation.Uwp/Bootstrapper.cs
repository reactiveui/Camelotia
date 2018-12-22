using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.Uwp.Services;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Providers;
using ReactiveUI;
using System.Reactive.Concurrency;

namespace Camelotia.Presentation.Uwp
{
    public sealed class Bootstrapper
    {
        public static IMainViewModel BuildMainViewModel()
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
                    new VkontakteFileSystemProvider(),
                    new YandexFileSystemProvider(
                        new UniversalWindowsYandexAuthenticator()
                    )
                ),
                new UniversalWindowsFileManager(),
                currentThread,
                mainThread
            );
        }
    }
}
