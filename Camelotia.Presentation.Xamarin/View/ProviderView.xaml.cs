using Camelotia.Presentation.Interfaces;
using System.Reactive.Disposables;
using Xamarin.Forms.Xaml;
using ReactiveUI.XamForms;
using ReactiveUI;

namespace Camelotia.Presentation.Xamarin.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProviderView : ReactiveContentPage<IProviderViewModel>
    {
        public ProviderView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => 
            {
                this.BindCommand(ViewModel, x => x.Back, x => x.BackButton)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.CurrentPath, x => x.PathLabel.Text)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel, x => x.Refresh, x => x.RefreshButton)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.Open, x => x.OpenButton)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, x => x.Files, x => x.FilesListView.ItemsSource)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, x => x.SelectedFile, x => x.FilesListView.SelectedItem)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.IsLoading, x => x.IsBusy)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, x => x.CanLogout, x => x.LogoutButton.IsVisible)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.Logout, x => x.LogoutButton)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.DeleteSelectedFile, x => x.DeleteButton)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, x => x.SelectedFile.Name, x => x.SelectedFileNameLabel.Text)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.UploadToCurrentPath, x => x.UploadButton)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.DownloadSelectedFile, x => x.DownloadButton)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, x => x.Name, x => x.Title)
                    .DisposeWith(disposables);
            });
        }
    }
}