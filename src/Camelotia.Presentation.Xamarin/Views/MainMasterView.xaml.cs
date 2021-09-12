using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Camelotia.Presentation.Interfaces;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using ReactiveUI.XamForms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Camelotia.Presentation.Xamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainMasterView : ReactiveContentPage<IMainViewModel>, IDisposable
    {
        private CompositeDisposable _listeners;

        public MainMasterView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                OpenButton
                    .Events()
                    .Clicked
                    .Select(args => ViewModel.SelectedProvider)
                    .Where(provider => provider != null)
                    .Select(x => new CloudExplorerView { ViewModel = x })
                    .Subscribe(NavigateToProvider)
                    .DisposeWith(disposables);
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _listeners.Dispose();
            }
        }

        private void NavigateToProvider(CloudExplorerView view)
        {
            _listeners?.Dispose();
            _listeners = new CompositeDisposable();

            view.WhenAnyValue(x => x.ViewModel.Auth.IsAuthenticated)
                .Where(authenticated => authenticated)
                .Select(x => new CloudExplorerView { ViewModel = ViewModel.SelectedProvider })
                .Subscribe(NavigateWithoutBackStack)
                .DisposeWith(_listeners);

            view.WhenAnyValue(x => x.ViewModel.Auth.IsAuthenticated)
                .Where(authenticated => !authenticated)
                .Select(x => new AuthView { ViewModel = ViewModel.SelectedProvider.Auth })
                .Subscribe(NavigateWithoutBackStack)
                .DisposeWith(_listeners);

            view.WhenAnyValue(x => x.ViewModel.Folder.IsVisible)
                .Where(visible => visible)
                .Select(x => new CreateFolderView { ViewModel = ViewModel.SelectedProvider.Folder })
                .Subscribe(NavigateWithoutBackStack)
                .DisposeWith(_listeners);

            view.WhenAnyValue(x => x.ViewModel.Folder.IsVisible)
                .Where(visible => !visible)
                .Skip(1)
                .Select(x => new CloudExplorerView { ViewModel = ViewModel.SelectedProvider })
                .Subscribe(NavigateWithoutBackStack)
                .DisposeWith(_listeners);

            view.WhenAnyValue(x => x.ViewModel.Rename.IsVisible)
                .Where(visible => visible)
                .Select(x => new RenameFileView { ViewModel = ViewModel.SelectedProvider.Rename })
                .Subscribe(NavigateWithoutBackStack)
                .DisposeWith(_listeners);

            view.WhenAnyValue(x => x.ViewModel.Rename.IsVisible)
                .Where(visible => !visible)
                .Skip(1)
                .Select(x => new CloudExplorerView { ViewModel = ViewModel.SelectedProvider })
                .Subscribe(NavigateWithoutBackStack)
                .DisposeWith(_listeners);
        }

        private async void NavigateWithoutBackStack(Page page)
        {
            while (Navigation.NavigationStack.Count > 1)
                await Navigation.PopAsync(false).ConfigureAwait(false);
            await Navigation.PushAsync(page).ConfigureAwait(false);
        }
    }
}
