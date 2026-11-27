using Camelotia.Presentation.Infrastructure;
using Camelotia.Presentation.Interfaces;
using ReactiveUI.Avalonia.Reactive;
using ReactiveUI.Primitives.Reactive.Disposables;
using ReactiveUI.Reactive;

namespace Camelotia.Presentation.Avalonia.Views;

public sealed partial class OAuthView : ReactiveUserControl<IOAuthViewModel>
{
    public OAuthView()
    {
        InitializeComponent();
        this.WhenActivated((ContainerDisposable disposables) => { });
    }
}