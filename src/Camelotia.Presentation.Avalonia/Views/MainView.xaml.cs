using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia.Views
{
    public sealed class MainView : ReactiveWindow<IMainViewModel>
    {
        public Button SwitchThemeButton => this.FindControl<Button>("SwitchThemeButton");
        
        public MainView()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}