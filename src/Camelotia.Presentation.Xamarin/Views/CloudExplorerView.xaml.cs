using System.Reactive.Disposables;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.XamForms;
using Xamarin.Forms.Xaml;

namespace Camelotia.Presentation.Xamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CloudExplorerView : ReactiveContentPage<ICloudViewModel>
    {
        public CloudExplorerView()
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