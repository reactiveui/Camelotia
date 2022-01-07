using Avalonia.ReactiveUI;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia.Views;

public sealed partial class RenameFileView : ReactiveUserControl<IRenameFileViewModel>
{
    public RenameFileView()
    {
        InitializeComponent();
        this.WhenActivated(disposables => { });
    }
}