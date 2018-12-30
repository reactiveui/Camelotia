using Camelotia.Presentation.Interfaces;
using System.Reactive.Disposables;
using Xamarin.Forms.Xaml;
using ReactiveUI.XamForms;
using ReactiveUI;

namespace Camelotia.Presentation.Xamarin.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OAuthView : ReactiveContentPage<IOAuthViewModel>
    {
        public OAuthView()
        {
            InitializeComponent();
            this.WhenActivated(disposables =>
            {
                this.BindCommand(ViewModel, x => x.Login, x => x.LoginButton)
                    .DisposeWith(disposables);
            });
        }
    }
}