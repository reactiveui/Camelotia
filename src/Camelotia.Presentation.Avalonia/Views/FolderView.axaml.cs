using ReactiveUI.Avalonia.Reactive;
using Camelotia.Presentation.Interfaces;

namespace Camelotia.Presentation.Avalonia.Views;

public sealed partial class FolderView : ReactiveUserControl<IFolderViewModel>
{
    public FolderView()
    {
        InitializeComponent();
    }
}