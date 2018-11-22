using Avalonia;
using Avalonia.Markup.Xaml;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia.Views
{
    public sealed class ProviderView : ReactiveUserControl<IProviderViewModel>
    {
        public ProviderView()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}