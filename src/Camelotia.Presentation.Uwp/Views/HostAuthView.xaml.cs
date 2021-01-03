using System;
using System.Reactive.Disposables;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Formatters;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Camelotia.Presentation.Uwp.Views
{
    public sealed partial class HostAuthView : UserControl, IViewFor<IHostAuthViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IHostAuthViewModel), typeof(DirectAuthView), null);

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
                    .BindTo(this, x => x.HostNameErrorLabel.Visibility)
                    .DisposeWith(disposables);
                this.WhenAnyValue(x => x.PortErrorLabel.Text, text => !string.IsNullOrWhiteSpace(text))
                    .BindTo(this, x => x.PortErrorLabel.Visibility)
                    .DisposeWith(disposables);
                this.WhenAnyValue(x => x.UserNameErrorLabel.Text, text => !string.IsNullOrWhiteSpace(text))
                    .BindTo(this, x => x.UserNameErrorLabel.Visibility)
                    .DisposeWith(disposables);
                this.WhenAnyValue(x => x.PasswordErrorLabel.Text, text => !string.IsNullOrWhiteSpace(text))
                    .BindTo(this, x => x.PasswordErrorLabel.Visibility)
                    .DisposeWith(disposables);
                this.WhenAnyValue(x => x.FormErrorLabel.Text, text => !string.IsNullOrWhiteSpace(text))
                    .BindTo(this, x => x.FormErrorLabel.Visibility)
                    .DisposeWith(disposables);
            });
        }

        public IHostAuthViewModel ViewModel
        {
            get => (IHostAuthViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (IHostAuthViewModel)value;
        }
    }
}
