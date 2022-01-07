using Avalonia.ReactiveUI;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia.Views;

public sealed partial class MainView : ReactiveUserControl<IMainViewModel>
{
    public MainView()
    {
        InitializeComponent();
        this.WhenActivated(disposables => { });
    }
}