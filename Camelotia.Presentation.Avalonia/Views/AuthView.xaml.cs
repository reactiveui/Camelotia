using System.Reactive.Linq;
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;

namespace Camelotia.Presentation.Avalonia.Views
{
    public sealed class AuthView : ReactiveUserControl<IAuthViewModel>
    {
        public AuthView()
        {
            this.WhenActivated(disposables =>
            {
                var tabs = this.FindControl<TabControl>("AuthTabs");
                this.WhenAnyValue(x => x.ViewModel.SupportsDirectAuth)
                    .Where(supports => supports)
                    .Subscribe(supports => tabs.SelectedIndex = 0);
                
                this.WhenAnyValue(x => x.ViewModel.SupportsOAuth)
                    .Where(supports => supports)
                    .Subscribe(supports => tabs.SelectedIndex = 1);
                
                this.WhenAnyValue(x => x.ViewModel.SupportsHostAuth)
                    .Where(supports => supports)
                    .Subscribe(supports => tabs.SelectedIndex = 2);
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}