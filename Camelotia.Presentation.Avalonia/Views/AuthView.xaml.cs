using Avalonia;
using Avalonia.Markup.Xaml;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia.Views
{
    public sealed class AuthView : ReactiveUserControl<IAuthViewModel>
    {
        public AuthView()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}