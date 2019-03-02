using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.XamForms;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Xamarin.Forms.Xaml;

namespace Camelotia.Presentation.Xamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainMasterView : ReactiveContentPage<IMainViewModel>
    {
        public MainMasterView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => 
            {
                this.OneWayBind(ViewModel, x => x.IsReady, x => x.ProvidersView.IsVisible)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.IsLoading, x => x.LoadingBar.IsVisible)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel, x => x.Refresh, x => x.RefreshButton)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.Remove, x => x.RemoveButton)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, x => x.Providers, x => x.ProvidersView.ItemsSource)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, x => x.SelectedProvider, x => x.ProvidersView.SelectedItem)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.SupportedTypes)
                    .Select(types => types.ToList())
                    .BindTo(this, x => x.SupportedTypesPicker.ItemsSource)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, x => x.SelectedSupportedType, x => x.SupportedTypesPicker.SelectedItem)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel, x => x.Add, x => x.AddButton)
                    .DisposeWith(disposables);
            });
        }
    }
}
