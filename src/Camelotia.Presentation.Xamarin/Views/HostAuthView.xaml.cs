using System;
using System.Reactive.Disposables;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Formatters;
using ReactiveUI.XamForms;
using Xamarin.Forms.Xaml;

namespace Camelotia.Presentation.Xamarin.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class HostAuthView : ReactiveContentPage<IHostAuthViewModel>
{
    public HostAuthView()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            this.BindValidation(ViewModel, x => x.Address, x => x.HostNameErrorLabel.Text)
                .DisposeWith(disposables);
            this.BindValidation(ViewModel, x => x.Port, x => x.PortErrorLabel.Text)
                .DisposeWith(disposables);
            this.BindValidation(ViewModel, x => x.Username, x => x.UserNameErrorLabel.Text)
                .DisposeWith(disposables);
            this.BindValidation(ViewModel, x => x.Password, x => x.PasswordErrorLabel.Text)
                .DisposeWith(disposables);
            this.BindValidation(ViewModel, x => x.FormErrorLabel.Text, new SingleLineFormatter(Environment.NewLine))
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.HostNameErrorLabel.Text, text => !string.IsNullOrWhiteSpace(text))
                .BindTo(this, x => x.HostNameErrorLabel.IsVisible)
                .DisposeWith(disposables);
            this.WhenAnyValue(x => x.PortErrorLabel.Text, text => !string.IsNullOrWhiteSpace(text))
                .BindTo(this, x => x.PortErrorLabel.IsVisible)
                .DisposeWith(disposables);
            this.WhenAnyValue(x => x.UserNameErrorLabel.Text, text => !string.IsNullOrWhiteSpace(text))
                .BindTo(this, x => x.UserNameErrorLabel.IsVisible)
                .DisposeWith(disposables);
            this.WhenAnyValue(x => x.PasswordErrorLabel.Text, text => !string.IsNullOrWhiteSpace(text))
                .BindTo(this, x => x.PasswordErrorLabel.IsVisible)
                .DisposeWith(disposables);
            this.WhenAnyValue(x => x.FormErrorLabel.Text, text => !string.IsNullOrWhiteSpace(text))
                .BindTo(this, x => x.FormErrorLabel.IsVisible)
                .DisposeWith(disposables);
        });
    }
}
