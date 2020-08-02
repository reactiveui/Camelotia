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
            Akavache.BlobCache.ApplicationName = "Camelotia";
            var cache = Akavache.BlobCache.UserAccount;
            var login = new UniversalWindowsYandexAuthenticator();
            var files = new UniversalWindowsFileManager();

            return new MainViewModel(
                (provider, auth) => new ProviderViewModel(
                    model => new CreateFolderViewModel(model, provider),
                    model => new RenameFileViewModel(model, provider),
                    (file, model) => new FileViewModel(model, file),
                    auth, files, provider
                ),
                provider => new AuthViewModel(
                    new DirectAuthViewModel(provider),
                    new HostAuthViewModel(provider),
                    new OAuthViewModel(provider),
                    provider
                ),
                new AkavacheStorage(
                    new Dictionary<string, Func<ProviderModel, IProvider>>
                    {
                        ["Yandex Disk"] = id => new YandexDiskProvider(id, login, cache),
                        ["Vkontakte Docs"] = id => new VkDocsProvider(id, cache),
                        ["FTP"] = id => new FtpProvider(id),
                        ["SFTP"] = id => new SftpProvider(id),
                        ["GitHub"] = id => new GitHubProvider(id, cache)
                    },
                    cache
                )
            );
        }
    }
}
