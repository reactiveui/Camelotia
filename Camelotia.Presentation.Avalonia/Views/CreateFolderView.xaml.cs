using Avalonia;
using Avalonia.Markup.Xaml;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia.Views
{
    public sealed class CreateFolderView : ReactiveUserControl<ICreateFolderViewModel>
    {
        public CreateFolderView()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}