using Camelotia.Presentation.Interfaces;
using Xamarin.Forms.Xaml;
using ReactiveUI.XamForms;
using ReactiveUI;

namespace Camelotia.Presentation.Xamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DirectAuthView : ReactiveContentPage<IDirectAuthViewModel>
    {
        public DirectAuthView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => { });
        }
    }
}