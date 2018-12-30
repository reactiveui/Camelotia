using Camelotia.Presentation.Interfaces;
using System.Reactive.Disposables;
using Xamarin.Forms.Xaml;
using ReactiveUI.XamForms;
using ReactiveUI;

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
                this.OneWayBind(ViewModel, x => x.DirectAuth, x => x.DirectAuthPage.ViewModel)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.OAuth, x => x.OpenAuthPage.ViewModel)
                    .DisposeWith(disposables);
                
                this.OneWayBind(ViewModel, x => x.SupportsDirectAuth, x => x.DirectAuthPage.IsVisible)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.SupportsOAuth, x => x.OpenAuthPage.IsVisible)
                    .DisposeWith(disposables);
            });
        }
    }
}