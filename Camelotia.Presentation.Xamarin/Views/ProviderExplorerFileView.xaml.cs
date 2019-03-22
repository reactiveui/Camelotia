using System.IO;
using Camelotia.Services.Models;
using ReactiveUI;
using ReactiveUI.XamForms;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Xamarin.Forms.Xaml;
using Color = Xamarin.Forms.Color;

namespace Camelotia.Presentation.Xamarin.Views
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

                this.WhenAnyValue(x => x.ViewModel.Name)
                    .Where(name => ViewModel.IsFile)
                    .Select(Path.GetExtension)
                    .Where(ext => ext?.Length <= 4)
                    .Select(ext => ext.ToUpperInvariant().TrimStart('.'))
                    .Merge(this
                        .WhenAnyValue(x => x.ViewModel.IsFolder)
                        .Where(folder => folder)
                        .Select(none => string.Empty))
                    .BindTo(this, x => x.ExtensionLabel.Text)
                    .DisposeWith(disposables);
            });
        }
    }
}