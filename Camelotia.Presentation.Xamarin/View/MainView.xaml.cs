using Camelotia.Presentation.Interfaces;
using Xamarin.Forms.Xaml;
using ReactiveUI.XamForms;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System;

namespace Camelotia.Presentation.Xamarin.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainView : ReactiveMasterDetailPage<IMainViewModel>
    {
        public MainView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(x => x.ViewModel.SelectedProvider)
                    .Select(provider => false)
                    .Subscribe(x => IsPresented = x)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.SelectedProvider.Auth.IsAuthenticated)
                    .Where(authenticated => authenticated)
                    .DistinctUntilChanged()
                    .Select(x => new ProviderView { ViewModel = ViewModel.SelectedProvider })
                    .Subscribe(view => NavigationView.PushAsync(view))
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.SelectedProvider.Auth.IsAuthenticated)
                    .Where(authenticated => !authenticated)
                    .DistinctUntilChanged()
                    .Select(x => new AuthView { ViewModel = ViewModel.SelectedProvider.Auth })
                    .Subscribe(view => NavigationView.PushAsync(view))
                    .DisposeWith(disposables);
            });
        }
    }
}