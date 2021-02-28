using System.Reactive.Disposables;
using Avalonia.ReactiveUI;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Camelotia.Presentation.Avalonia.Views
{
    public sealed partial class HostAuthView : ReactiveUserControl<IHostAuthViewModel>
    {
        public HostAuthView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.BindValidation(ViewModel, x => x.Address, x => x.AddressValidation.Text)
                    .DisposeWith(disposables);
                this.BindValidation(ViewModel, x => x.Port, x => x.PortValidation.Text)
                    .DisposeWith(disposables);
                this.BindValidation(ViewModel, x => x.Username, x => x.UsernameValidation.Text)
                    .DisposeWith(disposables);
                this.BindValidation(ViewModel, x => x.Password, x => x.PasswordValidation.Text)
                    .DisposeWith(disposables);
                this.BindValidation(ViewModel, x => x.FormValidation.Text)
                    .DisposeWith(disposables);
            });
        }
    }
}
