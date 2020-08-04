using Avalonia;
using Avalonia.Markup.Xaml;
using Camelotia.Presentation.Avalonia.Services;
using Camelotia.Presentation.Avalonia.Views;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using Camelotia.Services.Storages;
using System;
using System.Collections.Generic;

namespace Camelotia.Presentation.Avalonia
{
    public class App : Application
    {
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            Akavache.BlobCache.ApplicationName = "Camelotia";
            var cache = Akavache.BlobCache.UserAccount;
            var window = new MainView();
            var files = new AvaloniaFileManager(window);
            var styles = new AvaloniaStyleManager(window);
            window.SwitchThemeButton.Click += (sender, args) => styles.UseNextTheme(); 

            var login = new AvaloniaYandexAuthenticator();
            var context = new MainViewModel(
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
                        ["Local File System"] = id => new LocalProvider(id),
                        ["Vkontakte Docs"] = id => new VkDocsProvider(id, cache),
                        ["Yandex Disk"] = id => new YandexDiskProvider(id, login, cache),
                        ["FTP"] = id => new FtpProvider(id),
                        ["SFTP"] = id => new SftpProvider(id),
                        ["GitHub"] = id => new GitHubProvider(id, cache),
                        ["Google Drive"] = id => new GoogleDriveProvider(id, cache)
                    },
                    cache
                )
            );

            window.DataContext = context;
            window.Show();
            base.OnFrameworkInitializationCompleted();
        }
    }
}