using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Presentation.Xamarin.Droid.Services;
using Camelotia.Services.Interfaces;
using Camelotia.Services.Providers;
using Camelotia.Services.Storages;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;
using ReactiveUI;

namespace Camelotia.Presentation.Xamarin.Droid
{
    [Activity(
        Label = "Camelotia.Presentation.Xamarin", 
        Icon = "@mipmap/icon", 
        Theme = "@style/MainTheme", 
        MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private readonly ISubject<string> _authenticationCodeReceived = new Subject<string>();

        public IObservable<string> AuthenticationCodeReceived => _authenticationCodeReceived;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Plugin.Iconize.Iconize.Init(Resource.Id.toolbar, Resource.Id.sliding_tabs);
            LoadApplication(new App(BuildMainViewModel()));
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode != 42 || resultCode != Result.Ok) return;
            var code = data.GetStringExtra("token");
            _authenticationCodeReceived.OnNext(code);
        }

        private IMainViewModel BuildMainViewModel()
        {
            var current = CurrentThreadScheduler.Instance;
            var main = RxApp.MainThreadScheduler;

            Akavache.BlobCache.ApplicationName = "Camelotia";
            var cache = Akavache.BlobCache.UserAccount;
            var login = new AndroidYandexAuthenticator(this);

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
                        ["Vkontakte Docs"] = id => new VkDocsProvider(id, cache),
                        ["Yandex Disk"] = id => new YandexDiskProvider(id, login, cache),
                        ["FTP"] = id => new FtpProvider(id),
                        ["SFTP"] = id => new SftpProvider(id),
                        ["GitHub"] = id => new GitHubProvider(id, cache)
                    },
                    cache
                ),
                new AndroidFileManager(this),
                current, main
            );
        }
    }
}