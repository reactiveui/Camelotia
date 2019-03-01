using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using Camelotia.Presentation.Avalonia.Views;
using Camelotia.Presentation.Avalonia.Services;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Providers;
using Camelotia.Services.Storages;
using Camelotia.Services.Interfaces;
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

            Akavache.BlobCache.ApplicationName = "Camelotia";
            var cache = Akavache.BlobCache.UserAccount;
            var login = new AvaloniaAuthenticator();

            return new MainViewModel(
                (provider, files, auth) => new ProviderViewModel(
                    model => new CreateFolderViewModel(model, currentThread, mainThread, provider),
                    model => new RenameFileViewModel(model, currentThread, mainThread, provider),
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
                    new Dictionary<string, Func<Guid, IProvider>>
                    {
                        [typeof(LocalFileSystemProvider).Name] = id => new LocalFileSystemProvider(id),
                        [typeof(VkontakteFileSystemProvider).Name] = id => new VkontakteFileSystemProvider(id, cache),
                        [typeof(YandexFileSystemProvider).Name] = id => new YandexFileSystemProvider(id, login, cache),
                        [typeof(FtpFileSystemProvider).Name] = id => new FtpFileSystemProvider(id),
                        [typeof(SftpFileSystemProvider).Name] = id => new SftpFileSystemProvider(id),
                        [typeof(GitHubFileSystemProvider).Name] = id => new GitHubFileSystemProvider(id)
                    },
                    cache
                ),
                new AvaloniaFileManager(),
                currentThread,
                mainThread
            );
        }
    }
}
