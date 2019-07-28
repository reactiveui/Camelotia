using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.XamForms;
using System.Reactive.Disposables;
using Xamarin.Forms.Xaml;

namespace Camelotia.Presentation.Xamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RenameFileView : ReactiveContentPage<IRenameFileViewModel>
    {
        public RenameFileView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.BindCommand(ViewModel, x => x.Rename, x => x.RenameFileButton)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.Close, x => x.CloseButton)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, x => x.NewName, x => x.NameEntry.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.ErrorMessage, x => x.ErrorLabel.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.HasErrors, x => x.ErrorLabel.IsVisible)
                    .DisposeWith(disposables);
            });
        }
    }
}