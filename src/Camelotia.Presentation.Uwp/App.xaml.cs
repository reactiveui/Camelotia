using System;
using Camelotia.Presentation.AppState;
using Camelotia.Presentation.Infrastructure;
using Camelotia.Presentation.Uwp.Views;
using ReactiveUI;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Camelotia.Presentation.Uwp;

public sealed partial class App : Application
{
    public App() => InitializeComponent();

    protected override async void OnLaunched(LaunchActivatedEventArgs e)
    {
        var stateFile = await ApplicationData
            .Current.LocalFolder
            .CreateFileAsync("state.json", CreationCollisionOption.OpenIfExists);

        var autoSuspendHelper = new AutoSuspendHelper(this);
        RxApp.MainThreadScheduler = new SingleWindowDispatcherScheduler();
        RxApp.SuspensionHost.CreateNewAppState = () => new MainState();
        RxApp.SuspensionHost.SetupDefaultSuspendResume(new NewtonsoftJsonSuspensionDriver(stateFile.Path));
        autoSuspendHelper.OnLaunched(e);

        if (!(Window.Current.Content is Frame rootFrame))
        {
            rootFrame = new Frame();
            rootFrame.NavigationFailed += OnNavigationFailed;
            Window.Current.Content = rootFrame;
        }

        if (e.PrelaunchActivated == false)
        {
            if (rootFrame.Content == null)
                rootFrame.Navigate(typeof(MainView), e.Arguments);
            Window.Current.Activate();
        }
    }

    private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }
}
