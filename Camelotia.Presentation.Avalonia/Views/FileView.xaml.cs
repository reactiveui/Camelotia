using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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
            this.WhenActivated(disposables =>
            {
                Observable.FromEventPattern<PointerReleasedEventArgs>(
                        handler => PointerReleased += handler,
                        handler => PointerReleased -= handler)
                    .Where(args => args.EventArgs.MouseButton == MouseButton.Right)
                    .Select(args => (FileView)args.Sender)
                    .Select(element => element.ViewModel)
                    .Subscribe(file => file.Provider.SelectedFile = file)
                    .DisposeWith(disposables);
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}