using Avalonia;
using Avalonia.Markup.Xaml;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia.Views
{
    public sealed class MainView : ReactiveWindow<IMainViewModel>
    {
        public MainView()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}