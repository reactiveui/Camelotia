using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Presentation.Xamarin.Droid.Services;
using Camelotia.Services.Providers;
using Camelotia.Services.Storages;
using System.Reactive.Concurrency;
using Android.App;
using Android.Content.PM;
using Android.OS;
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
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App(BuildMainViewModel()));
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
                    new OAuthViewModel(currentThread, mainThread, provider),
                    currentThread,
                    provider
                ),
                new ProviderStorage(
                    new VkontakteFileSystemProvider(cache),
                    new YandexFileSystemProvider(
                        new AndroidAuthenticator(), cache
                    )
                ),
                new AndroidFileManager(this),
                currentThread,
                mainThread
            );
        }
    }
}