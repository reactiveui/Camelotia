using System;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI.Avalonia.Reactive;
using Camelotia.Presentation.Interfaces;
using ReactiveMarbles.ObservableEvents;
using Camelotia.Presentation.Infrastructure;
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
                .InvokeCommand(this, x => x.ViewModel.Provider.Open)
                .DisposeWith(disposables);

            ContextMenu
                .Events()
                .Opening
                .Subscribe(args => ViewModel.Provider.SelectedFile = ViewModel)
                .DisposeWith(disposables);
        });
    }
}
