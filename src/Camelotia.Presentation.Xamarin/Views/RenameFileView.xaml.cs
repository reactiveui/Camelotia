using System;
using System.Reactive.Disposables;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Formatters;
using ReactiveUI.XamForms;
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
                this.BindValidation(ViewModel, x => x.NewName, x => x.FileNameErrorLabel.Text)
                    .DisposeWith(disposables);
                this.BindValidation(ViewModel, x => x.FormErrorLabel.Text, new SingleLineFormatter(Environment.NewLine))
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.FileNameErrorLabel.Text, text => !string.IsNullOrWhiteSpace(text))
                    .BindTo(this, x => x.FileNameErrorLabel.IsVisible)
                    .DisposeWith(disposables);
                this.WhenAnyValue(x => x.FormErrorLabel.Text, text => !string.IsNullOrWhiteSpace(text))
                    .BindTo(this, x => x.FormErrorLabel.IsVisible)
                    .DisposeWith(disposables);
            });
        }
    }
}