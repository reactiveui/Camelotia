using ReactiveUI;
using ReactiveUI.XamForms;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Camelotia.Presentation.Interfaces;

namespace Camelotia.Presentation.Xamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProviderExplorerFileView : ReactiveViewCell<IFileViewModel>
    {
        public ProviderExplorerFileView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(x => x.ViewModel.IsFolder)
                    .Select(folder => folder ? "fas-folder" : "fas-file")
                    .BindTo(this, x => x.IconImage.Icon)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.IsFile)
                    .Select(file => file ? Color.FromRgb(130, 113, 209) : Color.FromRgb(100, 83, 179))
                    .BindTo(this, x => x.IconImage.IconColor)
                    .DisposeWith(disposables);
            });
        }
    }
}