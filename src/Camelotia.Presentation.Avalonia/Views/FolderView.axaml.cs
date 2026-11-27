using Camelotia.Presentation.Interfaces;
using ReactiveUI.Avalonia.Reactive;

namespace Camelotia.Presentation.Avalonia.Views;

public sealed partial class FolderView : ReactiveUserControl<IFolderViewModel>
{
    public FolderView()
    {
        InitializeComponent();
    }
}