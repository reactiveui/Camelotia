using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Camelotia.Presentation.Infrastructure;
using Camelotia.Presentation.Interfaces;
using ReactiveUI.Avalonia.Reactive;
using ReactiveUI.Primitives.Reactive.Disposables;
using ReactiveUI.Reactive;

namespace Camelotia.Presentation.Avalonia.Views;

public sealed partial class AuthView : ReactiveUserControl<IAuthViewModel>
{
    public AuthView()
    {
        InitializeComponent();
        this.WhenActivated((ContainerDisposable disposables) =>
        {
            this.WhenAnyValue(x => x.ViewModel)
                .Where(context => context != null)
                .Select(ResolveControl)
                .BindTo(this, x => x.Content)
                .DisposeWith(disposables);
        });
    }

    private static Control ResolveControl(IAuthViewModel context)
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