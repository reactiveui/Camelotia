using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia.Views
{
    public sealed partial class AuthView : ReactiveUserControl<IAuthViewModel>
    {
        public AuthView()
        {
            AvaloniaXamlLoader.Load(this);
            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(x => x.ViewModel)
                    .Where(context => context != null)
                    .Select(ResolveControl)
                    .BindTo(this, x => x.Content)
                    .DisposeWith(disposables);
            });
        }

        private static IControl ResolveControl(IAuthViewModel context)
        {
            if (context.SupportsDirectAuth)
                return new DirectAuthView { DataContext = context.DirectAuth };
            if (context.SupportsOAuth)
                return new OAuthView { DataContext = context.OAuth };
            if (context.SupportsHostAuth)
                return new HostAuthView { DataContext = context.HostAuth };
            return new TextBlock
            {
                Text = "No supported authentication method found.",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }
    }
}
