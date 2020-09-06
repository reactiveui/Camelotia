using Camelotia.Presentation.Interfaces;
using System.Reactive.Disposables;
using Xamarin.Forms.Xaml;
using ReactiveUI.XamForms;
using ReactiveUI;

namespace Camelotia.Presentation.Xamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProviderExplorerView : ReactiveContentPage<IProviderViewModel>
    {
        public ProviderExplorerView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel, x => x.IsLoading, x => x.FilesListView.IsRefreshing)
                    .DisposeWith(disposables);
            });
        }
    }
}