using Avalonia;
using Avalonia.Markup.Xaml;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia.Views
{
    public sealed class OAuthView : ReactiveUserControl<IOAuthViewModel>
    {
        public OAuthView()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}