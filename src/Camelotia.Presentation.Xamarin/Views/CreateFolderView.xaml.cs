using Camelotia.Presentation.Interfaces;
using ReactiveUI;
using ReactiveUI.XamForms;
using Xamarin.Forms.Xaml;

namespace Camelotia.Presentation.Xamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateFolderView : ReactiveContentPage<ICreateFolderViewModel>
    {
        public CreateFolderView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => { });
        }
    }
}