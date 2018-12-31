using Camelotia.Services.Models;
using ReactiveUI;
using ReactiveUI.XamForms;
using System.Reactive.Disposables;
using Xamarin.Forms.Xaml;

namespace Camelotia.Presentation.Xamarin.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProviderExplorerFileView : ReactiveViewCell<FileModel>
    {
        public ProviderExplorerFileView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel, x => x.Name, x => x.NameLabel.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.Size, x => x.SizeLabel.Text)
                    .DisposeWith(disposables);
            });
        }
    }
}