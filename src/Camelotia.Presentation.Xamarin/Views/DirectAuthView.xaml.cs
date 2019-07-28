using Camelotia.Presentation.Interfaces;
using System.Reactive.Disposables;
using Xamarin.Forms.Xaml;
using ReactiveUI.XamForms;
using ReactiveUI;

namespace Camelotia.Presentation.Xamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DirectAuthView : ReactiveContentPage<IDirectAuthViewModel>
    {
        public DirectAuthView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => 
            {
                this.Bind(ViewModel, x => x.Username, x => x.LoginEntry.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, x => x.Password, x => x.PasswordEntry.Text)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.Login, x => x.LoginButton)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, x => x.HasErrors, x => x.ErrorLabel.IsVisible)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.ErrorMessage, x => x.ErrorLabel.Text)
                    .DisposeWith(disposables);
            });
        }
    }
}