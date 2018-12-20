using Camelotia.Presentation.Uwp.Services;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Providers;
using System.Reactive.Concurrency;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using ReactiveUI;
using Camelotia.Presentation.Interfaces;

namespace Camelotia.Presentation.Uwp
{
    public sealed partial class MainView : Page, IViewFor<IMainViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IMainViewModel), typeof(MainView), null);

        public MainView()
        {
            InitializeComponent();
            ViewModel = BuildMainViewModel();
            this.WhenActivated(disposables => { });
        }

        public IMainViewModel ViewModel
        {
            get => (IMainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (IMainViewModel)value;
        }

        private static IMainViewModel BuildMainViewModel()
        {
            var currentThread = CurrentThreadScheduler.Instance;
            var mainThread = RxApp.MainThreadScheduler;

            return new MainViewModel(
                (provider, files, auth) => new ProviderViewModel(auth, files, currentThread, mainThread, provider),
                provider => new AuthViewModel(
                    new DirectAuthViewModel(currentThread, mainThread, provider),
                    new OAuthViewModel(currentThread, mainThread, provider),
                    currentThread,
                    provider
                ),
                new ProviderStorage(
                    new YandexFileSystemProvider(),
                    new VkontakteFileSystemProvider()
                ),
                new UniversalFileManager(),
                currentThread,
                mainThread
            );
        }
    }
}
