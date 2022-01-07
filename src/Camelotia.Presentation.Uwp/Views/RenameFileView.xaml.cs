using System;
using System.Reactive.Disposables;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Formatters;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Camelotia.Presentation.Uwp.Views;

public sealed partial class RenameFileView : UserControl, IViewFor<IRenameFileViewModel>
{
    public static readonly DependencyProperty ViewModelProperty = DependencyProperty
        .Register(nameof(ViewModel), typeof(IRenameFileViewModel), typeof(RenameFileView), null);

    public RenameFileView()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            this.BindValidation(ViewModel, x => x.NewName, x => x.FileNameErrorLabel.Text)
                .DisposeWith(disposables);
            this.BindValidation(ViewModel, x => x.FormErrorLabel.Text, new SingleLineFormatter(Environment.NewLine))
                .DisposeWith(disposables);

            this.WhenAnyValue(x => x.FileNameErrorLabel.Text, text => !string.IsNullOrWhiteSpace(text))
                .BindTo(this, x => x.FileNameErrorLabel.Visibility)
                .DisposeWith(disposables);
            this.WhenAnyValue(x => x.FormErrorLabel.Text, text => !string.IsNullOrWhiteSpace(text))
                .BindTo(this, x => x.FormErrorLabel.Visibility)
                .DisposeWith(disposables);
        });
    }

    public IRenameFileViewModel ViewModel
    {
        get => (IRenameFileViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (IRenameFileViewModel)value;
    }
}
