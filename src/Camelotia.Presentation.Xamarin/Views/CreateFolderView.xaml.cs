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
    public partial class CreateFolderView : ReactiveContentPage<ICreateFolderViewModel>
    {
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
                    .BindTo(this, x => x.FolderNameErrorLabel.IsVisible)
                    .DisposeWith(disposables);
                this.WhenAnyValue(x => x.FormErrorLabel.Text, text => !string.IsNullOrWhiteSpace(text))
                    .BindTo(this, x => x.FormErrorLabel.IsVisible)
                    .DisposeWith(disposables);
            });
        }
    }
}