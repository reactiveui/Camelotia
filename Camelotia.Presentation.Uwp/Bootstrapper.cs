using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.Uwp.Services;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Providers;
using Camelotia.Services.Storages;
using Camelotia.Services.Models;
using System.Reactive.Concurrency;
using System.Collections.Generic;
using ReactiveUI;
using System;

namespace Camelotia.Presentation.Uwp
{
    public sealed class Bootstrapper
    {
        public static IMainViewModel BuildMainViewModel()
        {
            var current = CurrentThreadScheduler.Instance;
            var main = RxApp.MainThreadScheduler;

            Akavache.BlobCache.ApplicationName = "Camelotia";
            var cache = Akavache.BlobCache.UserAccount;
            var login = new UniversalWindowsYandexAuthenticator();

            return new MainViewModel(
                (provider, files, auth) => new ProviderViewModel(
                    model => new CreateFolderViewModel(model, provider, current, main),
                    model => new RenameFileViewModel(model, provider, current, main),
                    (file, model) => new FileViewModel(model, file),
                    auth, files, provider, current, main
                ),
                provider => new AuthViewModel(
                    new DirectAuthViewModel(provider, current, main),
                    new HostAuthViewModel(provider, current, main),
                    new OAuthViewModel(provider, current, main),
                    provider, current, main
                ),
                new ProviderStorage(
                    new Dictionary<string, Func<ProviderModel, IProvider>>
                    {
                        ["Yandex Disk"] = id => new YandexDiskProvider(id, login, cache),
                        ["Vkontakte Docs"] = id => new VkDocsProvider(id, cache),
                        ["FTP"] = id => new FtpProvider(id),
                        ["SFTP"] = id => new SftpProvider(id),
                        ["GitHub"] = id => new GitHubProvider(id, cache)
                    },
                    cache
                ),
                new UniversalWindowsFileManager(),
                current, main
            );
        }
    }
}
