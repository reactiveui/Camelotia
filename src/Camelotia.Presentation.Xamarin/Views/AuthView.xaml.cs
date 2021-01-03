using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.XamForms;
using Xamarin.Forms.Xaml;

namespace Camelotia.Presentation.Xamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AuthView : ReactiveTabbedPage<IAuthViewModel>
    {
        public AuthView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(x => x.ViewModel.SupportsDirectAuth)
                    .Where(supportsDirectAuth => supportsDirectAuth)
                    .Select(supports => new DirectAuthView { ViewModel = ViewModel.DirectAuth })
                    .Do(view => Children.Clear())
                    .Subscribe(view => Children.Add(view))
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.SupportsOAuth)
                    .Where(supportsOAuth => supportsOAuth)
                    .Select(supports => new OAuthView { ViewModel = ViewModel.OAuth })
                    .Do(view => Children.Clear())
                    .Subscribe(view => Children.Add(view))
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.SupportsHostAuth)
                    .Where(supportsHostAuth => supportsHostAuth)
                    .Select(supports => new HostAuthView { ViewModel = ViewModel.HostAuth })
                    .Do(view => Children.Clear())
                    .Subscribe(view => Children.Add(view))
                    .DisposeWith(disposables);
            });
        }
    }
}