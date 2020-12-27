using Camelotia.Presentation.Interfaces;
using Xamarin.Forms.Xaml;
using ReactiveUI.XamForms;
using ReactiveUI;

namespace Camelotia.Presentation.Xamarin.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainCloudView : ReactiveViewCell<ICloudViewModel>
    {
        public MainCloudView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => { });
        }
    }
}