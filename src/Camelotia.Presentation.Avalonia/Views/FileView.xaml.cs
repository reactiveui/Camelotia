using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Input;
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
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {
                this.Events()
                    .DoubleTapped
                    .Do(args => ViewModel.Provider.SelectedFile = ViewModel)
                    .Select(args => Unit.Default)
                    .InvokeCommand(this, x => x.ViewModel.Provider.Open)
                    .DisposeWith(disposables);

                this.ContextMenu
                    .Events()
                    .ContextMenuOpening
                    .Do(args => args.Cancel = false)
                    .Subscribe(args => ViewModel.Provider.SelectedFile = ViewModel)
                    .DisposeWith(disposables);
            });
        }
    }
}