using Camelotia.Presentation.Interfaces;
using Xamarin.Forms.Xaml;
using Xamarin.Forms;
using ReactiveUI.XamForms;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Linq;
using System;

namespace Camelotia.Presentation.Xamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProviderView : ReactiveNavigationPage<IProviderViewModel>
    {
        public ProviderView()
        {
            InitializeComponent();
            SetHasNavigationBar(this, false);
            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(x => x.ViewModel.Auth.IsAuthenticated)
                    .Where(authenticated => authenticated)
                    .DistinctUntilChanged()
                    .Select(x => new ProviderExplorerView { ViewModel = ViewModel })
                    .Subscribe(NavigateWithoutBackStack)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Auth.IsAuthenticated)
                    .Where(authenticated => !authenticated)
                    .DistinctUntilChanged()
                    .Select(x => new AuthView { ViewModel = ViewModel.Auth })
                    .Subscribe(NavigateWithoutBackStack)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Folder.IsVisible)
                    .Where(visible => visible)
                    .Select(x => new CreateFolderView { ViewModel = ViewModel.Folder })
                    .Subscribe(NavigateWithoutBackStack)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Folder.IsVisible)
                    .Where(visible => !visible)
                    .Skip(1)
                    .Select(x => new ProviderExplorerView { ViewModel = ViewModel })
                    .Subscribe(NavigateWithoutBackStack)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Rename.IsVisible)
                    .Where(visible => visible)
                    .Select(x => new RenameFileView { ViewModel = ViewModel.Rename })
                    .Subscribe(NavigateWithoutBackStack)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Rename.IsVisible)
                    .Where(visible => !visible)
                    .Skip(1)
                    .Select(x => new ProviderExplorerView { ViewModel = ViewModel })
                    .Subscribe(NavigateWithoutBackStack)
                    .DisposeWith(disposables);
            });
        }

        private async void NavigateWithoutBackStack(Page page)
        {
            if (Navigation.NavigationStack.Any())
                Navigation.InsertPageBefore(page, Navigation.NavigationStack.First());
            else await Navigation.PushAsync(page);
            await Navigation.PopToRootAsync();
        }
    }
}