using System;
using System.Reactive;
using System.Reactive.Linq;
using Camelotia.Presentation.Infrastructure;
using Camelotia.Presentation.Interfaces;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI.Avalonia.Reactive;
using ReactiveUI.Primitives.Reactive.Disposables;
using ReactiveUI.Reactive;

namespace Camelotia.Presentation.Avalonia.Views;

public sealed partial class FileView : ReactiveUserControl<IFileViewModel>
{
    public FileView()
    {
        InitializeComponent();
        this.WhenActivated((ContainerDisposable disposables) =>
        {
            this.Events()
                .DoubleTapped
                .Do(args => ViewModel.Provider.SelectedFile = ViewModel)
                .Select(args => Unit.Default)
                .InvokeCommand(ViewModel.Provider.Open)
                .DisposeWith(disposables);

            ContextMenu
                .Events()
                .Opening
                .Subscribe(args => ViewModel.Provider.SelectedFile = ViewModel)
                .DisposeWith(disposables);
        });
    }
}
