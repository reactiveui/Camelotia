using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Presentation.Xamarin.Droid.Services;
using Camelotia.Services.Providers;
using Camelotia.Services.Storages;
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
            var currentThread = CurrentThreadScheduler.Instance;
            var mainThread = RxApp.MainThreadScheduler;
            var cache = new AkavacheTokenStorage();

            return new MainViewModel(
                (provider, files, auth) => new ProviderViewModel(auth, files, currentThread, mainThread, provider),
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
                        new AndroidAuthenticator(this), cache
                    ),
                    new FtpFileSystemProvider(),
                    new SftpFileSystemProvider(),
                    new GitHubFileSystemProvider()
                ),
                new AndroidFileManager(this),
                currentThread,
                mainThread
            );
        }
    }
}