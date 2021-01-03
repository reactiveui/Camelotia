using System;
using System.Reactive.Subjects;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Infrastructure;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.ViewModels;
using Camelotia.Presentation.Xamarin.Droid.Services;
using Camelotia.Services;
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
            var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var file = System.IO.Path.Combine(path, "state.json");

            var autoSuspendHelper = new AutoSuspendHelper(Application);
            RxApp.SuspensionHost.CreateNewAppState = () => new MainState();
            RxApp.SuspensionHost.SetupDefaultSuspendResume(new NewtonsoftJsonSuspensionDriver(file));

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
            return new MainViewModel(
                RxApp.SuspensionHost.GetAppState<MainState>(),
                new CloudFactory(new AndroidYandexAuthenticator(this), Akavache.BlobCache.UserAccount),
                (state, provider) => new CloudViewModel(
                    state,
                    owner => new CreateFolderViewModel(state.CreateFolderState, owner, provider),
                    owner => new RenameFileViewModel(state.RenameFileState, owner, provider),
                    (file, owner) => new FileViewModel(owner, file),
                    (folder, owner) => new FolderViewModel(owner, folder),
                    new AuthViewModel(
                        new DirectAuthViewModel(state.AuthState.DirectAuthState, provider),
                        new HostAuthViewModel(state.AuthState.HostAuthState, provider),
                        new OAuthViewModel(provider),
                        provider),
                    new AndroidFileManager(this),
                    provider));
        }
    }
}