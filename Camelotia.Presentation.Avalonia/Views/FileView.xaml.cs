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
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}