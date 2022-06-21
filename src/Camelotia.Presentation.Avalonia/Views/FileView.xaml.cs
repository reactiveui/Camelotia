using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using Camelotia.Presentation.Interfaces;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia.Views;

public sealed partial class FileView : ReactiveUserControl<IFileViewModel>
{
    public FileView()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            this.Events()
                .DoubleTapped
                .Do(args => ViewModel.Provider.SelectedFile = ViewModel)
                .Select(args => Unit.Default)
                .InvokeCommand(this, x => x.ViewModel.Provider.Open)
                .DisposeWith(disposables);

            ContextMenu
                .Events()
                .MenuOpened
                .Subscribe(args => ViewModel.Provider.SelectedFile = ViewModel)
                .DisposeWith(disposables);
        });
    }
}
