using Camelotia.Presentation.Infrastructure;
using Camelotia.Presentation.Interfaces;
using ReactiveUI.Avalonia.Reactive;
using ReactiveUI.Primitives.Reactive.Disposables;
using ReactiveUI.Reactive;

namespace Camelotia.Presentation.Avalonia.Views;

public sealed partial class MainView : ReactiveUserControl<IMainViewModel>
{
    public MainView()
    {
        InitializeComponent();
        this.WhenActivated((ContainerDisposable disposables) => { });
    }
}