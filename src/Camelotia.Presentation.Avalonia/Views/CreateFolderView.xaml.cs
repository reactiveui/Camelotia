using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia.Views
{
    public sealed partial class CreateFolderView : ReactiveUserControl<ICreateFolderViewModel>
    {
        public CreateFolderView()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables => { });
        }
    }
}
