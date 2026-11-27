using Camelotia.Presentation.Infrastructure;
using Camelotia.Presentation.Interfaces;
using ReactiveUI.Avalonia.Reactive;
using ReactiveUI.Primitives.Reactive.Disposables;
using ReactiveUI.Reactive;
using ReactiveUI.Validation.Reactive.Extensions;

namespace Camelotia.Presentation.Avalonia.Views;

public sealed partial class DirectAuthView : ReactiveUserControl<IDirectAuthViewModel>
{
    public DirectAuthView()
    {
        InitializeComponent();
        this.WhenActivated((ContainerDisposable disposables) =>
        {
            this.BindValidation(ViewModel, x => x.Username, x => x.UsernameValidation.Text)
                .DisposeWith(disposables);
            this.BindValidation(ViewModel, x => x.Password, x => x.PasswordValidation.Text)
                .DisposeWith(disposables);
            this.BindValidation(ViewModel, x => x.FormValidation.Text)
                .DisposeWith(disposables);
        });
    }
}