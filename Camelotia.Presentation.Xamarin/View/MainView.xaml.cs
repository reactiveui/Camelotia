using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.XamForms;
using System.Reactive.Disposables;
using Xamarin.Forms.Xaml;

namespace Camelotia.Presentation.Xamarin.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainView : ReactiveContentPage<IMainViewModel>
    {
        public MainView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => 
            {
                this.OneWayBind(ViewModel, x => x.IsReady, x => x.ProvidersView.IsVisible)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.IsLoading, x => x.LoadingBar.IsVisible)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.LoadProviders, x => x.RefreshButton)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, x => x.SelectedProvider, x => x.ProvidersView.SelectedItem)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.Providers, x => x.ProvidersView.ItemsSource)
                    .DisposeWith(disposables);
            });
        }
    }
}
