using Camelotia.Presentation.Interfaces;
using Xamarin.Forms.Xaml;
using Xamarin.Forms;
using ReactiveUI.XamForms;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System;

namespace Camelotia.Presentation.Xamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainView : ReactiveNavigationPage<IMainViewModel>
    {
        private CompositeDisposable _currentProviderListeners;

        public MainView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(x => x.ViewModel)
                    .Select(x => new MainMasterView { ViewModel = x })
                    .Subscribe(view => PushAsync(view))
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.SelectedProvider)
                    .Where(provider => provider != null)
                    .Skip(1)
                    .Select(x => new ProviderExplorerView { ViewModel = ViewModel.SelectedProvider })
                    .Subscribe(HandleSelectedProviderViewChange)
                    .DisposeWith(disposables);
            });
        }

        private void HandleSelectedProviderViewChange(ProviderExplorerView view)
        {
            _currentProviderListeners?.Dispose();
            _currentProviderListeners = new CompositeDisposable();

            view.WhenAnyValue(x => x.ViewModel.Auth.IsAuthenticated)
                .Where(authenticated => authenticated)
                .Select(x => new ProviderExplorerView { ViewModel = ViewModel.SelectedProvider })
                .Subscribe(NavigateWithoutBackStack)
                .DisposeWith(_currentProviderListeners);

            view.WhenAnyValue(x => x.ViewModel.Auth.IsAuthenticated)
                .Where(authenticated => !authenticated)
                .Select(x => new AuthView { ViewModel = ViewModel.SelectedProvider.Auth })
                .Subscribe(NavigateWithoutBackStack)
                .DisposeWith(_currentProviderListeners);

            view.WhenAnyValue(x => x.ViewModel.Folder.IsVisible)
                .Where(visible => visible)
                .Select(x => new CreateFolderView { ViewModel = ViewModel.SelectedProvider.Folder })
                .Subscribe(NavigateWithoutBackStack)
                .DisposeWith(_currentProviderListeners);

            view.WhenAnyValue(x => x.ViewModel.Folder.IsVisible)
                .Where(visible => !visible)
                .Skip(1)
                .Select(x => new ProviderExplorerView { ViewModel = ViewModel.SelectedProvider })
                .Subscribe(NavigateWithoutBackStack)
                .DisposeWith(_currentProviderListeners);

            view.WhenAnyValue(x => x.ViewModel.Rename.IsVisible)
                .Where(visible => visible)
                .Select(x => new RenameFileView { ViewModel = ViewModel.SelectedProvider.Rename })
                .Subscribe(NavigateWithoutBackStack)
                .DisposeWith(_currentProviderListeners);

            view.WhenAnyValue(x => x.ViewModel.Rename.IsVisible)
                .Where(visible => !visible)
                .Skip(1)
                .Select(x => new ProviderExplorerView { ViewModel = ViewModel.SelectedProvider })
                .Subscribe(NavigateWithoutBackStack)
                .DisposeWith(_currentProviderListeners);
        }

        private async void NavigateWithoutBackStack(Page page)
        {
            while (Navigation.NavigationStack.Count > 1)
                await Navigation.PopAsync(true);
            await PushAsync(page);
        }
    }
}