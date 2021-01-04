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
    public sealed partial class DirectAuthView : UserControl, IViewFor<IDirectAuthViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(IDirectAuthViewModel), typeof(DirectAuthView), null);

        public DirectAuthView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.BindValidation(ViewModel, x => x.Username, x => x.UserNameErrorLabel.Text)
                    .DisposeWith(disposables);
                this.BindValidation(ViewModel, x => x.Password, x => x.PasswordErrorLabel.Text)
                    .DisposeWith(disposables);
                this.BindValidation(ViewModel, x => x.FormErrorLabel.Text, new SingleLineFormatter(Environment.NewLine))
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

        public IDirectAuthViewModel ViewModel
        {
            get => (IDirectAuthViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (IDirectAuthViewModel)value;
        }
    }
}
