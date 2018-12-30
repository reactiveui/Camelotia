using Camelotia.Presentation.Interfaces;
using ReactiveUI.XamForms;
using ReactiveUI;
using Xamarin.Forms.Xaml;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System;

namespace Camelotia.Presentation.Xamarin.View
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
            });
        }
    }
}