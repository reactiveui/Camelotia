using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.XamForms;
using System.Reactive.Disposables;
using Xamarin.Forms.Xaml;

namespace Camelotia.Presentation.Xamarin.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DirectAuthView : ReactiveContentPage<IDirectAuthViewModel>
    {
        public DirectAuthView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => 
            {
                this.Bind(ViewModel, x => x.Username, x => x.LoginEntry.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, x => x.Password, x => x.PasswordEntry.Text)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.Login, x => x.LoginButton)
                    .DisposeWith(disposables);
            });
        }
    }
}