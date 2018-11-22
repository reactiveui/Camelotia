using Avalonia;
using Avalonia.Markup.Xaml;

namespace Camelotia.Presentation.Avalonia
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            base.Initialize();
        }
    }
}