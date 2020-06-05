using Avalonia;
using Avalonia.Markup.Xaml;
using Camelotia.Presentation.Avalonia.Services;
using Camelotia.Presentation.Avalonia.Views;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Models;
using Camelotia.Services.Providers;
using Camelotia.Services.Storages;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using Avalonia.Controls;

namespace Camelotia.Presentation.Avalonia
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            base.Initialize();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var current = CurrentThreadScheduler.Instance;
            var main = RxApp.MainThreadScheduler;

            Akavache.BlobCache.ApplicationName = "Camelotia";
            var cache = Akavache.BlobCache.UserAccount;

            var window = new MainView();
            var files = new AvaloniaFileManager(window);
            var styles = new AvaloniaStyleManager(window);
            window.SwitchThemeButton.Click += (sender, args) => styles.UseNextTheme(); 

            var login = new AvaloniaYandexAuthenticator();
            var context = new MainViewModel(
                (provider, auth) => new ProviderViewModel(
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
                current, main
            );

            window.DataContext = context;
            window.Show();
            base.OnFrameworkInitializationCompleted();
        }
    }
}