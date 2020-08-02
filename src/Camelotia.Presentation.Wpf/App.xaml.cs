using Camelotia.Presentation.ViewModels;
using Camelotia.Presentation.Wpf.Services;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using Camelotia.Services.Storages;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Windows;

namespace Camelotia.Presentation.Wpf
{
    public partial class App : Application
    {
        public App() => InitializeComponent();

        private void OnApplicationStartup(object sender, StartupEventArgs e)
        {
            Akavache.BlobCache.ApplicationName = "Camelotia";
            var cache = Akavache.BlobCache.UserAccount;
            var login = new WindowsPresentationYandexAuthenticator();
            var files = new WindowsPresentationFileManager();

            var mainViewModel = new MainViewModel(
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
                        ["Local Storage"] = id => new LocalProvider(id),
                        ["Yandex Disk"] = id => new YandexDiskProvider(id, login, cache),
                        ["Vkontakte Docs"] = id => new VkDocsProvider(id, cache),
                        ["Google Drive"] = id => new GoogleDriveProvider(id, cache),
                        ["FTP"] = id => new FtpProvider(id),
                        ["SFTP"] = id => new SftpProvider(id),
                        ["GitHub"] = id => new GitHubProvider(id, cache)
                    },
                    cache
                )
            );

            var window = new MainView { DataContext = mainViewModel };
            window.Closed += delegate { Shutdown(); };
            window.Show();
        }
    }
}
