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
    public sealed partial class CreateFolderView : UserControl, IViewFor<ICreateFolderViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(ICreateFolderViewModel), typeof(CreateFolderView), null);

        public CreateFolderView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.BindValidation(ViewModel, x => x.Name, x => x.FolderNameErrorLabel.Text)
                    .DisposeWith(disposables);
                this.BindValidation(ViewModel, x => x.FormErrorLabel.Text, new SingleLineFormatter(Environment.NewLine))
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.FolderNameErrorLabel.Text, text => !string.IsNullOrWhiteSpace(text))
                    .BindTo(this, x => x.FolderNameErrorLabel.Visibility)
                    .DisposeWith(disposables);
                this.WhenAnyValue(x => x.FormErrorLabel.Text, text => !string.IsNullOrWhiteSpace(text))
                    .BindTo(this, x => x.FormErrorLabel.Visibility)
                    .DisposeWith(disposables);
            });
        }

        public ICreateFolderViewModel ViewModel
        {
            get => (ICreateFolderViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ICreateFolderViewModel)value;
        }
    }
}
