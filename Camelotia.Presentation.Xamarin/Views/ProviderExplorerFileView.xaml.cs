using System.IO;
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
                this.OneWayBind(ViewModel, x => x.Name, x => x.NameLabel.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.Modified, x => x.ModifiedLabel.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.Size, x => x.SizeLabel.Text)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.IsFolder)
                    .Select(folder => folder ? "fas-folder" : "fas-file")
                    .BindTo(this, x => x.IconImage.Icon)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.IsFile)
                    .Select(file => file ? Color.FromRgb(130, 113, 209) : Color.FromRgb(100, 83, 179))
                    .BindTo(this, x => x.IconImage.IconColor)
                    .DisposeWith(disposables);

                var isFolder = this
                    .WhenAnyValue(x => x.ViewModel.IsFolder)
                    .Where(folder => folder)
                    .Select(none => string.Empty);

                this.WhenAnyValue(x => x.ViewModel.Name)
                    .Where(name => ViewModel.IsFile)
                    .Select(Path.GetExtension)
                    .Where(ext => ext?.Length <= 4)
                    .Select(ext => ext.ToUpperInvariant().TrimStart('.'))
                    .Merge(isFolder)
                    .BindTo(this, x => x.ExtensionLabel.Text)
                    .DisposeWith(disposables);
            });
        }
    }
}