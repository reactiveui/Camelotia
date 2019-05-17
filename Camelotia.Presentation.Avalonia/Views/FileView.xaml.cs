using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia.Views
{
    public sealed class FileView : ReactiveUserControl<IFileViewModel>
    {
        public FileView()
        {
            this.WhenActivated(disposables =>
            {
                Observable.FromEventPattern<CancelEventHandler, CancelEventArgs>(
                        handler => ContextMenu.ContextMenuOpening += handler,
                        handler => ContextMenu.ContextMenuOpening -= handler)
                    .Do(args => args.EventArgs.Cancel = false)
                    .Subscribe(args => ViewModel.Provider.SelectedFile = ViewModel)
                    .DisposeWith(disposables);

                Observable.FromEventPattern<RoutedEventArgs>(
                        handler => DoubleTapped += handler,
                        handler => DoubleTapped -= handler)
                    .Select(args => Unit.Default)
                    .InvokeCommand(this, x => x.ViewModel.Provider.Open)
                    .DisposeWith(disposables);
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}