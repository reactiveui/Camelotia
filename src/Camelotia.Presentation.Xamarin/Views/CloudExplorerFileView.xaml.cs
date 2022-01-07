using System.Reactive.Disposables;
using System.Reactive.Linq;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.XamForms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Camelotia.Presentation.Xamarin.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class CloudExplorerFileView : ReactiveViewCell<IFileViewModel>
{
    public CloudExplorerFileView()
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
