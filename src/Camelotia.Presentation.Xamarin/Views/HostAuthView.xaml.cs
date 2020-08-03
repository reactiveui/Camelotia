using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.XamForms;
using System.Reactive.Disposables;
using Xamarin.Forms.Xaml;

namespace Camelotia.Presentation.Xamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HostAuthView : ReactiveContentPage<IHostAuthViewModel>
    {
        public HostAuthView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel, x => x.Address, x => x.AddressEntry.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, x => x.Port, x => x.PortEntry.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, x => x.Username, x => x.LoginEntry.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, x => x.Password, x => x.PasswordEntry.Text)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.Login, x => x.LoginButton)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, x => x.HasErrorMessage, x => x.ErrorLabel.IsVisible)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.ErrorMessage, x => x.ErrorLabel.Text)
                    .DisposeWith(disposables);
            });
        }
    }
}