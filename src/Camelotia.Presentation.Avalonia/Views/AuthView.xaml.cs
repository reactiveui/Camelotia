using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
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
                this.WhenAnyValue(x => x.ViewModel.SupportsDirectAuth)
                    .Where(supports => supports)
                    .Subscribe(supports => AuthTabs.SelectedIndex = 0)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.SupportsOAuth)
                    .Where(supports => supports)
                    .Subscribe(supports => AuthTabs.SelectedIndex = 1)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.SupportsHostAuth)
                    .Where(supports => supports)
                    .Subscribe(supports => AuthTabs.SelectedIndex = 2)
                    .DisposeWith(disposables);
            });
        }
    }
}
