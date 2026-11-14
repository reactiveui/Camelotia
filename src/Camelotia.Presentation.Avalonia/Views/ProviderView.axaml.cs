using ReactiveUI.Avalonia.Reactive;
using Camelotia.Presentation.Interfaces;
using Camelotia.Presentation.Infrastructure;
using ReactiveUI.Primitives.Reactive.Disposables;
using ReactiveUI.Reactive;

namespace Camelotia.Presentation.Avalonia.Views;

public sealed partial class ProviderView : ReactiveUserControl<ICloudViewModel>
{
    public ProviderView()
    {
        InitializeComponent();
        this.WhenActivated((ContainerDisposable disposables) => { });
    }
}