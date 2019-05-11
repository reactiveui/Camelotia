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
            var current = CurrentThreadScheduler.Instance;
            var main = RxApp.MainThreadScheduler;

            Akavache.BlobCache.ApplicationName = "Camelotia";
            var cache = Akavache.BlobCache.UserAccount;
            var login = new AvaloniaYandexAuthenticator();

            return new MainViewModel(
                (provider, files, auth) => new ProviderViewModel(
                    model => new CreateFolderViewModel(model, provider, current, main),
                    model => new RenameFileViewModel(model, provider, current, main),
                    auth, files, provider, current, main
                ),
                provider => new AuthViewModel(
                    new DirectAuthViewModel(provider, current, main),
                    new HostAuthViewModel(provider, current, main), 
                    new OAuthViewModel(provider, current, main),
                    provider, current, main
                ),
                new ProviderStorage(
                    new Dictionary<string, Func<Guid, IProvider>>
                    {
                        ["Local File System"] = id => new LocalProvider(id),
                        ["Vkontakte Docs"] = id => new VkDocsProvider(id, cache),
                        ["Yandex Disk"] = id => new YandexDiskProvider(id, login, cache),
                        ["FTP"] = id => new FtpProvider(id),
                        ["SFTP"] = id => new SftpProvider(id),
                        ["GitHub"] = id => new GitHubProvider(id, cache),
                        ["Google Drive"] = id => new GoogleDriveProvider(id, cache)
                    },
                    cache
                ),
                new AvaloniaFileManager(),
                current, main
            );
        }
    }
}
