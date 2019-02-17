using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.XamForms;
using System.Reactive.Disposables;
using Xamarin.Forms.Xaml;

namespace Camelotia.Presentation.Xamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateFolderView : ReactiveContentPage<ICreateFolderViewModel>
    {
        public CreateFolderView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.BindCommand(ViewModel, x => x.Create, x => x.CreateFolderButton)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.Close, x => x.CloseButton)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, x => x.Name, x => x.NameEntry.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.ErrorMessage, x => x.ErrorLabel.Text)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.HasErrors, x => x.ErrorLabel.IsVisible)
                    .DisposeWith(disposables);
            });
        }
    }
}