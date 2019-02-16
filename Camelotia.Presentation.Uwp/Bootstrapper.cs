using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.Uwp.Services;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Providers;
using Camelotia.Services.Storages;
using System.Reactive.Concurrency;
using ReactiveUI;

namespace Camelotia.Presentation.Uwp
{
    public sealed class Bootstrapper
    {
        public static IMainViewModel BuildMainViewModel()
        {
            var currentThread = CurrentThreadScheduler.Instance;
            var mainThread = RxApp.MainThreadScheduler;
            var cache = new AkavacheTokenStorage();

            return new MainViewModel(
                (provider, files, auth) => new ProviderViewModel(
                    model => new CreateFolderViewModel(model, mainThread, provider),
                    auth, files,
                    currentThread,
                    mainThread,
                    provider
                ),
                provider => new AuthViewModel(
                    new DirectAuthViewModel(currentThread, mainThread, provider),
                    new HostAuthViewModel(currentThread, mainThread, provider),
                    new OAuthViewModel(currentThread, mainThread, provider),
                    currentThread,
                    mainThread,
                    provider
                ),
                new ProviderStorage(
                    new VkontakteFileSystemProvider(cache),
                    new YandexFileSystemProvider(
                        new UniversalWindowsAuthenticator(), cache
                    ),
                    new FtpFileSystemProvider(),
                    new SftpFileSystemProvider(),
                    new GitHubFileSystemProvider()
                ),
                new UniversalWindowsFileManager(),
                currentThread,
                mainThread
            );
        }
    }
}
